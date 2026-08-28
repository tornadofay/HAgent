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

            var selected = new List<IndexedMessage>();
            var selectedIndices = new HashSet<int>();

            for (var i = 0; i < leading && selected.Count < maxMessages; i++)
            {
                selected.Add(new IndexedMessage(i, messages[i]));
                selectedIndices.Add(i);
            }

            var recentStart = Math.Max(leading, messages.Count - recent);
            for (var i = recentStart; i < messages.Count && selected.Count < maxMessages; i++)
            {
                if (!selectedIndices.Contains(i))
                {
                    selected.Add(new IndexedMessage(i, messages[i]));
                    selectedIndices.Add(i);
                }
            }

            for (var i = messages.Count - 1; i >= 0 && selected.Count < maxMessages; i--)
            {
                if (selectedIndices.Contains(i)) continue;
                selected.Add(new IndexedMessage(i, messages[i]));
                selectedIndices.Add(i);
            }

            selected.Sort(delegate(IndexedMessage x, IndexedMessage y)
            {
                return x.Index.CompareTo(y.Index);
            });

            while (CountCharacters(selected) > maxCharacters && selected.Count > 1)
            {
                var removeIndex = FindOldestRemovableIndex(selected, leading);
                if (removeIndex < 0) break;
                selected.RemoveAt(removeIndex);
            }

            var result = new List<AIMessage>(selected.Count);
            foreach (var item in selected)
                result.Add(item.Message);
            return result.AsReadOnly();
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
                total += message.Role == null ? 0 : message.Role.Length;
                total += message.Content == null ? 0 : message.Content.Length;
            }
            return total;
        }

        private static int CountCharacters(List<IndexedMessage> messages)
        {
            var total = 0;
            foreach (var item in messages)
            {
                if (item.Message == null) continue;
                total += item.Message.Role == null ? 0 : item.Message.Role.Length;
                total += item.Message.Content == null ? 0 : item.Message.Content.Length;
            }
            return total;
        }

        private static int FindOldestRemovableIndex(List<IndexedMessage> messages, int leadingCount)
        {
            for (var i = leadingCount; i < messages.Count; i++)
                return i;
            return -1;
        }

        private sealed class IndexedMessage
        {
            public IndexedMessage(int index, AIMessage message)
            {
                Index = index;
                Message = message;
            }

            public int Index { get; private set; }
            public AIMessage Message { get; private set; }
        }
    }
}
