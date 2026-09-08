using System;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Composes the existing HAgent policy engine and effective resource capability snapshot for context admission.
    /// </summary>
    public sealed class ContextPolicyAdmissionEvaluator : IContextAdmissionEvaluator
    {
        private readonly IAiPolicyEngine _policyEngine;
        private readonly AiResourceCapabilitySnapshot _resourceCapabilities;

        public ContextPolicyAdmissionEvaluator(
            IAiPolicyEngine policyEngine,
            AiResourceCapabilitySnapshot resourceCapabilities)
        {
            if (policyEngine == null) throw new ArgumentNullException(nameof(policyEngine));
            if (resourceCapabilities == null) throw new ArgumentNullException(nameof(resourceCapabilities));

            _policyEngine = policyEngine;
            _resourceCapabilities = resourceCapabilities.Clone();
            _resourceCapabilities.Validate();
        }

        public ContextAdmissionDecision EvaluateSource(
            ContextRetrievalSource source,
            ContextAdmissionContext context)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (context == null) throw new ArgumentNullException(nameof(context));
            source.Validate();
            context.Validate();

            var decision = new ContextAdmissionDecision
            {
                Allowed = true,
                Reason = "Context source is admitted.",
                SourceKind = source.Source.Kind,
                SourceId = source.Source.Id,
                ResourceCapabilityState = _resourceCapabilities.GetState(source.Source.Kind, source.Source.Id)
            };

            if (decision.ResourceCapabilityState == AiResourceCapabilityState.Disabled)
            {
                decision.Allowed = false;
                decision.Reason = "Context source is disabled by effective resource capability configuration.";
                return decision;
            }

            decision.PolicyDecision = _policyEngine.Evaluate(CreatePolicyContext(
                "context.retrieve",
                source.Source.Kind,
                source.Source.Id,
                string.Empty,
                string.Empty,
                context));
            EnsureDecision(decision.PolicyDecision);

            if (IsBlocked(decision.PolicyDecision))
            {
                decision.Allowed = false;
                decision.Reason = BuildPolicyReason(decision.PolicyDecision);
            }

            return decision;
        }

        public ContextAdmissionDecision EvaluateItem(
            ContextRetrievalSource source,
            ContextItem item,
            ContextAdmissionContext context)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (context == null) throw new ArgumentNullException(nameof(context));
            source.Validate();
            item.Validate();
            context.Validate();

            var decision = new ContextAdmissionDecision
            {
                Allowed = true,
                Reason = "Context item is admitted.",
                SourceKind = source.Source.Kind,
                SourceId = source.Source.Id,
                ItemId = item.Id,
                ItemType = item.Type,
                ResourceCapabilityState = _resourceCapabilities.GetState(source.Source.Kind, item.Id)
            };

            if (decision.ResourceCapabilityState == AiResourceCapabilityState.Disabled)
            {
                decision.Allowed = false;
                decision.Reason = "Context item is disabled by effective resource capability configuration.";
                return decision;
            }

            decision.PolicyDecision = _policyEngine.Evaluate(CreatePolicyContext(
                "context.include",
                source.Source.Kind,
                item.Id,
                item.Type,
                source.Source.Id,
                context));
            EnsureDecision(decision.PolicyDecision);

            if (IsBlocked(decision.PolicyDecision))
            {
                decision.Allowed = false;
                decision.Reason = BuildPolicyReason(decision.PolicyDecision);
            }

            return decision;
        }

        private static AiPolicyEvaluationContext CreatePolicyContext(
            string operation,
            string resourceType,
            string resourceId,
            string itemType,
            string sourceId,
            ContextAdmissionContext context)
        {
            var evaluation = new AiPolicyEvaluationContext
            {
                Operation = operation,
                ResourceType = resourceType ?? string.Empty,
                ResourceId = resourceId ?? string.Empty,
                AgentProfileId = context.AgentProfileId ?? string.Empty,
                RuntimeInstanceId = context.RuntimeInstanceId ?? string.Empty,
                ExecutionId = context.ExecutionId ?? string.Empty,
                Identity = context.Identity == null ? new AgentIdentityContext() : context.Identity.Clone()
            };

            if (!string.IsNullOrWhiteSpace(itemType))
                evaluation.Attributes["context.itemType"] = itemType;
            if (!string.IsNullOrWhiteSpace(sourceId))
                evaluation.Attributes["context.sourceId"] = sourceId;
            return evaluation;
        }

        private static bool IsBlocked(AiPolicyDecision decision)
        {
            return decision.IsDenied || decision.RequiresApproval || decision.IsDeferred;
        }

        private static string BuildPolicyReason(AiPolicyDecision decision)
        {
            var reason = string.IsNullOrWhiteSpace(decision.Reason) ? "No policy reason was supplied." : decision.Reason;
            if (decision.RequiresApproval)
                return "Context admission requires approval by policy. " + reason;
            if (decision.IsDeferred)
                return "Context admission was deferred by policy. " + reason;
            return "Context admission was denied by policy. " + reason;
        }

        private static void EnsureDecision(AiPolicyDecision decision)
        {
            if (decision == null)
                throw new InvalidOperationException("The policy engine returned no context admission decision.");
        }
    }
}
