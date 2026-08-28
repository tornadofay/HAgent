using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed partial class HAgentClient
    {
        private readonly IToolRegistry _toolRegistry = new InMemoryToolRegistry();

        public IReadOnlyList<AiTool> GetToolDefinitions()
        {
            return _toolRegistry.GetDefinitions();
        }

        public bool RegisterTool(IAgentTool tool)
        {
            if (tool == null) throw new ArgumentNullException(nameof(tool));
            if (tool.Definition == null || string.IsNullOrWhiteSpace(tool.Definition.Id))
                throw new ArgumentException("A tool must have a definition with an id.", nameof(tool));

            _toolRegistry.Register(tool);
            return true;
        }

        public void UnregisterTool(string toolId)
        {
            _toolRegistry.Unregister(toolId);
        }

        public bool TryGetTool(string toolId, out IAgentTool tool)
        {
            return _toolRegistry.TryGet(toolId, out tool);
        }

        public async Task<ToolExecutionResult> ExecuteToolAsync(
            string agentId,
            string toolId,
            string toolCallId,
            IReadOnlyDictionary<string, object> arguments,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(agentId))
                throw new ArgumentException("Agent id is required.", nameof(agentId));
            if (string.IsNullOrWhiteSpace(toolId))
                throw new ArgumentException("Tool id is required.", nameof(toolId));

            IAgentTool tool;
            if (!_toolRegistry.TryGet(toolId, out tool))
                throw new InvalidOperationException("Tool was not found: " + toolId);
            if (!tool.Definition.Enabled)
                return ToolExecutionResult.Failure("Tool is disabled: " + tool.Definition.Name);

            var context = new ToolExecutionContext
            {
                AgentId = agentId,
                ToolCallId = toolCallId ?? string.Empty,
                Arguments = arguments ?? new Dictionary<string, object>(),
                CancellationToken = cancellationToken
            };

            return await tool.ExecuteAsync(context).ConfigureAwait(false);
        }
    }
}
