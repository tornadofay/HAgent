using System;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Models
{
    public enum AiInterventionApplicationStatus
    {
        Applied,
        Stale,
        NotFound,
        NotApplicable,
        Rejected,
        Failed
    }

    public sealed class AiInterventionApplicationResult
    {
        public AiInterventionApplicationStatus Status { get; set; }
        public string RequestId { get; set; }
        public string Reason { get; set; }

        public bool Applied
        {
            get { return Status == AiInterventionApplicationStatus.Applied; }
        }

        public bool IsStale
        {
            get { return Status == AiInterventionApplicationStatus.Stale; }
        }

        public static AiInterventionApplicationResult Create(
            AiInterventionApplicationStatus status,
            string requestId,
            string reason)
        {
            return new AiInterventionApplicationResult
            {
                Status = status,
                RequestId = requestId ?? string.Empty,
                Reason = reason ?? string.Empty
            };
        }
    }

    /// <summary>
    /// Runtime-owned target boundary for intervention application.
    /// Implementations are executable runtime objects and are never persisted.
    /// </summary>
    public interface IAiInterventionTargetHandler
    {
        bool CanHandle(AiInterventionRequest request);

        Task<AiInterventionApplicationResult> ApplyAsync(
            AiInterventionRequest request,
            CancellationToken cancellationToken = default(CancellationToken));
    }

    public sealed class AiInterventionAuthorizationRequest
    {
        public AiInterventionRequest Intervention { get; private set; }
        public AgentIdentityContext ResponderIdentity { get; private set; }

        public AiInterventionAuthorizationRequest(
            AiInterventionRequest intervention,
            AgentIdentityContext responderIdentity)
        {
            Intervention = intervention == null ? null : intervention.Clone();
            ResponderIdentity = responderIdentity == null
                ? new AgentIdentityContext()
                : responderIdentity.Clone();
        }
    }

    /// <summary>
    /// Optional host authorization callback for intervention control.
    /// HAgent does not authenticate the responder.
    /// </summary>
    public interface IAiInterventionAuthorizer
    {
        Task<bool> AuthorizeAsync(
            AiInterventionAuthorizationRequest request,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
