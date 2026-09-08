namespace HAgent.Models
{
    public sealed class AiInterventionQuery
    {
        public AiInterventionRequestStatus? Status { get; set; }
        public AiInterventionTargetKind? TargetKind { get; set; }
        public string TargetId { get; set; }
        public int MaxResults { get; set; }

        public AiInterventionQuery()
        {
            TargetId = string.Empty;
            MaxResults = 100;
        }

        internal void Validate()
        {
            if (MaxResults <= 0) MaxResults = 100;
            if (MaxResults > 1000) MaxResults = 1000;
        }
    }
}
