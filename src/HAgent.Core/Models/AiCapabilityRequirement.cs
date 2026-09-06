using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum CapabilityRequirementStrength
    {
        Required,
        Preferred,
        Optional,
        Forbidden
    }

    public sealed class AiCapabilityRequirement
    {
        public AiCapabilityRequirement()
        {
            Capability = AiCapability.None;
            Strength = CapabilityRequirementStrength.Required;
        }

        public AiCapability Capability { get; set; }
        public CapabilityRequirementStrength Strength { get; set; }
    }

    public sealed class AiCapabilityRequirements
    {
        public AiCapabilityRequirements()
        {
            Items = new List<AiCapabilityRequirement>();
        }

        public IList<AiCapabilityRequirement> Items { get; private set; }

        public void Require(AiCapability capability)
        {
            Add(capability, CapabilityRequirementStrength.Required);
        }

        public void Prefer(AiCapability capability)
        {
            Add(capability, CapabilityRequirementStrength.Preferred);
        }

        public void Allow(AiCapability capability)
        {
            Add(capability, CapabilityRequirementStrength.Optional);
        }

        public void Forbid(AiCapability capability)
        {
            Add(capability, CapabilityRequirementStrength.Forbidden);
        }

        private void Add(AiCapability capability, CapabilityRequirementStrength strength)
        {
            if (capability == AiCapability.None)
                throw new ArgumentException("A real capability is required.", nameof(capability));

            Items.Add(new AiCapabilityRequirement
            {
                Capability = capability,
                Strength = strength
            });
        }
    }
}
