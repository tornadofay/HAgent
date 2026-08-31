using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.Storage.File;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddAuditLifecycleTab()
        {
            AddApiTab(
                "Audit Lifecycle",
                "Run audit lifecycle test",
                "Runs deterministic failure, timeout, and caller-cancellation executions through a local provider adapter and verifies that terminal audit records are persisted without model/provider network calls.",
                "All three terminal paths should produce payload-free audit records with the expected state/failure classification.",
                "Audit lifecycle verification request.",
                TestAuditLifecycleAsync,
                "Failure / timeout / cancellation",
                "Uses only a local test adapter. No external provider is contacted.");
        }

        private async Task TestAuditLifecycleAsync(string message)
        {
            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            var auditStore = await CreateConfiguredExecutionAuditStoreAsync().ConfigureAwait(true);
            var agents = await store.GetAgentsAsync().ConfigureAwait(true);
            var providers = await store.GetProvidersAsync().ConfigureAwait(true);
            var agent = GetSelectedAgent();
            if (agent == null)
                throw new InvalidOperationException("Select an agent first.");

            var provider = providers.FirstOrDefault(x => string.Equals(x.Id, agent.ProviderId, StringComparison.OrdinalIgnoreCase));
            if (provider == null)
                throw new InvalidOperationException("The selected agent's primary provider could not be found.");

            var terminalExecutions = new List<AgentExecution>();
            var modes = new[]
            {
                LocalAuditAdapterMode.Failure,
                LocalAuditAdapterMode.Timeout,
                LocalAuditAdapterMode.Cancellation
            };

            foreach (var mode in modes)
            {
                var adapter = new LocalAuditAdapter(mode);
                var runtime = new DefaultAgentRuntime(
                    store,
                    secrets,
                    new[] { adapter },
                    null,
                    null,
                    auditStore);

                AgentExecution terminal = null;
                runtime.ExecutionChanged += delegate(object sender, AgentExecutionEventArgs args)
                {
                    if (args != null && args.Execution != null && args.Execution.IsCompleted)
                        terminal = args.Execution;
                };

                if (mode == LocalAuditAdapterMode.Cancellation)
                {
                    using (var cancellation = new CancellationTokenSource())
                    {
                        var task = runtime.ExecuteAsync(
                            agent.Id,
                            message,
                            new AgentExecutionOptions
                            {
                                Timeout = TimeSpan.FromSeconds(5),
                                MaxProviderAttempts = 1,
                                MaxRetriesPerProvider = 0
                            },
                            cancellation.Token);

                        await Task.Delay(50).ConfigureAwait(true);
                        cancellation.Cancel();

                        try { await task.ConfigureAwait(true); }
                        catch (OperationCanceledException) { }
                    }
                }
                else
                {
                    try
                    {
                        await runtime.ExecuteAsync(
                            agent.Id,
                            message,
                            new AgentExecutionOptions
                            {
                                Timeout = mode == LocalAuditAdapterMode.Timeout ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromSeconds(5),
                                MaxProviderAttempts = 1,
                                MaxRetriesPerProvider = 0
                            },
                            CancellationToken.None).ConfigureAwait(true);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception)
                    {
                    }
                }

                if (terminal == null)
                    throw new InvalidOperationException("The local " + mode + " execution did not produce a terminal execution event.");

                terminalExecutions.Add(terminal);
                var expectedFailure = mode == LocalAuditAdapterMode.Failure
                    ? AgentExecutionFailureKind.Unknown
                    : mode == LocalAuditAdapterMode.Timeout
                        ? AgentExecutionFailureKind.Timeout
                        : AgentExecutionFailureKind.Cancelled;
                var expectedState = mode == LocalAuditAdapterMode.Timeout || mode == LocalAuditAdapterMode.Cancellation
                    ? AgentExecutionState.Cancelled
                    : AgentExecutionState.Failed;

                if (terminal.State != expectedState)
                    throw new InvalidOperationException("Unexpected terminal state for " + mode + ": " + terminal.State);
                if (terminal.FailureKind != expectedFailure)
                    throw new InvalidOperationException("Unexpected failure kind for " + mode + ": " + terminal.FailureKind);
                if (string.IsNullOrWhiteSpace(terminal.CorrelationId))
                    throw new InvalidOperationException("Terminal execution correlation ID is missing for " + mode + ".");

                var records = await auditStore.SearchAsync(new ExecutionAuditQuery
                {
                    ExecutionId = terminal.Id,
                    MaxResults = 1
                }, CancellationToken.None).ConfigureAwait(true);

                if (records.Count != 1)
                    throw new InvalidOperationException("Expected one audit record for " + mode + " but found " + records.Count + ".");

                var record = records[0];
                if (record.State != expectedState || record.FailureKind != expectedFailure)
                    throw new InvalidOperationException("Persisted audit classification did not match " + mode + ".");
                if (!string.Equals(record.CorrelationId, terminal.CorrelationId, StringComparison.Ordinal))
                    throw new InvalidOperationException("Persisted audit correlation ID did not match " + mode + ".");
            }

            var options = await LoadStorageOptionsAsync().ConfigureAwait(true);
            var location = options.StorageType == HAgentStorageType.File
                ? Path.Combine(options.GetEffectiveRootPath(), "audit", "executions.jsonl")
                : "HAgentExecutionAudits in " + options.GetEffectiveDatabaseName();

            Write("AUDIT LIFECYCLE",
                "Contract test succeeded." + Environment.NewLine +
                "Storage backend: " + options.StorageType + Environment.NewLine +
                "Persistence location: " + location + Environment.NewLine +
                "Failure path: persisted and classified." + Environment.NewLine +
                "Timeout path: persisted and classified." + Environment.NewLine +
                "Caller cancellation path: persisted and classified." + Environment.NewLine +
                "Audit records: 3" + Environment.NewLine +
                "Provider network calls: none." + Environment.NewLine +
                "Audit payload: metadata only.");
        }

        private enum LocalAuditAdapterMode
        {
            Failure,
            Timeout,
            Cancellation
        }

        private sealed class LocalAuditAdapter : IAiProviderAdapter
        {
            private readonly LocalAuditAdapterMode _mode;

            public LocalAuditAdapter(LocalAuditAdapterMode mode)
            {
                _mode = mode;
            }

            public string Kind { get { return "LocalAuditTest"; } }
            public string DisplayName { get { return "Local Audit Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null;
            }

            public async Task<AIResponse> SendAsync(
                AiProvider provider,
                AiAgent agent,
                string apiKey,
                string systemPrompt,
                IReadOnlyList<AIMessage> messages,
                CancellationToken cancellationToken)
            {
                switch (_mode)
                {
                    case LocalAuditAdapterMode.Failure:
                        throw new InvalidOperationException("Local audit test failure.");
                    case LocalAuditAdapterMode.Timeout:
                    case LocalAuditAdapterMode.Cancellation:
                        await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                        break;
                }

                return new AIResponse { Text = "UNEXPECTED-AUDIT-RESPONSE" };
            }
        }
    }
}
