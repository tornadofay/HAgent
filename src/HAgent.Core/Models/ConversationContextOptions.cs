using System;

namespace HAgent.Models
{
    /// <summary>
    /// Limits the amount of conversation history sent to an AI provider.
    /// Character limits are intentionally provider-neutral; a real tokenizer is optional.
    /// </summary>
    public sealed class ConversationContextOptions
    {
        public ConversationContextOptions()
        {
            MaxMessages = 40;
            MaxCharacters = 80000;
            PreserveRecentMessages = 12;
            PreserveLeadingMessages = 2;
        }

        public int MaxMessages { get; set; }
        public int MaxCharacters { get; set; }
        public int PreserveRecentMessages { get; set; }
        public int PreserveLeadingMessages { get; set; }

        internal void Validate()
        {
            if (MaxMessages <= 0) throw new ArgumentOutOfRangeException(nameof(MaxMessages), "MaxMessages must be greater than zero.");
            if (MaxCharacters <= 0) throw new ArgumentOutOfRangeException(nameof(MaxCharacters), "MaxCharacters must be greater than zero.");
            if (PreserveRecentMessages < 0) throw new ArgumentOutOfRangeException(nameof(PreserveRecentMessages), "PreserveRecentMessages cannot be negative.");
            if (PreserveLeadingMessages < 0) throw new ArgumentOutOfRangeException(nameof(PreserveLeadingMessages), "PreserveLeadingMessages cannot be negative.");
        }
    }
}
