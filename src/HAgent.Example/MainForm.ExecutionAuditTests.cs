using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddExecutionAuditTab()
        {
            AddApiTab(
                "Execution Audit",
                "Run audit test",
                "Executes one agent request, projects the terminal execution into a secret-safe audit record, persists it using the selected HAgent storage backend, and reads it back through the trusted internal audit tool.",
                "The reopened audit record should match the execution/correlation identity and terminal lifecycle metadata without containing prompts, responses, credentials, or raw exceptions.",
                "Reply with the word AUDIT-OK and nothing else.",
                TestExecutionAuditAsync,
                "Audit boundary",
                "Audit persistence stores metadata only. It is not a transcript store and does not persist provider secrets or model payloads.");
        }

        private async Task TestExecutionAuditAsync(string message)
        {
            var selection = await CreateClientAndAgentAsync().ConfigureAwait(true);
            var request = RequireInput(message);
            var execution = await selection.Client.ExecuteAsync(
                selection.Agent.Id,
                request,
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(30),
                    MaxProviderAttempts = 2,
                    MaxRetriesPerProvider = 1
                },
                CancellationToken.None).ConfigureAwait(true);

            var record = AgentExecutionAuditRecord.FromExecution(execution);
            if (string.IsNullOrWhiteSpace(record.ExecutionId))
                throw new InvalidOperationException("Audit record did not preserve the execution ID.");
            if (string.IsNullOrWhiteSpace(record.CorrelationId))
                throw new InvalidOperationException("Audit record did not preserve the correlation ID.");
            if (!string.Equals(record.ExecutionId, execution.Id, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(record.CorrelationId, execution.CorrelationId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Audit record identity did not match the execution.");
            if (record.StartedAt.HasValue && record.CompletedAt.HasValue && record.CompletedAt.Value < record.StartedAt.Value)
                throw new InvalidOperationException("Audit record completion time precedes its start time.");

            var forbidden = new[]
            {
                request,
                execution.Response == null ? string.Empty : execution.Response.Text
            };
            var serializedShape = string.Join(" | ", new[]
            {
                record.ExecutionId,
                record.CorrelationId,
                record.AgentId,
                record.AgentName,
                record.Model,
                record.LastProviderId,
                record.LastProviderName,
                record.State.ToString(),
                record.FailureKind.ToString(),
                record.ProviderErrorKind.ToString()
            });

            foreach (var value in forbidden.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                if (serializedShape.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                    throw new InvalidOperationException("Audit projection exposed a prompt or response payload.");
            }

            var auditStore = await CreateConfiguredExecutionAuditStoreAsync(CancellationToken.None).ConfigureAwait(true);
            await auditStore.AppendAsync(record, CancellationToken.None).ConfigureAwait(true);

            var auditTool = new HAgentInternalExecutionAuditTool(auditStore);
            var internalRead = await auditTool.ExecuteAsync(new ToolExecutionContext
            {
                AgentId = selection.Agent.Id,
                ToolCallId = "execution-audit-read-42",
                CorrelationId = Guid.NewGuid().ToString("N"),
                Arguments = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "correlationId", record.CorrelationId },
                    { "maxResults", 1 }
                },
                CancellationToken = CancellationToken.None
            }).ConfigureAwait(false);

            if (!internalRead.Succeeded)
                throw new InvalidOperationException("Internal execution audit tool failed: " + internalRead.Error);
            if (internalRead.Output.IndexOf("Execution | " + record.ExecutionId, StringComparison.Ordinal) < 0)
                throw new InvalidOperationException("Internal execution audit tool did not return the persisted execution.");
            if (internalRead.Output.IndexOf("Correlation | " + record.CorrelationId, StringComparison.Ordinal) < 0)
                throw new InvalidOperationException("Internal execution audit tool did not return the persisted correlation ID.");

            var wrongAgent = await auditTool.ExecuteAsync(new ToolExecutionContext
            {
                AgentId = "different-agent-42",
                ToolCallId = "execution-audit-read-43",
                CorrelationId = Guid.NewGuid().ToString("N"),
                Arguments = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "correlationId", record.CorrelationId },
                    { "agentId", selection.Agent.Id },
                    { "maxResults", 1 }
                },
                CancellationToken = CancellationToken.None
            }).ConfigureAwait(false);

            if (!wrongAgent.Succeeded || wrongAgent.Output.IndexOf("not available", StringComparison.OrdinalIgnoreCase) < 0)
                throw new InvalidOperationException("Internal execution audit tool allowed a cross-agent audit request.");

            var invalidMax = false;
            try
            {
                await auditTool.ExecuteAsync(new ToolExecutionContext
                {
                    AgentId = selection.Agent.Id,
                    ToolCallId = "execution-audit-read-44",
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    Arguments = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "correlationId", record.CorrelationId },
                        { "maxResults", 51 }
                    },
                    CancellationToken = CancellationToken.None
                }).ConfigureAwait(false);
            }
            catch (ArgumentOutOfRangeException)
            {
                invalidMax = true;
            }

            if (!invalidMax)
                throw new InvalidOperationException("Internal execution audit tool did not reject maxResults above its hard limit.");

            var options = await LoadStorageOptionsAsync(CancellationToken.None).ConfigureAwait(true);
            var location = options.StorageType == HAgentStorageType.File
                ? Path.Combine(options.GetEffectiveRootPath(), "audit", "executions.jsonl")
                : "HAgentExecutionAudits in " + options.GetEffectiveDatabaseName();

            Write("EXECUTION AUDIT",
                "Contract test succeeded." + Environment.NewLine +
                "Storage backend: " + options.StorageType + Environment.NewLine +
                "Persistence location: " + location + Environment.NewLine +
                "Execution ID: " + record.ExecutionId + Environment.NewLine +
                "Correlation ID: " + record.CorrelationId + Environment.NewLine +
                "Agent: " + record.AgentName + Environment.NewLine +
                "Model: " + record.Model + Environment.NewLine +
                "State: " + record.State + Environment.NewLine +
                "Audit projection: payload-free and secret-safe." + Environment.NewLine +
                "Round-trip search: succeeded." + Environment.NewLine +
                "Cross-agent audit access: rejected." + Environment.NewLine +
                "maxResults=51: rejected.");
        }
    }
}
