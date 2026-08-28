using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IToolRegistry
    {
        IReadOnlyList<AiTool> GetDefinitions();
        bool TryGet(string toolId, out IAgentTool tool);
        void Register(IAgentTool tool);
        void Unregister(string toolId);
    }
}
