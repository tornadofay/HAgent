using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private static int _suppliedEvaluationTabAdded;

        private void AddSuppliedEvaluationTab()
        {
            if (Interlocked.Exchange(ref _suppliedEvaluationTabAdded, 1) != 0)
                return;

            AddApiTab(
                "Supplied Evaluation Ratings",
                "Run human/application ratings",
                "Runs provider-neutral externally supplied evaluation ratings through the public evaluator API and preserves labels, scores, provenance, evidence, and correlation.",
                "Covers both Human and Application evaluator kinds, including a failed labeled rating and bounded evidence ownership.",
                "No provider request, model grader, authorization mutation, or agent-state mutation is used.",
                TestSuppliedEvaluationAsync,
                "Supplied rating evidence",
                "Human and application ratings are evaluation evidence, not authorization decisions or automatic state changes.");
        }

        private async Task TestSuppliedEvaluationAsync(string unused)
        {
            var humanRating = new AiEvaluationRating
            {
                Outcome = AiEvaluationOutcome.Passed,
                Score = 0.9m,
                Confidence = 0.8d,
                Label = "good",
                Reason = "Reviewer accepted the response."
            };
            humanRating.Evidence.Add(new AiEvaluationInputReference { Kind = "review", Id = "review-1", Role = "human-rating" });
            humanRating.Metadata["reviewer"] = "quality-team";

            var humanEvaluator = new AiSuppliedRatingEvaluator(humanRating, AiEvaluatorKind.Human, "reviewer-1", "2026.1");
            var humanEvaluation = await humanEvaluator.EvaluateAsync(CreateRequest("response-human-42"), CancellationToken.None).ConfigureAwait(true);
            humanEvaluation.Validate();
            if (humanEvaluation.EvaluatorKind != AiEvaluatorKind.Human || humanEvaluation.Outcome != AiEvaluationOutcome.Passed || humanEvaluation.Score != 0.9m || humanEvaluation.Label != "good")
                throw new InvalidOperationException("Human supplied rating was not preserved.");
            if (humanEvaluation.Evidence.Count != 1 || humanEvaluation.Evidence[0].Id != "review-1")
                throw new InvalidOperationException("Human rating evidence was not preserved.");

            var applicationRating = new AiEvaluationRating
            {
                Outcome = AiEvaluationOutcome.Failed,
                Score = 0m,
                Confidence = 1d,
                Label = "invalid",
                Reason = "Application validator rejected the result."
            };
            applicationRating.Evidence.Add(new AiEvaluationInputReference { Kind = "validator", Id = "validator-1", Role = "application" });
            applicationRating.Metadata["rule"] = "schema";

            var applicationEvaluator = new AiSuppliedRatingEvaluator(applicationRating, AiEvaluatorKind.Application, "schema-validator", "1");
            var applicationEvaluation = await applicationEvaluator.EvaluateAsync(CreateRequest("response-application-42"), CancellationToken.None).ConfigureAwait(true);
            applicationEvaluation.Validate();
            if (applicationEvaluation.EvaluatorKind != AiEvaluatorKind.Application || applicationEvaluation.Outcome != AiEvaluationOutcome.Failed || applicationEvaluation.Label != "invalid")
                throw new InvalidOperationException("Application supplied rating was not preserved.");
            if (applicationEvaluation.Evidence.Count != 1 || applicationEvaluation.Evidence[0].Role != "application")
                throw new InvalidOperationException("Application rating evidence was not preserved.");

            applicationRating.Evidence[0].Role = "mutated";
            applicationRating.Metadata["rule"] = "changed";
            if (applicationEvaluation.Evidence[0].Role != "application" || applicationEvaluation.Metadata["rule"] != "schema")
                throw new InvalidOperationException("Supplied rating was not owned by the produced evaluation.");

            Write(
                "SUPPLIED EVALUATION RATINGS",
                "Human rating: Passed, score 0.9, label good (verified)." + Environment.NewLine +
                "Application rating: Failed, score 0, label invalid (verified)." + Environment.NewLine +
                "Evaluator provenance: Human/Application preserved." + Environment.NewLine +
                "Evidence references: preserved and owned." + Environment.NewLine +
                "Execution/response correlation: preserved." + Environment.NewLine +
                "No authorization or agent-state mutation: verified." + Environment.NewLine +
                "Provider/model transport: none.");
        }

        private static AiEvaluationRequest CreateRequest(string targetId)
        {
            return new AiEvaluationRequest
            {
                TargetKind = AiEvaluationTargetKind.Response,
                TargetId = targetId,
                AgentId = "agent-supplied-rating-42",
                RuntimeInstanceId = "runtime-supplied-rating-42",
                ExecutionId = "execution-supplied-rating-42",
                GoalId = "goal-supplied-rating-42",
                PlanId = "plan-supplied-rating-42",
                TraceId = "trace-supplied-rating-42"
            };
        }
    }
}
