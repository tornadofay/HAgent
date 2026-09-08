using System;

namespace HAgent.Models
{
    /// <summary>
    /// Explicit ownership/version identity for a reusable context component.
    /// </summary>
    public sealed class ContextCacheKey
    {
        public ContextCacheKey()
        {
            ComponentId = string.Empty;
            ScopeType = string.Empty;
            ScopeId = string.Empty;
            ConfigurationVersion = string.Empty;
            ResourceVersion = string.Empty;
            FreshnessVersion = string.Empty;
        }

        public string ComponentId { get; set; }
        public string ScopeType { get; set; }
        public string ScopeId { get; set; }
        public string ConfigurationVersion { get; set; }
        public string ResourceVersion { get; set; }
        public string FreshnessVersion { get; set; }

        public ContextCacheKey Clone()
        {
            return new ContextCacheKey
            {
                ComponentId = ComponentId,
                ScopeType = ScopeType,
                ScopeId = ScopeId,
                ConfigurationVersion = ConfigurationVersion,
                ResourceVersion = ResourceVersion,
                FreshnessVersion = FreshnessVersion
            };
        }

        public string ToCanonicalString()
        {
            Validate();
            return string.Join("|", new[]
            {
                Escape(ComponentId),
                Escape(ScopeType),
                Escape(ScopeId),
                Escape(ConfigurationVersion),
                Escape(ResourceVersion),
                Escape(FreshnessVersion)
            });
        }

        public void Validate()
        {
            ValidateRequired(ComponentId, 256, nameof(ComponentId));
            ValidateRequired(ScopeType, 64, nameof(ScopeType));
            ValidateOptional(ScopeId, 256, nameof(ScopeId));
            ValidateOptional(ConfigurationVersion, 128, nameof(ConfigurationVersion));
            ValidateOptional(ResourceVersion, 128, nameof(ResourceVersion));
            ValidateOptional(FreshnessVersion, 128, nameof(FreshnessVersion));
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("|", "\\|");
        }

        private static void ValidateRequired(string value, int maximum, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Context cache key value is required.", name);
            if (value.Length > maximum)
                throw new ArgumentOutOfRangeException(name);
        }

        private static void ValidateOptional(string value, int maximum, string name)
        {
            if (value != null && value.Length > maximum)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
