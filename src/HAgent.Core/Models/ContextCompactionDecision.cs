using System;

namespace HAgent.Models
{
    public enum ContextCompactionDecisionReason
    {
        Included = 0,
        ItemBudgetExhausted = 1,
        CharacterBudgetExceeded = 2,
        TokenBudgetExceeded = 3,
        CharacterAndTokenBudgetExceeded = 4,
        UnknownTokenEstimate = 5
    }

    /// <summary>
    /// Safe metadata describing one deterministic compaction decision. Payload content is never copied here.
    /// </summary>
    public sealed class ContextCompactionDecision
    {
        public ContextCompactionDecision(
            int candidateIndex,
            int outputIndex,
            string itemId,
            string source,
            bool included,
            ContextCompactionDecisionReason reason,
            int estimatedCharacters,
            int? estimatedTokens,
            int usedItemsBefore,
            int usedCharactersBefore,
            int? usedEstimatedTokensBefore)
        {
            if (candidateIndex < 0) throw new ArgumentOutOfRangeException(nameof(candidateIndex));
            if (outputIndex < -1) throw new ArgumentOutOfRangeException(nameof(outputIndex));
            if (string.IsNullOrWhiteSpace(itemId)) throw new ArgumentException("Item ID is required.", nameof(itemId));
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Source is required.", nameof(source));
            if (estimatedCharacters < 0) throw new ArgumentOutOfRangeException(nameof(estimatedCharacters));
            if (estimatedTokens.HasValue && estimatedTokens.Value < 0) throw new ArgumentOutOfRangeException(nameof(estimatedTokens));
            if (usedItemsBefore < 0) throw new ArgumentOutOfRangeException(nameof(usedItemsBefore));
            if (usedCharactersBefore < 0) throw new ArgumentOutOfRangeException(nameof(usedCharactersBefore));
            if (usedEstimatedTokensBefore.HasValue && usedEstimatedTokensBefore.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(usedEstimatedTokensBefore));

            CandidateIndex = candidateIndex;
            OutputIndex = outputIndex;
            ItemId = itemId;
            Source = source;
            Included = included;
            Reason = reason;
            EstimatedCharacters = estimatedCharacters;
            EstimatedTokens = estimatedTokens;
            UsedItemsBefore = usedItemsBefore;
            UsedCharactersBefore = usedCharactersBefore;
            UsedEstimatedTokensBefore = usedEstimatedTokensBefore;
        }

        public int CandidateIndex { get; private set; }
        public int OutputIndex { get; private set; }
        public string ItemId { get; private set; }
        public string Source { get; private set; }
        public bool Included { get; private set; }
        public ContextCompactionDecisionReason Reason { get; private set; }
        public int EstimatedCharacters { get; private set; }
        public int? EstimatedTokens { get; private set; }
        public int UsedItemsBefore { get; private set; }
        public int UsedCharactersBefore { get; private set; }
        public int? UsedEstimatedTokensBefore { get; private set; }

        public ContextCompactionDecision Clone()
        {
            return new ContextCompactionDecision(
                CandidateIndex,
                OutputIndex,
                ItemId,
                Source,
                Included,
                Reason,
                EstimatedCharacters,
                EstimatedTokens,
                UsedItemsBefore,
                UsedCharactersBefore,
                UsedEstimatedTokensBefore);
        }
    }
}
