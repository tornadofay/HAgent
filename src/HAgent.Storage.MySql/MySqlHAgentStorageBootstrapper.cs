using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using MySqlConnector;

namespace HAgent.Storage.MySql
{
    /// <summary>
    /// Provisions and upgrades HAgent's own MySQL database. It never inspects or changes host application tables.
    /// </summary>
    public sealed class MySqlHAgentStorageBootstrapper
    {
        public const int CurrentSchemaVersion = 2;

        public async Task EnsureCreatedAsync(
            HAgentStorageOptions options,
            string password,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.StorageType != HAgentStorageType.MySql)
                throw new ArgumentException("The storage options must use MySQL.", nameof(options));

            var profile = options.GetDatabaseProfile(HAgentStorageType.MySql);
            if (profile == null)
                throw new ArgumentException("MySQL storage profile is required.", nameof(options));

            var databaseName = options.GetEffectiveDatabaseName();
            var port = profile.GetEffectivePort(HAgentStorageType.MySql);
            var serverConnection = BuildConnectionString(profile.ServerName, port, profile.UserName, password, null);
            await EnsureDatabaseAsync(serverConnection, databaseName, cancellationToken).ConfigureAwait(false);

            var databaseConnection = BuildConnectionString(profile.ServerName, port, profile.UserName, password, databaseName);
            using (var connection = new MySqlConnection(databaseConnection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (var command = new MySqlCommand(GetSchemaSql(), connection))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                await MigrateToolTypeColumnAsync(connection, cancellationToken).ConfigureAwait(false);
            }
        }

        public static string BuildConnectionString(string serverName, string userName, string password, string databaseName)
        {
            return BuildConnectionString(serverName, 3306, userName, password, databaseName);
        }

        public static string BuildConnectionString(string serverName, int port, string userName, string password, string databaseName)
        {
            if (string.IsNullOrWhiteSpace(serverName)) throw new ArgumentException("Server name is required.", nameof(serverName));
            if (port < 1 || port > 65535) throw new ArgumentOutOfRangeException(nameof(port));
            var builder = new MySqlConnectionStringBuilder
            {
                Server = serverName.Trim(),
                Port = (uint)port,
                Database = databaseName ?? string.Empty,
                UserID = userName ?? string.Empty,
                Password = password ?? string.Empty,
                ConnectionTimeout = 15,
                DefaultCommandTimeout = 60
            };
            return builder.ConnectionString;
        }

