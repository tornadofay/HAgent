using System;

namespace HAgent.Models
{
    /// <summary>
    /// Bounded execution identity supplied to context admission. The admission layer never authenticates it.
    /// </summary>
    public sealed class ContextAdmissionContext
    {
        public ContextAdmissionContext()
        {
            AgentProfileId = string.Empty;
            RuntimeInstanceId = string.Empty;
            ExecutionId = string.Empty;
            Identity = new AgentIdentityContext();
        }

        public string AgentProfileId { get; set; }
        public string RuntimeInstanceId { get; set; }
        public string ExecutionId { get; set; }
        public AgentIdentityContext Identity { get; set; }

        public ContextAdmissionContext Clone()
        {
            return new ContextAdmissionContext
            {
                AgentProfileId = AgentProfileId,
                RuntimeInstanceId = RuntimeInstanceId,
                ExecutionId = ExecutionId,
                Identity = Identity == null ? new AgentIdentityContext() : Identity.Clone()
            };
        }

        public void Validate()
        {
            if (AgentProfileId != null && AgentProfileId.Length > 512)
                throw new ArgumentOutOfRangeException(nameof(AgentProfileId));
            if (RuntimeInstanceId != null && RuntimeInstanceId.Length > 512)
                throw new ArgumentOutOfRangeException(nameof(RuntimeInstanceId));
            if (ExecutionId != null && ExecutionId.Length > 512)
                throw new ArgumentOutOfRangeException(nameof(ExecutionId));
            if (Identity != null)
                Identity.Validate();
        }
    }
}
