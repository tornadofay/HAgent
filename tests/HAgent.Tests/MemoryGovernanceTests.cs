using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class MemoryGovernanceTests
    {
        [Fact]
        public void Policy_UsesExactTypeBeforeFamilyAndGlobalLimit()
        {
            var policy = new AiMemoryGovernancePolicy { MaxResults = 20 };
            policy.Rules.Add(new AiMemoryPolicyRule { Family = AiMemoryFamily.Semantic, MaxResults = 8 });
            policy.Rules.Add(new AiMemoryPolicyRule { TypeId = "semantic.preference", MaxResults = 3 });
            policy.Validate();

            Assert.Equal(8, policy.GetMaxResults(new MemoryQuery { Family = AiMemoryFamily.Semantic, TypeId = "semantic.fact" }));
            Assert.Equal(3, policy.GetMaxResults(new MemoryQuery { Family = AiMemoryFamily.Semantic, TypeId = "semantic.preference" }));
            Assert.Equal(2, policy.GetMaxResults(new MemoryQuery { Family = AiMemoryFamily.Semantic, TypeId = "semantic.preference", MaxResults = 2 }));
            Assert.Equal(20, policy.GetMaxResults(new MemoryQuery { Family = AiMemoryFamily.Procedural, TypeId = "procedural.strategy" }));
        }

        [Fact]
        public void Policy_RetentionNeverExtendsExplicitExpiration()
        {
            var created = DateTimeOffset.UtcNow.AddDays(-1);
            var entry = CreateEntry(AiMemoryFamily.Episodic, "episodic.experience", created);
            entry.ExpiresAt = created.AddDays(2);
            var policy = new AiMemoryGovernancePolicy();
            policy.Rules.Add(new AiMemoryPolicyRule { Family = AiMemoryFamily.Episodic, RetentionDays = 30 });

            Assert.Equal(entry.ExpiresAt, policy.GetEffectiveExpiration(entry));
        }

        [Fact]
        public void Policy_RetentionCapsUnboundedMemory()
        {
            var created = DateTimeOffset.UtcNow.AddDays(-1);
            var entry = CreateEntry(AiMemoryFamily.Semantic, "semantic.fact", created);
            var policy = new AiMemoryGovernancePolicy();
            policy.Rules.Add(new AiMemoryPolicyRule { Family = AiMemoryFamily.Semantic, RetentionDays = 7 });

            Assert.Equal(created.AddDays(7), policy.GetEffectiveExpiration(entry));
        }

        [Fact]
        public void CapabilityEvaluator_RejectsDisabledFamilyAndType()
        {
            var profile = new AiResourceCapabilityPolicy();
            profile.Set(AiMemoryResourceTypes.Memory, AiResourceCapabilityState.Enabled);
            profile.Set(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Semantic), AiResourceCapabilityState.Disabled);
            profile.Set(AiMemoryResourceTypes.Type, "semantic.secret", AiResourceCapabilityState.Disabled);
            var snapshot = AiResourceCapabilitySnapshot.Resolve(profile);

            Assert.Throws<InvalidOperationException>(() => AiMemoryGovernanceEvaluator.EnsureReadable(snapshot, AiMemoryFamily.Semantic, "semantic.fact"));

            profile.Set(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Semantic), AiResourceCapabilityState.Enabled);
            snapshot = AiResourceCapabilitySnapshot.Resolve(profile);
            Assert.Throws<InvalidOperationException>(() => AiMemoryGovernanceEvaluator.EnsureReadable(snapshot, AiMemoryFamily.Semantic, "semantic.secret"));
        }

        [Fact]
        public void CapabilityEvaluator_RuntimeOverrideEnablesDisabledProfileFamily()
        {
            var profile = new AiResourceCapabilityPolicy();
            profile.Set(AiMemoryResourceTypes.Memory, AiResourceCapabilityState.Enabled);
            profile.Set(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Semantic), AiResourceCapabilityState.Disabled);
            var runtime = new AiResourceCapabilityPolicy();
            runtime.Set(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Semantic), AiResourceCapabilityState.Enabled);

            var snapshot = AiResourceCapabilitySnapshot.Resolve(profile, runtime);
            AiMemoryGovernanceEvaluator.EnsureReadable(snapshot, AiMemoryFamily.Semantic, "semantic.fact");

            Assert.Equal(AiResourceCapabilitySource.RuntimeOverride,
                snapshot.GetSource(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Semantic)));
        }

        [Fact]
        public async Task GovernedStore_BlocksWritesToDisabledMemoryAndReturnsOnlyAuthorizedNonExpiredEntries()
        {
            var capabilitiesPolicy = new AiResourceCapabilityPolicy();
            capabilitiesPolicy.Set(AiMemoryResourceTypes.Memory, AiResourceCapabilityState.Enabled);
            capabilitiesPolicy.Set(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Semantic), AiResourceCapabilityState.Enabled);
            capabilitiesPolicy.Set(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Procedural), AiResourceCapabilityState.Disabled);
            capabilitiesPolicy.Set(AiMemoryResourceTypes.Type, "semantic.secret", AiResourceCapabilityState.Disabled);
            var snapshot = AiResourceCapabilitySnapshot.Resolve(capabilitiesPolicy);

            var memory = new InMemoryMemoryStore();
            await memory.AddAsync(CreateEntry(AiMemoryFamily.Semantic, "semantic.fact", DateTimeOffset.UtcNow));
            await memory.AddAsync(CreateEntry(AiMemoryFamily.Procedural, "procedural.strategy", DateTimeOffset.UtcNow));
            var expired = CreateEntry(AiMemoryFamily.Semantic, "semantic.fact", DateTimeOffset.UtcNow.AddDays(-10));
            expired.ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1);
            await memory.AddAsync(expired);

            var governed = new AiGovernedMemoryStore(memory, snapshot, new AiMemoryGovernancePolicy { MaxResults = 10 });
            var results = await governed.SearchAsync(new MemoryQuery { OwnerId = "owner-42", Text = "memory" });

            Assert.Single(results);
            Assert.Equal(AiMemoryFamily.Semantic, results[0].Family);
            Assert.Equal("semantic.fact", results[0].TypeId);

            await Assert.ThrowsAsync<InvalidOperationException>(() => governed.AddAsync(CreateEntry(AiMemoryFamily.Procedural, "procedural.strategy", DateTimeOffset.UtcNow)));
        }

        [Fact]
        public async Task GovernedStore_AppliesRetentionAndPerTypeResultLimit()
        {
            var capabilitiesPolicy = new AiResourceCapabilityPolicy();
            capabilitiesPolicy.Set(AiMemoryResourceTypes.Memory, AiResourceCapabilityState.Enabled);
            capabilitiesPolicy.Set(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Semantic), AiResourceCapabilityState.Enabled);
            capabilitiesPolicy.Set(AiMemoryResourceTypes.Type, "semantic.fact", AiResourceCapabilityState.Enabled);
            var snapshot = AiResourceCapabilitySnapshot.Resolve(capabilitiesPolicy);

            var policy = new AiMemoryGovernancePolicy { MaxResults = 10 };
            policy.Rules.Add(new AiMemoryPolicyRule { TypeId = "semantic.fact", MaxResults = 1, RetentionDays = 1 });
            var memory = new InMemoryMemoryStore();
            var governed = new AiGovernedMemoryStore(memory, snapshot, policy);

            await governed.AddAsync(CreateEntry(AiMemoryFamily.Semantic, "semantic.fact", DateTimeOffset.UtcNow));
            await governed.AddAsync(CreateEntry(AiMemoryFamily.Semantic, "semantic.fact", DateTimeOffset.UtcNow.AddMinutes(-1)));

            var results = await governed.SearchAsync(new MemoryQuery { Family = AiMemoryFamily.Semantic, TypeId = "semantic.fact", OwnerId = "owner-42", MaxResults = 100 });
            Assert.Single(results);
            Assert.True(results[0].ExpiresAt.HasValue);
            Assert.True(results[0].ExpiresAt.Value <= results[0].CreatedAt.AddDays(1));
        }

        [Fact]
        public void Policy_CloneIsIndependent()
        {
            var policy = new AiMemoryGovernancePolicy();
            policy.Rules.Add(new AiMemoryPolicyRule { Family = AiMemoryFamily.Working, MaxResults = 5, RetentionDays = 1 });
            var clone = policy.Clone();
            clone.Rules[0].MaxResults = 1;

            Assert.Equal(5, policy.Rules[0].MaxResults);
            Assert.Equal(1, clone.Rules[0].MaxResults);
        }

        private static MemoryEntry CreateEntry(AiMemoryFamily family, string typeId, DateTimeOffset createdAt)
        {
            return new MemoryEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                Scope = MemoryScope.Agent,
                Kind = MemoryKind.Fact,
                Family = family,
                TypeId = typeId,
                OwnerId = "owner-42",
                Content = "memory governance content",
                Metadata = new Dictionary<string, string>(),
                Provenance = new AiMemoryProvenance { Kind = AiMemoryProvenanceKind.HostProvided, Source = "test" },
                CreatedAt = createdAt,
                OccurredAt = createdAt
            };
        }
    }
}
