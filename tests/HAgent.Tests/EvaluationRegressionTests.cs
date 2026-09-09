using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class EvaluationRegressionTests
    {
        [Fact]
        public void SuiteValidation_RejectsDuplicateCaseAndTargetIds()
        {
            var suite = CreateSuite();
            suite.Cases.Add(Case("case-1"));
            suite.Cases.Add(Case("case-1"));
            Assert.Throws<ArgumentException>(() => suite.Validate());

            suite = CreateSuite();
            suite.Cases.Add(Case("case-1"));
            suite.Targets.Add(Target("variant-a"));
            suite.Targets.Add(Target("variant-a"));
            Assert.Throws<ArgumentException>(() => suite.Validate());
        }

        [Fact]
        public void SuiteValidation_RejectsMoreThanMaximumCaseTargetExecutions()
        {
            var suite = new AiEvaluationRegressionSuite
            {
                Id = "suite-1",
                Name = "Bounded suite",
                MaxConcurrency = 4
            };

            for (var i = 0; i < 128; i++)
                suite.Cases.Add(Case("case-" + i));
            for (var i = 0; i < 9; i++)
                suite.Targets.Add(Target("variant-" + i));

            Assert.Throws<ArgumentOutOfRangeException>(() => suite.Validate());
        }

        [Fact]
        public async Task RunAsync_ExecutesEveryCaseAgainstEveryTargetAndOrdersResultsDeterministically()
        {
            var suite = CreateSuite();
            suite.MaxConcurrency = 3;
            suite.Cases.Add(Case("case-b"));
            suite.Cases.Add(Case("case-a"));
            suite.Targets.Add(Target("variant-b"));
            suite.Targets.Add(Target("variant-a"));

            var executor = new DelayedExecutor();
            var run = await AiEvaluationRegressionRunner.RunAsync(suite, executor, CancellationToken.None);

            Assert.Equal(AiEvaluationRegressionRunStatus.Completed, run.Status);
            Assert.Equal(4, run.Results.Count);
            Assert.All(run.Results, result => Assert.Equal(AiEvaluationRegressionCaseStatus.Completed, result.Status));
            Assert.Equal(4, run.Samples.Count);
            Assert.Equal(
                new[] { "case-a/variant-a", "case-a/variant-b", "case-b/variant-a", "case-b/variant-b" },
                run.Results.Select(x => x.CaseId + "/" + x.VariantId).ToArray());
            Assert.Equal(4, executor.Calls.Count);
        }

        [Fact]
        public async Task RunAsync_RespectsConfiguredMaximumConcurrency()
        {
            var suite = CreateSuite();
            suite.MaxConcurrency = 2;
            for (var i = 0; i < 4; i++)
                suite.Cases.Add(Case("case-" + i));
            suite.Targets.Add(Target("variant-a"));
            suite.Targets.Add(Target("variant-b"));

            var executor = new ConcurrencyTrackingExecutor();
            var run = await AiEvaluationRegressionRunner.RunAsync(suite, executor, CancellationToken.None);

            Assert.Equal(AiEvaluationRegressionRunStatus.Completed, run.Status);
            Assert.Equal(8, run.Results.Count);
            Assert.True(executor.MaximumObserved <= 2);
            Assert.True(executor.MaximumObserved > 1);
        }

        [Fact]
        public async Task RunAsync_CapturesExecutorFailureWithoutFabricatingEvaluation()
        {
            var suite = CreateSuite();
            suite.Cases.Add(Case("case-1"));
            suite.Targets.Add(Target("variant-a"));
            suite.Targets.Add(Target("variant-b"));

            var executor = new FailingExecutor("variant-b");
            var run = await AiEvaluationRegressionRunner.RunAsync(suite, executor, CancellationToken.None);

            Assert.Equal(AiEvaluationRegressionRunStatus.Failed, run.Status);
            var failed = run.Results.Single(x => x.VariantId == "variant-b");
            Assert.Equal(AiEvaluationRegressionCaseStatus.Failed, failed.Status);
            Assert.Equal("executor-failed", failed.ErrorCode);
            Assert.Null(failed.Sample);
            Assert.Single(run.Samples);
            Assert.Equal("variant-a", run.Samples[0].VariantId);
        }

        [Fact]
        public async Task RunAsync_RejectsMismatchedSampleIdentity()
        {
            var suite = CreateSuite();
            suite.Cases.Add(Case("case-1"));
            suite.Targets.Add(Target("variant-a"));

            var executor = new MismatchExecutor();
            var run = await AiEvaluationRegressionRunner.RunAsync(suite, executor, CancellationToken.None);

            Assert.Equal(AiEvaluationRegressionRunStatus.Failed, run.Status);
            var result = Assert.Single(run.Results);
            Assert.Equal(AiEvaluationRegressionCaseStatus.Failed, result.Status);
            Assert.Equal("sample-identity-mismatch", result.ErrorCode);
            Assert.Empty(run.Samples);
        }

        [Fact]
        public async Task RunAsync_LateExecutorCompletionAfterCancellationIsDiscarded()
        {
            var suite = CreateSuite();
            suite.MaxConcurrency = 1;
            suite.Cases.Add(Case("case-1"));
            suite.Targets.Add(Target("variant-a"));
            suite.Targets.Add(Target("variant-b"));

            using (var cancellation = new CancellationTokenSource())
            {
                var executor = new LateCompletionExecutor(() => cancellation.Cancel());
                var run = await AiEvaluationRegressionRunner.RunAsync(suite, executor, cancellation.Token);

                Assert.Equal(AiEvaluationRegressionRunStatus.Canceled, run.Status);
                Assert.All(run.Results, result => Assert.Equal(AiEvaluationRegressionCaseStatus.Canceled, result.Status));
                Assert.Empty(run.Samples);
            }
        }

        [Fact]
        public async Task RunAsync_UsesDetachedSuiteSnapshotBeforeExecutorInvocations()
        {
            var suite = CreateSuite();
            suite.Cases.Add(Case("case-1"));
            suite.Targets.Add(Target("variant-a"));

            var executor = new SnapshotExecutor();
            var task = AiEvaluationRegressionRunner.RunAsync(suite, executor, CancellationToken.None);
            await executor.Started.Task;

            suite.Cases[0].Id = "mutated-case";
            suite.Targets[0].Id = "mutated-target";
            suite.Cases[0].Parameters["prompt"] = "mutated";
            executor.Release.TrySetResult(true);

            var run = await task;
            Assert.Equal(AiEvaluationRegressionCaseStatus.Completed, Assert.Single(run.Results).Status);
            Assert.Equal("case-1", run.Results[0].CaseId);
            Assert.Equal("variant-a", run.Results[0].VariantId);
            Assert.Equal("original", executor.ObservedParameter);
        }

        [Fact]
        public async Task CreateAggregationRequest_ReturnsOnlyCompletedSamplesAndOwnsThem()
        {
            var suite = CreateSuite();
            suite.Cases.Add(Case("case-1"));
            suite.Cases.Add(Case("case-2"));
            suite.Targets.Add(Target("variant-a"));
            suite.Targets.Add(Target("variant-b"));

            var executor = new FailingExecutor("variant-b", "case-2");
            var run = await AiEvaluationRegressionRunner.RunAsync(suite, executor, CancellationToken.None);

            var request = run.CreateAggregationRequest();
            Assert.Equal(3, request.Samples.Count);
            Assert.Equal(2, request.Samples.Count(sample => sample.VariantId == "variant-a"));
            Assert.Equal(1, request.Samples.Count(sample => sample.VariantId == "variant-b"));

            request.Samples[0].Evaluation.Score = 0m;
            Assert.NotEqual(0m, run.Samples[0].Evaluation.Score);
        }

        private static AiEvaluationRegressionSuite CreateSuite()
        {
            return new AiEvaluationRegressionSuite
            {
                Id = "suite-1",
                Name = "Regression suite",
                MaxConcurrency = 4
            };
        }

        private static AiEvaluationRegressionCase Case(string id)
        {
            var testCase = new AiEvaluationRegressionCase
            {
                Id = id,
                Name = "Case " + id
            };
            testCase.Inputs.Add(new AiEvaluationInputReference
            {
                Kind = "regression-input",
                Id = id + "-input",
                Role = "test"
            });
            testCase.Parameters["prompt"] = "original";
            return testCase;
        }

        private static AiEvaluationRegressionTarget Target(string id)
        {
            return new AiEvaluationRegressionTarget
            {
                Id = id,
                Name = "Target " + id
            };
        }

        private static AiEvaluationSample Sample(AiEvaluationRegressionCase testCase, AiEvaluationRegressionTarget target)
        {
            return new AiEvaluationSample
            {
                CaseId = testCase.Id,
                VariantId = target.Id,
                Evaluation = new AiEvaluation
                {
                    TargetKind = AiEvaluationTargetKind.Execution,
                    TargetId = target.Id + ":" + testCase.Id,
                    EvaluatorId = "regression-test",
                    EvaluatorVersion = "1",
                    Outcome = AiEvaluationOutcome.Passed,
                    Score = 0.8m
                }
            };
        }

        private sealed class DelayedExecutor : IAiEvaluationRegressionExecutor
        {
            public readonly List<string> Calls = new List<string>();

            public async Task<AiEvaluationSample> ExecuteAsync(AiEvaluationRegressionCase testCase, AiEvaluationRegressionTarget target, CancellationToken cancellationToken)
            {
                Calls.Add(testCase.Id + "/" + target.Id);
                await Task.Delay((testCase.Id == "case-b" ? 5 : 20) + (target.Id == "variant-b" ? 5 : 0), cancellationToken);
                return Sample(testCase, target);
            }
        }

        private sealed class ConcurrencyTrackingExecutor : IAiEvaluationRegressionExecutor
        {
            private int _active;
            private int _maximumObserved;

            public int MaximumObserved { get { return Volatile.Read(ref _maximumObserved); } }

            public async Task<AiEvaluationSample> ExecuteAsync(AiEvaluationRegressionCase testCase, AiEvaluationRegressionTarget target, CancellationToken cancellationToken)
            {
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
                    await Task.Delay(25, cancellationToken);
                    return Sample(testCase, target);
                }
                finally
                {
                    Interlocked.Decrement(ref _active);
                }
            }
        }

        private sealed class FailingExecutor : IAiEvaluationRegressionExecutor
        {
            private readonly string _failedVariant;
            private readonly string _failedCase;

            public FailingExecutor(string failedVariant, string failedCase = null)
            {
                _failedVariant = failedVariant;
                _failedCase = failedCase;
            }

            public Task<AiEvaluationSample> ExecuteAsync(AiEvaluationRegressionCase testCase, AiEvaluationRegressionTarget target, CancellationToken cancellationToken)
            {
                if (string.Equals(target.Id, _failedVariant, StringComparison.Ordinal) && (_failedCase == null || string.Equals(testCase.Id, _failedCase, StringComparison.Ordinal)))
                    throw new InvalidOperationException("simulated failure");
                return Task.FromResult(Sample(testCase, target));
            }
        }

        private sealed class MismatchExecutor : IAiEvaluationRegressionExecutor
        {
            public Task<AiEvaluationSample> ExecuteAsync(AiEvaluationRegressionCase testCase, AiEvaluationRegressionTarget target, CancellationToken cancellationToken)
            {
                var sample = Sample(testCase, target);
                sample.VariantId = "wrong-variant";
                return Task.FromResult(sample);
            }
        }

        private sealed class LateCompletionExecutor : IAiEvaluationRegressionExecutor
        {
            private readonly Action _cancel;

            public LateCompletionExecutor(Action cancel)
            {
                _cancel = cancel;
            }

            public async Task<AiEvaluationSample> ExecuteAsync(AiEvaluationRegressionCase testCase, AiEvaluationRegressionTarget target, CancellationToken cancellationToken)
            {
                _cancel();
                await Task.Delay(10);
                return Sample(testCase, target);
            }
        }

        private sealed class SnapshotExecutor : IAiEvaluationRegressionExecutor
        {
            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
            public readonly TaskCompletionSource<bool> Release = new TaskCompletionSource<bool>();
            public string ObservedParameter { get; private set; }

            public async Task<AiEvaluationSample> ExecuteAsync(AiEvaluationRegressionCase testCase, AiEvaluationRegressionTarget target, CancellationToken cancellationToken)
            {
                ObservedParameter = testCase.Parameters["prompt"];
                Started.TrySetResult(true);
                await Release.Task;
                return Sample(testCase, target);
            }
        }
    }
}
