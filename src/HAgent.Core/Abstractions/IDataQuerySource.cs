using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Executes structured data-query intent against an application-owned data source.
    /// Implementations remain responsible for schema validation, authorization, and physical execution.
    /// </summary>
    public interface IDataQuerySource
    {
        Task<DataQueryResult> QueryAsync(DataQueryRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }
}
