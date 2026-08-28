using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private Task RunContextBudgetTestAsync(string unused)
        {
            var messages = new List<AIMessage>();
            for (var i = 1; i <= 30; i++)
            {
                messages.Add(new AIMessage(
                    i % 2 == 0 ? "assistant" : "user",
                    "Message " + i + " - " + new string('x', 1000) + " - context budget test."));
            }

            var options = new ConversationContextOptions
            {
                MaxMessages = 10,
                MaxCharacters = 7000,
                PreserveLeadingMessages = 2,
                PreserveRecentMessages = 3
            };
            var builder = new ConversationContextBuilder(options);
            var selected = builder.Build(messages);

            var selectedIndexes = selected
                .Select(message => messages.IndexOf(message) + 1)
                .ToList();

            if (selected.Count > options.MaxMessages)
                throw new InvalidOperationException("The context builder exceeded MaxMessages.");
            if (builder.EstimateTokens(selected) <= 0)
                throw new InvalidOperationException("The context builder returned an invalid token estimate.");
            if (selected.Count == 0)
                throw new InvalidOperationException("The context builder returned no messages.");
            if (selectedIndexes[0] != 1 || selectedIndexes[1] != 2)
                throw new InvalidOperationException("The configured leading messages were not preserved.");
            if (selectedIndexes[selectedIndexes.Count - 1] != 30)
                throw new InvalidOperationException("The newest message was not preserved.");

            Write("CONTEXT BUDGET", "Original messages: " + messages.Count + Environment.NewLine +
                                    "Original characters: " + CountCharacters(messages) + Environment.NewLine +
                                    "Selected messages: " + selected.Count + Environment.NewLine +
                                    "Selected characters: " + CountCharacters(selected) + Environment.NewLine +
                                    "Estimated tokens: " + builder.EstimateTokens(selected) + Environment.NewLine +
                                    "Limits: maxMessages=" + options.MaxMessages + ", maxCharacters=" + options.MaxCharacters + Environment.NewLine +
                                    "Selected message numbers: " + string.Join(", ", selectedIndexes));
            return Task.CompletedTask;
        }

        private static int CountCharacters(IReadOnlyList<AIMessage> messages)
        {
            var total = 0;
            foreach (var message in messages)
            {
                if (message == null) continue;
                total += message.Role == null ? 0 : message.Role.Length;
                total += message.Content == null ? 0 : message.Content.Length;
            }
            return total;
        }
    }
}
