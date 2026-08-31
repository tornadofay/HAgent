using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using Microsoft.Data.SqlClient;

namespace HAgent.Storage.SqlServer
{
    public sealed class SqlServerExecutionAuditStore : IExecutionAuditStore
    {
        private readonly string _connectionString;

        public SqlServerExecutionAuditStore(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task AppendAsync(AgentExecutionAuditRecord record, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"INSERT INTO dbo.HAgentExecutionAudits
(ExecutionId, CorrelationId, AgentId, AgentName, Model, LastProviderId, LastProviderName, State, FailureKind, ProviderErrorKind, CreatedAt, StartedAt, CompletedAt, DurationMs)
VALUES
(@ExecutionId, @CorrelationId, @AgentId, @AgentName, @Model, @LastProviderId, @LastProviderName, @State, @FailureKind, @ProviderErrorKind, @CreatedAt, @StartedAt, @CompletedAt, @DurationMs);";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                Bind(command, record);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyList<AgentExecutionAuditRecord>> SearchAsync(ExecutionAuditQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            query = query ?? new ExecutionAuditQuery();
            const string sql = @"SELECT TOP (@MaxResults)
ExecutionId, CorrelationId, AgentId, AgentName, Model, LastProviderId, LastProviderName, State, FailureKind, ProviderErrorKind, CreatedAt, StartedAt, CompletedAt, DurationMs
FROM dbo.HAgentExecutionAudits
WHERE (@ExecutionId = N'' OR ExecutionId = @ExecutionId)
  AND (@CorrelationId = N'' OR CorrelationId = @CorrelationId)
  AND (@AgentId = N'' OR AgentId = @AgentId)
ORDER BY CreatedAt DESC;";

            var result = new List<AgentExecutionAuditRecord>();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@MaxResults", query.GetEffectiveMaxResults());
                command.Parameters.AddWithValue("@ExecutionId", query.ExecutionId ?? string.Empty);
                command.Parameters.AddWithValue("@CorrelationId", query.CorrelationId ?? string.Empty);
                command.Parameters.AddWithValue("@AgentId", query.AgentId ?? string.Empty);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        result.Add(Read(reader));
                }
            }
            return result.AsReadOnly();
        }

        private static void Bind(SqlCommand command, AgentExecutionAuditRecord record)
        {
            command.Parameters.AddWithValue("@ExecutionId", record.ExecutionId ?? string.Empty);
            command.Parameters.AddWithValue("@CorrelationId", record.CorrelationId ?? string.Empty);
            command.Parameters.AddWithValue("@AgentId", record.AgentId ?? string.Empty);
            command.Parameters.AddWithValue("@AgentName", record.AgentName ?? string.Empty);
            command.Parameters.AddWithValue("@Model", record.Model ?? string.Empty);
            command.Parameters.AddWithValue("@LastProviderId", record.LastProviderId ?? string.Empty);
            command.Parameters.AddWithValue("@LastProviderName", record.LastProviderName ?? string.Empty);
            command.Parameters.AddWithValue("@State", record.State.ToString());
            command.Parameters.AddWithValue("@FailureKind", record.FailureKind.ToString());
            command.Parameters.AddWithValue("@ProviderErrorKind", record.ProviderErrorKind.ToString());
            command.Parameters.AddWithValue("@CreatedAt", record.CreatedAt);
            command.Parameters.AddWithValue("@StartedAt", record.StartedAt.HasValue ? (object)record.StartedAt.Value : DBNull.Value);
            command.Parameters.AddWithValue("@CompletedAt", record.CompletedAt.HasValue ? (object)record.CompletedAt.Value : DBNull.Value);
            command.Parameters.AddWithValue("@DurationMs", record.Duration.HasValue ? (object)record.Duration.Value.TotalMilliseconds : DBNull.Value);
        }

        private static AgentExecutionAuditRecord Read(SqlDataReader reader)
        {
            Runtime.AgentExecutionState state;
            AgentExecutionFailureKind failure;
            ProviderErrorKind providerError;
            Enum.TryParse(reader.GetString(7), true, out state);
            Enum.TryParse(reader.GetString(8), true, out failure);
            Enum.TryParse(reader.GetString(9), true, out providerError);

            return new AgentExecutionAuditRecord
            {
                ExecutionId = reader.GetString(0),
                CorrelationId = reader.GetString(1),
                AgentId = reader.GetString(2),
                AgentName = reader.GetString(3),
                Model = reader.GetString(4),
                LastProviderId = reader.GetString(5),
                LastProviderName = reader.GetString(6),
                State = state,
                FailureKind = failure,
                ProviderErrorKind = providerError,
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(10),
                StartedAt = reader.IsDBNull(11) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(11),
                CompletedAt = reader.IsDBNull(12) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(12),
                Duration = reader.IsDBNull(13) ? (TimeSpan?)null : TimeSpan.FromMilliseconds(Convert.ToDouble(reader.GetValue(13)))
            };
        }
    }
}
