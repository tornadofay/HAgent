using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class ModelAssistedEvaluationTests
    {
        [Fact]
        public async Task ModelJudge_ProducesNonAuthoritativeEvaluationWithJudgeProvenance()
        {
            var judge = new RecordingJudge
            {
                Rating = CreateRating(AiEvaluationOutcome.Passed, 0.84m, 0.91d, "good")
            };
            var evaluator = new AiModelAssistedEvaluationEvaluator(judge, "judge-evaluator-1", "2");

            var evaluation = await evaluator.EvaluateAsync(CreateRequest("response-42"), CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(AiEvaluatorKind.ModelAssisted, evaluation.EvaluatorKind);
            Assert.Equal("judge-evaluator-1", evaluation.EvaluatorId);
            Assert.Equal("2", evaluation.EvaluatorVersion);
            Assert.Equal(AiEvaluationOutcome.Passed, evaluation.Outcome);
            Assert.Equal(0.84m, evaluation.Score);
            Assert.Equal(0.91d, evaluation.Confidence);
            Assert.Equal("good", evaluation.Label);
            Assert.Equal("model-assisted", evaluation.Metadata["evaluation.source"]);
            Assert.Equal("false", evaluation.Metadata["evaluation.authoritative"]);
            Assert.Equal("judge-42", evaluation.Metadata["judge.id"]);
            Assert.Equal("1.4", evaluation.Metadata["judge.version"]);
            Assert.Equal("execution-42", evaluation.ExecutionId);
            Assert.Equal("trace-42", evaluation.TraceId);
        }

        [Fact]
        public async Task ModelJudge_PreservesNeedsReviewAndOwnedEvidence()
        {
            var judge = new RecordingJudge
            {
                Rating = CreateRating(AiEvaluationOutcome.NeedsReview, null, 0.42d, "uncertain")
            };
            judge.Rating.Evidence.Add(new AiEvaluationInputReference { Kind = "review-input", Id = "input-1", Role = "bounded-reference" });
            judge.Rating.Metadata["model"] = "grader-model-1";

            var evaluator = new AiModelAssistedEvaluationEvaluator(judge, "model-evaluator", "1");
            var evaluation = await evaluator.EvaluateAsync(CreateRequest("response-review-42"), CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(AiEvaluationOutcome.NeedsReview, evaluation.Outcome);
            Assert.Null(evaluation.Score);
            Assert.Equal("uncertain", evaluation.Label);
            Assert.Single(evaluation.Evidence);
            Assert.Equal("input-1", evaluation.Evidence[0].Id);
            Assert.Equal("grader-model-1", evaluation.Metadata["model"]);

            judge.Rating.Evidence[0].Role = "mutated";
            judge.Rating.Metadata["model"] = "changed";
            Assert.Equal("bounded-reference", evaluation.Evidence[0].Role);
            Assert.Equal("grader-model-1", evaluation.Metadata["model"]);
        }

        [Fact]
        public async Task ModelJudge_UsesDetachedRequestSnapshot()
        {
            var entered = new TaskCompletionSource<bool>();
            var release = new TaskCompletionSource<bool>();
            var judge = new RecordingJudge
            {
                Rating = CreateRating(AiEvaluationOutcome.Passed, 1m, 1d, "pass"),
                BeforeReturn = async request =>
                {
                    Assert.Equal("response-original", request.EvaluationRequest.TargetId);
                    entered.SetResult(true);
                    await release.Task.ConfigureAwait(false);
                }
            };
            var evaluator = new AiModelAssistedEvaluationEvaluator(judge, "snapshot-evaluator");
            var request = CreateRequest("response-original");

            var task = evaluator.EvaluateAsync(request, CancellationToken.None);
            await entered.Task.ConfigureAwait(false);
            request.TargetId = "response-mutated";
            release.SetResult(true);

            var evaluation = await task.ConfigureAwait(false);
            Assert.Equal("response-original", evaluation.TargetId);
        }

        [Fact]
        public async Task ModelJudge_IsConcurrencySafeAcrossIndependentRequests()
        {
            var judge = new RecordingJudge
            {
                RatingFactory = request => CreateRating(
                    AiEvaluationOutcome.Passed,
                    request.EvaluationRequest.TargetId.EndsWith("-1", StringComparison.Ordinal) ? 1m : 0.5m,
                    1d,
                    request.EvaluationRequest.TargetId)
            };
            var evaluator = new AiModelAssistedEvaluationEvaluator(judge, "concurrent-evaluator");

            var first = evaluator.EvaluateAsync(CreateRequest("response-1"), CancellationToken.None);
            var second = evaluator.EvaluateAsync(CreateRequest("response-2"), CancellationToken.None);
            var evaluations = await Task.WhenAll(first, second).ConfigureAwait(false);

            Assert.Equal(2, judge.RequestCount);
            Assert.Equal("response-1", evaluations[0].TargetId);
            Assert.Equal(1m, evaluations[0].Score);
            Assert.Equal("response-2", evaluations[1].TargetId);
            Assert.Equal(0.5m, evaluations[1].Score);
        }

        [Fact]
        public async Task ModelJudge_ObservesCancellationBeforeJudge()
        {
            var judge = new RecordingJudge { Rating = CreateRating(AiEvaluationOutcome.Passed, 1m, 1d, "pass") };
            var evaluator = new AiModelAssistedEvaluationEvaluator(judge, "cancel-evaluator");
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                    await evaluator.EvaluateAsync(CreateRequest("response-cancel"), cancellation.Token).ConfigureAwait(false));
            }

            Assert.Equal(0, judge.RequestCount);
        }

        [Fact]
        public async Task ModelJudge_RejectsCanceledResultEvenWhenJudgeReturnsAfterCancellation()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var judge = new RecordingJudge
                {
                    RatingFactory = request =>
                    {
                        cancellation.Cancel();
                        return CreateRating(AiEvaluationOutcome.Passed, 1m, 1d, "late");
                    }
                };
                var evaluator = new AiModelAssistedEvaluationEvaluator(judge, "late-cancel-evaluator");

                await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                    await evaluator.EvaluateAsync(CreateRequest("response-late-cancel"), cancellation.Token).ConfigureAwait(false));
            }
        }

        [Fact]
        public async Task ModelJudge_PropagatesJudgeFailureAndRejectsNullRating()
        {
            var failing = new RecordingJudge { Failure = new InvalidOperationException("judge failure") };
            var evaluator = new AiModelAssistedEvaluationEvaluator(failing, "failure-evaluator");
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await evaluator.EvaluateAsync(CreateRequest("response-failure"), CancellationToken.None).ConfigureAwait(false));
            Assert.Equal("judge failure", exception.Message);

            var nullJudge = new RecordingJudge { ReturnNull = true };
            evaluator = new AiModelAssistedEvaluationEvaluator(nullJudge, "null-evaluator");
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await evaluator.EvaluateAsync(CreateRequest("response-null"), CancellationToken.None).ConfigureAwait(false));
        }

        [Fact]
        public void ModelJudgeConstructor_RequiresExplicitBoundedIdentity()
        {
            var rating = CreateRating(AiEvaluationOutcome.NeedsReview, null, null, "review");
            var judge = new RecordingJudge { Rating = rating };

            Assert.Throws<ArgumentNullException>(() => new AiModelAssistedEvaluationEvaluator(null, "evaluator"));
            Assert.Throws<ArgumentException>(() => new AiModelAssistedEvaluationEvaluator(judge, " "));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AiModelAssistedEvaluationEvaluator(judge, new string('x', 257)));
        }

        private static AiEvaluationRequest CreateRequest(string targetId)
        {
            return new AiEvaluationRequest
            {
                TargetKind = AiEvaluationTargetKind.Response,
                TargetId = targetId,
                AgentId = "agent-42",
                RuntimeInstanceId = "runtime-42",
                ExecutionId = "execution-42",
                GoalId = "goal-42",
                PlanId = "plan-42",
                TraceId = "trace-42"
            };
        }

        private static AiEvaluationRating CreateRating(
            AiEvaluationOutcome outcome,
            decimal? score,
            double? confidence,
            string label)
        {
            return new AiEvaluationRating
            {
                Outcome = outcome,
                Score = score,
                Confidence = confidence,
                Label = label,
                Reason = "Model-assisted judge result."
            };
        }

        private sealed class RecordingJudge : IAiEvaluationJudge
        {
            private int _requestCount;

            public string Id { get { return "judge-42"; } }
            public string Version { get { return "1.4"; } }
            public int RequestCount { get { return _requestCount; } }
            public AiEvaluationRating Rating { get; set; }
            public Func<AiEvaluationJudgeRequest, AiEvaluationRating> RatingFactory { get; set; }
            public Func<AiEvaluationJudgeRequest, Task> BeforeReturn { get; set; }
            public Exception Failure { get; set; }
            public bool ReturnNull { get; set; }

            public async Task<AiEvaluationRating> JudgeAsync(AiEvaluationJudgeRequest request, CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref _requestCount);
                cancellationToken.ThrowIfCancellationRequested();
                if (Failure != null)
                    throw Failure;
                if (BeforeReturn != null)
                    await BeforeReturn(request).ConfigureAwait(false);
                if (ReturnNull)
                    return null;
                if (RatingFactory != null)
                    return RatingFactory(request);
                return Rating == null ? null : Rating.Clone();
            }
        }
    }
}
