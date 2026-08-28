using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class AIUsage
    {
        public AIUsage()
        {
            ProviderUsage = new Dictionary<string, object>();
        }

        public long? PromptTokens { get; set; }
        public long? CompletionTokens { get; set; }
        public long? ReasoningTokens { get; set; }
        public long? CachedPromptTokens { get; set; }
        public long? TotalTokens { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string CostCurrency { get; set; }
        public IDictionary<string, object> ProviderUsage { get; private set; }

        public bool HasTokenUsage
        {
            get
            {
                return PromptTokens.HasValue || CompletionTokens.HasValue ||
                       ReasoningTokens.HasValue || CachedPromptTokens.HasValue || TotalTokens.HasValue;
            }
        }
    }
}
