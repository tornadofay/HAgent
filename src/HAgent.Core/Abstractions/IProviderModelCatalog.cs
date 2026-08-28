using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>Optional capability exposed by a provider adapter when the service can enumerate models.</summary>
    public interface IProviderModelCatalog
    {
        Task<IReadOnlyList<string>> GetModelsAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken = default(CancellationToken));
    }
}
