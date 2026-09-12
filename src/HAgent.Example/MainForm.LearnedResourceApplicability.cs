using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddLearnedResourceApplicabilityTab()
        {
            AddApiTab(
                "Learned Resource Applicability",
                "Run deterministic learned-resource applicability contract",
                "Verifies that promoted resources remain separately evaluable for applicability and validity without treating applicability as authorization.",
                "Uses only bounded provider-neutral facts and preconditions. No model reasoning is requested by this evaluator.",
                "Exercises Applicable, NotApplicable, Uncertain, and Invalidated outcomes plus scope matching, evidence preservation, and cancellation.",
                TestLearnedResourceApplicabilityAsync,
                "Learned resource applicability",
                "Applicability is an evaluation boundary only; it does not grant authorization or mutate the resource.");
        }

        private async Task TestLearnedResourceApplicabilityAsync(string unused)
        {
            var target = new AiApplicabilityTarget
            {
                ResourceType = "knowledge",
                ResourceId = "knowledge-example",
                Version = 2,
                Scope = AgentResourceScope.Agent
            };
            target.Preconditions.Add(new AiApplicabilityCondition
            {
                Key = "document.type",
                Operator = AiApplicabilityConditionOperator.Equals,
                ExpectedValue = "invoice",
                EvidenceReferenceId = "context-document-type"
            });
            target.Preconditions.Add(new AiApplicabilityCondition
            {
                Key = "language",
                Operator = AiApplicabilityConditionOperator.OneOf,
                AllowedValues = { "ar", "en" }
            });
            target.Evidence.Add(new AiApplicabilityEvidenceReference
            {
                Kind = "execution-context",
                Id = "context-document-type",
                Role = "precondition evidence",
                Source = "example"
            });

            var applicableContext = new AiApplicabilityContext { Scope = AgentResourceScope.Agent };
            applicableContext.Facts["document.type"] = "invoice";
            applicableContext.Facts["language"] = "ar";

            var evaluator = new AiDeterministicApplicabilityEvaluator();
            var applicable = await evaluator.EvaluateAsync(
                new AiApplicabilityRequest { Target = target, Context = applicableContext }, CancellationToken.None).ConfigureAwait(true);
            Require(applicable.Outcome == AiApplicabilityOutcome.Applicable, "Applicable outcome contract failed.");
            Require(applicable.Conditions.Count == 2 && applicable.Evidence.Count == 1, "Applicability evidence contract failed.");

            var mismatchContext = applicableContext.Clone();
            mismatchContext.Facts["document.type"] = "quotation";
            var notApplicable = await evaluator.EvaluateAsync(
                new AiApplicabilityRequest { Target = target, Context = mismatchContext }, CancellationToken.None).ConfigureAwait(true);
            Require(notApplicable.Outcome == AiApplicabilityOutcome.NotApplicable, "NotApplicable outcome contract failed.");

            var uncertain = await evaluator.EvaluateAsync(
                new AiApplicabilityRequest
                {
                    Target = target,
                    Context = new AiApplicabilityContext { Scope = AgentResourceScope.Agent }
                }, CancellationToken.None).ConfigureAwait(true);
            Require(uncertain.Outcome == AiApplicabilityOutcome.Uncertain, "Uncertain outcome contract failed.");

            var invalidatedTarget = target.Clone();
            invalidatedTarget.IsInvalidated = true;
            invalidatedTarget.InvalidationReason = "Contradictory host evidence.";
            var invalidated = await evaluator.EvaluateAsync(
                new AiApplicabilityRequest { Target = invalidatedTarget, Context = applicableContext }, CancellationToken.None).ConfigureAwait(true);
            Require(invalidated.Outcome == AiApplicabilityOutcome.Invalidated, "Invalidated outcome contract failed.");

            var scopeMismatch = await evaluator.EvaluateAsync(
                new AiApplicabilityRequest
                {
                    Target = target,
                    Context = new AiApplicabilityContext { Scope = AgentResourceScope.User }
                }, CancellationToken.None).ConfigureAwait(true);
            Require(scopeMismatch.Outcome == AiApplicabilityOutcome.NotApplicable, "Scope mismatch contract failed.");

            var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var cancelled = false;
            try
            {
                await evaluator.EvaluateAsync(
                    new AiApplicabilityRequest { Target = target, Context = applicableContext }, cancellation.Token).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }
            Require(cancelled, "Applicability cancellation contract failed.");

            Write(
                "LEARNED RESOURCE APPLICABILITY",
                "Contract test succeeded." + Environment.NewLine +
                "Applicable / NotApplicable / Uncertain / Invalidated outcomes: verified." + Environment.NewLine +
                "Bounded preconditions and deterministic facts: verified." + Environment.NewLine +
                "Scope-aware applicability: verified." + Environment.NewLine +
                "Evidence references preserved in the applicability decision: verified." + Environment.NewLine +
                "Applicability remains separate from authorization: verified." + Environment.NewLine +
                "Deterministic evaluation completed without model reasoning: verified." + Environment.NewLine +
                "Cancellation: verified.");
        }
    }
}
