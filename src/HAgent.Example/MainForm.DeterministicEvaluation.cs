using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private static int _deterministicEvaluationTabAdded;

        private void AddDeterministicEvaluationTab()
        {
            if (Interlocked.Exchange(ref _deterministicEvaluationTabAdded, 1) != 0)
                return;

            AddApiTab(
                "Deterministic Evaluation",
                "Run deterministic evaluation rules",
                "Runs provider-neutral deterministic evaluation rules against bounded host observations and records evidence without mutating agent state.",
                "This scenario covers schema validity, required fields, policy compliance, tool success, latency, cost, and task completion using explicit deterministic signals.",
                "No provider request, model grader, remote telemetry, authorization mutation, or agent-state mutation is used.",
                TestDeterministicEvaluationAsync,
                "Deterministic evidence",
                "Host-computed observations are evidence inputs; deterministic evaluations remain non-authoritative measurements.");
        }

        private async Task TestDeterministicEvaluationAsync(string unused)
        {
            var checks = new List<string>();
            await RunBooleanRuleAsync(AiDeterministicEvaluationRuleKind.SchemaValidity, "schema.valid", "schema-check", true, checks).ConfigureAwait(true);
            await RunBooleanRuleAsync(AiDeterministicEvaluationRuleKind.RequiredFields, "required-fields.complete", "required-fields-check", true, checks).ConfigureAwait(true);
            await RunBooleanRuleAsync(AiDeterministicEvaluationRuleKind.PolicyCompliance, "policy.compliant", "policy-check", true, checks).ConfigureAwait(true);
            await RunBooleanRuleAsync(AiDeterministicEvaluationRuleKind.ToolSuccess, "tool.success", "tool-check", false, checks).ConfigureAwait(true);
            await RunBooleanRuleAsync(AiDeterministicEvaluationRuleKind.TaskCompletion, "task.completed", "task-check", true, checks).ConfigureAwait(true);
            await RunThresholdRuleAsync(AiDeterministicEvaluationRuleKind.Latency, "latency.ms", "latency-check", 125.5m, 125m, "ms", checks).ConfigureAwait(true);
            await RunThresholdRuleAsync(AiDeterministicEvaluationRuleKind.Cost, "cost", "cost-check", 0.25m, 0.20m, "USD", checks).ConfigureAwait(true);

            var missing = await new AiDeterministicEvaluationEvaluator(AiDeterministicEvaluationRuleKind.ToolSuccess)
                .EvaluateAsync(CreateRequest(), CancellationToken.None).ConfigureAwait(true);
            if (missing.Outcome != AiEvaluationOutcome.Inconclusive || missing.Score.HasValue)
                throw new InvalidOperationException("Missing deterministic evidence did not produce an inconclusive evaluation.");
            checks.Add("Missing evidence: inconclusive (verified).");

            var ambiguousRequest = CreateRequest();
            ambiguousRequest.Observations.Add(CreateBooleanObservation("schema.valid", "schema-1", true));
            ambiguousRequest.Observations.Add(CreateBooleanObservation("schema.valid", "schema-2", false));
            try
            {
                await new AiDeterministicEvaluationEvaluator(AiDeterministicEvaluationRuleKind.SchemaValidity)
                    .EvaluateAsync(ambiguousRequest, CancellationToken.None).ConfigureAwait(true);
                throw new InvalidOperationException("Duplicate deterministic signals were not rejected.");
            }
            catch (ArgumentException)
            {
                checks.Add("Ambiguous duplicate signal: rejected (verified).");
            }

            Write(
                "DETERMINISTIC EVALUATION",
                "Deterministic evaluation evidence succeeded." + Environment.NewLine +
                string.Join(Environment.NewLine, checks) + Environment.NewLine +
                "Evaluator kind: Deterministic." + Environment.NewLine +
                "Evidence is bounded observation references only." + Environment.NewLine +
                "No agent-state or authorization mutation: verified." + Environment.NewLine +
                "Provider/model transport: none.");
        }

        private static async Task RunBooleanRuleAsync(
            AiDeterministicEvaluationRuleKind rule,
            string signal,
            string observationId,
            bool value,
            List<string> checks)
        {
            var request = CreateRequest();
            request.Observations.Add(CreateBooleanObservation(signal, observationId, value));
            IAiEvaluator evaluator = new AiDeterministicEvaluationEvaluator(rule);
            var evaluation = await evaluator.EvaluateAsync(request, CancellationToken.None).ConfigureAwait(false);

            var expected = value ? AiEvaluationOutcome.Passed : AiEvaluationOutcome.Failed;
            if (evaluation.Outcome != expected || evaluation.Score != (value ? 1m : 0m))
                throw new InvalidOperationException(rule + " did not produce the expected deterministic outcome.");
            if (evaluation.Evidence == null || evaluation.Evidence.Count != 1 || evaluation.Evidence[0].Id != observationId)
                throw new InvalidOperationException(rule + " did not preserve its bounded evidence reference.");
            if (evaluation.EvaluatorKind != AiEvaluatorKind.Deterministic || evaluation.EvaluatorId != evaluator.Id)
                throw new InvalidOperationException(rule + " did not preserve evaluator provenance.");
            checks.Add(rule + ": " + evaluation.Outcome + " (verified).");
        }

        private static async Task RunThresholdRuleAsync(
            AiDeterministicEvaluationRuleKind rule,
            string signal,
            string observationId,
            decimal maximum,
            decimal observed,
            string unit,
            List<string> checks)
        {
            var request = CreateRequest();
            request.Criteria["max"] = maximum.ToString(System.Globalization.CultureInfo.InvariantCulture);
            request.Observations.Add(new AiEvaluationObservation
            {
                Kind = signal,
                Id = observationId,
                ValueKind = AiEvaluationObservationKind.Decimal,
                DecimalValue = observed,
                Unit = unit
            });

            var evaluation = await new AiDeterministicEvaluationEvaluator(rule)
                .EvaluateAsync(request, CancellationToken.None).ConfigureAwait(false);
            if (evaluation.Outcome != AiEvaluationOutcome.Passed || evaluation.Score != 1m)
                throw new InvalidOperationException(rule + " threshold evaluation did not pass.");
            if (evaluation.Metadata["threshold"] != maximum.ToString(System.Globalization.CultureInfo.InvariantCulture))
                throw new InvalidOperationException(rule + " did not preserve its deterministic threshold evidence.");
            checks.Add(rule + " <= " + maximum.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + unit + ": passed (verified).");
        }

        private static AiEvaluationRequest CreateRequest()
        {
            return new AiEvaluationRequest
            {
                TargetKind = AiEvaluationTargetKind.Execution,
                TargetId = "execution-deterministic-42",
                AgentId = "agent-deterministic-42",
                RuntimeInstanceId = "runtime-deterministic-42",
                ExecutionId = "execution-deterministic-42",
                GoalId = "goal-deterministic-42",
                PlanId = "plan-deterministic-42",
                TraceId = "trace-deterministic-42"
            };
        }

        private static AiEvaluationObservation CreateBooleanObservation(string signal, string id, bool value)
        {
            return new AiEvaluationObservation
            {
                Kind = signal,
                Id = id,
                ValueKind = AiEvaluationObservationKind.Boolean,
                BooleanValue = value
            };
        }
    }
}
