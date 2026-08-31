using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Read-only trusted tool for bounded inspection of HAgent execution audit metadata.
    /// </summary>
    public sealed class HAgentInternalExecutionAuditTool : IAgentTool
    {
        private const int DefaultMaxResults = 20;
        private const int MaximumMaxResults = 50;

        private readonly IExecutionAuditStore _auditStore;

        public HAgentInternalExecutionAuditTool(IExecutionAuditStore auditStore)
        {
            if (auditStore == null) throw new ArgumentNullException(nameof(auditStore));
            _auditStore = auditStore;
            Definition = CreateDefinition();
        }

        public AiTool Definition { get; private set; }

        public async Task<ToolExecutionResult> ExecuteAsync(ToolExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.CancellationToken.ThrowIfCancellationRequested();

            var requestedAgentId = ResolveOptionalString(context.Arguments, "agentId");
            if (!string.IsNullOrWhiteSpace(context.AgentId) &&
                !string.IsNullOrWhiteSpace(requestedAgentId) &&
                !string.Equals(context.AgentId, requestedAgentId, StringComparison.OrdinalIgnoreCase))
                return ToolExecutionResult.Success("Audit records are not available for another agent.");

            var query = new ExecutionAuditQuery
            {
                ExecutionId = ResolveOptionalString(context.Arguments, "executionId"),
                CorrelationId = ResolveOptionalString(context.Arguments, "correlationId"),
                AgentId = string.IsNullOrWhiteSpace(context.AgentId) ? requestedAgentId : context.AgentId,
                MaxResults = ResolveMaxResults(context.Arguments)
            };

            if (string.IsNullOrWhiteSpace(query.ExecutionId) &&
                string.IsNullOrWhiteSpace(query.CorrelationId) &&
                string.IsNullOrWhiteSpace(query.AgentId))
                throw new ArgumentException("At least one of executionId, correlationId, or agentId is required.", nameof(context.Arguments));

            var records = await _auditStore.SearchAsync(query, context.CancellationToken).ConfigureAwait(false);
            var output = new StringBuilder();
            output.AppendLine("HAgent execution audit");
            output.AppendLine("Returned records: " + records.Count);

            foreach (var record in records)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                output.AppendLine("Execution | " + Safe(record.ExecutionId));
                output.AppendLine("Correlation | " + Safe(record.CorrelationId));
                output.AppendLine("Agent | " + Safe(record.AgentName) + " (" + Safe(record.AgentId) + ")");
                output.AppendLine("Model | " + Safe(record.Model));
                output.AppendLine("Provider | " + Safe(record.LastProviderName) + " (" + Safe(record.LastProviderId) + ")");
                output.AppendLine("State | " + record.State);
                output.AppendLine("Failure | " + record.FailureKind);
                output.AppendLine("ProviderError | " + record.ProviderErrorKind);
                output.AppendLine("CreatedAt | " + record.CreatedAt.ToString("O"));
                output.AppendLine("StartedAt | " + (record.StartedAt.HasValue ? record.StartedAt.Value.ToString("O") : string.Empty));
                output.AppendLine("CompletedAt | " + (record.CompletedAt.HasValue ? record.CompletedAt.Value.ToString("O") : string.Empty));
                output.AppendLine("DurationMs | " + (record.Duration.HasValue ? record.Duration.Value.TotalMilliseconds.ToString("0.###") : string.Empty));
                output.AppendLine();
            }

            return ToolExecutionResult.Success(output.ToString().TrimEnd());
        }

        private static string ResolveOptionalString(IReadOnlyDictionary<string, object> arguments, string key)
        {
            object value;
            if (arguments == null || !arguments.TryGetValue(key, out value) || value == null)
                return string.Empty;
            return Convert.ToString(value).Trim();
        }

        private static int ResolveMaxResults(IReadOnlyDictionary<string, object> arguments)
        {
            object rawValue;
            if (arguments == null || !arguments.TryGetValue("maxResults", out rawValue) || rawValue == null)
                return DefaultMaxResults;

            int value;
            try { value = Convert.ToInt32(rawValue); }
            catch (Exception ex)
            {
                throw new ArgumentException("maxResults must be an integer between 1 and " + MaximumMaxResults + ".", nameof(arguments), ex);
            }

            if (value < 1 || value > MaximumMaxResults)
                throw new ArgumentOutOfRangeException(nameof(arguments), "maxResults must be between 1 and " + MaximumMaxResults + ".");

            return value;
        }

        private static AiTool CreateDefinition()
        {
            return new AiTool
            {
                Id = "hagent.internal.execution-audit",
                Name = "HAgent Internal Execution Audit",
                Description = "Read-only bounded inspection of secret-safe HAgent execution audit metadata.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"executionId\":{\"type\":\"string\",\"minLength\":1},\"correlationId\":{\"type\":\"string\",\"minLength\":1},\"agentId\":{\"type\":\"string\",\"minLength\":1},\"maxResults\":{\"type\":\"integer\",\"minimum\":1,\"maximum\":50}},\"additionalProperties\":false}",
                Category = "BuiltIn",
                Type = AiToolType.BuiltIn,
                IsBuiltIn = true,
                Enabled = true
            };
        }

        private static string Safe(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\r", " ").Replace("\n", " ");
        }
    }
}
