using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IProviderToolCallingAdapter
    {
        Task<AIResponse> SendWithToolsAsync(
            AiProvider provider,
            AiAgent agent,
            string apiKey,
            string systemPrompt,
            IReadOnlyList<AIMessage> messages,
            IReadOnlyList<AiTool> tools,
            CancellationToken cancellationToken);
    }
}
