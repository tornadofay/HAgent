using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using Microsoft.Data.SqlClient;

namespace HAgent.Storage.SqlServer
{
    /// <summary>
    /// Read-only SQL Server adapter for the provider-neutral structured data-query contract.
    /// SQL is generated only from validated schema identifiers and structured operators; values are always parameters.
    /// </summary>
    public sealed class SqlServerDataQuerySource : IDataQuerySource
    {
        private readonly SqlServerDataQuerySourceOptions _options;

        public SqlServerDataQuerySource(SqlServerDataQuerySourceOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.Validate();
        }

        public async Task<DataQueryResult> QueryAsync(DataQueryRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            _options.Permissions.DemandProjectionQuery();
            _options.ExecutionPolicy.ValidateRequest(request);
            _options.Schema.ValidateRequest(
                request,
                _options.ExecutionPolicy.MaximumTake,
                _options.ExecutionPolicy.MaximumSkip,
                _options.ExecutionPolicy.MaximumFilters,
                _options.ExecutionPolicy.MaximumSorts);

            using (var executionCancellation = _options.ExecutionPolicy.CreateCancellationSource(cancellationToken))
            {
                var authorization = await _options.Authorizer.AuthorizeAsync(new DataAuthorizationRequest
                {
                    Operation = DataAccessOperation.ProjectionQuery,
                    SourceId = _options.SourceId,
                    RuntimeIdentity = _options.RuntimeIdentity,
                    Query = request
                }, executionCancellation.Token).ConfigureAwait(false);

                if (!authorization)
                    throw new UnauthorizedAccessException("The host authorization callback denied the SQL Server structured data projection/query operation.");

                executionCancellation.Token.ThrowIfCancellationRequested();

                var commandSpec = BuildCommand(request);
                using (var connection = new SqlConnection(_options.ConnectionString))
                using (var command = new SqlCommand(commandSpec.CommandText, connection))
                {
                    command.CommandTimeout = GetCommandTimeoutSeconds(_options.ExecutionPolicy.Timeout);
                    foreach (var parameter in commandSpec.Parameters)
                        command.Parameters.Add(parameter);

                    await connection.OpenAsync(executionCancellation.Token).ConfigureAwait(false);
                    using (var reader = await command.ExecuteReaderAsync(executionCancellation.Token).ConfigureAwait(false))
                    {
                        var rows = new List<IReadOnlyDictionary<string, object>>(request.Take);
                        while (await reader.ReadAsync(executionCancellation.Token).ConfigureAwait(false))
                        {
                            if (rows.Count == _options.ExecutionPolicy.MaximumResultRows + 1)
                                throw new InvalidOperationException("The SQL Server source returned more rows than the configured result budget.");

                            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            for (var i = 0; i < reader.FieldCount; i++)
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            rows.Add(row);
                        }

                        var hasMore = rows.Count > request.Take;
                        if (hasMore)
                            rows.RemoveAt(rows.Count - 1);

                        return new DataQueryResult
                        {
                            Rows = rows.AsReadOnly(),
                            Skipped = request.Skip,
                            Returned = rows.Count,
                            HasMore = hasMore
                        };
                    }
                }
            }
        }

        private CommandSpec BuildCommand(DataQueryRequest request)
        {
            var sql = new StringBuilder();
            var parameters = new List<SqlParameter>();

            sql.Append("SELECT ");
            sql.Append(string.Join(", ", request.Fields.Select(SqlServerDataQuerySourceOptions.QuoteIdentifier)));
            sql.Append(" FROM ");
            sql.Append(SqlServerDataQuerySourceOptions.QuoteIdentifier(_options.SchemaName));
            sql.Append(".");
            sql.Append(SqlServerDataQuerySourceOptions.QuoteIdentifier(_options.TableName));

            if (request.Filters.Count > 0)
            {
                sql.Append(" WHERE ");
                var clauses = new List<string>(request.Filters.Count);
                for (var i = 0; i < request.Filters.Count; i++)
                {
                    var filter = request.Filters[i];
                    clauses.Add(BuildFilter(filter, i, parameters));
                }
                sql.Append(string.Join(" AND ", clauses));
            }

            sql.Append(" ORDER BY ");
            if (request.Sorts.Count == 0)
            {
                sql.Append(SqlServerDataQuerySourceOptions.QuoteIdentifier(request.Fields[0]));
            }
            else
            {
                var sorts = request.Sorts.Select(sort =>
                    SqlServerDataQuerySourceOptions.QuoteIdentifier(sort.Field) + (sort.Descending ? " DESC" : " ASC"));
                sql.Append(string.Join(", ", sorts));
            }

            sql.Append(" OFFSET @__skip ROWS FETCH NEXT @__take ROWS ONLY;");
            parameters.Add(new SqlParameter("@__skip", request.Skip));
            parameters.Add(new SqlParameter("@__take", request.Take + 1));

            return new CommandSpec(sql.ToString(), parameters);
        }

