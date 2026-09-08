using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HAgent.Models
{
    /// <summary>
    /// Execution-owned, provider-neutral context snapshot produced from acquired candidates.
    /// Item and budget accessors return defensive copies so callers cannot mutate the stored snapshot state.
    /// </summary>
    public sealed class ContextSnapshot
    {
        private readonly IReadOnlyList<ContextItem> _items;
        private readonly ContextBudget _budget;

        public ContextSnapshot(
            IReadOnlyList<ContextItem> items,
            ContextBudget budget,
            int usedItems,
            int usedCharacters,
            int? usedEstimatedTokens,
            int sourceCount,
            DateTimeOffset? createdAt = null)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            budget.Validate();
            if (usedItems < 0 || usedItems > budget.MaxItems)
                throw new ArgumentOutOfRangeException(nameof(usedItems));
            if (usedCharacters < 0 || usedCharacters > budget.MaxCharacters)
                throw new ArgumentOutOfRangeException(nameof(usedCharacters));
            if (usedEstimatedTokens.HasValue &&
                (usedEstimatedTokens.Value < 0 ||
                 (budget.MaxEstimatedTokens.HasValue && usedEstimatedTokens.Value > budget.MaxEstimatedTokens.Value)))
                throw new ArgumentOutOfRangeException(nameof(usedEstimatedTokens));
            if (sourceCount < 0) throw new ArgumentOutOfRangeException(nameof(sourceCount));
            if (items.Count != usedItems)
                throw new ArgumentException("Context snapshot item count must match used items.", nameof(usedItems));

            var clones = new List<ContextItem>(items.Count);
            foreach (var item in items)
            {
                if (item == null) throw new ArgumentException("Context snapshot cannot contain null items.", nameof(items));
                var clone = item.Clone();
                clone.Validate();
                clones.Add(clone);
            }

            _items = new ReadOnlyCollection<ContextItem>(clones);
            _budget = budget.Clone();
            UsedItems = usedItems;
            UsedCharacters = usedCharacters;
            UsedEstimatedTokens = usedEstimatedTokens;
            RemainingItems = budget.MaxItems - usedItems;
            RemainingCharacters = budget.MaxCharacters - usedCharacters;
            RemainingEstimatedTokens = budget.MaxEstimatedTokens.HasValue && usedEstimatedTokens.HasValue
                ? budget.MaxEstimatedTokens.Value - usedEstimatedTokens.Value
                : budget.MaxEstimatedTokens;
            SourceCount = sourceCount;
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
            Version = 1;
        }

        public IReadOnlyList<ContextItem> Items
        {
            get
            {
                var clones = new List<ContextItem>(_items.Count);
                foreach (var item in _items)
                    clones.Add(item.Clone());
                return new ReadOnlyCollection<ContextItem>(clones);
            }
        }

        public ContextBudget Budget { get { return _budget.Clone(); } }
        public int UsedItems { get; private set; }
        public int RemainingItems { get; private set; }
        public int UsedCharacters { get; private set; }
        public int RemainingCharacters { get; private set; }
        public int? UsedEstimatedTokens { get; private set; }
        public int? RemainingEstimatedTokens { get; private set; }
        public int SourceCount { get; private set; }
        public int Version { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
    }
}
