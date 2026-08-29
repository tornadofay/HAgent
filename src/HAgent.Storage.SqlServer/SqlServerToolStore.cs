using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using Microsoft.Data.SqlClient;

namespace HAgent.Storage.SqlServer
{
    public sealed class SqlServerToolStore : IToolStore
    {
        private readonly string _connectionString;

        public SqlServerToolStore(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public static async Task EnsureSchemaAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.HAgentTools', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HAgentTools (
        Id nvarchar(64) NOT NULL CONSTRAINT PK_HAgentTools PRIMARY KEY,
        Name nvarchar(200) NOT NULL,
        Description nvarchar(max) NULL,
        InputSchemaJson nvarchar(max) NOT NULL,
        Category nvarchar(100) NULL,
        Type int NOT NULL,
        IsBuiltIn bit NOT NULL CONSTRAINT DF_HAgentTools_IsBuiltIn DEFAULT(0),
        Enabled bit NOT NULL CONSTRAINT DF_HAgentTools_Enabled DEFAULT(1)
    );
END;";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyList<AiTool>> GetToolsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<AiTool>();
            const string sql = "SELECT Id, Name, Description, InputSchemaJson, Category, Type, IsBuiltIn, Enabled FROM dbo.HAgentTools ORDER BY Name";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        list.Add(new AiTool
                        {
                            Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            InputSchemaJson = reader.GetString(3),
                            Category = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Type = (AiToolType)reader.GetInt32(5),
                            IsBuiltIn = reader.GetBoolean(6),
                            Enabled = reader.GetBoolean(7)
                        });
                    }
                }
            }
            return list.AsReadOnly();
        }

        public async Task SaveToolAsync(AiTool tool, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tool == null) throw new ArgumentNullException(nameof(tool));
            if (string.IsNullOrWhiteSpace(tool.Id)) throw new ArgumentException("Tool ID is required.", nameof(tool));

            const string sql = @"MERGE dbo.HAgentTools AS target
USING (SELECT @Id Id) AS source ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET Name=@Name, Description=@Description, InputSchemaJson=@InputSchemaJson, Category=@Category, Type=@Type, IsBuiltIn=@IsBuiltIn, Enabled=@Enabled
WHEN NOT MATCHED THEN INSERT (Id, Name, Description, InputSchemaJson, Category, Type, IsBuiltIn, Enabled)
VALUES (@Id, @Name, @Description, @InputSchemaJson, @Category, @Type, @IsBuiltIn, @Enabled);";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                Bind(command, tool);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteToolAsync(string toolId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(toolId)) return;
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("DELETE FROM dbo.HAgentTools WHERE Id=@id", connection))
            {
                command.Parameters.AddWithValue("@id", toolId);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static void Bind(SqlCommand command, AiTool tool)
        {
            command.Parameters.AddWithValue("@Id", tool.Id);
            command.Parameters.AddWithValue("@Name", tool.Name);
            command.Parameters.AddWithValue("@Description", (object)tool.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@InputSchemaJson", tool.InputSchemaJson);
            command.Parameters.AddWithValue("@Category", (object)tool.Category ?? DBNull.Value);
            command.Parameters.AddWithValue("@Type", (int)tool.Type);
            command.Parameters.AddWithValue("@IsBuiltIn", tool.IsBuiltIn);
            command.Parameters.AddWithValue("@Enabled", tool.Enabled);
        }
    }
}
