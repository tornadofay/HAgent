using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using MySqlConnector;

namespace HAgent.Storage.MySql
{
    public sealed class MySqlAiStore : IAiStore
    {
        private readonly string _connectionString;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions();

        public MySqlAiStore(string connectionString) { _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString)); }

        public static async Task EnsureSchemaAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS HAgentProviders (
 Id varchar(64) NOT NULL PRIMARY KEY, Name varchar(200) NOT NULL, Kind varchar(100) NOT NULL,
 BaseUrl varchar(1000) NOT NULL, DefaultModel varchar(200) NULL, DefaultSystemPrompt longtext NULL,
 SecretId varchar(200) NULL, Enabled bit NOT NULL DEFAULT 1
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS HAgentAgents (
 Id varchar(64) NOT NULL PRIMARY KEY, Name varchar(200) NOT NULL,
 SystemPrompt longtext NULL, UseProviderSystemPrompt bit NOT NULL DEFAULT 1,
 Temperature double NULL, MaxOutputTokens int NULL, Enabled bit NOT NULL DEFAULT 1,
 ToolIdsJson longtext NULL, ExecutionSelectionJson longtext NULL, CapabilityRequirementsJson longtext NULL,
 LearningMode varchar(50) NULL
) ENGINE=InnoDB;
ALTER TABLE HAgentAgents ADD COLUMN IF NOT EXISTS ToolIdsJson longtext NULL;
ALTER TABLE HAgentAgents ADD COLUMN IF NOT EXISTS ExecutionSelectionJson longtext NULL;
ALTER TABLE HAgentAgents ADD COLUMN IF NOT EXISTS CapabilityRequirementsJson longtext NULL;
ALTER TABLE HAgentAgents ADD COLUMN IF NOT EXISTS LearningMode varchar(50) NULL;
UPDATE HAgentAgents SET LearningMode='Disabled' WHERE LearningMode IS NULL;
CREATE TABLE IF NOT EXISTS HAgentPolicies (
 Id varchar(64) NOT NULL PRIMARY KEY, PolicyVersion varchar(128) NOT NULL, PolicyJson longtext NOT NULL
) ENGINE=InnoDB;";
            using (var connection = new MySqlConnection(connectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyList<AiProvider>> GetProvidersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<AiProvider>();
            const string sql = "SELECT Id,Name,Kind,BaseUrl,DefaultModel,DefaultSystemPrompt,SecretId,Enabled FROM HAgentProviders ORDER BY Name";
            using (var c = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand(sql, c))
            {
                await c.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var r = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    while (await r.ReadAsync(cancellationToken).ConfigureAwait(false))
                        list.Add(new AiProvider { Id = r.GetString(0), Name = r.GetString(1), Kind = r.GetString(2), BaseUrl = r.GetString(3), DefaultModel = r.IsDBNull(4) ? string.Empty : r.GetString(4), DefaultSystemPrompt = r.IsDBNull(5) ? string.Empty : r.GetString(5), SecretId = r.IsDBNull(6) ? string.Empty : r.GetString(6), Enabled = r.GetBoolean(7) });
            }
            return list.AsReadOnly();
        }

        public async Task<IReadOnlyList<AiAgent>> GetAgentsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<AiAgent>();
            const string sql = "SELECT Id,Name,SystemPrompt,UseProviderSystemPrompt,Temperature,MaxOutputTokens,Enabled,ToolIdsJson,ExecutionSelectionJson,CapabilityRequirementsJson,LearningMode FROM HAgentAgents ORDER BY Name";
            using (var c = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand(sql, c))
            {
                await c.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var r = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (await r.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var agent = new AiAgent
                        {
                            Id = r.GetString(0), Name = r.GetString(1), SystemPrompt = r.IsDBNull(2) ? string.Empty : r.GetString(2),
                            UseProviderSystemPrompt = r.GetBoolean(3), Temperature = r.IsDBNull(4) ? (double?)null : r.GetDouble(4),
                            MaxOutputTokens = r.IsDBNull(5) ? (int?)null : r.GetInt32(5), Enabled = r.GetBoolean(6),
                            LearningMode = ParseLearningMode(r.IsDBNull(10) ? null : r.GetString(10))
                        };
                        DeserializeInto(r.IsDBNull(7) ? null : r.GetString(7), agent.ToolIds);
                        var selection = Deserialize<AiExecutionSelectionPolicy>(r.IsDBNull(8) ? null : r.GetString(8)); if (selection != null) agent.ExecutionSelection = selection;
                        var requirements = Deserialize<AiCapabilityRequirements>(r.IsDBNull(9) ? null : r.GetString(9)); if (requirements != null) agent.CapabilityRequirements = requirements;
                        list.Add(agent);
                    }
                }
            }
            return list.AsReadOnly();
        }

        public async Task<AiPolicySet> GetPolicySetAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "SELECT PolicyJson FROM HAgentPolicies WHERE Id=@id";
            using (var c = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand(sql, c))
            {
                cmd.Parameters.AddWithValue("@id", "default");
                await c.OpenAsync(cancellationToken).ConfigureAwait(false);
                var value = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (value == null || value == DBNull.Value) return new AiPolicySet();
                var policy = Deserialize<AiPolicySet>(Convert.ToString(value));
                if (policy == null) throw new InvalidOperationException("Persisted HAgent policy configuration is invalid.");
                policy.Validate(); return policy.Clone();
            }
        }

        public Task SaveProviderAsync(AiProvider p, CancellationToken t = default(CancellationToken)) => UpsertProvider(p, t);
        public Task SaveAgentAsync(AiAgent a, CancellationToken t = default(CancellationToken)) => UpsertAgent(a, t);

        public async Task SavePolicySetAsync(AiPolicySet policy, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy)); policy.Validate();
            const string sql = "INSERT INTO HAgentPolicies(Id,PolicyVersion,PolicyJson) VALUES(@Id,@PolicyVersion,@PolicyJson) ON DUPLICATE KEY UPDATE PolicyVersion=VALUES(PolicyVersion),PolicyJson=VALUES(PolicyJson);";
            using (var c = new MySqlConnection(_connectionString)) using (var cmd = new MySqlCommand(sql, c))
            { cmd.Parameters.AddWithValue("@Id", "default"); cmd.Parameters.AddWithValue("@PolicyVersion", policy.Version); cmd.Parameters.AddWithValue("@PolicyJson", JsonSerializer.Serialize(policy, JsonOptions)); await c.OpenAsync(cancellationToken).ConfigureAwait(false); await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false); }
        }

        public async Task DeleteProviderAsync(string id, CancellationToken t = default(CancellationToken))
        {
            using (var c = new MySqlConnection(_connectionString)) using (var cmd = new MySqlCommand("SELECT ExecutionSelectionJson FROM HAgentAgents", c))
            { await c.OpenAsync(t).ConfigureAwait(false); using (var r = await cmd.ExecuteReaderAsync(t).ConfigureAwait(false)) while (await r.ReadAsync(t).ConfigureAwait(false)) { var selection = Deserialize<AiExecutionSelectionPolicy>(r.IsDBNull(0) ? null : r.GetString(0)); if (selection != null && string.Equals(selection.PreferredProviderId, id, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Provider cannot be deleted while an agent explicitly prefers it."); } }
            await ExecuteAsync("DELETE FROM HAgentProviders WHERE Id=@id", id, t).ConfigureAwait(false);
        }

        public Task DeleteAgentAsync(string id, CancellationToken t = default(CancellationToken)) => ExecuteAsync("DELETE FROM HAgentAgents WHERE Id=@id", id, t);

        private async Task UpsertProvider(AiProvider p, CancellationToken t)
        {
            const string sql = "INSERT INTO HAgentProviders(Id,Name,Kind,BaseUrl,DefaultModel,DefaultSystemPrompt,SecretId,Enabled) VALUES(@Id,@Name,@Kind,@BaseUrl,@DefaultModel,@DefaultSystemPrompt,@SecretId,@Enabled) ON DUPLICATE KEY UPDATE Name=VALUES(Name),Kind=VALUES(Kind),BaseUrl=VALUES(BaseUrl),DefaultModel=VALUES(DefaultModel),DefaultSystemPrompt=VALUES(DefaultSystemPrompt),SecretId=VALUES(SecretId),Enabled=VALUES(Enabled);";
            using (var c = new MySqlConnection(_connectionString)) using (var cmd = new MySqlCommand(sql, c)) { BindProvider(cmd, p); await c.OpenAsync(t).ConfigureAwait(false); await cmd.ExecuteNonQueryAsync(t).ConfigureAwait(false); }
        }

        private async Task UpsertAgent(AiAgent a, CancellationToken t)
        {
            const string sql = "INSERT INTO HAgentAgents(Id,Name,SystemPrompt,UseProviderSystemPrompt,Temperature,MaxOutputTokens,Enabled,ToolIdsJson,ExecutionSelectionJson,CapabilityRequirementsJson,LearningMode) VALUES(@Id,@Name,@SystemPrompt,@UseProviderSystemPrompt,@Temperature,@MaxOutputTokens,@Enabled,@ToolIdsJson,@ExecutionSelectionJson,@CapabilityRequirementsJson,@LearningMode) ON DUPLICATE KEY UPDATE Name=VALUES(Name),SystemPrompt=VALUES(SystemPrompt),UseProviderSystemPrompt=VALUES(UseProviderSystemPrompt),Temperature=VALUES(Temperature),MaxOutputTokens=VALUES(MaxOutputTokens),Enabled=VALUES(Enabled),ToolIdsJson=VALUES(ToolIdsJson),ExecutionSelectionJson=VALUES(ExecutionSelectionJson),CapabilityRequirementsJson=VALUES(CapabilityRequirementsJson),LearningMode=VALUES(LearningMode);";
            using (var c = new MySqlConnection(_connectionString)) using (var cmd = new MySqlCommand(sql, c)) { BindAgent(cmd, a); await c.OpenAsync(t).ConfigureAwait(false); await cmd.ExecuteNonQueryAsync(t).ConfigureAwait(false); }
        }

        private async Task ExecuteAsync(string sql, string id, CancellationToken t)
        { using (var c = new MySqlConnection(_connectionString)) using (var cmd = new MySqlCommand(sql, c)) { cmd.Parameters.AddWithValue("@id", id); await c.OpenAsync(t).ConfigureAwait(false); await cmd.ExecuteNonQueryAsync(t).ConfigureAwait(false); } }

        private static void BindProvider(MySqlCommand c, AiProvider p)
        { c.Parameters.AddWithValue("@Id", p.Id); c.Parameters.AddWithValue("@Name", p.Name); c.Parameters.AddWithValue("@Kind", p.Kind); c.Parameters.AddWithValue("@BaseUrl", p.BaseUrl); c.Parameters.AddWithValue("@DefaultModel", p.DefaultModel); c.Parameters.AddWithValue("@DefaultSystemPrompt", p.DefaultSystemPrompt); c.Parameters.AddWithValue("@SecretId", p.SecretId); c.Parameters.AddWithValue("@Enabled", p.Enabled); }

        private static void BindAgent(MySqlCommand c, AiAgent a)
        {
            c.Parameters.AddWithValue("@Id", a.Id); c.Parameters.AddWithValue("@Name", a.Name); c.Parameters.AddWithValue("@SystemPrompt", a.SystemPrompt); c.Parameters.AddWithValue("@UseProviderSystemPrompt", a.UseProviderSystemPrompt); c.Parameters.AddWithValue("@Temperature", (object)a.Temperature ?? DBNull.Value); c.Parameters.AddWithValue("@MaxOutputTokens", (object)a.MaxOutputTokens ?? DBNull.Value); c.Parameters.AddWithValue("@Enabled", a.Enabled); c.Parameters.AddWithValue("@ToolIdsJson", JsonSerializer.Serialize(a.ToolIds ?? new List<string>(), JsonOptions)); c.Parameters.AddWithValue("@ExecutionSelectionJson", JsonSerializer.Serialize(a.ExecutionSelection ?? new AiExecutionSelectionPolicy(), JsonOptions)); c.Parameters.AddWithValue("@CapabilityRequirementsJson", JsonSerializer.Serialize(a.CapabilityRequirements ?? new AiCapabilityRequirements(), JsonOptions)); c.Parameters.AddWithValue("@LearningMode", a.LearningMode.ToString());
        }

        private static AiLearningMode ParseLearningMode(string value)
        { AiLearningMode parsed; return Enum.TryParse(value, true, out parsed) && Enum.IsDefined(typeof(AiLearningMode), parsed) ? parsed : AiLearningMode.Disabled; }

        private static T Deserialize<T>(string json) where T : class
        { if (string.IsNullOrWhiteSpace(json)) return null; try { return JsonSerializer.Deserialize<T>(json, JsonOptions); } catch (JsonException) { return null; } }

        private static void DeserializeInto(string json, IList<string> target)
        { if (target == null || string.IsNullOrWhiteSpace(json)) return; try { var values = JsonSerializer.Deserialize<List<string>>(json, JsonOptions); if (values == null) return; target.Clear(); foreach (var value in values) target.Add(value); } catch (JsonException) { } }
    }
}
