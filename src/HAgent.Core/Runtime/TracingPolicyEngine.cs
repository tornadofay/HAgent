using System;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Policy engine decorator that exposes policy evaluation as bounded trace metadata.
    /// </summary>
    public sealed class TracingPolicyEngine : IAiPolicyEngine
    {
        private readonly IAiPolicyEngine _inner;
        private readonly ITraceRecorder _recorder;

        public TracingPolicyEngine(IAiPolicyEngine inner, ITraceRecorder recorder)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        }

        public string PolicyVersion { get { return _inner.PolicyVersion; } }

        public AiPolicySet GetPolicySnapshot()
        {
            return _inner.GetPolicySnapshot();
        }

        public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var metadata = new TraceMetadata();
            metadata.Add("policy.operation", context.Operation ?? string.Empty);
            metadata.Add("policy.resource.type", context.ResourceType ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(context.ResourceId))
                metadata.Add("policy.resource.id", context.ResourceId);
            if (!string.IsNullOrWhiteSpace(context.ToolId))
                metadata.Add("policy.tool.id", context.ToolId);
            if (!string.IsNullOrWhiteSpace(context.ProviderId))
                metadata.Add("policy.provider.id", context.ProviderId);

            var span = _recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = TraceAmbient.Current,
                OperationName = "policy.evaluate",
                Kind = "Policy",
                Metadata = metadata
            });

            using (TraceAmbient.Push(span.Context))
            {
                try
                {
                    var decision = _inner.Evaluate(context.Clone());
                    if (decision == null)
                    {
                        span.TryComplete(TraceSpanStatus.Failed);
                        return null;
                    }

                    var decisionMetadata = span.Record.Metadata;
                    if (decision.IsDenied || decision.RequiresApproval)
                        decisionMetadata.Add("policy.outcome", decision.Outcome.ToString());
                    else if (decision.IsDeferred)
                        decisionMetadata.Add("policy.outcome", decision.Outcome.ToString());
                    else
                        decisionMetadata.Add("policy.outcome", decision.Outcome.ToString());

                    if (!string.IsNullOrWhiteSpace(decision.RuleId))
                        decisionMetadata.Add("policy.rule.id", decision.RuleId);
                    if (!string.IsNullOrWhiteSpace(decision.Reason))
                    {
                        var boundedReason = decision.Reason.Length > 512
                            ? decision.Reason.Substring(0, 512)
                            : decision.Reason;
                        decisionMetadata.Add("policy.reason", boundedReason);
                    }

                    span.TryComplete(decision.IsDenied || decision.RequiresApproval
                        ? TraceSpanStatus.Rejected
                        : decision.IsDeferred
                            ? TraceSpanStatus.Skipped
                            : TraceSpanStatus.Succeeded);
                    return decision;
                }
                catch
                {
                    span.TryComplete(TraceSpanStatus.Failed);
                    throw;
                }
            }
        }
    }
}
