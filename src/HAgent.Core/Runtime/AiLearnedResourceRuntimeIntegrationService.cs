using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class AiLearnedResourceRuntimeIntegrationService
    {
        private readonly IAiResourceReliabilityStore _reliabilityStore;
        private readonly IAiLearnedResourceLifecycleStore _lifecycleStore;
        private readonly IAiPolicyEngine _policyEngine;

        public AiLearnedResourceRuntimeIntegrationService(
            IAiResourceReliabilityStore reliabilityStore,
            IAiLearnedResourceLifecycleStore lifecycleStore,
            IAiPolicyEngine policyEngine)
        {
            _reliabilityStore = reliabilityStore ?? throw new ArgumentNullException(nameof(reliabilityStore));
            _lifecycleStore = lifecycleStore ?? throw new ArgumentNullException(nameof(lifecycleStore));
            _policyEngine = policyEngine ?? throw new ArgumentNullException(nameof(policyEngine));
        }

        public async Task<AiLearnedResourceRuntimeAssessmentResult> AssessForExecutionAsync(
            AiLearnedResourceRuntimeAssessmentRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var reliability = await _reliabilityStore.GetAsync(request.Identity, cancellationToken).ConfigureAwait(false);
            var lifecycle = await _lifecycleStore.GetAsync(request.Identity, cancellationToken).ConfigureAwait(false);
            if (reliability == null || lifecycle == null)
                throw new InvalidOperationException("Reliability and lifecycle state must both be initialized before runtime assessment.");

            reliability.Validate();
            lifecycle.Validate();

            var reliabilityCurrent = reliability.Revision == request.ExpectedReliabilityRevision;
            var lifecycleCurrent = lifecycle.Revision == request.ExpectedLifecycleRevision;
            var snapshot = CreateSnapshot(reliability, lifecycle, request.Applicability);

            var policy = EvaluatePolicy(request, snapshot, reliabilityCurrent, lifecycleCurrent);
            if (policy == null)
                throw new InvalidOperationException("The policy engine returned no learned-resource runtime decision.");

            var canUse = reliabilityCurrent && lifecycleCurrent && snapshot.IsSafeToUse;
            if (canUse && policy.IsAllowed)
            {
                var success = new AiLearnedResourceRuntimeAssessmentResult
                {
                    Identity = request.Identity.Clone(),
                    Decision = AiLearnedResourceRuntimeDecision.Use,
                    Fallback = AiLearnedResourceFallbackKind.HostEscalation,
                    ReliabilityRevisionCurrent = true,
                    LifecycleRevisionCurrent = true,
                    Snapshot = snapshot,
                    PolicyRuleId = policy.RuleId,
                    PolicyVersion = policy.PolicyVersion,
                    Reason = "The exact reliability and lifecycle revisions are current; applicability is positive; the resource is active, current, and not under reliability quarantine or review.",
                    EvaluatedAtUtc = DateTimeOffset.UtcNow
                };
                success.Validate();
                return success;
            }

            var fallback = SelectFallback(request, reliabilityCurrent, lifecycleCurrent, snapshot, policy);
            var reason = BuildFallbackReason(reliabilityCurrent, lifecycleCurrent, snapshot, policy);
            var result = new AiLearnedResourceRuntimeAssessmentResult
            {
                Identity = request.Identity.Clone(),
                Decision = AiLearnedResourceRuntimeDecision.FallBack,
                Fallback = fallback,
                ReliabilityRevisionCurrent = reliabilityCurrent,
                LifecycleRevisionCurrent = lifecycleCurrent,
                Snapshot = snapshot,
                PolicyRuleId = policy.RuleId,
                PolicyVersion = policy.PolicyVersion,
                Reason = reason,
                EvaluatedAtUtc = DateTimeOffset.UtcNow
            };
            result.Validate();
            return result;
        }

        private static AiLearnedResourceExecutionSnapshot CreateSnapshot(
            AiResourceReliabilityRecord reliability,
            AiLearnedResourceLifecycleRecord lifecycle,
            AiApplicabilityDecision applicability)
        {
            return new AiLearnedResourceExecutionSnapshot
            {
                Identity = reliability.Identity.Clone(),
                ReliabilityRevision = reliability.Revision,
                ReliabilityScore = reliability.ReliabilityScore,
                ReliabilityRequiresReview = reliability.RequiresReview,
                ReliabilityQuarantineRecommended = reliability.QuarantineRecommended,
                LifecycleRevision = lifecycle.Revision,
                LifecycleStatus = lifecycle.Status,
                LifecycleCondition = lifecycle.LastCondition,
                RetentionDecision = lifecycle.LastRetentionDecision,
                ApplicabilityOutcome = applicability.Outcome,
                CapturedAtUtc = DateTimeOffset.UtcNow
            };
        }

        private AiPolicyDecision EvaluatePolicy(
            AiLearnedResourceRuntimeAssessmentRequest request,
            AiLearnedResourceExecutionSnapshot snapshot,
            bool reliabilityCurrent,
            bool lifecycleCurrent)
        {
            var context = new AiPolicyEvaluationContext
            {
                Operation = "resource.runtime.use",
                ResourceType = request.Identity.ResourceType,
                ResourceId = request.Identity.ResourceId,
                Identity = request.PolicyIdentity.Clone()
            };
            context.Attributes["resourceVersion"] = request.Identity.Version.HasValue ? request.Identity.Version.Value.ToString() : string.Empty;
            context.Attributes["resourceScope"] = request.Identity.Scope.ToString();
            context.Attributes["reliabilityRevision"] = snapshot.ReliabilityRevision.ToString();
            context.Attributes["reliabilityScore"] = snapshot.ReliabilityScore.ToString("0.000");
            context.Attributes["reliabilityCurrent"] = reliabilityCurrent.ToString();
            context.Attributes["lifecycleRevision"] = snapshot.LifecycleRevision.ToString();
            context.Attributes["lifecycleStatus"] = snapshot.LifecycleStatus.ToString();
            context.Attributes["lifecycleCondition"] = snapshot.LifecycleCondition.ToString();
            context.Attributes["lifecycleCurrent"] = lifecycleCurrent.ToString();
            context.Attributes["applicabilityOutcome"] = snapshot.ApplicabilityOutcome.ToString();
            context.Attributes["retentionDecision"] = snapshot.RetentionDecision.ToString();
            context.Validate();
            return _policyEngine.Evaluate(context);
        }

        private static AiLearnedResourceFallbackKind SelectFallback(
            AiLearnedResourceRuntimeAssessmentRequest request,
            bool reliabilityCurrent,
            bool lifecycleCurrent,
            AiLearnedResourceExecutionSnapshot snapshot,
            AiPolicyDecision policy)
        {
            if (policy == null || !policy.IsAllowed)
                return AiLearnedResourceFallbackKind.HostEscalation;

            if (!reliabilityCurrent || !lifecycleCurrent)
                return FirstSafeFallback(request, AiLearnedResourceFallbackKind.HostEscalation);

            if (snapshot.ApplicabilityOutcome == AiApplicabilityOutcome.Invalidated)
                return FirstSafeFallback(request, AiLearnedResourceFallbackKind.DeterministicSafeAction);

            if (snapshot.ApplicabilityOutcome == AiApplicabilityOutcome.Uncertain ||
                snapshot.LifecycleStatus != AiLearnedResourceLifecycleStatus.Active ||
                snapshot.LifecycleCondition != AiLearnedResourceCondition.Current ||
                snapshot.ReliabilityRequiresReview ||
                snapshot.ReliabilityQuarantineRecommended)
                return FirstSafeFallback(request, AiLearnedResourceFallbackKind.BoundedReasoning);

            return FirstSafeFallback(request, AiLearnedResourceFallbackKind.HostEscalation);
        }

        private static AiLearnedResourceFallbackKind FirstSafeFallback(
            AiLearnedResourceRuntimeAssessmentRequest request,
            AiLearnedResourceFallbackKind preferred)
        {
            foreach (var fallback in request.PreferredFallbacks)
            {
                if (fallback == preferred) return fallback;
            }

            foreach (var fallback in request.PreferredFallbacks)
            {
                if (fallback == AiLearnedResourceFallbackKind.DeterministicSafeAction ||
                    fallback == AiLearnedResourceFallbackKind.AlternateResource ||
                    fallback == AiLearnedResourceFallbackKind.BoundedReasoning ||
                    fallback == AiLearnedResourceFallbackKind.HostEscalation)
                    return fallback;
            }

            return AiLearnedResourceFallbackKind.HostEscalation;
        }

        private static string BuildFallbackReason(
            bool reliabilityCurrent,
            bool lifecycleCurrent,
            AiLearnedResourceExecutionSnapshot snapshot,
            AiPolicyDecision policy)
        {
            if (policy == null || !policy.IsAllowed)
                return "Policy did not authorize learned-resource runtime use; the host must remain outside the learned-resource execution path.";
            if (!reliabilityCurrent || !lifecycleCurrent)
                return "The supplied learned-resource execution state is stale; newer reliability or lifecycle state exists and the stale snapshot must not be used.";
            if (snapshot.ApplicabilityOutcome != AiApplicabilityOutcome.Applicable)
                return "The learned resource is not currently applicable with sufficient deterministic confidence.";
            if (snapshot.LifecycleStatus != AiLearnedResourceLifecycleStatus.Active || snapshot.LifecycleCondition != AiLearnedResourceCondition.Current)
                return "The learned resource is not in the Active + Current lifecycle state required for automatic use.";
            if (snapshot.ReliabilityRequiresReview || snapshot.ReliabilityQuarantineRecommended)
                return "Reliability evidence requires review or quarantine before the learned resource can be used automatically.";
            return "The learned-resource runtime boundary did not establish a safe automatic-use condition.";
        }
    }
}
