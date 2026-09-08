using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class DefaultAgentRuntime : IAgentRuntime, IInterventionControllableRuntime
    {
        private readonly IAiStore _store;
        private readonly ISecretStore _secrets;
        private readonly IReadOnlyList<IAiProviderAdapter> _adapters;
        private readonly IProviderRouter _router;
        private readonly IProviderErrorClassifier _errorClassifier;
        private readonly IExecutionPlanner _executionPlanner;
        private readonly IExecutionTargetCatalog _executionTargetCatalog;
        private readonly IExecutionAuditStore _auditStore;
        private readonly ExecutionAuditOptions _auditOptions;
        private readonly IAiPolicyEngine _configuredPolicyEngine;
        private readonly AiInterventionCoordinator _interventionCoordinator;

        public DefaultAgentRuntime(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IProviderRouter router = null,
            IProviderErrorClassifier errorClassifier = null,
            IExecutionAuditStore auditStore = null)
            : this(store, secrets, adapters, router, errorClassifier, auditStore, null, null, null, null, null)
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
            IExecutionPlanner executionPlanner = null,
            IExecutionTargetCatalog executionTargetCatalog = null,
            IAiPolicyEngine policyEngine = null,
            IAiInterventionWorkflow interventionWorkflow = null)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? throw new ArgumentNullException(nameof(adapters))).ToList().AsReadOnly();
            _router = router ?? new DefaultProviderRouter();
            _errorClassifier = errorClassifier ?? new DefaultProviderErrorClassifier();
            _executionPlanner = executionPlanner ?? new DefaultExecutionPlanner();
            _executionTargetCatalog = executionTargetCatalog ?? new DefaultExecutionTargetCatalog(
                new ProviderDiscoveryService(_adapters),
                _secrets);
            _auditStore = auditStore;
            _auditOptions = auditOptions ?? new ExecutionAuditOptions();
            _auditOptions.Validate();
            _configuredPolicyEngine = policyEngine;
            _interventionCoordinator = new AiInterventionCoordinator(
                interventionWorkflow ?? new InMemoryAiInterventionWorkflow());
        }

        public AiInterventionCoordinator InterventionCoordinator
        {
            get { return _interventionCoordinator; }
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
                    HostContext = options == null || options.HostContext == null
                        ? null
                        : options.HostContext.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase),
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
            var policyEngine = _configuredPolicyEngine;
            AiPolicySet effectivePolicy;
            if (policyEngine == null)
            {
                effectivePolicy = await _store.GetPolicySetAsync(cancellationToken).ConfigureAwait(false);
                if (effectivePolicy == null) effectivePolicy = new AiPolicySet();
                effectivePolicy.Validate();
                policyEngine = new DefaultAiPolicyEngine(effectivePolicy);
            }
            else
            {
                effectivePolicy = policyEngine.GetPolicySnapshot();
            }

            var snapshot = new AgentExecutionSnapshot(
                agent,
                providers,
                options.RuntimeOverrides,
                request.HostContext,
                request.Identity,
                effectivePolicy,
                null,
                request.Context);
            var messages = new List<AIMessage>(request.Messages).AsReadOnly();
            var execution = new AgentExecution(snapshot, messages);
            execution.HostCorrelationId = request.HostCorrelationId ?? string.Empty;
            execution.RuntimeInstanceId = options.RuntimeInstanceId;
            execution.RuntimeInstanceRevision = options.RuntimeInstanceRevision;
            _interventionCoordinator.RegisterExecution(execution);

            try
            {
                Notify(execution);

                using (var timeoutCts = new CancellationTokenSource(options.Timeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    timeoutCts.Token,
                    _interventionCoordinator.GetExecutionCancellationToken(execution.Id)))
                {
                    var token = linkedCts.Token;
                    try
                    {
                        await _interventionCoordinator.WaitIfPausedAsync(execution.Id, token).ConfigureAwait(false);

                        var selectionPolicy = request.ExecutionSelection == null
                            ? (snapshot.Agent.ExecutionSelection == null ? new AiExecutionSelectionPolicy() : snapshot.Agent.ExecutionSelection.Clone())
                            : request.ExecutionSelection.Clone();
                        var requirements = request.CapabilityRequirements == null
                            ? (snapshot.Agent.CapabilityRequirements == null ? new AiCapabilityRequirements() : snapshot.Agent.CapabilityRequirements.Clone())
                            : request.CapabilityRequirements.Clone();

                        if (request.StructuredOutput != null)
                            requirements.Require(AiCapability.StructuredOutput);

                        selectionPolicy.Validate();
                        var targets = await _executionTargetCatalog
                            .GetTargetsAsync(snapshot.Providers, token)
                            .ConfigureAwait(false);
                        var plan = _executionPlanner.Plan(targets, requirements, selectionPolicy);
                        if (!plan.HasSelection)
                        {
                            throw new InvalidOperationException(
                                "No compatible execution target was selected for agent '" + snapshot.Agent.Name + "'. " +
                                BuildPlannerFailureSummary(plan));
                        }

                        var selectedTarget = plan.SelectedTarget;
                        var policyContext = new AiPolicyEvaluationContext
                        {
                            Operation = "model.invoke",
                            ResourceType = "execution-target",
                            ResourceId = selectedTarget.Id,
                            AgentProfileId = snapshot.Agent.Id,
                            RuntimeInstanceId = execution.RuntimeInstanceId ?? string.Empty,
                            ExecutionId = execution.Id,
                            ProviderId = selectedTarget.ProviderId,
                            ExecutionTargetId = selectedTarget.Id,
                            CostStatus = selectedTarget.Cost,
                            RequestedCostPolicy = selectionPolicy.CostPolicy,
                            Identity = execution.Identity == null ? new AgentIdentityContext() : execution.Identity.Clone()
                        };
                        var policyDecision = policyEngine.Evaluate(policyContext);
                        execution.PolicyDecision = policyDecision;

                        if (policyDecision.IsDenied || policyDecision.RequiresApproval || policyDecision.IsDeferred)
                        {
                            var outcome = policyDecision.Outcome == AiPolicyOutcome.Deny
                                ? "denied"
                                : policyDecision.Outcome == AiPolicyOutcome.RequireApproval
                                    ? "requires approval"
                                    : "deferred";
                            throw new InvalidOperationException(
                                "Execution policy " + outcome + ". " +
                                (string.IsNullOrWhiteSpace(policyDecision.Reason) ? "No additional policy detail was provided." : policyDecision.Reason));
                        }

                        var candidates = _router
                            .OrderProviders(snapshot.Agent, snapshot.Providers)
                            .Where(x => string.Equals(x.Id, selectedTarget.ProviderId, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        if (candidates.Count == 0)
                            throw new InvalidOperationException("The selected execution target references a provider that is not available: " + selectedTarget.ProviderId);

                        var instructionProvider = candidates.FirstOrDefault(x => _adapters.Any(adapter => adapter.CanHandle(x))) ?? candidates[0];
                        var instructionComposition = AiInstructionComposer.Compose(
                            BuildExecutionInstructionSources(
                                instructionProvider,
                                snapshot.Agent,
                                options.SystemPromptLayers,
                                request.InstructionSources,
                                execution),
                            DateTimeOffset.UtcNow);
                        execution.CaptureInstructionSnapshot(instructionComposition.Snapshot);

                        execution.State = AgentExecutionState.Running;
                        execution.StartedAt = DateTimeOffset.UtcNow;
                        Notify(execution);

                        var effectiveSystemPrompt = instructionComposition.ComposedText;
                        var attempts = 0;
                        Exception lastError = null;
                        ProviderErrorKind lastErrorKind = ProviderErrorKind.Unknown;
                        string lastProviderName = string.Empty;
                        string lastModel = selectedTarget.ModelId;

                        foreach (var provider in candidates)
                        {
                            if (attempts >= options.MaxProviderAttempts) break;
                            await _interventionCoordinator.WaitIfPausedAsync(execution.Id, token).ConfigureAwait(false);
                            token.ThrowIfCancellationRequested();

                            var adapter = _adapters.FirstOrDefault(x => x.CanHandle(provider));
                            if (adapter == null) continue;

                            execution.LastProviderId = provider.Id;
                            var retries = 0;

                            while (true)
                            {
                                await _interventionCoordinator.WaitIfPausedAsync(execution.Id, token).ConfigureAwait(false);
                                token.ThrowIfCancellationRequested();
                                attempts++;
                                if (attempts > options.MaxProviderAttempts) break;

                                try
                                {
                                    var apiKey = string.IsNullOrWhiteSpace(provider.SecretId)
                                        ? string.Empty
                                        : await _secrets.GetAsync(provider.SecretId, token).ConfigureAwait(false);
                                    lastProviderName = provider.Name;

                                    var providerRequest = new ProviderExecutionRequest
                                    {
                                        Provider = provider,
                                        Agent = snapshot.Agent,
                                        ExecutionTarget = selectedTarget,
                                        ApiKey = apiKey,
                                        SystemPrompt = effectiveSystemPrompt,
                                        Messages = execution.Messages,
                                        Context = snapshot.Context,
                                        StructuredOutput = request.StructuredOutput
                                    };

                                    var response = await AwaitProviderResponseAsync(
                                        adapter.SendAsync(providerRequest, token),
                                        token).ConfigureAwait(false);
                                    await _interventionCoordinator.WaitIfPausedAsync(execution.Id, token).ConfigureAwait(false);
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
                                    {
                                        await _interventionCoordinator.WaitIfPausedAsync(execution.Id, token).ConfigureAwait(false);
                                        await Task.Delay(delay, token).ConfigureAwait(false);
                                    }
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
                        var interventionCancelled = _interventionCoordinator.IsInterventionCancellationRequested(execution.Id);
                        var cancellationFailureKind = cancellationToken.IsCancellationRequested || interventionCancelled
                            ? AgentExecutionFailureKind.Cancelled
                            : AgentExecutionFailureKind.Timeout;
                        Exception cancellationError = cancellationToken.IsCancellationRequested
                            ? new OperationCanceledException("Agent execution was cancelled by the caller.", cancellationToken)
                            : interventionCancelled
                                ? new OperationCanceledException("Agent execution was cancelled by intervention.", token)
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
                            var interventionCancelled = _interventionCoordinator.IsInterventionCancellationRequested(execution.Id);
                            var cancellationFailureKind = cancellationToken.IsCancellationRequested || interventionCancelled
                                ? AgentExecutionFailureKind.Cancelled
                                : AgentExecutionFailureKind.Timeout;
                            Exception cancellationError = cancellationToken.IsCancellationRequested
                                ? new OperationCanceledException("Agent execution was cancelled by the caller.", cancellationToken)
                                : interventionCancelled
                                    ? new OperationCanceledException("Agent execution was cancelled by intervention.", token)
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
            finally
            {
                _interventionCoordinator.UnregisterExecution(execution.Id);
            }
        }

        private static IReadOnlyList<AiInstructionSource> BuildExecutionInstructionSources(
            AiProvider provider,
            AiAgent agent,
            IEnumerable<SystemPromptLayer> executionLayers,
            IEnumerable<AiInstructionSource> hostSources,
            AgentExecution execution)
        {
            var sources = new List<AiInstructionSource>();
            if (hostSources != null)
            {
                foreach (var source in hostSources)
                {
                    if (source == null) continue;
                    var clone = source.Clone();
                    StampExecutionProvenance(clone, execution);
                    sources.Add(clone);
                }
            }

            var capturedAt = DateTimeOffset.UtcNow;
            if (provider != null && agent.UseProviderSystemPrompt && !string.IsNullOrWhiteSpace(provider.DefaultSystemPrompt))
            {
                sources.Add(CreateExecutionSource(
                    "provider",
                    "Provider",
                    AiInstructionSourceType.SystemPolicy,
                    AiInstructionAuthority.SystemPolicy,
                    AiInstructionTrustLevel.SystemTrusted,
                    100,
                    provider.DefaultSystemPrompt,
                    "provider",
                    provider.Id,
                    execution,
                    capturedAt));
            }

            if (!string.IsNullOrWhiteSpace(agent.SystemPrompt))
            {
                sources.Add(CreateExecutionSource(
                    "agent",
                    "Agent",
                    AiInstructionSourceType.Agent,
                    AiInstructionAuthority.Agent,
                    AiInstructionTrustLevel.HAgentTrusted,
                    200,
                    agent.SystemPrompt,
                    "agent",
                    agent.Id,
                    execution,
                    capturedAt));
            }

            if (executionLayers != null)
            {
                foreach (var layer in executionLayers)
                {
                    if (layer == null || string.IsNullOrWhiteSpace(layer.Text)) continue;
                    sources.Add(CreateExecutionSource(
                        string.IsNullOrWhiteSpace(layer.Id) ? Guid.NewGuid().ToString("N") : layer.Id,
                        string.IsNullOrWhiteSpace(layer.Name) ? "Execution instruction" : layer.Name,
                        ResolveInstructionSourceType(layer.Id),
                        ResolveInstructionAuthority(layer.Id),
                        AiInstructionTrustLevel.HAgentTrusted,
                        layer.Priority,
                        layer.Text,
                        "system-prompt-layer",
                        layer.Id,
                        execution,
                        capturedAt));
                }
            }

            return sources.AsReadOnly();
        }

        private static AiInstructionSource CreateExecutionSource(
            string id,
            string name,
            AiInstructionSourceType sourceType,
            AiInstructionAuthority authority,
            AiInstructionTrustLevel trust,
            int priority,
            string content,
            string sourceKind,
            string sourceId,
            AgentExecution execution,
            DateTimeOffset capturedAt)
        {
            var source = new AiInstructionSource
            {
                Id = id,
                Name = name,
                SourceType = sourceType,
                Authority = authority,
                TrustLevel = trust,
                Scope = new AiInstructionScope { ScopeType = "Execution", ScopeId = execution.Id },
                Lifecycle = AiInstructionLifecycleState.Active,
                Availability = AiInstructionAvailability.Available,
                Priority = priority,
                ConflictKey = string.Empty,
                Version = "1",
                CreatedAt = capturedAt,
                UpdatedAt = capturedAt,
                Content = content,
                Provenance = new AiInstructionProvenance
                {
                    SourceKind = sourceKind,
                    SourceId = sourceId,
                    SourceVersion = "1",
                    ExecutionId = execution.Id,
                    RuntimeInstanceId = execution.RuntimeInstanceId ?? string.Empty,
                    PrincipalId = execution.Identity == null ? string.Empty : execution.Identity.PrincipalId,
                    CapturedAt = capturedAt
                }
            };
            source.Validate();
            return source;
        }

        private static void StampExecutionProvenance(AiInstructionSource source, AgentExecution execution)
        {
            if (source == null || source.Provenance == null) return;
            source.Provenance.ExecutionId = execution.Id;
            source.Provenance.RuntimeInstanceId = execution.RuntimeInstanceId ?? string.Empty;
            source.Provenance.PrincipalId = execution.Identity == null ? string.Empty : execution.Identity.PrincipalId;
            source.Provenance.CapturedAt = DateTimeOffset.UtcNow;
        }

        private static AiInstructionSourceType ResolveInstructionSourceType(string id)
        {
            switch ((id ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "PROVIDER": return AiInstructionSourceType.SystemPolicy;
                case "AGENT": return AiInstructionSourceType.Agent;
                case "RUNTIME": return AiInstructionSourceType.RuntimeContext;
                case "CONTEXT": return AiInstructionSourceType.HostContext;
                default: return AiInstructionSourceType.SystemPolicy;
            }
        }

        private static AiInstructionAuthority ResolveInstructionAuthority(string id)
        {
            switch ((id ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "AGENT": return AiInstructionAuthority.Agent;
                case "RUNTIME": return AiInstructionAuthority.Runtime;
                case "CONTEXT": return AiInstructionAuthority.Runtime;
                default: return AiInstructionAuthority.SystemPolicy;
            }
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
                task => { var ignored = task.Exception; },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            cancellationToken.ThrowIfCancellationRequested();
            throw new OperationCanceledException(cancellationToken);
        }

        private static void ValidateOptions(AgentExecutionOptions options)
        {
            if (options.Timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(options.Timeout), "Timeout must be greater than zero.");
            if (options.MaxProviderAttempts <= 0) throw new ArgumentOutOfRangeException(nameof(options.MaxProviderAttempts), "MaxProviderAttempts must be greater than zero.");
            if (options.MaxRetriesPerProvider < 0) throw new ArgumentOutOfRangeException(nameof(options.MaxRetriesPerProvider), "MaxRetriesPerProvider cannot be negative.");
            if (options.RetryBaseDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(options.RetryBaseDelay), "RetryBaseDelay cannot be negative.");
        }

        private async Task PersistAuditAsync(AgentExecution execution)
        {
            if (_auditStore == null || !_auditOptions.Enabled) return;
            try
            {
                await _auditStore.AppendAsync(AgentExecutionAuditRecord.FromExecution(execution), CancellationToken.None).ConfigureAwait(false);
                await _auditStore.TrimAsync(_auditOptions.GetEffectiveMaxRecords(), CancellationToken.None).ConfigureAwait(false);
            }
            catch { }
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
            return TimeSpan.FromMilliseconds(Math.Min(baseDelay.TotalMilliseconds * multiplier, 30000d));
        }

        private void Notify(AgentExecution execution)
        {
            var handler = ExecutionChanged;
            if (handler != null) handler(this, new AgentExecutionEventArgs(execution));
        }
    }
}
