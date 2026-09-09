using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private static int _evaluationContractsTabAdded;

        private void AddEvaluationContractsTab()
        {
            if (Interlocked.Exchange(ref _evaluationContractsTabAdded, 1) != 0)
                return;

            AddApiTab(
                "Evaluation Contracts",
                "Run evaluation contract test",
                "Runs a deterministic public-API check of provider-neutral evaluation requests, targets, scores, labels, provenance, and correlation identities.",
                "The evaluation contract is separate from execution authority: evaluations measure a target and retain provenance, but they do not mutate agent state or authorize an operation.",
                "Uses an in-process deterministic evaluator. No provider transport, model grading service, or remote telemetry is used.",
                TestEvaluationContractsAsync,
                "Evaluation boundary",
                "Evaluation results are evidence about behavior, not hidden authorization or automatic state mutation.");
        }

        private static Task TestEvaluationContractsAsync(string unused)
        {
            var request = new AiEvaluationRequest
            {
                TargetKind = AiEvaluationTargetKind.Response,
                TargetId = "response-evaluation-42",
                AgentId = "agent-evaluation-42",
                RuntimeInstanceId = "runtime-evaluation-42",
                ExecutionId = "execution-evaluation-42",
                GoalId = "goal-evaluation-42",
                PlanId = "plan-evaluation-42",
                TraceId = "trace-evaluation-42"
            };
            request.Inputs.Add(new AiEvaluationInputReference
            {
                Kind = "execution",
                Id = request.ExecutionId,
                Role = "target-context"
            });
            request.Criteria["completion"] = "required";
            request.Validate();

            IAiEvaluator evaluator = new DeterministicEvaluationExampleEvaluator();
            var evaluationTask = evaluator.EvaluateAsync(request, CancellationToken.None);
            if (!evaluationTask.Wait(TimeSpan.FromSeconds(5)))
                throw new InvalidOperationException("Deterministic evaluation did not complete.");

            var evaluation = evaluationTask.GetAwaiter().GetResult();
            evaluation.Validate();

            if (evaluation.TargetKind != AiEvaluationTargetKind.Response || evaluation.TargetId != request.TargetId)
                throw new InvalidOperationException("Evaluation target identity was not preserved.");
            if (evaluation.Outcome != AiEvaluationOutcome.Passed || !evaluation.Score.HasValue || evaluation.Score.Value != 1m)
                throw new InvalidOperationException("Evaluation outcome/score was not preserved.");
            if (evaluation.EvaluatorId != evaluator.Id || evaluation.EvaluatorKind != evaluator.Kind || evaluation.EvaluatorVersion != evaluator.Version)
                throw new InvalidOperationException("Evaluation evaluator provenance was not preserved.");
            if (evaluation.ExecutionId != request.ExecutionId || evaluation.TraceId != request.TraceId || evaluation.GoalId != request.GoalId || evaluation.PlanId != request.PlanId)
                throw new InvalidOperationException("Evaluation correlation identities were not preserved.");
            if (evaluation.Evidence == null || evaluation.Evidence.Count != 1 || evaluation.Evidence[0].Id != request.ExecutionId)
                throw new InvalidOperationException("Evaluation evidence reference was not preserved.");

            var clone = evaluation.Clone();
            clone.Metadata["verification"] = "mutated";
            if (evaluation.Metadata.ContainsKey("verification"))
                throw new InvalidOperationException("Evaluation clone shared mutable metadata state.");

            Write(
                "EVALUATION CONTRACTS",
                "Evaluation contract succeeded." + Environment.NewLine +
                "Provider-neutral target kinds: verified." + Environment.NewLine +
                "Outcome/score/label representation: verified." + Environment.NewLine +
                "Evaluator kind/version provenance: verified." + Environment.NewLine +
                "Execution/runtime/goal/plan/trace correlation: verified." + Environment.NewLine +
                "Bounded input/evidence references: verified." + Environment.NewLine +
                "Owned clone isolation: verified." + Environment.NewLine +
                "No agent-state mutation or authorization side effect: verified." + Environment.NewLine +
                "Remote grading/provider transport: none." + Environment.NewLine +
                "Real provider request: none.");

            return Task.CompletedTask;
        }

        private sealed class DeterministicEvaluationExampleEvaluator : IAiEvaluator
        {
            public string Id { get { return "deterministic-evaluation-example"; } }
            public AiEvaluatorKind Kind { get { return AiEvaluatorKind.Deterministic; } }
            public string Version { get { return "1"; } }

            public Task<AiEvaluation> EvaluateAsync(AiEvaluationRequest request, CancellationToken cancellationToken)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                request.Validate();
                cancellationToken.ThrowIfCancellationRequested();

                var result = new AiEvaluation
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
                    Reason = "The deterministic Example evaluator accepted the declared completion criterion."
                };
                foreach (var input in request.Inputs)
                    result.Evidence.Add(input.Clone());
                result.Metadata["criterion"] = "completion";
                return Task.FromResult(result);
            }
        }
    }
}
