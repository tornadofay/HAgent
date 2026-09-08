using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Models
{
    public enum AiApprovalRequestKind
    {
        Approval,
        Deferral
    }

    public enum AiApprovalRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled,
        Expired
    }

    public sealed class AiApprovalRequest
    {
        public AiApprovalRequest()
        {
            RequestId = Guid.NewGuid().ToString("N");
            Kind = AiApprovalRequestKind.Approval;
            Status = AiApprovalRequestStatus.Pending;
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
            RequesterIdentity = new AgentIdentityContext();
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public string RequestId { get; private set; }
        public AiApprovalRequestKind Kind { get; private set; }
        public AiApprovalRequestStatus Status { get; private set; }
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

        internal static AiApprovalRequest Create(
            AiApprovalRequestKind kind,
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

            return new AiApprovalRequest
            {
                RequestId = Guid.NewGuid().ToString("N"),
                Kind = kind,
                Status = AiApprovalRequestStatus.Pending,
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

        internal void Resolve(AiApprovalRequestStatus status, AgentIdentityContext responderIdentity, string reason)
        {
            if (Status != AiApprovalRequestStatus.Pending)
                throw new InvalidOperationException("Approval request is no longer pending: " + RequestId);
            if (status != AiApprovalRequestStatus.Approved && status != AiApprovalRequestStatus.Rejected &&
                status != AiApprovalRequestStatus.Cancelled && status != AiApprovalRequestStatus.Expired)
                throw new ArgumentOutOfRangeException(nameof(status));

            Status = status;
            ResponderIdentity = responderIdentity == null ? new AgentIdentityContext() : responderIdentity.Clone();
            ResolutionReason = reason ?? string.Empty;
            ResolvedAt = DateTimeOffset.UtcNow;
        }

        public AiApprovalRequest Clone()
        {
            var clone = new AiApprovalRequest
            {
                RequestId = RequestId,
                Kind = Kind,
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
            return clone;
        }
    }

    public interface IAiApprovalWorkflow
    {
        Task<AiApprovalRequest> CreateAsync(
            AiApprovalRequestKind kind,
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

        Task<AiApprovalRequest> GetAsync(string requestId, CancellationToken cancellationToken = default(CancellationToken));

        Task<AiApprovalRequest> ResolveAsync(
            string requestId,
            AiApprovalRequestStatus resolution,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyList<AiApprovalRequest>> GetPendingAsync(CancellationToken cancellationToken = default(CancellationToken));
    }

    public sealed class InMemoryAiApprovalWorkflow : IAiApprovalWorkflow
    {
        private readonly object _sync = new object();
        private readonly Dictionary<string, AiApprovalRequest> _requests = new Dictionary<string, AiApprovalRequest>(StringComparer.OrdinalIgnoreCase);

        public Task<AiApprovalRequest> CreateAsync(
            AiApprovalRequestKind kind,
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
            var request = AiApprovalRequest.Create(
                kind, operation, resourceType, resourceId, correlationId, hostCorrelationId,
                agentProfileId, runtimeInstanceId, executionId, toolId, reason, requesterIdentity);
            lock (_sync)
            {
                _requests.Add(request.RequestId, request);
            }
            return Task.FromResult(request.Clone());
        }

        public Task<AiApprovalRequest> GetAsync(string requestId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(requestId)) throw new ArgumentException("Request ID is required.", nameof(requestId));
            lock (_sync)
            {
                AiApprovalRequest request;
                if (!_requests.TryGetValue(requestId, out request))
                    return Task.FromResult<AiApprovalRequest>(null);
                return Task.FromResult(request.Clone());
            }
        }

        public Task<AiApprovalRequest> ResolveAsync(
            string requestId,
            AiApprovalRequestStatus resolution,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(requestId)) throw new ArgumentException("Request ID is required.", nameof(requestId));
            lock (_sync)
            {
                AiApprovalRequest request;
                if (!_requests.TryGetValue(requestId, out request))
                    throw new InvalidOperationException("Approval request was not found: " + requestId);
                request.Resolve(resolution, responderIdentity, reason);
                return Task.FromResult(request.Clone());
            }
        }

        public Task<IReadOnlyList<AiApprovalRequest>> GetPendingAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_sync)
            {
                var pending = _requests.Values
                    .Where(x => x.Status == AiApprovalRequestStatus.Pending)
                    .Select(x => x.Clone())
                    .ToList()
                    .AsReadOnly();
                return Task.FromResult<IReadOnlyList<AiApprovalRequest>>(pending);
            }
        }
    }
}
