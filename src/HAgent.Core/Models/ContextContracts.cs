using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HAgent.Models
{
    /// <summary>
    /// Canonical provider-neutral ownership/scope metadata for context.
    /// </summary>
    public sealed class ContextScope
    {
        public ContextScope()
        {
            ScopeType = "Global";
            ScopeId = string.Empty;
        }

        public string ScopeType { get; set; }
        public string ScopeId { get; set; }

        public ContextScope Clone()
        {
            return new ContextScope
            {
                ScopeType = ScopeType,
                ScopeId = ScopeId
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ScopeType))
                throw new ArgumentException("Context scope type is required.", nameof(ScopeType));
            if (ScopeType.Length > 64)
                throw new ArgumentOutOfRangeException(nameof(ScopeType));
            if (ScopeId != null && ScopeId.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(ScopeId));
        }
    }

    /// <summary>
    /// Bounded provenance retained with a context item so inclusion decisions remain explainable.
    /// </summary>
    public sealed class ContextProvenance
    {
        public ContextProvenance()
        {
            SourceKind = string.Empty;
            SourceId = string.Empty;
            SourceVersion = string.Empty;
            Evidence = string.Empty;
            CapturedAt = DateTimeOffset.UtcNow;
        }

        public string SourceKind { get; set; }
        public string SourceId { get; set; }
        public string SourceVersion { get; set; }
        public string Evidence { get; set; }
        public DateTimeOffset CapturedAt { get; set; }

        public ContextProvenance Clone()
        {
            return new ContextProvenance
            {
                SourceKind = SourceKind,
                SourceId = SourceId,
                SourceVersion = SourceVersion,
                Evidence = Evidence,
                CapturedAt = CapturedAt
            };
        }

        public void Validate()
        {
            ValidateLength(SourceKind, 128, nameof(SourceKind));
            ValidateLength(SourceId, 256, nameof(SourceId));
            ValidateLength(SourceVersion, 128, nameof(SourceVersion));
            ValidateLength(Evidence, 2000, nameof(Evidence));
            if (CapturedAt == default(DateTimeOffset))
                throw new ArgumentException("Context provenance capture time is required.", nameof(CapturedAt));
        }

        private static void ValidateLength(string value, int maximum, string name)
        {
            if (value != null && value.Length > maximum)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    /// <summary>
    /// Canonical provider-neutral unit of context information.
    /// </summary>
    public sealed class ContextItem
    {
        public ContextItem()
        {
            Id = Guid.NewGuid().ToString("N");
            Source = string.Empty;
            Type = string.Empty;
            Payload = null;
            Provenance = new ContextProvenance();
            Trust = 0d;
            Importance = 0d;
            Relevance = 0d;
            CapturedAt = DateTimeOffset.UtcNow;
            Scope = new ContextScope();
            EstimatedCharacters = 0;
            EstimatedTokens = null;
        }

        public string Id { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public object Payload { get; set; }
        public ContextProvenance Provenance { get; set; }
        public double Trust { get; set; }
        public double Importance { get; set; }
        public double Relevance { get; set; }
        public DateTimeOffset CapturedAt { get; set; }
        public ContextScope Scope { get; set; }
        public int EstimatedCharacters { get; set; }
        public int? EstimatedTokens { get; set; }

        public ContextItem Clone()
        {
            return new ContextItem
            {
                Id = Id,
                Source = Source,
                Type = Type,
                Payload = Payload,
                Provenance = Provenance == null ? null : Provenance.Clone(),
                Trust = Trust,
                Importance = Importance,
                Relevance = Relevance,
                CapturedAt = CapturedAt,
                Scope = Scope == null ? null : Scope.Clone(),
                EstimatedCharacters = EstimatedCharacters,
                EstimatedTokens = EstimatedTokens
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentException("Context item ID is required.", nameof(Id));
            if (Id.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(Id));
            if (string.IsNullOrWhiteSpace(Source))
                throw new ArgumentException("Context item source is required.", nameof(Source));
            if (Source.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(Source));
            if (string.IsNullOrWhiteSpace(Type))
                throw new ArgumentException("Context item type is required.", nameof(Type));
            if (Type.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(Type));
            if (Payload == null)
                throw new ArgumentException("Context item payload is required.", nameof(Payload));
            if (Provenance == null)
                throw new ArgumentException("Context item provenance is required.", nameof(Provenance));
            if (Scope == null)
                throw new ArgumentException("Context item scope is required.", nameof(Scope));
            Provenance.Validate();
            Scope.Validate();
            ValidateScore(Trust, nameof(Trust));
            ValidateScore(Importance, nameof(Importance));
            ValidateScore(Relevance, nameof(Relevance));
            if (CapturedAt == default(DateTimeOffset))
                throw new ArgumentException("Context item capture time is required.", nameof(CapturedAt));
            if (EstimatedCharacters < 0)
                throw new ArgumentOutOfRangeException(nameof(EstimatedCharacters));
            if (EstimatedTokens.HasValue && EstimatedTokens.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(EstimatedTokens));
        }

        private static void ValidateScore(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d || value > 1d)
                throw new ArgumentOutOfRangeException(name, "Context scores must be finite values between 0 and 1.");
        }
    }

    /// <summary>
    /// Explicit multidimensional budget for bounded context assembly.
    /// </summary>
    public sealed class ContextBudget
    {
        public ContextBudget()
        {
            MaxItems = 100;
            MaxCharacters = 80000;
            MaxEstimatedTokens = null;
        }

        public int MaxItems { get; set; }
        public int MaxCharacters { get; set; }
        public int? MaxEstimatedTokens { get; set; }

        public ContextBudget Clone()
        {
            return new ContextBudget
            {
                MaxItems = MaxItems,
                MaxCharacters = MaxCharacters,
                MaxEstimatedTokens = MaxEstimatedTokens
            };
        }

        public void Validate()
        {
            if (MaxItems <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxItems));
            if (MaxCharacters <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxCharacters));
            if (MaxEstimatedTokens.HasValue && MaxEstimatedTokens.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxEstimatedTokens));
        }
    }

    /// <summary>
    /// Bounded request supplied to a context source. It contains retrieval intent and a hard candidate bound,
    /// but no provider- or database-specific query language.
    /// </summary>
    public sealed class ContextSourceRequest
    {
        public ContextSourceRequest()
        {
            Query = string.Empty;
            MaxItems = 100;
        }

        public string Query { get; set; }
        public int MaxItems { get; set; }

        public ContextSourceRequest Clone()
        {
            return new ContextSourceRequest
            {
                Query = Query,
                MaxItems = MaxItems
            };
        }

        public void Validate(int maximumItems = 1000)
        {
            if (Query != null && Query.Length > 4096)
                throw new ArgumentOutOfRangeException(nameof(Query));
            if (MaxItems < 1 || MaxItems > maximumItems)
                throw new ArgumentOutOfRangeException(nameof(MaxItems));
        }
    }

    /// <summary>
    /// Immutable provider-neutral context collection produced by a later assembly stage.
    /// This contract is intentionally snapshot-only; it performs no retrieval or ranking.
    /// </summary>
    public sealed class ContextSnapshot
    {
        public ContextSnapshot(IEnumerable<ContextItem> items, ContextBudget budget)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (budget == null) throw new ArgumentNullException(nameof(budget));

            var clones = new List<ContextItem>();
            foreach (var item in items)
            {
                if (item == null) throw new ArgumentException("Context snapshot cannot contain null items.", nameof(items));
                item.Validate();
                clones.Add(item.Clone());
            }

            var copiedBudget = budget.Clone();
            copiedBudget.Validate();

            Items = new ReadOnlyCollection<ContextItem>(clones);
            Budget = copiedBudget;
        }

        public IReadOnlyList<ContextItem> Items { get; private set; }
        public ContextBudget Budget { get; private set; }

        public ContextSnapshot Clone()
        {
            return new ContextSnapshot(Items, Budget);
        }
    }
}
