using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddExecutionAuditTab()
        {
            AddApiTab(
                "Execution Audit",
                "Run audit test",
                "Executes one agent request, projects the terminal execution into a secret-safe audit record, persists it using the selected HAgent storage backend, and reads it back by correlation ID.",
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
            if (record.ExecutionId != execution.Id || record.CorrelationId != execution.CorrelationId)
                throw new InvalidOperationException("Audit record identity did not match the execution.");
            if (record.StartedAt.HasValue && record.CompletedAt.HasValue && record.CompletedAt.Value < record.StartedAt.Value)
                throw new InvalidOperationException("Audit record completion time precedes its start time.");

            var forbidden = new[]
            {
                request,
                execution.Response == null ? string.Empty : execution.Response.Text,
                "password",
                "apikey",
                "connectionstring"
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
                    throw new InvalidOperationException("Audit projection exposed forbidden payload or sensitive text.");
            }

            var auditStore = await CreateConfiguredExecutionAuditStoreAsync(CancellationToken.None).ConfigureAwait(true);
            await auditStore.AppendAsync(record, CancellationToken.None).ConfigureAwait(true);
            var reopened = await auditStore.SearchAsync(new ExecutionAuditQuery
            {
                CorrelationId = record.CorrelationId,
                MaxResults = 1
            }, CancellationToken.None).ConfigureAwait(true);

            var restored = reopened.FirstOrDefault();
            if (restored == null)
                throw new InvalidOperationException("Persisted audit record could not be reopened by correlation ID.");
            if (!string.Equals(restored.ExecutionId, record.ExecutionId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(restored.CorrelationId, record.CorrelationId, StringComparison.OrdinalIgnoreCase) ||
                restored.State != record.State ||
                restored.FailureKind != record.FailureKind ||
                restored.ProviderErrorKind != record.ProviderErrorKind)
                throw new InvalidOperationException("Persisted audit record did not round-trip the execution metadata.");

            var options = await LoadStorageOptionsAsync(CancellationToken.None).ConfigureAwait(true);
            var location = options.StorageType == HAgentStorageType.File
                ? Path.Combine(options.GetEffectiveRootPath(), "audit", "executions.jsonl")
                : "HAgentExecutionAudits in " + options.GetEffectiveDatabaseName();

            Write("EXECUTION AUDIT",
                "Contract test succeeded." + Environment.NewLine +
                "Storage backend: " + options.StorageType + Environment.NewLine +
                "Persistence location: " + location + Environment.NewLine +
                "Execution ID: " + restored.ExecutionId + Environment.NewLine +
                "Correlation ID: " + restored.CorrelationId + Environment.NewLine +
                "Agent: " + restored.AgentName + Environment.NewLine +
                "Model: " + restored.Model + Environment.NewLine +
                "State: " + restored.State + Environment.NewLine +
                "Audit projection: payload-free and secret-safe." + Environment.NewLine +
                "Round-trip search: succeeded.");
        }
    }
}
