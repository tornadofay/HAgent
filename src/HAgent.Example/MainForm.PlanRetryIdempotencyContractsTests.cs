using System;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddPlanRetryIdempotencyContractsTab()
        {
            AddApiTab(
                "RETRY & IDEMPOTENCY",
                "Run retry/idempotency contract test",
                "Verifies stable operation identity, explicit requested/completed/failed/unknown states, and bounded retry decisions.",
                "A failed operation is retryable only when the host establishes retry safety. Requested and unknown external outcomes require reconciliation before another attempt.",
                "Retry and idempotency contract verification.",
                TestPlanRetryIdempotencyContracts,
                "Durable operation retry/idempotency boundary",
                "Provider-free contract exercise; no external provider or persistence backend is contacted.");
        }

        private Task TestPlanRetryIdempotencyContracts(string message)
        {
            var now = DateTimeOffset.UtcNow;
            var failed = new AiPlanStepOperation(
                "operation-example",
                "plan-example",
                "step-2",
                3L,
                2,
                AiPlanOperationStatus.Failed,
                "host-correlation-example",
                "Host observed the attempt fail before any external side effect.",
                now);

            var allowed = AiPlanRetryDecision.Evaluate(failed, true, now.AddSeconds(1));
            var unknown = new AiPlanStepOperation(
                "operation-unknown",
                "plan-example",
                "step-3",
                3L,
                1,
                AiPlanOperationStatus.UnknownOutcome,
                "host-correlation-unknown",
                "Timeout after request submission.",
                now.AddSeconds(2));
            var reconciliation = AiPlanRetryDecision.Evaluate(unknown, true, now.AddSeconds(3));

            if (failed.Id != "operation-example" || failed.Attempt != 2)
                throw new InvalidOperationException("Operation identity or attempt metadata was not preserved.");
            if (allowed.Kind != AiPlanRetryDecisionKind.RetryAllowed)
                throw new InvalidOperationException("A host-confirmed safe failure was not classified as retryable.");
            if (reconciliation.Kind != AiPlanRetryDecisionKind.ReconciliationRequired)
                throw new InvalidOperationException("Unknown external outcome was allowed to bypass reconciliation.");

            Write("RETRY & IDEMPOTENCY",
                "Contract test succeeded." + Environment.NewLine +
                "Operation ID: " + failed.Id + Environment.NewLine +
                "Plan revision: " + failed.PlanRevision + Environment.NewLine +
                "Attempt: " + failed.Attempt + Environment.NewLine +
                "Failed operation retry: " + allowed.Kind + Environment.NewLine +
                "Unknown operation retry: " + reconciliation.Kind + Environment.NewLine +
                "Unknown outcome automatically retryable: no");

            return Task.CompletedTask;
        }
    }
}
