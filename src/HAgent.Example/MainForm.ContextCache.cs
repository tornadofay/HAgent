using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddContextCacheTab()
        {
            AddApiTab(
                "Context Cache",
                "Run cache test",
                "Exercises reusable context caching with explicit component, scope, configuration, resource, and freshness identity plus expiration.",
                "The same key should reuse a cached snapshot; changed scope/version/freshness identity or elapsed expiration must miss; cached snapshot metadata must remain isolated from caller mutation.",
                "No AI request is sent. The cache stores reusable context snapshots separately from execution assembly.",
                RunContextCacheTestAsync,
                "Cache-safe reusable component boundary",
                "This slice defines reusable cache identity and in-memory reuse. Execution/provider integration remains the final 0.955 slice.");
        }

        private Task RunContextCacheTestAsync(string unused)
        {
            var cache = new InMemoryContextCache();
            var key = CreateExampleCacheKey("User", "user-42", "cfg-7", "resource-12", "fresh-3");
            var expiry = new DateTimeOffset(2026, 9, 9, 1, 0, 0, TimeSpan.Zero);
            var snapshot = CreateExampleCachedSnapshot();

            cache.Set(key, snapshot, expiry);

            ContextSnapshot reused;
            var hit = cache.TryGet(key, new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out reused);
            if (!hit || reused == null || reused.Items.Count != 1 || reused.Items[0].Id != "cached-customer")
                throw new InvalidOperationException("Valid reusable context cache entry was not reused.");

            var scopeChanged = CreateExampleCacheKey("User", "user-99", "cfg-7", "resource-12", "fresh-3");
            var configurationChanged = CreateExampleCacheKey("User", "user-42", "cfg-8", "resource-12", "fresh-3");
            var resourceChanged = CreateExampleCacheKey("User", "user-42", "cfg-7", "resource-13", "fresh-3");
            var freshnessChanged = CreateExampleCacheKey("User", "user-42", "cfg-7", "resource-12", "fresh-4");

            ContextSnapshot ignored;
            if (cache.TryGet(scopeChanged, new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out ignored) ||
                cache.TryGet(configurationChanged, new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out ignored) ||
                cache.TryGet(resourceChanged, new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out ignored) ||
                cache.TryGet(freshnessChanged, new DateTimeOffset(2026, 9, 9, 0, 30, 0, TimeSpan.Zero), out ignored))
                throw new InvalidOperationException("Context cache crossed an ownership or version boundary.");

            var item = reused.Items[0];
            item.Id = "mutated";
            item.Scope.ScopeId = "other-user";
            if (!cache.TryGet(key, new DateTimeOffset(2026, 9, 9, 0, 31, 0, TimeSpan.Zero), out reused) ||
                reused.Items[0].Id != "cached-customer" ||
                reused.Items[0].Scope.ScopeId != "user-42")
                throw new InvalidOperationException("Cached context snapshot was mutable through a returned item copy.");

            if (cache.TryGet(key, new DateTimeOffset(2026, 9, 9, 1, 0, 0, TimeSpan.Zero), out ignored))
                throw new InvalidOperationException("Expired context cache entry was still reusable.");

            Write(
                "CONTEXT CACHE",
                "Cache-safe reusable context component verification succeeded." + Environment.NewLine +
                "Valid cache reuse: verified." + Environment.NewLine +
                "Scope/configuration/resource/freshness isolation: verified." + Environment.NewLine +
                "Snapshot mutation isolation: verified." + Environment.NewLine +
                "Expiration invalidation: verified." + Environment.NewLine +
                "Provider request: none.");

            return Task.CompletedTask;
        }

        private static ContextCacheKey CreateExampleCacheKey(string scopeType, string scopeId, string configurationVersion, string resourceVersion, string freshnessVersion)
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

        private static ContextSnapshot CreateExampleCachedSnapshot()
        {
            var captured = new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero);
            var item = new ContextItem
            {
                Id = "cached-customer",
                Source = "ContextCacheExample",
                Type = "Customer",
                Payload = new Dictionary<string, object> { { "Id", 42 }, { "Name", "Cached Customer" } },
                Provenance = new ContextProvenance
                {
                    SourceKind = "Example",
                    SourceId = "customer-42",
                    SourceVersion = "1",
                    Evidence = "Deterministic reusable context cache example",
                    CapturedAt = captured
                },
                Scope = new ContextScope { ScopeType = "User", ScopeId = "user-42" },
                Trust = 1d,
                Importance = 1d,
                Relevance = 1d,
                CapturedAt = captured,
                EstimatedCharacters = 30,
                EstimatedTokens = 8
            };

            return new ContextSnapshot(
                new[] { item },
                new ContextBudget { MaxItems = 10, MaxCharacters = 1000, MaxEstimatedTokens = 100 },
                1,
                30,
                8,
                1,
                captured);
        }
    }
}
