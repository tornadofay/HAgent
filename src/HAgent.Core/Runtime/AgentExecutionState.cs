namespace HAgent.Runtime
{
    public enum AgentExecutionState
    {
        Created,
        Running,
        WaitingForIntervention,
        Paused,
        Succeeded,
        Failed,
        Cancelled
    }
}
