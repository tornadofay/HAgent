using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class AiLearnedResourceLifecycleService
    {
        private readonly IAiLearnedResourceLifecycleStore _store;
        private readonly IAiPolicyEngine _policyEngine;

        public AiLearnedResourceLifecycleService(
            IAiLearnedResourceLifecycleStore store,
            IAiPolicyEngine policyEngine)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _policyEngine = policyEngine ?? throw new ArgumentNullException(nameof(policyEngine));
        }

        public async Task<AiLearnedResourceLifecycleRecord> InitializeAsync(
            AiResourceReliabilityIdentity identity,
            DateTimeOffset promotedAtUtc,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            identity.Validate();
            if (promotedAtUtc == default(DateTimeOffset)) throw new ArgumentException("PromotedAtUtc is required.", nameof(promotedAtUtc));

            cancellationToken.ThrowIfCancellationRequested();
            var existing = await _store.GetAsync(identity, cancellationToken).ConfigureAwait(false);
            if (existing != null) return existing;

            var record = new AiLearnedResourceLifecycleRecord
            {
                Identity = identity.Clone(),
                Revision = 0,
                Status = AiLearnedResourceLifecycleStatus.Active,
                LastCondition = AiLearnedResourceCondition.Current,
                PromotedAtUtc = promotedAtUtc,
                LastReason = "Resource was initialized as an active promoted version."
            };
            record.Validate();

            if (await _store.TryCreateAsync(record, cancellationToken).ConfigureAwait(false))
                return record;

            existing = await _store.GetAsync(identity, cancellationToken).ConfigureAwait(false);
            if (existing == null)
                throw new InvalidOperationException("Lifecycle state could not be created or recovered after concurrent initialization.");
            return existing;
        }

        public async Task<AiLearnedResourceRevalidationResult> RevalidateAsync(
            AiLearnedResourceRevalidationRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var current = await _store.GetAsync(request.Identity, cancellationToken).ConfigureAwait(false);
            if (current == null)
                throw new InvalidOperationException("Lifecycle state must be initialized before revalidation.");

            var previousStatus = current.Status;
            if (current.Status == AiLearnedResourceLifecycleStatus.Retired)
            {
                return CreateResult(current, previousStatus, AiLearnedResourceCondition.Current,
                    "Retired learned resources remain retired and cannot be reactivated by revalidation.",
                    null, null, request.EvaluatedAtUtc);
            }

            var condition = DetermineCondition(current, request);
            var targetStatus = DetermineStatus(current.Status, condition, request.Evaluation);
            var reason = BuildReason(condition, current, request);

            var authorization = EvaluateLifecyclePolicy(request, current, condition, targetStatus);
            if (authorization == null)
                throw new InvalidOperationException("The policy engine returned no learned-resource lifecycle decision.");
            if (!authorization.IsAllowed)
                throw new UnauthorizedAccessException("Learned-resource lifecycle update denied: " + (authorization.Reason ?? string.Empty));

            current.LastCondition = condition;
            current.LastAssessedAtUtc = request.EvaluatedAtUtc;
            current.LastReliabilityRevision = request.Reliability.Revision;
            current.LastReliabilityScore = request.Reliability.ReliabilityScore;
            current.LastApplicabilityOutcome = request.CurrentApplicability.Outcome;
            current.LastEvaluationOutcome = request.Evaluation == null ? (AiEvaluationOutcome?)null : request.Evaluation.Outcome;
            current.LastReason = reason;

            if (targetStatus != current.Status)
            {
                current.History.Add(new AiLearnedResourceLifecycleEvent
                {
                    OccurredAtUtc = request.EvaluatedAtUtc,
                    FromStatus = current.Status,
                    ToStatus = targetStatus,
                    Condition = condition,
                    Reason = reason,
                    PolicyRuleId = authorization.RuleId,
                    PolicyVersion = authorization.PolicyVersion
                });
                while (current.History.Count > 64)
                    current.History.RemoveAt(0);
                current.LastTransitionAtUtc = request.EvaluatedAtUtc;
                current.Status = targetStatus;
            }

            current.Revision++;
            current.Validate();

            var expectedRevision = current.Revision - 1;
            if (!await _store.TryUpdateAsync(current, expectedRevision, cancellationToken).ConfigureAwait(false))
                throw new InvalidOperationException("Learned-resource lifecycle state changed before revalidation could be committed. Retry from the latest revision.");

            return CreateResult(
                current,
                previousStatus,
                condition,
                reason,
                authorization.RuleId,
                authorization.PolicyVersion,
                request.EvaluatedAtUtc);
        }

        public async Task<AiLearningCandidate> CreateReplacementCandidateAsync(
            AiLearnedResourceReplacementCandidateRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var authorization = EvaluateReplacementPolicy(request);
            if (authorization == null)
                throw new InvalidOperationException("The policy engine returned no learned-resource replacement decision.");
            if (!authorization.IsAllowed)
                throw new UnauthorizedAccessException("Learned-resource replacement proposal denied: " + (authorization.Reason ?? string.Empty));

            var candidate = new AiLearningCandidate
            {
                Type = request.CandidateType,
                ProposedScope = request.ProposedScope,
                Provenance = request.Provenance + " Replaces " +
                             request.Identity.ResourceType + ":" + request.Identity.ResourceId +
                             (request.Identity.Version.HasValue ? ":v" + request.Identity.Version.Value : string.Empty) + ".",
                Evidence = request.Evidence,
                SourceExecutionId = request.SourceExecutionId ?? string.Empty,
                SourceRuntimeInstanceId = request.SourceRuntimeInstanceId ?? string.Empty,
                SourceAgentProfileId = request.SourceAgentProfileId ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(candidate.Provenance) || candidate.Provenance.Length > 4096)
                throw new ArgumentException("Replacement candidate provenance is required and bounded.");
            if (string.IsNullOrWhiteSpace(candidate.Evidence) || candidate.Evidence.Length > 4096)
                throw new ArgumentException("Replacement candidate evidence is required and bounded.");

            return await Task.FromResult(candidate).ConfigureAwait(false);
        }

        private static AiLearnedResourceCondition DetermineCondition(
            AiLearnedResourceLifecycleRecord lifecycle,
            AiLearnedResourceRevalidationRequest request)
        {
            if (request.CurrentApplicability.Outcome == AiApplicabilityOutcome.Invalidated ||
                request.Reliability.LastOutcomeKind == AiReliabilityOutcomeKind.Contradiction)
                return AiLearnedResourceCondition.Contradicted;

            if (request.PreviousApplicability.Outcome == AiApplicabilityOutcome.Applicable &&
                request.CurrentApplicability.Outcome == AiApplicabilityOutcome.NotApplicable)
                return AiLearnedResourceCondition.Drifted;

            if (request.Reliability.RequiresReview ||
                request.Reliability.QuarantineRecommended ||
                request.Evaluation != null &&
                (request.Evaluation.Outcome == AiEvaluationOutcome.Failed ||
                 request.Evaluation.Outcome == AiEvaluationOutcome.NeedsReview))
                return AiLearnedResourceCondition.Degraded;

            if (request.EvaluatedAtUtc - lifecycle.PromotedAtUtc >= request.MaxAge)
                return AiLearnedResourceCondition.Stale;

            if (request.CurrentApplicability.Outcome == AiApplicabilityOutcome.Uncertain)
                return AiLearnedResourceCondition.Degraded;

            return AiLearnedResourceCondition.Current;
        }

        private static AiLearnedResourceLifecycleStatus DetermineStatus(
            AiLearnedResourceLifecycleStatus current,
            AiLearnedResourceCondition condition,
            AiEvaluation evaluation)
        {
            if (current == AiLearnedResourceLifecycleStatus.Retired)
                return AiLearnedResourceLifecycleStatus.Retired;

            if (condition == AiLearnedResourceCondition.Contradicted)
                return AiLearnedResourceLifecycleStatus.Quarantined;

            if (condition == AiLearnedResourceCondition.Current)
            {
                if (evaluation != null && evaluation.Outcome != AiEvaluationOutcome.Passed)
                    return AiLearnedResourceLifecycleStatus.UnderReview;
                return AiLearnedResourceLifecycleStatus.Active;
            }

            return AiLearnedResourceLifecycleStatus.UnderReview;
        }

        private static string BuildReason(
            AiLearnedResourceCondition condition,
            AiLearnedResourceLifecycleRecord lifecycle,
            AiLearnedResourceRevalidationRequest request)
        {
            switch (condition)
            {
                case AiLearnedResourceCondition.Contradicted:
                    return "The learned resource has explicit invalidation or a directly contradictory validated outcome; automatic use must stop.";
                case AiLearnedResourceCondition.Drifted:
                    return "A previously applicable learned resource is no longer applicable in the current bounded context.";
                case AiLearnedResourceCondition.Degraded:
                    return "Reliability or evaluation evidence indicates that the learned resource requires review before continued trust.";
                case AiLearnedResourceCondition.Stale:
                    return "The learned resource exceeded its configured freshness window since promotion.";
                default:
                    return lifecycle.Status == AiLearnedResourceLifecycleStatus.Quarantined
                        ? "The revalidated evidence is current and supports returning the resource to active use."
                        : "The learned resource remains current under the supplied applicability, reliability, and evaluation evidence.";
            }
        }

        private AiPolicyDecision EvaluateLifecyclePolicy(
            AiLearnedResourceRevalidationRequest request,
            AiLearnedResourceLifecycleRecord current,
            AiLearnedResourceCondition condition,
            AiLearnedResourceLifecycleStatus targetStatus)
        {
            var context = new AiPolicyEvaluationContext
            {
                Operation = "resource.lifecycle.revalidate",
                ResourceType = request.Identity.ResourceType,
                ResourceId = request.Identity.ResourceId,
                Identity = request.PolicyIdentity.Clone()
            };
            context.Attributes["resourceVersion"] = request.Identity.Version.HasValue ? request.Identity.Version.Value.ToString() : string.Empty;
            context.Attributes["resourceScope"] = request.Identity.Scope.ToString();
            context.Attributes["condition"] = condition.ToString();
            context.Attributes["previousStatus"] = current.Status.ToString();
            context.Attributes["targetStatus"] = targetStatus.ToString();
            context.Attributes["reliabilityScore"] = request.Reliability.ReliabilityScore.ToString("0.000");
            context.Attributes["applicabilityOutcome"] = request.CurrentApplicability.Outcome.ToString();
            context.Attributes["evaluationOutcome"] = request.Evaluation == null ? string.Empty : request.Evaluation.Outcome.ToString();
            context.Validate();
            return _policyEngine.Evaluate(context);
        }

        private AiPolicyDecision EvaluateReplacementPolicy(AiLearnedResourceReplacementCandidateRequest request)
        {
            var context = new AiPolicyEvaluationContext
            {
                Operation = "resource.lifecycle.propose-replacement",
                ResourceType = request.Identity.ResourceType,
                ResourceId = request.Identity.ResourceId,
                Identity = request.PolicyIdentity.Clone()
            };
            context.Attributes["resourceVersion"] = request.Identity.Version.HasValue ? request.Identity.Version.Value.ToString() : string.Empty;
            context.Attributes["resourceScope"] = request.Identity.Scope.ToString();
            context.Attributes["candidateType"] = request.CandidateType.ToString();
            context.Validate();
            return _policyEngine.Evaluate(context);
        }

        private static AiLearnedResourceRevalidationResult CreateResult(
            AiLearnedResourceLifecycleRecord record,
            AiLearnedResourceLifecycleStatus previousStatus,
            AiLearnedResourceCondition condition,
            string reason,
            string policyRuleId,
            string policyVersion,
            DateTimeOffset evaluatedAtUtc)
        {
            var result = new AiLearnedResourceRevalidationResult
            {
                Identity = record.Identity.Clone(),
                PreviousStatus = previousStatus,
                Status = record.Status,
                Condition = condition,
                Revision = record.Revision,
                ReplacementRecommended = condition != AiLearnedResourceCondition.Current,
                PolicyRuleId = policyRuleId,
                PolicyVersion = policyVersion,
                Reason = reason,
                EvaluatedAtUtc = evaluatedAtUtc
            };
            result.Validate();
            return result;
        }
    }
}
