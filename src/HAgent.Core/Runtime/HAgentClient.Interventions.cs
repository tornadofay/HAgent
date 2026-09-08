using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed partial class HAgentClient
    {
        private readonly IAiInterventionWorkflow _interventionWorkflow = new InMemoryAiInterventionWorkflow();

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

        public Task<AiInterventionRequest> ResolveInterventionRequestAsync(
            string requestId,
            AiInterventionRequestStatus resolution,
            AgentIdentityContext responderIdentity,
            string reason = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var controllable = _runtime as IInterventionControllableRuntime;
            if (controllable == null)
            {
                return _interventionWorkflow.ResolveAsync(
                    requestId,
                    resolution,
                    responderIdentity,
                    reason,
                    cancellationToken);
            }

            return controllable.InterventionCoordinator.ResolveInterventionAsync(
                requestId,
                resolution,
                responderIdentity,
                reason,
                cancellationToken);
        }
    }
}
