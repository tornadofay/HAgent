using System;
using System.Text.Json;
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
            foreach (var family in new[] { AiMemoryFamily.Working, AiMemoryFamily.Episodic, AiMemoryFamily.Semantic, AiMemoryFamily.Procedural })
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
        public void MemoryEntry_JsonRoundTripPreservesFamilyTypeProvenanceAndExpiration()
        {
            var entry = CreateEntry();
            entry.Family = AiMemoryFamily.Procedural;
            entry.TypeId = "procedural.review-step";
            entry.Provenance = new AiMemoryProvenance
            {
                Kind = AiMemoryProvenanceKind.ModelGenerated,
                Source = "model",
                SourceExecutionId = "execution-42",
                SourceRuntimeInstanceId = "runtime-42",
                Evidence = "Repeated successful execution.",
                Confidence = 0.91m
            };
            entry.ExpiresAt = entry.CreatedAt.AddHours(4);

            var json = JsonSerializer.Serialize(entry);
            var roundTrip = JsonSerializer.Deserialize<MemoryEntry>(json);
            roundTrip.Validate();

            Assert.Equal(AiMemoryFamily.Procedural, roundTrip.Family);
            Assert.Equal("procedural.review-step", roundTrip.TypeId);
            Assert.Equal(AiMemoryProvenanceKind.ModelGenerated, roundTrip.Provenance.Kind);
            Assert.Equal("execution-42", roundTrip.Provenance.SourceExecutionId);
            Assert.Equal(0.91m, roundTrip.Provenance.Confidence);
            Assert.Equal(entry.ExpiresAt, roundTrip.ExpiresAt);
        }

        [Fact]
        public void MemoryEntry_RejectsUnboundedProvenanceAndMetadata()
        {
            var entry = CreateEntry();
            entry.Provenance.Evidence = new string('x', 4097);
            Assert.Throws<ArgumentException>(() => entry.Validate());

            entry = CreateEntry();
            entry.Provenance.Evidence = null;
            for (var i = 0; i < 33; i++) entry.Metadata["key-" + i] = "value";
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
