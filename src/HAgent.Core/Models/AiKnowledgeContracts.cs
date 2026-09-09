using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Models
{
    public enum AiKnowledgeResourceKind
    {
        Knowledge = 0,
        Wiki = 1
    }

    public enum AiKnowledgeLifecycleStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }

    public enum AiKnowledgeProvenanceKind
    {
        HostProvided = 0,
        UserProvided = 1,
        Imported = 2,
        ModelGenerated = 3,
        SystemGenerated = 4
    }

    public sealed class AiKnowledgeProvenance
    {
        public AiKnowledgeProvenanceKind Kind { get; set; }
        public string Source { get; set; }
        public string SourceId { get; set; }
        public string SourceUri { get; set; }
        public string CreatedBy { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string Evidence { get; set; }
        public decimal? Confidence { get; set; }

        public AiKnowledgeProvenance Clone()
        {
            return new AiKnowledgeProvenance
            {
                Kind = Kind,
                Source = Source,
                SourceId = SourceId,
                SourceUri = SourceUri,
                CreatedBy = CreatedBy,
                SourceExecutionId = SourceExecutionId,
                SourceRuntimeInstanceId = SourceRuntimeInstanceId,
                Evidence = Evidence,
                Confidence = Confidence
            };
        }

        public void Validate()
        {
            if (!Enum.IsDefined(typeof(AiKnowledgeProvenanceKind), Kind))
                throw new ArgumentException("Invalid knowledge provenance kind.");
            ValidateText(Source, 256, false, "Source");
            ValidateText(SourceId, 512, false, "SourceId");
            ValidateText(SourceUri, 2048, false, "SourceUri");
            ValidateText(CreatedBy, 512, false, "CreatedBy");
            ValidateText(SourceExecutionId, 256, false, "SourceExecutionId");
            ValidateText(SourceRuntimeInstanceId, 256, false, "SourceRuntimeInstanceId");
            ValidateText(Evidence, 4096, false, "Evidence");
            if (Confidence.HasValue && (Confidence.Value < 0m || Confidence.Value > 1m))
                throw new ArgumentException("Knowledge provenance confidence must be between 0 and 1.");
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength)
                throw new ArgumentException(name + " exceeds its maximum length.");
        }
    }

    public sealed class AiKnowledgeRelationship
    {
        public string RelationshipType { get; set; }
        public string TargetResourceType { get; set; }
        public string TargetResourceId { get; set; }
        public IDictionary<string, string> Metadata { get; private set; }

        public AiKnowledgeRelationship()
        {
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public AiKnowledgeRelationship Clone()
        {
            var clone = new AiKnowledgeRelationship
            {
                RelationshipType = RelationshipType,
                TargetResourceType = TargetResourceType,
                TargetResourceId = TargetResourceId
            };
            foreach (var pair in Metadata)
                clone.Metadata[pair.Key] = pair.Value;
            return clone;
        }

        public void Validate()
        {
            ValidateText(RelationshipType, 128, true, "RelationshipType");
            ValidateText(TargetResourceType, 256, true, "TargetResourceType");
            ValidateText(TargetResourceId, 512, true, "TargetResourceId");
            ValidateMetadata(Metadata, 16, 128, 1024);
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength)
                throw new ArgumentException(name + " exceeds its maximum length.");
        }

        private static void ValidateMetadata(IDictionary<string, string> metadata, int maxEntries, int maxKeyLength, int maxValueLength)
        {
            if (metadata == null || metadata.Count > maxEntries)
                throw new ArgumentException("Knowledge relationship metadata exceeds its bounds.");
            foreach (var pair in metadata)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > maxKeyLength ||
                    pair.Value == null || pair.Value.Length > maxValueLength)
                    throw new ArgumentException("Knowledge relationship metadata contains an invalid entry.");
            }
        }
    }

    public sealed class AiKnowledgeResource
    {
        public string Id { get; set; }
        public AiKnowledgeResourceKind Kind { get; set; }
        public AgentResourceScope Scope { get; set; }
        public string OwnerId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public AiKnowledgeLifecycleStatus Status { get; set; }
        public long Version { get; set; }
        public string Source { get; set; }
        public AiKnowledgeProvenance Provenance { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public IDictionary<string, string> Metadata { get; private set; }
        public IList<string> Tags { get; private set; }
        public IList<string> Categories { get; private set; }
        public IList<AiKnowledgeRelationship> Relationships { get; private set; }

        public AiKnowledgeResource()
        {
            Status = AiKnowledgeLifecycleStatus.Draft;
            Version = 1;
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = CreatedUtc;
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Tags = new List<string>();
            Categories = new List<string>();
            Relationships = new List<AiKnowledgeRelationship>();
        }

        public bool IsAuthoritative
        {
            get { return Status == AiKnowledgeLifecycleStatus.Published; }
        }

        public AiKnowledgeResource Clone()
        {
            var clone = new AiKnowledgeResource
            {
                Id = Id,
                Kind = Kind,
                Scope = Scope,
                OwnerId = OwnerId,
                Title = Title,
                Summary = Summary,
                Content = Content,
                Status = Status,
                Version = Version,
                Source = Source,
                Provenance = Provenance == null ? null : Provenance.Clone(),
                CreatedUtc = CreatedUtc,
                UpdatedUtc = UpdatedUtc
            };
            foreach (var pair in Metadata)
                clone.Metadata[pair.Key] = pair.Value;
            foreach (var tag in Tags)
                clone.Tags.Add(tag);
            foreach (var category in Categories)
                clone.Categories.Add(category);
            foreach (var relationship in Relationships)
                clone.Relationships.Add(relationship == null ? null : relationship.Clone());
            return clone;
        }

        public void Validate()
        {
            ValidateText(Id, 128, true, "Id");
            if (!Enum.IsDefined(typeof(AiKnowledgeResourceKind), Kind))
                throw new ArgumentException("Invalid knowledge resource kind.");
            if (!Enum.IsDefined(typeof(AgentResourceScope), Scope))
                throw new ArgumentException("Invalid knowledge resource scope.");
            ValidateText(OwnerId, 2048, Scope != AgentResourceScope.Global, "OwnerId");
            ValidateText(Title, 500, true, "Title");
            ValidateText(Summary, 4096, false, "Summary");
            ValidateText(Content, 2000000, true, "Content");
            if (!Enum.IsDefined(typeof(AiKnowledgeLifecycleStatus), Status))
                throw new ArgumentException("Invalid knowledge lifecycle status.");
            if (Version <= 0)
                throw new ArgumentException("Knowledge version must be positive.");
            ValidateText(Source, 256, false, "Source");
            if (Provenance == null)
                throw new ArgumentException("Knowledge provenance is required.");
            Provenance.Validate();
            if (CreatedUtc == default(DateTime) || UpdatedUtc == default(DateTime))
                throw new ArgumentException("Knowledge timestamps are required.");
            if (UpdatedUtc < CreatedUtc)
                throw new ArgumentException("Knowledge UpdatedUtc cannot be earlier than CreatedUtc.");
            ValidateMetadata(Metadata, 32, 128, 4096);
            ValidateStringList(Tags, 64, 128, "Tags");
            ValidateStringList(Categories, 32, 128, "Categories");
            if (Relationships == null || Relationships.Count > 64)
                throw new ArgumentException("Knowledge relationships exceed their bounds.");
            foreach (var relationship in Relationships)
            {
                if (relationship == null)
                    throw new ArgumentException("Knowledge relationships cannot contain null entries.");
                relationship.Validate();
            }
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength)
                throw new ArgumentException(name + " exceeds its maximum length.");
        }

        private static void ValidateMetadata(IDictionary<string, string> metadata, int maxEntries, int maxKeyLength, int maxValueLength)
        {
            if (metadata == null || metadata.Count > maxEntries)
                throw new ArgumentException("Knowledge metadata exceeds its bounds.");
            foreach (var pair in metadata)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > maxKeyLength ||
                    pair.Value == null || pair.Value.Length > maxValueLength)
                    throw new ArgumentException("Knowledge metadata contains an invalid entry.");
            }
        }

        private static void ValidateStringList(IList<string> values, int maxEntries, int maxItemLength, string name)
        {
            if (values == null || values.Count > maxEntries)
                throw new ArgumentException(name + " exceed their bounds.");
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length > maxItemLength)
                    throw new ArgumentException(name + " contain an invalid entry.");
            }
        }
    }

    public sealed class AiKnowledgeChunk
    {
        public string Id { get; set; }
        public string ResourceId { get; set; }
        public int ChunkIndex { get; set; }
        public string Content { get; set; }
        public IDictionary<string, string> Metadata { get; private set; }

        public AiKnowledgeChunk()
        {
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public AiKnowledgeChunk Clone()
        {
            var clone = new AiKnowledgeChunk
            {
                Id = Id,
                ResourceId = ResourceId,
                ChunkIndex = ChunkIndex,
                Content = Content
            };
            foreach (var pair in Metadata)
                clone.Metadata[pair.Key] = pair.Value;
            return clone;
        }

        public void Validate()
        {
            ValidateText(Id, 128, true, "Id");
            ValidateText(ResourceId, 128, true, "ResourceId");
            if (ChunkIndex < 0)
                throw new ArgumentException("Knowledge chunk index cannot be negative.");
            ValidateText(Content, 200000, true, "Content");
            if (Metadata == null || Metadata.Count > 16)
                throw new ArgumentException("Knowledge chunk metadata exceeds its bounds.");
            foreach (var pair in Metadata)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > 128 ||
                    pair.Value == null || pair.Value.Length > 2048)
                    throw new ArgumentException("Knowledge chunk metadata contains an invalid entry.");
            }
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength)
                throw new ArgumentException(name + " exceeds its maximum length.");
        }
    }

    public sealed class AiKnowledgeRetrievalRequest
    {
        public string Query { get; set; }
        public string ResourceType { get; set; }
        public AgentIdentityContext Identity { get; set; }
        public IList<string> ResourceIds { get; private set; }
        public int MaxResults { get; set; }
        public int MaxCharacters { get; set; }
        public int MaxChunks { get; set; }
        public int MaxChunkCharacters { get; set; }
        public bool IncludeDraft { get; set; }
        public IDictionary<string, string> Filters { get; private set; }

        public AiKnowledgeRetrievalRequest()
        {
            ResourceIds = new List<string>();
            Filters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            MaxResults = 8;
            MaxCharacters = 16000;
            MaxChunks = 16;
            MaxChunkCharacters = 4000;
        }

        public AiKnowledgeRetrievalRequest Clone()
        {
            var clone = new AiKnowledgeRetrievalRequest
            {
                Query = Query,
                ResourceType = ResourceType,
                Identity = Identity == null ? null : Identity.Clone(),
                MaxResults = MaxResults,
                MaxCharacters = MaxCharacters,
                MaxChunks = MaxChunks,
                MaxChunkCharacters = MaxChunkCharacters,
                IncludeDraft = IncludeDraft
            };
            foreach (var resourceId in ResourceIds)
                clone.ResourceIds.Add(resourceId);
            foreach (var pair in Filters)
                clone.Filters[pair.Key] = pair.Value;
            return clone;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Query) || Query.Length > 4096)
                throw new ArgumentException("Knowledge retrieval query is required and must be bounded.");
            if (ResourceType != null && ResourceType.Length > 256)
                throw new ArgumentException("Knowledge retrieval resource type is too long.");
            if (ResourceIds == null || ResourceIds.Count > 128)
                throw new ArgumentException("Knowledge retrieval resource ID filter exceeds its bounds.");
            foreach (var resourceId in ResourceIds)
            {
                if (string.IsNullOrWhiteSpace(resourceId) || resourceId.Length > 128)
                    throw new ArgumentException("Knowledge retrieval resource ID filter contains an invalid value.");
            }
            if (MaxResults < 1 || MaxResults > 100)
                throw new ArgumentException("Knowledge retrieval MaxResults must be between 1 and 100.");
            if (MaxCharacters < 1 || MaxCharacters > 200000)
                throw new ArgumentException("Knowledge retrieval MaxCharacters is outside its allowed bounds.");
            if (MaxChunks < 1 || MaxChunks > 256)
                throw new ArgumentException("Knowledge retrieval MaxChunks is outside its allowed bounds.");
            if (MaxChunkCharacters < 1 || MaxChunkCharacters > 50000)
                throw new ArgumentException("Knowledge retrieval MaxChunkCharacters is outside its allowed bounds.");
            if (Filters == null || Filters.Count > 32)
                throw new ArgumentException("Knowledge retrieval filters exceed their bounds.");
            foreach (var pair in Filters)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > 128 ||
                    pair.Value == null || pair.Value.Length > 2048)
                    throw new ArgumentException("Knowledge retrieval filters contain an invalid entry.");
            }
        }
    }

    public sealed class AiKnowledgeRetrievalCandidate
    {
        public AiKnowledgeResource Resource { get; set; }
        public AiKnowledgeChunk Chunk { get; set; }
        public double Score { get; set; }

        public AiKnowledgeRetrievalCandidate Clone()
        {
            return new AiKnowledgeRetrievalCandidate
            {
                Resource = Resource == null ? null : Resource.Clone(),
                Chunk = Chunk == null ? null : Chunk.Clone(),
                Score = Score
            };
        }
    }

    public sealed class AiKnowledgeRetrievalResult
    {
        public IList<AiKnowledgeRetrievalCandidate> Candidates { get; private set; }
        public bool Truncated { get; set; }
        public int ConsideredCount { get; set; }
        public int ReturnedCharacterCount { get; set; }

        public AiKnowledgeRetrievalResult()
        {
            Candidates = new List<AiKnowledgeRetrievalCandidate>();
        }

        public AiKnowledgeRetrievalResult Clone()
        {
            var clone = new AiKnowledgeRetrievalResult
            {
                Truncated = Truncated,
                ConsideredCount = ConsideredCount,
                ReturnedCharacterCount = ReturnedCharacterCount
            };
            foreach (var candidate in Candidates)
                clone.Candidates.Add(candidate == null ? null : candidate.Clone());
            return clone;
        }

        public void Validate()
        {
            if (ConsideredCount < 0)
                throw new ArgumentException("Knowledge retrieval considered count cannot be negative.");
            if (ReturnedCharacterCount < 0)
                throw new ArgumentException("Knowledge retrieval returned character count cannot be negative.");
            if (Candidates == null || Candidates.Count > 100)
                throw new ArgumentException("Knowledge retrieval candidate count exceeds its bound.");
            foreach (var candidate in Candidates)
            {
                if (candidate == null || candidate.Resource == null || candidate.Chunk == null)
                    throw new ArgumentException("Knowledge retrieval candidates must contain resource and chunk evidence.");
                if (double.IsNaN(candidate.Score) || double.IsInfinity(candidate.Score))
                    throw new ArgumentException("Knowledge retrieval candidate score must be finite.");
            }
        }
    }

    public interface IAiKnowledgeRetriever
    {
        Task<AiKnowledgeRetrievalResult> RetrieveAsync(
            AiKnowledgeRetrievalRequest request,
            CancellationToken cancellationToken);
    }

    public sealed class AiGovernedKnowledgeRetriever : IAiKnowledgeRetriever
    {
        private readonly IAiKnowledgeRetriever _inner;
        private readonly AiResourceGovernanceEvaluator _governance;
        private readonly AiKnowledgeResource[] _resources;

        public AiGovernedKnowledgeRetriever(
            IAiKnowledgeRetriever inner,
            AiResourceGovernanceEvaluator governance,
            IEnumerable<AiKnowledgeResource> resources)
        {
            if (inner == null)
                throw new ArgumentNullException("inner");
            if (governance == null)
                throw new ArgumentNullException("governance");
            if (resources == null)
                throw new ArgumentNullException("resources");
            _inner = inner;
            _governance = governance;
            _resources = resources.Select(resource =>
            {
                if (resource == null)
                    throw new ArgumentException("Knowledge resources cannot contain null entries.");
                resource.Validate();
                return resource.Clone();
            }).ToArray();
        }

        public async Task<AiKnowledgeRetrievalResult> RetrieveAsync(
            AiKnowledgeRetrievalRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            request.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var governedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var resource in _resources)
            {
                if (!MatchesRequest(resource, request))
                    continue;
                if (resource.Status == AiKnowledgeLifecycleStatus.Draft && !request.IncludeDraft)
                    continue;

                var decision = _governance.Evaluate(new AiResourceGovernanceRequest
                {
                    Operation = "knowledge.retrieve",
                    ResourceType = resource.Kind == AiKnowledgeResourceKind.Wiki ? "wiki" : "knowledge",
                    ResourceId = resource.Id,
                    Scope = resource.Scope,
                    ResourceOwnerId = resource.OwnerId,
                    Identity = request.Identity
                });

                if (decision.Allowed)
                    governedIds.Add(resource.Id);
            }

            var retrievalRequest = request.Clone();
            retrievalRequest.ResourceIds.Clear();
            foreach (var id in governedIds)
                retrievalRequest.ResourceIds.Add(id);

            if (governedIds.Count == 0)
                return new AiKnowledgeRetrievalResult();

            var result = await _inner.RetrieveAsync(retrievalRequest, cancellationToken).ConfigureAwait(false);
            if (result == null)
                throw new InvalidOperationException("Knowledge retriever returned no result.");
            result.Validate();
            return result.Clone();
        }

        private static bool MatchesRequest(AiKnowledgeResource resource, AiKnowledgeRetrievalRequest request)
        {
            if (request.ResourceIds.Count > 0 && !request.ResourceIds.Contains(resource.Id, StringComparer.OrdinalIgnoreCase))
                return false;
            if (!string.IsNullOrWhiteSpace(request.ResourceType))
            {
                var expected = resource.Kind == AiKnowledgeResourceKind.Wiki ? "wiki" : "knowledge";
                if (!string.Equals(expected, request.ResourceType, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }
    }
}
