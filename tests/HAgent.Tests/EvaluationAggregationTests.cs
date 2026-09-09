using System;
using System.Linq;
using System.Threading;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class EvaluationAggregationTests
    {
        [Fact]
        public void MetricValidation_RequiresExplicitDirectionForCustomMetric()
        {
            var metric = new AiEvaluationMetric
            {
                Kind = AiEvaluationMetricKind.Custom,
                Name = "answer-completeness",
                Value = 0.75m
            };

            Assert.Throws<ArgumentException>(() => metric.Validate());

            metric.HigherIsBetter = true;
            metric.Validate();
        }

        [Fact]
        public void Aggregate_GroupsByVariantAndComputesOutcomeAndScoreMetrics()
        {
            var request = new AiEvaluationAggregationRequest();
            request.Samples.Add(CreateSample("case-1", "variant-a", AiEvaluationOutcome.Passed, 0.9m,
                Metric(AiEvaluationMetricKind.LatencyMilliseconds, "latency-ms", 120m, "ms")));
            request.Samples.Add(CreateSample("case-2", "variant-a", AiEvaluationOutcome.Failed, 0.3m,
                Metric(AiEvaluationMetricKind.LatencyMilliseconds, "latency-ms", 80m, "ms")));
            request.Samples.Add(CreateSample("case-3", "variant-a", AiEvaluationOutcome.NeedsReview, null,
                Metric(AiEvaluationMetricKind.LatencyMilliseconds, "latency-ms", 100m, "ms")));
            request.Samples.Add(CreateSample("case-1", "variant-b", AiEvaluationOutcome.Passed, 0.8m,
                Metric(AiEvaluationMetricKind.LatencyMilliseconds, "latency-ms", 60m, "ms")));
            request.Samples.Add(CreateSample("case-2", "variant-b", AiEvaluationOutcome.Passed, 0.7m,
                Metric(AiEvaluationMetricKind.LatencyMilliseconds, "latency-ms", 70m, "ms")));

            var result = AiEvaluationAggregator.Aggregate(request, CancellationToken.None);

            Assert.Equal(2, result.Count);
            var a = result.Single(x => x.VariantId == "variant-a");
            var b = result.Single(x => x.VariantId == "variant-b");

            Assert.Equal(3, a.SampleCount);
            Assert.Equal(1, a.PassedCount);
            Assert.Equal(1, a.FailedCount);
            Assert.Equal(1, a.NeedsReviewCount);
            Assert.Equal(0.333333m, a.SuccessRate);
            Assert.Equal(0.6m, a.QualityScoreAverage.Value);

            Assert.Equal(2, b.SampleCount);
            Assert.Equal(2, b.PassedCount);
            Assert.Equal(1m, b.SuccessRate);
            Assert.Equal(0.75m, b.QualityScoreAverage.Value);

            var latency = a.Metrics.Single(x => x.Kind == AiEvaluationMetricKind.LatencyMilliseconds);
            Assert.Equal(100m, latency.Average);
            Assert.Equal(80m, latency.Minimum);
            Assert.Equal(120m, latency.Maximum);
            Assert.False(latency.HigherIsBetter);

            var derivedSuccess = a.Metrics.Single(x => x.Kind == AiEvaluationMetricKind.SuccessRate);
            Assert.Equal("success-rate", derivedSuccess.Name);
            Assert.True(derivedSuccess.HigherIsBetter);
        }

        [Fact]
        public void Aggregate_PreservesDistinctExplicitMetricDefinitions()
        {
            var request = new AiEvaluationAggregationRequest();
            request.Samples.Add(CreateSample("case-1", "variant-a", AiEvaluationOutcome.Passed, 1m,
                Metric(AiEvaluationMetricKind.Custom, "custom-score", 0.4m, "ratio", true)));
            request.Samples.Add(CreateSample("case-2", "variant-a", AiEvaluationOutcome.Passed, 1m,
                Metric(AiEvaluationMetricKind.Custom, "custom-score", 0.8m, "ratio", true)));
            request.Samples.Add(CreateSample("case-3", "variant-a", AiEvaluationOutcome.Passed, 1m,
                Metric(AiEvaluationMetricKind.Custom, "other-score", 0.9m, "ratio", false)));

            var aggregate = AiEvaluationAggregator.Aggregate(request, CancellationToken.None).Single();

            Assert.Equal(2, aggregate.Metrics.Count(x => x.Kind == AiEvaluationMetricKind.Custom));
            Assert.Equal(0.6m, aggregate.Metrics.Single(x => x.Name == "custom-score").Average);
            Assert.Equal(0.9m, aggregate.Metrics.Single(x => x.Name == "other-score").Average);
        }

        [Fact]
        public void Aggregate_DetectsCancellationBeforeWorkAndBetweenGroups()
        {
            var request = new AiEvaluationAggregationRequest();
            request.Samples.Add(CreateSample("case-1", "variant-a", AiEvaluationOutcome.Passed, 1m));
            request.Samples.Add(CreateSample("case-2", "variant-b", AiEvaluationOutcome.Passed, 1m));

            using (var cancelled = new CancellationTokenSource())
            {
                cancelled.Cancel();
                Assert.Throws<OperationCanceledException>(() => AiEvaluationAggregator.Aggregate(request, cancelled.Token));
            }
        }

        [Fact]
        public void Compare_ReportsMetricDeltasAndNonAuthoritativePreferenceByDirection()
        {
            var request = new AiEvaluationAggregationRequest();
            request.Samples.Add(CreateSample("case-1", "slow", AiEvaluationOutcome.Passed, 0.7m,
                Metric(AiEvaluationMetricKind.LatencyMilliseconds, "latency-ms", 200m, "ms"),
                Metric(AiEvaluationMetricKind.Cost, "cost", 0.08m, "usd"),
                Metric(AiEvaluationMetricKind.FallbackFrequency, "fallback-frequency", 0.2m, "ratio")));
            request.Samples.Add(CreateSample("case-2", "slow", AiEvaluationOutcome.Failed, 0.4m,
                Metric(AiEvaluationMetricKind.LatencyMilliseconds, "latency-ms", 180m, "ms"),
                Metric(AiEvaluationMetricKind.Cost, "cost", 0.06m, "usd"),
                Metric(AiEvaluationMetricKind.FallbackFrequency, "fallback-frequency", 0.1m, "ratio")));
            request.Samples.Add(CreateSample("case-1", "fast", AiEvaluationOutcome.Passed, 0.9m,
                Metric(AiEvaluationMetricKind.LatencyMilliseconds, "latency-ms", 100m, "ms"),
                Metric(AiEvaluationMetricKind.Cost, "cost", 0.03m, "usd"),
                Metric(AiEvaluationMetricKind.FallbackFrequency, "fallback-frequency", 0.05m, "ratio")));
            request.Samples.Add(CreateSample("case-2", "fast", AiEvaluationOutcome.Passed, 0.8m,
                Metric(AiEvaluationMetricKind.LatencyMilliseconds, "latency-ms", 120m, "ms"),
                Metric(AiEvaluationMetricKind.Cost, "cost", 0.04m, "usd"),
                Metric(AiEvaluationMetricKind.FallbackFrequency, "fallback-frequency", 0.02m, "ratio")));

            var aggregates = AiEvaluationAggregator.Aggregate(request, CancellationToken.None);
            var slow = aggregates.Single(x => x.VariantId == "slow");
            var fast = aggregates.Single(x => x.VariantId == "fast");
            var comparison = AiEvaluationAggregator.Compare(fast, slow, CancellationToken.None);

            var latency = comparison.Metrics.Single(x => x.Name == "latency-ms");
            Assert.Equal(110m, latency.LeftAverage);
            Assert.Equal(190m, latency.RightAverage);
            Assert.Equal(-80m, latency.Delta);
            Assert.False(latency.HigherIsBetter);
            Assert.Equal("fast", latency.PreferredVariantId);

            var cost = comparison.Metrics.Single(x => x.Name == "cost");
            Assert.Equal("fast", cost.PreferredVariantId);

            var fallback = comparison.Metrics.Single(x => x.Name == "fallback-frequency");
            Assert.Equal("fast", fallback.PreferredVariantId);

            var success = comparison.Metrics.Single(x => x.Name == "success-rate");
            Assert.Equal(1m, success.LeftAverage);
            Assert.Equal(0.5m, success.RightAverage);
            Assert.Equal("fast", success.PreferredVariantId);

            var quality = comparison.Metrics.Single(x => x.Name == "quality-score");
            Assert.Equal(0.85m, quality.LeftAverage);
            Assert.Equal(0.55m, quality.RightAverage);
            Assert.Equal("fast", quality.PreferredVariantId);
        }

        [Fact]
        public void Compare_AllowsOneSidedMetricsWithoutFabricatingPreference()
        {
            var leftRequest = new AiEvaluationAggregationRequest();
            leftRequest.Samples.Add(CreateSample("case-1", "left", AiEvaluationOutcome.Passed, 1m,
                Metric(AiEvaluationMetricKind.Custom, "left-only", 0.5m, "ratio", true)));
            var rightRequest = new AiEvaluationAggregationRequest();
            rightRequest.Samples.Add(CreateSample("case-1", "right", AiEvaluationOutcome.Passed, 1m));

            var left = AiEvaluationAggregator.Aggregate(leftRequest, CancellationToken.None).Single();
            var right = AiEvaluationAggregator.Aggregate(rightRequest, CancellationToken.None).Single();
            var comparison = AiEvaluationAggregator.Compare(left, right, CancellationToken.None);

            var metric = comparison.Metrics.Single(x => x.Name == "left-only");
            Assert.Equal(0.5m, metric.LeftAverage);
            Assert.Null(metric.RightAverage);
            Assert.Null(metric.Delta);
            Assert.Equal(string.Empty, metric.PreferredVariantId);
        }

        [Fact]
        public void Aggregate_UsesDetachedSampleValues()
        {
            var evaluation = new AiEvaluation
            {
                TargetId = "response-1",
                EvaluatorId = "deterministic",
                EvaluatorVersion = "1",
                Outcome = AiEvaluationOutcome.Passed,
                Score = 0.9m
            };
            var sample = new AiEvaluationSample
            {
                CaseId = "case-1",
                VariantId = "variant-a",
                Evaluation = evaluation
            };
            sample.Metrics.Add(Metric(AiEvaluationMetricKind.LatencyMilliseconds, "latency-ms", 50m, "ms"));
            var request = new AiEvaluationAggregationRequest();
            request.Samples.Add(sample);

            var result = AiEvaluationAggregator.Aggregate(request, CancellationToken.None).Single();
            evaluation.Score = 0m;
            sample.Metrics[0].Value = 999m;

            Assert.Equal(0.9m, result.QualityScoreAverage.Value);
            Assert.Equal(50m, result.Metrics.Single(x => x.Name == "latency-ms").Average);
        }

        private static AiEvaluationSample CreateSample(string caseId, string variantId, AiEvaluationOutcome outcome, decimal? score, params AiEvaluationMetric[] metrics)
        {
            var evaluation = new AiEvaluation
            {
                TargetKind = AiEvaluationTargetKind.Execution,
                TargetId = variantId + ":" + caseId,
                EvaluatorId = "test-evaluator",
                EvaluatorVersion = "1",
                Outcome = outcome,
                Score = score
            };
            var sample = new AiEvaluationSample
            {
                CaseId = caseId,
                VariantId = variantId,
                Evaluation = evaluation
            };
            foreach (var metric in metrics)
                sample.Metrics.Add(metric);
            return sample;
        }

        private static AiEvaluationMetric Metric(AiEvaluationMetricKind kind, string name, decimal value, string unit, bool? higherIsBetter = null)
        {
            return new AiEvaluationMetric
            {
                Kind = kind,
                Name = name,
                Value = value,
                Unit = unit,
                HigherIsBetter = higherIsBetter
            };
        }
    }
}
