using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAgentTool
    {
        AiTool Definition { get; }
        Task<ToolExecutionResult> ExecuteAsync(ToolExecutionContext context);
    }
}
