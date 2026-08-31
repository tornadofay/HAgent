using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestDataQueryContractAsync(string unused)
        {
            var schema = new DataQuerySchema(new[] { "Id", "Name", "Amount" });
            var authorizer = new ExampleDataAccessAuthorizer();
            var source = new InMemoryDataQuerySource(new[]
            {
                Row(1, "Alice", 120),
                Row(2, "Bob", 40),
                Row(3, "Carol", 90),
                Row(4, "David", 150),
                Row(5, "Eve", 60)
            }, schema, authorizer, "orders", "example-agent");

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
                throw new InvalidOperationException("Structured data query did not enforce bounded paging correctly.");
            if (!string.Equals(Convert.ToString(result.Rows[0]["Name"]), "David", StringComparison.Ordinal) ||
                !string.Equals(Convert.ToString(result.Rows[1]["Name"]), "Alice", StringComparison.Ordinal))
                throw new InvalidOperationException("Structured data query did not apply the requested sort/filter semantics.");
            if (result.Rows[0].Count != 3 || result.Rows[0].ContainsKey("Secret"))
                throw new InvalidOperationException("Structured data query did not enforce the explicit field projection.");

            var unauthorizedFieldRequest = new DataQueryRequest
            {
                Fields = new[] { "Id", "Secret" },
                Take = 1
            };
            try
            {
                await source.QueryAsync(unauthorizedFieldRequest, CancellationToken.None);
                throw new InvalidOperationException("Application-owned data source accepted a field outside its authoritative schema.");
            }
            catch (ArgumentException)
            {
            }

            var duplicateFields = new DataQueryRequest
            {
                Fields = new[] { "Id", "id" },
                Take = 1
            };
            try
            {
                duplicateFields.Validate();
                throw new InvalidOperationException("Structured data query accepted duplicate projected fields.");
            }
            catch (ArgumentException)
            {
            }

            var deniedAuthorizer = new ExampleDataAccessAuthorizer(false);
            var deniedSource = new InMemoryDataQuerySource(new[]
            {
                Row(1, "Alice", 120)
            }, schema, deniedAuthorizer, "orders", "example-agent");
            try
            {
                await deniedSource.QueryAsync(request, CancellationToken.None);
                throw new InvalidOperationException("Data source executed a query after host authorization denied the operation.");
            }
            catch (UnauthorizedAccessException)
            {
            }

            var observed = authorizer.LastRequest;
            if (observed == null || observed.Operation != DataAccessOperation.ProjectionQuery ||
                !string.Equals(observed.SourceId, "orders", StringComparison.Ordinal) ||
                !string.Equals(observed.RuntimeIdentity, "example-agent", StringComparison.Ordinal) ||
                !object.ReferenceEquals(observed.Query, request))
                throw new InvalidOperationException("Host authorization callback did not receive the runtime identity, source, operation, and query context.");

            Write("DATA QUERY CONTRACT",
                "Contract test succeeded." + Environment.NewLine +
                "Fields: Id, Name, Amount" + Environment.NewLine +
                "Filters: Amount >= 60; Name != Eve" + Environment.NewLine +
                "Sort: Amount descending" + Environment.NewLine +
                "Page: 0 / 2" + Environment.NewLine +
                "Returned: " + result.Returned + Environment.NewLine +
                "Has more: " + result.HasMore + Environment.NewLine +
                "First rows: " + Convert.ToString(result.Rows[0]["Name"]) + ", " + Convert.ToString(result.Rows[1]["Name"]) + Environment.NewLine +
                "Authoritative schema rejected the non-approved Secret field." + Environment.NewLine +
                "Projection/query permission accepted the authorized source and rejected the denied source." + Environment.NewLine +
                "Host authorization callback received operation, source, runtime identity, and query context." + Environment.NewLine +
                "No SQL or executable expression accepted by the query contract.");
        }

        private static IReadOnlyDictionary<string, object> Row(int id, string name, int amount)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = id,
                ["Name"] = name,
                ["Amount"] = amount,
                ["Secret"] = "not projected"
            };
        }

        private sealed class ExampleDataAccessAuthorizer : IDataAccessAuthorizer
        {
            private readonly bool _allow;

            public ExampleDataAccessAuthorizer(bool allow = true)
            {
                _allow = allow;
            }

            public DataAuthorizationRequest LastRequest { get; private set; }

            public Task<bool> AuthorizeAsync(DataAuthorizationRequest request, CancellationToken cancellationToken = default(CancellationToken))
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                cancellationToken.ThrowIfCancellationRequested();
                LastRequest = request;
                return Task.FromResult(_allow);
            }
        }

        private sealed class InMemoryDataQuerySource : IDataQuerySource
        {
            private readonly IReadOnlyList<IReadOnlyDictionary<string, object>> _rows;
            private readonly DataQuerySchema _schema;
            private readonly IDataAccessAuthorizer _authorizer;
            private readonly string _sourceId;
            private readonly string _runtimeIdentity;

            public InMemoryDataQuerySource(
                IEnumerable<IReadOnlyDictionary<string, object>> rows,
                DataQuerySchema schema,
                IDataAccessAuthorizer authorizer,
                string sourceId,
                string runtimeIdentity)
            {
                if (rows == null) throw new ArgumentNullException(nameof(rows));
                _schema = schema ?? throw new ArgumentNullException(nameof(schema));
                _authorizer = authorizer ?? throw new ArgumentNullException(nameof(authorizer));
                if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("Source ID is required.", nameof(sourceId));
                if (string.IsNullOrWhiteSpace(runtimeIdentity)) throw new ArgumentException("Runtime identity is required.", nameof(runtimeIdentity));
                _sourceId = sourceId;
                _runtimeIdentity = runtimeIdentity;
                _rows = rows.ToList().AsReadOnly();
            }

            public async Task<DataQueryResult> QueryAsync(DataQueryRequest request, CancellationToken cancellationToken = default(CancellationToken))
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                _schema.ValidateRequest(request);

                var authorized = await _authorizer.AuthorizeAsync(new DataAuthorizationRequest
                {
                    Operation = DataAccessOperation.ProjectionQuery,
                    SourceId = _sourceId,
                    RuntimeIdentity = _runtimeIdentity,
                    Query = request
                }, cancellationToken).ConfigureAwait(false);
                if (!authorized)
                    throw new UnauthorizedAccessException("The host authorization callback denied the structured data projection/query operation.");

                cancellationToken.ThrowIfCancellationRequested();

                var filtered = _rows.Where(row => MatchesFilters(row, request.Filters)).ToList();
                foreach (var sort in request.Sorts.Reverse())
                {
                    filtered = sort.Descending
                        ? filtered.OrderByDescending(row => ValueOf(row, sort.Field)).ToList()
                        : filtered.OrderBy(row => ValueOf(row, sort.Field)).ToList();
                }

                var skipped = Math.Min(request.Skip, filtered.Count);
                var page = filtered.Skip(skipped).Take(request.Take).ToList();
                var projected = page.Select(row =>
                {
                    var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (var field in request.Fields)
                        result[field] = ValueOf(row, field);
                    return (IReadOnlyDictionary<string, object>)result;
                }).ToList();

                return new DataQueryResult
                {
                    Rows = projected.AsReadOnly(),
                    Skipped = skipped,
                    Returned = projected.Count,
                    HasMore = skipped + projected.Count < filtered.Count
                };
            }

            private static bool MatchesFilters(IReadOnlyDictionary<string, object> row, IReadOnlyList<DataFilterCondition> filters)
            {
                foreach (var filter in filters)
                {
                    var actual = ValueOf(row, filter.Field);
                    if (filter.Operator == DataQueryOperator.IsNull && actual != null) return false;
                    if (filter.Operator == DataQueryOperator.IsNotNull && actual == null) return false;
                    if (!Evaluate(actual, filter.Operator, filter.Value)) return false;
                }
                return true;
            }

            private static bool Evaluate(object actual, DataQueryOperator op, object expected)
            {
                if (op == DataQueryOperator.IsNull || op == DataQueryOperator.IsNotNull) return true;
                var left = actual as IComparable;
                var right = expected as IComparable;
                var comparison = left == null || right == null ? StringComparer.OrdinalIgnoreCase.Compare(Convert.ToString(actual), Convert.ToString(expected)) : left.CompareTo(expected);
                switch (op)
                {
                    case DataQueryOperator.Equal: return object.Equals(actual, expected);
                    case DataQueryOperator.NotEqual: return !object.Equals(actual, expected);
                    case DataQueryOperator.GreaterThan: return comparison > 0;
                    case DataQueryOperator.GreaterThanOrEqual: return comparison >= 0;
                    case DataQueryOperator.LessThan: return comparison < 0;
                    case DataQueryOperator.LessThanOrEqual: return comparison <= 0;
                    case DataQueryOperator.StartsWith: return Convert.ToString(actual).StartsWith(Convert.ToString(expected), StringComparison.OrdinalIgnoreCase);
                    case DataQueryOperator.Contains: return Convert.ToString(actual).IndexOf(Convert.ToString(expected), StringComparison.OrdinalIgnoreCase) >= 0;
                    case DataQueryOperator.EndsWith: return Convert.ToString(actual).EndsWith(Convert.ToString(expected), StringComparison.OrdinalIgnoreCase);
                    default: return false;
                }
            }

            private static object ValueOf(IReadOnlyDictionary<string, object> row, string field)
            {
                object value;
                if (!row.TryGetValue(field, out value))
                    throw new ArgumentException("The requested field does not exist in the source: " + field, nameof(field));
                return value;
            }
        }
    }
}