        private static async Task EnsureDatabaseAsync(string serverConnection, string databaseName, CancellationToken cancellationToken)
        {
            using (var connection = new MySqlConnection(serverConnection))
            using (var command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS `" + EscapeIdentifier(databaseName) + "`;", connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task MigrateToolTypeColumnAsync(MySqlConnection connection, CancellationToken cancellationToken)
        {
            const string columnSql = @"
SELECT
    SUM(CASE WHEN COLUMN_NAME = 'Type' THEN 1 ELSE 0 END),
    SUM(CASE WHEN COLUMN_NAME = 'ToolType' THEN 1 ELSE 0 END)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'HAgentTools';";

            int hasType;
            int hasLegacyType;
            using (var command = new MySqlCommand(columnSql, connection))
            using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
            {
                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) return;
                hasType = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                hasLegacyType = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            }

            if (hasType > 0 || hasLegacyType == 0)
                return;

            const string migrateSql = @"
ALTER TABLE HAgentTools ADD COLUMN Type int NOT NULL DEFAULT 1;
UPDATE HAgentTools
SET Type = CASE LOWER(COALESCE(ToolType, ''))
    WHEN 'built-in' THEN 0
    WHEN 'builtin' THEN 0
    WHEN 'application' THEN 1
    WHEN 'declarative' THEN 2
    WHEN 'ui' THEN 3
    WHEN 'sqlserver' THEN 4
    WHEN 'mysql' THEN 5
    WHEN 'extension' THEN 6
    ELSE 1
END;
ALTER TABLE HAgentTools DROP COLUMN ToolType;";

            using (var command = new MySqlCommand(migrateSql, connection))
            {
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static string GetSchemaSql()
        {
            var sql = new StringBuilder();
            sql.AppendLine("CREATE TABLE IF NOT EXISTS HAgentSchemaInfo (SchemaName varchar(128) NOT NULL, SchemaVersion int NOT NULL, UpdatedAt datetime(0) NOT NULL, PRIMARY KEY (SchemaName));");
            sql.AppendLine("CREATE TABLE IF NOT EXISTS HAgentProviders (Id varchar(128) NOT NULL, Name varchar(200) NOT NULL, Kind varchar(100) NOT NULL, BaseUrl varchar(1000) NOT NULL, DefaultModel varchar(200) NULL, DefaultSystemPrompt longtext NULL, SecretId varchar(200) NULL, Enabled boolean NOT NULL DEFAULT TRUE, PRIMARY KEY (Id));");
            sql.AppendLine("CREATE TABLE IF NOT EXISTS HAgentAgents (Id varchar(128) NOT NULL, Name varchar(200) NOT NULL, ProviderId varchar(128) NULL, Model varchar(200) NULL, SystemPrompt longtext NULL, UseProviderSystemPrompt boolean NOT NULL DEFAULT TRUE, Temperature double NULL, MaxOutputTokens int NULL, Enabled boolean NOT NULL DEFAULT TRUE, PRIMARY KEY (Id));");
            sql.AppendLine("CREATE TABLE IF NOT EXISTS HAgentTools (Id varchar(128) NOT NULL, Name varchar(200) NOT NULL, Category varchar(100) NULL, Description longtext NULL, InputSchemaJson longtext NULL, Type int NOT NULL DEFAULT 1, Enabled boolean NOT NULL DEFAULT TRUE, IsBuiltIn boolean NOT NULL DEFAULT FALSE, PRIMARY KEY (Id));");
            sql.AppendLine("CREATE TABLE IF NOT EXISTS HAgentMemoryEntries (Id varchar(128) NOT NULL, Scope varchar(50) NOT NULL, Kind varchar(50) NOT NULL, OwnerId varchar(128) NOT NULL, TaskId varchar(128) NULL, Content longtext NOT NULL, MetadataJson longtext NULL, CreatedAt datetime(6) NOT NULL, OccurredAt datetime(6) NOT NULL, PRIMARY KEY (Id));");
            sql.AppendLine("CREATE TABLE IF NOT EXISTS HAgentConversations (SessionId varchar(128) NOT NULL, AgentId varchar(128) NOT NULL, CreatedAt datetime(6) NOT NULL, UpdatedAt datetime(6) NOT NULL, MessagesJson longtext NOT NULL, PRIMARY KEY (SessionId));");
            sql.AppendLine("CREATE TABLE IF NOT EXISTS HAgentSkills (Id varchar(128) NOT NULL, Name varchar(200) NOT NULL, Description longtext NULL, DefinitionJson longtext NULL, Enabled boolean NOT NULL DEFAULT TRUE, PRIMARY KEY (Id));");
            sql.AppendLine("CREATE TABLE IF NOT EXISTS HAgentWikiDocuments (Id varchar(128) NOT NULL, Title varchar(500) NOT NULL, Content longtext NOT NULL, Source varchar(1000) NULL, Version varchar(64) NULL, CreatedAt datetime(6) NOT NULL, UpdatedAt datetime(6) NOT NULL, PRIMARY KEY (Id));");
            sql.AppendLine("CREATE TABLE IF NOT EXISTS HAgentWikiChunks (Id varchar(128) NOT NULL, DocumentId varchar(128) NOT NULL, ChunkIndex int NOT NULL, Content longtext NOT NULL, MetadataJson longtext NULL, PRIMARY KEY (Id), CONSTRAINT FK_HAgentWikiChunks_Document FOREIGN KEY (DocumentId) REFERENCES HAgentWikiDocuments(Id));");
            sql.AppendLine("INSERT INTO HAgentSchemaInfo (SchemaName, SchemaVersion, UpdatedAt) VALUES ('core', " + CurrentSchemaVersion + ", UTC_TIMESTAMP()) ON DUPLICATE KEY UPDATE SchemaVersion=VALUES(SchemaVersion), UpdatedAt=VALUES(UpdatedAt);");
            return sql.ToString();
        }

        private static string EscapeIdentifier(string value)
        {
            return (value ?? string.Empty).Replace("`", "``");
        }
    }
}
