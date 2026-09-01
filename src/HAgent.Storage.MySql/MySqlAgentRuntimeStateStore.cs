using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using MySqlConnector;

namespace HAgent.Storage.MySql
{
    public sealed class MySqlAgentRuntimeStateStore : IAgentRuntimeStateStore
    {
        private readonly string _connectionString;

        public MySqlAgentRuntimeStateStore(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public static async Task EnsureSchemaAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            var statements = new[]
            {
                @"CREATE TABLE IF NOT EXISTS HAgentRuntimeInstances (
                    InstanceId varchar(128) NOT NULL,
                    ProfileId varchar(128) NOT NULL,
                    HostInstanceId varchar(128) NULL,
                    UserId varchar(128) NULL,
                    WorkspaceId varchar(128) NULL,
                    SessionId varchar(128) NULL,
                    Scope varchar(50) NOT NULL,
                    State varchar(50) NOT NULL,
                    CreatedAt datetime(6) NOT NULL,
                    UpdatedAt datetime(6) NOT NULL,
                    PRIMARY KEY (InstanceId)
                ) ENGINE=InnoDB;",
                @"CREATE INDEX IX_HAgentRuntimeInstances_ProfileUpdated ON HAgentRuntimeInstances (ProfileId, UpdatedAt);",
                @"CREATE INDEX IX_HAgentRuntimeInstances_HostUser ON HAgentRuntimeInstances (HostInstanceId, UserId, UpdatedAt);",
                @"CREATE INDEX IX_HAgentRuntimeInstances_Workspace ON HAgentRuntimeInstances (WorkspaceId, UpdatedAt);"
            };

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                foreach (var statement in statements)
                {
                    try
                    {
                        using (var command = new MySqlCommand(statement, connection))
                        {
                            command.CommandTimeout = 60;
                            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (MySqlException ex) when (statement.IndexOf("CREATE INDEX", StringComparison.OrdinalIgnoreCase) >= 0 && ex.Number == 1061)
                    {
                    }
                }
            }
        }

        public async Task SaveAsync(AgentRuntimeStateRecord record, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateRecord(record);
            const string sql = @"INSERT INTO HAgentRuntimeInstances
(InstanceId, ProfileId, HostInstanceId, UserId, WorkspaceId, SessionId, Scope, State, CreatedAt, UpdatedAt)
VALUES (@InstanceId, @ProfileId, @HostInstanceId, @UserId, @WorkspaceId, @SessionId, @Scope, @State, @CreatedAt, @UpdatedAt)
ON DUPLICATE KEY UPDATE
ProfileId=VALUES(ProfileId), HostInstanceId=VALUES(HostInstanceId), UserId=VALUES(UserId),
WorkspaceId=VALUES(WorkspaceId), SessionId=VALUES(SessionId), Scope=VALUES(Scope), State=VALUES(State),
CreatedAt=VALUES(CreatedAt), UpdatedAt=VALUES(UpdatedAt);";
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                Bind(command, record);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<AgentRuntimeStateRecord> GetAsync(string instanceId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(instanceId)) throw new ArgumentException("Runtime instance ID is required.", nameof(instanceId));
            const string sql = @"SELECT InstanceId, ProfileId, HostInstanceId, UserId, WorkspaceId, SessionId, Scope, State, CreatedAt, UpdatedAt
FROM HAgentRuntimeInstances WHERE InstanceId=@InstanceId;";
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@InstanceId", instanceId);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? Read(reader) : null;
            }
        }

        public async Task<IReadOnlyList<AgentRuntimeStateRecord>> SearchAsync(AgentRuntimeStateQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            query = query ?? new AgentRuntimeStateQuery();
            const string sql = @"SELECT InstanceId, ProfileId, HostInstanceId, UserId, WorkspaceId, SessionId, Scope, State, CreatedAt, UpdatedAt
FROM HAgentRuntimeInstances
WHERE (@HostInstanceId='' OR HostInstanceId=@HostInstanceId)
  AND (@UserId='' OR UserId=@UserId)
  AND (@WorkspaceId='' OR WorkspaceId=@WorkspaceId)
  AND (@SessionId='' OR SessionId=@SessionId)
  AND (@ProfileId='' OR ProfileId=@ProfileId)
  AND (@Scope='' OR Scope=@Scope)
ORDER BY UpdatedAt DESC LIMIT @MaxResults;";
            var result = new List<AgentRuntimeStateRecord>();
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@HostInstanceId", query.HostInstanceId ?? string.Empty);
                command.Parameters.AddWithValue("@UserId", query.UserId ?? string.Empty);
                command.Parameters.AddWithValue("@WorkspaceId", query.WorkspaceId ?? string.Empty);
                command.Parameters.AddWithValue("@SessionId", query.SessionId ?? string.Empty);
                command.Parameters.AddWithValue("@ProfileId", query.ProfileId ?? string.Empty);
                command.Parameters.AddWithValue("@Scope", query.Scope.HasValue ? query.Scope.Value.ToString() : string.Empty);
                command.Parameters.AddWithValue("@MaxResults", query.GetEffectiveMaxResults());
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) result.Add(Read(reader));
            }
            return result.AsReadOnly();
        }

        public async Task DeleteAsync(string instanceId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(instanceId)) throw new ArgumentException("Runtime instance ID is required.", nameof(instanceId));
            const string sql = "DELETE FROM HAgentRuntimeInstances WHERE InstanceId=@InstanceId;";
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@InstanceId", instanceId);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static void Bind(MySqlCommand command, AgentRuntimeStateRecord record)
        {
            command.Parameters.AddWithValue("@InstanceId", record.InstanceId);
            command.Parameters.AddWithValue("@ProfileId", record.ProfileId);
            command.Parameters.AddWithValue("@HostInstanceId", DbValue(record.HostInstanceId));
            command.Parameters.AddWithValue("@UserId", DbValue(record.UserId));
            command.Parameters.AddWithValue("@WorkspaceId", DbValue(record.WorkspaceId));
            command.Parameters.AddWithValue("@SessionId", DbValue(record.SessionId));
            command.Parameters.AddWithValue("@Scope", record.Scope.ToString());
            command.Parameters.AddWithValue("@State", record.State.ToString());
            command.Parameters.AddWithValue("@CreatedAt", record.CreatedAt.UtcDateTime);
            command.Parameters.AddWithValue("@UpdatedAt", record.UpdatedAt.UtcDateTime);
        }

        private static object DbValue(string value) { return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value; }

        private static AgentRuntimeStateRecord Read(MySqlDataReader reader)
        {
            AgentRuntimeScope scope;
            AgentRuntimeInstanceState state;
            Enum.TryParse(reader.GetString(6), true, out scope);
            Enum.TryParse(reader.GetString(7), true, out state);
            return new AgentRuntimeStateRecord
            {
                InstanceId = reader.GetString(0), ProfileId = reader.GetString(1),
                HostInstanceId = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                UserId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                WorkspaceId = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                SessionId = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Scope = scope, State = state,
                CreatedAt = new DateTimeOffset(reader.GetDateTime(8), TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(reader.GetDateTime(9), TimeSpan.Zero)
            };
        }

        private static void ValidateRecord(AgentRuntimeStateRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(record.InstanceId)) throw new ArgumentException("Runtime instance ID is required.", nameof(record));
            if (string.IsNullOrWhiteSpace(record.ProfileId)) throw new ArgumentException("Runtime profile ID is required.", nameof(record));
        }
    }
}
