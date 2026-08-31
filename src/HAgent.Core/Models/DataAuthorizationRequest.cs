using System.Collections.Generic;

namespace HAgent.Models
{
    /// <summary>
    /// Host-supplied authorization context for one data operation.
    /// The query request remains separate and contains only data-access intent.
    /// </summary>
    public sealed class DataAuthorizationRequest
    {
        public DataAccessOperation Operation { get; set; }
        public string SourceId { get; set; }
        public string RuntimeIdentity { get; set; }
        public IReadOnlyDictionary<string, object> RuntimeContext { get; set; }
        public DataQueryRequest Query { get; set; }
    }
}
