using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class SuppliedEvaluationTests
    {
        [Fact]
        public async Task HumanRating_ProducesEvaluationWithSourceAndCorrelation()
        {
            var rating = new AiEvaluationRating
            {
                Outcome = AiEvaluationOutcome.Passed,
                Score = 0.9m,
                Confidence = 0.8d,
                Label = "good",
                Reason = "Reviewer accepted the response."
            };
            rating.Evidence.Add(new AiEvaluationInputReference { Kind = "review", Id = "review-1", Role = "human-rating" });

            IAiEvaluator evaluator = new AiSuppliedRatingEvaluator(rating, AiEvaluatorKind.Human, "reviewer-1", "2026.1");
            var evaluation = await evaluator.EvaluateAsync(CreateRequest(), CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(AiEvaluatorKind.Human, evaluation.EvaluatorKind);
            Assert.Equal("reviewer-1", evaluation.EvaluatorId);
            Assert.Equal("2026.1", evaluation.EvaluatorVersion);
            Assert.Equal(AiEvaluationOutcome.Passed, evaluation.Outcome);
            Assert.Equal(0.9m, evaluation.Score);
            Assert.Equal(0.8d, evaluation.Confidence);
            Assert.Equal("good", evaluation.Label);
            Assert.Equal("review-1", evaluation.Evidence[0].Id);
            Assert.Equal("Human", evaluation.Metadata["rating.source"]);
            Assert.Equal("execution-42", evaluation.ExecutionId);
        }

        [Fact]
        public async Task ApplicationRating_PreservesFailedLabelAndOwnedEvidence()
        {
            var rating = new AiEvaluationRating
            {
                Outcome = AiEvaluationOutcome.Failed,
                Score = 0m,
                Confidence = 1d,
                Label = "invalid",
                Reason = "Application validator rejected the result."
            };
            rating.Evidence.Add(new AiEvaluationInputReference { Kind = "validator", Id = "validator-1", Role = "application" });
            rating.Metadata["rule"] = "schema";

            var evaluator = new AiSuppliedRatingEvaluator(rating, AiEvaluatorKind.Application, "schema-validator", "1");
            var evaluation = await evaluator.EvaluateAsync(CreateRequest(), CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(AiEvaluatorKind.Application, evaluation.EvaluatorKind);
            Assert.Equal(AiEvaluationOutcome.Failed, evaluation.Outcome);
            Assert.Equal("invalid", evaluation.Label);
            Assert.Equal("schema", evaluation.Metadata["rule"]);
            Assert.Single(evaluation.Evidence);

            rating.Evidence[0].Role = "mutated";
            rating.Metadata["rule"] = "changed";
            Assert.Equal("application", evaluation.Evidence[0].Role);
            Assert.Equal("schema", evaluation.Metadata["rule"]);
        }

        [Fact]
        public void RatingValidation_RejectsOutOfRangeScoreConfidenceAndOversizedCollections()
        {
            var rating = new AiEvaluationRating { Score = 1.01m };
            Assert.Throws<ArgumentOutOfRangeException>(() => rating.Validate());

            rating.Score = 0.5m;
            rating.Confidence = -0.01d;
            Assert.Throws<ArgumentOutOfRangeException>(() => rating.Validate());

            rating.Confidence = 0.5d;
            for (var i = 0; i < 33; i++)
                rating.Metadata["m-" + i] = "value";
            Assert.Throws<ArgumentOutOfRangeException>(() => rating.Validate());
        }

        [Theory]
        [InlineData(AiEvaluatorKind.Deterministic)]
        [InlineData(AiEvaluatorKind.ModelAssisted)]
        public void SuppliedRatingEvaluator_RejectsNonExternalEvaluatorKinds(AiEvaluatorKind kind)
        {
            var rating = new AiEvaluationRating { Outcome = AiEvaluationOutcome.NeedsReview };
            Assert.Throws<ArgumentException>(() => new AiSuppliedRatingEvaluator(rating, kind, "evaluator"));
        }

        [Fact]
        public async Task SuppliedRatingEvaluator_ObservesCancellation()
        {
            var rating = new AiEvaluationRating { Outcome = AiEvaluationOutcome.NeedsReview };
            var evaluator = new AiSuppliedRatingEvaluator(rating, AiEvaluatorKind.Human, "reviewer");
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                    await evaluator.EvaluateAsync(CreateRequest(), cancellation.Token).ConfigureAwait(false));
            }
        }

        private static AiEvaluationRequest CreateRequest()
        {
            return new AiEvaluationRequest
            {
                TargetKind = AiEvaluationTargetKind.Response,
                TargetId = "response-42",
                AgentId = "agent-42",
                RuntimeInstanceId = "runtime-42",
                ExecutionId = "execution-42",
                GoalId = "goal-42",
                PlanId = "plan-42",
                TraceId = "trace-42"
            };
        }
    }
}
