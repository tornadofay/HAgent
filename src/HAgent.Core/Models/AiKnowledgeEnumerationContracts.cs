using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class AiKnowledgeEnumerationQuery
    {
        public AiKnowledgeResourceKind? Kind { get; set; }
        public AgentResourceScope? Scope { get; set; }
        public string OwnerId { get; set; }
        public AiKnowledgeLifecycleStatus? Status { get; set; }
        public long? Version { get; set; }
        public DateTime? UpdatedAfterUtc { get; set; }
        public DateTime? UpdatedBeforeUtc { get; set; }
        public string SearchText { get; set; }
        public bool AuthoritativeOnly { get; set; }
        public int MaxResults { get; set; }

        public AiKnowledgeEnumerationQuery()
        {
            MaxResults = 200;
        }

        public AiKnowledgeEnumerationQuery Clone()
        {
            return new AiKnowledgeEnumerationQuery
            {
                Kind = Kind,
                Scope = Scope,
                OwnerId = OwnerId,
                Status = Status,
                Version = Version,
                UpdatedAfterUtc = UpdatedAfterUtc,
                UpdatedBeforeUtc = UpdatedBeforeUtc,
                SearchText = SearchText,
                AuthoritativeOnly = AuthoritativeOnly,
                MaxResults = MaxResults
            };
        }

        public void Validate()
        {
            if (Kind.HasValue && !Enum.IsDefined(typeof(AiKnowledgeResourceKind), Kind.Value))
                throw new ArgumentException("Invalid knowledge enumeration kind.");
            if (Scope.HasValue && !Enum.IsDefined(typeof(AgentResourceScope), Scope.Value))
                throw new ArgumentException("Invalid knowledge enumeration scope.");
            if (OwnerId != null && OwnerId.Length > 2048)
                throw new ArgumentException("Knowledge enumeration OwnerId is too long.");
            if (Status.HasValue && !Enum.IsDefined(typeof(AiKnowledgeLifecycleStatus), Status.Value))
                throw new ArgumentException("Invalid knowledge enumeration status.");
            if (Version.HasValue && Version.Value <= 0)
                throw new ArgumentException("Knowledge enumeration Version must be positive.");
            if (UpdatedAfterUtc.HasValue && UpdatedBeforeUtc.HasValue && UpdatedAfterUtc.Value > UpdatedBeforeUtc.Value)
                throw new ArgumentException("Knowledge enumeration update range is invalid.");
            if (SearchText != null && SearchText.Length > 512)
                throw new ArgumentException("Knowledge enumeration SearchText is too long.");
            if (MaxResults < 1 || MaxResults > 1000)
                throw new ArgumentException("Knowledge enumeration MaxResults must be between 1 and 1000.");
        }
    }
}
