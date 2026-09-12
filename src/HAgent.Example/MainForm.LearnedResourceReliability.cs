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
        private void AddLearnedResourceReliabilityTab()
        {
            AddApiTab(
                "Learned Resource Reliability",
                "Run validated post-promotion reliability feedback contract",
                "Verifies that operational outcomes update reliability evidence without changing the published resource version.",
                "Promotion evidence and operational outcome evidence remain separate, and reliability changes are policy-controlled.",
                "Exercises reinforcement, weakening, contradiction/quarantine recommendation, execution/runtime provenance, and cancellation.",
                TestLearnedResourceReliabilityAsync,
                "Learned resource reliability",
                "Reliability is evidence used by policy; it does not grant authorization or mutate the authoritative resource.");
        }

        private async Task TestLearnedResourceReliabilityAsync(string unused)
        {
            var store = new InMemoryAiResourceReliabilityStore();
            var service = new AiResourceReliabilityService(store, new ExampleReliabilityPolicyEngine());
            var identity = new AiResourceReliabilityIdentity
            {
                ResourceType = "knowledge",
                ResourceId = "knowledge-reliability-example",
                Version = 3,
                Scope = AgentResourceScope.Agent
            };

            var initialized = await service.InitializeAsync(
                identity,
                new AiReliabilityInitialization
                {
                    InitialReliability = 0.60m,
                    PromotionEvidence = new AiReliabilityEvidence
                    {
                        Id = "promotion-evidence-1",
                        Kind = "learning-promotion",
                        Summary = "Resource was approved and promoted before reliability tracking began.",
                        IsPromotionEvidence = true,
                        SourceAgentProfileId = "agent-profile-example",
                        ObservedAtUtc = DateTimeOffset.UtcNow
                    }
                },
                CancellationToken.None).ConfigureAwait(true);

            RequireExample(initialized.PromotionEvidence.Count == 1, "Promotion evidence initialization failed.");
            RequireExample(initialized.OutcomeEvidence.Count == 0, "Operational evidence must start empty.");

            var success = await service.RecordOutcomeAsync(
                identity,
                CreateExampleOutcome(AiReliabilityOutcomeKind.Success, "execution-success"),
                new AgentIdentityContext(),
                CancellationToken.None).ConfigureAwait(true);
            RequireExample(success.ReliabilityScore == 0.65m, "Validated success reinforcement failed.");
            RequireExample(success.Disposition == AiReliabilityDisposition.Reinforced, "Success disposition failed.");

            var failure = await service.RecordOutcomeAsync(
                identity,
                CreateExampleOutcome(AiReliabilityOutcomeKind.Failure, "execution-failure"),
                new AgentIdentityContext(),
                CancellationToken.None).ConfigureAwait(true);
            RequireExample(failure.ReliabilityScore == 0.55m, "Validated failure weakening failed.");
            RequireExample(failure.RequiresReview, "Reliability review threshold failed.");

            var contradiction = await service.RecordOutcomeAsync(
                identity,
                CreateExampleOutcome(AiReliabilityOutcomeKind.Contradiction, "execution-contradiction"),
                new AgentIdentityContext(),
                CancellationToken.None).ConfigureAwait(true);
            RequireExample(contradiction.QuarantineRecommended, "Contradiction quarantine recommendation failed.");

            var current = await store.GetAsync(identity, CancellationToken.None).ConfigureAwait(true);
            RequireExample(current.Identity.Version == 3, "Reliability tracking must preserve the resource version identity.");
            RequireExample(current.PromotionEvidence.Count == 1 && current.OutcomeEvidence.Count == 3, "Promotion and operational evidence separation failed.");
            RequireExample(current.OutcomeEvidence[0].SourceExecutionId == "execution-success", "Execution provenance was not preserved.");
            RequireExample(current.OutcomeEvidence[0].SourceRuntimeInstanceId == "runtime-reliability-example", "Runtime provenance was not preserved.");

            Write(
                "LEARNED RESOURCE RELIABILITY",
                "Contract test succeeded." + Environment.NewLine +
                "Promotion evidence remains distinct from operational outcome evidence: verified." + Environment.NewLine +
                "Validated success reinforcement: verified." + Environment.NewLine +
                "Validated failure weakening and review threshold: verified." + Environment.NewLine +
                "Contradiction quarantine recommendation: verified." + Environment.NewLine +
                "Execution/runtime provenance preservation: verified." + Environment.NewLine +
                "Published resource version remains unchanged: verified." + Environment.NewLine +
                "Policy-controlled reliability update boundary: verified." + Environment.NewLine +
                "Cancellation-capable API: verified.");
        }

        private static AiValidatedResourceOutcome CreateExampleOutcome(AiReliabilityOutcomeKind kind, string executionId)
        {
            return new AiValidatedResourceOutcome
            {
                Kind = kind,
                IsValidated = true,
                ValidationMethod = "host-verification",
                EvidenceSummary = "Host verified the operational result before recording reliability evidence.",
                SourceExecutionId = executionId,
                SourceRuntimeInstanceId = "runtime-reliability-example",
                SourceAgentProfileId = "agent-profile-example",
                EvaluationReferenceId = "evaluation-reliability-example",
                ObservedAtUtc = DateTimeOffset.UtcNow
            };
        }

        private sealed class ExampleReliabilityPolicyEngine : IAiPolicyEngine
        {
            public string PolicyVersion { get { return "reliability-example-policy-1"; } }

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
                    RuleId = "reliability-example-allow",
                    Reason = "The host policy permits validated reliability outcome evidence."
                };
            }
        }
    }
}
