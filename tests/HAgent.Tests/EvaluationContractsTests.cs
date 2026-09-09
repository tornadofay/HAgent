using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class EvaluationContractsTests
    {
        [Fact]
        public void EvaluationRequest_CloneIsIndependentAndPreservesCorrelation()
        {
            var request = new AiEvaluationRequest
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
            request.Inputs.Add(new AiEvaluationInputReference { Kind = "execution", Id = "execution-42", Role = "target" });
            request.Criteria["schema"] = "valid";
            request.Validate();

            var clone = request.Clone();
            Assert.NotSame(request, clone);
            Assert.Equal(request.Id, clone.Id);
            Assert.Equal(request.TargetKind, clone.TargetKind);
            Assert.Equal(request.TargetId, clone.TargetId);
            Assert.Equal(request.ExecutionId, clone.ExecutionId);
            Assert.Equal(request.TraceId, clone.TraceId);
            Assert.NotSame(request.Inputs, clone.Inputs);
            Assert.NotSame(request.Inputs[0], clone.Inputs[0]);
            Assert.Equal("target", clone.Inputs[0].Role);
            Assert.NotSame(request.Criteria, clone.Criteria);
            Assert.Equal("valid", clone.Criteria["schema"]);

            clone.Inputs[0].Role = "mutated";
            clone.Criteria["schema"] = "changed";
            Assert.Equal("target", request.Inputs[0].Role);
            Assert.Equal("valid", request.Criteria["schema"]);
        }

        [Fact]
        public void Evaluation_ValidateAcceptsScoresAndConfidenceWithinBounds()
        {
            var evaluation = CreateEvaluation();
            evaluation.Score = 0.87m;
            evaluation.Confidence = 0.91d;
            evaluation.Validate();
        }

        [Fact]
        public void Evaluation_ValidateRejectsOutOfRangeScores()
        {
            var evaluation = CreateEvaluation();
            evaluation.Score = 1.01m;
            Assert.Throws<ArgumentOutOfRangeException>(() => evaluation.Validate());

            evaluation.Score = 0.5m;
            evaluation.Confidence = -0.01d;
            Assert.Throws<ArgumentOutOfRangeException>(() => evaluation.Validate());

            evaluation.Confidence = 1.01d;
            Assert.Throws<ArgumentOutOfRangeException>(() => evaluation.Validate());
        }

        [Fact]
        public void Evaluation_CloneDoesNotShareEvidenceOrMetadata()
        {
            var evaluation = CreateEvaluation();
            evaluation.Evidence.Add(new AiEvaluationInputReference { Kind = "trace", Id = "trace-42", Role = "provenance" });
            evaluation.Metadata["criterion"] = "completion";

            var clone = evaluation.Clone();
            clone.Evidence[0].Role = "changed";
            clone.Metadata["criterion"] = "changed";

            Assert.Equal("provenance", evaluation.Evidence[0].Role);
            Assert.Equal("completion", evaluation.Metadata["criterion"]);
        }

        [Fact]
        public async Task EvaluatorContract_IsProviderNeutralAndPreservesEvaluatorProvenance()
        {
            IAiEvaluator evaluator = new DeterministicEvaluator();
            var request = new AiEvaluationRequest
            {
                TargetKind = AiEvaluationTargetKind.Execution,
                TargetId = "execution-42",
                ExecutionId = "execution-42",
                TraceId = "trace-42"
            };

            var evaluation = await evaluator.EvaluateAsync(request, CancellationToken.None).ConfigureAwait(false);

            Assert.Equal("deterministic-test", evaluator.Id);
            Assert.Equal(AiEvaluatorKind.Deterministic, evaluator.Kind);
            Assert.Equal("1", evaluator.Version);
            Assert.Equal(AiEvaluationTargetKind.Execution, evaluation.TargetKind);
            Assert.Equal("execution-42", evaluation.TargetId);
            Assert.Equal("deterministic-test", evaluation.EvaluatorId);
            Assert.Equal(AiEvaluatorKind.Deterministic, evaluation.EvaluatorKind);
            Assert.Equal("1", evaluation.EvaluatorVersion);
            Assert.Equal("execution-42", evaluation.ExecutionId);
            Assert.Equal("trace-42", evaluation.TraceId);
            Assert.Equal(AiEvaluationOutcome.Passed, evaluation.Outcome);
        }

        [Fact]
        public void Evaluation_RejectsOversizedCollectionsAndInputObjects()
        {
            var request = new AiEvaluationRequest { TargetId = "execution-42" };
            for (var i = 0; i < 33; i++)
                request.Inputs.Add(new AiEvaluationInputReference { Kind = "execution", Id = "execution-" + i });
            Assert.Throws<ArgumentOutOfRangeException>(() => request.Validate());

            var evaluation = CreateEvaluation();
            for (var i = 0; i < 33; i++)
                evaluation.Metadata["key-" + i] = "value";
            Assert.Throws<ArgumentOutOfRangeException>(() => evaluation.Validate());
        }

        private static AiEvaluation CreateEvaluation()
        {
            return new AiEvaluation
            {
                TargetKind = AiEvaluationTargetKind.Response,
                TargetId = "response-42",
                Outcome = AiEvaluationOutcome.NeedsReview,
                EvaluatorId = "evaluator-42",
                EvaluatorKind = AiEvaluatorKind.Application,
                EvaluatorVersion = "2026.09",
                Label = "review",
                Reason = "Deterministic test evaluation.",
                AgentId = "agent-42",
                RuntimeInstanceId = "runtime-42",
                ExecutionId = "execution-42",
                TraceId = "trace-42"
            };
        }

        private sealed class DeterministicEvaluator : IAiEvaluator
        {
            public string Id { get { return "deterministic-test"; } }
            public AiEvaluatorKind Kind { get { return AiEvaluatorKind.Deterministic; } }
            public string Version { get { return "1"; } }

            public Task<AiEvaluation> EvaluateAsync(AiEvaluationRequest request, CancellationToken cancellationToken)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                request.Validate();
                cancellationToken.ThrowIfCancellationRequested();

                return Task.FromResult(new AiEvaluation
                {
                    TargetKind = request.TargetKind,
                    TargetId = request.TargetId,
                    Outcome = AiEvaluationOutcome.Passed,
                    EvaluatorId = Id,
                    EvaluatorKind = Kind,
                    EvaluatorVersion = Version,
                    AgentId = request.AgentId,
                    RuntimeInstanceId = request.RuntimeInstanceId,
                    ExecutionId = request.ExecutionId,
                    GoalId = request.GoalId,
                    PlanId = request.PlanId,
                    TraceId = request.TraceId,
                    Score = 1m,
                    Confidence = 1d,
                    Label = "pass",
                    Reason = "Deterministic evaluator contract test passed."
                });
            }
        }
    }
}
