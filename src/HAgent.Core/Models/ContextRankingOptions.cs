using System;

namespace HAgent.Models
{
    /// <summary>
    /// Provider-neutral weighting and duplicate policy for deterministic context ranking.
    /// </summary>
    public sealed class ContextRankingOptions
    {
        public ContextRankingOptions()
        {
            RelevanceWeight = 0.40d;
            ImportanceWeight = 0.25d;
            TrustWeight = 0.20d;
            FreshnessWeight = 0.10d;
            CostWeight = 0.05d;
            FreshnessReference = DateTimeOffset.UtcNow;
            MaxFreshnessAge = TimeSpan.FromDays(30);
            Deduplicate = true;
        }

        public double RelevanceWeight { get; set; }
        public double ImportanceWeight { get; set; }
        public double TrustWeight { get; set; }
        public double FreshnessWeight { get; set; }
        public double CostWeight { get; set; }
        public DateTimeOffset FreshnessReference { get; set; }
        public TimeSpan MaxFreshnessAge { get; set; }
        public bool Deduplicate { get; set; }

        public ContextRankingOptions Clone()
        {
            return new ContextRankingOptions
            {
                RelevanceWeight = RelevanceWeight,
                ImportanceWeight = ImportanceWeight,
                TrustWeight = TrustWeight,
                FreshnessWeight = FreshnessWeight,
                CostWeight = CostWeight,
                FreshnessReference = FreshnessReference,
                MaxFreshnessAge = MaxFreshnessAge,
                Deduplicate = Deduplicate
            };
        }

        public void Validate()
        {
            ValidateWeight(RelevanceWeight, nameof(RelevanceWeight));
            ValidateWeight(ImportanceWeight, nameof(ImportanceWeight));
            ValidateWeight(TrustWeight, nameof(TrustWeight));
            ValidateWeight(FreshnessWeight, nameof(FreshnessWeight));
            ValidateWeight(CostWeight, nameof(CostWeight));
            if (FreshnessReference == default(DateTimeOffset))
                throw new ArgumentException("Ranking freshness reference is required.", nameof(FreshnessReference));
            if (MaxFreshnessAge <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(MaxFreshnessAge));

            var total = RelevanceWeight + ImportanceWeight + TrustWeight + FreshnessWeight + CostWeight;
            if (total <= 0d || double.IsNaN(total) || double.IsInfinity(total))
                throw new ArgumentException("At least one ranking weight must be greater than zero.");
        }

        private static void ValidateWeight(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
