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

        public Task<AiInterventionRequest> GetInterventionRequestAsync(string requestId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _interventionWorkflow.GetAsync(requestId, cancellationToken);
        }

        public Task<IReadOnlyList<AiInterventionRequest>> GetPendingInterventionRequestsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _interventionWorkflow.GetPendingAsync(cancellationToken);
        }

        public Task<AiInterventionRequest> ResolveInterventionRequestAsync(
            string requestId,
            AiInterventionRequestStatus resolution,
            AgentIdentityContext responderIdentity,
            string reason = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _interventionWorkflow.ResolveAsync(
                requestId,
                resolution,
                responderIdentity,
                reason,
                cancellationToken);
        }
    }
}
