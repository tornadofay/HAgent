using System;

namespace HAgent.Models
{
    public sealed class AgentExecutionEventArgs : EventArgs
    {
        public AgentExecutionEventArgs(AgentExecution execution)
        {
            Execution = execution ?? throw new ArgumentNullException(nameof(execution));
        }

        public AgentExecution Execution { get; private set; }
    }
}
