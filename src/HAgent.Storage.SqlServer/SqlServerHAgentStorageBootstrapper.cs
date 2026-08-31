using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using Microsoft.Data.SqlClient;

namespace HAgent.Storage.SqlServer
{
    /// <summary>
    /// Provisions and upgrades HAgent's own SQL Server database. It never inspects or changes host application tables.
    /// </summary>
    public sealed class SqlServerHAgentStorageBootstrapper
    {
        public const int CurrentSchemaVersion = 2;

        public async Task EnsureCreatedAsync(
            HAgentStorageOptions options,
            string password,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.StorageType != HAgentStorageType.SqlServer)
                throw new ArgumentException("The storage options must use SQL Server.", nameof(options));

            var profile = options.GetDatabaseProfile(HAgentStorageType.SqlServer);
            if (profile == null)
                throw new ArgumentException("SQL Server storage profile is required.", nameof(options));

            var databaseName = options.GetEffectiveDatabaseName();
            var port = profile.GetEffectivePort(HAgentStorageType.SqlServer);
            var serverConnection = BuildConnectionString(profile.ServerName, port, profile.UserName, password, null);
            await EnsureDatabaseAsync(serverConnection, databaseName, cancellationToken).ConfigureAwait(false);

            var databaseConnection = BuildConnectionString(profile.ServerName, port, profile.UserName, password, databaseName);
            using (var connection = new SqlConnection(databaseConnection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var command = new SqlCommand(GetSchemaSql(), connection))
                {
                    command.CommandTimeout = 60;
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                await ApplyMigrationsAsync(connection, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Verifies that SQL Server accepts the supplied endpoint and credentials without creating a database or changing schema.
        /// </summary>
        public static async Task TestConnectionAsync(
            string serverName,
            int port,
            string userName,
            string password,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var connectionString = BuildConnectionString(serverName, port, userName, password, null);
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public static string BuildConnectionString(string serverName, string userName, string password, string databaseName)
        {
            return BuildConnectionString(serverName, 1433, userName, password, databaseName);
        }

        public static string BuildConnectionString(string serverName, int port, string userName, string password, string databaseName)
        {
            if (string.IsNullOrWhiteSpace(serverName)) throw new ArgumentException("Server name is required.", nameof(serverName));
            if (port < 1 || port > 65535) throw new ArgumentOutOfRangeException(nameof(port));
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = serverName.Trim() + "," + port,
                InitialCatalog = databaseName ?? string.Empty,
                TrustServerCertificate = true,
                Encrypt = true,
                ConnectTimeout = 15
            };

            if (string.IsNullOrWhiteSpace(userName))
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.UserID = userName;
                builder.Password = password ?? string.Empty;
            }

            return builder.ConnectionString;
        }

        private static async Task EnsureDatabaseAsync(string serverConnection, string databaseName, CancellationToken cancellationToken)
        {
            const string sql = @"
IF DB_ID(@databaseName) IS NULL
BEGIN
    DECLARE @createDatabaseSql nvarchar(776);
    SET @createDatabaseSql = N'CREATE DATABASE ' + QUOTENAME(@databaseName);
    EXEC sys.sp_executesql @createDatabaseSql;
END;";

            using (var connection = new SqlConnection(serverConnection))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@databaseName", databaseName);
                command.CommandTimeout = 60;
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static string GetSchemaSql()
        {
            var sql = new StringBuilder();
            sql.AppendLine("IF OBJECT_ID(N'dbo.HAgentSchemaInfo', N'U') IS NULL");
            sql.AppendLine("BEGIN");
            sql.AppendLine("    CREATE TABLE dbo.HAgentSchemaInfo (SchemaName nvarchar(128) NOT NULL CONSTRAINT PK_HAgentSchemaInfo PRIMARY KEY, SchemaVersion int NOT NULL, UpdatedAt datetime2(0) NOT NULL CONSTRAINT DF_HAgentSchemaInfo_UpdatedAt DEFAULT SYSUTCDATETIME());");
            sql.AppendLine("END;");
            sql.AppendLine("IF OBJECT_ID(N'dbo.HAgentProviders', N'U') IS NULL CREATE TABLE dbo.HAgentProviders (Id nvarchar(128) NOT NULL CONSTRAINT PK_HAgentProviders PRIMARY KEY, Name nvarchar(200) NOT NULL, Kind nvarchar(100) NOT NULL, BaseUrl nvarchar(1000) NOT NULL, DefaultModel nvarchar(200) NULL, DefaultSystemPrompt nvarchar(max) NULL, SecretId nvarchar(200) NULL, Enabled bit NOT NULL CONSTRAINT DF_HAgentProviders_Enabled DEFAULT(1));");
            sql.AppendLine("IF OBJECT_ID(N'dbo.HAgentAgents', N'U') IS NULL CREATE TABLE dbo.HAgentAgents (Id nvarchar(128) NOT NULL CONSTRAINT PK_HAgentAgents PRIMARY KEY, Name nvarchar(200) NOT NULL, ProviderId nvarchar(128) NULL, Model nvarchar(200) NULL, SystemPrompt nvarchar(max) NULL, UseProviderSystemPrompt bit NOT NULL CONSTRAINT DF_HAgentAgents_UseProviderPrompt DEFAULT(1), Temperature float NULL, MaxOutputTokens int NULL, Enabled bit NOT NULL CONSTRAINT DF_HAgentAgents_Enabled DEFAULT(1));");
            sql.AppendLine("IF OBJECT_ID(N'dbo.HAgentTools', N'U') IS NULL CREATE TABLE dbo.HAgentTools (Id nvarchar(128) NOT NULL CONSTRAINT PK_HAgentTools PRIMARY KEY, Name nvarchar(200) NOT NULL, Category nvarchar(100) NULL, Description nvarchar(max) NULL, InputSchemaJson nvarchar(max) NULL, Type int NOT NULL, Enabled bit NOT NULL CONSTRAINT DF_HAgentTools_Enabled DEFAULT(1), IsBuiltIn bit NOT NULL CONSTRAINT DF_HAgentTools_IsBuiltIn DEFAULT(0));");
            sql.AppendLine("IF OBJECT_ID(N'dbo.HAgentMemoryEntries', N'U') IS NULL CREATE TABLE dbo.HAgentMemoryEntries (Id nvarchar(128) NOT NULL CONSTRAINT PK_HAgentMemoryEntries PRIMARY KEY, Scope nvarchar(50) NOT NULL, Kind nvarchar(50) NOT NULL, OwnerId nvarchar(128) NOT NULL, TaskId nvarchar(128) NULL, Content nvarchar(max) NOT NULL, MetadataJson nvarchar(max) NULL, CreatedAt datetimeoffset NOT NULL, OccurredAt datetimeoffset NOT NULL);");
            sql.AppendLine("IF OBJECT_ID(N'dbo.HAgentConversations', N'U') IS NULL CREATE TABLE dbo.HAgentConversations (SessionId nvarchar(128) NOT NULL CONSTRAINT PK_HAgentConversations PRIMARY KEY, AgentId nvarchar(128) NOT NULL, CreatedAt datetimeoffset NOT NULL, UpdatedAt datetimeoffset NOT NULL, MessagesJson nvarchar(max) NOT NULL);");
            sql.AppendLine("IF OBJECT_ID(N'dbo.HAgentSkills', N'U') IS NULL CREATE TABLE dbo.HAgentSkills (Id nvarchar(128) NOT NULL CONSTRAINT PK_HAgentSkills PRIMARY KEY, Name nvarchar(200) NOT NULL, Description nvarchar(max) NULL, DefinitionJson nvarchar(max) NULL, Enabled bit NOT NULL CONSTRAINT DF_HAgentSkills_Enabled DEFAULT(1));");
            sql.AppendLine("IF OBJECT_ID(N'dbo.HAgentWikiDocuments', N'U') IS NULL CREATE TABLE dbo.HAgentWikiDocuments (Id nvarchar(128) NOT NULL CONSTRAINT PK_HAgentWikiDocuments PRIMARY KEY, Title nvarchar(500) NOT NULL, Content nvarchar(max) NOT NULL, Source nvarchar(1000) NULL, Version nvarchar(64) NULL, CreatedAt datetimeoffset NOT NULL, UpdatedAt datetimeoffset NOT NULL);");
            sql.AppendLine("IF OBJECT_ID(N'dbo.HAgentWikiChunks', N'U') IS NULL CREATE TABLE dbo.HAgentWikiChunks (Id nvarchar(128) NOT NULL CONSTRAINT PK_HAgentWikiChunks PRIMARY KEY, DocumentId nvarchar(128) NOT NULL, ChunkIndex int NOT NULL, Content nvarchar(max) NOT NULL, MetadataJson nvarchar(max) NULL, CONSTRAINT FK_HAgentWikiChunks_Document FOREIGN KEY (DocumentId) REFERENCES dbo.HAgentWikiDocuments(Id));");
            sql.AppendLine("IF NOT EXISTS (SELECT 1 FROM dbo.HAgentSchemaInfo WHERE SchemaName=N'core') INSERT INTO dbo.HAgentSchemaInfo (SchemaName, SchemaVersion) VALUES (N'core', 1);");
            return sql.ToString();
        }

        private static async Task ApplyMigrationsAsync(SqlConnection connection, CancellationToken cancellationToken)
        {
            var version = await GetSchemaVersionAsync(connection, cancellationToken).ConfigureAwait(false);
            if (version > CurrentSchemaVersion)
                throw new InvalidOperationException("Unsupported HAgent SQL Server schema version: " + version + ".");

            while (version < CurrentSchemaVersion)
            {
                switch (version)
                {
                    case 1:
                        await MigrateV1ToV2Async(connection, cancellationToken).ConfigureAwait(false);
                        version = 2;
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported HAgent SQL Server schema version: " + version + ".");
                }

                await SetSchemaVersionAsync(connection, version, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<int> GetSchemaVersionAsync(SqlConnection connection, CancellationToken cancellationToken)
        {
            const string sql = "SELECT SchemaVersion FROM dbo.HAgentSchemaInfo WHERE SchemaName=N'core';";
            using (var command = new SqlCommand(sql, connection))
            {
                var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (value == null || value == DBNull.Value)
                    throw new InvalidOperationException("The HAgent SQL Server schema version record is missing.");
                return Convert.ToInt32(value);
            }
        }

        private static async Task SetSchemaVersionAsync(SqlConnection connection, int version, CancellationToken cancellationToken)
        {
            const string sql = "UPDATE dbo.HAgentSchemaInfo SET SchemaVersion=@Version, UpdatedAt=SYSUTCDATETIME() WHERE SchemaName=N'core';";
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Version", version);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task MigrateV1ToV2Async(SqlConnection connection, CancellationToken cancellationToken)
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_HAgentMemoryEntries_OwnerScopeOccurred' AND object_id=OBJECT_ID(N'dbo.HAgentMemoryEntries'))
BEGIN
    CREATE INDEX IX_HAgentMemoryEntries_OwnerScopeOccurred ON dbo.HAgentMemoryEntries(OwnerId, Scope, OccurredAt DESC, CreatedAt DESC);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_HAgentMemoryEntries_TaskOccurred' AND object_id=OBJECT_ID(N'dbo.HAgentMemoryEntries'))
BEGIN
    CREATE INDEX IX_HAgentMemoryEntries_TaskOccurred ON dbo.HAgentMemoryEntries(TaskId, OccurredAt DESC, CreatedAt DESC);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_HAgentConversations_UpdatedAt' AND object_id=OBJECT_ID(N'dbo.HAgentConversations'))
BEGIN
    CREATE INDEX IX_HAgentConversations_UpdatedAt ON dbo.HAgentConversations(UpdatedAt DESC);
END;";

            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = 60;
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
