using System;

namespace HAgent.Models
{
    /// <summary>
    /// Concrete provider execution target. A logical model may have many targets.
    /// </summary>
    public sealed class AiExecutionTarget
    {
        public AiExecutionTarget()
        {
            Id = string.Empty;
            ProviderId = string.Empty;
            AccountId = string.Empty;
            ProjectId = string.Empty;
            EndpointId = string.Empty;
            ModelId = string.Empty;
            LogicalModelId = string.Empty;
            ModelVersion = string.Empty;
            DeploymentId = string.Empty;
            Capabilities = new AiModelCapabilities();
            Cost = AiCostStatus.Unknown;
            Availability = AiAvailabilityState.Unknown;
        }

        public string Id { get; set; }
        public string ProviderId { get; set; }
        public string AccountId { get; set; }
        public string ProjectId { get; set; }
        public string EndpointId { get; set; }
        public string ModelId { get; set; }
        public string LogicalModelId { get; set; }
        public string ModelVersion { get; set; }
        public string DeploymentId { get; set; }
        public AiModelCapabilities Capabilities { get; set; }
        public AiCostStatus Cost { get; set; }
        public AiAvailabilityState Availability { get; set; }

        public void Validate()
        {
            Require(Id, nameof(Id));
            Require(ProviderId, nameof(ProviderId));
            Require(ModelId, nameof(ModelId));
            if (Id.Length > 512) throw new ArgumentOutOfRangeException(nameof(Id));
            if (ProviderId.Length > 256) throw new ArgumentOutOfRangeException(nameof(ProviderId));
            if (ModelId.Length > 512) throw new ArgumentOutOfRangeException(nameof(ModelId));
            if (LogicalModelId != null && LogicalModelId.Length > 512) throw new ArgumentOutOfRangeException(nameof(LogicalModelId));
            if (Capabilities == null) throw new ArgumentNullException(nameof(Capabilities));
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A value is required.", name);
        }
    }

    public enum AiCostStatus
    {
        Unknown,
        Free,
        FreeWithinQuota,
        Paid
    }

    public enum AiAvailabilityState
    {
        Unknown,
        Available,
        Degraded,
        Unavailable
    }
}
