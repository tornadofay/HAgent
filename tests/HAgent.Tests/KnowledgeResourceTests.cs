using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class KnowledgeResourceTests
    {
        [Fact]
        public void KnowledgeResource_DefaultsToNonAuthoritativeDraftAndPreservesMetadata()
        {
            var resource = CreateResource(AiKnowledgeResourceKind.Wiki, "wiki-42", "owner-42");
            resource.Provenance = new AiKnowledgeProvenance
            {
                Kind = AiKnowledgeProvenanceKind.ModelGenerated,
                Source = "model",
                SourceExecutionId = "execution-42",
                SourceRuntimeInstanceId = "runtime-42",
                Evidence = "repeated successful answer",
                Confidence = 0.82m
            };
            resource.Tags.Add("policies");
            resource.Categories.Add("operations");
            resource.Metadata["language"] = "en";
            resource.Relationships.Add(new AiKnowledgeRelationship
            {
                RelationshipType = "related-to",
                TargetResourceType = "knowledge",
                TargetResourceId = "knowledge-7"
            });
            resource.Validate();

            Assert.Equal(AiKnowledgeLifecycleStatus.Draft, resource.Status);
            Assert.False(resource.IsAuthoritative);
            var clone = resource.Clone();
            Assert.Equal("model", clone.Provenance.Source);
            Assert.Equal("execution-42", clone.Provenance.SourceExecutionId);
            Assert.Equal("en", clone.Metadata["language"]);
            Assert.Single(clone.Relationships);
        }

        [Fact]
        public void KnowledgeResource_PublishedStatusIsExplicitAuthority()
        {
            var resource = CreateResource(AiKnowledgeResourceKind.Knowledge, "knowledge-42", "owner-42");
            resource.Status = AiKnowledgeLifecycleStatus.Published;
            resource.Validate();
            Assert.True(resource.IsAuthoritative);
        }

        [Fact]
        public void KnowledgeResource_RequiresOwnerOutsideGlobalScope()
        {
            var resource = CreateResource(AiKnowledgeResourceKind.Knowledge, "knowledge-private-42", null);
            resource.Scope = AgentResourceScope.User;
            Assert.Throws<ArgumentException>(() => resource.Validate());
        }

        [Fact]
        public void RetrievalRequest_IsBoundedAndClonedWithIdentity()
        {
            var identity = new AgentIdentityContext(
                deploymentId: "deployment-42",
                tenantId: "tenant-42",
                userId: "user-42");
            var request = new AiKnowledgeRetrievalRequest
            {
                Query = "retention policy",
                ResourceType = "wiki",
                Identity = identity,
                MaxResults = 2,
                MaxCharacters = 1000,
                MaxChunks = 3,
                MaxChunkCharacters = 200
            };
            request.ResourceIds.Add("wiki-42");
            request.Filters["tag"] = "policy";
            request.Validate();

            var clone = request.Clone();
            Assert.Equal("user-42", clone.Identity.UserId);
            Assert.Equal("wiki-42", clone.ResourceIds.Single());
            Assert.Equal("policy", clone.Filters["tag"]);
        }

        [Fact]
        public async Task GovernedRetriever_OnlyRetrievesAuthorizedPublishedResources()
        {
            var identity = new AgentIdentityContext(
                deploymentId: "deployment-42",
                tenantId: "tenant-42",
                userId: "user-42");
            var ownerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, identity);

            var policy = new AiPolicySet { Version = "knowledge-policy-42" };
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "allow-knowledge-42",
                Name = "Allow knowledge 42",
                Scope = AiPolicyScopeKind.User,
                ScopeId = "user-42",
                Outcome = AiPolicyOutcome.Allow,
                Operations = { "knowledge.retrieve" },
                ResourceTypes = { "knowledge" },
                ResourceIds = { "knowledge-42" }
            });

            var capabilities = new AiResourceCapabilityPolicy();
            capabilities.Set("knowledge", AiResourceCapabilityState.Enabled);
            var governance = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(policy),
                AiResourceCapabilitySnapshot.Resolve(capabilities));

            var allowed = CreateResource(AiKnowledgeResourceKind.Knowledge, "knowledge-42", ownerId);
            allowed.Status = AiKnowledgeLifecycleStatus.Published;
            var forbidden = CreateResource(AiKnowledgeResourceKind.Knowledge, "knowledge-99", ownerId);
            forbidden.Status = AiKnowledgeLifecycleStatus.Published;
            var draft = CreateResource(AiKnowledgeResourceKind.Knowledge, "knowledge-draft-42", ownerId);

            var inner = new RecordingKnowledgeRetriever(new[] { allowed, forbidden, draft });
            var governed = new AiGovernedKnowledgeRetriever(inner, governance, new[] { allowed, forbidden, draft });

            var result = await governed.RetrieveAsync(new AiKnowledgeRetrievalRequest
            {
                Query = "retention",
                Identity = identity,
                MaxResults = 4,
                MaxCharacters = 4000,
                MaxChunks = 4,
                MaxChunkCharacters = 500
            }, CancellationToken.None);

            Assert.Single(result.Candidates);
            Assert.Equal("knowledge-42", result.Candidates[0].Resource.Id);
            Assert.Contains("knowledge-42", inner.LastRequest.ResourceIds);
            Assert.DoesNotContain("knowledge-99", inner.LastRequest.ResourceIds);
            Assert.DoesNotContain("knowledge-draft-42", inner.LastRequest.ResourceIds);
        }

        [Fact]
        public async Task GovernedRetriever_RejectsCrossOwnerAndDraftEvenWhenRequested()
        {
            var identity = new AgentIdentityContext(
                deploymentId: "deployment-42",
                tenantId: "tenant-42",
                userId: "user-42");
            var ownerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, identity);
            var otherIdentity = new AgentIdentityContext(
                deploymentId: "deployment-42",
                tenantId: "tenant-42",
                userId: "user-other");
            var otherOwnerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, otherIdentity);

            var policy = new AiPolicySet { Version = "knowledge-policy-43" };
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "allow-knowledge-43",
                Name = "Allow knowledge 43",
                Scope = AiPolicyScopeKind.User,
                ScopeId = "user-42",
                Outcome = AiPolicyOutcome.Allow,
                Operations = { "knowledge.retrieve" },
                ResourceTypes = { "knowledge" },
                ResourceIds = { "knowledge-43" }
            });

            var capabilities = new AiResourceCapabilityPolicy();
            capabilities.Set("knowledge", AiResourceCapabilityState.Enabled);
            var governance = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(policy),
                AiResourceCapabilitySnapshot.Resolve(capabilities));

            var otherOwner = CreateResource(AiKnowledgeResourceKind.Knowledge, "knowledge-43", otherOwnerId);
            otherOwner.Status = AiKnowledgeLifecycleStatus.Published;
            var draft = CreateResource(AiKnowledgeResourceKind.Knowledge, "knowledge-draft-43", ownerId);
            draft.Status = AiKnowledgeLifecycleStatus.Draft;

            var inner = new RecordingKnowledgeRetriever(new[] { otherOwner, draft });
            var governed = new AiGovernedKnowledgeRetriever(inner, governance, new[] { otherOwner, draft });

            var result = await governed.RetrieveAsync(new AiKnowledgeRetrievalRequest
            {
                Query = "governance",
                Identity = identity,
                ResourceIds = { "knowledge-43" },
                IncludeDraft = true
            }, CancellationToken.None);

            Assert.Empty(result.Candidates);
            Assert.Empty(inner.LastRequest.ResourceIds);
        }

        [Fact]
        public async Task GovernedRetriever_HonorsCancellationBeforeRetrieval()
        {
            var identity = new AgentIdentityContext(userId: "user-42");
            var ownerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, identity);
            var resource = CreateResource(AiKnowledgeResourceKind.Knowledge, "knowledge-cancel-42", ownerId);
            resource.Status = AiKnowledgeLifecycleStatus.Published;

            var policy = new AiPolicySet { Version = "knowledge-cancel-policy" };
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "allow-cancel",
                Name = "Allow cancel",
                Scope = AiPolicyScopeKind.User,
                ScopeId = "user-42",
                Outcome = AiPolicyOutcome.Allow,
                Operations = { "knowledge.retrieve" },
                ResourceTypes = { "knowledge" },
                ResourceIds = { resource.Id }
            });
            var capabilities = new AiResourceCapabilityPolicy();
            capabilities.Set("knowledge", AiResourceCapabilityState.Enabled);
            var governance = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(policy),
                AiResourceCapabilitySnapshot.Resolve(capabilities));
            var inner = new RecordingKnowledgeRetriever(new[] { resource });
            var governed = new AiGovernedKnowledgeRetriever(inner, governance, new[] { resource });
            var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() => governed.RetrieveAsync(
                new AiKnowledgeRetrievalRequest { Query = "cancel", Identity = identity },
                cancellation.Token));
            Assert.Null(inner.LastRequest);
        }

        private static AiKnowledgeResource CreateResource(AiKnowledgeResourceKind kind, string id, string ownerId)
        {
            var now = DateTime.UtcNow;
            return new AiKnowledgeResource
            {
                Id = id,
                Kind = kind,
                Scope = ownerId == null ? AgentResourceScope.Global : AgentResourceScope.User,
                OwnerId = ownerId,
                Title = id,
                Summary = "Test resource",
                Content = "Bounded knowledge content for " + id,
                Source = kind == AiKnowledgeResourceKind.Wiki ? "wiki" : "knowledge",
                Provenance = new AiKnowledgeProvenance
                {
                    Kind = AiKnowledgeProvenanceKind.HostProvided,
                    Source = "tests",
                    SourceId = id
                },
                CreatedUtc = now,
                UpdatedUtc = now
            };
        }

        private sealed class RecordingKnowledgeRetriever : IAiKnowledgeRetriever
        {
            private readonly AiKnowledgeResource[] _resources;

            public RecordingKnowledgeRetriever(IEnumerable<AiKnowledgeResource> resources)
            {
                _resources = resources.Select(r => r.Clone()).ToArray();
            }

            public AiKnowledgeRetrievalRequest LastRequest { get; private set; }

            public Task<AiKnowledgeRetrievalResult> RetrieveAsync(
                AiKnowledgeRetrievalRequest request,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                LastRequest = request.Clone();
                var result = new AiKnowledgeRetrievalResult
                {
                    ConsideredCount = _resources.Length
                };
                foreach (var resource in _resources)
                {
                    if (request.ResourceIds.Count > 0 && !request.ResourceIds.Contains(resource.Id))
                        continue;
                    result.Candidates.Add(new AiKnowledgeRetrievalCandidate
                    {
                        Resource = resource.Clone(),
                        Chunk = new AiKnowledgeChunk
                        {
                            Id = resource.Id + ":0",
                            ResourceId = resource.Id,
                            ChunkIndex = 0,
                            Content = resource.Content
                        },
                        Score = 1.0
                    });
                }
                result.ReturnedCharacterCount = result.Candidates.Sum(c => c.Chunk.Content.Length);
                result.Validate();
                return Task.FromResult(result);
            }
        }
    }
}
