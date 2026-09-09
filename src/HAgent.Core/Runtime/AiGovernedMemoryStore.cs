using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class AiGovernedMemoryStore : IMemoryStore
    {
        private readonly IMemoryStore _inner;
        private readonly AiResourceCapabilitySnapshot _capabilities;
        private readonly AiMemoryGovernancePolicy _policy;

        public AiGovernedMemoryStore(
            IMemoryStore inner,
            AiResourceCapabilitySnapshot capabilities,
            AiMemoryGovernancePolicy policy = null)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
            _policy = policy == null ? new AiMemoryGovernancePolicy() : policy.Clone();
            _policy.Validate();
        }

        public async Task AddAsync(MemoryEntry entry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            cancellationToken.ThrowIfCancellationRequested();
            entry.Validate();
            AiMemoryGovernanceEvaluator.EnsureWritable(_capabilities, entry);

            var governed = entry.Clone();
            governed.ExpiresAt = _policy.GetEffectiveExpiration(governed);
            governed.Validate();
            await _inner.AddAsync(governed, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<MemoryEntry>> SearchAsync(MemoryQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            query = query == null ? new MemoryQuery() : query;
            cancellationToken.ThrowIfCancellationRequested();

            var maxResults = _policy.GetMaxResults(query);
            var innerQuery = new MemoryQuery
            {
                Scope = query.Scope,
                Kind = query.Kind,
                Family = query.Family,
                TypeId = query.TypeId,
                OwnerId = query.OwnerId,
                TaskId = query.TaskId,
                Text = query.Text,
                MaxResults = 1000,
                IncludeExpired = true,
                Metadata = query.Metadata
            };

            if (query.Family.HasValue && !string.IsNullOrWhiteSpace(query.TypeId))
                AiMemoryGovernanceEvaluator.EnsureReadable(_capabilities, query.Family.Value, query.TypeId);
            else if (query.Family.HasValue)
                AiMemoryGovernanceEvaluator.EnsureReadable(_capabilities, query.Family.Value, GetDefaultTypeId(query.Family.Value));

            var entries = await _inner.SearchAsync(innerQuery, cancellationToken).ConfigureAwait(false);
            var result = new List<MemoryEntry>();
            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (entry == null) continue;
                if (!query.IncludeExpired && _policy.ExcludeExpired && GetEffectiveExpiration(entry) <= DateTimeOffset.UtcNow) continue;

                try { AiMemoryGovernanceEvaluator.EnsureReadable(_capabilities, entry.Family, entry.TypeId); }
                catch (InvalidOperationException) { continue; }

                result.Add(entry.Clone());
                if (result.Count >= maxResults) break;
            }

            return result.AsReadOnly();
        }

        public Task RemoveAsync(string memoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _inner.RemoveAsync(memoryId, cancellationToken);
        }

        public Task ClearAsync(string scope, string ownerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _inner.ClearAsync(scope, ownerId, cancellationToken);
        }

        private DateTimeOffset GetEffectiveExpiration(MemoryEntry entry)
        {
            var expiration = _policy.GetEffectiveExpiration(entry);
            return expiration ?? DateTimeOffset.MaxValue;
        }

        private static string GetDefaultTypeId(AiMemoryFamily family)
        {
            switch (family)
            {
                case AiMemoryFamily.Working: return "working.state";
                case AiMemoryFamily.Episodic: return "episodic.experience";
                case AiMemoryFamily.Semantic: return "semantic.fact";
                case AiMemoryFamily.Procedural: return "procedural.strategy";
                case AiMemoryFamily.Custom: return "custom.type";
                default: throw new ArgumentOutOfRangeException(nameof(family));
            }
        }
    }
}
