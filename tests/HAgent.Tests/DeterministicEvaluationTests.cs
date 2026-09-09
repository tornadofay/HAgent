using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class DeterministicEvaluationTests
    {
        [Fact]
        public void EvaluationRequest_CloneCopiesObservationsWithoutSharingMutableState()
        {
            var request = CreateRequest();
            request.Observations.Add(new AiEvaluationObservation
            {
                Kind = "schema.valid",
                Id = "observation-1",
                ValueKind = AiEvaluationObservationKind.Boolean,
                BooleanValue = true
            });
            request.Validate();

            var clone = request.Clone();
            Assert.NotSame(request.Observations, clone.Observations);
            Assert.NotSame(request.Observations[0], clone.Observations[0]);
            clone.Observations[0].BooleanValue = false;
            Assert.True(request.Observations[0].BooleanValue.Value);
        }

        [Fact]
        public void Observation_ValidationRejectsAmbiguousOrUnboundedValues()
        {
            var observation = new AiEvaluationObservation
            {
                Kind = "schema.valid",
                Id = "observation-1",
                ValueKind = AiEvaluationObservationKind.Boolean,
                BooleanValue = true,
                DecimalValue = 1m
            };
            Assert.Throws<ArgumentException>(() => observation.Validate());

            observation.DecimalValue = null;
            observation.TextValue = new string('x', 2049);
            observation.ValueKind = AiEvaluationObservationKind.Text;
            Assert.Throws<ArgumentOutOfRangeException>(() => observation.Validate());
        }

        [Fact]
        public async Task BooleanRules_PassAndFailWithBoundedEvidence()
        {
            var request = CreateRequest();
            request.Observations.Add(new AiEvaluationObservation
            {
                Kind = "schema.valid",
                Id = "schema-check",
                ValueKind = AiEvaluationObservationKind.Boolean,
                BooleanValue = true
            });

            IAiEvaluator evaluator = new AiDeterministicEvaluationEvaluator(AiDeterministicEvaluationRuleKind.SchemaValidity);
            var evaluation = await evaluator.EvaluateAsync(request, CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(AiEvaluationOutcome.Passed, evaluation.Outcome);
            Assert.Equal(1m, evaluation.Score);
            Assert.Equal("pass", evaluation.Label);
            Assert.Single(evaluation.Evidence);
            Assert.Equal("schema-check", evaluation.Evidence[0].Id);
            Assert.Equal("schema.valid", evaluation.Evidence[0].Role);
            Assert.Equal("SchemaValidity", evaluation.Metadata["rule"]);
        }

        [Theory]
        [InlineData(AiDeterministicEvaluationRuleKind.RequiredFields, "required-fields.complete")]
        [InlineData(AiDeterministicEvaluationRuleKind.PolicyCompliance, "policy.compliant")]
        [InlineData(AiDeterministicEvaluationRuleKind.ToolSuccess, "tool.success")]
        [InlineData(AiDeterministicEvaluationRuleKind.TaskCompletion, "task.completed")]
        public async Task BooleanRules_FalseObservationProducesFailedEvaluation(AiDeterministicEvaluationRuleKind rule, string signal)
        {
            var request = CreateRequest();
            request.Observations.Add(new AiEvaluationObservation
            {
                Kind = signal,
                Id = "signal-1",
                ValueKind = AiEvaluationObservationKind.Boolean,
                BooleanValue = false
            });

            var evaluator = new AiDeterministicEvaluationEvaluator(rule);
            var evaluation = await evaluator.EvaluateAsync(request, CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(AiEvaluationOutcome.Failed, evaluation.Outcome);
            Assert.Equal(0m, evaluation.Score);
            Assert.Equal(1d, evaluation.Confidence);
            Assert.Equal("fail", evaluation.Label);
            Assert.Single(evaluation.Evidence);
        }

        [Fact]
        public async Task ThresholdRules_PassAndFailUsingInvariantMaximum()
        {
            var request = CreateRequest();
            request.Criteria["max"] = "125.5";
            request.Observations.Add(new AiEvaluationObservation
            {
                Kind = "latency.ms",
                Id = "latency-1",
                ValueKind = AiEvaluationObservationKind.Decimal,
                DecimalValue = 125.5m,
                Unit = "ms"
            });

            var evaluator = new AiDeterministicEvaluationEvaluator(AiDeterministicEvaluationRuleKind.Latency);
            var passed = await evaluator.EvaluateAsync(request, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(AiEvaluationOutcome.Passed, passed.Outcome);
            Assert.Equal("125.5", passed.Metadata["threshold"]);
            Assert.Equal("ms", passed.Metadata["unit"]);

            request.Observations[0].DecimalValue = 125.51m;
            var failed = await evaluator.EvaluateAsync(request, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(AiEvaluationOutcome.Failed, failed.Outcome);
            Assert.Equal(0m, failed.Score);
        }

        [Fact]
        public async Task CostRule_UsesTheSameProviderNeutralThresholdContract()
        {
            var request = CreateRequest();
            request.Criteria["max"] = "0.25";
            request.Observations.Add(new AiEvaluationObservation
            {
                Kind = "cost",
                Id = "cost-1",
                ValueKind = AiEvaluationObservationKind.Decimal,
                DecimalValue = 0.20m,
                Unit = "USD"
            });

            var evaluation = await new AiDeterministicEvaluationEvaluator(AiDeterministicEvaluationRuleKind.Cost)
                .EvaluateAsync(request, CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(AiEvaluationOutcome.Passed, evaluation.Outcome);
            Assert.Equal("0.2", evaluation.Metadata["observed"]);
            Assert.Equal("0.25", evaluation.Metadata["threshold"]);
        }

        [Fact]
        public async Task MissingOrMismatchedSignalsAreInconclusiveRatherThanAutoritative()
        {
            var missing = await new AiDeterministicEvaluationEvaluator(AiDeterministicEvaluationRuleKind.ToolSuccess)
                .EvaluateAsync(CreateRequest(), CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(AiEvaluationOutcome.Inconclusive, missing.Outcome);
            Assert.Null(missing.Score);
            Assert.Equal("missing-signal", missing.Label);

            var request = CreateRequest();
            request.Observations.Add(new AiEvaluationObservation
            {
                Kind = "tool.success",
                Id = "tool-1",
                ValueKind = AiEvaluationObservationKind.Text,
                TextValue = "sensitive-looking-provider-payload"
            });
            var mismatched = await new AiDeterministicEvaluationEvaluator(AiDeterministicEvaluationRuleKind.ToolSuccess)
                .EvaluateAsync(request, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(AiEvaluationOutcome.Inconclusive, mismatched.Outcome);
            Assert.Null(mismatched.Score);
            Assert.False(mismatched.Metadata.ContainsKey("observed"));
        }

        [Fact]
        public async Task ThresholdRulesRejectMissingOrInvalidThresholdWithoutPassing()
        {
            var missingThreshold = CreateRequest();
            missingThreshold.Observations.Add(new AiEvaluationObservation
            {
                Kind = "cost",
                Id = "cost-1",
                ValueKind = AiEvaluationObservationKind.Decimal,
                DecimalValue = 0.2m
            });
            var evaluator = new AiDeterministicEvaluationEvaluator(AiDeterministicEvaluationRuleKind.Cost);
            var missing = await evaluator.EvaluateAsync(missingThreshold, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(AiEvaluationOutcome.Inconclusive, missing.Outcome);
            Assert.Null(missing.Score);

            missingThreshold.Criteria["max"] = "not-a-number";
            var invalid = await evaluator.EvaluateAsync(missingThreshold, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(AiEvaluationOutcome.Inconclusive, invalid.Outcome);
            Assert.Null(invalid.Score);
        }

        [Fact]
        public async Task DuplicateDeterministicSignalsAreRejectedAsAmbiguous()
        {
            var request = CreateRequest();
            request.Observations.Add(new AiEvaluationObservation
            {
                Kind = "schema.valid",
                Id = "schema-1",
                ValueKind = AiEvaluationObservationKind.Boolean,
                BooleanValue = true
            });
            request.Observations.Add(new AiEvaluationObservation
            {
                Kind = "schema.valid",
                Id = "schema-2",
                ValueKind = AiEvaluationObservationKind.Boolean,
                BooleanValue = false
            });

            var evaluator = new AiDeterministicEvaluationEvaluator(AiDeterministicEvaluationRuleKind.SchemaValidity);
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await evaluator.EvaluateAsync(request, CancellationToken.None).ConfigureAwait(false));
        }

        [Fact]
        public async Task Evaluator_ObservesCancellationBeforeProducingEvidence()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                var request = CreateRequest();
                request.Observations.Add(new AiEvaluationObservation
                {
                    Kind = "task.completed",
                    Id = "task-1",
                    ValueKind = AiEvaluationObservationKind.Boolean,
                    BooleanValue = true
                });

                var evaluator = new AiDeterministicEvaluationEvaluator(AiDeterministicEvaluationRuleKind.TaskCompletion);
                await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                    await evaluator.EvaluateAsync(request, cancellation.Token).ConfigureAwait(false));
            }
        }

        private static AiEvaluationRequest CreateRequest()
        {
            return new AiEvaluationRequest
            {
                TargetKind = AiEvaluationTargetKind.Execution,
                TargetId = "execution-42",
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
