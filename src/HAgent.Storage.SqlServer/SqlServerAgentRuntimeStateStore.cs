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
        LifecycleRevision bigint NOT NULL CONSTRAINT DF_HAgentRuntimeInstances_LifecycleRevision DEFAULT (0),
        HealthStatus nvarchar(50) NOT NULL CONSTRAINT DF_HAgentRuntimeInstances_HealthStatus DEFAULT ('Unknown'),
        HealthSource nvarchar(50) NOT NULL CONSTRAINT DF_HAgentRuntimeInstances_HealthSource DEFAULT ('RuntimeObservation'),
        HealthFailureKind nvarchar(50) NOT NULL CONSTRAINT DF_HAgentRuntimeInstances_HealthFailureKind DEFAULT ('None'),
        HealthReason nvarchar(512) NOT NULL CONSTRAINT DF_HAgentRuntimeInstances_HealthReason DEFAULT (''),
        HealthEvidence nvarchar(2048) NOT NULL CONSTRAINT DF_HAgentRuntimeInstances_HealthEvidence DEFAULT (''),
        HealthObservedAt datetimeoffset NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
    CREATE INDEX IX_HAgentRuntimeInstances_ProfileUpdated ON dbo.HAgentRuntimeInstances(ProfileId, UpdatedAt DESC);
    CREATE INDEX IX_HAgentRuntimeInstances_HostUser ON dbo.HAgentRuntimeInstances(HostInstanceId, UserId, UpdatedAt DESC);
    CREATE INDEX IX_HAgentRuntimeInstances_Workspace ON dbo.HAgentRuntimeInstances(WorkspaceId, UpdatedAt DESC);
