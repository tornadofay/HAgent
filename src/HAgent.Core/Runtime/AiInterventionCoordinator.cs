using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Owns the runtime-side intervention control boundary. It resolves authorization, routes target actions,
    /// and never persists or executes target handlers itself.
    /// </summary>
    public sealed class AiInterventionCoordinator
    {
        private readonly object _sync = new object();
        private readonly IAiInterventionWorkflow _workflow;
        private readonly Func<CancellationToken, Task<IAiPolicyEngine>> _policyResolver;
        private readonly IAiInterventionAuthorizer _authorizer;
        private readonly Dictionary<string, AgentExecution> _executions = new Dictionary<string, AgentExecution>(StringComparer.OrdinalIgnoreCase);
        private readonly List<IAiInterventionTargetHandler> _handlers = new List<IAiInterventionTargetHandler>();

        internal event EventHandler<AgentExecutionEventArgs> ExecutionChanged;

        public AiInterventionCoordinator(
            IAiInterventionWorkflow workflow,
            Func<CancellationToken, Task<IAiPolicyEngine>> policyResolver,
            IAiInterventionAuthorizer authorizer = null)
        {
            _workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
            _policyResolver = policyResolver ?? throw new ArgumentNullException(nameof(policyResolver));
            _authorizer = authorizer;
            _handlers.Add(new AgentExecutionInterventionHandler(this));
        }

        public IAiInterventionWorkflow Workflow { get { return _workflow; } }

        internal void TrackExecution(AgentExecution execution)
        {
            if (execution == null) return;
            lock (_sync)
            {
                if (!execution.IsCompleted)
                    _executions[execution.Id] = execution;
                else
                    _executions.Remove(execution.Id);
            }
        }

        internal void UntrackExecution(AgentExecution execution)
        {
            if (execution == null) return;
            lock (_sync) _executions.Remove(execution.Id);
        }

        public void RegisterTargetHandler(IAiInterventionTargetHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_sync) _handlers.Add(handler);
        }

        public bool UnregisterTargetHandler(IAiInterventionTargetHandler handler)
        {
            if (handler == null) return false;
            lock (_sync) return _handlers.Remove(handler);
        }

        public async Task<AiInterventionRequest> ResolveAsync(
            string requestId,
            AiInterventionRequestStatus resolution,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = await RequireRequestAsync(requestId, cancellationToken).ConfigureAwait(false);
            if (request.Status != AiInterventionRequestStatus.Pending)
                throw new InvalidOperationException("Intervention request is no longer pending: " + request.RequestId);
            if (resolution != AiInterventionRequestStatus.Approved &&
                resolution != AiInterventionRequestStatus.Rejected &&
                resolution != AiInterventionRequestStatus.Cancelled &&
                resolution != AiInterventionRequestStatus.Expired)
                throw new ArgumentOutOfRangeException(nameof(resolution));

            await AuthorizeAsync(request, responderIdentity, "intervention.resolve", cancellationToken).ConfigureAwait(false);
            return await _workflow.ResolveAsync(requestId, resolution, responderIdentity, reason, cancellationToken).ConfigureAwait(false);
        }

        public async Task<AiInterventionApplicationResult> ApplyAsync(
            string requestId,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = await RequireRequestAsync(requestId, cancellationToken).ConfigureAwait(false);
            if (request.Status != AiInterventionRequestStatus.Approved)
                return AiInterventionApplicationResult.Create(
                    AiInterventionApplicationStatus.Rejected,
                    request.RequestId,
                    "Intervention must be explicitly approved before its target action can be applied.");

            await AuthorizeAsync(request, responderIdentity, "intervention.apply", cancellationToken).ConfigureAwait(false);

            IAiInterventionTargetHandler handler = null;
            lock (_sync)
            {
                handler = _handlers.FirstOrDefault(x => x != null && x.CanHandle(request));
            }

            if (handler == null)
                return AiInterventionApplicationResult.Create(
                    AiInterventionApplicationStatus.NotApplicable,
                    request.RequestId,
                    "No runtime target handler is registered for the intervention target.");

            var result = await handler.ApplyAsync(request, cancellationToken).ConfigureAwait(false);
            if (result == null)
                result = AiInterventionApplicationResult.Create(
                    AiInterventionApplicationStatus.Failed,
                    request.RequestId,
                    "The intervention target handler returned no application result.");

            if (result.Applied)
            {
                await _workflow.CompleteAsync(
                    request.RequestId,
                    responderIdentity,
                    string.IsNullOrWhiteSpace(reason) ? result.Reason : reason,
                    cancellationToken).ConfigureAwait(false);
                RaiseExecutionChanged(request);
            }

            return result;
        }

        private async Task AuthorizeAsync(
            AiInterventionRequest request,
            AgentIdentityContext responderIdentity,
            string operation,
            CancellationToken cancellationToken)
        {
            var policyEngine = await _policyResolver(cancellationToken).ConfigureAwait(false);
            if (policyEngine != null)
            {
                var decision = policyEngine.Evaluate(new AiPolicyEvaluationContext
                {
                    Operation = operation,
                    ResourceType = "intervention",
                    ResourceId = request.RequestId,
                    AgentProfileId = request.AgentProfileId ?? string.Empty,
                    RuntimeInstanceId = request.RuntimeInstanceId ?? string.Empty,
                    ExecutionId = request.ExecutionId ?? string.Empty,
                    ToolId = request.ToolId ?? string.Empty,
                    Identity = responderIdentity == null ? new AgentIdentityContext() : responderIdentity.Clone(),
                    Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "targetKind", request.TargetKind.ToString() },
                        { "action", request.RequestedAction.ToString() },
                        { "targetId", request.ResourceId ?? string.Empty }
                    }
                });

                if (decision == null || decision.IsDenied || decision.RequiresApproval || decision.IsDeferred)
                {
                    throw new UnauthorizedAccessException(
                        "Intervention authorization was not permitted by policy. " +
                        (decision == null || string.IsNullOrWhiteSpace(decision.Reason)
                            ? "No policy reason was provided."
                            : decision.Reason));
                }
            }

            if (_authorizer != null)
            {
                var allowed = await _authorizer.AuthorizeAsync(
                    new AiInterventionAuthorizationRequest(request, responderIdentity),
                    cancellationToken).ConfigureAwait(false);
                if (!allowed)
                    throw new UnauthorizedAccessException("The host intervention authorization boundary rejected the requested operation.");
            }
        }

        private async Task<AiInterventionRequest> RequireRequestAsync(string requestId, CancellationToken cancellationToken)
        {
            var request = await _workflow.GetAsync(requestId, cancellationToken).ConfigureAwait(false);
            if (request == null)
                throw new InvalidOperationException("Intervention request was not found: " + requestId);
            return request;
        }

        private AgentExecution FindExecution(string executionId)
        {
            if (string.IsNullOrWhiteSpace(executionId)) return null;
            lock (_sync)
            {
                AgentExecution execution;
                return _executions.TryGetValue(executionId, out execution) ? execution : null;
            }
        }

        private void RaiseExecutionChanged(AiInterventionRequest request)
        {
            if (request == null || request.TargetKind != AiInterventionTargetKind.Execution) return;
            var execution = FindExecution(request.ExecutionId);
            if (execution == null) return;
            var handler = ExecutionChanged;
            if (handler != null) handler(this, new AgentExecutionEventArgs(execution));
        }

        private sealed class AgentExecutionInterventionHandler : IAiInterventionTargetHandler
        {
            private readonly AiInterventionCoordinator _owner;

            public AgentExecutionInterventionHandler(AiInterventionCoordinator owner)
            {
                _owner = owner;
            }

            public bool CanHandle(AiInterventionRequest request)
            {
                return request != null && request.TargetKind == AiInterventionTargetKind.Execution;
            }

            public Task<AiInterventionApplicationResult> ApplyAsync(
                AiInterventionRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var execution = _owner.FindExecution(request.ExecutionId);
                if (execution == null)
                    return Task.FromResult(AiInterventionApplicationResult.Create(
                        AiInterventionApplicationStatus.Stale,
                        request.RequestId,
                        "The target execution is no longer active and the intervention is stale."));

                if (execution.ControlRevision != request.TargetRevision)
                    return Task.FromResult(AiInterventionApplicationResult.Create(
                        AiInterventionApplicationStatus.Stale,
                        request.RequestId,
                        "The target execution changed state after this intervention request was created."));

                bool applied;
                switch (request.RequestedAction)
                {
                    case AiInterventionAction.Inspect:
                        applied = true;
                        break;
                    case AiInterventionAction.Pause:
                        applied = execution.TryPauseByIntervention();
                        break;
                    case AiInterventionAction.Resume:
                        applied = execution.TryResumeByIntervention();
                        break;
                    case AiInterventionAction.Cancel:
                        applied = execution.TryCancelByIntervention(request.ResolutionReason);
                        break;
                    default:
                        return Task.FromResult(AiInterventionApplicationResult.Create(
                            AiInterventionApplicationStatus.NotApplicable,
                            request.RequestId,
                            "The execution runtime does not implement the requested intervention action."));
                }

                return Task.FromResult(AiInterventionApplicationResult.Create(
                    applied ? AiInterventionApplicationStatus.Applied : AiInterventionApplicationStatus.Stale,
                    request.RequestId,
                    applied ? "Execution intervention was applied." : "The target execution can no longer accept this intervention."));
            }
        }
    }
}
