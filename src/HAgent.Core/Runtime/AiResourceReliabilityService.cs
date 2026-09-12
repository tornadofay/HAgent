using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class AiResourceReliabilityService
    {
        private const decimal SuccessDelta = 0.05m;
        private const decimal FailureDelta = -0.10m;
        private const decimal ContradictionDelta = -0.25m;
        private const decimal InvalidPreconditionDelta = -0.15m;
        private const decimal ReviewThreshold = 0.50m;
        private const decimal QuarantineThreshold = 0.25m;

        private readonly IAiResourceReliabilityStore _store;
        private readonly IAiPolicyEngine _policyEngine;

        public AiResourceReliabilityService(
            IAiResourceReliabilityStore store,
            IAiPolicyEngine policyEngine)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _policyEngine = policyEngine ?? throw new ArgumentNullException(nameof(policyEngine));
        }

        public async Task<AiResourceReliabilityRecord> InitializeAsync(
            AiResourceReliabilityIdentity identity,
            AiReliabilityInitialization initialization,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (initialization == null) throw new ArgumentNullException(nameof(initialization));
            identity.Validate();
            initialization.Validate();

            var existing = await _store.GetAsync(identity, cancellationToken).ConfigureAwait(false);
            if (existing != null) return existing;

            var record = new AiResourceReliabilityRecord
            {
                Identity = identity.Clone(),
                Revision = 0,
                ReliabilityScore = initialization.InitialReliability,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };
            record.PromotionEvidence.Add(initialization.PromotionEvidence.Clone());
            record.RequiresReview = record.ReliabilityScore < ReviewThreshold;
            record.QuarantineRecommended = record.ReliabilityScore <= QuarantineThreshold;
            record.Validate();

            if (await _store.TryCreateAsync(record, cancellationToken).ConfigureAwait(false))
                return record;

            existing = await _store.GetAsync(identity, cancellationToken).ConfigureAwait(false);
            if (existing == null)
                throw new InvalidOperationException("Reliability state could not be created or recovered after a concurrent initialization.");
            return existing;
        }

        public async Task<AiResourceReliabilityOutcomeResult> RecordOutcomeAsync(
            AiResourceReliabilityIdentity identity,
            AiValidatedResourceOutcome outcome,
            AgentIdentityContext policyIdentity,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (outcome == null) throw new ArgumentNullException(nameof(outcome));
            if (policyIdentity == null) throw new ArgumentNullException(nameof(policyIdentity));
            identity.Validate();
            outcome.Validate();
            policyIdentity.Validate();

            var current = await _store.GetAsync(identity, cancellationToken).ConfigureAwait(false);
            if (current == null)
                throw new InvalidOperationException("Reliability state must be initialized from promotion evidence before outcome feedback can be recorded.");

            var delta = GetDelta(outcome.Kind);
            var authorization = EvaluatePolicy(identity, current, outcome, policyIdentity, delta);
            if (authorization == null)
                throw new InvalidOperationException("The policy engine returned no reliability decision.");
            if (!authorization.IsAllowed)
                throw new UnauthorizedAccessException("Reliability outcome update denied: " + (authorization.Reason ?? string.Empty));

            var previousScore = current.ReliabilityScore;
            current.ReliabilityScore = Clamp(previousScore + delta);
            current.ValidatedOutcomeCount++;
            switch (outcome.Kind)
            {
                case AiReliabilityOutcomeKind.Success:
                    current.SuccessCount++;
                    break;
                case AiReliabilityOutcomeKind.Failure:
                    current.FailureCount++;
                    break;
                case AiReliabilityOutcomeKind.Contradiction:
                    current.ContradictionCount++;
                    break;
                case AiReliabilityOutcomeKind.InvalidPrecondition:
                    current.InvalidPreconditionCount++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(outcome));
            }

            current.RequiresReview = current.ReliabilityScore < ReviewThreshold || current.ContradictionCount > 0;
            current.QuarantineRecommended = current.ReliabilityScore <= QuarantineThreshold || outcome.Kind == AiReliabilityOutcomeKind.Contradiction;
            current.LastOutcomeAtUtc = outcome.ObservedAtUtc;
            current.LastOutcomeKind = outcome.Kind;
            current.UpdatedAtUtc = DateTimeOffset.UtcNow;
            current.Revision++;

            var evidence = new AiReliabilityEvidence
            {
                Id = Guid.NewGuid().ToString("N"),
                Kind = "operational-outcome." + outcome.Kind,
                Summary = outcome.EvidenceSummary,
                IsPromotionEvidence = false,
                SourceExecutionId = outcome.SourceExecutionId,
                SourceRuntimeInstanceId = outcome.SourceRuntimeInstanceId,
                SourceAgentProfileId = outcome.SourceAgentProfileId,
                EvaluationReferenceId = outcome.EvaluationReferenceId,
                ObservedAtUtc = outcome.ObservedAtUtc
            };
            evidence.Validate();
            current.OutcomeEvidence.Add(evidence);
            while (current.OutcomeEvidence.Count > 64)
                current.OutcomeEvidence.RemoveAt(0);
            current.Validate();

            var expectedRevision = current.Revision - 1;
            if (!await _store.TryUpdateAsync(current, expectedRevision, cancellationToken).ConfigureAwait(false))
                throw new InvalidOperationException("Reliability state changed before this outcome could be committed. Retry from the latest revision.");

            var result = new AiResourceReliabilityOutcomeResult
            {
                Identity = identity.Clone(),
                PreviousRevision = expectedRevision,
                Revision = current.Revision,
                PreviousReliabilityScore = previousScore,
                ReliabilityScore = current.ReliabilityScore,
                OutcomeKind = outcome.Kind,
                Disposition = current.QuarantineRecommended
                    ? AiReliabilityDisposition.QuarantineRecommended
                    : outcome.Kind == AiReliabilityOutcomeKind.Success
                        ? AiReliabilityDisposition.Reinforced
                        : AiReliabilityDisposition.Weakened,
                RequiresReview = current.RequiresReview,
                QuarantineRecommended = current.QuarantineRecommended,
                PolicyRuleId = authorization.RuleId,
                PolicyVersion = authorization.PolicyVersion,
                UpdatedAtUtc = current.UpdatedAtUtc
            };
            result.Validate();
            return result;
        }

        private AiPolicyDecision EvaluatePolicy(
            AiResourceReliabilityIdentity identity,
            AiResourceReliabilityRecord current,
            AiValidatedResourceOutcome outcome,
            AgentIdentityContext policyIdentity,
            decimal delta)
        {
            var context = new AiPolicyEvaluationContext
            {
                Operation = "resource.reliability.record-outcome",
                ResourceType = identity.ResourceType,
                ResourceId = identity.ResourceId,
                RuntimeInstanceId = outcome.SourceRuntimeInstanceId ?? string.Empty,
                ExecutionId = outcome.SourceExecutionId ?? string.Empty,
                AgentProfileId = outcome.SourceAgentProfileId ?? string.Empty,
                Identity = policyIdentity.Clone()
            };
            context.Attributes["resourceVersion"] = identity.Version.HasValue ? identity.Version.Value.ToString() : string.Empty;
            context.Attributes["resourceScope"] = identity.Scope.ToString();
            context.Attributes["outcomeKind"] = outcome.Kind.ToString();
            context.Attributes["validated"] = outcome.IsValidated.ToString();
            context.Attributes["currentReliability"] = current.ReliabilityScore.ToString("0.000");
            context.Attributes["reliabilityDelta"] = delta.ToString("0.000");
            context.Validate();
            return _policyEngine.Evaluate(context);
        }

        private static decimal GetDelta(AiReliabilityOutcomeKind kind)
        {
            switch (kind)
            {
                case AiReliabilityOutcomeKind.Success: return SuccessDelta;
                case AiReliabilityOutcomeKind.Failure: return FailureDelta;
                case AiReliabilityOutcomeKind.Contradiction: return ContradictionDelta;
                case AiReliabilityOutcomeKind.InvalidPrecondition: return InvalidPreconditionDelta;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static decimal Clamp(decimal value)
        {
            if (value < 0m) return 0m;
            if (value > 1m) return 1m;
            return value;
        }
    }
}
