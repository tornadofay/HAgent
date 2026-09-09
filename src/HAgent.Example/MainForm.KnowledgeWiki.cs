using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddKnowledgeWikiTab()
        {
            AddApiTab(
                "Knowledge/Wiki",
                "Run knowledge test",
                "Creates provider-neutral Knowledge and managed Wiki resources with scope, version, provenance, relationships, lifecycle state, and bounded retrieval, then applies the same resource-governance boundary before retrieval.",
                "Only the published resource authorized for the current user should reach the retriever. Draft model-generated content and another user's resource must remain outside the retrieval set.",
                "Search the retention policy knowledge.",
                TestKnowledgeWikiAsync,
                "Knowledge boundary",
                "Knowledge is reusable retrievable information; Wiki is one managed knowledge source. Retrieval and physical indexing stay behind the provider-neutral retriever contract.");
        }

        private Task TestKnowledgeWikiAsync(string unused)
        {
            var identity = new AgentIdentityContext(
                deploymentId: "deployment-42",
                tenantId: "tenant-42",
                userId: "user-42");
            var ownerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, identity);
            var otherIdentity = new AgentIdentityContext(
                deploymentId: "deployment-42",
                tenantId: "tenant-42",
                userId: "user-99");
            var otherOwnerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, otherIdentity);

            var publishedWiki = new AiKnowledgeResource
            {
                Id = "wiki-retention-42",
                Kind = AiKnowledgeResourceKind.Wiki,
                Scope = AgentResourceScope.User,
                OwnerId = ownerId,
                Title = "Retention Policy",
                Summary = "Authoritative retention guidance",
                Content = "Retention policy requires records to be preserved for seven years.",
                Status = AiKnowledgeLifecycleStatus.Published,
                Version = 3,
                Source = "managed-wiki",
                Provenance = new AiKnowledgeProvenance
                {
                    Kind = AiKnowledgeProvenanceKind.HostProvided,
                    Source = "administrator",
                    SourceId = "policy-42"
                }
            };
            publishedWiki.Tags.Add("retention");
            publishedWiki.Categories.Add("compliance");
            publishedWiki.Metadata["language"] = "en";
            publishedWiki.Relationships.Add(new AiKnowledgeRelationship
            {
                RelationshipType = "references",
                TargetResourceType = "knowledge",
                TargetResourceId = "knowledge-retention-42"
            });
            publishedWiki.Validate();

            var draftModelKnowledge = new AiKnowledgeResource
            {
                Id = "knowledge-model-draft-42",
                Kind = AiKnowledgeResourceKind.Knowledge,
                Scope = AgentResourceScope.User,
                OwnerId = ownerId,
                Title = "Model draft",
                Summary = "Candidate information only",
                Content = "The model suggests a shorter retention period.",
                Provenance = new AiKnowledgeProvenance
                {
                    Kind = AiKnowledgeProvenanceKind.ModelGenerated,
                    Source = "model",
                    SourceExecutionId = "execution-42",
                    SourceRuntimeInstanceId = "runtime-42",
                    Confidence = 0.91m
                }
            };
            draftModelKnowledge.Validate();

            var otherUserKnowledge = new AiKnowledgeResource
            {
                Id = "knowledge-other-42",
                Kind = AiKnowledgeResourceKind.Knowledge,
                Scope = AgentResourceScope.User,
                OwnerId = otherOwnerId,
                Title = "Other user policy",
                Summary = "Private resource for another user",
                Content = "This information must not cross the owner boundary.",
                Status = AiKnowledgeLifecycleStatus.Published,
                Version = 2,
                Source = "host",
                Provenance = new AiKnowledgeProvenance
                {
                    Kind = AiKnowledgeProvenanceKind.UserProvided,
                    Source = "user-99",
                    SourceId = "knowledge-other-42"
                }
            };
            otherUserKnowledge.Validate();

            var policy = new AiPolicySet { Version = "knowledge-example-policy-42" };
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "allow-wiki-retention",
                Name = "Allow retention wiki retrieval",
                Scope = AiPolicyScopeKind.User,
                ScopeId = "user-42",
                Outcome = AiPolicyOutcome.Allow,
                Operations = { "knowledge.retrieve" },
                ResourceTypes = { "wiki" },
                ResourceIds = { publishedWiki.Id }
            });

            var capabilityPolicy = new AiResourceCapabilityPolicy();
            capabilityPolicy.Set("wiki", AiResourceCapabilityState.Enabled);
            capabilityPolicy.Set("knowledge", AiResourceCapabilityState.Enabled);
            var governance = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(policy),
                AiResourceCapabilitySnapshot.Resolve(capabilityPolicy));

            var inner = new ExampleKnowledgeRetriever(new[]
            {
                publishedWiki,
                draftModelKnowledge,
                otherUserKnowledge
            });
            var governed = new AiGovernedKnowledgeRetriever(
                inner,
                governance,
                new[] { publishedWiki, draftModelKnowledge, otherUserKnowledge });

            var task = governed.RetrieveAsync(
                new AiKnowledgeRetrievalRequest
                {
                    Query = "retention policy",
                    Identity = identity,
                    MaxResults = 4,
                    MaxCharacters = 2000,
                    MaxChunks = 4,
                    MaxChunkCharacters = 500
                },
                CancellationToken.None);
            var result = task.GetAwaiter().GetResult();

            if (result.Candidates.Count != 1 || result.Candidates[0].Resource.Id != publishedWiki.Id)
                throw new InvalidOperationException("Knowledge governance returned an unexpected resource set.");
            if (!result.Candidates[0].Resource.IsAuthoritative || result.Candidates[0].Resource.Version != 3)
                throw new InvalidOperationException("Published knowledge authority/version metadata was not preserved.");
            if (result.Candidates[0].Resource.Provenance.SourceId != "policy-42")
                throw new InvalidOperationException("Knowledge provenance was not preserved through retrieval.");
            if (inner.LastRequest == null || inner.LastRequest.ResourceIds.Count != 1 ||
                inner.LastRequest.ResourceIds[0] != publishedWiki.Id)
                throw new InvalidOperationException("Governed retrieval did not constrain the underlying retriever.");

            Write(
                "KNOWLEDGE / WIKI",
                "Knowledge/Wiki succeeded." + Environment.NewLine +
                "Managed Wiki resource: verified." + Environment.NewLine +
                "Scope/owner boundary: verified." + Environment.NewLine +
                "Published version preserved: 3." + Environment.NewLine +
                "Provenance preserved: policy-42." + Environment.NewLine +
                "Model-generated draft remains non-authoritative." + Environment.NewLine +
                "Other-user resource excluded before retrieval." + Environment.NewLine +
                "Retrieval remained provider/index independent." + Environment.NewLine +
                "Retrieval bounds: MaxResults 4 / MaxCharacters 2000 / MaxChunks 4 / MaxChunkCharacters 500.");

            return Task.CompletedTask;
        }

        private sealed class ExampleKnowledgeRetriever : IAiKnowledgeRetriever
        {
            private readonly AiKnowledgeResource[] _resources;

            public ExampleKnowledgeRetriever(IEnumerable<AiKnowledgeResource> resources)
            {
                _resources = resources.Select(resource => resource.Clone()).ToArray();
            }

            public AiKnowledgeRetrievalRequest LastRequest { get; private set; }

            public Task<AiKnowledgeRetrievalResult> RetrieveAsync(
                AiKnowledgeRetrievalRequest request,
                CancellationToken cancellationToken)
            {
                if (request == null)
                    throw new ArgumentNullException("request");
                request.Validate();
                cancellationToken.ThrowIfCancellationRequested();
                LastRequest = request.Clone();

                var result = new AiKnowledgeRetrievalResult();
                foreach (var resource in _resources)
                {
                    if (request.ResourceIds.Count > 0 && !request.ResourceIds.Contains(resource.Id))
                        continue;

                    var chunkContent = resource.Content;
                    var score = 0.1;
                    var terms = request.Query.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var term in terms)
                    {
                        if (resource.Title.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                            score += 1.0;
                        if (resource.Content.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                            score += 0.5;
                    }

                    if (score <= 0.1)
                        continue;

                    var chunk = new AiKnowledgeChunk
                    {
                        Id = resource.Id + ":0",
                        ResourceId = resource.Id,
                        ChunkIndex = 0,
                        Content = chunkContent
                    };
                    if (chunk.Content.Length > request.MaxChunkCharacters)
                        chunk.Content = chunk.Content.Substring(0, request.MaxChunkCharacters);

                    result.Candidates.Add(new AiKnowledgeRetrievalCandidate
                    {
                        Resource = resource.Clone(),
                        Chunk = chunk,
                        Score = score
                    });
                }

                result.Candidates = result.Candidates
                    .OrderByDescending(candidate => candidate.Score)
                    .ThenBy(candidate => candidate.Resource.Id, StringComparer.OrdinalIgnoreCase)
                    .Take(request.MaxResults)
                    .ToList();
                result.ConsideredCount = _resources.Length;
                result.ReturnedCharacterCount = result.Candidates.Sum(candidate => candidate.Chunk.Content.Length);
                result.Validate();
                return Task.FromResult(result);
            }
        }
    }
}
