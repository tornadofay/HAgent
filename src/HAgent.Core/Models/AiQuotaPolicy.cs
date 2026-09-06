using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiQuotaDimension
    {
        RequestCount,
        InputTokens,
        OutputTokens,
        TotalTokens,
        Concurrency,
        AudioDurationMilliseconds,
        ImageCount,
        Bytes,
        SpendMinorUnits
    }

    public sealed class AiQuotaLimit
    {
        public AiQuotaLimit()
        {
            Dimension = AiQuotaDimension.RequestCount;
            Maximum = 0L;
            Window = TimeSpan.FromMinutes(1);
            Scope = string.Empty;
        }

        public AiQuotaDimension Dimension { get; set; }
        public long Maximum { get; set; }
        public TimeSpan Window { get; set; }
        public string Scope { get; set; }

        public void Validate()
        {
            if (Maximum < 0L) throw new ArgumentOutOfRangeException(nameof(Maximum));
            if (Window <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(Window));
            if (Scope != null && Scope.Length > 256) throw new ArgumentOutOfRangeException(nameof(Scope));
        }
    }

    public sealed class AiQuotaPolicy
    {
        public AiQuotaPolicy()
        {
            Limits = new List<AiQuotaLimit>();
        }

        public IList<AiQuotaLimit> Limits { get; private set; }

        public void Validate()
        {
            if (Limits.Count > 128) throw new ArgumentOutOfRangeException(nameof(Limits));
            foreach (var limit in Limits)
            {
                if (limit == null) throw new ArgumentException("Quota limits cannot contain null entries.", nameof(Limits));
                limit.Validate();
            }
        }
    }
}