END;
IF OBJECT_ID(N'dbo.HAgentRuntimeInstances', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.HAgentRuntimeInstances', N'LifecycleRevision') IS NULL
BEGIN
    ALTER TABLE dbo.HAgentRuntimeInstances
        ADD LifecycleRevision bigint NOT NULL
            CONSTRAINT DF_HAgentRuntimeInstances_LifecycleRevision DEFAULT (0) WITH VALUES;
END;
IF OBJECT_ID(N'dbo.HAgentRuntimeInstances', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.HAgentRuntimeInstances', N'HealthStatus') IS NULL
BEGIN
    ALTER TABLE dbo.HAgentRuntimeInstances ADD HealthStatus nvarchar(50) NOT NULL CONSTRAINT DF_HAgentRuntimeInstances_HealthStatus DEFAULT ('Unknown') WITH VALUES;
END;
IF OBJECT_ID(N'dbo.HAgentRuntimeInstances', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.HAgentRuntimeInstances', N'HealthSource') IS NULL
BEGIN
    ALTER TABLE dbo.HAgentRuntimeInstances ADD HealthSource nvarchar(50) NOT NULL CONSTRAINT DF_HAgentRuntimeInstances_HealthSource DEFAULT ('RuntimeObservation') WITH VALUES;
END;
IF OBJECT_ID(N'dbo.HAgentRuntimeInstances', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.HAgentRuntimeInstances', N'HealthFailureKind') IS NULL
BEGIN
    ALTER TABLE dbo.HAgentRuntimeInstances ADD HealthFailureKind nvarchar(50) NOT NULL CONSTRAINT DF_HAgentRuntimeInstances_HealthFailureKind DEFAULT ('None') WITH VALUES;
END;
IF OBJECT_ID(N'dbo.HAgentRuntimeInstances', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.HAgentRuntimeInstances', N'HealthReason') IS NULL
BEGIN
    ALTER TABLE dbo.HAgentRuntimeInstances ADD HealthReason nvarchar(512) NOT NULL CONSTRAINT DF_HAgentRuntimeInstances_HealthReason DEFAULT ('') WITH VALUES;
END;
IF OBJECT_ID(N'dbo.HAgentRuntimeInstances', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.HAgentRuntimeInstances', N'HealthEvidence') IS NULL
BEGIN
    ALTER TABLE dbo.HAgentRuntimeInstances ADD HealthEvidence nvarchar(2048) NOT NULL CONSTRAINT DF_HAgentRuntimeInstances_HealthEvidence DEFAULT ('') WITH VALUES;
END;
IF OBJECT_ID(N'dbo.HAgentRuntimeInstances', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.HAgentRuntimeInstances', N'HealthObservedAt') IS NULL
BEGIN
    ALTER TABLE dbo.HAgentRuntimeInstances ADD HealthObservedAt datetimeoffset NULL;
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
    LifecycleRevision=@LifecycleRevision, HealthStatus=@HealthStatus, HealthSource=@HealthSource,
    HealthFailureKind=@HealthFailureKind, HealthReason=@HealthReason, HealthEvidence=@HealthEvidence,
    HealthObservedAt=@HealthObservedAt, CreatedAt=@CreatedAt, UpdatedAt=@UpdatedAt
WHEN NOT MATCHED THEN INSERT
    (InstanceId, ProfileId, HostInstanceId, UserId, WorkspaceId, SessionId, Scope, State, LifecycleRevision,
     HealthStatus, HealthSource, HealthFailureKind, HealthReason, HealthEvidence, HealthObservedAt, CreatedAt, UpdatedAt)
VALUES
    (@InstanceId, @ProfileId, @HostInstanceId, @UserId, @WorkspaceId, @SessionId, @Scope, @State, @LifecycleRevision,
     @HealthStatus, @HealthSource, @HealthFailureKind, @HealthReason, @HealthEvidence, @HealthObservedAt, @CreatedAt, @UpdatedAt);";
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
            const string sql = @"SELECT InstanceId, ProfileId, HostInstanceId, UserId, WorkspaceId, SessionId, Scope, State, LifecycleRevision,
HealthStatus, HealthSource, HealthFailureKind, HealthReason, HealthEvidence, HealthObservedAt, CreatedAt, UpdatedAt
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
InstanceId, ProfileId, HostInstanceId, UserId, WorkspaceId, SessionId, Scope, State, LifecycleRevision,
HealthStatus, HealthSource, HealthFailureKind, HealthReason, HealthEvidence, HealthObservedAt, CreatedAt, UpdatedAt
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
            command.Parameters.AddWithValue("@LifecycleRevision", record.LifecycleRevision);
            command.Parameters.AddWithValue("@HealthStatus", record.Health.Status.ToString());
            command.Parameters.AddWithValue("@HealthSource", record.Health.Source.ToString());
            command.Parameters.AddWithValue("@HealthFailureKind", record.Health.FailureKind.ToString());
            command.Parameters.AddWithValue("@HealthReason", record.Health.Reason);
            command.Parameters.AddWithValue("@HealthEvidence", record.Health.Evidence);
            command.Parameters.AddWithValue("@HealthObservedAt", record.Health.ObservedAt.HasValue ? (object)record.Health.ObservedAt.Value : DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", record.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", record.UpdatedAt);
        }

        private static object DbValue(string value) { return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value; }

        private static AgentRuntimeStateRecord Read(SqlDataReader reader)
        {
            AgentRuntimeScope scope;
            AgentRuntimeInstanceState state;
            AiRuntimeHealthStatus healthStatus;
            AiRuntimeHealthSource healthSource;
            AiRuntimeHealthFailureKind healthFailureKind;
            Enum.TryParse(reader.GetString(6), true, out scope);
            Enum.TryParse(reader.GetString(7), true, out state);
            Enum.TryParse(reader.GetString(9), true, out healthStatus);
            Enum.TryParse(reader.GetString(10), true, out healthSource);
            Enum.TryParse(reader.GetString(11), true, out healthFailureKind);
            DateTimeOffset? observedAt = reader.IsDBNull(14) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(14);
            var health = new AiRuntimeHealth(
                healthStatus,
                healthSource,
                healthFailureKind,
                reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                observedAt);
            health.Validate();

            return new AgentRuntimeStateRecord
            {
                InstanceId = reader.GetString(0), ProfileId = reader.GetString(1),
                HostInstanceId = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                UserId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                WorkspaceId = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                SessionId = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Scope = scope, State = state,
                LifecycleRevision = reader.GetInt64(8),
                Health = health,
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(15),
                UpdatedAt = reader.GetFieldValue<DateTimeOffset>(16)
            };
        }

        private static void ValidateRecord(AgentRuntimeStateRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(record.InstanceId)) throw new ArgumentException("Runtime instance ID is required.", nameof(record));
            if (string.IsNullOrWhiteSpace(record.ProfileId)) throw new ArgumentException("Runtime profile ID is required.", nameof(record));
            if (record.LifecycleRevision < 0) throw new ArgumentException("Runtime lifecycle revision cannot be negative.", nameof(record));
            if (record.Health == null) throw new ArgumentException("Runtime health state is required.", nameof(record));
            record.Health.Validate();
        }
    }
}
