using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace HAgent.Models
{
    public enum AiEvaluationMetricKind
    {
        SuccessRate,
        QualityScore,
        LatencyMilliseconds,
        Cost,
        FallbackFrequency,
        ToolSuccessRate,
        PlanCompletionRate,
        Custom
    }

    public sealed class AiEvaluationMetric
    {
        public AiEvaluationMetric()
        {
            Name = string.Empty;
            Unit = string.Empty;
        }

        public AiEvaluationMetricKind Kind { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Unit { get; set; }
        public bool? HigherIsBetter { get; set; }

        public AiEvaluationMetric Clone()
        {
            return new AiEvaluationMetric
            {
                Kind = Kind,
                Name = Name,
                Value = Value,
                Unit = Unit,
                HigherIsBetter = HigherIsBetter
            };
        }

        public void Validate()
        {
            Require(Name, nameof(Name), 128);
            if (Unit != null && Unit.Length > 64)
                throw new ArgumentOutOfRangeException(nameof(Unit));

            if ((Kind == AiEvaluationMetricKind.SuccessRate ||
                 Kind == AiEvaluationMetricKind.QualityScore ||
                 Kind == AiEvaluationMetricKind.FallbackFrequency ||
                 Kind == AiEvaluationMetricKind.ToolSuccessRate ||
                 Kind == AiEvaluationMetricKind.PlanCompletionRate) &&
                (Value < 0m || Value > 1m))
                throw new ArgumentOutOfRangeException(nameof(Value));

            if (Value < 0m)
                throw new ArgumentOutOfRangeException(nameof(Value));

            if (Kind != AiEvaluationMetricKind.Custom && HigherIsBetter.HasValue)
                throw new ArgumentException("Standard evaluation metrics derive comparison direction from their metric kind.", nameof(HigherIsBetter));
            if (Kind == AiEvaluationMetricKind.Custom && !HigherIsBetter.HasValue)
                throw new ArgumentException("Custom evaluation metrics require an explicit comparison direction.", nameof(HigherIsBetter));
        }

        internal static string GetCanonicalName(AiEvaluationMetricKind kind)
        {
            switch (kind)
            {
                case AiEvaluationMetricKind.SuccessRate: return "success-rate";
                case AiEvaluationMetricKind.QualityScore: return "quality-score";
                case AiEvaluationMetricKind.LatencyMilliseconds: return "latency-ms";
                case AiEvaluationMetricKind.Cost: return "cost";
                case AiEvaluationMetricKind.FallbackFrequency: return "fallback-frequency";
                case AiEvaluationMetricKind.ToolSuccessRate: return "tool-success-rate";
                case AiEvaluationMetricKind.PlanCompletionRate: return "plan-completion-rate";
                default: return string.Empty;
            }
        }

        internal static bool GetHigherIsBetter(AiEvaluationMetricKind kind, bool? customDirection)
        {
            if (kind == AiEvaluationMetricKind.Custom)
                return customDirection.GetValueOrDefault();

            return kind == AiEvaluationMetricKind.SuccessRate ||
                   kind == AiEvaluationMetricKind.QualityScore ||
                   kind == AiEvaluationMetricKind.ToolSuccessRate ||
                   kind == AiEvaluationMetricKind.PlanCompletionRate;
        }

        private static void Require(string value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiEvaluationSample
    {
        public AiEvaluationSample()
        {
            CaseId = string.Empty;
            VariantId = string.Empty;
            Metrics = new List<AiEvaluationMetric>();
        }

        public string CaseId { get; set; }
        public string VariantId { get; set; }
        public AiEvaluation Evaluation { get; set; }
        public IList<AiEvaluationMetric> Metrics { get; private set; }

        public AiEvaluationSample Clone()
        {
            var clone = new AiEvaluationSample
            {
                CaseId = CaseId,
                VariantId = VariantId,
                Evaluation = Evaluation == null ? null : Evaluation.Clone()
            };
            foreach (var metric in Metrics ?? new List<AiEvaluationMetric>())
                if (metric != null) clone.Metrics.Add(metric.Clone());
            return clone;
        }

        public void Validate()
        {
            Require(CaseId, nameof(CaseId), 256);
            Require(VariantId, nameof(VariantId), 256);
            if (Evaluation == null)
                throw new ArgumentNullException(nameof(Evaluation));
            Evaluation.Validate();
            if (Metrics == null)
                throw new ArgumentNullException(nameof(Metrics));
            if (Metrics.Count > 32)
                throw new ArgumentOutOfRangeException(nameof(Metrics));
            foreach (var metric in Metrics)
            {
                if (metric == null)
                    throw new ArgumentException("Evaluation metrics cannot contain null entries.", nameof(Metrics));
                metric.Validate();
            }
        }

        private static void Require(string value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiEvaluationAggregationRequest
    {
        public AiEvaluationAggregationRequest()
        {
            Samples = new List<AiEvaluationSample>();
        }

        public IList<AiEvaluationSample> Samples { get; private set; }

        public AiEvaluationAggregationRequest Clone()
        {
            var clone = new AiEvaluationAggregationRequest();
            foreach (var sample in Samples ?? new List<AiEvaluationSample>())
                if (sample != null) clone.Samples.Add(sample.Clone());
            return clone;
        }

        public void Validate()
        {
            if (Samples == null)
                throw new ArgumentNullException(nameof(Samples));
            if (Samples.Count == 0)
                throw new ArgumentException("At least one evaluation sample is required.", nameof(Samples));
            if (Samples.Count > 256)
                throw new ArgumentOutOfRangeException(nameof(Samples));
            foreach (var sample in Samples)
            {
                if (sample == null)
                    throw new ArgumentException("Evaluation samples cannot contain null entries.", nameof(Samples));
                sample.Validate();
            }
        }
    }

    public sealed class AiEvaluationAggregateMetric
    {
        public AiEvaluationAggregateMetric()
        {
            Name = string.Empty;
            Unit = string.Empty;
        }

        public AiEvaluationMetricKind Kind { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public decimal Average { get; set; }
        public decimal Minimum { get; set; }
        public decimal Maximum { get; set; }
        public string Unit { get; set; }
        public bool HigherIsBetter { get; set; }

        public AiEvaluationAggregateMetric Clone()
        {
            return new AiEvaluationAggregateMetric
            {
                Kind = Kind,
                Name = Name,
                Count = Count,
                Average = Average,
                Minimum = Minimum,
                Maximum = Maximum,
                Unit = Unit,
                HigherIsBetter = HigherIsBetter
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name) || Name.Length > 128)
                throw new ArgumentException("A bounded metric name is required.", nameof(Name));
            if (Count <= 0 || Count > 256)
                throw new ArgumentOutOfRangeException(nameof(Count));
            if (Average < 0m || Minimum < 0m || Maximum < 0m || Minimum > Maximum)
                throw new ArgumentOutOfRangeException(nameof(Average));
            if (Unit != null && Unit.Length > 64)
                throw new ArgumentOutOfRangeException(nameof(Unit));
            if ((Kind == AiEvaluationMetricKind.SuccessRate ||
                 Kind == AiEvaluationMetricKind.QualityScore ||
                 Kind == AiEvaluationMetricKind.FallbackFrequency ||
                 Kind == AiEvaluationMetricKind.ToolSuccessRate ||
                 Kind == AiEvaluationMetricKind.PlanCompletionRate) &&
                (Average > 1m || Maximum > 1m))
                throw new ArgumentOutOfRangeException(nameof(Average));
        }
    }

    public sealed class AiEvaluationAggregate
    {
        public AiEvaluationAggregate()
        {
            VariantId = string.Empty;
            Metrics = new List<AiEvaluationAggregateMetric>();
        }

        public string VariantId { get; set; }
        public int SampleCount { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public int InconclusiveCount { get; set; }
        public int NeedsReviewCount { get; set; }
        public int NotEvaluatedCount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal? QualityScoreAverage { get; set; }
        public IList<AiEvaluationAggregateMetric> Metrics { get; private set; }

        public AiEvaluationAggregate Clone()
        {
            var clone = new AiEvaluationAggregate
            {
                VariantId = VariantId,
                SampleCount = SampleCount,
                PassedCount = PassedCount,
                FailedCount = FailedCount,
                InconclusiveCount = InconclusiveCount,
                NeedsReviewCount = NeedsReviewCount,
                NotEvaluatedCount = NotEvaluatedCount,
                SuccessRate = SuccessRate,
                QualityScoreAverage = QualityScoreAverage
            };
            foreach (var metric in Metrics ?? new List<AiEvaluationAggregateMetric>())
                if (metric != null) clone.Metrics.Add(metric.Clone());
            return clone;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(VariantId) || VariantId.Length > 256)
                throw new ArgumentException("A bounded variant identifier is required.", nameof(VariantId));
            if (SampleCount <= 0 || SampleCount > 256)
                throw new ArgumentOutOfRangeException(nameof(SampleCount));
            if (PassedCount < 0 || FailedCount < 0 || InconclusiveCount < 0 || NeedsReviewCount < 0 || NotEvaluatedCount < 0)
                throw new ArgumentOutOfRangeException(nameof(PassedCount));
            if (PassedCount + FailedCount + InconclusiveCount + NeedsReviewCount + NotEvaluatedCount != SampleCount)
                throw new ArgumentException("Evaluation outcome counts must equal SampleCount.", nameof(SampleCount));
            if (SuccessRate < 0m || SuccessRate > 1m)
                throw new ArgumentOutOfRangeException(nameof(SuccessRate));
            if (QualityScoreAverage.HasValue && (QualityScoreAverage.Value < 0m || QualityScoreAverage.Value > 1m))
                throw new ArgumentOutOfRangeException(nameof(QualityScoreAverage));
            if (Metrics == null)
                throw new ArgumentNullException(nameof(Metrics));
            if (Metrics.Count > 64)
                throw new ArgumentOutOfRangeException(nameof(Metrics));
            foreach (var metric in Metrics)
            {
                if (metric == null)
                    throw new ArgumentException("Aggregate metrics cannot contain null entries.", nameof(Metrics));
                metric.Validate();
            }
        }
    }

    public sealed class AiEvaluationMetricComparison
    {
        public AiEvaluationMetricComparison()
        {
            Name = string.Empty;
            Unit = string.Empty;
            PreferredVariantId = string.Empty;
        }

        public AiEvaluationMetricKind Kind { get; set; }
        public string Name { get; set; }
        public decimal? LeftAverage { get; set; }
        public decimal? RightAverage { get; set; }
        public decimal? Delta { get; set; }
        public string Unit { get; set; }
        public bool HigherIsBetter { get; set; }
        public string PreferredVariantId { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name) || Name.Length > 128)
                throw new ArgumentException("A bounded metric name is required.", nameof(Name));
            if (Unit != null && Unit.Length > 64)
                throw new ArgumentOutOfRangeException(nameof(Unit));
            if (LeftAverage.HasValue && LeftAverage.Value < 0m)
                throw new ArgumentOutOfRangeException(nameof(LeftAverage));
            if (RightAverage.HasValue && RightAverage.Value < 0m)
                throw new ArgumentOutOfRangeException(nameof(RightAverage));
            if (Delta.HasValue && Delta.Value != LeftAverage.GetValueOrDefault() - RightAverage.GetValueOrDefault())
                throw new ArgumentException("Metric delta must equal left minus right.", nameof(Delta));
            if (Kind == AiEvaluationMetricKind.SuccessRate || Kind == AiEvaluationMetricKind.QualityScore ||
                Kind == AiEvaluationMetricKind.FallbackFrequency || Kind == AiEvaluationMetricKind.ToolSuccessRate ||
                Kind == AiEvaluationMetricKind.PlanCompletionRate)
            {
                if ((LeftAverage.HasValue && LeftAverage.Value > 1m) || (RightAverage.HasValue && RightAverage.Value > 1m))
                    throw new ArgumentOutOfRangeException(nameof(LeftAverage));
            }
        }
    }

    public sealed class AiEvaluationComparison
    {
        public AiEvaluationComparison()
        {
            LeftVariantId = string.Empty;
            RightVariantId = string.Empty;
            Metrics = new List<AiEvaluationMetricComparison>();
        }

        public string LeftVariantId { get; set; }
        public string RightVariantId { get; set; }
        public IList<AiEvaluationMetricComparison> Metrics { get; private set; }

        public AiEvaluationComparison Clone()
        {
            var clone = new AiEvaluationComparison
            {
                LeftVariantId = LeftVariantId,
                RightVariantId = RightVariantId
            };
            foreach (var metric in Metrics ?? new List<AiEvaluationMetricComparison>())
                if (metric != null)
                    clone.Metrics.Add(new AiEvaluationMetricComparison
                    {
                        Kind = metric.Kind,
                        Name = metric.Name,
                        LeftAverage = metric.LeftAverage,
                        RightAverage = metric.RightAverage,
                        Delta = metric.Delta,
                        Unit = metric.Unit,
                        HigherIsBetter = metric.HigherIsBetter,
                        PreferredVariantId = metric.PreferredVariantId
                    });
            return clone;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(LeftVariantId) || LeftVariantId.Length > 256)
                throw new ArgumentException("A bounded left variant identifier is required.", nameof(LeftVariantId));
            if (string.IsNullOrWhiteSpace(RightVariantId) || RightVariantId.Length > 256)
                throw new ArgumentException("A bounded right variant identifier is required.", nameof(RightVariantId));
            if (string.Equals(LeftVariantId, RightVariantId, StringComparison.Ordinal))
                throw new ArgumentException("Compared variants must be distinct.", nameof(RightVariantId));
            if (Metrics == null)
                throw new ArgumentNullException(nameof(Metrics));
            if (Metrics.Count > 64)
                throw new ArgumentOutOfRangeException(nameof(Metrics));
            foreach (var metric in Metrics)
            {
                if (metric == null)
                    throw new ArgumentException("Metric comparisons cannot contain null entries.", nameof(Metrics));
                metric.Validate();
            }
        }
    }

    public static class AiEvaluationAggregator
    {
        public static IList<AiEvaluationAggregate> Aggregate(AiEvaluationAggregationRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            cancellationToken.ThrowIfCancellationRequested();
            request.Validate();

            var snapshot = request.Clone();
            var groups = snapshot.Samples
                .GroupBy(s => s.VariantId, StringComparer.Ordinal)
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .ToList();

            var result = new List<AiEvaluationAggregate>();
            foreach (var group in groups)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var aggregate = new AiEvaluationAggregate
                {
                    VariantId = group.Key,
                    SampleCount = group.Count(),
                    PassedCount = group.Count(s => s.Evaluation.Outcome == AiEvaluationOutcome.Passed),
                    FailedCount = group.Count(s => s.Evaluation.Outcome == AiEvaluationOutcome.Failed),
                    InconclusiveCount = group.Count(s => s.Evaluation.Outcome == AiEvaluationOutcome.Inconclusive),
                    NeedsReviewCount = group.Count(s => s.Evaluation.Outcome == AiEvaluationOutcome.NeedsReview),
                    NotEvaluatedCount = group.Count(s => s.Evaluation.Outcome == AiEvaluationOutcome.NotEvaluated)
                };

                aggregate.SuccessRate = aggregate.SampleCount == 0
                    ? 0m
                    : decimal.Round((decimal)aggregate.PassedCount / aggregate.SampleCount, 6, MidpointRounding.AwayFromZero);

                var scores = group.Select(s => s.Evaluation.Score).Where(s => s.HasValue).Select(s => s.Value).ToList();
                if (scores.Count > 0)
                    aggregate.QualityScoreAverage = decimal.Round(scores.Average(), 6, MidpointRounding.AwayFromZero);

                AddDerivedMetric(aggregate, AiEvaluationMetricKind.SuccessRate, aggregate.SuccessRate, "ratio");
                if (aggregate.QualityScoreAverage.HasValue)
                    AddDerivedMetric(aggregate, AiEvaluationMetricKind.QualityScore, aggregate.QualityScoreAverage.Value, "ratio");

                var explicitMetrics = group.SelectMany(s => s.Metrics)
                    .GroupBy(m => new MetricGroupKey(m.Kind, m.Name, m.Unit, m.HigherIsBetter), MetricGroupKeyComparer.Instance)
                    .OrderBy(g => g.Key.Kind)
                    .ThenBy(g => g.Key.Name, StringComparer.Ordinal);

                foreach (var metricGroup in explicitMetrics)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var values = metricGroup.Select(m => m.Value).ToList();
                    var metric = new AiEvaluationAggregateMetric
                    {
                        Kind = metricGroup.Key.Kind,
                        Name = metricGroup.Key.Name,
                        Count = values.Count,
                        Average = decimal.Round(values.Average(), 6, MidpointRounding.AwayFromZero),
                        Minimum = values.Min(),
                        Maximum = values.Max(),
                        Unit = metricGroup.Key.Unit,
                        HigherIsBetter = AiEvaluationMetric.GetHigherIsBetter(metricGroup.Key.Kind, metricGroup.Key.HigherIsBetter)
                    };
                    metric.Validate();
                    aggregate.Metrics.Add(metric);
                }

                aggregate.Validate();
                result.Add(aggregate);
            }

            return result;
        }

        public static AiEvaluationComparison Compare(AiEvaluationAggregate left, AiEvaluationAggregate right, CancellationToken cancellationToken)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));
            cancellationToken.ThrowIfCancellationRequested();
            left.Validate();
            right.Validate();

            var comparison = new AiEvaluationComparison
            {
                LeftVariantId = left.VariantId,
                RightVariantId = right.VariantId
            };

            var metrics = new Dictionary<string, MetricPair>(StringComparer.Ordinal);
            AddComparisonMetric(metrics, new AiEvaluationAggregateMetric
            {
                Kind = AiEvaluationMetricKind.SuccessRate,
                Name = "success-rate",
                Count = left.SampleCount,
                Average = left.SuccessRate,
                Minimum = left.SuccessRate,
                Maximum = left.SuccessRate,
                Unit = "ratio",
                HigherIsBetter = true
            });
            if (left.QualityScoreAverage.HasValue || right.QualityScoreAverage.HasValue)
                AddComparisonMetric(metrics, new AiEvaluationAggregateMetric
                {
                    Kind = AiEvaluationMetricKind.QualityScore,
                    Name = "quality-score",
                    Count = left.SampleCount,
                    Average = left.QualityScoreAverage.GetValueOrDefault(),
                    Minimum = left.QualityScoreAverage.GetValueOrDefault(),
                    Maximum = left.QualityScoreAverage.GetValueOrDefault(),
                    Unit = "ratio",
                    HigherIsBetter = true
                });

            foreach (var metric in left.Metrics)
                if (!string.Equals(metric.Name, "success-rate", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(metric.Name, "quality-score", StringComparison.OrdinalIgnoreCase))
                    AddComparisonMetric(metrics, metric);
            foreach (var metric in right.Metrics)
                if (!string.Equals(metric.Name, "success-rate", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(metric.Name, "quality-score", StringComparison.OrdinalIgnoreCase))
                    AddComparisonMetric(metrics, metric);

            foreach (var pair in metrics.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var key = pair.Value.Left ?? pair.Value.Right;
                var leftAverage = pair.Value.Left == null ? (decimal?)null : pair.Value.Left.Average;
                var rightAverage = pair.Value.Right == null ? (decimal?)null : pair.Value.Right.Average;
                decimal? delta = leftAverage.HasValue && rightAverage.HasValue ? leftAverage.Value - rightAverage.Value : (decimal?)null;
                var preferred = string.Empty;
                if (leftAverage.HasValue && rightAverage.HasValue && delta.HasValue && delta.Value != 0m)
                {
                    bool leftBetter = key.HigherIsBetter ? delta.Value > 0m : delta.Value < 0m;
                    preferred = leftBetter ? left.VariantId : right.VariantId;
                }

                var comparisonMetric = new AiEvaluationMetricComparison
                {
                    Kind = key.Kind,
                    Name = key.Name,
                    LeftAverage = leftAverage,
                    RightAverage = rightAverage,
                    Delta = delta,
                    Unit = key.Unit,
                    HigherIsBetter = key.HigherIsBetter,
                    PreferredVariantId = preferred
                };
                comparisonMetric.Validate();
                comparison.Metrics.Add(comparisonMetric);
            }

            comparison.Validate();
            return comparison;
        }

        private static void AddDerivedMetric(AiEvaluationAggregate aggregate, AiEvaluationMetricKind kind, decimal value, string unit)
        {
            aggregate.Metrics.Add(new AiEvaluationAggregateMetric
            {
                Kind = kind,
                Name = AiEvaluationMetric.GetCanonicalName(kind),
                Count = aggregate.SampleCount,
                Average = value,
                Minimum = value,
                Maximum = value,
                Unit = unit,
                HigherIsBetter = AiEvaluationMetric.GetHigherIsBetter(kind, null)
            });
        }

        private static void AddComparisonMetric(IDictionary<string, MetricPair> metrics, AiEvaluationAggregateMetric metric)
        {
            string key = metric.Kind.ToString() + "|" + metric.Name.ToUpperInvariant() + "|" + (metric.Unit ?? string.Empty).ToUpperInvariant();
            MetricPair pair;
            if (!metrics.TryGetValue(key, out pair))
            {
                pair = new MetricPair(metric);
                metrics[key] = pair;
            }
            else
            {
                pair.Set(metric);
            }
        }

        private sealed class MetricPair
        {
            public MetricPair(AiEvaluationAggregateMetric first)
            {
                HigherIsBetter = first.HigherIsBetter;
                Name = first.Name;
                Kind = first.Kind;
                Unit = first.Unit;
                Left = null;
                Right = null;
            }

            public AiEvaluationAggregateMetric Left { get; private set; }
            public AiEvaluationAggregateMetric Right { get; private set; }
            public AiEvaluationMetricKind Kind { get; private set; }
            public string Name { get; private set; }
            public string Unit { get; private set; }
            public bool HigherIsBetter { get; private set; }

            public void Set(AiEvaluationAggregateMetric metric)
            {
                if (Left == null)
                    Left = metric;
                else if (Right == null)
                    Right = metric;
            }
        }

        private struct MetricGroupKey
        {
            public MetricGroupKey(AiEvaluationMetricKind kind, string name, string unit, bool? higherIsBetter)
            {
                Kind = kind;
                Name = name;
                Unit = unit ?? string.Empty;
                HigherIsBetter = higherIsBetter;
            }

            public AiEvaluationMetricKind Kind;
            public string Name;
            public string Unit;
            public bool? HigherIsBetter;
        }

        private sealed class MetricGroupKeyComparer : IEqualityComparer<MetricGroupKey>
        {
            public static readonly MetricGroupKeyComparer Instance = new MetricGroupKeyComparer();

            public bool Equals(MetricGroupKey x, MetricGroupKey y)
            {
                return x.Kind == y.Kind &&
                       string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(x.Unit, y.Unit, StringComparison.OrdinalIgnoreCase) &&
                       x.HigherIsBetter == y.HigherIsBetter;
            }

            public int GetHashCode(MetricGroupKey obj)
            {
                unchecked
                {
                    int hash = (int)obj.Kind;
                    hash = (hash * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name ?? string.Empty);
                    hash = (hash * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Unit ?? string.Empty);
                    hash = (hash * 397) ^ obj.HigherIsBetter.GetHashCode();
                    return hash;
                }
            }
        }
    }
}
