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
    public sealed class SqlServerAiStore : IAiStore
    {
        private readonly string _connectionString;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions();

        public SqlServerAiStore(string connectionString) { _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString)); }

        public static async Task EnsureSchemaAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.HAgentProviders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HAgentProviders (
        Id nvarchar(64) NOT NULL CONSTRAINT PK_HAgentProviders PRIMARY KEY,
        Name nvarchar(200) NOT NULL,
        Kind nvarchar(100) NOT NULL,
        BaseUrl nvarchar(1000) NOT NULL,
        DefaultModel nvarchar(200) NULL,
        DefaultSystemPrompt nvarchar(max) NULL,
        SecretId nvarchar(200) NULL,
        Enabled bit NOT NULL CONSTRAINT DF_HAgentProviders_Enabled DEFAULT(1)
    );
END;
IF OBJECT_ID(N'dbo.HAgentAgents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HAgentAgents (
        Id nvarchar(64) NOT NULL CONSTRAINT PK_HAgentAgents PRIMARY KEY,
        Name nvarchar(200) NOT NULL,
        SystemPrompt nvarchar(max) NULL,
        UseProviderSystemPrompt bit NOT NULL CONSTRAINT DF_HAgentAgents_UseProviderPrompt DEFAULT(1),
        Temperature float NULL,
        MaxOutputTokens int NULL,
        Enabled bit NOT NULL CONSTRAINT DF_HAgentAgents_Enabled DEFAULT(1),
        ToolIdsJson nvarchar(max) NULL,
        ExecutionSelectionJson nvarchar(max) NULL,
        CapabilityRequirementsJson nvarchar(max) NULL
    );
END;
IF COL_LENGTH(N'dbo.HAgentAgents', N'ToolIdsJson') IS NULL ALTER TABLE dbo.HAgentAgents ADD ToolIdsJson nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.HAgentAgents', N'ExecutionSelectionJson') IS NULL ALTER TABLE dbo.HAgentAgents ADD ExecutionSelectionJson nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.HAgentAgents', N'CapabilityRequirementsJson') IS NULL ALTER TABLE dbo.HAgentAgents ADD CapabilityRequirementsJson nvarchar(max) NULL;
IF OBJECT_ID(N'dbo.HAgentPolicies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HAgentPolicies (
        Id nvarchar(64) NOT NULL CONSTRAINT PK_HAgentPolicies PRIMARY KEY,
        PolicyVersion nvarchar(128) NOT NULL,
        PolicyJson nvarchar(max) NOT NULL
    );
