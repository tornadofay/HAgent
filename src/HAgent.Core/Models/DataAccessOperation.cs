namespace HAgent.Models
{
    /// <summary>
    /// Operation classes that can be independently authorized for a data source.
    /// </summary>
    public enum DataAccessOperation
    {
        Discovery,
        ProjectionQuery,
        Export,
        Write
    }
}
