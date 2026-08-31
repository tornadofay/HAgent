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
        public const int CurrentSchemaVersion = 3;

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
                await EnsureBaseSchemaAsync(connection, cancellationToken).ConfigureAwait(false);
                await ApplyMigrationsAsync(connection, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Verifies that MySQL/MariaDB accepts the supplied endpoint and credentials without creating a database or changing schema.
        /// </summary>
        public static async Task TestConnectionAsync(
            string serverName,
            int port,
            string userName,
            string password,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var connectionString = BuildConnectionString(serverName, port, userName, password, null);
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
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

        private static async Task EnsureBaseSchemaAsync(MySqlConnection connection, CancellationToken cancellationToken)
        {
            var statements = new[]
            {
                "CREATE TABLE IF NOT EXISTS HAgentSchemaInfo (SchemaName varchar(128) NOT NULL, SchemaVersion int NOT NULL, UpdatedAt datetime(0) NOT NULL, PRIMARY KEY (SchemaName));",
                "CREATE TABLE IF NOT EXISTS HAgentProviders (Id varchar(128) NOT NULL, Name varchar(200) NOT NULL, Kind varchar(100) NOT NULL, BaseUrl varchar(1000) NOT NULL, DefaultModel varchar(200) NULL, DefaultSystemPrompt longtext NULL, SecretId varchar(200) NULL, Enabled boolean NOT NULL DEFAULT TRUE, PRIMARY KEY (Id));",
                "CREATE TABLE IF NOT EXISTS HAgentAgents (Id varchar(128) NOT NULL, Name varchar(200) NOT NULL, ProviderId varchar(128) NULL, Model varchar(200) NULL, SystemPrompt longtext NULL, UseProviderSystemPrompt boolean NOT NULL DEFAULT TRUE, Temperature double NULL, MaxOutputTokens int NULL, Enabled boolean NOT NULL DEFAULT TRUE, PRIMARY KEY (Id));",
                "CREATE TABLE IF NOT EXISTS HAgentTools (Id varchar(128) NOT NULL, Name varchar(200) NOT NULL, Category varchar(100) NULL, Description longtext NULL, InputSchemaJson longtext NULL, Type int NOT NULL DEFAULT 1, Enabled boolean NOT NULL DEFAULT TRUE, IsBuiltIn boolean NOT NULL DEFAULT FALSE, PRIMARY KEY (Id));",
                "CREATE TABLE IF NOT EXISTS HAgentMemoryEntries (Id varchar(128) NOT NULL, Scope varchar(50) NOT NULL, Kind varchar(50) NOT NULL, OwnerId varchar(128) NOT NULL, TaskId varchar(128) NULL, Content longtext NOT NULL, MetadataJson longtext NULL, CreatedAt datetime(6) NOT NULL, OccurredAt datetime(6) NOT NULL, PRIMARY KEY (Id));",
                "CREATE TABLE IF NOT EXISTS HAgentConversations (SessionId varchar(128) NOT NULL, AgentId varchar(128) NOT NULL, CreatedAt datetime(6) NOT NULL, UpdatedAt datetime(6) NOT NULL, MessagesJson longtext NOT NULL, PRIMARY KEY (SessionId));",
                "CREATE TABLE IF NOT EXISTS HAgentSkills (Id varchar(128) NOT NULL, Name varchar(200) NOT NULL, Description longtext NULL, DefinitionJson longtext NULL, Enabled boolean NOT NULL DEFAULT TRUE, PRIMARY KEY (Id));",
                "CREATE TABLE IF NOT EXISTS HAgentWikiDocuments (Id varchar(128) NOT NULL, Title varchar(500) NOT NULL, Content longtext NOT NULL, Source varchar(1000) NULL, Version varchar(64) NULL, CreatedAt datetime(6) NOT NULL, UpdatedAt datetime(6) NOT NULL, PRIMARY KEY (Id));",
                "CREATE TABLE IF NOT EXISTS HAgentWikiChunks (Id varchar(128) NOT NULL, DocumentId varchar(128) NOT NULL, ChunkIndex int NOT NULL, Content longtext NOT NULL, MetadataJson longtext NULL, PRIMARY KEY (Id), CONSTRAINT FK_HAgentWikiChunks_Document FOREIGN KEY (DocumentId) REFERENCES HAgentWikiDocuments(Id));"
            };

            foreach (var sql in statements)
                await ExecuteNonQueryAsync(connection, sql, cancellationToken).ConfigureAwait(false);

            const string schemaVersionSql = @"
INSERT INTO HAgentSchemaInfo (SchemaName, SchemaVersion, UpdatedAt)
SELECT 'core', 1, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM HAgentSchemaInfo WHERE SchemaName = 'core');";

            await ExecuteNonQueryAsync(connection, schemaVersionSql, cancellationToken).ConfigureAwait(false);
        }

        private static async Task ApplyMigrationsAsync(MySqlConnection connection, CancellationToken cancellationToken)
        {
            var version = await GetSchemaVersionAsync(connection, cancellationToken).ConfigureAwait(false);
            if (version > CurrentSchemaVersion)
                throw new InvalidOperationException("Unsupported HAgent MySQL schema version: " + version + ".");

            while (version < CurrentSchemaVersion)
            {
                switch (version)
                {
                    case 1:
                        await MigrateV1ToV2Async(connection, cancellationToken).ConfigureAwait(false);
                        version = 2;
                        break;
                    case 2:
                        await MigrateV2ToV3Async(connection, cancellationToken).ConfigureAwait(false);
                        version = 3;
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported HAgent MySQL schema version: " + version + ".");
                }

                await SetSchemaVersionAsync(connection, version, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<int> GetSchemaVersionAsync(MySqlConnection connection, CancellationToken cancellationToken)
        {
            const string sql = "SELECT SchemaVersion FROM HAgentSchemaInfo WHERE SchemaName='core';";
            using (var command = new MySqlCommand(sql, connection))
            {
                var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (value == null || value == DBNull.Value)
                    throw new InvalidOperationException("The HAgent MySQL schema version record is missing.");
                return Convert.ToInt32(value);
            }
        }

        private static async Task SetSchemaVersionAsync(MySqlConnection connection, int version, CancellationToken cancellationToken)
        {
            const string sql = "UPDATE HAgentSchemaInfo SET SchemaVersion=@Version, UpdatedAt=UTC_TIMESTAMP() WHERE SchemaName='core';";
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Version", version);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task MigrateV1ToV2Async(MySqlConnection connection, CancellationToken cancellationToken)
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
                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    return;
                hasType = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                hasLegacyType = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
            }

            if (hasType > 0 || hasLegacyType == 0)
                return;

            await ExecuteNonQueryAsync(connection,
                "ALTER TABLE HAgentTools ADD COLUMN Type int NOT NULL DEFAULT 1;",
                cancellationToken).ConfigureAwait(false);

            await ExecuteNonQueryAsync(connection, @"
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
END;", cancellationToken).ConfigureAwait(false);

            await ExecuteNonQueryAsync(connection,
                "ALTER TABLE HAgentTools DROP COLUMN ToolType;",
                cancellationToken).ConfigureAwait(false);
        }

        private static async Task MigrateV2ToV3Async(MySqlConnection connection, CancellationToken cancellationToken)
        {
            await CreateIndexIfMissingAsync(connection, "HAgentMemoryEntries", "IX_HAgentMemoryEntries_OwnerScopeOccurred", "CREATE INDEX IX_HAgentMemoryEntries_OwnerScopeOccurred ON HAgentMemoryEntries(OwnerId, Scope, OccurredAt, CreatedAt);", cancellationToken).ConfigureAwait(false);
            await CreateIndexIfMissingAsync(connection, "HAgentMemoryEntries", "IX_HAgentMemoryEntries_TaskOccurred", "CREATE INDEX IX_HAgentMemoryEntries_TaskOccurred ON HAgentMemoryEntries(TaskId, OccurredAt, CreatedAt);", cancellationToken).ConfigureAwait(false);
            await CreateIndexIfMissingAsync(connection, "HAgentConversations", "IX_HAgentConversations_UpdatedAt", "CREATE INDEX IX_HAgentConversations_UpdatedAt ON HAgentConversations(UpdatedAt);", cancellationToken).ConfigureAwait(false);
        }

        private static async Task CreateIndexIfMissingAsync(MySqlConnection connection, string tableName, string indexName, string createSql, CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @TableName
  AND INDEX_NAME = @IndexName;";

            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@TableName", tableName);
                command.Parameters.AddWithValue("@IndexName", indexName);
                var exists = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)) > 0;
                if (exists) return;
            }

            await ExecuteNonQueryAsync(connection, createSql, cancellationToken).ConfigureAwait(false);
        }

        private static async Task ExecuteNonQueryAsync(MySqlConnection connection, string sql, CancellationToken cancellationToken)
        {
            using (var command = new MySqlCommand(sql, connection))
            {
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static string EscapeIdentifier(string value)
        {
            return (value ?? string.Empty).Replace("`", "``");
        }
    }
}
