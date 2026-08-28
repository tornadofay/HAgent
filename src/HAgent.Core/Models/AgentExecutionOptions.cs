using System;

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
        }

        public TimeSpan Timeout { get; set; }
        public int MaxProviderAttempts { get; set; }
        public int MaxRetriesPerProvider { get; set; }
        public TimeSpan RetryBaseDelay { get; set; }
    }
}
