using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Read-only trusted tool for inspecting one HAgent-owned conversation by explicit session ID.
    /// </summary>
    public sealed class HAgentInternalConversationTool : IAgentTool
    {
        private const int DefaultMaxMessages = 20;
        private const int MaximumMaxMessages = 50;
        private const int MaximumContentLength = 4000;

        private readonly IConversationStore _conversationStore;

        public HAgentInternalConversationTool(IConversationStore conversationStore)
        {
            if (conversationStore == null) throw new ArgumentNullException(nameof(conversationStore));

            _conversationStore = conversationStore;
            Definition = CreateDefinition();
        }

        public AiTool Definition { get; private set; }

        public async Task<ToolExecutionResult> ExecuteAsync(ToolExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.CancellationToken.ThrowIfCancellationRequested();

            var sessionId = ResolveRequiredString(context.Arguments, "sessionId");
            var maxMessages = ResolveMaxMessages(context.Arguments);
            var snapshot = await _conversationStore.LoadAsync(sessionId, context.CancellationToken).ConfigureAwait(false);

            if (snapshot == null)
                return ToolExecutionResult.Success("Conversation not found for sessionId: " + Safe(sessionId));

            if (!string.IsNullOrWhiteSpace(context.AgentId)
                && !string.IsNullOrWhiteSpace(snapshot.AgentId)
                && !string.Equals(context.AgentId, snapshot.AgentId, StringComparison.Ordinal))
            {
                return ToolExecutionResult.Success("Conversation not available for the requesting agent.");
            }

            var result = new StringBuilder();
            result.AppendLine("HAgent conversation");
            result.AppendLine("Session ID: " + Safe(snapshot.SessionId));
            result.AppendLine("Agent ID: " + Safe(snapshot.AgentId));
            result.AppendLine("Created: " + snapshot.CreatedAt.ToString("O"));
            result.AppendLine("Updated: " + snapshot.UpdatedAt.ToString("O"));

            var messages = snapshot.Messages ?? new List<AIMessage>();
            var returned = Math.Min(messages.Count, maxMessages);
            result.AppendLine("Messages: " + messages.Count);
            result.AppendLine("Returned messages: " + returned);

            for (var i = 0; i < returned; i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var message = messages[i];
                result.AppendLine("Message | " + i + " | Role=" + Safe(message == null ? string.Empty : message.Role));
                result.AppendLine("Content | " + Limit(message == null ? string.Empty : message.Content));
                if (message != null && !string.IsNullOrWhiteSpace(message.ToolCallId))
                    result.AppendLine("ToolCallId | " + Safe(message.ToolCallId));
            }

            if (returned < messages.Count)
                result.AppendLine("Additional messages omitted by maxMessages.");

            return ToolExecutionResult.Success(result.ToString().TrimEnd());
        }

        private static string ResolveRequiredString(IReadOnlyDictionary<string, object> arguments, string key)
        {
            object value;
            if (arguments == null || !arguments.TryGetValue(key, out value) || value == null || string.IsNullOrWhiteSpace(Convert.ToString(value)))
                throw new ArgumentException(key + " is required.", nameof(arguments));

            return Convert.ToString(value).Trim();
        }

        private static int ResolveMaxMessages(IReadOnlyDictionary<string, object> arguments)
        {
            object rawValue;
            if (arguments == null || !arguments.TryGetValue("maxMessages", out rawValue) || rawValue == null)
                return DefaultMaxMessages;

            int value;
            try
            {
                value = Convert.ToInt32(rawValue);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("maxMessages must be an integer between 1 and " + MaximumMaxMessages + ".", nameof(arguments), ex);
            }

            if (value < 1 || value > MaximumMaxMessages)
                throw new ArgumentOutOfRangeException(nameof(arguments), "maxMessages must be between 1 and " + MaximumMaxMessages + ".");

            return value;
        }

        private static AiTool CreateDefinition()
        {
            return new AiTool
            {
                Id = "hagent.internal.conversation",
                Name = "HAgent Internal Conversation",
                Description = "Read-only bounded inspection of one HAgent-owned conversation identified by session ID.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"sessionId\":{\"type\":\"string\",\"minLength\":1},\"maxMessages\":{\"type\":\"integer\",\"minimum\":1,\"maximum\":50}},\"required\":[\"sessionId\"],\"additionalProperties\":false}",
                Category = "BuiltIn",
                Type = AiToolType.BuiltIn,
                IsBuiltIn = true,
                Enabled = true
            };
        }

        private static string Limit(string value)
        {
            var safe = Safe(value);
            if (safe.Length <= MaximumContentLength)
                return safe;

            return safe.Substring(0, MaximumContentLength) + "... [truncated]";
        }

        private static string Safe(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\r", " ").Replace("\n", " ");
        }
    }
}
