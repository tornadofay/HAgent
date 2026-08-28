using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Abstractions
{
    public interface ISecretStore
    {
        Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken));
        Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken));
    }
}
