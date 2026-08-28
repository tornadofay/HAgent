namespace HAgent.Models
{
    public enum ProviderErrorKind
    {
        Unknown = 0,
        Authentication = 1,
        InvalidRequest = 2,
        RateLimited = 3,
        Unavailable = 4,
        Transient = 5,
        Cancelled = 6
    }
}
