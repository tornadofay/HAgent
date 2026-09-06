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
        private readonly IExecutionPlanner _executionPlanner;
        private readonly IExecutionAuditStore _auditStore;
        private readonly ExecutionAuditOptions _auditOptions;

        public DefaultAgentRuntime(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IProviderRouter router = null,
            IProviderErrorClassifier errorClassifier = null,
            IExecutionAuditStore auditStore = null)
            : this(store, secrets, adapters, router, errorClassifier, auditStore, null, null)
        {
        }

        public DefaultAgentRuntime(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IProviderRouter router,
            IProviderErrorClassifier errorClassifier,
            IExecutionAuditStore auditStore,
            ExecutionAuditOptions auditOptions,
            IExecutionPlanner executionPlanner = null)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? throw new ArgumentNullException(nameof(adapters))).ToList().AsReadOnly();
            _router = router ?? new DefaultProviderRouter();
            _errorClassifier = errorClassifier ?? new DefaultProviderErrorClassifier();
            _executionPlanner = executionPlanner ?? new DefaultExecutionPlanner();
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
            var snapshot = new AgentExecutionSnapshot(
                agent,
                providers,
                options.RuntimeOverrides,
                request.HostContext,
                request.Identity);
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
                    var selectionPolicy = request.ExecutionSelection == null
                        ? (snapshot.Agent.ExecutionSelection == null ? new AiExecutionSelectionPolicy() : snapshot.Agent.ExecutionSelection.Clone())
                        : request.ExecutionSelection.Clone();
                    var requirements = request.CapabilityRequirements == null
                        ? (snapshot.Agent.CapabilityRequirements == null ? new AiCapabilityRequirements() : snapshot.Agent.CapabilityRequirements.Clone())
                        : request.CapabilityRequirements.Clone();

                    if (request.StructuredOutput != null)
                        requirements.Require(AiCapability.StructuredOutput);

                    selectionPolicy.Validate();
                    var targets = BuildExecutionTargets(snapshot.Providers, snapshot.Agent);
                    var plan = _executionPlanner.Plan(targets, requirements, selectionPolicy);
                    if (!plan.HasSelection)
                    {
                        throw new InvalidOperationException(
                            "No compatible execution target was selected for agent '" + snapshot.Agent.Name + "'. " +
                            BuildPlannerFailureSummary(plan));
                    }

                    var selectedTarget = plan.SelectedTarget;
                    var candidates = _router
                        .OrderProviders(snapshot.Agent, snapshot.Providers)
                        .Where(x => string.Equals(x.Id, selectedTarget.ProviderId, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (candidates.Count == 0)
                        throw new InvalidOperationException("The selected execution target references a provider that is not available: " + selectedTarget.ProviderId);

                    var attempts = 0;
                    Exception lastError = null;
                    ProviderErrorKind lastErrorKind = ProviderErrorKind.Unknown;
                    string lastProviderName = string.Empty;
                    string lastModel = selectedTarget.ModelId;

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

                                var providerRequest = new ProviderExecutionRequest
                                {
                                    Provider = provider,
                                    Agent = snapshot.Agent,
                                    ApiKey = apiKey,
                                    SystemPrompt = systemPrompt,
                                    Messages = execution.Messages,
                                    StructuredOutput = request.StructuredOutput
                                };

                                var response = await AwaitProviderResponseAsync(
                                    adapter.SendAsync(providerRequest, token),
                                    token).ConfigureAwait(false);
                                if (token.IsCancellationRequested)
                                    throw new OperationCanceledException("Agent execution was cancelled before the provider response became authoritative.", token);

                                if (request.StructuredOutput != null)
                                {
                                    var structuredValidation = StructuredOutputValidator.Validate(
                                        request.StructuredOutput,
                                        response == null ? string.Empty : response.StructuredOutputJson);
                                    if (!structuredValidation.IsValid)
                                    {
                                        throw new InvalidOperationException(
                                            "Structured output validation failed: " + string.Join(" ", structuredValidation.Errors));
                                    }
                                }

                                if (execution.TryCompleteSucceeded(response, DateTimeOffset.UtcNow))
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
                        "Execution planner selected no executable provider target for agent: " + snapshot.Agent.Name);
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
                    Exception cancellationError;
                    if (cancellationToken.IsCancellationRequested)
                    {
                        cancellationError = new OperationCanceledException(
                            "Agent execution was cancelled by the caller.",
                            cancellationToken);
                    }
                    else
                    {
                        cancellationError = new TimeoutException(
                            "Agent execution exceeded its configured timeout.");
                    }

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
                        Exception cancellationError;
                        if (cancellationToken.IsCancellationRequested)
                        {
                            cancellationError = new OperationCanceledException(
                                "Agent execution was cancelled by the caller.",
                                cancellationToken);
                        }
                        else
                        {
                            cancellationError = new TimeoutException(
                                "Agent execution exceeded its configured timeout.");
                        }

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

        private static IReadOnlyList<AiExecutionTarget> BuildExecutionTargets(
            IReadOnlyList<AiProvider> providers,
            AiAgent agent)
        {
            var targets = new List<AiExecutionTarget>();
            if (providers == null) return targets.AsReadOnly();

            foreach (var provider in providers)
            {
                if (provider == null || !provider.Enabled) continue;
                var modelId = string.Empty;
                if (agent != null && string.Equals(provider.Id, agent.ProviderId, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(agent.Model))
                    modelId = agent.Model;
                if (string.IsNullOrWhiteSpace(modelId))
                    modelId = provider.DefaultModel;
                if (string.IsNullOrWhiteSpace(modelId)) continue;

                var target = new AiExecutionTarget
                {
                    Id = provider.Id + "::" + modelId,
                    ProviderId = provider.Id,
                    ModelId = modelId,
                    LogicalModelId = modelId,
                    DeploymentId = provider.Id + "::" + modelId,
                    Capabilities = new AiModelCapabilities(),
                    Cost = AiCostStatus.Unknown,
                    Availability = AiAvailabilityState.Unknown
                };
                target.Validate();
                targets.Add(target);
            }

            return targets.AsReadOnly();
        }

        private static string BuildPlannerFailureSummary(AiExecutionPlan plan)
        {
            if (plan == null || plan.Evaluations == null || plan.Evaluations.Count == 0)
                return "No execution candidates were available.";

            var rejected = plan.Evaluations
                .Where(x => x != null)
                .Select(x => string.Join("; ", x.Reasons ?? new List<string>()))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(3)
                .ToArray();
            return rejected.Length == 0
                ? "All candidates were rejected."
                : "Candidate diagnostics: " + string.Join(" | ", rejected);
        }

        private static async Task<AIResponse> AwaitProviderResponseAsync(Task<AIResponse> providerTask, CancellationToken cancellationToken)
        {
            if (providerTask == null) throw new ArgumentNullException(nameof(providerTask));
            if (providerTask.IsCompleted)
                return await providerTask.ConfigureAwait(false);

            var cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            var completedTask = await Task.WhenAny(providerTask, cancellationTask).ConfigureAwait(false);
            if (completedTask == providerTask)
                return await providerTask.ConfigureAwait(false);

            providerTask.ContinueWith(
                task =>
                {
                    var ignored = task.Exception;
                },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            cancellationToken.ThrowIfCancellationRequested();
            throw new OperationCanceledException(cancellationToken);
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
