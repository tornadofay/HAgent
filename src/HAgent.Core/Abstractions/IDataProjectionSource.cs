using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Executes an explicit bounded field projection against an application-owned data source.
    /// Implementations decide how fields are resolved and how data is accessed; no SQL is implied by this contract.
    /// </summary>
    public interface IDataProjectionSource
    {
        Task<DataProjectionResult> ProjectAsync(DataProjectionRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }
}
