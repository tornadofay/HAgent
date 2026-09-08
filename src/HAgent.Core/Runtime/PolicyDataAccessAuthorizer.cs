using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Composes HAgent policy enforcement with the host-owned data authorization callback.
    /// HAgent policy can further restrict an operation, but an allow decision never grants
    /// host authorization by itself.
    /// </summary>
    public sealed class PolicyDataAccessAuthorizer : IDataAccessAuthorizer
    {
        private readonly IAiPolicyEngine _policyEngine;
        private readonly IDataAccessAuthorizer _hostAuthorizer;

        public PolicyDataAccessAuthorizer(IAiPolicyEngine policyEngine, IDataAccessAuthorizer hostAuthorizer)
        {
            _policyEngine = policyEngine ?? throw new ArgumentNullException(nameof(policyEngine));
            _hostAuthorizer = hostAuthorizer ?? throw new ArgumentNullException(nameof(hostAuthorizer));
        }

        public async Task<bool> AuthorizeAsync(DataAuthorizationRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            cancellationToken.ThrowIfCancellationRequested();

            var identity = request.Identity == null ? new AgentIdentityContext() : request.Identity.Clone();
            identity.Validate();

            var decision = _policyEngine.Evaluate(new AiPolicyEvaluationContext
            {
                Operation = GetPolicyOperation(request.Operation),
                ResourceType = "data-source",
                ResourceId = request.SourceId ?? string.Empty,
                Identity = identity
            });

            if (decision == null)
                throw new InvalidOperationException("The policy engine returned no data authorization decision.");

            if (decision.IsDenied || decision.RequiresApproval || decision.IsDeferred)
                return false;

            // Host authorization remains authoritative. Policy Allow is only permission to ask
            // the host authority; it can never bypass or replace that host decision.
            return await _hostAuthorizer.AuthorizeAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private static string GetPolicyOperation(DataAccessOperation operation)
        {
            switch (operation)
            {
                case DataAccessOperation.Discovery:
                    return "data.discovery";
                case DataAccessOperation.ProjectionQuery:
                    return "data.query";
                case DataAccessOperation.Export:
                    return "data.export";
                case DataAccessOperation.Write:
                    return "data.write";
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unsupported data access operation.");
            }
        }
    }
}
