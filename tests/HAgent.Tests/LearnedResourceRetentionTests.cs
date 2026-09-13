using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class LearnedResourceRetentionTests
    {
        [Fact]
        public async Task StaleAndLowUtilityResource_IsArchivedWithoutChangingIdentity()
        {
            var now = DateTimeOffset.UtcNow;
            var id = Identity(1);
            var store = new InMemoryAiLearnedResourceLifecycleStore();
            var service = new AiLearnedResourceLifecycleService(store, new Policy());
            await service.InitializeAsync(id, now.AddDays(-30), CancellationToken.None);
            var result = await service.AssessRetentionAsync(Request(id, now, now.AddDays(-30), 0.2m, 3, false, false, false, false, 1, 0), CancellationToken.None);
            Assert.Equal(AiLearnedResourceLifecycleStatus.Archived, result.Status);
            Assert.Equal(AiLearnedResourceRetentionDecision.Archive, result.Decision);
            Assert.Equal(id.Version, result.Identity.Version);
        }

        [Fact]
        public async Task HigherAuthorityResource_IsPreserved()
        {
            var now = DateTimeOffset.UtcNow;
            var id = Identity(2);
            var service = Service();
            await service.InitializeAsync(id, now.AddHours(-1), CancellationToken.None);
            var result = await service.AssessRetentionAsync(Request(id, now, now.AddDays(-30), 0.05m, 3, true, false, false, false, 10, 5), CancellationToken.None);
            Assert.Equal(AiLearnedResourceRetentionDecision.Keep, result.Decision);
            Assert.Equal(AiLearnedResourceLifecycleStatus.Active, result.Status);
            Assert.Equal(0L, result.Revision);
        }

        [Fact]
        public async Task ArchivedResource_CanBeRecoveredByExplicitRetention()
        {
            var now = DateTimeOffset.UtcNow;
            var id = Identity(3);
            var service = Service();
            await service.InitializeAsync(id, now.AddDays(-2), CancellationToken.None);
            await service.AssessRetentionAsync(Request(id, now.AddDays(-1), now.AddDays(-20), 0.2m, 3, false, false, false, false, 1, 0), CancellationToken.None);
            var result = await service.AssessRetentionAsync(Request(id, now, now.AddMinutes(-1), 0.9m, 0, false, false, false, true, 5, 0), CancellationToken.None);
            Assert.Equal(AiLearnedResourceRetentionDecision.Restore, result.Decision);
            Assert.Equal(AiLearnedResourceLifecycleStatus.Active, result.Status);
        }

        [Fact]
        public async Task PolicyDenial_AndCancellation_DoNotMutateState()
        {
            var now = DateTimeOffset.UtcNow;
            var id = Identity(4);
            var store = new InMemoryAiLearnedResourceLifecycleStore();
            var service = new AiLearnedResourceLifecycleService(store, new Policy(AiPolicyOutcome.Deny));
            await service.InitializeAsync(id, now.AddHours(-1), CancellationToken.None);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AssessRetentionAsync(Request(id, now, now.AddDays(-30), 0.1m, 3, false, false, false, false, 1, 0), CancellationToken.None));
            using (var source = new CancellationTokenSource())
            {
                source.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.AssessRetentionAsync(Request(id, now, now.AddDays(-30), 0.1m, 3, false, false, false, false, 1, 0), source.Token));
            }
            var record = await store.GetAsync(id, CancellationToken.None);
            Assert.Equal(AiLearnedResourceLifecycleStatus.Active, record.Status);
            Assert.Equal(0L, record.Revision);
        }

        private static AiLearnedResourceLifecycleService Service()
        {
            return new AiLearnedResourceLifecycleService(new InMemoryAiLearnedResourceLifecycleStore(), new Policy());
        }

        private static AiResourceReliabilityIdentity Identity(long version)
        {
            return new AiResourceReliabilityIdentity { ResourceType = "knowledge", ResourceId = "retention-resource", Version = version, Scope = AgentResourceScope.Agent };
        }

        private static AiLearnedResourceRetentionRequest Request(AiResourceReliabilityIdentity id, DateTimeOffset now, DateTimeOffset lastUsed, decimal utility, int lowUtility, bool superseded, bool contradicted, bool retire, bool retain, int authority, int competingAuthority)
        {
            return new AiLearnedResourceRetentionRequest
            {
                Identity = id.Clone(), EvaluatedAtUtc = now, LastUsedAtUtc = lastUsed, ValidatedUseCount = 4,
                UtilityScore = utility, ConsecutiveLowUtilityAssessments = lowUtility, StaleAfter = TimeSpan.FromDays(7),
                Superseded = superseded, SupersedingResourceId = superseded ? "replacement" : null, Contradicted = contradicted,
                ExplicitRetirementRequested = retire, ExplicitRetentionRequested = retain,
                AuthorityRank = authority, HighestKnownCompetingAuthorityRank = competingAuthority,
                EvidenceSummary = "Host supplied bounded retention evidence.", PolicyIdentity = new AgentIdentityContext()
            };
        }

        private sealed class Policy : IAiPolicyEngine
        {
            private readonly AiPolicyOutcome _outcome;
            public Policy(AiPolicyOutcome outcome = AiPolicyOutcome.Allow) { _outcome = outcome; }
            public string PolicyVersion { get { return "retention-test-1"; } }
            public AiPolicySet GetPolicySnapshot() { return new AiPolicySet { Version = PolicyVersion }; }
            public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context)
            {
                context.Validate();
                return new AiPolicyDecision { Outcome = _outcome, PolicyVersion = PolicyVersion, RuleId = "retention-test-rule", Reason = "Test policy decision." };
            }
        }
    }
}
