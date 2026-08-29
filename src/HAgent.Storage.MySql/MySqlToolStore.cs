using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using MySqlConnector;

namespace HAgent.Storage.MySql
{
    public sealed class MySqlToolStore : IToolStore
    {
        private readonly string _connectionString;

        public MySqlToolStore(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public static async Task EnsureSchemaAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS HAgentTools (
 Id varchar(64) NOT NULL PRIMARY KEY,
 Name varchar(200) NOT NULL,
 Description longtext NULL,
 InputSchemaJson longtext NOT NULL,
 Category varchar(100) NULL,
 Type int NOT NULL,
 IsBuiltIn bit NOT NULL DEFAULT 0,
 Enabled bit NOT NULL DEFAULT 1
) ENGINE=InnoDB;";

            using (var connection = new MySqlConnection(connectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyList<AiTool>> GetToolsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<AiTool>();
            const string sql = "SELECT Id, Name, Description, InputSchemaJson, Category, Type, IsBuiltIn, Enabled FROM HAgentTools ORDER BY Name";
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand(sql, connection))
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

            const string sql = @"INSERT INTO HAgentTools(Id,Name,Description,InputSchemaJson,Category,Type,IsBuiltIn,Enabled)
VALUES(@Id,@Name,@Description,@InputSchemaJson,@Category,@Type,@IsBuiltIn,@Enabled)
ON DUPLICATE KEY UPDATE Name=VALUES(Name), Description=VALUES(Description), InputSchemaJson=VALUES(InputSchemaJson), Category=VALUES(Category), Type=VALUES(Type), IsBuiltIn=VALUES(IsBuiltIn), Enabled=VALUES(Enabled);";

            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                Bind(command, tool);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteToolAsync(string toolId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(toolId)) return;
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand("DELETE FROM HAgentTools WHERE Id=@id", connection))
            {
                command.Parameters.AddWithValue("@id", toolId);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static void Bind(MySqlCommand command, AiTool tool)
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
