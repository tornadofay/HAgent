using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Optional provider capability for incremental responses.
    /// The final AIResponse remains the canonical completed result.
    /// </summary>
    public interface IProviderStreamingAdapter
    {
        Task<AIResponse> SendStreamingAsync(
            ProviderExecutionRequest request,
            CancellationToken cancellationToken);
    }
}
