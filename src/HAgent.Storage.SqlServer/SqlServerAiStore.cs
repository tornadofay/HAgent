using System;
using System.Collections.Generic;
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

        public SqlServerAiStore(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

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
        ProviderId nvarchar(64) NOT NULL,
        Model nvarchar(200) NULL,
        SystemPrompt nvarchar(max) NULL,
        UseProviderSystemPrompt bit NOT NULL CONSTRAINT DF_HAgentAgents_UseProviderPrompt DEFAULT(1),
        Temperature float NULL,
        MaxOutputTokens int NULL,
        Enabled bit NOT NULL CONSTRAINT DF_HAgentAgents_Enabled DEFAULT(1),
        CONSTRAINT FK_HAgentAgents_Providers FOREIGN KEY (ProviderId) REFERENCES dbo.HAgentProviders(Id)
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
                        list.Add(new AiProvider {
                            Id = reader.GetString(0), Name = reader.GetString(1), Kind = reader.GetString(2), BaseUrl = reader.GetString(3),
                            DefaultModel = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            DefaultSystemPrompt = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            SecretId = reader.IsDBNull(6) ? string.Empty : reader.GetString(6), Enabled = reader.GetBoolean(7)
                        });
            }
            return list.AsReadOnly();
        }

        public async Task<IReadOnlyList<AiAgent>> GetAgentsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<AiAgent>();
            const string sql = "SELECT Id, Name, ProviderId, Model, SystemPrompt, UseProviderSystemPrompt, Temperature, MaxOutputTokens, Enabled FROM dbo.HAgentAgents ORDER BY Name";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        list.Add(new AiAgent {
                            Id = reader.GetString(0), Name = reader.GetString(1), ProviderId = reader.GetString(2),
                            Model = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            SystemPrompt = reader.IsDBNull(4) ? string.Empty : reader.GetString(4), UseProviderSystemPrompt = reader.GetBoolean(5),
                            Temperature = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                            MaxOutputTokens = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7), Enabled = reader.GetBoolean(8)
                        });
            }
            return list.AsReadOnly();
        }

        public Task SaveProviderAsync(AiProvider p, CancellationToken cancellationToken = default(CancellationToken)) => ExecuteProviderAsync(p, cancellationToken);
        public Task SaveAgentAsync(AiAgent a, CancellationToken cancellationToken = default(CancellationToken)) => ExecuteAgentAsync(a, cancellationToken);

        public async Task DeleteProviderAsync(string providerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("DELETE FROM dbo.HAgentAgents WHERE ProviderId=@id; DELETE FROM dbo.HAgentProviders WHERE Id=@id;", connection))
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
WHEN MATCHED THEN UPDATE SET Name=@Name, ProviderId=@ProviderId, Model=@Model, SystemPrompt=@SystemPrompt, UseProviderSystemPrompt=@UseProviderSystemPrompt, Temperature=@Temperature, MaxOutputTokens=@MaxOutputTokens, Enabled=@Enabled
WHEN NOT MATCHED THEN INSERT (Id,Name,ProviderId,Model,SystemPrompt,UseProviderSystemPrompt,Temperature,MaxOutputTokens,Enabled) VALUES (@Id,@Name,@ProviderId,@Model,@SystemPrompt,@UseProviderSystemPrompt,@Temperature,@MaxOutputTokens,@Enabled);";
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
            c.Parameters.AddWithValue("@Id", a.Id); c.Parameters.AddWithValue("@Name", a.Name); c.Parameters.AddWithValue("@ProviderId", a.ProviderId);
            c.Parameters.AddWithValue("@Model", (object)a.Model ?? DBNull.Value); c.Parameters.AddWithValue("@SystemPrompt", (object)a.SystemPrompt ?? DBNull.Value);
            c.Parameters.AddWithValue("@UseProviderSystemPrompt", a.UseProviderSystemPrompt); c.Parameters.AddWithValue("@Temperature", (object)a.Temperature ?? DBNull.Value);
            c.Parameters.AddWithValue("@MaxOutputTokens", (object)a.MaxOutputTokens ?? DBNull.Value); c.Parameters.AddWithValue("@Enabled", a.Enabled);
        }
    }
}