        private static string BuildFilter(DataFilterCondition filter, int index, ICollection<SqlParameter> parameters)
        {
            var field = SqlServerDataQuerySourceOptions.QuoteIdentifier(filter.Field);
            switch (filter.Operator)
            {
                case DataQueryOperator.Equal:
                    return AddParameterComparison(field, "=", filter.Value, index, parameters);
                case DataQueryOperator.NotEqual:
                    return AddParameterComparison(field, "<>", filter.Value, index, parameters);
                case DataQueryOperator.GreaterThan:
                    return AddParameterComparison(field, ">", filter.Value, index, parameters);
                case DataQueryOperator.GreaterThanOrEqual:
                    return AddParameterComparison(field, ">=", filter.Value, index, parameters);
                case DataQueryOperator.LessThan:
                    return AddParameterComparison(field, "<", filter.Value, index, parameters);
                case DataQueryOperator.LessThanOrEqual:
                    return AddParameterComparison(field, "<=", filter.Value, index, parameters);
                case DataQueryOperator.StartsWith:
                    return BuildLike(field, filter.Value, index, parameters, "prefix");
                case DataQueryOperator.Contains:
                    return BuildLike(field, filter.Value, index, parameters, "contains");
                case DataQueryOperator.EndsWith:
                    return BuildLike(field, filter.Value, index, parameters, "suffix");
                case DataQueryOperator.IsNull:
                    return field + " IS NULL";
                case DataQueryOperator.IsNotNull:
                    return field + " IS NOT NULL";
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter.Operator), "Unsupported data-query operator.");
            }
        }

        private static string AddParameterComparison(string field, string comparison, object value, int index, ICollection<SqlParameter> parameters)
        {
            var name = "@p" + index;
            parameters.Add(new SqlParameter(name, value ?? DBNull.Value));
            return field + " " + comparison + " " + name;
        }

        private static string BuildLike(string field, object value, int index, ICollection<SqlParameter> parameters, string mode)
        {
            var text = value as string;
            if (text == null)
                throw new ArgumentException("StartsWith, Contains, and EndsWith filters require string values.", nameof(value));

            var escaped = EscapeLikeValue(text);
            var name = "@p" + index;
            parameters.Add(new SqlParameter(name, escaped));

            switch (mode)
            {
                case "prefix": return field + " LIKE " + name + " + '%' ESCAPE '\\'";
                case "contains": return field + " LIKE '%' + " + name + " + '%' ESCAPE '\\'";
                case "suffix": return field + " LIKE '%' + " + name + " ESCAPE '\\'";
                default: throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        private static string EscapeLikeValue(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_")
                .Replace("[", "\\[");
        }

        private static int GetCommandTimeoutSeconds(TimeSpan timeout)
        {
            var seconds = Math.Ceiling(timeout.TotalSeconds);
            if (seconds < 1) return 1;
            if (seconds > int.MaxValue) return int.MaxValue;
            return (int)seconds;
        }

        private sealed class CommandSpec
        {
            public CommandSpec(string commandText, IReadOnlyList<SqlParameter> parameters)
            {
                CommandText = commandText;
                Parameters = parameters;
            }

            public string CommandText { get; private set; }
            public IReadOnlyList<SqlParameter> Parameters { get; private set; }
        }
    }
}
