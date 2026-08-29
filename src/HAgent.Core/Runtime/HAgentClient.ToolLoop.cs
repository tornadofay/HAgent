using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class ToolLoopResult
    {
        public AIResponse Response { get; private set; }
        public int Turns { get; private set; }
        public int ToolCallsExecuted { get; private set; }

        public ToolLoopResult(AIResponse response, int turns, int toolCallsExecuted)
        {
            Response = response;
            Turns = turns;
            ToolCallsExecuted = toolCallsExecuted;
        }
    }

    public sealed partial class HAgentClient
    {
        public async Task<ToolLoopResult> RunToolLoopAsync(
            string agentId,
            string message,
            int maxTurns = 8,
            int maxToolCalls = 16,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
            if (maxTurns < 1) throw new ArgumentOutOfRangeException(nameof(maxTurns));
            if (maxToolCalls < 1) throw new ArgumentOutOfRangeException(nameof(maxToolCalls));

            var agents = await _store.GetAgentsAsync(cancellationToken).ConfigureAwait(false);
            var agent = agents.FirstOrDefault(x => string.Equals(x.Id, agentId, StringComparison.OrdinalIgnoreCase));
            if (agent == null) throw new InvalidOperationException("Agent was not found: " + agentId);

            var definitions = GetToolDefinitions();
            var enabledDefinitions = new List<AiTool>();
            if (agent.ToolIds != null && agent.ToolIds.Count > 0)
            {
                foreach (var id in agent.ToolIds)
                {
                    var definition = definitions.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
                    if (definition != null && definition.Enabled) enabledDefinitions.Add(definition);
                }
            }

            if (enabledDefinitions.Count == 0)
                throw new InvalidOperationException("Agent '" + agent.Name + "' has no enabled registered tools assigned to it.");

            var messages = new List<AIMessage> { new AIMessage("user", message) };
            AIResponse response = null;
            var executed = 0;

            for (var turn = 1; turn <= maxTurns; turn++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                response = await SendWithToolsAsync(agentId, messages, enabledDefinitions, cancellationToken).ConfigureAwait(false);

                if (response.ToolCalls == null || response.ToolCalls.Count == 0)
                    return new ToolLoopResult(response, turn, executed);

                if (executed + response.ToolCalls.Count > maxToolCalls)
                {
                    throw new InvalidOperationException("Tool loop exceeded the maximum allowed tool calls (" + maxToolCalls + ").");
                }

                messages.Add(new AIMessage
                {
                    Role = "assistant",
                    Content = response.Text,
                    ToolCalls = response.ToolCalls
                });

                foreach (var call in response.ToolCalls)
                {
                    var definition = enabledDefinitions.FirstOrDefault(x => string.Equals(x.Name, call.Name, StringComparison.OrdinalIgnoreCase));
                    ToolExecutionResult result;
                    if (definition == null)
                    {
                        result = ToolExecutionResult.Failure("The requested tool is not enabled for this agent: " + call.Name);
                    }
                    else
                    {
                        IReadOnlyDictionary<string, object> arguments;
                        IDictionary<string, object> parsedArguments;
                        string parseError;
                        if (!ToolArgumentParser.TryParseObject(call.ArgumentsJson, out parsedArguments, out parseError))
                        {
                            result = ToolExecutionResult.Failure("Tool arguments are not valid JSON: " + parseError);
                        }
                        else
                        {
                            arguments = parsedArguments as IReadOnlyDictionary<string, object>;
                            if (arguments == null)
                            {
                                var normalizedArguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                                foreach (var pair in parsedArguments)
                                    normalizedArguments[pair.Key] = pair.Value;
                                arguments = normalizedArguments;
                            }

                            result = await ExecuteToolAsync(agentId, definition.Id, call.Id, arguments, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    messages.Add(new AIMessage
                    {
                        Role = "tool",
                        ToolCallId = call.Id,
                        Content = result.Succeeded ? result.Output : "Tool error: " + result.Error
                    });
                    executed++;
                }
            }

            throw new InvalidOperationException("Tool loop exceeded the maximum allowed turns (" + maxTurns + ").");
        }
    }
}
