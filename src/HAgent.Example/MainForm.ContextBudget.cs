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
        private async Task TestContextBudgetAsync(string unused)
        {
            var messages = new List<AIMessage>();
            for (var i = 1; i <= 30; i++)
            {
                messages.Add(new AIMessage(
                    i % 2 == 0 ? "assistant" : "user",
                    "Message " + i + " - " + new string('x', 1200)));
            }

            var options = new ConversationContextOptions
            {
                MaxMessages = 10,
                MaxCharacters = 7000,
                PreserveLeadingMessages = 2,
                PreserveRecentMessages = 4
            };
            var builder = new ConversationContextBuilder(options);
            var selected = builder.Build(messages);

            var selectedNumbers = selected
                .Select(x => ExtractMessageNumber(x.Content))
                .ToList();
            var originalCharacters = messages.Sum(x => (x.Content ?? string.Empty).Length);
            var selectedCharacters = selected.Sum(x => (x.Content ?? string.Empty).Length);

            if (selected.Count >= messages.Count)
                throw new InvalidOperationException("The context builder did not reduce the oversized message list.");
            if (selectedCharacters > options.MaxCharacters && selected.Count > 1)
                throw new InvalidOperationException("The context builder exceeded the configured character budget.");
            if (selectedNumbers.Count < 2 || selectedNumbers[0] != 1 || selectedNumbers[1] != 2)
                throw new InvalidOperationException("The configured leading messages were not preserved.");
            if (selectedNumbers[selectedNumbers.Count - 1] != 30)
                throw new InvalidOperationException("The newest message was not preserved.");

            var estimatedTokens = builder.EstimateTokens(selected);
            Write("CONTEXT BUDGET", "Original messages: " + messages.Count + Environment.NewLine +
                                    "Original characters: " + originalCharacters + Environment.NewLine +
                                    "Selected messages: " + selected.Count + Environment.NewLine +
                                    "Selected characters: " + selectedCharacters + Environment.NewLine +
                                    "Estimated tokens: " + estimatedTokens + Environment.NewLine +
                                    "Limits: maxMessages=" + options.MaxMessages + ", maxCharacters=" + options.MaxCharacters + Environment.NewLine +
                                    "Selected message numbers: " + string.Join(", ", selectedNumbers));

            await Task.CompletedTask;
        }

        private static int ExtractMessageNumber(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return -1;
            const string prefix = "Message ";
            if (!content.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return -1;
            var end = content.IndexOf(' ', prefix.Length);
            var value = end > prefix.Length ? content.Substring(prefix.Length, end - prefix.Length) : content.Substring(prefix.Length);
            int number;
            return int.TryParse(value, out number) ? number : -1;
        }
    }
}
