using System;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddMemoryFamiliesTab()
        {
            AddApiTab(
                "Memory Families",
                "Run memory family test",
                "Exercises the provider-neutral MemoryEntry family/type contract with Working, Episodic, Semantic, Procedural, and Custom families, plus provenance, confidence, expiration, validation, and cloning.",
                "All four built-in families and one application-specific custom type should validate, provenance should survive cloning, and invalid confidence/namespace state should be rejected.",
                "No AI request is sent by this example.",
                TestMemoryFamiliesAsync,
                "Memory contract boundary",
                "Memory families identify the semantic role of a record. TypeId remains extensible so future memory types do not require a new persisted class or provider-specific implementation.");
        }

        private System.Threading.Tasks.Task TestMemoryFamiliesAsync(string unused)
        {
            var now = DateTimeOffset.UtcNow;
            var identity = new AgentIdentityContext(
                deploymentId: "deployment-42",
                tenantId: "tenant-42",
                userId: "user-42");
            var ownerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, identity);

            var families = new[]
            {
                new { Family = AiMemoryFamily.Working, TypeId = "working.current-state" },
                new { Family = AiMemoryFamily.Episodic, TypeId = "episodic.completed-task" },
                new { Family = AiMemoryFamily.Semantic, TypeId = "semantic.fact" },
                new { Family = AiMemoryFamily.Procedural, TypeId = "procedural.review-step" }
            };

            foreach (var item in families)
            {
                var entry = CreateMemoryEntry(ownerId, item.Family, item.TypeId, now);
                entry.Validate();
            }

            var custom = CreateMemoryEntry(ownerId, AiMemoryFamily.Custom, "crm.customer-preference", now);
            custom.Provenance = new AiMemoryProvenance
            {
                Kind = AiMemoryProvenanceKind.UserProvided,
                Source = "crm",
                SourceId = "customer-42",
                SourceExecutionId = "execution-42",
                SourceRuntimeInstanceId = "runtime-42",
                Evidence = "User explicitly chose annual billing.",
                Confidence = 0.94m
            };
            custom.ExpiresAt = now.AddDays(30);
            custom.Metadata["department"] = "billing";
            custom.Validate();

            var clone = custom.Clone();
            clone.Metadata["department"] = "modified";
            clone.Provenance.Evidence = "changed";

            if (clone.Metadata["department"] != "modified" || custom.Metadata["department"] != "billing")
                throw new InvalidOperationException("Memory metadata clone isolation failed.");
            if (custom.Provenance.Evidence != "User explicitly chose annual billing.")
                throw new InvalidOperationException("Memory provenance clone isolation failed.");
            if (!custom.ExpiresAt.HasValue || custom.IsExpired(now))
                throw new InvalidOperationException("Memory expiration metadata was not preserved correctly.");

            var invalid = CreateMemoryEntry(ownerId, AiMemoryFamily.Semantic, "custom.invalid", now);
            try
            {
                invalid.Validate();
                throw new InvalidOperationException("Invalid semantic TypeId was accepted.");
            }
            catch (ArgumentException)
            {
                // Expected boundary rejection.
            }

            Write(
                "MEMORY FAMILIES",
                "Memory family contract succeeded." + Environment.NewLine +
                "Working family/type: verified." + Environment.NewLine +
                "Episodic family/type: verified." + Environment.NewLine +
                "Semantic family/type: verified." + Environment.NewLine +
                "Procedural family/type: verified." + Environment.NewLine +
                "Custom family/type extensibility: crm.customer-preference." + Environment.NewLine +
                "Provenance/source execution/runtime: preserved." + Environment.NewLine +
                "Confidence: 0.94." + Environment.NewLine +
                "Expiration metadata: 30 days." + Environment.NewLine +
                "Deep clone isolation: verified." + Environment.NewLine +
                "Invalid built-in TypeId namespace: rejected.");

            return System.Threading.Tasks.Task.CompletedTask;
        }

        private static MemoryEntry CreateMemoryEntry(string ownerId, AiMemoryFamily family, string typeId, DateTimeOffset now)
        {
            return new MemoryEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                Scope = MemoryScope.Agent,
                Kind = MemoryKind.Fact,
                Family = family,
                TypeId = typeId,
                OwnerId = ownerId,
                Content = "Provider-neutral memory contract example.",
                Provenance = new AiMemoryProvenance
                {
                    Kind = AiMemoryProvenanceKind.HostProvided,
                    Source = "HAgent.Example",
                    SourceId = "memory-family-42"
                },
                CreatedAt = now,
                OccurredAt = now
            };
        }
    }
}
