using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IConversationMemoryPolicy
    {
        IReadOnlyList<string> ExtractMemories(AIMessage userMessage, AIMessage assistantMessage);
    }
}
