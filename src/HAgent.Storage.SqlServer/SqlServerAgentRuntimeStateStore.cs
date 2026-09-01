using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using Microsoft.Data.SqlClient;

namespace HAgent.Storage.SqlServer
{
    public sealed class SqlServerAgentRuntimeStateStore : IAgentRuntimeStateStore
    {
        private readonly string _connectionString;

        public SqlServerAgentRuntimeStateStore(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public static async Task EnsureSchemaAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.HAgentRuntimeInstances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HAgentRuntimeInstances (
        InstanceId nvarchar(128) NOT NULL CONSTRAINT PK_HAgentRuntimeInstances PRIMARY KEY,
        ProfileId nvarchar(128) NOT NULL,
        HostInstanceId nvarchar(128) NULL,
        UserId nvarchar(128) NULL,
        WorkspaceId nvarchar(128) NULL,
        SessionId nvarchar(128) NULL,
        Scope nvarchar(50) NOT NULL,
        State nvarchar(50) NOT NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
    CREATE INDEX IX_HAgentRuntimeInstances_ProfileUpdated ON dbo.HAgentRuntimeInstances(ProfileId, UpdatedAt DESC);
    CREATE INDEX IX_HAgentRuntimeInstances_HostUser ON dbo.HAgentRuntimeInstances(HostInstanceId, UserId, UpdatedAt DESC);
    CREATE INDEX IX_HAgentRuntimeInstances_Workspace ON dbo.HAgentRuntimeInstances(WorkspaceId, UpdatedAt DESC);
END;";
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = 60;
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task SaveAsync(AgentRuntimeStateRecord record, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateRecord(record);
            const string sql = @"
MERGE dbo.HAgentRuntimeInstances AS target
USING (SELECT @InstanceId AS InstanceId) AS source
ON target.InstanceId = source.InstanceId
WHEN MATCHED THEN UPDATE SET
    ProfileId=@ProfileId, HostInstanceId=@HostInstanceId, UserId=@UserId,
    WorkspaceId=@WorkspaceId, SessionId=@SessionId, Scope=@Scope, State=@State,
    CreatedAt=@CreatedAt, UpdatedAt=@UpdatedAt
WHEN NOT MATCHED THEN INSERT
    (InstanceId, ProfileId, HostInstanceId, UserId, WorkspaceId, SessionId, Scope, State, CreatedAt, UpdatedAt)
VALUES
    (@InstanceId, @ProfileId, @HostInstanceId, @UserId, @WorkspaceId, @SessionId, @Scope, @State, @CreatedAt, @UpdatedAt);";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
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
FROM dbo.HAgentRuntimeInstances WHERE InstanceId=@InstanceId;";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
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
            const string sql = @"SELECT TOP (@MaxResults)
InstanceId, ProfileId, HostInstanceId, UserId, WorkspaceId, SessionId, Scope, State, CreatedAt, UpdatedAt
FROM dbo.HAgentRuntimeInstances
WHERE (@HostInstanceId=N'' OR HostInstanceId=@HostInstanceId)
  AND (@UserId=N'' OR UserId=@UserId)
  AND (@WorkspaceId=N'' OR WorkspaceId=@WorkspaceId)
  AND (@SessionId=N'' OR SessionId=@SessionId)
  AND (@ProfileId=N'' OR ProfileId=@ProfileId)
  AND (@Scope=N'' OR Scope=@Scope)
ORDER BY UpdatedAt DESC;";
            var result = new List<AgentRuntimeStateRecord>();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@MaxResults", query.GetEffectiveMaxResults());
                command.Parameters.AddWithValue("@HostInstanceId", query.HostInstanceId ?? string.Empty);
                command.Parameters.AddWithValue("@UserId", query.UserId ?? string.Empty);
                command.Parameters.AddWithValue("@WorkspaceId", query.WorkspaceId ?? string.Empty);
                command.Parameters.AddWithValue("@SessionId", query.SessionId ?? string.Empty);
                command.Parameters.AddWithValue("@ProfileId", query.ProfileId ?? string.Empty);
                command.Parameters.AddWithValue("@Scope", query.Scope.HasValue ? query.Scope.Value.ToString() : string.Empty);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) result.Add(Read(reader));
            }
            return result.AsReadOnly();
        }

        public async Task DeleteAsync(string instanceId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(instanceId)) throw new ArgumentException("Runtime instance ID is required.", nameof(instanceId));
            const string sql = "DELETE FROM dbo.HAgentRuntimeInstances WHERE InstanceId=@InstanceId;";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@InstanceId", instanceId);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static void Bind(SqlCommand command, AgentRuntimeStateRecord record)
        {
            command.Parameters.AddWithValue("@InstanceId", record.InstanceId);
            command.Parameters.AddWithValue("@ProfileId", record.ProfileId);
            command.Parameters.AddWithValue("@HostInstanceId", DbValue(record.HostInstanceId));
            command.Parameters.AddWithValue("@UserId", DbValue(record.UserId));
            command.Parameters.AddWithValue("@WorkspaceId", DbValue(record.WorkspaceId));
            command.Parameters.AddWithValue("@SessionId", DbValue(record.SessionId));
            command.Parameters.AddWithValue("@Scope", record.Scope.ToString());
            command.Parameters.AddWithValue("@State", record.State.ToString());
            command.Parameters.AddWithValue("@CreatedAt", record.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", record.UpdatedAt);
        }

        private static object DbValue(string value) { return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value; }

        private static AgentRuntimeStateRecord Read(SqlDataReader reader)
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
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(8),
                UpdatedAt = reader.GetFieldValue<DateTimeOffset>(9)
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
