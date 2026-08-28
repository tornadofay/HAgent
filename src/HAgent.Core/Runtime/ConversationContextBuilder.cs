using System;
using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Deterministically selects a bounded conversation context without requiring a tokenizer.
    /// </summary>
    public sealed class ConversationContextBuilder
    {
        private readonly ConversationContextOptions _options;

        public ConversationContextBuilder(ConversationContextOptions options = null)
        {
            _options = options ?? new ConversationContextOptions();
            _options.Validate();
        }

        public ConversationContextOptions Options { get { return _options; } }

        public IReadOnlyList<AIMessage> Build(IReadOnlyList<AIMessage> messages)
        {
            if (messages == null || messages.Count == 0)
                return new List<AIMessage>().AsReadOnly();

            var maxMessages = Math.Max(1, _options.MaxMessages);
            var maxCharacters = Math.Max(1, _options.MaxCharacters);
            var leading = Math.Min(_options.PreserveLeadingMessages, messages.Count);
            var recent = Math.Min(_options.PreserveRecentMessages, messages.Count - leading);

            if (messages.Count <= maxMessages && CountCharacters(messages) <= maxCharacters)
                return new List<AIMessage>(messages).AsReadOnly();

            var selected = new List<AIMessage>();
            var selectedIndices = new HashSet<int>();

            for (var i = 0; i < leading && selected.Count < maxMessages; i++)
            {
                selected.Add(messages[i]);
                selectedIndices.Add(i);
            }

            var start = Math.Max(leading, messages.Count - recent);
            for (var i = start; i < messages.Count && selected.Count < maxMessages; i++)
            {
                if (!selectedIndices.Contains(i))
                {
                    selected.Add(messages[i]);
                    selectedIndices.Add(i);
                }
            }

            // Fill any remaining slots from the newest messages that fit the character budget.
            for (var i = messages.Count - 1; i >= 0 && selected.Count < maxMessages; i--)
            {
                if (selectedIndices.Contains(i)) continue;
                selected.Insert(leading, messages[i]);
                selectedIndices.Add(i);
            }

            selected.Sort(delegate(AIMessage x, AIMessage y)
            {
                return messages.IndexOf(x).CompareTo(messages.IndexOf(y));
            });

            while (CountCharacters(selected) > maxCharacters && selected.Count > 1)
            {
                var removeIndex = FindOldestRemovableIndex(selected, leading);
                if (removeIndex < 0) break;
                selected.RemoveAt(removeIndex);
            }

            return selected.AsReadOnly();
        }

        public int EstimateTokens(IReadOnlyList<AIMessage> messages)
        {
            // Deliberately approximate; providers may add a real tokenizer later.
            return (int)Math.Ceiling(CountCharacters(messages) / 4d);
        }

        private static int CountCharacters(IReadOnlyList<AIMessage> messages)
        {
            var total = 0;
            if (messages == null) return 0;
            foreach (var message in messages)
            {
                if (message == null) continue;
                total += (message.Role == null ? 0 : message.Role.Length);
                total += message.Content == null ? 0 : message.Content.Length;
            }
            return total;
        }

        private static int FindOldestRemovableIndex(List<AIMessage> messages, int leadingCount)
        {
            for (var i = 0; i < messages.Count; i++)
            {
                if (i >= leadingCount) return i;
            }
            return -1;
        }
    }
}
