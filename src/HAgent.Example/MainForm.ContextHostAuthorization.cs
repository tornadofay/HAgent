using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddContextHostAuthorizationTab()
        {
            AddApiTab(
                "Context Host Authorization",
                "Run host authorization test",
                "Exercises the opt-in host authorization boundary for a protected data-backed context source.",
                "Host authorization should receive the canonical identity and source identity, allow the source when authorized, and prevent source retrieval when denied or unavailable.",
                "No AI request is sent. The existing host IDataAccessAuthorizer remains authoritative for protected data access.",
                RunContextHostAuthorizationTestAsync,
                "Protected context host authorization",
                "Only sources that explicitly implement IContextDataAuthorizationSource enter the host authorization boundary.");
        }

        private async Task RunContextHostAuthorizationTestAsync(string unused)
        {
            var source = new ExampleAuthorizedContextSource();
            var hostAuthorizer = new RecordingHostAuthorizer(true);
            var policy = new AiPolicySet();
            policy.Validate();

            var capabilities = AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy());
            var admission = new ContextPolicyAdmissionEvaluator(
                new DefaultAiPolicyEngine(policy),
                capabilities);
            var assembler = new ContextPolicyAssembler(
                new ContextAcquirer(),
                admission,
                hostAuthorizer);

            var context = new ContextAdmissionContext
            {
                RuntimeInstanceId = "runtime-example-42",
                ExecutionId = "execution-example-42",
                Identity = new AgentIdentityContext(
                    deploymentId: "deployment-example",
                    tenantId: "tenant-example",
                    principalId: "principal-example",
                    userId: "user-example")
            };

            var result = await assembler.RetrieveCandidatesAsync(
                new[]
                {
                    new ContextRetrievalSource
                    {
                        Source = source,
                        Query = "orders",
                        MaxItems = 5
                    }
                },
                context,
                CancellationToken.None);

            if (result.Candidates.Count != 1 || source.CallCount != 1 || hostAuthorizer.CallCount != 1)
                throw new InvalidOperationException("Host-authorized context retrieval did not invoke the expected boundaries.");

            if (hostAuthorizer.LastRequest == null ||
                hostAuthorizer.LastRequest.SourceId != "orders" ||
                hostAuthorizer.LastRequest.RuntimeIdentity != "runtime-example-42" ||
                hostAuthorizer.LastRequest.Identity.PrincipalId != "principal-example")
                throw new InvalidOperationException("Host authorization did not receive the canonical context identity/source metadata.");

            if (result.Decisions.Count == 0 ||
                result.Decisions[0].HostAuthorizationState != ContextHostAuthorizationState.Allowed)
                throw new InvalidOperationException("Host authorization success was not retained as safe admission metadata.");

            var deniedSource = new ExampleAuthorizedContextSource();
            var deniedHost = new RecordingHostAuthorizer(false);
            var deniedAssembler = new ContextPolicyAssembler(
                new ContextAcquirer(),
                admission,
                deniedHost);
            var denied = await deniedAssembler.RetrieveCandidatesAsync(
                new[] { new ContextRetrievalSource { Source = deniedSource, Query = "orders", MaxItems = 5 } },
                context,
                CancellationToken.None);

            if (denied.Candidates.Count != 0 || deniedSource.CallCount != 0 ||
                denied.Decisions.Count == 0 || denied.Decisions[0].HostAuthorizationState != ContextHostAuthorizationState.Denied)
                throw new InvalidOperationException("Host denial did not prevent protected source retrieval.");

            Write(
                "CONTEXT HOST AUTHORIZATION",
                "Host authorization-aware context admission succeeded." + Environment.NewLine +
                "Canonical identity propagation: verified." + Environment.NewLine +
                "Host authorization allow path: verified." + Environment.NewLine +
                "Host authorization deny before source retrieval: verified." + Environment.NewLine +
                "Safe authorization diagnostics: verified." + Environment.NewLine +
                "Provider request: none.");
        }

        private sealed class ExampleAuthorizedContextSource : IContextSource, IContextDataAuthorizationSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public ExampleAuthorizedContextSource()
            {
                Id = "orders";
                Kind = "data";
                AuthorizationOperation = DataAccessOperation.ProjectionQuery;
                AuthorizationQuery = new DataQueryRequest
                {
                    Fields = new[] { "Id", "Customer" },
                    Take = 25
                };
                AuthorizationRuntimeContext = new Dictionary<string, object>
                {
                    { "FormId", "OrdersForm" }
                };
                _items = new[]
                {
                    new ContextItem
                    {
                        Id = "order-42",
                        Source = "orders",
                        Type = "record",
                        Payload = "authorized-order",
                        Provenance = new ContextProvenance
                        {
                            SourceKind = "data",
                            SourceId = "orders",
                            SourceVersion = "1",
                            Evidence = "Deterministic host authorization example"
                        },
                        Scope = new ContextScope { ScopeType = "User", ScopeId = "user-example" },
                        Trust = 1d,
                        Importance = 1d,
                        Relevance = 1d,
                        EstimatedCharacters = 16,
                        EstimatedTokens = 4
                    }
                };
            }

            public string Id { get; private set; }
            public string Kind { get; private set; }
            public int CallCount { get; private set; }
            public DataAccessOperation AuthorizationOperation { get; private set; }
            public DataQueryRequest AuthorizationQuery { get; private set; }
            public IReadOnlyDictionary<string, object> AuthorizationRuntimeContext { get; private set; }

            public Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(
                ContextSourceRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                CallCount++;
                return Task.FromResult(_items);
            }
        }

        private sealed class RecordingHostAuthorizer : IDataAccessAuthorizer
        {
            private readonly bool _allowed;

            public RecordingHostAuthorizer(bool allowed)
            {
                _allowed = allowed;
            }

            public int CallCount { get; private set; }
            public DataAuthorizationRequest LastRequest { get; private set; }

            public Task<bool> AuthorizeAsync(
                DataAuthorizationRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                CallCount++;
                LastRequest = request;
                return Task.FromResult(_allowed);
            }
        }
    }
}
