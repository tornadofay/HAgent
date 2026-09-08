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

        public Task<ToolExecutionResult> ExecuteToolAsync(
            string agentId,
            string toolId,
            string toolCallId,
            IReadOnlyDictionary<string, object> arguments,
            CancellationToken cancellationToken = default(CancellationToken),
            string hostCorrelationId = null)
        {
            return ExecuteToolAsync(
                agentId,
                toolId,
                toolCallId,
                arguments,
                cancellationToken,
                hostCorrelationId,
                null);
        }

        public async Task<ToolExecutionResult> ExecuteToolAsync(
            string agentId,
            string toolId,
            string toolCallId,
            IReadOnlyDictionary<string, object> arguments,
            CancellationToken cancellationToken,
            string hostCorrelationId,
            AgentIdentityContext identity)
        {
            var policyEngine = await ResolvePolicyEngineAsync(cancellationToken).ConfigureAwait(false);
            return await ExecuteToolInternalAsync(
                agentId,
                toolId,
                toolCallId,
                arguments,
                cancellationToken,
                hostCorrelationId,
                identity,
                policyEngine).ConfigureAwait(false);
        }

        private async Task<ToolExecutionResult> ExecuteToolInternalAsync(
            string agentId,
            string toolId,
            string toolCallId,
            IReadOnlyDictionary<string, object> arguments,
            CancellationToken cancellationToken,
            string hostCorrelationId,
            AgentIdentityContext identity,
            IAiPolicyEngine policyEngine)
        {
            if (string.IsNullOrWhiteSpace(agentId))
                throw new ArgumentException("Agent id is required.", nameof(agentId));
            if (string.IsNullOrWhiteSpace(toolId))
                throw new ArgumentException("Tool id is required.", nameof(toolId));
            if (policyEngine == null)
                throw new ArgumentNullException(nameof(policyEngine));

            var correlationId = Guid.NewGuid().ToString("N");
            var startedAt = DateTimeOffset.UtcNow;
            var effectiveIdentity = identity == null ? new AgentIdentityContext() : identity.Clone();
            effectiveIdentity.Validate();

            IAgentTool tool;
            if (!_toolRegistry.TryGet(toolId, out tool))
                return CreateFailure("Tool was not found: " + toolId, correlationId, hostCorrelationId, agentId, toolId, toolCallId, startedAt, effectiveIdentity);
            if (!tool.Definition.Enabled)
                return CreateFailure("Tool is disabled: " + tool.Definition.Name, correlationId, hostCorrelationId, agentId, toolId, toolCallId, startedAt, effectiveIdentity);

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
                    hostCorrelationId,
                    agentId,
                    toolId,
                    toolCallId,
                    startedAt,
                    effectiveIdentity);
            }

            var policyDecision = policyEngine.Evaluate(new AiPolicyEvaluationContext
            {
                Operation = "tool.invoke",
                ResourceType = "tool",
                ResourceId = toolId,
                AgentProfileId = agentId,
                ToolId = toolId,
                CostStatus = AiCostStatus.Unknown,
                RequestedCostPolicy = AiCostPolicy.NoRestriction,
                Identity = effectiveIdentity
            });

            if (policyDecision == null)
                throw new InvalidOperationException("The policy engine returned no tool execution decision.");

            if (policyDecision.IsDenied || policyDecision.RequiresApproval || policyDecision.IsDeferred)
            {
                var blocked = CreateFailure(
                    BuildPolicyFailure(policyDecision),
                    correlationId,
                    hostCorrelationId,
                    agentId,
                    toolId,
                    toolCallId,
                    startedAt,
                    effectiveIdentity);
                blocked.PolicyDecision = policyDecision;
                return blocked;
            }

            ToolExecutionResult result;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                result = await tool.ExecuteAsync(new ToolExecutionContext
                {
                    CorrelationId = correlationId,
                    HostCorrelationId = hostCorrelationId ?? string.Empty,
                    AgentId = agentId,
                    ToolId = toolId,
                    ToolCallId = toolCallId ?? string.Empty,
                    Identity = effectiveIdentity,
                    Arguments = validation.Arguments,
                    CancellationToken = cancellationToken
                }).ConfigureAwait(false);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                result = ToolExecutionResult.Failure(ex.Message);
            }

            if (result == null)
                result = ToolExecutionResult.Failure("Tool returned no execution result.");

            result.CorrelationId = correlationId;
            result.HostCorrelationId = hostCorrelationId ?? string.Empty;
            result.AgentId = agentId;
            result.ToolId = toolId;
            result.ToolCallId = toolCallId ?? string.Empty;
            result.Identity = effectiveIdentity;
            result.PolicyDecision = policyDecision;
            result.StartedAt = startedAt;
            result.CompletedAt = DateTimeOffset.UtcNow;
            return result;
        }

        private async Task<IAiPolicyEngine> ResolvePolicyEngineAsync(CancellationToken cancellationToken)
        {
            if (_configuredPolicyEngine != null)
                return _configuredPolicyEngine;

            var policy = await _store.GetPolicySetAsync(cancellationToken).ConfigureAwait(false);
            if (policy == null)
                policy = new AiPolicySet();
            policy.Validate();
            return new DefaultAiPolicyEngine(policy);
        }

        private static string BuildPolicyFailure(AiPolicyDecision decision)
        {
            var reason = string.IsNullOrWhiteSpace(decision.Reason) ? "No policy reason was supplied." : decision.Reason;
            if (decision.RequiresApproval)
                return "Tool execution requires approval by policy. " + reason;
            if (decision.IsDeferred)
                return "Tool execution was deferred by policy. " + reason;
            return "Tool execution was denied by policy. " + reason;
        }

        private static ToolExecutionResult CreateFailure(
            string error,
            string correlationId,
            string hostCorrelationId,
            string agentId,
            string toolId,
            string toolCallId,
            DateTimeOffset startedAt,
            AgentIdentityContext identity)
        {
            var result = ToolExecutionResult.Failure(error);
            result.CorrelationId = correlationId;
            result.HostCorrelationId = hostCorrelationId ?? string.Empty;
            result.AgentId = agentId;
            result.ToolId = toolId;
            result.ToolCallId = toolCallId ?? string.Empty;
            result.Identity = identity == null ? new AgentIdentityContext() : identity.Clone();
            result.StartedAt = startedAt;
            result.CompletedAt = DateTimeOffset.UtcNow;
            return result;
        }
    }
}
