using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Persistence boundary for conversations/sessions.
    /// </summary>
    public interface IConversationStore
    {
        Task SaveAsync(ConversationSnapshot conversation, CancellationToken cancellationToken = default(CancellationToken));
        Task<ConversationSnapshot> LoadAsync(string sessionId, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(string sessionId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
