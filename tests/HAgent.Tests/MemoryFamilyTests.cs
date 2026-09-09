using System;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class MemoryFamilyTests
    {
        [Fact]
        public void MemoryEntry_DefaultsToSemanticFactFamilyAndType()
        {
            var entry = CreateEntry();

            entry.Validate();

            Assert.Equal(AiMemoryFamily.Semantic, entry.Family);
            Assert.Equal("semantic.fact", entry.TypeId);
            Assert.False(entry.IsExpired());
        }

        [Fact]
        public void MemoryEntry_AllBuiltInFamiliesRequireTheirOwnTypeNamespace()
        {
            foreach (var family in new[]
            {
                AiMemoryFamily.Working,
                AiMemoryFamily.Episodic,
                AiMemoryFamily.Semantic,
                AiMemoryFamily.Procedural
            })
            {
                var entry = CreateEntry();
                entry.Family = family;
                entry.TypeId = family.ToString().ToLowerInvariant() + ".example";
                entry.Validate();
            }
        }

        [Fact]
        public void MemoryEntry_CustomFamilySupportsApplicationSpecificType()
        {
            var entry = CreateEntry();
            entry.Family = AiMemoryFamily.Custom;
            entry.TypeId = "crm.customer-preference";
            entry.Provenance.Kind = AiMemoryProvenanceKind.HostProvided;
            entry.Provenance.Source = "crm";
            entry.Provenance.SourceId = "customer-42";
            entry.Validate();
        }

        [Fact]
        public void MemoryEntry_RejectsCustomTypesThatCollideWithBuiltInNamespaces()
        {
            var entry = CreateEntry();
            entry.Family = AiMemoryFamily.Custom;
            entry.TypeId = "semantic.customer";

            Assert.Throws<ArgumentException>(() => entry.Validate());
        }

        [Fact]
        public void MemoryEntry_DeepCloneOwnsMetadataAndProvenance()
        {
            var entry = CreateEntry();
            entry.Metadata["source"] = "session";
            entry.Provenance.Source = "host";
            entry.Provenance.Evidence = "explicit user statement";
            entry.Provenance.Confidence = 0.88m;
            entry.ExpiresAt = DateTimeOffset.UtcNow.AddHours(2);

            var clone = entry.Clone();
            clone.Metadata["source"] = "modified";
            clone.Provenance.Source = "changed";

            Assert.Equal("session", entry.Metadata["source"]);
            Assert.Equal("host", entry.Provenance.Source);
            Assert.Equal(0.88m, clone.Provenance.Confidence);
            Assert.True(clone.ExpiresAt.HasValue);
        }

        [Fact]
        public void MemoryEntry_RejectsUnboundedProvenanceAndMetadata()
        {
            var entry = CreateEntry();
            entry.Provenance.Evidence = new string('x', 4097);
            Assert.Throws<ArgumentException>(() => entry.Validate());

            entry = CreateEntry();
            entry.Provenance.Evidence = null;
            for (var i = 0; i < 33; i++)
                entry.Metadata["key-" + i] = "value";
            Assert.Throws<ArgumentException>(() => entry.Validate());
        }

        [Fact]
        public void MemoryEntry_ConfidenceAndExpirationAreBounded()
        {
            var entry = CreateEntry();
            entry.Provenance.Confidence = 1.1m;
            Assert.Throws<ArgumentException>(() => entry.Validate());

            entry = CreateEntry();
            entry.ExpiresAt = entry.CreatedAt;
            Assert.Throws<ArgumentException>(() => entry.Validate());
        }

        [Fact]
        public void MemoryEntry_AllowsHistoricalOccurrenceBeforeCreation()
        {
            var entry = CreateEntry();
            entry.OccurredAt = entry.CreatedAt.AddDays(-3);

            entry.Validate();

            Assert.True(entry.OccurredAt < entry.CreatedAt);
        }

        private static MemoryEntry CreateEntry()
        {
            return new MemoryEntry
            {
                Id = "memory-42",
                OwnerId = "owner-42",
                Content = "Reusable memory content"
            };
        }
    }
}
