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
        /// The execution captures the instance revision so the host can identify stale late results.
        /// </summary>
        public Task<AgentExecution> ExecuteAsync(
            AgentRuntimeInstance instance,
            string message,
            AgentExecutionOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (instance.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Runtime agent instance is retired: " + instance.InstanceId);
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required.", nameof(message));

            var revision = instance.BeginExecution();
            var effective = options == null ? new AgentExecutionOptions() : CloneOptions(options);
            effective.RuntimeOverrides = instance.Overrides;
            effective.RuntimeInstanceId = instance.InstanceId;
            effective.RuntimeInstanceRevision = revision;
            return _runtime.ExecuteAsync(instance.ProfileId, message, effective, cancellationToken);
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
                SystemPromptLayers = source.SystemPromptLayers == null
                    ? new List<SystemPromptLayer>()
                    : new List<SystemPromptLayer>(source.SystemPromptLayers)
            };
            return clone;
        }
    }
}
