using System;

namespace HAgent.Models
{
    public enum AiSelectionMode
    {
        Auto,
        Preferred,
        Fixed
    }

    public enum AiFallbackMode
    {
        Fail,
        TryNextCandidate,
        Wait
    }

    public enum AiCostPolicy
    {
        FreeOnly,
        FreePreferred,
        NoRestriction
    }

    public sealed class AiExecutionSelectionPolicy
    {
        public AiExecutionSelectionPolicy()
        {
            Mode = AiSelectionMode.Auto;
            Fallback = AiFallbackMode.TryNextCandidate;
            CostPolicy = AiCostPolicy.NoRestriction;
            PreferredProviderId = string.Empty;
            PreferredTargetId = string.Empty;
            PreferredLogicalModelId = string.Empty;
            MaxQueueWait = TimeSpan.Zero;
        }

        public AiSelectionMode Mode { get; set; }
        public AiFallbackMode Fallback { get; set; }
        public AiCostPolicy CostPolicy { get; set; }
        public string PreferredProviderId { get; set; }
        public string PreferredTargetId { get; set; }
        public string PreferredLogicalModelId { get; set; }
        public TimeSpan MaxQueueWait { get; set; }

        public AiExecutionSelectionPolicy Clone()
        {
            return new AiExecutionSelectionPolicy
            {
                Mode = Mode,
                Fallback = Fallback,
                CostPolicy = CostPolicy,
                PreferredProviderId = PreferredProviderId,
                PreferredTargetId = PreferredTargetId,
                PreferredLogicalModelId = PreferredLogicalModelId,
                MaxQueueWait = MaxQueueWait
            };
        }

        public void Validate()
        {
            if (MaxQueueWait < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(MaxQueueWait));

            if (Mode == AiSelectionMode.Fixed && string.IsNullOrWhiteSpace(PreferredTargetId))
                throw new ArgumentException("Fixed selection requires a concrete execution target id.", nameof(PreferredTargetId));
        }
    }
}
