using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Models
{
    public sealed class ToolExecutionContext
    {
        public string AgentId { get; set; }
        public string ToolCallId { get; set; }
        public IReadOnlyDictionary<string, object> Arguments { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }

    public sealed class ToolExecutionResult
    {
        public bool Succeeded { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }

        public static ToolExecutionResult Success(string output)
        {
            return new ToolExecutionResult { Succeeded = true, Output = output ?? string.Empty };
        }

        public static ToolExecutionResult Failure(string error)
        {
            return new ToolExecutionResult { Succeeded = false, Error = error ?? string.Empty };
        }
    }

    public delegate Task<ToolExecutionResult> ToolExecutionHandler(ToolExecutionContext context);
}
