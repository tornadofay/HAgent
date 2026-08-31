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

            var schemaValidation = ToolSchemaValidator.ValidateSchema(tool.Definition);
            if (!schemaValidation.IsValid)
                throw new ArgumentException("Tool input schema is invalid: " + string.Join(" ", schemaValidation.Errors.Select(x => "[" + x + "]")), nameof(tool));

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

            var correlationId = Guid.NewGuid().ToString("N");
            var startedAt = DateTimeOffset.UtcNow;

            IAgentTool tool;
            if (!_toolRegistry.TryGet(toolId, out tool))
                return CreateFailure("Tool was not found: " + toolId, correlationId, agentId, toolId, toolCallId, startedAt);
            if (!tool.Definition.Enabled)
                return CreateFailure("Tool is disabled: " + tool.Definition.Name, correlationId, agentId, toolId, toolCallId, startedAt);

            var source = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (arguments != null)
            {
                foreach (var pair in arguments)
                    source[pair.Key] = pair.Value;
            }

            var validation = ToolSchemaValidator.Validate(tool.Definition, source);
            if (!validation.IsValid)
            {
                return CreateFailure(
                    "Tool arguments failed schema validation: " + string.Join(" ", validation.Errors.Select(x => "[" + x + "]")),
                    correlationId,
                    agentId,
                    toolId,
                    toolCallId,
                    startedAt);
            }

            var context = new ToolExecutionContext
            {
                CorrelationId = correlationId,
                AgentId = agentId,
                ToolId = toolId,
                ToolCallId = toolCallId ?? string.Empty,
                Arguments = validation.Arguments,
                CancellationToken = cancellationToken
            };

            ToolExecutionResult result;
            try
            {
                result = await tool.ExecuteAsync(context).ConfigureAwait(false);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                result = ToolExecutionResult.Failure(ex.Message);
            }

            if (result == null)
                result = ToolExecutionResult.Failure("Tool returned no execution result.");

            result.CorrelationId = correlationId;
            result.AgentId = agentId;
            result.ToolId = toolId;
            result.ToolCallId = toolCallId ?? string.Empty;
            result.StartedAt = startedAt;
            result.CompletedAt = DateTimeOffset.UtcNow;
            return result;
        }

        private static ToolExecutionResult CreateFailure(
            string error,
            string correlationId,
            string agentId,
            string toolId,
            string toolCallId,
            DateTimeOffset startedAt)
        {
            var result = ToolExecutionResult.Failure(error);
            result.CorrelationId = correlationId;
            result.AgentId = agentId;
            result.ToolId = toolId;
            result.ToolCallId = toolCallId ?? string.Empty;
            result.StartedAt = startedAt;
            result.CompletedAt = DateTimeOffset.UtcNow;
            return result;
        }
    }
}
