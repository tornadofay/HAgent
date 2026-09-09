using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private static int _evaluationAggregationTabAdded;

        private void AddEvaluationAggregationTab()
        {
            if (Interlocked.Exchange(ref _evaluationAggregationTabAdded, 1) != 0)
                return;

            AddApiTab(
                "Evaluation Aggregation",
                "Run aggregation test",
                "Aggregates public evaluation results by execution-target variant and compares success, quality, latency, cost, fallback, tool-success, and plan-completion metrics without making the comparison authoritative.",
                "Two variants should produce distinct aggregates and a comparison showing the faster/lower-cost/high-quality variant as preferred per metric direction.",
                "This scenario is fully local and uses only HAgent.Core public contracts; no provider or remote evaluation service is contacted.",
                TestEvaluationAggregationAsync,
                "Evaluation evidence boundary",
                "Aggregation is measurement only. A preferred variant is a comparative indication, not an authorization, routing, configuration, or cognitive decision.");
        }

        private Task TestEvaluationAggregationAsync(string unused)
        {
            return TestEvaluationAggregationCoreAsync();
        }

        private static Task TestEvaluationAggregationCoreAsync()
        {
            var request = new AiEvaluationAggregationRequest();
            AddSample(request, "case-1", "baseline", AiEvaluationOutcome.Passed, 0.70m, 220m, 0.08m, 0.20m, 0.90m, 0.60m);
            AddSample(request, "case-2", "baseline", AiEvaluationOutcome.Failed, 0.40m, 180m, 0.06m, 0.10m, 0.80m, 0.50m);
            AddSample(request, "case-3", "baseline", AiEvaluationOutcome.NeedsReview, null, 200m, 0.07m, 0.15m, 0.85m, 0.55m);
            AddSample(request, "case-1", "candidate", AiEvaluationOutcome.Passed, 0.90m, 110m, 0.03m, 0.04m, 0.98m, 0.92m);
            AddSample(request, "case-2", "candidate", AiEvaluationOutcome.Passed, 0.80m, 120m, 0.04m, 0.02m, 0.96m, 0.90m);
            AddSample(request, "case-3", "candidate", AiEvaluationOutcome.Passed, 0.85m, 100m, 0.02m, 0.03m, 0.97m, 0.94m);

            var aggregates = AiEvaluationAggregator.Aggregate(request, CancellationToken.None);
            var baseline = aggregates.Single(x => x.VariantId == "baseline");
            var candidate = aggregates.Single(x => x.VariantId == "candidate");

            if (baseline.SampleCount != 3 || baseline.PassedCount != 1 || baseline.FailedCount != 1 || baseline.NeedsReviewCount != 1)
                throw new InvalidOperationException("Baseline outcome aggregation was incorrect.");
            if (baseline.SuccessRate != 0.333333m || baseline.QualityScoreAverage.Value != 0.55m)
                throw new InvalidOperationException("Baseline success/quality aggregation was incorrect.");
            if (candidate.SuccessRate != 1m || candidate.QualityScoreAverage.Value != 0.85m)
                throw new InvalidOperationException("Candidate success/quality aggregation was incorrect.");

            var latency = candidate.Metrics.Single(x => x.Kind == AiEvaluationMetricKind.LatencyMilliseconds);
            if (latency.Average != 110m || latency.Minimum != 100m || latency.Maximum != 120m || latency.HigherIsBetter)
                throw new InvalidOperationException("Candidate latency aggregation was incorrect.");

            var comparison = AiEvaluationAggregator.Compare(candidate, baseline, CancellationToken.None);
            if (comparison.LeftVariantId != "candidate" || comparison.RightVariantId != "baseline")
                throw new InvalidOperationException("Comparison variant identity was not preserved.");
            if (comparison.Metrics.Single(x => x.Name == "success-rate").PreferredVariantId != "candidate")
                throw new InvalidOperationException("Success-rate comparison did not prefer the higher-performing variant.");
            if (comparison.Metrics.Single(x => x.Name == "latency-ms").PreferredVariantId != "candidate")
                throw new InvalidOperationException("Latency comparison did not prefer the lower-latency variant.");
            if (comparison.Metrics.Single(x => x.Name == "cost").PreferredVariantId != "candidate")
                throw new InvalidOperationException("Cost comparison did not prefer the lower-cost variant.");
            if (comparison.Metrics.Single(x => x.Name == "fallback-frequency").PreferredVariantId != "candidate")
                throw new InvalidOperationException("Fallback comparison did not prefer the lower-frequency variant.");
            if (comparison.Metrics.Single(x => x.Name == "tool-success-rate").PreferredVariantId != "candidate")
                throw new InvalidOperationException("Tool-success comparison did not prefer the higher-performing variant.");
            if (comparison.Metrics.Single(x => x.Name == "plan-completion-rate").PreferredVariantId != "candidate")
                throw new InvalidOperationException("Plan-completion comparison did not prefer the higher-performing variant.");

            baseline.Validate();
            candidate.Validate();
            comparison.Validate();

            return Task.Run(delegate
            {
                var summary =
                    "EVALUATION AGGREGATION" + Environment.NewLine +
                    "Evaluation aggregation succeeded." + Environment.NewLine +
                    "Variants: baseline=3 samples; candidate=3 samples." + Environment.NewLine +
                    "Baseline success rate: " + baseline.SuccessRate.ToString("0.######") + "." + Environment.NewLine +
                    "Candidate success rate: " + candidate.SuccessRate.ToString("0.######") + "." + Environment.NewLine +
                    "Candidate average quality score: " + candidate.QualityScoreAverage.Value.ToString("0.######") + "." + Environment.NewLine +
                    "Candidate average latency: " + latency.Average.ToString("0.######") + " ms." + Environment.NewLine +
                    "Per-metric comparison: candidate preferred where higher/lower direction supports it." + Environment.NewLine +
                    "Authoritative routing or authorization decision: none.";
                return summary;
            });
        }

        private static void AddSample(AiEvaluationAggregationRequest request, string caseId, string variantId, AiEvaluationOutcome outcome, decimal? score, decimal latencyMs, decimal cost, decimal fallback, decimal toolSuccessRate, decimal planCompletionRate)
        {
            var evaluation = new AiEvaluation
            {
                TargetKind = AiEvaluationTargetKind.Execution,
                TargetId = variantId + ":" + caseId,
                EvaluatorId = "example-evaluator",
                EvaluatorVersion = "1",
                Outcome = outcome,
                Score = score
            };
            var sample = new AiEvaluationSample { CaseId = caseId, VariantId = variantId, Evaluation = evaluation };
            sample.Metrics.Add(Metric(AiEvaluationMetricKind.LatencyMilliseconds, "latency-ms", latencyMs, "ms"));
            sample.Metrics.Add(Metric(AiEvaluationMetricKind.Cost, "cost", cost, "usd"));
            sample.Metrics.Add(Metric(AiEvaluationMetricKind.FallbackFrequency, "fallback-frequency", fallback, "ratio"));
            sample.Metrics.Add(Metric(AiEvaluationMetricKind.ToolSuccessRate, "tool-success-rate", toolSuccessRate, "ratio"));
            sample.Metrics.Add(Metric(AiEvaluationMetricKind.PlanCompletionRate, "plan-completion-rate", planCompletionRate, "ratio"));
            request.Samples.Add(sample);
        }

        private static AiEvaluationMetric Metric(AiEvaluationMetricKind kind, string name, decimal value, string unit)
        {
            return new AiEvaluationMetric { Kind = kind, Name = name, Value = value, Unit = unit };
        }
    }
}
