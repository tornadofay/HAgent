using System;

namespace HAgent.Models
{
    /// <summary>
    /// Coarse-grained data-access policy for a host-owned data source.
    /// This policy is separate from schema membership and host authorization.
    /// </summary>
    public sealed class DataAccessPermissions
    {
        public bool Discovery { get; set; }
        public bool ProjectionQuery { get; set; }
        public bool Export { get; set; }
        public bool Write { get; set; }

        public DataAccessPermissions()
        {
            Discovery = false;
            ProjectionQuery = false;
            Export = false;
            Write = false;
        }

        public DataAccessPermissions Clone()
        {
            return new DataAccessPermissions
            {
                Discovery = Discovery,
                ProjectionQuery = ProjectionQuery,
                Export = Export,
                Write = Write
            };
        }

        public void DemandProjectionQuery()
        {
            if (!ProjectionQuery)
                throw new UnauthorizedAccessException("Structured data projection/query is not permitted by the current data-access policy.");
        }
    }
}
