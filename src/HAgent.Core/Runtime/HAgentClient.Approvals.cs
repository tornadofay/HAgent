using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed partial class HAgentClient
    {
        private readonly IAiApprovalWorkflow _approvalWorkflow = new InMemoryAiApprovalWorkflow();

        public IAiApprovalWorkflow ApprovalWorkflow { get { return _approvalWorkflow; } }

        public Task<AiApprovalRequest> GetApprovalRequestAsync(string requestId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _approvalWorkflow.GetAsync(requestId, cancellationToken);
        }

        public Task<IReadOnlyList<AiApprovalRequest>> GetPendingApprovalRequestsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _approvalWorkflow.GetPendingAsync(cancellationToken);
        }

        public Task<AiApprovalRequest> ResolveApprovalRequestAsync(
            string requestId,
            bool approve,
            AgentIdentityContext responderIdentity,
            string reason = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _approvalWorkflow.ResolveAsync(
                requestId,
                approve ? AiApprovalRequestStatus.Approved : AiApprovalRequestStatus.Rejected,
                responderIdentity,
                reason,
                cancellationToken);
        }
    }
}
