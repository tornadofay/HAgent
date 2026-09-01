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
        private readonly IExecutionAuditStore _auditStore;
        private readonly ExecutionAuditOptions _auditOptions;

        public DefaultAgentRuntime(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IProviderRouter router = null,
            IProviderErrorClassifier errorClassifier = null,
            IExecutionAuditStore auditStore = null)
            : this(store, secrets, adapters, router, errorClassifier, auditStore, null)
        {
        }

        public DefaultAgentRuntime(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IProviderRouter router,
            IProviderErrorClassifier errorClassifier,
            IExecutionAuditStore auditStore,
            ExecutionAuditOptions auditOptions)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? throw new ArgumentNullException(nameof(adapters))).ToList().AsReadOnly();
            _router = router ?? new DefaultProviderRouter();
            _errorClassifier = errorClassifier ?? new DefaultProviderErrorClassifier();
            _auditStore = auditStore;
            _auditOptions = auditOptions ?? new ExecutionAuditOptions();
            _auditOptions.Validate();
        }

        public event EventHandler<AgentExecutionEventArgs> ExecutionChanged;

        public Task<AgentExecution> ExecuteAsync(
            string agentId,
            string message,
            AgentExecutionOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
            return ExecuteAsync(
                new AgentExecutionRequest
                {
                    AgentId = agentId,
                    Messages = new List<AIMessage> { new AIMessage("user", message) }.AsReadOnly(),
                    HostCorrelationId = options == null ? string.Empty : options.HostCorrelationId,
                    HostContext = options == null ? null : new Dictionary<string, string>(options.HostContext ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)),
                    Options = options ?? new AgentExecutionOptions()
                },
                cancellationToken);
        }

        public async Task<AgentExecution> ExecuteAsync(
            AgentExecutionRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();

            var options = request.Options ?? new AgentExecutionOptions();
            ValidateOptions(options);

            var agents = await _store.GetAgentsAsync(cancellationToken).ConfigureAwait(false);
            var agent = agents.FirstOrDefault(x => string.Equals(x.Id, request.AgentId, StringComparison.OrdinalIgnoreCase));
            if (agent == null) throw new InvalidOperationException("Agent was not found: " + request.AgentId);
            if (!agent.Enabled) throw new InvalidOperationException("Agent is disabled: " + agent.Name);

            var providers = await _store.GetProvidersAsync(cancellationToken).ConfigureAwait(false);
            var snapshot = new AgentExecutionSnapshot(agent, providers, options.RuntimeOverrides, request.HostContext);
            var messages = new List<AIMessage>(request.Messages).AsReadOnly();
            var execution = new AgentExecution(snapshot, messages);
            execution.HostCorrelationId = request.HostCorrelationId ?? string.Empty;
            execution.RuntimeInstanceId = options.RuntimeInstanceId;
            execution.RuntimeInstanceRevision = options.RuntimeInstanceRevision;

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
                    string lastProviderName = string.Empty;
                    string lastModel = string.Empty;

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
                                var systemPrompt = BuildSystemPrompt(provider, snapshot.Agent, options.SystemPromptLayers);
                                lastProviderName = provider.Name;
                                lastModel = string.IsNullOrWhiteSpace(snapshot.Agent.Model) ? provider.DefaultModel : snapshot.Agent.Model;

                                var providerRequest = new ProviderExecutionRequest
                                {
                                    Provider = provider,
                                    Agent = snapshot.Agent,
                                    ApiKey = apiKey,
                                    SystemPrompt = systemPrompt,
                                    Messages = execution.Messages,
                                    StructuredOutput = request.StructuredOutput
                                };

                                var response = await adapter.SendAsync(providerRequest, token).ConfigureAwait(false);
                                if (token.IsCancellationRequested)
                                    throw new OperationCanceledException("Agent execution was cancelled before the provider response became authoritative.", token);

                                execution.Response = response;

                                if (request.StructuredOutput != null)
                                {
                                    var structuredValidation = StructuredOutputValidator.Validate(
                                        request.StructuredOutput,
                                        execution.Response == null ? string.Empty : execution.Response.StructuredOutputJson);
                                    if (!structuredValidation.IsValid)
                                    {
                                        throw new InvalidOperationException(
                                            "Structured output validation failed: " + string.Join(" ", structuredValidation.Errors));
                                    }
                                }

                                if (execution.TryCompleteSucceeded(execution.Response, DateTimeOffset.UtcNow))
                                {
                                    Notify(execution);
                                    await PersistAuditAsync(execution).ConfigureAwait(false);
                                    return execution;
                                }

                                throw new InvalidOperationException("Execution reached a terminal state before the provider response could be committed.");
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

                    if (lastError != null)
                    {
                        var actionable = ProviderErrorAdvisor.GetActionableMessage(
                            lastErrorKind,
                            lastProviderName,
                            lastModel,
                            lastError.Message);

                        if (!string.Equals(actionable, lastError.Message, StringComparison.Ordinal))
                            throw new InvalidOperationException(actionable, lastError);
                    }

                    var finalFailure = lastError ?? new InvalidOperationException(
                        "No enabled and compatible provider could handle agent: " + snapshot.Agent.Name);
                    if (execution.TryCompleteFailed(
                        finalFailure,
                        execution.FailureKind,
                        lastErrorKind,
                        DateTimeOffset.UtcNow))
                    {
                        Notify(execution);
                        await PersistAuditAsync(execution).ConfigureAwait(false);
                    }
                    throw finalFailure;
                }
                catch (OperationCanceledException)
                {
                    var cancellationFailureKind = cancellationToken.IsCancellationRequested
                        ? AgentExecutionFailureKind.Cancelled
                        : AgentExecutionFailureKind.Timeout;
                    var cancellationError = cancellationToken.IsCancellationRequested
                        ? new OperationCanceledException("Agent execution was cancelled by the caller.", cancellationToken)
                        : new TimeoutException("Agent execution exceeded its configured timeout.");

                    if (execution.TryCompleteCancelled(
                        cancellationError,
                        cancellationFailureKind,
                        DateTimeOffset.UtcNow))
                    {
                        Notify(execution);
                        await PersistAuditAsync(execution).ConfigureAwait(false);
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    if (token.IsCancellationRequested)
                    {
                        var cancellationFailureKind = cancellationToken.IsCancellationRequested
                            ? AgentExecutionFailureKind.Cancelled
                            : AgentExecutionFailureKind.Timeout;
                        var cancellationError = cancellationToken.IsCancellationRequested
                            ? new OperationCanceledException("Agent execution was cancelled by the caller.", cancellationToken)
                            : new TimeoutException("Agent execution exceeded its configured timeout.");

                        if (execution.TryCompleteCancelled(
                            cancellationError,
                            cancellationFailureKind,
                            DateTimeOffset.UtcNow))
                        {
                            Notify(execution);
                            await PersistAuditAsync(execution).ConfigureAwait(false);
                        }
                        throw cancellationError;
                    }

                    var failureKind = execution.FailureKind == AgentExecutionFailureKind.None
                        ? AgentExecutionFailureKind.Unknown
                        : execution.FailureKind;
                    if (execution.TryCompleteFailed(
                        ex,
                        failureKind,
                        execution.ProviderErrorKind,
                        DateTimeOffset.UtcNow))
                    {
                        Notify(execution);
                        await PersistAuditAsync(execution).ConfigureAwait(false);
                    }
                    throw;
                }
            }
        }

        private static void ValidateOptions(AgentExecutionOptions options)
        {
            if (options.Timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(options.Timeout), "Timeout must be greater than zero.");
            if (options.MaxProviderAttempts <= 0)
                throw new ArgumentOutOfRangeException(nameof(options.MaxProviderAttempts), "MaxProviderAttempts must be greater than zero.");
            if (options.MaxRetriesPerProvider < 0)
                throw new ArgumentOutOfRangeException(nameof(options.MaxRetriesPerProvider), "MaxRetriesPerProvider cannot be negative.");
            if (options.RetryBaseDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(options.RetryBaseDelay), "RetryBaseDelay cannot be negative.");
        }

        private async Task PersistAuditAsync(AgentExecution execution)
        {
            if (_auditStore == null || !_auditOptions.Enabled) return;
            try
            {
                await _auditStore.AppendAsync(
                    AgentExecutionAuditRecord.FromExecution(execution),
                    CancellationToken.None).ConfigureAwait(false);
                await _auditStore.TrimAsync(_auditOptions.GetEffectiveMaxRecords(), CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // Audit persistence must not change the execution outcome.
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

        private static string BuildSystemPrompt(AiProvider provider, AiAgent agent, IEnumerable<SystemPromptLayer> executionLayers)
        {
            var layers = new List<SystemPromptLayer>();
            if (agent.UseProviderSystemPrompt && !string.IsNullOrWhiteSpace(provider.DefaultSystemPrompt))
                layers.Add(new SystemPromptLayer("provider", "Provider", provider.DefaultSystemPrompt, 100));

            if (!string.IsNullOrWhiteSpace(agent.SystemPrompt))
                layers.Add(new SystemPromptLayer("agent", "Agent", agent.SystemPrompt, 200));

            if (executionLayers != null)
                layers.AddRange(executionLayers);

            return SystemPromptComposer.Compose(layers);
        }

        private void Notify(AgentExecution execution)
        {
            var handler = ExecutionChanged;
            if (handler != null)
                handler(this, new AgentExecutionEventArgs(execution));
        }
    }
}
