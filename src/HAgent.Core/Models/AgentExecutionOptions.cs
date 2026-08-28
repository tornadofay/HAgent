using System;

namespace HAgent.Models
{
    public sealed class AgentExecutionOptions
    {
        public AgentExecutionOptions()
        {
            Timeout = TimeSpan.FromSeconds(120);
            MaxProviderAttempts = 3;
        }

        public TimeSpan Timeout { get; set; }
        public int MaxProviderAttempts { get; set; }
    }
}
