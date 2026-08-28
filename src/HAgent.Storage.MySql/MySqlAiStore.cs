using System;
using System.Collections.Generic;
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
 Id varchar(64) NOT NULL PRIMARY KEY, Name varchar(200) NOT NULL, ProviderId varchar(64) NOT NULL,
 Model varchar(200) NULL, SystemPrompt longtext NULL, UseProviderSystemPrompt bit NOT NULL DEFAULT 1,
 Temperature double NULL, MaxOutputTokens int NULL, Enabled bit NOT NULL DEFAULT 1,
 CONSTRAINT FK_HAgentAgents_Providers FOREIGN KEY (ProviderId) REFERENCES HAgentProviders(Id) ON DELETE CASCADE
) ENGINE=InnoDB;";
            using (var connection = new MySqlConnection(connectionString)) using (var command = new MySqlCommand(sql, connection))
            { await connection.OpenAsync(cancellationToken).ConfigureAwait(false); await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false); }
        }

        public async Task<IReadOnlyList<AiProvider>> GetProvidersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<AiProvider>();
            using (var c = new MySqlConnection(_connectionString)) using (var cmd = new MySqlCommand("SELECT Id,Name,Kind,BaseUrl,DefaultModel,DefaultSystemPrompt,SecretId,Enabled FROM HAgentProviders ORDER BY Name", c))
            { await c.OpenAsync(cancellationToken).ConfigureAwait(false); using (var r = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false)) while (await r.ReadAsync(cancellationToken).ConfigureAwait(false)) list.Add(new AiProvider { Id=r.GetString(0), Name=r.GetString(1), Kind=r.GetString(2), BaseUrl=r.GetString(3), DefaultModel=r.IsDBNull(4)?"":r.GetString(4), DefaultSystemPrompt=r.IsDBNull(5)?"":r.GetString(5), SecretId=r.IsDBNull(6)?"":r.GetString(6), Enabled=r.GetBoolean(7) }); }
            return list.AsReadOnly();
        }

        public async Task<IReadOnlyList<AiAgent>> GetAgentsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<AiAgent>();
            using (var c = new MySqlConnection(_connectionString)) using (var cmd = new MySqlCommand("SELECT Id,Name,ProviderId,Model,SystemPrompt,UseProviderSystemPrompt,Temperature,MaxOutputTokens,Enabled FROM HAgentAgents ORDER BY Name", c))
            { await c.OpenAsync(cancellationToken).ConfigureAwait(false); using (var r = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false)) while (await r.ReadAsync(cancellationToken).ConfigureAwait(false)) list.Add(new AiAgent { Id=r.GetString(0), Name=r.GetString(1), ProviderId=r.GetString(2), Model=r.IsDBNull(3)?"":r.GetString(3), SystemPrompt=r.IsDBNull(4)?"":r.GetString(4), UseProviderSystemPrompt=r.GetBoolean(5), Temperature=r.IsDBNull(6)?(double?)null:r.GetDouble(6), MaxOutputTokens=r.IsDBNull(7)?(int?)null:r.GetInt32(7), Enabled=r.GetBoolean(8) }); }
            return list.AsReadOnly();
        }

        public Task SaveProviderAsync(AiProvider p, CancellationToken t = default(CancellationToken)) => UpsertProvider(p, t);
        public Task SaveAgentAsync(AiAgent a, CancellationToken t = default(CancellationToken)) => UpsertAgent(a, t);
        public async Task DeleteProviderAsync(string id, CancellationToken t = default(CancellationToken)) { await ExecuteAsync("DELETE FROM HAgentProviders WHERE Id=@id", id, t).ConfigureAwait(false); }
        public async Task DeleteAgentAsync(string id, CancellationToken t = default(CancellationToken)) { await ExecuteAsync("DELETE FROM HAgentAgents WHERE Id=@id", id, t).ConfigureAwait(false); }

        private async Task UpsertProvider(AiProvider p, CancellationToken t) { const string sql="INSERT INTO HAgentProviders(Id,Name,Kind,BaseUrl,DefaultModel,DefaultSystemPrompt,SecretId,Enabled) VALUES(@Id,@Name,@Kind,@BaseUrl,@DefaultModel,@DefaultSystemPrompt,@SecretId,@Enabled) ON DUPLICATE KEY UPDATE Name=VALUES(Name),Kind=VALUES(Kind),BaseUrl=VALUES(BaseUrl),DefaultModel=VALUES(DefaultModel),DefaultSystemPrompt=VALUES(DefaultSystemPrompt),SecretId=VALUES(SecretId),Enabled=VALUES(Enabled);"; using(var c=new MySqlConnection(_connectionString)) using(var cmd=new MySqlCommand(sql,c)){ BindProvider(cmd,p); await c.OpenAsync(t).ConfigureAwait(false); await cmd.ExecuteNonQueryAsync(t).ConfigureAwait(false); } }
        private async Task UpsertAgent(AiAgent a, CancellationToken t) { const string sql="INSERT INTO HAgentAgents(Id,Name,ProviderId,Model,SystemPrompt,UseProviderSystemPrompt,Temperature,MaxOutputTokens,Enabled) VALUES(@Id,@Name,@ProviderId,@Model,@SystemPrompt,@UseProviderSystemPrompt,@Temperature,@MaxOutputTokens,@Enabled) ON DUPLICATE KEY UPDATE Name=VALUES(Name),ProviderId=VALUES(ProviderId),Model=VALUES(Model),SystemPrompt=VALUES(SystemPrompt),UseProviderSystemPrompt=VALUES(UseProviderSystemPrompt),Temperature=VALUES(Temperature),MaxOutputTokens=VALUES(MaxOutputTokens),Enabled=VALUES(Enabled);"; using(var c=new MySqlConnection(_connectionString)) using(var cmd=new MySqlCommand(sql,c)){ BindAgent(cmd,a); await c.OpenAsync(t).ConfigureAwait(false); await cmd.ExecuteNonQueryAsync(t).ConfigureAwait(false); } }
        private async Task ExecuteAsync(string sql,string id,CancellationToken t){ using(var c=new MySqlConnection(_connectionString)) using(var cmd=new MySqlCommand(sql,c)){cmd.Parameters.AddWithValue("@id",id); await c.OpenAsync(t).ConfigureAwait(false); await cmd.ExecuteNonQueryAsync(t).ConfigureAwait(false);} }
        private static void BindProvider(MySqlCommand c,AiProvider p){c.Parameters.AddWithValue("@Id",p.Id);c.Parameters.AddWithValue("@Name",p.Name);c.Parameters.AddWithValue("@Kind",p.Kind);c.Parameters.AddWithValue("@BaseUrl",p.BaseUrl);c.Parameters.AddWithValue("@DefaultModel",p.DefaultModel);c.Parameters.AddWithValue("@DefaultSystemPrompt",p.DefaultSystemPrompt);c.Parameters.AddWithValue("@SecretId",p.SecretId);c.Parameters.AddWithValue("@Enabled",p.Enabled);}
        private static void BindAgent(MySqlCommand c,AiAgent a){c.Parameters.AddWithValue("@Id",a.Id);c.Parameters.AddWithValue("@Name",a.Name);c.Parameters.AddWithValue("@ProviderId",a.ProviderId);c.Parameters.AddWithValue("@Model",a.Model);c.Parameters.AddWithValue("@SystemPrompt",a.SystemPrompt);c.Parameters.AddWithValue("@UseProviderSystemPrompt",a.UseProviderSystemPrompt);c.Parameters.AddWithValue("@Temperature",(object)a.Temperature??DBNull.Value);c.Parameters.AddWithValue("@MaxOutputTokens",(object)a.MaxOutputTokens??DBNull.Value);c.Parameters.AddWithValue("@Enabled",a.Enabled);}
    }
}
