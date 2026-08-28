namespace HAgent.Models
{
    public enum AgentExecutionFailureKind
    {
        None = 0,
        Cancelled = 1,
        Timeout = 2,
        Configuration = 3,
        ProviderUnavailable = 4,
        ProviderFailed = 5,
        Unknown = 99
    }
}
