using System;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddMemoryGovernanceTab()
        {
            AddApiTab(
                "Memory Governance",
                "Run memory governance test",
                "Exercises profile/runtime memory capability state, family/type authorization, bounded retrieval, expiration filtering, and policy retention using the canonical IMemoryStore boundary.",
                "Disabled memory families/types must not be writable or exposed, retrieval must stay bounded, and retention policy must cap expiration without creating a provider-specific memory implementation.",
                "No AI request is sent by this example.",
                TestMemoryGovernanceAsync,
                "Memory governance boundary",
                "Memory governance composes the existing generic resource capability snapshot with a provider-neutral memory policy. Storage remains behind IMemoryStore.");
        }

        private async Task TestMemoryGovernanceAsync(string unused)
        {
            var profile = new AiAgent
            {
                Id = "memory-governance-agent-42",
                Name = "Memory Governance Agent"
            };
            profile.ResourceCapabilities.Set(AiMemoryResourceTypes.Memory, AiResourceCapabilityState.Enabled);
            profile.ResourceCapabilities.Set(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Semantic), AiResourceCapabilityState.Enabled);
            profile.ResourceCapabilities.Set(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Procedural), AiResourceCapabilityState.Disabled);
            profile.ResourceCapabilities.Set(AiMemoryResourceTypes.Type, "semantic.secret", AiResourceCapabilityState.Disabled);
            var snapshot = AiResourceCapabilitySnapshot.Resolve(profile.ResourceCapabilities);

            var policy = new AiMemoryGovernancePolicy { MaxResults = 5, ExcludeExpired = true };
            policy.Rules.Add(new AiMemoryPolicyRule
            {
                Family = AiMemoryFamily.Semantic,
                MaxResults = 2,
                RetentionDays = 7
            });

            var backingStore = new InMemoryMemoryStore();
            var governedStore = new AiGovernedMemoryStore(backingStore, snapshot, policy);

            await governedStore.AddAsync(CreateEntry(AiMemoryFamily.Semantic, "semantic.fact", "Current fact.", DateTimeOffset.UtcNow));
            var expiring = CreateEntry(AiMemoryFamily.Semantic, "semantic.fact", "Expiring fact.", DateTimeOffset.UtcNow.AddDays(-8));
            await governedStore.AddAsync(expiring);

            if (!expiring.ExpiresAt.HasValue || expiring.ExpiresAt.Value > expiring.CreatedAt.AddDays(7))
                throw new InvalidOperationException("Retention policy did not cap the stored expiration.");

            await AssertDeniedAsync(CreateEntry(AiMemoryFamily.Procedural, "procedural.strategy", "Blocked procedure.", DateTimeOffset.UtcNow));
            await AssertDeniedAsync(CreateEntry(AiMemoryFamily.Semantic, "semantic.secret", "Blocked secret.", DateTimeOffset.UtcNow));

            var results = await governedStore.SearchAsync(new MemoryQuery
            {
                OwnerId = "owner-42",
                Family = AiMemoryFamily.Semantic,
                TypeId = "semantic.fact",
                Text = "fact",
                MaxResults = 100
            });
            if (results.Count != 1 || results[0].Content != "Current fact.")
                throw new InvalidOperationException("Governed retrieval did not apply capability, expiration, and result-limit rules correctly.");

            var broad = await governedStore.SearchAsync(new MemoryQuery { OwnerId = "owner-42", Text = "fact" });
            if (broad.Count != 1 || broad[0].Family != AiMemoryFamily.Semantic)
                throw new InvalidOperationException("Broad governed retrieval exposed an unauthorized or expired memory entry.");

            if (policy.GetMaxResults(new MemoryQuery { Family = AiMemoryFamily.Semantic, TypeId = "semantic.fact", MaxResults = 100 }) != 2)
                throw new InvalidOperationException("Per-family retrieval bound was not resolved correctly.");

            Write(
                "MEMORY GOVERNANCE",
                "Contract test succeeded." + Environment.NewLine +
                "Memory capability enabled: verified." + Environment.NewLine +
                "Semantic family enabled: verified." + Environment.NewLine +
                "Procedural family disabled: enforced." + Environment.NewLine +
                "semantic.secret type disabled: enforced." + Environment.NewLine +
                "Bounded retrieval: max 2 for Semantic family." + Environment.NewLine +
                "Expiration filtering: verified." + Environment.NewLine +
                "Retention cap: 7 days." + Environment.NewLine +
                "Broad retrieval filters unauthorized/expired entries: verified." + Environment.NewLine +
                "Provider-specific storage dependency: none.");
        }

        private static async Task AssertDeniedAsync(MemoryEntry entry)
        {
            var policy = new AiResourceCapabilityPolicy();
            policy.Set(AiMemoryResourceTypes.Memory, AiResourceCapabilityState.Enabled);
            policy.Set(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Semantic), AiResourceCapabilityState.Enabled);
            policy.Set(AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(AiMemoryFamily.Procedural), AiResourceCapabilityState.Disabled);
            policy.Set(AiMemoryResourceTypes.Type, "semantic.secret", AiResourceCapabilityState.Disabled);
            var snapshot = AiResourceCapabilitySnapshot.Resolve(policy);
            var store = new AiGovernedMemoryStore(new InMemoryMemoryStore(), snapshot);
            await AssertThrowsInvalidOperationAsync(() => store.AddAsync(entry));
        }

        private static async Task AssertThrowsInvalidOperationAsync(Func<Task> operation)
        {
            try
            {
                await operation().ConfigureAwait(true);
            }
            catch (InvalidOperationException)
            {
                return;
            }
            throw new InvalidOperationException("Memory governance allowed a disabled resource.");
        }

        private static MemoryEntry CreateEntry(AiMemoryFamily family, string typeId, string content, DateTimeOffset createdAt)
        {
            return new MemoryEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                Scope = MemoryScope.Agent,
                Kind = MemoryKind.Fact,
                Family = family,
                TypeId = typeId,
                OwnerId = "owner-42",
                Content = content,
                Provenance = new AiMemoryProvenance { Kind = AiMemoryProvenanceKind.HostProvided, Source = "HAgent.Example" },
                CreatedAt = createdAt,
                OccurredAt = createdAt
            };
        }
    }
}
