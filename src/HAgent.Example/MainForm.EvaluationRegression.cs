using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private static int _evaluationRegressionTabAdded;

        private void AddEvaluationRegressionTab()
        {
            if (Interlocked.Exchange(ref _evaluationRegressionTabAdded, 1) != 0)
                return;

            AddApiTab(
                "Evaluation Regression Suites",
                "Run regression suite",
                "Runs the same bounded evaluation cases against multiple execution-target variants with controlled concurrency, cancellation, failure isolation, and direct handoff to the existing aggregation contracts.",
                "Three cases should run against baseline and candidate targets, producing six completed samples in deterministic report order.",
                "This scenario is fully local and uses a deterministic in-process executor. No provider, model, network service, or persistence backend is contacted.",
                TestEvaluationRegressionAsync,
                "Evaluation regression boundary",
                "A regression result is measurement evidence. Execution failures are recorded as case failures rather than fabricated evaluations, and cancellation prevents late results from entering the report.");
        }

        private async Task TestEvaluationRegressionAsync(string unused)
        {
            var suite = new AiEvaluationRegressionSuite
            {
                Id = "example-evaluation-suite",
                Name = "Provider/model/agent comparison suite",
                MaxConcurrency = 2
            };

            suite.Cases.Add(CreateRegressionCase("case-1", "Verify customer lookup"));
            suite.Cases.Add(CreateRegressionCase("case-2", "Verify structured response"));
            suite.Cases.Add(CreateRegressionCase("case-3", "Verify tool-backed completion"));

            suite.Targets.Add(CreateRegressionTarget("baseline", "Baseline target", "provider-a", "model-a", "agent-a"));
            suite.Targets.Add(CreateRegressionTarget("candidate", "Candidate target", "provider-b", "model-b", "agent-b"));

            var executor = new ExampleRegressionExecutor();
            var run = await AiEvaluationRegressionRunner.RunAsync(suite, executor, CancellationToken.None);

            if (run.Status != AiEvaluationRegressionRunStatus.Completed || run.Results.Count != 6 || run.Samples.Count != 6)
                throw new InvalidOperationException("Regression suite did not complete all case-target executions.");
            if (executor.MaximumObserved > suite.MaxConcurrency)
                throw new InvalidOperationException("Regression suite exceeded its configured concurrency limit.");

            var aggregationRequest = run.CreateAggregationRequest();
            var aggregates = AiEvaluationAggregator.Aggregate(aggregationRequest, CancellationToken.None);
            var baseline = aggregates.Single(x => x.VariantId == "baseline");
            var candidate = aggregates.Single(x => x.VariantId == "candidate");
            var comparison = AiEvaluationAggregator.Compare(candidate, baseline, CancellationToken.None);

            if (candidate.SuccessRate <= baseline.SuccessRate)
                throw new InvalidOperationException("Candidate regression target did not improve success rate.");
            if (comparison.Metrics.Single(x => x.Name == "success-rate").PreferredVariantId != "candidate")
                throw new InvalidOperationException("Regression comparison did not prefer the candidate success rate.");

            var failingSuite = new AiEvaluationRegressionSuite
            {
                Id = "example-failure-suite",
                Name = "Failure handling",
                MaxConcurrency = 1
            };
            failingSuite.Cases.Add(CreateRegressionCase("case-failure", "Failure path"));
            failingSuite.Targets.Add(CreateRegressionTarget("failing", "Failing target", "provider-f", "model-f", "agent-f"));
            var failedRun = await AiEvaluationRegressionRunner.RunAsync(failingSuite, new ExampleRegressionExecutor("failing"), CancellationToken.None);
            var failedResult = failedRun.Results.Single();
            if (failedRun.Status != AiEvaluationRegressionRunStatus.Failed || failedResult.Status != AiEvaluationRegressionCaseStatus.Failed || failedResult.ErrorCode != "executor-failed" || failedRun.Samples.Count != 0)
                throw new InvalidOperationException("Regression executor failure was not isolated correctly.");

            var cancellation = new CancellationTokenSource();
            try
            {
                var canceledSuite = new AiEvaluationRegressionSuite
                {
                    Id = "example-cancel-suite",
                    Name = "Cancellation path",
                    MaxConcurrency = 1
                };
                canceledSuite.Cases.Add(CreateRegressionCase("case-cancel", "Cancellation path"));
                canceledSuite.Targets.Add(CreateRegressionTarget("slow", "Slow target", "provider-s", "model-s", "agent-s"));
                var canceledRun = await AiEvaluationRegressionRunner.RunAsync(canceledSuite, new CancelingRegressionExecutor(cancellation), cancellation.Token);
                if (canceledRun.Status != AiEvaluationRegressionRunStatus.Canceled || canceledRun.Samples.Count != 0 || canceledRun.Results.Any(x => x.Status != AiEvaluationRegressionCaseStatus.Canceled))
                    throw new InvalidOperationException("Regression cancellation did not discard late evaluation results.");
            }
            finally
            {
                cancellation.Dispose();
            }

            Write(
                "EVALUATION REGRESSION SUITES",
                "Regression suite succeeded." + Environment.NewLine +
                "Cases: 3; targets: 2; completed executions: " + run.Samples.Count + "." + Environment.NewLine +
                "Baseline success rate: " + baseline.SuccessRate.ToString("0.######") + "." + Environment.NewLine +
                "Candidate success rate: " + candidate.SuccessRate.ToString("0.######") + "." + Environment.NewLine +
                "Maximum observed concurrency: " + executor.MaximumObserved + "." + Environment.NewLine +
                "Failure handling: isolated without fabricating evaluation evidence." + Environment.NewLine +
                "Cancellation handling: late results discarded." + Environment.NewLine +
                "Aggregation handoff: successful samples only." + Environment.NewLine +
                "Authoritative routing or authorization decision: none.");
        }

        private static AiEvaluationRegressionCase CreateRegressionCase(string id, string name)
        {
            var testCase = new AiEvaluationRegressionCase
            {
                Id = id,
                Name = name
            };
            testCase.Inputs.Add(new AiEvaluationInputReference
            {
                Kind = "example-regression-input",
                Id = id + "-input",
                Role = "test"
            });
            testCase.Parameters["intent"] = id;
            return testCase;
        }

        private static AiEvaluationRegressionTarget CreateRegressionTarget(string id, string name, string providerId, string modelId, string agentId)
        {
            var target = new AiEvaluationRegressionTarget
            {
                Id = id,
                Name = name
            };
            target.Metadata["provider-id"] = providerId;
            target.Metadata["model-id"] = modelId;
            target.Metadata["agent-id"] = agentId;
            return target;
        }

        private sealed class ExampleRegressionExecutor : IAiEvaluationRegressionExecutor
        {
            private readonly string _failedTarget;
            private int _active;
            private int _maximumObserved;

            public ExampleRegressionExecutor(string failedTarget = null)
            {
                _failedTarget = failedTarget;
            }

            public int MaximumObserved { get { return Volatile.Read(ref _maximumObserved); } }

            public async Task<AiEvaluationSample> ExecuteAsync(AiEvaluationRegressionCase testCase, AiEvaluationRegressionTarget target, CancellationToken cancellationToken)
            {
                if (string.Equals(target.Id, _failedTarget, StringComparison.Ordinal))
                    throw new InvalidOperationException("simulated regression failure");

                var active = Interlocked.Increment(ref _active);
                while (true)
                {
                    var observed = Volatile.Read(ref _maximumObserved);
                    if (active <= observed)
                        break;
                    if (Interlocked.CompareExchange(ref _maximumObserved, active, observed) == observed)
                        break;
                }

                try
                {
                    await Task.Delay(target.Id == "candidate" ? 8 : 12, cancellationToken);
                    var passed = string.Equals(testCase.Id, "case-3", StringComparison.Ordinal) || string.Equals(target.Id, "candidate", StringComparison.Ordinal);
                    return new AiEvaluationSample
                    {
                        CaseId = testCase.Id,
                        VariantId = target.Id,
                        Evaluation = new AiEvaluation
                        {
                            TargetKind = AiEvaluationTargetKind.Execution,
                            TargetId = target.Id + ":" + testCase.Id,
                            EvaluatorId = "example-regression-evaluator",
                            EvaluatorVersion = "1",
                            Outcome = passed ? AiEvaluationOutcome.Passed : AiEvaluationOutcome.Failed,
                            Score = passed ? 0.9m : 0.4m
                        }
                    };
                }
                finally
                {
                    Interlocked.Decrement(ref _active);
                }
            }
        }

        private sealed class CancelingRegressionExecutor : IAiEvaluationRegressionExecutor
        {
            private readonly CancellationTokenSource _cancellation;

            public CancelingRegressionExecutor(CancellationTokenSource cancellation)
            {
                _cancellation = cancellation;
            }

            public async Task<AiEvaluationSample> ExecuteAsync(AiEvaluationRegressionCase testCase, AiEvaluationRegressionTarget target, CancellationToken cancellationToken)
            {
                _cancellation.Cancel();
                await Task.Delay(10);
                return new AiEvaluationSample
                {
                    CaseId = testCase.Id,
                    VariantId = target.Id,
                    Evaluation = new AiEvaluation
                    {
                        TargetKind = AiEvaluationTargetKind.Execution,
                        TargetId = target.Id + ":" + testCase.Id,
                        EvaluatorId = "late-example-evaluator",
                        EvaluatorVersion = "1",
                        Outcome = AiEvaluationOutcome.Passed,
                        Score = 1m
                    }
                };
            }
        }
    }
}
