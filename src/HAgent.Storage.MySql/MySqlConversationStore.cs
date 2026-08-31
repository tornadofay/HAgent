using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using MySqlConnector;

namespace HAgent.Storage.MySql
{
    /// <summary>
    /// Persists HAgent conversation snapshots in the HAgent-owned MySQL database.
    /// </summary>
    public sealed class MySqlConversationStore : IConversationStore
    {
        private readonly string _connectionString;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions();

        public MySqlConversationStore(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required.", nameof(connectionString));
            _connectionString = connectionString;
        }

        public static async Task EnsureSchemaAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS HAgentConversations (
    SessionId varchar(128) NOT NULL,
    AgentId varchar(128) NOT NULL,
    CreatedAt datetime(6) NOT NULL,
    UpdatedAt datetime(6) NOT NULL,
    MessagesJson longtext NOT NULL,
    PRIMARY KEY (SessionId)
);";

            using (var connection = new MySqlConnection(connectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task SaveAsync(ConversationSnapshot conversation, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateConversation(conversation);
            var messagesJson = JsonSerializer.Serialize(conversation.Messages ?? new List<AIMessage>(), JsonOptions);
            var createdAt = conversation.CreatedAt == default(DateTimeOffset) ? DateTimeOffset.UtcNow : conversation.CreatedAt;
            var updatedAt = DateTimeOffset.UtcNow;

            const string sql = @"INSERT INTO HAgentConversations (SessionId, AgentId, CreatedAt, UpdatedAt, MessagesJson)
VALUES (@SessionId, @AgentId, @CreatedAt, @UpdatedAt, @MessagesJson)
ON DUPLICATE KEY UPDATE AgentId=VALUES(AgentId), UpdatedAt=VALUES(UpdatedAt), MessagesJson=VALUES(MessagesJson);";

            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SessionId", conversation.SessionId);
                command.Parameters.AddWithValue("@AgentId", conversation.AgentId ?? string.Empty);
                command.Parameters.AddWithValue("@CreatedAt", createdAt.UtcDateTime);
                command.Parameters.AddWithValue("@UpdatedAt", updatedAt.UtcDateTime);
                command.Parameters.AddWithValue("@MessagesJson", messagesJson);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<ConversationSnapshot> LoadAsync(string sessionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateSessionId(sessionId);
            const string sql = "SELECT SessionId, AgentId, CreatedAt, UpdatedAt, MessagesJson FROM HAgentConversations WHERE SessionId=@SessionId";

            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SessionId", sessionId);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) return null;
                    var messagesJson = reader.IsDBNull(4) ? "[]" : reader.GetString(4);
                    List<AIMessage> messages;
                    try
                    {
                        messages = JsonSerializer.Deserialize<List<AIMessage>>(messagesJson, JsonOptions) ?? new List<AIMessage>();
                    }
                    catch (JsonException ex)
                    {
                        throw new InvalidDataException("Stored conversation messages are invalid JSON.", ex);
                    }

                    return new ConversationSnapshot
                    {
                        SessionId = reader.GetString(0),
                        AgentId = reader.GetString(1),
                        CreatedAt = new DateTimeOffset(reader.GetDateTime(2), TimeSpan.Zero),
                        UpdatedAt = new DateTimeOffset(reader.GetDateTime(3), TimeSpan.Zero),
                        Messages = messages
                    };
                }
            }
        }

        public async Task DeleteAsync(string sessionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(sessionId)) return;
            const string sql = "DELETE FROM HAgentConversations WHERE SessionId=@SessionId";
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SessionId", sessionId);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static void ValidateConversation(ConversationSnapshot conversation)
        {
            if (conversation == null) throw new ArgumentNullException(nameof(conversation));
            ValidateSessionId(conversation.SessionId);
            if (conversation.AgentId == null) conversation.AgentId = string.Empty;
        }

        private static void ValidateSessionId(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("Session ID is required.", nameof(sessionId));
            if (sessionId.Length > 128) throw new ArgumentException("Session ID cannot exceed 128 characters.", nameof(sessionId));
        }
    }
}
