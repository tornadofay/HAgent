using System;
using HAgent.Models;

namespace HAgent.Models
{
    /// <summary>
    /// Describes the host authorization state for a context source.
    /// </summary>
    public enum ContextHostAuthorizationState
    {
        NotRequired = 0,
        Allowed = 1,
        Denied = 2
    }

    /// <summary>
    /// Safe metadata describing why a context source or item was admitted or excluded.
    /// Payload content is never carried by this decision contract.
    /// </summary>
    public sealed class ContextAdmissionDecision
    {
        public bool Allowed { get; set; }
        public string Reason { get; set; }
        public string SourceKind { get; set; }
        public string SourceId { get; set; }
        public string ItemId { get; set; }
        public string ItemType { get; set; }
        public AiResourceCapabilityState ResourceCapabilityState { get; set; }
        public AiPolicyDecision PolicyDecision { get; set; }
        public ContextHostAuthorizationState HostAuthorizationState { get; set; }
        public string HostAuthorizationReason { get; set; }

        public ContextAdmissionDecision Clone()
        {
            return new ContextAdmissionDecision
            {
                Allowed = Allowed,
                Reason = Reason,
                SourceKind = SourceKind,
                SourceId = SourceId,
                ItemId = ItemId,
                ItemType = ItemType,
                ResourceCapabilityState = ResourceCapabilityState,
                PolicyDecision = ClonePolicyDecision(PolicyDecision),
                HostAuthorizationState = HostAuthorizationState,
                HostAuthorizationReason = HostAuthorizationReason
            };
        }

        private static AiPolicyDecision ClonePolicyDecision(AiPolicyDecision decision)
        {
            if (decision == null)
                return null;

            return new AiPolicyDecision
            {
                Outcome = decision.Outcome,
                PolicyVersion = decision.PolicyVersion,
                RuleId = decision.RuleId,
                RuleName = decision.RuleName,
                Scope = decision.Scope,
                Priority = decision.Priority,
                Reason = decision.Reason,
                IsBuiltIn = decision.IsBuiltIn
            };
        }
    }
}
