using System;
using System.Collections.Generic;
using System.Linq;

namespace HAgent.Models
{
    public static class AiMemoryResourceTypes
    {
        public const string Memory = "memory";
        public const string Family = "memory.family";
        public const string Type = "memory.type";

        public static string FamilyId(AiMemoryFamily family)
        {
            if (!Enum.IsDefined(typeof(AiMemoryFamily), family)) throw new ArgumentOutOfRangeException(nameof(family));
            return family.ToString();
        }

        public static string TypeId(string typeId)
        {
            if (string.IsNullOrWhiteSpace(typeId)) throw new ArgumentException("Memory TypeId is required.", nameof(typeId));
            return typeId.Trim();
        }
    }

    public sealed class AiMemoryPolicyRule
    {
        public AiMemoryPolicyRule()
        {
            TypeId = string.Empty;
            MaxResults = 0;
            RetentionDays = 0;
        }

        public AiMemoryFamily? Family { get; set; }
        public string TypeId { get; set; }
        public int MaxResults { get; set; }
        public int RetentionDays { get; set; }

        public void Validate()
        {
            if (!Family.HasValue && string.IsNullOrWhiteSpace(TypeId))
                throw new ArgumentException("A memory policy rule must target a family or TypeId.");
            if (Family.HasValue && !Enum.IsDefined(typeof(AiMemoryFamily), Family.Value))
                throw new ArgumentOutOfRangeException(nameof(Family));
            if (!string.IsNullOrWhiteSpace(TypeId) && TypeId.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(TypeId));
            if (MaxResults < 0 || MaxResults > 1000)
                throw new ArgumentOutOfRangeException(nameof(MaxResults));
            if (RetentionDays < 0 || RetentionDays > 3650)
                throw new ArgumentOutOfRangeException(nameof(RetentionDays));
        }

        public AiMemoryPolicyRule Clone()
        {
            return new AiMemoryPolicyRule
            {
                Family = Family,
                TypeId = TypeId,
                MaxResults = MaxResults,
                RetentionDays = RetentionDays
            };
        }
    }

    public sealed class AiMemoryGovernancePolicy
    {
        public AiMemoryGovernancePolicy()
        {
            MaxResults = 100;
            ExcludeExpired = true;
            Rules = new List<AiMemoryPolicyRule>();
        }

        public int MaxResults { get; set; }
        public bool ExcludeExpired { get; set; }
        public IList<AiMemoryPolicyRule> Rules { get; private set; }

        public int GetMaxResults(MemoryQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            Validate();
            var limit = MaxResults;
            var rule = FindRule(query.Family, query.TypeId);
            if (rule != null && rule.MaxResults > 0) limit = Math.Min(limit, rule.MaxResults);
            return Math.Max(1, Math.Min(limit, 1000));
        }

        public int GetMaxResults(AiMemoryFamily family, string typeId = null)
        {
            var query = new MemoryQuery { Family = family, TypeId = typeId ?? string.Empty };
            return GetMaxResults(query);
        }

        public DateTimeOffset? GetEffectiveExpiration(MemoryEntry entry, DateTimeOffset now)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            Validate();
            var rule = FindRule(entry.Family, entry.TypeId);
            if (rule == null || rule.RetentionDays <= 0) return entry.ExpiresAt;

            var policyExpiration = entry.CreatedAt.AddDays(rule.RetentionDays);
            if (!entry.ExpiresAt.HasValue || policyExpiration < entry.ExpiresAt.Value)
                return policyExpiration;
            return entry.ExpiresAt;
        }

        public AiMemoryGovernancePolicy Clone()
        {
            var clone = new AiMemoryGovernancePolicy
            {
                MaxResults = MaxResults,
                ExcludeExpired = ExcludeExpired
            };
            foreach (var rule in Rules ?? new List<AiMemoryPolicyRule>())
                if (rule != null) clone.Rules.Add(rule.Clone());
            return clone;
        }

        public void Validate()
        {
            if (MaxResults < 1 || MaxResults > 1000)
                throw new ArgumentOutOfRangeException(nameof(MaxResults));

            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rule in Rules ?? new List<AiMemoryPolicyRule>())
            {
                if (rule == null) throw new ArgumentException("Memory policy rules cannot contain null values.", nameof(Rules));
                rule.Validate();
                var key = (rule.Family.HasValue ? rule.Family.Value.ToString() : string.Empty) + "\n" + (rule.TypeId ?? string.Empty).Trim();
                if (!keys.Add(key)) throw new ArgumentException("Memory policy rules must be unique: " + key, nameof(Rules));
                if (rule.Family == null && !string.IsNullOrWhiteSpace(rule.TypeId))
                    MemoryEntryTypeValidator.ValidateKnownTypeId(rule.TypeId);
            }
        }

        private AiMemoryPolicyRule FindRule(AiMemoryFamily? family, string typeId)
        {
            var normalizedType = string.IsNullOrWhiteSpace(typeId) ? string.Empty : typeId.Trim();
            var exact = (Rules ?? new List<AiMemoryPolicyRule>()).FirstOrDefault(x =>
                x != null && !string.IsNullOrWhiteSpace(x.TypeId) &&
                string.Equals(x.TypeId.Trim(), normalizedType, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;

            if (family.HasValue)
            {
                var familyRule = (Rules ?? new List<AiMemoryPolicyRule>()).FirstOrDefault(x => x != null && !x.Family.HasValue ? false : x != null && x.Family == family && string.IsNullOrWhiteSpace(x.TypeId));
                if (familyRule != null) return familyRule;
            }
            return null;
        }
    }

    public static class AiMemoryGovernanceEvaluator
    {
        public static void EnsureReadable(AiResourceCapabilitySnapshot capabilities, AiMemoryFamily family, string typeId)
        {
            if (capabilities == null) throw new ArgumentNullException(nameof(capabilities));
            typeId = AiMemoryResourceTypes.TypeId(typeId);
            EnsureEnabled(capabilities, AiMemoryResourceTypes.Memory, null, "Memory capability is disabled.");
            EnsureEnabled(capabilities, AiMemoryResourceTypes.Family, AiMemoryResourceTypes.FamilyId(family), "Memory family capability is disabled: " + family + ".");
            EnsureEnabled(capabilities, AiMemoryResourceTypes.Type, typeId, "Memory type capability is disabled: " + typeId + ".");
        }

        public static void EnsureWritable(AiResourceCapabilitySnapshot capabilities, MemoryEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            EnsureReadable(capabilities, entry.Family, entry.TypeId);
        }

        private static void EnsureEnabled(AiResourceCapabilitySnapshot capabilities, string resourceType, string resourceId, string message)
        {
            if (!capabilities.IsEnabled(resourceType, resourceId))
                throw new InvalidOperationException(message);
        }
    }

    internal static class MemoryEntryTypeValidator
    {
        public static void ValidateKnownTypeId(string typeId)
        {
            if (string.IsNullOrWhiteSpace(typeId) || typeId.Length > 256)
                throw new ArgumentException("Memory TypeId is invalid.", nameof(typeId));
            var value = typeId.Trim();
            if (!(value.StartsWith("working.", StringComparison.OrdinalIgnoreCase) ||
                  value.StartsWith("episodic.", StringComparison.OrdinalIgnoreCase) ||
                  value.StartsWith("semantic.", StringComparison.OrdinalIgnoreCase) ||
                  value.StartsWith("procedural.", StringComparison.OrdinalIgnoreCase)))
                return;
        }
    }
}
