using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public class ContextHostAuthorizationTests
    {
        [Fact]
        public async Task HostAuthorizedSourceInvokesHostBoundaryAndRetrievesCandidates()
        {
            var source = new RecordingDataSource("orders", true);
            var host = new RecordingHostAuthorizer(true);
            var result = await CreateAssembler(new AiPolicySet(), host).RetrieveCandidatesAsync(
                new[] { CreatePlan(source) },
                CreateContext(),
                CancellationToken.None);

            Assert.Single(result.Candidates);
            Assert.Equal(1, source.CallCount);
            Assert.Equal(1, host.CallCount);
            Assert.Equal(DataAccessOperation.ProjectionQuery, host.LastRequest.Operation);
            Assert.Equal("orders", host.LastRequest.SourceId);
            Assert.Equal("runtime-42", host.LastRequest.RuntimeIdentity);
            Assert.Equal("principal-42", host.LastRequest.Identity.PrincipalId);
            Assert.Equal(ContextHostAuthorizationState.Allowed, result.Decisions[0].HostAuthorizationState);
        }

        [Fact]
        public async Task HostDeniedSourceNeverRetrievesCandidates()
        {
            var source = new RecordingDataSource("orders", true);
            var host = new RecordingHostAuthorizer(false);
            var result = await CreateAssembler(new AiPolicySet(), host).RetrieveCandidatesAsync(
                new[] { CreatePlan(source) },
                CreateContext(),
                CancellationToken.None);

            Assert.Empty(result.Candidates);
            Assert.Equal(0, source.CallCount);
            Assert.Equal(1, host.CallCount);
            Assert.False(result.Decisions[0].Allowed);
            Assert.Equal(ContextHostAuthorizationState.Denied, result.Decisions[0].HostAuthorizationState);
            Assert.Contains("host data authorization", result.Decisions[0].Reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PolicyDenyPrecedesHostAuthorization()
        {
            var source = new RecordingDataSource("orders", true);
            var host = new RecordingHostAuthorizer(true);
            var policy = new AiPolicySet();
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "deny-orders",
                Outcome = AiPolicyOutcome.Deny,
                Operations = { "context.retrieve" },
                ResourceTypes = { "data" },
                ResourceIds = { "orders" },
                Reason = "Policy denies this protected context source."
            });
            policy.Validate();

            var result = await CreateAssembler(policy, host, sourceKindOverride: "data").RetrieveCandidatesAsync(
                new[] { CreatePlan(source) },
                CreateContext(),
                CancellationToken.None);

            Assert.Empty(result.Candidates);
            Assert.Equal(0, source.CallCount);
            Assert.Equal(0, host.CallCount);
            Assert.Equal(ContextHostAuthorizationState.NotRequired, result.Decisions[0].HostAuthorizationState);
        }

        [Fact]
        public async Task HostAuthorizationReceivesClonedIdentityAndBoundedRuntimeContext()
        {
            var source = new RecordingDataSource("orders", true);
            var host = new RecordingHostAuthorizer(true);
            var context = CreateContext();
            var runtimeContext = source.AuthorizationRuntimeContext as IDictionary<string, object>;
            runtimeContext["FormId"] = "OrdersForm";

            var result = await CreateAssembler(new AiPolicySet(), host).RetrieveCandidatesAsync(
                new[] { CreatePlan(source) }, context, CancellationToken.None);

            Assert.True(result.Decisions[0].Allowed);
            Assert.NotSame(context.Identity, host.LastRequest.Identity);
            Assert.Equal("principal-42", host.LastRequest.Identity.PrincipalId);
            Assert.Equal("OrdersForm", host.LastRequest.RuntimeContext["FormId"]);

            Assert.NotNull(host.LastRequest.Query);
            Assert.NotSame(source.AuthorizationQuery, host.LastRequest.Query);
            source.AuthorizationQuery.Take = 1;
            Assert.Equal(25, host.LastRequest.Query.Take);
        }

        [Fact]
        public async Task MissingHostAuthorizerFailsClosedForProtectedSource()
        {
            var source = new RecordingDataSource("orders", true);
            var result = await CreateAssembler(new AiPolicySet(), null).RetrieveCandidatesAsync(
                new[] { CreatePlan(source) },
                CreateContext(),
                CancellationToken.None);

            Assert.Empty(result.Candidates);
            Assert.Equal(0, source.CallCount);
            Assert.False(result.Decisions[0].Allowed);
            Assert.Equal(ContextHostAuthorizationState.Denied, result.Decisions[0].HostAuthorizationState);
            Assert.Contains("unavailable", result.Decisions[0].HostAuthorizationReason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task HostAuthorizationCancellationPropagatesBeforeSourceRetrieval()
        {
            var source = new RecordingDataSource("orders", true);
            var host = new RecordingHostAuthorizer(true) { ThrowCancellation = true };

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                CreateAssembler(new AiPolicySet(), host).RetrieveCandidatesAsync(
                    new[] { CreatePlan(source) },
                    CreateContext(),
                    CancellationToken.None));

            Assert.Equal(1, host.CallCount);
            Assert.Equal(0, source.CallCount);
        }

        private static ContextRetrievalSource CreatePlan(RecordingDataSource source)
        {
            return new ContextRetrievalSource
            {
                Source = source,
                Query = "orders",
                MaxItems = 5
            };
        }

        private static ContextAdmissionContext CreateContext()
        {
            return new ContextAdmissionContext
            {
                RuntimeInstanceId = "runtime-42",
                ExecutionId = "execution-42",
                AgentProfileId = "assistant",
                Identity = new AgentIdentityContext(
                    deploymentId: "deployment-42",
                    tenantId: "tenant-42",
                    principalId: "principal-42",
                    userId: "user-42",
                    sessionId: "session-42",
                    workspaceId: "workspace-42")
            };
        }

        private static ContextPolicyAssembler CreateAssembler(
            AiPolicySet policy,
            IDataAccessAuthorizer hostAuthorizer,
            string sourceKindOverride = null)
        {
            policy.Validate();
            var engine = new DefaultAiPolicyEngine(policy);
            var capabilities = AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy());
            var admission = new ContextPolicyAdmissionEvaluator(engine, capabilities);
            return new ContextPolicyAssembler(new ContextAcquirer(), admission, hostAuthorizer);
        }

        private sealed class RecordingDataSource : IContextSource, IContextDataAuthorizationSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public RecordingDataSource(string id, bool includeItems)
            {
                Id = id;
                Kind = "data";
                AuthorizationOperation = DataAccessOperation.ProjectionQuery;
                AuthorizationQuery = new DataQueryRequest
                {
                    Fields = new[] { "Id", "Customer" },
                    Take = 25
                };
                AuthorizationRuntimeContext = new Dictionary<string, object>();
                _items = includeItems
                    ? new[] { CreateItem(id) }
                    : new ContextItem[0];
            }

            public string Id { get; private set; }
            public string Kind { get; private set; }
            public int CallCount { get; private set; }
            public DataAccessOperation AuthorizationOperation { get; private set; }
            public DataQueryRequest AuthorizationQuery { get; set; }
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
            public bool ThrowCancellation { get; set; }

            public Task<bool> AuthorizeAsync(
                DataAuthorizationRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                CallCount++;
                LastRequest = request;
                if (ThrowCancellation)
                    throw new OperationCanceledException(cancellationToken);
                return Task.FromResult(_allowed);
            }
        }

        private static ContextItem CreateItem(string sourceId)
        {
            return new ContextItem
            {
                Id = "order-1",
                Source = sourceId,
                Type = "record",
                Payload = "authorized-order",
                Provenance = new ContextProvenance
                {
                    SourceKind = "data",
                    SourceId = sourceId,
                    SourceVersion = "1",
                    Evidence = "deterministic host authorization test"
                },
                Scope = new ContextScope { ScopeType = "User", ScopeId = "user-42" },
                Trust = 1d,
                Importance = 1d,
                Relevance = 1d,
                EstimatedCharacters = 16,
                EstimatedTokens = 4
            };
        }
    }
}