END;";
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyList<AiProvider>> GetProvidersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<AiProvider>();
            const string sql = "SELECT Id, Name, Kind, BaseUrl, DefaultModel, DefaultSystemPrompt, SecretId, Enabled FROM dbo.HAgentProviders ORDER BY Name";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        list.Add(new AiProvider
                        {
                            Id = reader.GetString(0), Name = reader.GetString(1), Kind = reader.GetString(2), BaseUrl = reader.GetString(3),
                            DefaultModel = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            DefaultSystemPrompt = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            SecretId = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                            Enabled = reader.GetBoolean(7)
                        });
            }
            return list.AsReadOnly();
        }

        public async Task<IReadOnlyList<AiAgent>> GetAgentsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<AiAgent>();
            const string sql = "SELECT Id, Name, SystemPrompt, UseProviderSystemPrompt, Temperature, MaxOutputTokens, Enabled, ToolIdsJson, ExecutionSelectionJson, CapabilityRequirementsJson FROM dbo.HAgentAgents ORDER BY Name";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var agent = new AiAgent
                        {
                            Id = reader.GetString(0), Name = reader.GetString(1),
                            SystemPrompt = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            UseProviderSystemPrompt = reader.GetBoolean(3),
                            Temperature = reader.IsDBNull(4) ? (double?)null : reader.GetDouble(4),
                            MaxOutputTokens = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                            Enabled = reader.GetBoolean(6)
                        };
                        DeserializeInto(reader.IsDBNull(7) ? null : reader.GetString(7), agent.ToolIds);
                        var selection = Deserialize<AiExecutionSelectionPolicy>(reader.IsDBNull(8) ? null : reader.GetString(8));
                        if (selection != null) agent.ExecutionSelection = selection;
                        var requirements = Deserialize<AiCapabilityRequirements>(reader.IsDBNull(9) ? null : reader.GetString(9));
                        if (requirements != null) agent.CapabilityRequirements = requirements;
                        list.Add(agent);
                    }
                }
            }
            return list.AsReadOnly();
        }

        public async Task<AiPolicySet> GetPolicySetAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "SELECT PolicyJson FROM dbo.HAgentPolicies WHERE Id=N'default';";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (value == null || value == DBNull.Value)
                    return new AiPolicySet();

                var policy = Deserialize<AiPolicySet>(Convert.ToString(value));
                if (policy == null) throw new InvalidOperationException("Persisted HAgent policy configuration is invalid.");
                policy.Validate();
                return policy.Clone();
            }
        }

        public Task SaveProviderAsync(AiProvider p, CancellationToken cancellationToken = default(CancellationToken)) => ExecuteProviderAsync(p, cancellationToken);
        public Task SaveAgentAsync(AiAgent a, CancellationToken cancellationToken = default(CancellationToken)) => ExecuteAgentAsync(a, cancellationToken);

        public async Task SavePolicySetAsync(AiPolicySet policy, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            policy.Validate();
            const string sql = @"MERGE dbo.HAgentPolicies AS target
USING (SELECT @Id Id) AS source ON target.Id=source.Id
WHEN MATCHED THEN UPDATE SET PolicyVersion=@PolicyVersion, PolicyJson=@PolicyJson
WHEN NOT MATCHED THEN INSERT (Id,PolicyVersion,PolicyJson) VALUES (@Id,@PolicyVersion,@PolicyJson);";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", "default");
                command.Parameters.AddWithValue("@PolicyVersion", policy.Version);
                command.Parameters.AddWithValue("@PolicyJson", JsonSerializer.Serialize(policy, JsonOptions));
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteProviderAsync(string providerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "SELECT ExecutionSelectionJson FROM dbo.HAgentAgents";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var selection = Deserialize<AiExecutionSelectionPolicy>(reader.IsDBNull(0) ? null : reader.GetString(0));
                        if (selection != null && string.Equals(selection.PreferredProviderId, providerId, StringComparison.OrdinalIgnoreCase))
                            throw new InvalidOperationException("Provider cannot be deleted while an agent explicitly prefers it.");
                    }
                }
            }

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("DELETE FROM dbo.HAgentProviders WHERE Id=@id", connection))
            {
                command.Parameters.AddWithValue("@id", providerId);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteAgentAsync(string agentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("DELETE FROM dbo.HAgentAgents WHERE Id=@id", connection))
            {
                command.Parameters.AddWithValue("@id", agentId);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ExecuteProviderAsync(AiProvider p, CancellationToken token)
        {
            const string sql = @"MERGE dbo.HAgentProviders AS target
USING (SELECT @Id Id) AS source ON target.Id=source.Id
WHEN MATCHED THEN UPDATE SET Name=@Name, Kind=@Kind, BaseUrl=@BaseUrl, DefaultModel=@DefaultModel, DefaultSystemPrompt=@DefaultSystemPrompt, SecretId=@SecretId, Enabled=@Enabled
WHEN NOT MATCHED THEN INSERT (Id,Name,Kind,BaseUrl,DefaultModel,DefaultSystemPrompt,SecretId,Enabled) VALUES (@Id,@Name,@Kind,@BaseUrl,@DefaultModel,@DefaultSystemPrompt,@SecretId,@Enabled);";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                BindProvider(command, p); await connection.OpenAsync(token).ConfigureAwait(false); await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }
        }

        private async Task ExecuteAgentAsync(AiAgent a, CancellationToken token)
        {
            const string sql = @"MERGE dbo.HAgentAgents AS target
USING (SELECT @Id Id) AS source ON target.Id=source.Id
WHEN MATCHED THEN UPDATE SET Name=@Name, SystemPrompt=@SystemPrompt, UseProviderSystemPrompt=@UseProviderSystemPrompt, Temperature=@Temperature, MaxOutputTokens=@MaxOutputTokens, Enabled=@Enabled, ToolIdsJson=@ToolIdsJson, ExecutionSelectionJson=@ExecutionSelectionJson, CapabilityRequirementsJson=@CapabilityRequirementsJson
WHEN NOT MATCHED THEN INSERT (Id,Name,SystemPrompt,UseProviderSystemPrompt,Temperature,MaxOutputTokens,Enabled,ToolIdsJson,ExecutionSelectionJson,CapabilityRequirementsJson) VALUES (@Id,@Name,@SystemPrompt,@UseProviderSystemPrompt,@Temperature,@MaxOutputTokens,@Enabled,@ToolIdsJson,@ExecutionSelectionJson,@CapabilityRequirementsJson);";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                BindAgent(command, a); await connection.OpenAsync(token).ConfigureAwait(false); await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }
        }

        private static void BindProvider(SqlCommand c, AiProvider p)
        {
            c.Parameters.AddWithValue("@Id", p.Id); c.Parameters.AddWithValue("@Name", p.Name); c.Parameters.AddWithValue("@Kind", p.Kind);
            c.Parameters.AddWithValue("@BaseUrl", p.BaseUrl); c.Parameters.AddWithValue("@DefaultModel", (object)p.DefaultModel ?? DBNull.Value);
            c.Parameters.AddWithValue("@DefaultSystemPrompt", (object)p.DefaultSystemPrompt ?? DBNull.Value); c.Parameters.AddWithValue("@SecretId", (object)p.SecretId ?? DBNull.Value);
            c.Parameters.AddWithValue("@Enabled", p.Enabled);
        }

        private static void BindAgent(SqlCommand c, AiAgent a)
        {
            c.Parameters.AddWithValue("@Id", a.Id); c.Parameters.AddWithValue("@Name", a.Name);
            c.Parameters.AddWithValue("@SystemPrompt", (object)a.SystemPrompt ?? DBNull.Value); c.Parameters.AddWithValue("@UseProviderSystemPrompt", a.UseProviderSystemPrompt);
            c.Parameters.AddWithValue("@Temperature", (object)a.Temperature ?? DBNull.Value); c.Parameters.AddWithValue("@MaxOutputTokens", (object)a.MaxOutputTokens ?? DBNull.Value);
            c.Parameters.AddWithValue("@Enabled", a.Enabled);
            c.Parameters.AddWithValue("@ToolIdsJson", JsonSerializer.Serialize(a.ToolIds ?? new List<string>(), JsonOptions));
            c.Parameters.AddWithValue("@ExecutionSelectionJson", JsonSerializer.Serialize(a.ExecutionSelection ?? new AiExecutionSelectionPolicy(), JsonOptions));
            c.Parameters.AddWithValue("@CapabilityRequirementsJson", JsonSerializer.Serialize(a.CapabilityRequirements ?? new AiCapabilityRequirements(), JsonOptions));
        }

        private static T Deserialize<T>(string json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<T>(json, JsonOptions); }
            catch (JsonException) { return null; }
        }

        private static void DeserializeInto(string json, IList<string> target)
        {
            if (target == null || string.IsNullOrWhiteSpace(json)) return;
            try
            {
                var values = JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
                if (values == null) return;
                target.Clear();
                foreach (var value in values) target.Add(value);
            }
            catch (JsonException) { }
        }
    }
}
