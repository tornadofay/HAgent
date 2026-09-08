using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed partial class HAgentClient
    {
        private readonly IAiInterventionWorkflow _interventionWorkflow = new InMemoryAiInterventionWorkflow();
        private readonly ConcurrentDictionary<string, AiLearningCandidate> _interventionLearningCandidates = new ConcurrentDictionary<string, AiLearningCandidate>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _learningCandidateInterventionGates = new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        public IAiInterventionWorkflow InterventionWorkflow { get { return _interventionWorkflow; } }

        public event EventHandler<AgentExecutionEventArgs> ExecutionChanged
        {
            add { _runtime.ExecutionChanged += value; }
            remove { _runtime.ExecutionChanged -= value; }
        }

        public Task<AiInterventionRequest> GetInterventionRequestAsync(string requestId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _interventionWorkflow.GetAsync(requestId, cancellationToken);
        }

        public Task<IReadOnlyList<AiInterventionRequest>> GetPendingInterventionRequestsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _interventionWorkflow.GetPendingAsync(cancellationToken);
        }

        public Task<AiExecutionControlState> GetExecutionControlStateAsync(
            string executionId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var controllable = _runtime as IInterventionControllableRuntime;
            if (controllable == null)
                throw new InvalidOperationException("The configured runtime does not support execution intervention control.");
            return controllable.InterventionCoordinator.GetExecutionControlStateAsync(executionId, cancellationToken);
        }

        public Task<AiInterventionRequest> RequestExecutionInterventionAsync(
            string executionId,
            AiInterventionAction requestedAction,
            AgentIdentityContext requesterIdentity,
            string hostCorrelationId = null,
            string reason = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var controllable = _runtime as IInterventionControllableRuntime;
            if (controllable == null)
                throw new InvalidOperationException("The configured runtime does not support execution intervention control.");
            return controllable.InterventionCoordinator.RequestExecutionInterventionAsync(
                executionId,
                requestedAction,
                requesterIdentity,
                hostCorrelationId,
                reason,
                cancellationToken);
        }

        public async Task<AiInterventionRequest> RequestLearningCandidateInterventionAsync(
            AiLearningCandidate candidate,
            AiInterventionAction requestedAction,
            AgentIdentityContext requesterIdentity,
            string hostCorrelationId = null,
            string reason = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            if (string.IsNullOrWhiteSpace(candidate.Id)) throw new ArgumentException("Learning candidate ID is required.", nameof(candidate));
            if (requestedAction != AiInterventionAction.Approve && requestedAction != AiInterventionAction.Reject)
                throw new ArgumentException("Learning candidate intervention currently supports Approve or Reject.", nameof(requestedAction));

            var snapshot = candidate.GetSnapshot();
            if (requestedAction == AiInterventionAction.Approve && snapshot.State != AiLearningCandidateStatus.PendingReview)
                throw new InvalidOperationException("A learning candidate can only be approved from PendingReview.");

            _interventionLearningCandidates[candidate.Id] = candidate;
            var operation = requestedAction == AiInterventionAction.Approve
                ? "learning-candidate.approve"
                : "learning-candidate.reject";

            return await _interventionWorkflow.CreateAsync(
                AiInterventionRequestKind.Intervention,
                AiInterventionTargetKind.LearningCandidate,
                requestedAction,
                operation,
                "learning-candidate",
                candidate.Id,
                candidate.Id,
                hostCorrelationId,
                candidate.SourceAgentProfileId,
                candidate.SourceRuntimeInstanceId,
                candidate.SourceExecutionId,
                string.Empty,
                reason,
                requesterIdentity,
                cancellationToken,
                snapshot.State.ToString(),
                snapshot.Revision).ConfigureAwait(false);
        }

        public async Task<AiInterventionRequest> ResolveInterventionRequestAsync(
            string requestId,
            AiInterventionRequestStatus resolution,
            AgentIdentityContext responderIdentity,
            string reason = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = await _interventionWorkflow.GetAsync(requestId, cancellationToken).ConfigureAwait(false);
            if (request == null)
                throw new InvalidOperationException("Intervention request was not found: " + requestId);

            if (request.TargetKind == AiInterventionTargetKind.LearningCandidate &&
                (request.RequestedAction == AiInterventionAction.Approve || request.RequestedAction == AiInterventionAction.Reject) &&
                resolution == AiInterventionRequestStatus.Approved)
            {
                return await ResolveLearningCandidateInterventionAsync(request, responderIdentity, reason, cancellationToken).ConfigureAwait(false);
            }

            var controllable = _runtime as IInterventionControllableRuntime;
            if (controllable == null)
            {
                return await _interventionWorkflow.ResolveAsync(
                    requestId,
                    resolution,
                    responderIdentity,
                    reason,
                    cancellationToken).ConfigureAwait(false);
            }

            return await controllable.InterventionCoordinator.ResolveInterventionAsync(
                requestId,
                resolution,
                responderIdentity,
                reason,
                cancellationToken).ConfigureAwait(false);
        }

        private async Task<AiInterventionRequest> ResolveLearningCandidateInterventionAsync(
            AiInterventionRequest request,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken)
        {
            AiLearningCandidate candidate;
            if (!_interventionLearningCandidates.TryGetValue(request.ResourceId ?? string.Empty, out candidate))
            {
                return await _interventionWorkflow.ResolveAsync(
                    request.RequestId,
                    AiInterventionRequestStatus.Expired,
                    responderIdentity,
                    "The learning candidate is no longer available to the active runtime.",
                    cancellationToken).ConfigureAwait(false);
            }

            var gate = _learningCandidateInterventionGates.GetOrAdd(candidate.Id, delegate { return new SemaphoreSlim(1, 1); });
            await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var current = await _interventionWorkflow.GetAsync(request.RequestId, cancellationToken).ConfigureAwait(false);
                if (current == null)
                    throw new InvalidOperationException("Intervention request was not found: " + request.RequestId);
                if (current.Status != AiInterventionRequestStatus.Pending)
                    throw new InvalidOperationException("Intervention request is no longer pending: " + request.RequestId);

                string staleReason;
                if (!candidate.TryApplyIntervention(
                    current.RequestedAction,
                    current.TargetState,
                    current.TargetStateVersion,
                    out staleReason))
                {
                    return await _interventionWorkflow.ResolveAsync(
                        current.RequestId,
                        AiInterventionRequestStatus.Expired,
                        responderIdentity,
                        string.IsNullOrWhiteSpace(staleReason)
                            ? "The learning candidate changed before the intervention could be applied."
                            : staleReason,
                        cancellationToken).ConfigureAwait(false);
                }

                var approved = await _interventionWorkflow.ResolveAsync(
                    current.RequestId,
                    AiInterventionRequestStatus.Approved,
                    responderIdentity,
                    reason,
                    cancellationToken).ConfigureAwait(false);

                return await _interventionWorkflow.CompleteAsync(
                    approved.RequestId,
                    responderIdentity,
                    string.IsNullOrWhiteSpace(reason)
                        ? "Learning candidate intervention was applied."
                        : reason,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                gate.Release();
            }
        }
    }
}
