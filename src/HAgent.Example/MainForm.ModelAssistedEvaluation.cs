using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private static int _modelAssistedEvaluationTabAdded;

        private void AddModelAssistedEvaluationTab()
        {
            if (Interlocked.Exchange(ref _modelAssistedEvaluationTabAdded, 1) != 0)
                return;

            AddApiTab(
                "Model-Assisted Evaluation",
                "Run model-assisted evaluation",
                "Runs the public model-assisted evaluator boundary with a deterministic in-process judge and verifies detached request snapshots, judge provenance, bounded evidence, and non-authoritative output.",
                "The evaluation should preserve the response target, return NeedsReview from the judge, retain judge/evaluator provenance, and explicitly remain non-authoritative.",
                "This is a boundary verification scenario. The judge is an in-process stand-in; no provider or remote grading service is contacted.",
                TestModelAssistedEvaluationAsync,
                "Model-judge boundary",
                "Provider/model transport belongs behind the injected IAiEvaluationJudge implementation. Evaluation evidence never becomes authorization or authoritative agent state.");
        }

        private async Task TestModelAssistedEvaluationAsync(string unused)
        {
            var judge = new ExampleModelEvaluationJudge();
            var evaluator = new AiModelAssistedEvaluationEvaluator(judge, "example-model-evaluator", "1");
            var request = new AiEvaluationRequest
            {
                TargetKind = AiEvaluationTargetKind.Response,
                TargetId = "response-model-judge-42",
                AgentId = "agent-model-judge-42",
                RuntimeInstanceId = "runtime-model-judge-42",
                ExecutionId = "execution-model-judge-42",
                GoalId = "goal-model-judge-42",
                PlanId = "plan-model-judge-42",
                TraceId = "trace-model-judge-42"
            };
            request.Inputs.Add(new AiEvaluationInputReference
            {
                Kind = "response",
                Id = request.TargetId,
                Role = "judge-target"
            });
            request.Criteria["quality"] = "review-for-accuracy";

            var evaluation = await evaluator.EvaluateAsync(request, CancellationToken.None).ConfigureAwait(true);
            evaluation.Validate();

            if (evaluation.TargetId != request.TargetId || evaluation.TargetKind != AiEvaluationTargetKind.Response)
                throw new InvalidOperationException("Model-assisted evaluation did not preserve target identity.");
            if (evaluation.Outcome != AiEvaluationOutcome.NeedsReview || evaluation.Score.HasValue || evaluation.Label != "needs-review")
                throw new InvalidOperationException("Model-assisted judge outcome was not preserved.");
            if (evaluation.EvaluatorKind != AiEvaluatorKind.ModelAssisted || evaluation.EvaluatorId != evaluator.Id || evaluation.EvaluatorVersion != evaluator.Version)
                throw new InvalidOperationException("Model-assisted evaluator provenance was not preserved.");
            if (evaluation.Metadata["evaluation.source"] != "model-assisted" || evaluation.Metadata["evaluation.authoritative"] != "false")
                throw new InvalidOperationException("Model-assisted evaluation was not explicitly marked non-authoritative.");
            if (evaluation.Metadata["judge.id"] != judge.Id || evaluation.Metadata["judge.version"] != judge.Version)
                throw new InvalidOperationException("Judge provenance was not preserved.");
            if (evaluation.Evidence.Count != 1 || evaluation.Evidence[0].Id != request.TargetId)
                throw new InvalidOperationException("Bounded evaluation evidence was not preserved.");

            request.TargetId = "mutated-after-call";
            if (evaluation.TargetId == request.TargetId)
                throw new InvalidOperationException("Produced evaluation was coupled to caller mutation.");

            Write(
                "MODEL-ASSISTED EVALUATION",
                "Model-assisted evaluator boundary succeeded." + Environment.NewLine +
                "Target identity: preserved." + Environment.NewLine +
                "Outcome: NeedsReview; label needs-review (verified)." + Environment.NewLine +
                "Evaluator provenance: ModelAssisted/example version 1." + Environment.NewLine +
                "Judge provenance: " + judge.Id + "/" + judge.Version + "." + Environment.NewLine +
                "Bounded evidence reference: preserved." + Environment.NewLine +
                "Detached request snapshot: verified." + Environment.NewLine +
                "Authoritative agent/configuration mutation: none." + Environment.NewLine +
                "Provider/model transport: none.");
        }

        private sealed class ExampleModelEvaluationJudge : IAiEvaluationJudge
        {
            public string Id { get { return "example-model-judge"; } }
            public string Version { get { return "1"; } }

            public Task<AiEvaluationRating> JudgeAsync(AiEvaluationJudgeRequest request, CancellationToken cancellationToken)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                request.Validate();
                cancellationToken.ThrowIfCancellationRequested();

                var rating = new AiEvaluationRating
                {
                    Outcome = AiEvaluationOutcome.NeedsReview,
                    Confidence = 0.42d,
                    Label = "needs-review",
                    Reason = "The example model judge could not establish sufficient confidence for automatic acceptance."
                };
                rating.Evidence.Add(new AiEvaluationInputReference
                {
                    Kind = "response",
                    Id = request.EvaluationRequest.TargetId,
                    Role = "judged-response"
                });
                rating.Metadata["judge.mode"] = "deterministic-example";
                return Task.FromResult(rating);
            }
        }
    }
}
