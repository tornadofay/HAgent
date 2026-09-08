using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;

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
            Version = 0;
            TargetRevision = 0;
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

        /// <summary>
        /// Optimistic-concurrency version for durable stores and race-safe lifecycle transitions.
        /// </summary>
        public long Version { get; private set; }

        /// <summary>
        /// Revision of the target observed when the request was created.
        /// Target handlers use it to reject stale control requests.
        /// </summary>
        public long TargetRevision { get; private set; }

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
            AgentIdentityContext requesterIdentity,
            long targetRevision)
        {
            if (string.IsNullOrWhiteSpace(operation)) throw new ArgumentException("Operation is required.", nameof(operation));
            if (string.IsNullOrWhiteSpace(resourceType)) throw new ArgumentException("Resource type is required.", nameof(resourceType));
            if (targetRevision < 0) throw new ArgumentOutOfRangeException(nameof(targetRevision));

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
                CreatedAt = DateTimeOffset.UtcNow,
                Version = 0,
                TargetRevision = targetRevision
            };
        }

        internal void Resolve(AiInterventionRequestStatus status, AgentIdentityContext responderIdentity, string reason)
        {
            if (Status != AiInterventionRequestStatus.Pending)
                throw new InvalidOperationException("Intervention request is no longer pending: " + RequestId);
            if (status != AiInterventionRequestStatus.Approved && status != AiInterventionRequestStatus.Rejected &&
                status != AiInterventionRequestStatus.Cancelled && status != AiInterventionRequestStatus.Expired)
                throw new ArgumentOutOfRangeException(nameof(status), "Only an explicit resolution state can resolve a pending intervention request.");

            Status = status;
            ResponderIdentity = responderIdentity == null ? new AgentIdentityContext() : responderIdentity.Clone();
            ResolutionReason = reason ?? string.Empty;
            ResolvedAt = DateTimeOffset.UtcNow;
            ++Version;
        }

        internal void Complete(AgentIdentityContext responderIdentity, string reason)
        {
            if (Status != AiInterventionRequestStatus.Approved)
                throw new InvalidOperationException("Only an approved intervention request can be completed: " + RequestId);

            Status = AiInterventionRequestStatus.Completed;
            ResponderIdentity = responderIdentity == null ? new AgentIdentityContext() : responderIdentity.Clone();
            ResolutionReason = reason ?? string.Empty;
            ResolvedAt = DateTimeOffset.UtcNow;
            ++Version;
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
                ResolvedAt = ResolvedAt,
                Version = Version,
                TargetRevision = TargetRevision
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
            CancellationToken cancellationToken = default(CancellationToken),
            long targetRevision = 0);

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

        Task<AiInterventionRequest> ExpireAsync(
            string requestId,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<AiInterventionRequest> WaitForResolutionAsync(
            string requestId,
            TimeSpan pollInterval,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyList<AiInterventionRequest>> GetPendingAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyList<AiInterventionRequest>> SearchAsync(
            AiInterventionQuery query,
            CancellationToken cancellationToken = default(CancellationToken));
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
            CancellationToken cancellationToken = default(CancellationToken),
            long targetRevision = 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = AiInterventionRequest.Create(
                kind, targetKind, requestedAction, operation, resourceType, resourceId, correlationId,
                hostCorrelationId, agentProfileId, runtimeInstanceId, executionId, toolId, reason,
                requesterIdentity, targetRevision);
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

        public Task<AiInterventionRequest> ExpireAsync(
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
                if (request.Status == AiInterventionRequestStatus.Pending)
                    request.Resolve(AiInterventionRequestStatus.Expired, responderIdentity, reason);
                return Task.FromResult(request.Clone());
            }
        }

        public async Task<AiInterventionRequest> WaitForResolutionAsync(
            string requestId,
            TimeSpan pollInterval,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (pollInterval <= TimeSpan.Zero) pollInterval = TimeSpan.FromMilliseconds(50);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var request = await GetAsync(requestId, cancellationToken).ConfigureAwait(false);
                if (request == null) throw new InvalidOperationException("Intervention request was not found: " + requestId);
                if (request.Status != AiInterventionRequestStatus.Pending)
                    return request;
                await Task.Delay(pollInterval, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task<IReadOnlyList<AiInterventionRequest>> GetPendingAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SearchAsync(new AiInterventionQuery { Status = AiInterventionRequestStatus.Pending }, cancellationToken);
        }

        public Task<IReadOnlyList<AiInterventionRequest>> SearchAsync(
            AiInterventionQuery query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            query = query ?? new AiInterventionQuery();
            query.Validate();
            lock (_sync)
            {
                var results = _requests.Values
                    .Where(x => !query.Status.HasValue || x.Status == query.Status.Value)
                    .Where(x => !query.TargetKind.HasValue || x.TargetKind == query.TargetKind.Value)
                    .Where(x => string.IsNullOrWhiteSpace(query.TargetId) ||
                                string.Equals(x.ResourceId, query.TargetId, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(x.ExecutionId, query.TargetId, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(x.ToolId, query.TargetId, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(query.MaxResults)
                    .Select(x => x.Clone())
                    .ToList()
                    .AsReadOnly();
                return Task.FromResult<IReadOnlyList<AiInterventionRequest>>(results);
            }
        }
    }

    /// <summary>
    /// Store-backed intervention workflow. Lifecycle transitions use optimistic concurrency and therefore remain safe
    /// when more than one process resolves the same persisted request.
    /// </summary>
    public sealed class StoreBackedAiInterventionWorkflow : IAiInterventionWorkflow
    {
        private readonly IAiInterventionStore _store;

        public StoreBackedAiInterventionWorkflow(IAiInterventionStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<AiInterventionRequest> CreateAsync(
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
            CancellationToken cancellationToken = default(CancellationToken),
            long targetRevision = 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = AiInterventionRequest.Create(
                kind, targetKind, requestedAction, operation, resourceType, resourceId, correlationId,
                hostCorrelationId, agentProfileId, runtimeInstanceId, executionId, toolId, reason,
                requesterIdentity, targetRevision);
            await _store.CreateAsync(request, cancellationToken).ConfigureAwait(false);
            return request.Clone();
        }

        public Task<AiInterventionRequest> GetAsync(string requestId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _store.GetAsync(requestId, cancellationToken);
        }

        public async Task<AiInterventionRequest> ResolveAsync(
            string requestId,
            AiInterventionRequestStatus resolution,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await TransitionAsync(requestId, responderIdentity, reason, false, resolution, cancellationToken).ConfigureAwait(false);
        }

        public async Task<AiInterventionRequest> CompleteAsync(
            string requestId,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await TransitionAsync(requestId, responderIdentity, reason, true, AiInterventionRequestStatus.Completed, cancellationToken).ConfigureAwait(false);
        }

        public async Task<AiInterventionRequest> ExpireAsync(
            string requestId,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await TransitionAsync(requestId, responderIdentity, reason, false, AiInterventionRequestStatus.Expired, cancellationToken).ConfigureAwait(false);
        }

        private async Task<AiInterventionRequest> TransitionAsync(
            string requestId,
            AgentIdentityContext responderIdentity,
            string reason,
            bool complete,
            AiInterventionRequestStatus resolution,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(requestId)) throw new ArgumentException("Request ID is required.", nameof(requestId));
            for (var attempt = 0; attempt < 5; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var current = await _store.GetAsync(requestId, cancellationToken).ConfigureAwait(false);
                if (current == null) throw new InvalidOperationException("Intervention request was not found: " + requestId);
                var expectedVersion = current.Version;
                if (complete)
                    current.Complete(responderIdentity, reason);
                else
                    current.Resolve(resolution, responderIdentity, reason);

                if (await _store.TryUpdateAsync(current, expectedVersion, cancellationToken).ConfigureAwait(false))
                    return current.Clone();
            }

            throw new InvalidOperationException("Intervention request changed concurrently and could not be resolved safely: " + requestId);
        }

        public async Task<AiInterventionRequest> WaitForResolutionAsync(
            string requestId,
            TimeSpan pollInterval,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (pollInterval <= TimeSpan.Zero) pollInterval = TimeSpan.FromMilliseconds(100);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var request = await _store.GetAsync(requestId, cancellationToken).ConfigureAwait(false);
                if (request == null) throw new InvalidOperationException("Intervention request was not found: " + requestId);
                if (request.Status != AiInterventionRequestStatus.Pending)
                    return request;
                await Task.Delay(pollInterval, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task<IReadOnlyList<AiInterventionRequest>> GetPendingAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SearchAsync(new AiInterventionQuery { Status = AiInterventionRequestStatus.Pending }, cancellationToken);
        }

        public Task<IReadOnlyList<AiInterventionRequest>> SearchAsync(
            AiInterventionQuery query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _store.SearchAsync(query, cancellationToken);
        }
    }
}
