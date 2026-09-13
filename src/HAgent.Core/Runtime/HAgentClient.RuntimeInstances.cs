using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed partial class HAgentClient
    {
        /// <summary>
        /// Executes a live runtime instance without mutating its persistent profile.
        /// The execution captures the instance execution and lifecycle revisions so the host can identify stale late results.
        /// Shutdown cancels instance-bound work in addition to preventing new work.
        /// </summary>
        public Task<AgentExecution> ExecuteAsync(
            AgentRuntimeInstance instance,
            string message,
            AgentExecutionOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required.", nameof(message));

            var request = new AgentExecutionRequest
            {
                AgentId = instance.ProfileId,
                Messages = new List<AIMessage> { new AIMessage("user", message) }.AsReadOnly(),
                Options = options ?? new AgentExecutionOptions()
            };

            return ExecuteAsync(instance, request, cancellationToken);
        }

        /// <summary>
        /// Executes a live runtime instance using the canonical provider-neutral execution request.
        /// The request describes execution input while the runtime instance supplies execution identity,
        /// lifecycle, revisions, overrides, shutdown, and private-memory ownership.
        /// </summary>
        public async Task<AgentExecution> ExecuteAsync(
            AgentRuntimeInstance instance,
            AgentExecutionRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();

            if (!string.Equals(request.AgentId, instance.ProfileId, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Execution request agent ID must match the runtime instance profile ID.",
                    nameof(request));
            }

            if (instance.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Runtime agent instance is not active: " + instance.InstanceId);

            long lifecycleRevision;
            var revision = instance.BeginExecution(out lifecycleRevision);
            var sourceOptions = request.Options ?? new AgentExecutionOptions();
            var effective = CloneOptions(sourceOptions);
            effective.RuntimeOverrides = instance.Overrides;
            effective.RuntimeInstanceId = instance.InstanceId;
            effective.RuntimeInstanceRevision = revision;
            effective.RuntimeLifecycleRevision = lifecycleRevision;

            var effectiveRequest = new AgentExecutionRequest
            {
                AgentId = request.AgentId,
                Messages = request.Messages,
                HostCorrelationId = request.HostCorrelationId,
                HostContext = request.HostContext,
                Options = effective,
                StructuredOutput = request.StructuredOutput,
                Identity = request.Identity,
                Context = request.Context,
                InstructionSources = request.InstructionSources,
                ExecutionSelection = request.ExecutionSelection,
                CapabilityRequirements = request.CapabilityRequirements
            };

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                instance.ShutdownToken))
            {
                var execution = await _runtime.ExecuteAsync(
                    effectiveRequest,
                    linkedCts.Token).ConfigureAwait(false);

                // The runtime options carry the admission snapshot. Preserve that exact value on
                // the returned execution so result authority checks compare against the revision
                // captured by BeginExecution rather than the runtime's later state.
                if (execution != null)
                    execution.RuntimeLifecycleRevision = lifecycleRevision;

                return execution;
            }
        }

        private static AgentExecutionOptions CloneOptions(AgentExecutionOptions source)
        {
            var clone = new AgentExecutionOptions
            {
                Timeout = source.Timeout,
                MaxProviderAttempts = source.MaxProviderAttempts,
                MaxRetriesPerProvider = source.MaxRetriesPerProvider,
                RetryBaseDelay = source.RetryBaseDelay,
                RuntimeOverrides = source.RuntimeOverrides,
                RuntimeInstanceId = source.RuntimeInstanceId,
                RuntimeInstanceRevision = source.RuntimeInstanceRevision,
                RuntimeLifecycleRevision = source.RuntimeLifecycleRevision,
                SystemPromptLayers = source.SystemPromptLayers == null
                    ? new List<SystemPromptLayer>()
                    : new List<SystemPromptLayer>(source.SystemPromptLayers)
            };
            return clone;
        }
    }
}