using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    public static class EpisodicMemoryExtensions
    {
        public static async Task<string> RememberEpisodeAsync(
            this HAgentClient client,
            EpisodicMemory episode,
            MemoryScope scope = MemoryScope.Agent,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (episode == null) throw new ArgumentNullException(nameof(episode));
            if (string.IsNullOrWhiteSpace(episode.OwnerId)) throw new ArgumentException("Episode owner ID is required.", nameof(episode));
            if (string.IsNullOrWhiteSpace(episode.Summary)) throw new ArgumentException("Episode summary is required.", nameof(episode));

            var content = string.IsNullOrWhiteSpace(episode.Title)
                ? episode.Summary.Trim()
                : episode.Title.Trim() + ": " + episode.Summary.Trim();

            var metadata = new Dictionary<string, string>(episode.Metadata ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase)
            {
                { "memoryType", "episodic" },
                { "outcome", episode.Outcome ?? string.Empty },
                { "sessionId", episode.SessionId ?? string.Empty }
            };

            return await client.RememberAsync(
                episode.OwnerId,
                content,
                scope,
                metadata,
                cancellationToken).ConfigureAwait(false);
        }

        public static async Task<IReadOnlyList<MemoryEntry>> RecallEpisodesAsync(
            this HAgentClient client,
            string ownerId,
            string text = null,
            string taskId = null,
            int maxResults = 10,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var recalled = await client.RecallAsync(
                ownerId,
                text ?? string.Empty,
                null,
                maxResults,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "memoryType", "episodic" }
                },
                cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(taskId)) return recalled;

            return recalled
                .Where(x => string.Equals(x.TaskId, taskId, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }
    }
}
