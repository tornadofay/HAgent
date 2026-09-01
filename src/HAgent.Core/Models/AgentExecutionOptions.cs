using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class AgentExecutionOptions
    {
        public AgentExecutionOptions()
        {
            Timeout = TimeSpan.FromSeconds(120);
            MaxProviderAttempts = 3;
            MaxRetriesPerProvider = 0;
            RetryBaseDelay = TimeSpan.FromMilliseconds(250);
            SystemPromptLayers = new List<SystemPromptLayer>();
            HostContext = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public TimeSpan Timeout { get; set; }
        public int MaxProviderAttempts { get; set; }
        public int MaxRetriesPerProvider { get; set; }
        public TimeSpan RetryBaseDelay { get; set; }

        /// <summary>
        /// Runtime-instance overrides applied to a cloned execution snapshot.
        /// The persisted agent profile is never mutated.
        /// </summary>
        public AgentRuntimeOverrides RuntimeOverrides { get; set; }

        /// <summary>
        /// Runtime-instance identity captured for stale-result protection.
        /// These values are assigned by HAgentClient for instance-bound executions.
        /// </summary>
        public string RuntimeInstanceId { get; internal set; }
        public long RuntimeInstanceRevision { get; internal set; }

        /// <summary>
        /// Additional additive system-prompt layers supplied for this execution.
        /// These layers are composed with the provider and agent layers and never replace them.
        /// </summary>
        public IList<SystemPromptLayer> SystemPromptLayers { get; set; }

        /// <summary>
        /// Host-provided correlation identity. It is kept separate from HAgent execution and runtime-instance IDs.
        /// </summary>
        public string HostCorrelationId { get; set; }

        /// <summary>
        /// Bounded host context captured into the immutable execution snapshot.
        /// </summary>
        public IDictionary<string, string> HostContext { get; set; }
    }
}
