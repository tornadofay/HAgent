using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using Microsoft.Data.SqlClient;

namespace HAgent.Storage.SqlServer
{
    public sealed class SqlServerMemoryStore : IMemoryStore
    {
        private readonly string _connectionString;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = false };

        public SqlServerMemoryStore(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public static async Task EnsureSchemaAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.HAgentMemoryEntries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HAgentMemoryEntries (
        Id nvarchar(128) NOT NULL CONSTRAINT PK_HAgentMemoryEntries PRIMARY KEY,
        Scope nvarchar(50) NOT NULL,
        Kind nvarchar(50) NOT NULL,
        OwnerId nvarchar(128) NOT NULL,
        TaskId nvarchar(128) NULL,
        Content nvarchar(max) NOT NULL,
        MetadataJson nvarchar(max) NULL,
        CreatedAt datetimeoffset NOT NULL,
        OccurredAt datetimeoffset NOT NULL
    );
    CREATE INDEX IX_HAgentMemoryEntries_OwnerScope ON dbo.HAgentMemoryEntries(OwnerId, Scope);
    CREATE INDEX IX_HAgentMemoryEntries_TaskId ON dbo.HAgentMemoryEntries(TaskId);
END;";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task AddAsync(MemoryEntry entry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(entry.Id)) entry.Id = Guid.NewGuid().ToString("N");
            if (entry.Metadata == null) entry.Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (entry.CreatedAt == default(DateTimeOffset)) entry.CreatedAt = DateTimeOffset.UtcNow;
            if (entry.OccurredAt == default(DateTimeOffset)) entry.OccurredAt = entry.CreatedAt;

            const string sql = @"INSERT INTO dbo.HAgentMemoryEntries
(Id, Scope, Kind, OwnerId, TaskId, Content, MetadataJson, CreatedAt, OccurredAt)
VALUES (@Id, @Scope, @Kind, @OwnerId, @TaskId, @Content, @MetadataJson, @CreatedAt, @OccurredAt);";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                BindEntry(command, entry);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyList<MemoryEntry>> SearchAsync(MemoryQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            query = query ?? new MemoryQuery();
            var maxResults = query.MaxResults <= 0 ? 10 : Math.Min(query.MaxResults, 1000);
            const string sql = @"SELECT TOP (@MaxResults)
    Id, Scope, Kind, OwnerId, TaskId, Content, MetadataJson, CreatedAt, OccurredAt
FROM dbo.HAgentMemoryEntries
WHERE (@Scope IS NULL OR Scope = @Scope)
  AND (@Kind IS NULL OR Kind = @Kind)
  AND (@OwnerId = N'' OR OwnerId = @OwnerId)
  AND (@TaskId = N'' OR TaskId = @TaskId)
ORDER BY OccurredAt DESC, CreatedAt DESC;";

            var candidates = new List<MemoryEntry>();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@MaxResults", maxResults);
                command.Parameters.AddWithValue("@Scope", query.Scope.HasValue ? query.Scope.Value.ToString() : (object)DBNull.Value);
                command.Parameters.AddWithValue("@Kind", query.Kind.HasValue ? query.Kind.Value.ToString() : (object)DBNull.Value);
                command.Parameters.AddWithValue("@OwnerId", query.OwnerId ?? string.Empty);
                command.Parameters.AddWithValue("@TaskId", query.TaskId ?? string.Empty);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var entry = ReadEntry(reader);
                        if (MatchesMetadata(entry, query)) candidates.Add(entry);
                    }
                }
            }

            var terms = SplitTerms(query.Text);
            var scored = new List<ScoredEntry>();
            foreach (var entry in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var score = Score(entry, terms, query.Kind.HasValue || !string.IsNullOrWhiteSpace(query.TaskId));
                if (terms.Count > 0 && score <= 0) continue;
                scored.Add(new ScoredEntry(entry, score));
            }

            scored.Sort(Compare);
            if (scored.Count > maxResults) scored.RemoveRange(maxResults, scored.Count - maxResults);

            var result = new List<MemoryEntry>(scored.Count);
            foreach (var item in scored) result.Add(item.Entry);
            return result.AsReadOnly();
        }

        public async Task RemoveAsync(string memoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(memoryId)) return;
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("DELETE FROM dbo.HAgentMemoryEntries WHERE Id=@Id", connection))
            {
                command.Parameters.AddWithValue("@Id", memoryId);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task ClearAsync(string scope, string ownerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            MemoryScope parsed;
            var hasScope = Enum.TryParse(scope, true, out parsed);
            var sql = hasScope
                ? "DELETE FROM dbo.HAgentMemoryEntries WHERE Scope=@Scope AND (@OwnerId=N'' OR OwnerId=@OwnerId)"
                : "DELETE FROM dbo.HAgentMemoryEntries WHERE (@OwnerId=N'' OR OwnerId=@OwnerId)";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (hasScope) command.Parameters.AddWithValue("@Scope", parsed.ToString());
                command.Parameters.AddWithValue("@OwnerId", ownerId ?? string.Empty);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static void BindEntry(SqlCommand command, MemoryEntry entry)
        {
            command.Parameters.AddWithValue("@Id", entry.Id);
            command.Parameters.AddWithValue("@Scope", entry.Scope.ToString());
            command.Parameters.AddWithValue("@Kind", entry.Kind.ToString());
            command.Parameters.AddWithValue("@OwnerId", entry.OwnerId ?? string.Empty);
            command.Parameters.AddWithValue("@TaskId", string.IsNullOrWhiteSpace(entry.TaskId) ? (object)DBNull.Value : entry.TaskId);
            command.Parameters.AddWithValue("@Content", entry.Content ?? string.Empty);
            command.Parameters.AddWithValue("@MetadataJson", JsonSerializer.Serialize(entry.Metadata ?? new Dictionary<string, string>(), JsonOptions));
            command.Parameters.AddWithValue("@CreatedAt", entry.CreatedAt);
            command.Parameters.AddWithValue("@OccurredAt", entry.OccurredAt);
        }

        private static MemoryEntry ReadEntry(SqlDataReader reader)
        {
            var metadataJson = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
            IDictionary<string, string> metadata;
            try
            {
                metadata = string.IsNullOrWhiteSpace(metadataJson)
                    ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson, JsonOptions) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            MemoryScope scope;
            MemoryKind kind;
            Enum.TryParse(reader.GetString(1), true, out scope);
            Enum.TryParse(reader.GetString(2), true, out kind);

            return new MemoryEntry
            {
                Id = reader.GetString(0),
                Scope = scope,
                Kind = kind,
                OwnerId = reader.GetString(3),
                TaskId = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Content = reader.GetString(5),
                Metadata = metadata,
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(7),
                OccurredAt = reader.GetFieldValue<DateTimeOffset>(8)
            };
        }

        private static bool MatchesMetadata(MemoryEntry entry, MemoryQuery query)
        {
            if (query.Metadata == null || query.Metadata.Count == 0) return true;
            if (entry.Metadata == null) return false;
            foreach (var pair in query.Metadata)
            {
                string value;
                if (!entry.Metadata.TryGetValue(pair.Key, out value) || !string.Equals(value, pair.Value, StringComparison.OrdinalIgnoreCase)) return false;
            }
            return true;
        }

        private static int Score(MemoryEntry entry, IReadOnlyList<string> terms, bool typedQuery)
        {
            if (terms.Count == 0) return typedQuery ? 1 : 1;
            var content = entry.Content ?? string.Empty;
            var metadata = entry.Metadata == null ? string.Empty : string.Join(" ", entry.Metadata.Values);
            var haystack = (content + " " + metadata).ToLowerInvariant();
            var score = 0;
            foreach (var term in terms)
            {
                var index = 0;
                while ((index = haystack.IndexOf(term, index, StringComparison.Ordinal)) >= 0)
                {
                    score++;
                    index += Math.Max(1, term.Length);
                }
                if (content.Equals(term, StringComparison.OrdinalIgnoreCase)) score += 5;
                else if (content.IndexOf(" " + term + " ", StringComparison.OrdinalIgnoreCase) >= 0) score += 2;
            }
            return score;
        }

        private static List<string> SplitTerms(string text)
        {
            var terms = new List<string>();
            foreach (var part in (text ?? string.Empty).Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (part.Length < 2) continue;
                var normalized = part.ToLowerInvariant();
                if (!terms.Contains(normalized)) terms.Add(normalized);
            }
            return terms;
        }

        private static int Compare(ScoredEntry x, ScoredEntry y)
        {
            var score = y.Score.CompareTo(x.Score);
            return score != 0 ? score : y.Entry.OccurredAt.CompareTo(x.Entry.OccurredAt);
        }

        private sealed class ScoredEntry
        {
            public ScoredEntry(MemoryEntry entry, int score) { Entry = entry; Score = score; }
            public MemoryEntry Entry { get; private set; }
            public int Score { get; private set; }
        }
    }
}
