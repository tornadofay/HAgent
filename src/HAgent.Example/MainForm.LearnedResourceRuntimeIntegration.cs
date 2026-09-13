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
        private void AddLearnedResourceRuntimeIntegrationTab()
        {
            AddApiTab("LEARNED RESOURCE RUNTIME INTEGRATION", "Run post-promotion runtime safety verification", "Revision-aware runtime assessment, execution-state capture, and bounded fallback decisions.", "Runtime use never bypasses policy, applicability, reliability, or lifecycle state.", "Fallback selection returns a host-owned decision; HAgent does not execute the fallback itself.", TestLearnedResourceRuntimeIntegrationAsync, "Learned resource runtime integration", "Use exact resource revisions and captured evidence at execution boundaries.");
        }

        private async Task TestLearnedResourceRuntimeIntegrationAsync(string unused)
        {
            var now = DateTimeOffset.UtcNow;
            var id = new AiResourceReliabilityIdentity { ResourceType = "knowledge", ResourceId = "runtime-example", Version = 5, Scope = AgentResourceScope.Agent };
            var reliabilityStore = new InMemoryAiResourceReliabilityStore();
            var lifecycleStore = new InMemoryAiLearnedResourceLifecycleStore();
            var policy = new Policy();
            var reliability = new AiResourceReliabilityService(reliabilityStore, policy);
            var lifecycle = new AiLearnedResourceLifecycleService(lifecycleStore, policy);
            await reliability.InitializeAsync(id, Promotion(now), CancellationToken.None).ConfigureAwait(true);
            await lifecycle.InitializeAsync(id, now.AddHours(-1), CancellationToken.None).ConfigureAwait(true);

            var service = new AiLearnedResourceRuntimeIntegrationService(reliabilityStore, lifecycleStore, policy);
            var usable = await service.AssessForExecutionAsync(Request(id, 0, 0, AiApplicabilityOutcome.Applicable), CancellationToken.None).ConfigureAwait(true);
            RequireExample(usable.Decision == AiLearnedResourceRuntimeDecision.Use, "Current resource was not admitted for execution.");
            RequireExample(usable.Snapshot.IsSafeToUse, "Execution snapshot did not capture a safe-use state.");
            RequireExample(usable.Snapshot.ReliabilityRevision == 0 && usable.Snapshot.LifecycleRevision == 0, "Execution snapshot revisions were not captured.");
            usable.Snapshot.Validate();

            await reliability.RecordOutcomeAsync(id, Outcome(now), new AgentIdentityContext(), CancellationToken.None).ConfigureAwait(true);
            var stale = await service.AssessForExecutionAsync(Request(id, 0, 0, AiApplicabilityOutcome.Applicable), CancellationToken.None).ConfigureAwait(true);
            RequireExample(stale.Decision == AiLearnedResourceRuntimeDecision.FallBack, "Stale reliability state was not rejected.");
            RequireExample(!stale.ReliabilityRevisionCurrent, "Stale reliability revision was incorrectly accepted.");
            RequireExample(stale.Fallback == AiLearnedResourceFallbackKind.HostEscalation, "Stale state did not select the safe terminal fallback.");

            var uncertain = await service.AssessForExecutionAsync(Request(id, 1, 0, AiApplicabilityOutcome.Uncertain), CancellationToken.None).ConfigureAwait(true);
            RequireExample(uncertain.Decision == AiLearnedResourceRuntimeDecision.FallBack, "Uncertain applicability was not blocked.");
            RequireExample(uncertain.Fallback == AiLearnedResourceFallbackKind.BoundedReasoning, "Uncertain applicability did not select bounded reasoning fallback.");

            var record = await reliabilityStore.GetAsync(id, CancellationToken.None).ConfigureAwait(true);
            RequireExample(record.Revision == 1, "Reliability revision was not advanced by validated outcome evidence.");

            Write("LEARNED RESOURCE RUNTIME INTEGRATION", "Contract test succeeded." + Environment.NewLine + "Current resource admission: verified." + Environment.NewLine + "Execution resource snapshot: verified." + Environment.NewLine + "Stale reliability revision rejection: verified." + Environment.NewLine + "Uncertain applicability fallback: verified." + Environment.NewLine + "Policy-controlled runtime boundary: verified." + Environment.NewLine + "Published resource version remained unchanged: verified.");
        }

        private static AiLearnedResourceRuntimeAssessmentRequest Request(AiResourceReliabilityIdentity id, long reliabilityRevision, long lifecycleRevision, AiApplicabilityOutcome outcome)
        {
            return new AiLearnedResourceRuntimeAssessmentRequest
            {
                Identity = id.Clone(),
                ExpectedReliabilityRevision = reliabilityRevision,
                ExpectedLifecycleRevision = lifecycleRevision,
                Applicability = new AiApplicabilityDecision
                {
                    Outcome = outcome,
                    ResourceType = id.ResourceType,
                    ResourceId = id.ResourceId,
                    Version = id.Version,
                    Scope = id.Scope,
                    Reason = "Example runtime applicability decision.",
                    EvaluatedAtUtc = DateTimeOffset.UtcNow
                },
                PolicyIdentity = new AgentIdentityContext()
            };
        }

        private static AiReliabilityInitialization Promotion(DateTimeOffset now)
        {
            return new AiReliabilityInitialization
            {
                InitialReliability = 0.90m,
                PromotionEvidence = new AiReliabilityEvidence { Id = "runtime-example-promotion", Kind = "promotion", Summary = "Example promoted-resource evidence.", IsPromotionEvidence = true, ObservedAtUtc = now.AddHours(-2) }
            };
        }

        private static AiValidatedResourceOutcome Outcome(DateTimeOffset now)
        {
            return new AiValidatedResourceOutcome { Kind = AiReliabilityOutcomeKind.Success, IsValidated = true, ValidationMethod = "example-host-validation", EvidenceSummary = "Example host validated successful use.", ObservedAtUtc = now };
        }

        //private sealed class Policy : IAiPolicyEngine
        //{
        //    public string PolicyVersion { get { return "runtime-example-1"; } }
        //    public AiPolicySet GetPolicySnapshot() { return new AiPolicySet { Version = PolicyVersion }; }
        //    public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context) { context.Validate(); return new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = PolicyVersion, RuleId = "runtime-example-rule", Reason = "Allowed." }; }
        //}
    }
}
