using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiProviderAdapter
    {
        string Kind { get; }
        string DisplayName { get; }
        bool CanHandle(AiProvider provider);

        Task<AIResponse> SendAsync(
            AiProvider provider,
            AiAgent agent,
            string apiKey,
            string systemPrompt,
            IReadOnlyList<AIMessage> messages,
            CancellationToken cancellationToken);
    }
}
