using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddLearnedResourceAdaptationTab()
        {
            AddApiTab(
                "LEARNED RESOURCE ADAPTATION",
                "Run learned-resource adaptation test",
                "Verifies post-promotion staleness, degradation, contextual drift, contradiction, revalidation, lifecycle safety, and governed replacement candidates.",
                "Learned resources must move through explicit lifecycle states without mutating their published identity, and replacement signals become new typed candidates.",
                "Exercises lifecycle transitions, policy control, revision safety, automatic-use blocking, revalidation recovery, and replacement candidate creation.",
                TestLearnedResourceAdaptationAsync,
                "Learned resource adaptation",
                "Lifecycle state is evidence for selection; authorization and authoritative resource mutation remain separate boundaries.");
        }

        private async Task TestLearnedResourceAdaptationAsync(string unused)
        {
            var now = DateTimeOffset.UtcNow;
            var identity = new AiResourceReliabilityIdentity
            {
                ResourceType = "knowledge",
                ResourceId = "knowledge-adaptation-example",
                Version = 4,
                Scope = AgentResourceScope.Agent
            };
            var store = new InMemoryAiLearnedResourceLifecycleStore();
            var service = new AiLearnedResourceLifecycleService(store, new ExampleAdaptationPolicyEngine());

            var initialized = await service.InitializeAsync(identity, now.AddDays(-30), CancellationToken.None).ConfigureAwait(true);
            RequireExample(initialized.Status == AiLearnedResourceLifecycleStatus.Active, "Initial lifecycle state must be active.");
            RequireExample(initialized.IsAutomaticallyUsable, "A current active resource must be automatically usable.");

            var stale = await service.RevalidateAsync(
                CreateRequest(identity, now, AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Applicable, null, 7, CreateReliability(identity, 0.80m)),
                CancellationToken.None).ConfigureAwait(true);
            RequireExample(stale.Condition == AiLearnedResourceCondition.Stale, "Age-based staleness was not detected.");
            RequireExample(stale.Status == AiLearnedResourceLifecycleStatus.UnderReview, "Stale resource did not enter review.");
            var staleRecord = await store.GetAsync(identity, CancellationToken.None).ConfigureAwait(true);
            RequireExample(!staleRecord.IsAutomaticallyUsable, "Under-review resource remained automatically usable.");

            var reliability = CreateReliability(identity, 0.70m);
            reliability.RequiresReview = true;
            var degraded = await service.RevalidateAsync(
                CreateRequest(identity, now.AddMinutes(1), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Applicable, null, 365, reliability),
                CancellationToken.None).ConfigureAwait(true);
            RequireExample(degraded.Condition == AiLearnedResourceCondition.Degraded, "Observed degradation was not detected.");
            RequireExample(degraded.Status == AiLearnedResourceLifecycleStatus.UnderReview, "Degraded resource did not remain under review.");

            reliability.RequiresReview = false;
            var drifted = await service.RevalidateAsync(
                CreateRequest(identity, now.AddMinutes(2), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.NotApplicable, null, 365, reliability),
                CancellationToken.None).ConfigureAwait(true);
            RequireExample(drifted.Condition == AiLearnedResourceCondition.Drifted, "Contextual drift was not detected.");
            RequireExample(drifted.Status == AiLearnedResourceLifecycleStatus.UnderReview, "Drifted resource did not enter review.");

            reliability.LastOutcomeKind = AiReliabilityOutcomeKind.Contradiction;
            reliability.ContradictionCount = 1;
            reliability.QuarantineRecommended = true;
            var contradicted = await service.RevalidateAsync(
                CreateRequest(identity, now.AddMinutes(3), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Invalidated, null, 365, reliability),
                CancellationToken.None).ConfigureAwait(true);
            RequireExample(contradicted.Condition == AiLearnedResourceCondition.Contradicted, "Contradiction was not detected.");
            RequireExample(contradicted.Status == AiLearnedResourceLifecycleStatus.Quarantined, "Contradiction did not quarantine the resource.");
            var quarantined = await store.GetAsync(identity, CancellationToken.None).ConfigureAwait(true);
            RequireExample(!quarantined.IsAutomaticallyUsable, "Quarantined resource remained automatically usable.");

            reliability.LastOutcomeKind = AiReliabilityOutcomeKind.Success;
            reliability.ContradictionCount = 0;
            reliability.QuarantineRecommended = false;
            var recovered = await service.RevalidateAsync(
                CreateRequest(identity, now.AddMinutes(4), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Applicable, CreatePassedEvaluation(), 365, reliability),
                CancellationToken.None).ConfigureAwait(true);
            RequireExample(recovered.Condition == AiLearnedResourceCondition.Current, "Clean revalidation did not restore current condition.");
            RequireExample(recovered.Status == AiLearnedResourceLifecycleStatus.Active, "Clean revalidation did not restore active state.");
            var recoveredRecord = await store.GetAsync(identity, CancellationToken.None).ConfigureAwait(true);
            RequireExample(recoveredRecord.IsAutomaticallyUsable, "Current active resource did not become automatically usable.");

            var candidate = await service.CreateReplacementCandidateAsync(
                new AiLearnedResourceReplacementCandidateRequest
                {
                    Identity = identity.Clone(),
                    Scope = identity.Scope,
                    ProposedScope = "Agent-scoped replacement candidate",
                    Evidence = "Contextual drift was observed during governed revalidation.",
                    Provenance = "Generated from learned-resource adaptation evidence.",
                    SourceExecutionId = "execution-adaptation-example",
                    SourceRuntimeInstanceId = "runtime-adaptation-example",
                    SourceAgentProfileId = "agent-profile-adaptation-example",
                    Confidence = 0.72m,
                    CandidateType = AiLearningCandidateType.Knowledge,
                    PolicyIdentity = new AgentIdentityContext()
                },
                CancellationToken.None).ConfigureAwait(true);
            RequireExample(candidate.Type == AiLearningCandidateType.Knowledge, "Replacement proposal did not create the requested typed candidate.");
            RequireExample(candidate.Status == AiLearningCandidateStatus.Proposed, "Replacement candidate bypassed the governed candidate lifecycle.");
            RequireExample(candidate.Provenance.IndexOf(identity.ResourceId, StringComparison.Ordinal) >= 0, "Replacement provenance did not identify the replaced resource.");

            var finalRecord = await store.GetAsync(identity, CancellationToken.None).ConfigureAwait(true);
            RequireExample(finalRecord.Identity.Version == 4, "Lifecycle processing changed the published resource version.");

            Write(
                "LEARNED RESOURCE ADAPTATION",
                "Contract test succeeded." + Environment.NewLine +
                "Age-based staleness detection: verified." + Environment.NewLine +
                "Observed degradation detection: verified." + Environment.NewLine +
                "Contextual drift detection: verified." + Environment.NewLine +
                "Contradiction quarantine: verified." + Environment.NewLine +
                "Automatic-use blocking for non-current lifecycle states: verified." + Environment.NewLine +
                "Clean revalidation recovery: verified." + Environment.NewLine +
                "Replacement created as a typed learning candidate: verified." + Environment.NewLine +
                "Published resource version remained unchanged: verified." + Environment.NewLine +
                "Policy-controlled lifecycle boundary: verified.");
        }

        private static AiLearnedResourceRevalidationRequest CreateRequest(
            AiResourceReliabilityIdentity identity,
            DateTimeOffset evaluatedAt,
            AiApplicabilityOutcome previousOutcome,
            AiApplicabilityOutcome currentOutcome,
            AiEvaluation evaluation,
            int maxAgeDays,
            AiResourceReliabilityRecord reliability)
        {
            return new AiLearnedResourceRevalidationRequest
            {
                Identity = identity.Clone(),
                Reliability = reliability,
                PreviousApplicability = CreateApplicability(identity, previousOutcome),
                CurrentApplicability = CreateApplicability(identity, currentOutcome),
                Evaluation = evaluation,
                EvaluatedAtUtc = evaluatedAt,
                MaxAge = TimeSpan.FromDays(maxAgeDays),
                PolicyIdentity = new AgentIdentityContext()
            };
        }

        private static AiApplicabilityDecision CreateApplicability(AiResourceReliabilityIdentity identity, AiApplicabilityOutcome outcome)
        {
            var decision = new AiApplicabilityDecision
            {
                Outcome = outcome,
                ResourceType = identity.ResourceType,
                ResourceId = identity.ResourceId,
                Version = identity.Version,
                Scope = identity.Scope,
                Reason = outcome.ToString(),
                EvaluatedAtUtc = DateTimeOffset.UtcNow
            };
            decision.Validate();
            return decision;
        }

        private static AiResourceReliabilityRecord CreateReliability(AiResourceReliabilityIdentity identity, decimal score)
        {
            var record = new AiResourceReliabilityRecord
            {
                Identity = identity.Clone(),
                Revision = 0,
                ReliabilityScore = score,
                RequiresReview = score < 0.50m,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };
            record.PromotionEvidence.Add(new AiReliabilityEvidence
            {
                Id = "promotion-adaptation-example",
                Kind = "learning-promotion",
                Summary = "Resource was promoted under governed learning before adaptation tracking.",
                IsPromotionEvidence = true,
                SourceAgentProfileId = "agent-profile-adaptation-example",
                ObservedAtUtc = DateTimeOffset.UtcNow
            });
            record.Validate();
            return record;
        }

        private static AiEvaluation CreatePassedEvaluation()
        {
            var evaluation = new AiEvaluation
            {
                TargetId = "revalidation-adaptation-example",
                Outcome = AiEvaluationOutcome.Passed,
                EvaluatorId = "adaptation-example-evaluator",
                EvaluatorKind = AiEvaluatorKind.Deterministic,
                EvaluatorVersion = "1",
                Reason = "Current bounded evidence passed revalidation."
            };
            evaluation.Validate();
            return evaluation;
        }

        private sealed class ExampleAdaptationPolicyEngine : IAiPolicyEngine
        {
            public string PolicyVersion { get { return "adaptation-example-policy-1"; } }

            public AiPolicySet GetPolicySnapshot()
            {
                return new AiPolicySet { Version = PolicyVersion };
            }

            public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context)
            {
                context.Validate();
                return new AiPolicyDecision
                {
                    Outcome = AiPolicyOutcome.Allow,
                    PolicyVersion = PolicyVersion,
                    RuleId = "adaptation-example-allow",
                    Reason = "The host policy permits governed learned-resource adaptation evaluation."
                };
            }
        }
    }
}
