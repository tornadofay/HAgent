using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class AiResourceInventoryQuery
    {
        public IList<string> ResourceTypes { get; private set; }
        public AgentResourceScope? Scope { get; set; }
        public string OwnerId { get; set; }
        public string SearchText { get; set; }
        public bool AuthoritativeOnly { get; set; }
        public int MaxResults { get; set; }

        public AiResourceInventoryQuery()
        {
            ResourceTypes = new List<string>();
            MaxResults = 200;
        }

        public AiResourceInventoryQuery Clone()
        {
            var clone = new AiResourceInventoryQuery
            {
                Scope = Scope,
                OwnerId = OwnerId,
                SearchText = SearchText,
                AuthoritativeOnly = AuthoritativeOnly,
                MaxResults = MaxResults
            };
            foreach (var resourceType in ResourceTypes)
                clone.ResourceTypes.Add(resourceType);
            return clone;
        }

        public void Validate()
        {
            if (ResourceTypes == null || ResourceTypes.Count > 64)
                throw new ArgumentException("Resource type filter exceeds its bounds.");
            foreach (var resourceType in ResourceTypes)
            {
                if (string.IsNullOrWhiteSpace(resourceType) || resourceType.Length > 128)
                    throw new ArgumentException("Resource type filters must be bounded and non-empty.");
            }

            if (OwnerId != null && OwnerId.Length > 2048)
                throw new ArgumentException("Resource inventory OwnerId exceeds its maximum length.");
            if (SearchText != null && SearchText.Length > 512)
                throw new ArgumentException("Resource inventory SearchText exceeds its maximum length.");
            if (MaxResults < 1 || MaxResults > 1000)
                throw new ArgumentException("Resource inventory MaxResults must be between 1 and 1000.");
        }
    }

    public sealed class AiResourceInventoryItem
    {
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public long? Version { get; set; }
        public AgentResourceScope Scope { get; set; }
        public string OwnerId { get; set; }
        public string DisplayName { get; set; }
        public string LifecycleStatus { get; set; }
        public bool IsAuthoritative { get; set; }
        public DateTimeOffset UpdatedUtc { get; set; }
        public string Source { get; set; }

        public AiResourceInventoryItem Clone()
        {
            return new AiResourceInventoryItem
            {
                ResourceType = ResourceType,
                ResourceId = ResourceId,
                Version = Version,
                Scope = Scope,
                OwnerId = OwnerId,
                DisplayName = DisplayName,
                LifecycleStatus = LifecycleStatus,
                IsAuthoritative = IsAuthoritative,
                UpdatedUtc = UpdatedUtc,
                Source = Source
            };
        }

        public void Validate()
        {
            ValidateText(ResourceType, 128, true, nameof(ResourceType));
            ValidateText(ResourceId, 512, true, nameof(ResourceId));
            ValidateText(OwnerId, 2048, false, nameof(OwnerId));
            ValidateText(DisplayName, 500, false, nameof(DisplayName));
            ValidateText(LifecycleStatus, 128, false, nameof(LifecycleStatus));
            ValidateText(Source, 256, false, nameof(Source));
            if (!Enum.IsDefined(typeof(AgentResourceScope), Scope))
                throw new ArgumentException("Invalid resource inventory scope.");
            if (Version.HasValue && Version.Value <= 0)
                throw new ArgumentException("Resource inventory version must be positive when specified.");
            if (UpdatedUtc == default(DateTimeOffset))
                throw new ArgumentException("Resource inventory UpdatedUtc is required.");
            if (Scope == AgentResourceScope.Global && !string.IsNullOrWhiteSpace(OwnerId))
                throw new ArgumentException("Global resource inventory items cannot specify OwnerId.");
            if (Scope != AgentResourceScope.Global && string.IsNullOrWhiteSpace(OwnerId))
                throw new ArgumentException("Non-global resource inventory items require OwnerId.");
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength)
                throw new ArgumentException(name + " exceeds its maximum length.");
        }
    }
}
