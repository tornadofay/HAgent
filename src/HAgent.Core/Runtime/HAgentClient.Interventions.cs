using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed partial class HAgentClient
    {
        public IAiInterventionWorkflow InterventionWorkflow { get { return _runtime.InterventionWorkflow; } }
        public AiInterventionCoordinator InterventionCoordinator { get { return _runtime.InterventionCoordinator; } }

        public event EventHandler<AgentExecutionEventArgs> ExecutionChanged
        {
            add { _runtime.ExecutionChanged += value; }
            remove { _runtime.ExecutionChanged -= value; }
        }

        public void RegisterInterventionTargetHandler(IAiInterventionTargetHandler handler)
        {
            _runtime.InterventionCoordinator.RegisterTargetHandler(handler);
        }

        public bool UnregisterInterventionTargetHandler(IAiInterventionTargetHandler handler)
        {
            return _runtime.InterventionCoordinator.UnregisterTargetHandler(handler);
        }

        public Task<AiInterventionRequest> GetInterventionRequestAsync(
            string requestId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _runtime.InterventionWorkflow.GetAsync(requestId, cancellationToken);
        }

        public Task<IReadOnlyList<AiInterventionRequest>> GetPendingInterventionRequestsAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _runtime.InterventionWorkflow.GetPendingAsync(cancellationToken);
        }

        public Task<IReadOnlyList<AiInterventionRequest>> SearchInterventionRequestsAsync(
            AiInterventionQuery query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _runtime.InterventionWorkflow.SearchAsync(query, cancellationToken);
        }

        public Task<AiInterventionRequest> ResolveInterventionRequestAsync(
            string requestId,
            AiInterventionRequestStatus resolution,
            AgentIdentityContext responderIdentity,
            string reason = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _runtime.InterventionCoordinator.ResolveAsync(
                requestId,
                resolution,
                responderIdentity,
                reason,
                cancellationToken);
        }

        public Task<AiInterventionApplicationResult> ApplyInterventionRequestAsync(
            string requestId,
            AgentIdentityContext responderIdentity,
            string reason = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _runtime.InterventionCoordinator.ApplyAsync(
                requestId,
                responderIdentity,
                reason,
                cancellationToken);
        }
    }
}
