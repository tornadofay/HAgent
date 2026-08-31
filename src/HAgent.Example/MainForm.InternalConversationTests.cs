using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestInternalConversationAsync(string input)
        {
            var store = await CreateConfiguredConversationStoreAsync(CancellationToken.None).ConfigureAwait(false);
            var sessionId = "example-internal-conversation-42-" + Guid.NewGuid().ToString("N");
            var agentId = "example-internal-conversation-agent-42";

            var snapshot = new ConversationSnapshot
            {
                SessionId = sessionId,
                AgentId = agentId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Messages = new List<AIMessage>
                {
                    new AIMessage("user", "Internal conversation test value: HAgent-conversation-42."),
                    new AIMessage("assistant", "Conversation persistence verification message.")
                }
            };

            await store.SaveAsync(snapshot, CancellationToken.None).ConfigureAwait(false);
            try
            {
                var tool = new HAgentInternalConversationTool(store);
                var result = await tool.ExecuteAsync(new ToolExecutionContext
                {
                    AgentId = agentId,
                    ToolCallId = "internal-conversation-call-42",
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    Arguments = new Dictionary<string, object>
                    {
                        { "sessionId", sessionId },
                        { "maxMessages", 1 }
                    },
                    CancellationToken = CancellationToken.None
                }).ConfigureAwait(false);

                if (!result.Succeeded)
                    throw new InvalidOperationException("Internal conversation tool failed: " + result.Error);
                if (result.Output.IndexOf("HAgent-conversation-42", StringComparison.Ordinal) < 0)
                    throw new InvalidOperationException("Expected conversation content was not returned.");
                if (result.Output.IndexOf("Additional messages omitted by maxMessages.", StringComparison.Ordinal) < 0)
                    throw new InvalidOperationException("Conversation maxMessages bound was not enforced.");

                var wrongAgent = await tool.ExecuteAsync(new ToolExecutionContext
                {
                    AgentId = "different-agent-42",
                    ToolCallId = "internal-conversation-call-43",
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    Arguments = new Dictionary<string, object>
                    {
                        { "sessionId", sessionId }
                    },
                    CancellationToken = CancellationToken.None
                }).ConfigureAwait(false);

                if (!wrongAgent.Succeeded || wrongAgent.Output.IndexOf("not available", StringComparison.OrdinalIgnoreCase) < 0)
                    throw new InvalidOperationException("Cross-agent conversation access was not rejected.");

                var missingSession = await tool.ExecuteAsync(new ToolExecutionContext
                {
                    AgentId = agentId,
                    ToolCallId = "internal-conversation-call-44",
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    Arguments = new Dictionary<string, object>
                    {
                        { "sessionId", "missing-conversation-42" }
                    },
                    CancellationToken = CancellationToken.None
                }).ConfigureAwait(false);

                if (!missingSession.Succeeded || missingSession.Output.IndexOf("Conversation not found", StringComparison.Ordinal) < 0)
                    throw new InvalidOperationException("Missing conversation handling was not deterministic.");

                Write("INTERNAL CONVERSATION",
                    "Contract test succeeded." + Environment.NewLine +
                    "Storage backend: " + (await LoadStorageOptionsAsync(CancellationToken.None).ConfigureAwait(false)).StorageType + Environment.NewLine +
                    "Tool: " + tool.Definition.Name + Environment.NewLine +
                    "Tool ID: " + tool.Definition.Id + Environment.NewLine +
                    "Session read: yes" + Environment.NewLine +
                    "Message bound: maxMessages=1" + Environment.NewLine +
                    "Cross-agent access: rejected" + Environment.NewLine +
                    "Missing session: handled" + Environment.NewLine +
                    "Write operations exposed by tool: none.");
            }
            finally
            {
                await store.DeleteAsync(sessionId, CancellationToken.None).ConfigureAwait(false);
            }
        }
    }
}
