using System;
using System.Collections.Generic;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Conservative, dependency-free memory policy. It only promotes a user message
    /// when the user explicitly asks HAgent to remember or retain information.
    /// </summary>
    public sealed class ExplicitConversationMemoryPolicy : IConversationMemoryPolicy
    {
        private static readonly string[] EnglishTriggers =
        {
            "remember this:",
            "remember this ",
            "remember that ",
            "please remember ",
            "don't forget ",
            "do not forget ",
            "keep this in mind:",
            "keep in mind:"
        };

        public IReadOnlyList<string> ExtractMemories(AIMessage userMessage, AIMessage assistantMessage)
        {
            var result = new List<string>();
            if (userMessage == null || string.IsNullOrWhiteSpace(userMessage.Content))
                return result.AsReadOnly();

            var text = userMessage.Content.Trim();
            foreach (var trigger in EnglishTriggers)
            {
                if (!text.StartsWith(trigger, StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = text.Substring(trigger.Length).Trim();
                value = value.Trim(' ', ':', '"', '\'');
                value = value.TrimEnd('.', '!', '?').Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result.Add(value);
                    break;
                }
            }

            return result.AsReadOnly();
        }
    }
}
