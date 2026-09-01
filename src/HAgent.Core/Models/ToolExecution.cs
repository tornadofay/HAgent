using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Models
{
    public sealed class ToolExecutionContext
    {
        public string CorrelationId { get; set; }
        public string HostCorrelationId { get; set; }
        public string AgentId { get; set; }
        public string ToolId { get; set; }
        public string ToolCallId { get; set; }
        public IReadOnlyDictionary<string, object> Arguments { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public ToolExecutionContext()
        {
            CorrelationId = string.Empty;
            HostCorrelationId = string.Empty;
            AgentId = string.Empty;
            ToolId = string.Empty;
            ToolCallId = string.Empty;
            Arguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public sealed class ToolExecutionResult
    {
        public bool Succeeded { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public string CorrelationId { get; set; }
        public string HostCorrelationId { get; set; }
        public string AgentId { get; set; }
        public string ToolId { get; set; }
        public string ToolCallId { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public TimeSpan? Duration
        {
            get { return CompletedAt.HasValue ? CompletedAt.Value - StartedAt : (TimeSpan?)null; }
        }

        public ToolExecutionResult()
        {
            Output = string.Empty;
            Error = string.Empty;
            CorrelationId = string.Empty;
            HostCorrelationId = string.Empty;
            AgentId = string.Empty;
            ToolId = string.Empty;
            ToolCallId = string.Empty;
            StartedAt = DateTimeOffset.UtcNow;
        }

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
