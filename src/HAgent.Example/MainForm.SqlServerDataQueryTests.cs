using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Storage.SqlServer;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestSqlServerDataQueryAsync(string input)
        {
            var fields = ReadConnectionFields(input);
            var connectionString = BuildConnectionString(fields.ServerName, fields.UserName, fields.Password, fields.Database);
            var schema = new DataQuerySchema(new[] { "Id", "Name", "Amount" });
            var executionPolicy = new DataQueryExecutionPolicy
            {
                MaximumTake = 20,
                MaximumSkip = 1000,
                MaximumFilters = 8,
                MaximumSorts = 4,
                MaximumResultRows = 20,
                Timeout = TimeSpan.FromSeconds(15)
            };
            var permissions = new DataAccessPermissions
            {
                ProjectionQuery = true
            };
            var authorizer = new ExampleDataAccessAuthorizer();
            var source = new SqlServerDataQuerySource(new SqlServerDataQuerySourceOptions
            {
                ConnectionString = connectionString,
                SourceId = "sqlserver-example",
                RuntimeIdentity = "example-agent",
                SchemaName = "dbo",
                TableName = "HAgentExampleCustomers",
                Schema = schema,
                Permissions = permissions,
                Authorizer = authorizer,
                ExecutionPolicy = executionPolicy
            });

            var request = new DataQueryRequest
            {
                Fields = new[] { "Id", "Name", "Amount" },
                Filters = new[]
                {
                    new DataFilterCondition { Field = "Amount", Operator = DataQueryOperator.GreaterThanOrEqual, Value = 60 },
                    new DataFilterCondition { Field = "Name", Operator = DataQueryOperator.NotEqual, Value = "Eve" }
                },
                Sorts = new[]
                {
                    new DataSort { Field = "Amount", Descending = true }
                },
                Skip = 0,
                Take = 2
            };

            var result = await source.QueryAsync(request, CancellationToken.None);
            if (result.Returned != 2 || !result.HasMore)
                throw new InvalidOperationException("SQL Server structured query did not return the expected bounded first page.");
            if (!string.Equals(Convert.ToString(result.Rows[0]["Name"]), "David", StringComparison.Ordinal) ||
                !string.Equals(Convert.ToString(result.Rows[1]["Name"]), "Alice", StringComparison.Ordinal))
                throw new InvalidOperationException("SQL Server structured query did not apply the expected filter and sort semantics.");
            if (result.Rows[0].Count != 3 || result.Rows[0].ContainsKey("Secret"))
                throw new InvalidOperationException("SQL Server structured query returned a field outside the explicit projection.");

            var unauthorizedFieldRequest = new DataQueryRequest
            {
                Fields = new[] { "Id", "Secret" },
                Take = 1
            };
            try
            {
                await source.QueryAsync(unauthorizedFieldRequest, CancellationToken.None);
                throw new InvalidOperationException("SQL Server source accepted a field outside its authoritative schema.");
            }
            catch (ArgumentException)
            {
            }

            var deniedSource = new SqlServerDataQuerySource(new SqlServerDataQuerySourceOptions
            {
                ConnectionString = connectionString,
                SourceId = "sqlserver-example",
                RuntimeIdentity = "example-agent",
                SchemaName = "dbo",
                TableName = "HAgentExampleCustomers",
                Schema = schema,
                Permissions = permissions,
                Authorizer = new ExampleDataAccessAuthorizer(false),
                ExecutionPolicy = executionPolicy
            });
            try
            {
                await deniedSource.QueryAsync(request, CancellationToken.None);
                throw new InvalidOperationException("SQL Server source executed a query after host authorization denied it.");
            }
            catch (UnauthorizedAccessException)
            {
            }

            var injectionValue = "David' OR 1=1 --";
            var injectionRequest = new DataQueryRequest
            {
                Fields = new[] { "Id" },
                Filters = new[]
                {
                    new DataFilterCondition { Field = "Name", Operator = DataQueryOperator.Equal, Value = injectionValue }
                },
                Take = 2
            };
            var injectionResult = await source.QueryAsync(injectionRequest, CancellationToken.None);
            if (injectionResult.Returned != 0)
                throw new InvalidOperationException("SQL Server adapter did not preserve a filter value as data.");

            Write("SQL SERVER DATA QUERY",
                "Structured SQL Server read succeeded." + Environment.NewLine +
                "Table: dbo.HAgentExampleCustomers" + Environment.NewLine +
                "Fields: Id, Name, Amount" + Environment.NewLine +
                "Filters: Amount >= 60; Name != Eve" + Environment.NewLine +
                "Sort: Amount descending" + Environment.NewLine +
                "Page: 0 / 2" + Environment.NewLine +
                "Returned: " + result.Returned + Environment.NewLine +
                "Has more: " + result.HasMore + Environment.NewLine +
                "First rows: " + Convert.ToString(result.Rows[0]["Name"]) + ", " + Convert.ToString(result.Rows[1]["Name"]) + Environment.NewLine +
                "Authoritative schema rejected the non-approved Secret field." + Environment.NewLine +
                "Host authorization denial blocked execution." + Environment.NewLine +
                "Injection-shaped filter text remained a parameter value." + Environment.NewLine +
                "Connection values were runtime-only and were not persisted or logged.");
        }

        private static ConnectionFields ReadConnectionFields(string input)
        {
            using (var reader = new StringReader(input ?? string.Empty))
            {
                var serverName = reader.ReadLine();
                var userName = reader.ReadLine();
                var password = reader.ReadLine();
                var database = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(serverName) || string.IsNullOrWhiteSpace(database))
                    throw new ArgumentException("Enter Server Name on line 1 and Database on line 4.");

                return new ConnectionFields(serverName, userName ?? string.Empty, password ?? string.Empty, database);
            }
        }

        private static string BuildConnectionString(string serverName, string userName, string password, string database)
        {
            var builder = new StringBuilder();
            builder.Append("Server=");
            builder.Append(QuoteConnectionValue(serverName));
            builder.Append(";Database=");
            builder.Append(QuoteConnectionValue(database));
            if (string.IsNullOrWhiteSpace(userName))
            {
                builder.Append(";Integrated Security=True");
            }
            else
            {
                builder.Append(";User Id=");
                builder.Append(QuoteConnectionValue(userName));
                builder.Append(";Password=");
                builder.Append(QuoteConnectionValue(password));
            }

            builder.Append(";Encrypt=True;TrustServerCertificate=True");
            return builder.ToString();
        }

        private static string QuoteConnectionValue(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private sealed class ConnectionFields
        {
            public ConnectionFields(string serverName, string userName, string password, string database)
            {
                ServerName = serverName;
                UserName = userName;
                Password = password;
                Database = database;
            }

            public string ServerName { get; private set; }
            public string UserName { get; private set; }
            public string Password { get; private set; }
            public string Database { get; private set; }
        }
    }
}
