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
        }

        public TimeSpan Timeout { get; set; }
        public int MaxProviderAttempts { get; set; }
        public int MaxRetriesPerProvider { get; set; }
        public TimeSpan RetryBaseDelay { get; set; }

        /// <summary>
        /// Additional additive system-prompt layers supplied for this execution.
        /// These layers are composed with the provider and agent layers and never replace them.
        /// </summary>
        public IList<SystemPromptLayer> SystemPromptLayers { get; set; }
    }
}
