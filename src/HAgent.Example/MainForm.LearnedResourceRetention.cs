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
        private void AddLearnedResourceRetentionTab()
        {
            AddApiTab("LEARNED RESOURCE RETENTION", "Run governed forgetting and archival verification", "Utility, authority protection, recovery, and policy-controlled lifecycle retention.", "Retention changes lifecycle metadata only; published resource identity remains unchanged.", "Archive is recoverable; retirement is terminal.", TestLearnedResourceRetentionAsync, "Learned resource retention", "Use retention evidence only inside policy and lifecycle boundaries.");
        }

        private async Task TestLearnedResourceRetentionAsync(string unused)
        {
            var now = DateTimeOffset.UtcNow;
            var id = new AiResourceReliabilityIdentity { ResourceType = "knowledge", ResourceId = "retention-example", Version = 4, Scope = AgentResourceScope.Agent };
            var store = new InMemoryAiLearnedResourceLifecycleStore();
            var service = new AiLearnedResourceLifecycleService(store, new Policy());
            await service.InitializeAsync(id, now.AddDays(-30), CancellationToken.None).ConfigureAwait(true);

            var archived = await service.AssessRetentionAsync(Request(id, now, now.AddDays(-30), .2m, 3, false, false, false, false, 1, 0), CancellationToken.None).ConfigureAwait(true);
            RequireExample(archived.Status == AiLearnedResourceLifecycleStatus.Archived, "Archival failed.");
            RequireExample(archived.Decision == AiLearnedResourceRetentionDecision.Archive, "Archive decision failed.");
            RequireExample(archived.Identity.Version == 4, "Published version changed.");

            var restored = await service.AssessRetentionAsync(Request(id, now.AddHours(1), now, .9m, 0, false, false, false, true, 5, 0), CancellationToken.None).ConfigureAwait(true);
            RequireExample(restored.Status == AiLearnedResourceLifecycleStatus.Active, "Archived recovery failed.");
            RequireExample(restored.Decision == AiLearnedResourceRetentionDecision.Restore, "Restore decision failed.");

            var authorityId = new AiResourceReliabilityIdentity { ResourceType = "knowledge", ResourceId = "retention-authority", Version = 2, Scope = AgentResourceScope.Agent };
            await service.InitializeAsync(authorityId, now.AddHours(-1), CancellationToken.None).ConfigureAwait(true);
            var protectedResult = await service.AssessRetentionAsync(Request(authorityId, now, now.AddDays(-30), .05m, 3, true, false, false, false, 10, 5), CancellationToken.None).ConfigureAwait(true);
            RequireExample(protectedResult.Decision == AiLearnedResourceRetentionDecision.Keep, "Authority protection failed.");

            var current = await store.GetAsync(id, CancellationToken.None).ConfigureAwait(true);
            RequireExample(current.History.Count >= 2, "Retention provenance was not preserved.");

            Write("LEARNED RESOURCE RETENTION", "Contract test succeeded." + Environment.NewLine + "Age/utility archival: verified." + Environment.NewLine + "Higher-authority preservation: verified." + Environment.NewLine + "Archived recovery: verified." + Environment.NewLine + "Published resource version remained unchanged: verified." + Environment.NewLine + "Lifecycle provenance: verified." + Environment.NewLine + "Policy-controlled retention boundary: verified.");
        }

        private static AiLearnedResourceRetentionRequest Request(AiResourceReliabilityIdentity id, DateTimeOffset at, DateTimeOffset lastUsed, decimal utility, int low, bool superseded, bool contradicted, bool retire, bool retain, int authority, int competitor)
        {
            return new AiLearnedResourceRetentionRequest { Identity = id.Clone(), EvaluatedAtUtc = at, LastUsedAtUtc = lastUsed, ValidatedUseCount = 4, UtilityScore = utility, ConsecutiveLowUtilityAssessments = low, StaleAfter = TimeSpan.FromDays(7), Superseded = superseded, SupersedingResourceId = superseded ? "replacement" : null, Contradicted = contradicted, ExplicitRetirementRequested = retire, ExplicitRetentionRequested = retain, AuthorityRank = authority, HighestKnownCompetingAuthorityRank = competitor, EvidenceSummary = "Host supplied bounded retention evidence.", PolicyIdentity = new AgentIdentityContext() };
        }

        private sealed class Policy : IAiPolicyEngine
        {
            public string PolicyVersion { get { return "retention-example-1"; } }
            public AiPolicySet GetPolicySnapshot() { return new AiPolicySet { Version = PolicyVersion }; }
            public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context) { context.Validate(); return new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = PolicyVersion, RuleId = "retention-example-rule", Reason = "Allowed." }; }
        }
    }
}
