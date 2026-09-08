using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Models
{
    public enum AiInterventionRequestKind
    {
        Approval,
        Deferral,
        Intervention
    }

    public enum AiInterventionTargetKind
    {
        Execution,
        Tool,
        PlanStep,
        Goal,
        LearningCandidate,
        ConsequentialAction
    }

    public enum AiInterventionAction
    {
        Inspect,
        Approve,
        Reject,
        Pause,
        Resume,
        Cancel,
        Retire,
        Shutdown,
        Redirect,
        Defer
    }

    public enum AiInterventionRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled,
        Expired,
        Completed
    }

    public sealed class AiInterventionRequest
    {
        public AiInterventionRequest()
        {
            RequestId = Guid.NewGuid().ToString("N");
            Kind = AiInterventionRequestKind.Approval;
            TargetKind = AiInterventionTargetKind.ConsequentialAction;
            RequestedAction = AiInterventionAction.Approve;
            Status = AiInterventionRequestStatus.Pending;
            Operation = string.Empty;
            ResourceType = string.Empty;
            ResourceId = string.Empty;
            CorrelationId = string.Empty;
            HostCorrelationId = string.Empty;
            AgentProfileId = string.Empty;
            RuntimeInstanceId = string.Empty;
            ExecutionId = string.Empty;
            ToolId = string.Empty;
            Reason = string.Empty;
            ResolutionReason = string.Empty;
            RequesterIdentity = new AgentIdentityContext();
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public string RequestId { get; private set; }
        public AiInterventionRequestKind Kind { get; private set; }
        public AiInterventionTargetKind TargetKind { get; private set; }
        public AiInterventionAction RequestedAction { get; private set; }
        public AiInterventionRequestStatus Status { get; private set; }
        public string Operation { get; private set; }
        public string ResourceType { get; private set; }
        public string ResourceId { get; private set; }
        public string CorrelationId { get; private set; }
        public string HostCorrelationId { get; private set; }
        public string AgentProfileId { get; private set; }
        public string RuntimeInstanceId { get; private set; }
        public string ExecutionId { get; private set; }
        public string ToolId { get; private set; }
        public string Reason { get; private set; }
        public AgentIdentityContext RequesterIdentity { get; private set; }
        public AgentIdentityContext ResponderIdentity { get; private set; }
        public string ResolutionReason { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? ResolvedAt { get; private set; }

        internal static AiInterventionRequest Create(
            AiInterventionRequestKind kind,
            AiInterventionTargetKind targetKind,
            AiInterventionAction requestedAction,
            string operation,
            string resourceType,
            string resourceId,
            string correlationId,
            string hostCorrelationId,
            string agentProfileId,
            string runtimeInstanceId,
            string executionId,
            string toolId,
            string reason,
            AgentIdentityContext requesterIdentity)
        {
            if (string.IsNullOrWhiteSpace(operation)) throw new ArgumentException("Operation is required.", nameof(operation));
            if (string.IsNullOrWhiteSpace(resourceType)) throw new ArgumentException("Resource type is required.", nameof(resourceType));

            return new AiInterventionRequest
            {
                RequestId = Guid.NewGuid().ToString("N"),
                Kind = kind,
                TargetKind = targetKind,
                RequestedAction = requestedAction,
                Status = AiInterventionRequestStatus.Pending,
                Operation = operation.Trim(),
                ResourceType = resourceType.Trim(),
                ResourceId = resourceId == null ? string.Empty : resourceId.Trim(),
                CorrelationId = correlationId ?? string.Empty,
                HostCorrelationId = hostCorrelationId ?? string.Empty,
                AgentProfileId = agentProfileId ?? string.Empty,
                RuntimeInstanceId = runtimeInstanceId ?? string.Empty,
                ExecutionId = executionId ?? string.Empty,
                ToolId = toolId ?? string.Empty,
                Reason = reason ?? string.Empty,
                RequesterIdentity = requesterIdentity == null ? new AgentIdentityContext() : requesterIdentity.Clone(),
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        internal void Resolve(AiInterventionRequestStatus status, AgentIdentityContext responderIdentity, string reason)
        {
            if (Status != AiInterventionRequestStatus.Pending)
                throw new InvalidOperationException("Intervention request is no longer pending: " + RequestId);
            if (status != AiInterventionRequestStatus.Approved &&
                status != AiInterventionRequestStatus.Rejected &&
                status != AiInterventionRequestStatus.Cancelled &&
                status != AiInterventionRequestStatus.Expired)
                throw new ArgumentOutOfRangeException(nameof(status));

            Status = status;
            ResponderIdentity = responderIdentity == null ? new AgentIdentityContext() : responderIdentity.Clone();
            ResolutionReason = reason ?? string.Empty;
            ResolvedAt = DateTimeOffset.UtcNow;
        }

        internal void Complete(AgentIdentityContext responderIdentity, string reason)
        {
            if (Status != AiInterventionRequestStatus.Approved)
                throw new InvalidOperationException("Only an approved intervention request can be completed: " + RequestId);

            Status = AiInterventionRequestStatus.Completed;
            if (responderIdentity != null)
                ResponderIdentity = responderIdentity.Clone();
            ResolutionReason = reason ?? ResolutionReason ?? string.Empty;
        }

        public AiInterventionRequest Clone()
        {
            return new AiInterventionRequest
            {
                RequestId = RequestId,
                Kind = Kind,
                TargetKind = TargetKind,
                RequestedAction = RequestedAction,
                Status = Status,
                Operation = Operation,
                ResourceType = ResourceType,
                ResourceId = ResourceId,
                CorrelationId = CorrelationId,
                HostCorrelationId = HostCorrelationId,
                AgentProfileId = AgentProfileId,
                RuntimeInstanceId = RuntimeInstanceId,
                ExecutionId = ExecutionId,
                ToolId = ToolId,
                Reason = Reason,
                RequesterIdentity = RequesterIdentity == null ? new AgentIdentityContext() : RequesterIdentity.Clone(),
                ResponderIdentity = ResponderIdentity == null ? null : ResponderIdentity.Clone(),
                ResolutionReason = ResolutionReason ?? string.Empty,
                CreatedAt = CreatedAt,
                ResolvedAt = ResolvedAt
            };
        }
    }

    public interface IAiInterventionWorkflow
    {
        Task<AiInterventionRequest> CreateAsync(
            AiInterventionRequestKind kind,
            AiInterventionTargetKind targetKind,
            AiInterventionAction requestedAction,
            string operation,
            string resourceType,
            string resourceId,
            string correlationId,
            string hostCorrelationId,
            string agentProfileId,
            string runtimeInstanceId,
            string executionId,
            string toolId,
            string reason,
            AgentIdentityContext requesterIdentity,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<AiInterventionRequest> GetAsync(string requestId, CancellationToken cancellationToken = default(CancellationToken));

        Task<AiInterventionRequest> ResolveAsync(
            string requestId,
            AiInterventionRequestStatus resolution,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<AiInterventionRequest> CompleteAsync(
            string requestId,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyList<AiInterventionRequest>> GetPendingAsync(CancellationToken cancellationToken = default(CancellationToken));
    }

    public sealed class InMemoryAiInterventionWorkflow : IAiInterventionWorkflow
    {
        private readonly object _sync = new object();
        private readonly Dictionary<string, AiInterventionRequest> _requests = new Dictionary<string, AiInterventionRequest>(StringComparer.OrdinalIgnoreCase);

        public Task<AiInterventionRequest> CreateAsync(
            AiInterventionRequestKind kind,
            AiInterventionTargetKind targetKind,
            AiInterventionAction requestedAction,
            string operation,
            string resourceType,
            string resourceId,
            string correlationId,
            string hostCorrelationId,
            string agentProfileId,
            string runtimeInstanceId,
            string executionId,
            string toolId,
            string reason,
            AgentIdentityContext requesterIdentity,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = AiInterventionRequest.Create(
                kind, targetKind, requestedAction, operation, resourceType, resourceId, correlationId,
                hostCorrelationId, agentProfileId, runtimeInstanceId, executionId, toolId, reason, requesterIdentity);
            lock (_sync)
            {
                _requests.Add(request.RequestId, request);
            }
            return Task.FromResult(request.Clone());
        }

        public Task<AiInterventionRequest> GetAsync(string requestId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(requestId)) throw new ArgumentException("Request ID is required.", nameof(requestId));
            lock (_sync)
            {
                AiInterventionRequest request;
                if (!_requests.TryGetValue(requestId, out request))
                    return Task.FromResult<AiInterventionRequest>(null);
                return Task.FromResult(request.Clone());
            }
        }

        public Task<AiInterventionRequest> ResolveAsync(
            string requestId,
            AiInterventionRequestStatus resolution,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(requestId)) throw new ArgumentException("Request ID is required.", nameof(requestId));
            lock (_sync)
            {
                AiInterventionRequest request;
                if (!_requests.TryGetValue(requestId, out request))
                    throw new InvalidOperationException("Intervention request was not found: " + requestId);
                request.Resolve(resolution, responderIdentity, reason);
                return Task.FromResult(request.Clone());
            }
        }

        public Task<AiInterventionRequest> CompleteAsync(
            string requestId,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(requestId)) throw new ArgumentException("Request ID is required.", nameof(requestId));
            lock (_sync)
            {
                AiInterventionRequest request;
                if (!_requests.TryGetValue(requestId, out request))
                    throw new InvalidOperationException("Intervention request was not found: " + requestId);
                request.Complete(responderIdentity, reason);
                return Task.FromResult(request.Clone());
            }
        }

        public Task<IReadOnlyList<AiInterventionRequest>> GetPendingAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_sync)
            {
                var pending = _requests.Values
                    .Where(x => x.Status == AiInterventionRequestStatus.Pending)
                    .Select(x => x.Clone())
                    .ToList()
                    .AsReadOnly();
                return Task.FromResult<IReadOnlyList<AiInterventionRequest>>(pending);
            }
        }
    }
}
