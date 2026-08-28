using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class DefaultAgentRuntime : IAgentRuntime
    {
        private readonly IAiStore _store;
        private readonly ISecretStore _secrets;
        private readonly IReadOnlyList<IAiProviderAdapter> _adapters;
        private readonly IProviderRouter _router;

        public DefaultAgentRuntime(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IProviderRouter router = null)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? throw new ArgumentNullException(nameof(adapters))).ToList().AsReadOnly();
            _router = router ?? new DefaultProviderRouter();
        }

        public event EventHandler<AgentExecutionEventArgs> ExecutionChanged;

        public async Task<AgentExecution> ExecuteAsync(
            string agentId,
            string message,
            AgentExecutionOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("Agent id is required.", nameof(agentId));
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));

            options = options ?? new AgentExecutionOptions();
            if (options.Timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(options.Timeout), "Timeout must be greater than zero.");
            if (options.MaxProviderAttempts <= 0)
                throw new ArgumentOutOfRangeException(nameof(options.MaxProviderAttempts), "MaxProviderAttempts must be greater than zero.");

            var agents = await _store.GetAgentsAsync(cancellationToken).ConfigureAwait(false);
            var agent = agents.FirstOrDefault(x => string.Equals(x.Id, agentId, StringComparison.OrdinalIgnoreCase));
            if (agent == null) throw new InvalidOperationException("Agent was not found: " + agentId);
            if (!agent.Enabled) throw new InvalidOperationException("Agent is disabled: " + agent.Name);

            var providers = await _store.GetProvidersAsync(cancellationToken).ConfigureAwait(false);
            var snapshot = new AgentExecutionSnapshot(agent, providers);
            var messages = new List<AIMessage> { new AIMessage("user", message) }.AsReadOnly();
            var execution = new AgentExecution(snapshot, messages);

            Notify(execution);
            execution.State = AgentExecutionState.Running;
            execution.StartedAt = DateTimeOffset.UtcNow;
            Notify(execution);

            using (var timeoutCts = new CancellationTokenSource(options.Timeout))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token))
            {
                var token = linkedCts.Token;
                try
                {
                    var candidates = _router.OrderProviders(snapshot.Agent, snapshot.Providers);
                    var attempts = 0;
                    Exception lastError = null;

                    foreach (var provider in candidates)
                    {
                        if (attempts >= options.MaxProviderAttempts) break;
                        token.ThrowIfCancellationRequested();
                        attempts++;

                        var adapter = _adapters.FirstOrDefault(x => x.CanHandle(provider));
                        if (adapter == null) continue;

                        try
                        {
                            var apiKey = string.IsNullOrWhiteSpace(provider.SecretId)
                                ? string.Empty
                                : await _secrets.GetAsync(provider.SecretId, token).ConfigureAwait(false);
                            var systemPrompt = BuildSystemPrompt(provider, snapshot.Agent);
                            var outgoing = BuildOutgoingMessages(systemPrompt, execution.Messages);

                            execution.Response = await adapter.SendAsync(
                                provider,
                                snapshot.Agent,
                                apiKey,
                                systemPrompt,
                                outgoing,
                                token).ConfigureAwait(false);

                            execution.State = AgentExecutionState.Succeeded;
                            execution.CompletedAt = DateTimeOffset.UtcNow;
                            Notify(execution);
                            return execution;
                        }
                        catch (Exception ex)
                        {
                            lastError = ex;
                            if (token.IsCancellationRequested) throw;
                        }
                    }

                    throw lastError ?? new InvalidOperationException(
                        "No enabled and compatible provider could handle agent: " + snapshot.Agent.Name);
                }
                catch (OperationCanceledException)
                {
                    execution.State = AgentExecutionState.Cancelled;
                    execution.CompletedAt = DateTimeOffset.UtcNow;
                    execution.Error = cancellationToken.IsCancellationRequested
                        ? new OperationCanceledException("Agent execution was cancelled by the caller.", cancellationToken)
                        : new TimeoutException("Agent execution exceeded its configured timeout.");
                    Notify(execution);
                    throw;
                }
                catch (Exception ex)
                {
                    execution.State = AgentExecutionState.Failed;
                    execution.CompletedAt = DateTimeOffset.UtcNow;
                    execution.Error = ex;
                    Notify(execution);
                    throw;
                }
            }
        }

        private static IReadOnlyList<AIMessage> BuildOutgoingMessages(string systemPrompt, IReadOnlyList<AIMessage> messages)
        {
            var outgoing = new List<AIMessage>();
            if (!string.IsNullOrWhiteSpace(systemPrompt)) outgoing.Add(new AIMessage("system", systemPrompt));
            if (messages != null)
            {
                foreach (var message in messages)
                    if (message != null) outgoing.Add(message);
            }
            return outgoing.AsReadOnly();
        }

        private static string BuildSystemPrompt(AiProvider provider, AiAgent agent)
        {
            var providerPrompt = agent.UseProviderSystemPrompt ? provider.DefaultSystemPrompt : string.Empty;
            var agentPrompt = agent.SystemPrompt;

            if (string.IsNullOrWhiteSpace(providerPrompt)) return agentPrompt ?? string.Empty;
            if (string.IsNullOrWhiteSpace(agentPrompt)) return providerPrompt;
            return providerPrompt.Trim() + Environment.NewLine + Environment.NewLine + agentPrompt.Trim();
        }

        private void Notify(AgentExecution execution)
        {
            var handler = ExecutionChanged;
            if (handler != null)
                handler(this, new AgentExecutionEventArgs(execution));
        }
    }
}
