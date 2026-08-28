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
        private readonly IProviderErrorClassifier _errorClassifier;

        public DefaultAgentRuntime(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IProviderRouter router = null,
            IProviderErrorClassifier errorClassifier = null)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? throw new ArgumentNullException(nameof(adapters))).ToList().AsReadOnly();
            _router = router ?? new DefaultProviderRouter();
            _errorClassifier = errorClassifier ?? new DefaultProviderErrorClassifier();
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
            if (options.MaxRetriesPerProvider < 0)
                throw new ArgumentOutOfRangeException(nameof(options.MaxRetriesPerProvider), "MaxRetriesPerProvider cannot be negative.");
            if (options.RetryBaseDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(options.RetryBaseDelay), "RetryBaseDelay cannot be negative.");

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
                    ProviderErrorKind lastErrorKind = ProviderErrorKind.Unknown;

                    foreach (var provider in candidates)
                    {
                        if (attempts >= options.MaxProviderAttempts) break;
                        token.ThrowIfCancellationRequested();

                        var adapter = _adapters.FirstOrDefault(x => x.CanHandle(provider));
                        if (adapter == null) continue;

                        execution.LastProviderId = provider.Id;
                        var retries = 0;

                        while (true)
                        {
                            token.ThrowIfCancellationRequested();
                            attempts++;
                            if (attempts > options.MaxProviderAttempts) break;

                            try
                            {
                                var apiKey = string.IsNullOrWhiteSpace(provider.SecretId)
                                    ? string.Empty
                                    : await _secrets.GetAsync(provider.SecretId, token).ConfigureAwait(false);
                                var systemPrompt = BuildSystemPrompt(provider, snapshot.Agent);

                                execution.Response = await adapter.SendAsync(
                                    provider,
                                    snapshot.Agent,
                                    apiKey,
                                    systemPrompt,
                                    execution.Messages,
                                    token).ConfigureAwait(false);

                                execution.State = AgentExecutionState.Succeeded;
                                execution.FailureKind = AgentExecutionFailureKind.None;
                                execution.ProviderErrorKind = ProviderErrorKind.Unknown;
                                execution.CompletedAt = DateTimeOffset.UtcNow;
                                Notify(execution);
                                return execution;
                            }
                            catch (Exception ex)
                            {
                                lastError = ex;
                                lastErrorKind = ClassifyProviderError(ex);
                                execution.ProviderErrorKind = lastErrorKind;
                                if (token.IsCancellationRequested) throw;

                                var retryable = lastErrorKind == ProviderErrorKind.Transient ||
                                                lastErrorKind == ProviderErrorKind.Unavailable ||
                                                lastErrorKind == ProviderErrorKind.RateLimited;

                                if (!retryable || retries >= options.MaxRetriesPerProvider)
                                    break;

                                retries++;
                                var delay = CalculateBackoff(options.RetryBaseDelay, retries, lastErrorKind == ProviderErrorKind.RateLimited);
                                if (delay > TimeSpan.Zero)
                                    await Task.Delay(delay, token).ConfigureAwait(false);
                            }
                        }
                    }

                    execution.FailureKind = lastErrorKind == ProviderErrorKind.Authentication ||
                                            lastErrorKind == ProviderErrorKind.InvalidRequest ||
                                            lastErrorKind == ProviderErrorKind.ModelTermsRequired ||
                                            lastErrorKind == ProviderErrorKind.PermissionDenied ||
                                            lastErrorKind == ProviderErrorKind.ModelNotFound
                        ? AgentExecutionFailureKind.Configuration
                        : lastErrorKind == ProviderErrorKind.Unavailable
                            ? AgentExecutionFailureKind.ProviderUnavailable
                            : lastErrorKind == ProviderErrorKind.Transient || lastErrorKind == ProviderErrorKind.RateLimited
                                ? AgentExecutionFailureKind.ProviderFailed
                                : AgentExecutionFailureKind.Unknown;

                    throw lastError ?? new InvalidOperationException(
                        "No enabled and compatible provider could handle agent: " + snapshot.Agent.Name);
                }
                catch (OperationCanceledException)
                {
                    execution.State = AgentExecutionState.Cancelled;
                    execution.CompletedAt = DateTimeOffset.UtcNow;
                    execution.FailureKind = cancellationToken.IsCancellationRequested
                        ? AgentExecutionFailureKind.Cancelled
                        : AgentExecutionFailureKind.Timeout;
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
                    if (execution.FailureKind == AgentExecutionFailureKind.None)
                        execution.FailureKind = AgentExecutionFailureKind.Unknown;
                    execution.Error = ex;
                    Notify(execution);
                    throw;
                }
            }
        }

        private ProviderErrorKind ClassifyProviderError(Exception exception)
        {
            var message = exception == null ? string.Empty : (exception.Message ?? string.Empty);
            if (message.IndexOf("model_terms_required", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("requires terms acceptance", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("terms acceptance", StringComparison.OrdinalIgnoreCase) >= 0)
                return ProviderErrorKind.ModelTermsRequired;

            if (message.IndexOf("model_not_found", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("model not found", StringComparison.OrdinalIgnoreCase) >= 0)
                return ProviderErrorKind.ModelNotFound;

            if (message.IndexOf("permission_denied", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("permission denied", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("does not have access", StringComparison.OrdinalIgnoreCase) >= 0)
                return ProviderErrorKind.PermissionDenied;

            return _errorClassifier.Classify(exception);
        }

        private static TimeSpan CalculateBackoff(TimeSpan baseDelay, int retryNumber, bool rateLimited)
        {
            if (baseDelay <= TimeSpan.Zero) return TimeSpan.Zero;
            var multiplier = Math.Pow(2, Math.Max(0, retryNumber - 1));
            if (rateLimited) multiplier *= 2;
            var milliseconds = Math.Min(baseDelay.TotalMilliseconds * multiplier, 30000d);
            return TimeSpan.FromMilliseconds(milliseconds);
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
