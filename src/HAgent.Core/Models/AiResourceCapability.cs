using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HAgent.Models
{
    /// <summary>
    /// Runtime/profile state for one governed HAgent resource or resource family.
    /// Inherit means that the next lower configuration layer decides the effective state.
    /// </summary>
    public enum AiResourceCapabilityState
    {
        Inherit,
        Enabled,
        Disabled
    }

    public sealed class AiResourceCapabilityEntry
    {
        public AiResourceCapabilityEntry()
        {
            ResourceType = string.Empty;
            ResourceId = string.Empty;
            State = AiResourceCapabilityState.Inherit;
        }

        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public AiResourceCapabilityState State { get; set; }

        public AiResourceCapabilityEntry Clone()
        {
            return new AiResourceCapabilityEntry
            {
                ResourceType = ResourceType,
                ResourceId = ResourceId,
                State = State
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ResourceType))
                throw new ArgumentException("Resource type is required.", nameof(ResourceType));
            if (ResourceType.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(ResourceType));
            if (ResourceId != null && ResourceId.Length > 512)
                throw new ArgumentOutOfRangeException(nameof(ResourceId));
            if (!Enum.IsDefined(typeof(AiResourceCapabilityState), State))
                throw new ArgumentOutOfRangeException(nameof(State));
        }
    }

    /// <summary>
    /// Profile-level resource/capability defaults or runtime-only overrides.
    /// Exact resource entries override family/type entries. Inherit continues resolution.
    /// </summary>
    public sealed class AiResourceCapabilityPolicy
    {
        public AiResourceCapabilityPolicy()
        {
            Entries = new List<AiResourceCapabilityEntry>();
        }

        public IList<AiResourceCapabilityEntry> Entries { get; private set; }

        public void Set(string resourceType, AiResourceCapabilityState state)
        {
            Set(resourceType, string.Empty, state);
        }

        public void Set(string resourceType, string resourceId, AiResourceCapabilityState state)
        {
            var normalizedType = NormalizeRequired(resourceType, nameof(resourceType));
            var normalizedId = NormalizeOptional(resourceId);
            var existing = Entries.FirstOrDefault(x =>
                string.Equals(NormalizeRequired(x.ResourceType, nameof(x.ResourceType)), normalizedType, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(NormalizeOptional(x.ResourceId), normalizedId, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                Entries.Add(new AiResourceCapabilityEntry
                {
                    ResourceType = normalizedType,
                    ResourceId = normalizedId,
                    State = state
                });
            }
            else
            {
                existing.State = state;
            }
        }

        public AiResourceCapabilityPolicy Clone()
        {
            var clone = new AiResourceCapabilityPolicy();
            foreach (var entry in Entries ?? new List<AiResourceCapabilityEntry>())
            {
                if (entry != null)
                    clone.Entries.Add(entry.Clone());
            }
            return clone;
        }

        public void Validate()
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in Entries ?? new List<AiResourceCapabilityEntry>())
            {
                if (entry == null)
                    throw new ArgumentException("Resource capability entries cannot contain null values.", nameof(Entries));
                entry.Validate();
                var key = NormalizeRequired(entry.ResourceType, nameof(entry.ResourceType)) + "\n" + NormalizeOptional(entry.ResourceId);
                if (!keys.Add(key))
                    throw new ArgumentException("Resource capability entries must be unique: " + key, nameof(Entries));
            }
        }

        internal AiResourceCapabilityEntry Find(string resourceType, string resourceId)
        {
            var normalizedType = NormalizeRequired(resourceType, nameof(resourceType));
            var normalizedId = NormalizeOptional(resourceId);

            var exact = Entries == null ? null : Entries.FirstOrDefault(x =>
                x != null &&
                string.Equals(NormalizeOptional(x.ResourceId), normalizedId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(NormalizeRequired(x.ResourceType, nameof(x.ResourceType)), normalizedType, StringComparison.OrdinalIgnoreCase));
            if (exact != null && exact.State != AiResourceCapabilityState.Inherit)
                return exact;

            if (!string.IsNullOrEmpty(normalizedId))
            {
                var type = Entries == null ? null : Entries.FirstOrDefault(x =>
                    x != null &&
                    string.IsNullOrEmpty(NormalizeOptional(x.ResourceId)) &&
                    string.Equals(NormalizeRequired(x.ResourceType, nameof(x.ResourceType)), normalizedType, StringComparison.OrdinalIgnoreCase));
                if (type != null && type.State != AiResourceCapabilityState.Inherit)
                    return type;
            }
            else if (exact != null)
            {
                return exact;
            }

            return null;
        }

        private static string NormalizeRequired(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Resource type is required.", name);
            return value.Trim();
        }

        private static string NormalizeOptional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    /// <summary>
    /// Immutable-for-execution effective resource state. No entry can remain Inherit.
    /// </summary>
    public sealed class AiResourceCapabilitySnapshot
    {
        private readonly IReadOnlyDictionary<string, AiResourceCapabilityState> _states;

        private AiResourceCapabilitySnapshot(IReadOnlyDictionary<string, AiResourceCapabilityState> states, IReadOnlyList<AiResourceCapabilityEntry> entries)
        {
            _states = states;
            Entries = entries;
        }

        public IReadOnlyList<AiResourceCapabilityEntry> Entries { get; private set; }

        public bool IsEnabled(string resourceType, string resourceId = null)
        {
            return GetState(resourceType, resourceId) == AiResourceCapabilityState.Enabled;
        }

        public AiResourceCapabilityState GetState(string resourceType, string resourceId = null)
        {
            var type = resourceType == null ? string.Empty : resourceType.Trim();
            var id = string.IsNullOrWhiteSpace(resourceId) ? string.Empty : resourceId.Trim();
            var key = MakeKey(type, id);

            AiResourceCapabilityState state;
            if (_states.TryGetValue(key, out state))
                return state;

            if (!string.IsNullOrEmpty(id) && _states.TryGetValue(MakeKey(type, string.Empty), out state))
                return state;

            return AiResourceCapabilityState.Enabled;
        }

        public AiResourceCapabilitySnapshot Clone()
        {
            var states = new Dictionary<string, AiResourceCapabilityState>(_states, StringComparer.OrdinalIgnoreCase);
            var entries = Entries.Select(x => x.Clone()).ToList().AsReadOnly();
            return new AiResourceCapabilitySnapshot(new ReadOnlyDictionary<string, AiResourceCapabilityState>(states), entries);
        }

        public void Validate()
        {
            foreach (var entry in Entries)
            {
                if (entry == null) throw new ArgumentException("Effective resource capability entries cannot contain null values.", nameof(Entries));
                entry.Validate();
                if (entry.State == AiResourceCapabilityState.Inherit)
                    throw new ArgumentException("Effective resource capability snapshots cannot contain Inherit entries.", nameof(Entries));
            }
        }

        public static AiResourceCapabilitySnapshot Resolve(AiResourceCapabilityPolicy profile, AiResourceCapabilityPolicy runtimeOverrides = null)
        {
            var profilePolicy = profile == null ? new AiResourceCapabilityPolicy() : profile.Clone();
            var runtimePolicy = runtimeOverrides == null ? new AiResourceCapabilityPolicy() : runtimeOverrides.Clone();
            profilePolicy.Validate();
            runtimePolicy.Validate();

            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in profilePolicy.Entries)
                keys.Add(MakeKey(entry.ResourceType, entry.ResourceId));
            foreach (var entry in runtimePolicy.Entries)
                keys.Add(MakeKey(entry.ResourceType, entry.ResourceId));

            var orderedKeys = keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            var states = new Dictionary<string, AiResourceCapabilityState>(StringComparer.OrdinalIgnoreCase);
            var effectiveEntries = new List<AiResourceCapabilityEntry>();
            foreach (var key in orderedKeys)
            {
                SplitKey(key, out var type, out var id);
                var state = ResolveState(profilePolicy, runtimePolicy, type, id);
                states[key] = state;
                effectiveEntries.Add(new AiResourceCapabilityEntry
                {
                    ResourceType = type,
                    ResourceId = id,
                    State = state
                });
            }

            var snapshot = new AiResourceCapabilitySnapshot(
                new ReadOnlyDictionary<string, AiResourceCapabilityState>(states),
                effectiveEntries.AsReadOnly());
            snapshot.Validate();
            return snapshot;
        }

        private static AiResourceCapabilityState ResolveState(AiResourceCapabilityPolicy profile, AiResourceCapabilityPolicy runtime, string resourceType, string resourceId)
        {
            var runtimeEntry = runtime.Find(resourceType, resourceId);
            if (runtimeEntry != null && runtimeEntry.State != AiResourceCapabilityState.Inherit)
                return runtimeEntry.State;

            var profileEntry = profile.Find(resourceType, resourceId);
            if (profileEntry != null && profileEntry.State != AiResourceCapabilityState.Inherit)
                return profileEntry.State;

            return AiResourceCapabilityState.Enabled;
        }

        private static string MakeKey(string resourceType, string resourceId)
        {
            return (resourceType ?? string.Empty).Trim() + "\n" + (string.IsNullOrWhiteSpace(resourceId) ? string.Empty : resourceId.Trim());
        }

        private static void SplitKey(string key, out string resourceType, out string resourceId)
        {
            var separator = key.IndexOf('\n');
            if (separator < 0)
            {
                resourceType = key;
                resourceId = string.Empty;
                return;
            }
            resourceType = key.Substring(0, separator);
            resourceId = key.Substring(separator + 1);
        }
    }
}
