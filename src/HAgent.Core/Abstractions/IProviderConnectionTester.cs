using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>Optional capability exposed by a provider adapter to verify connectivity and credentials.</summary>
    public interface IProviderConnectionTester
    {
        Task TestConnectionAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken = default(CancellationToken));
    }
}
