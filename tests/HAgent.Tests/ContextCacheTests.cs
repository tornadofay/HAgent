using System;
using System.Collections.Generic;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class ContextCacheTests
    {
        [Fact]
        public void Cache_ReusesEntryForSameOwnershipAndVersions()
        {
            var cache = new InMemoryContextCache();
            var key = CreateKey("User", "user-1", "cfg-1", "resource-1", "fresh-1");
            var snapshot = CreateSnapshot("cached-1");
            var expiresAt = new DateTimeOffset(2026, 9, 9, 0, 10, 0, TimeSpan.Zero);

            cache.Set(key, snapshot, expiresAt);

            ContextSnapshot actual;
            Assert.True(cache.TryGet(key, new DateTimeOffset(2026, 9, 9, 0, 5, 0, TimeSpan.Zero), out actual));
            Assert.NotNull(actual);
            Assert.Equal("cached-1", actual.Items[0].Id);
        }

        [Fact]
        public void Cache_DoesNotCrossScopeOrVersionBoundaries()
        {
            var cache = new InMemoryContextCache();
            var baseKey = CreateKey("User", "user-1", "cfg-1", "resource-1", "fresh-1");
            cache.Set(baseKey, CreateSnapshot("user-1"), new DateTimeOffset(2026, 9, 9, 1, 0, 0, TimeSpan.Zero));

            ContextSnapshot actual;
            Assert.False(cache.TryGet(CreateKey("User", "user-2", "cfg-1", "resource-1", "fresh-1"),
                new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out actual));
            Assert.False(cache.TryGet(CreateKey("User", "user-1", "cfg-2", "resource-1", "fresh-1"),
                new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out actual));
            Assert.False(cache.TryGet(CreateKey("User", "user-1", "cfg-1", "resource-2", "fresh-1"),
                new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out actual));
            Assert.False(cache.TryGet(CreateKey("User", "user-1", "cfg-1", "resource-1", "fresh-2"),
                new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out actual));
        }

        [Fact]
        public void Cache_ExpiresEntryAndInvalidatesIt()
        {
            var cache = new InMemoryContextCache();
            var key = CreateKey("Workspace", "workspace-1", "cfg-1", "resource-1", "fresh-1");
            cache.Set(key, CreateSnapshot("cached-1"), new DateTimeOffset(2026, 9, 9, 0, 10, 0, TimeSpan.Zero));

            ContextSnapshot actual;
            Assert.False(cache.TryGet(key, new DateTimeOffset(2026, 9, 9, 0, 10, 0, TimeSpan.Zero), out actual));
            Assert.False(cache.TryGet(key, new DateTimeOffset(2026, 9, 9, 0, 10, 1, TimeSpan.Zero), out actual));
        }

        [Fact]
        public void Cache_InvalidationAndClearAreExplicit()
        {
            var cache = new InMemoryContextCache();
            var key1 = CreateKey("User", "user-1", "cfg", "resource", "fresh");
            var key2 = CreateKey("User", "user-2", "cfg", "resource", "fresh");
            var expires = new DateTimeOffset(2026, 9, 9, 1, 0, 0, TimeSpan.Zero);
            cache.Set(key1, CreateSnapshot("one"), expires);
            cache.Set(key2, CreateSnapshot("two"), expires);

            cache.Invalidate(key1);
            ContextSnapshot actual;
            Assert.False(cache.TryGet(key1, new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out actual));
            Assert.True(cache.TryGet(key2, new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out actual));

            cache.Clear();
            Assert.False(cache.TryGet(key2, new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out actual));
        }

        [Fact]
        public void Cache_ReturnsSnapshotStateThatCallerCannotMutateThroughItemMetadata()
        {
            var cache = new InMemoryContextCache();
            var key = CreateKey("Runtime", "runtime-1", "cfg", "resource", "fresh");
            var snapshot = CreateSnapshot("cached-1");
            cache.Set(key, snapshot, new DateTimeOffset(2026, 9, 9, 1, 0, 0, TimeSpan.Zero));

            ContextSnapshot actual;
            Assert.True(cache.TryGet(key, new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out actual));
            var item = actual.Items[0];
            item.Id = "mutated";
            item.Scope.ScopeId = "other-runtime";

            Assert.True(cache.TryGet(key, new DateTimeOffset(2026, 9, 9, 0, 31, 0, TimeSpan.Zero), out actual));
            Assert.Equal("cached-1", actual.Items[0].Id);
            Assert.Equal("runtime-1", actual.Items[0].Scope.ScopeId);
        }

        private static ContextCacheKey CreateKey(string scopeType, string scopeId, string configurationVersion, string resourceVersion, string freshnessVersion)
        {
            return new ContextCacheKey
            {
                ComponentId = "customer-context",
                ScopeType = scopeType,
                ScopeId = scopeId,
                ConfigurationVersion = configurationVersion,
                ResourceVersion = resourceVersion,
                FreshnessVersion = freshnessVersion
            };
        }

        private static ContextSnapshot CreateSnapshot(string id)
        {
            var captured = new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero);
            var item = new ContextItem
            {
                Id = id,
                Source = "CacheTest",
                Type = "Observation",
                Payload = new Dictionary<string, object> { { "id", id } },
                Provenance = new ContextProvenance
                {
                    SourceKind = "CacheTest",
                    SourceId = id,
                    SourceVersion = "1",
                    Evidence = "Deterministic cache test",
                    CapturedAt = captured
                },
                Scope = new ContextScope { ScopeType = "Runtime", ScopeId = "runtime-1" },
                Trust = 1d,
                Importance = 1d,
                Relevance = 1d,
                CapturedAt = captured,
                EstimatedCharacters = 10,
                EstimatedTokens = 2
            };

            return new ContextSnapshot(
                new[] { item },
                new ContextBudget { MaxItems = 10, MaxCharacters = 100, MaxEstimatedTokens = 10 },
                1,
                10,
                2,
                1,
                captured);
        }
    }
}
