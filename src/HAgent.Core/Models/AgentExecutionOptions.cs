using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
            HostContext = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        }

        public TimeSpan Timeout { get; set; }
        public int MaxProviderAttempts { get; set; }
        public int MaxRetriesPerProvider { get; set; }
        public TimeSpan RetryBaseDelay { get; set; }
        public AgentRuntimeOverrides RuntimeOverrides { get; set; }
        public string RuntimeInstanceId { get; internal set; }
        public long RuntimeInstanceRevision { get; internal set; }
        public IList<SystemPromptLayer> SystemPromptLayers { get; set; }
        public string HostCorrelationId { get; set; }
        public IReadOnlyDictionary<string, string> HostContext { get; set; }
    }
}
