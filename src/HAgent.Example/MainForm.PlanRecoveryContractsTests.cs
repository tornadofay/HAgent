using System;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddPlanRecoveryContractsTab()
        {
            AddApiTab(
                "RESTART & RECOVERY",
                "Run restart/recovery contract test",
                "Verifies 0.9591 Slice 5 recovery boundaries for process/runtime restart, stale authority invalidation, incomplete-step reconciliation, and durable plan revision preservation.",
                "Recovery creates no new plan revision, invalidates the previous runtime authority, keeps terminal steps terminal, and routes unknown external outcomes to host review.",
                "Restart and recovery contract verification.",
                TestPlanRecoveryContracts,
                "Provider-neutral restart/recovery boundary",
                "Contract exercise only; persistence backends and external side effects are not contacted.");
        }

        private Task TestPlanRecoveryContracts(string message)
        {
            var now = DateTimeOffset.UtcNow;
            var plan = new AiPlan
            {
                Id = "plan-recovery-example",
                GoalId = "goal-recovery-example",
                IntentionId = "intention-recovery-example",
                Title = "Recovery example plan",
                Status = AiPlanStatus.Active,
                Revision = 3L,
                RevisionReason = "Recovery example"
            };
            plan.Provenance.Source = "Example";
            plan.Provenance.Authority = AiGoalAuthority.HostSupplied;

            plan.Steps.Add(new AiPlanStep
            {
                Id = "step-running",
                PlanId = plan.Id,
                Sequence = 1,
                Title = "Interrupted step",
                Status = AiPlanStepStatus.Running,
                Revision = 3L
            });
            plan.Steps.Add(new AiPlanStep
            {
                Id = "step-complete",
                PlanId = plan.Id,
                Sequence = 2,
                Title = "Completed step",
                Status = AiPlanStepStatus.Completed,
                Revision = 3L
            });
            plan.Steps[0].Provenance.Source = "Example";
            plan.Steps[0].Provenance.Authority = AiGoalAuthority.HostSupplied;
            plan.Steps[1].Provenance.Source = "Example";
            plan.Steps[1].Provenance.Authority = AiGoalAuthority.HostSupplied;
            plan.Validate();

            var unknownOperation = new AiPlanStepOperation(
                "operation-recovery-example",
                plan.Id,
                "step-running",
                plan.Revision,
                1,
                AiPlanOperationStatus.UnknownOutcome,
                "host-correlation-recovery",
                "External side effect result was not confirmed before restart.",
                now.AddSeconds(1));

            var record = AiPlanRecoveryEvaluator.Recover(
                plan,
                new[] { unknownOperation },
                "runtime-before-restart",
                "runtime-after-restart",
                7L,
                8L,
                AiPlanRecoveryReason.Restart,
                now.AddSeconds(2));

            if (record.PlanRevision != 3L || !record.PreviousAuthorityInvalidated)
                throw new InvalidOperationException("Recovery did not preserve plan revision and invalidate previous authority.");
            if (record.StepDecisions[0].Disposition != AiPlanRecoveryStepDisposition.RequiresHostReview)
                throw new InvalidOperationException("Unknown external outcome did not require host review.");
            if (record.StepDecisions[1].Disposition != AiPlanRecoveryStepDisposition.AlreadyTerminal)
                throw new InvalidOperationException("Completed step was incorrectly made recoverable.");

            Write("RESTART & RECOVERY",
                "Contract test succeeded." + Environment.NewLine +
                "Plan ID: " + record.PlanId + Environment.NewLine +
                "Plan revision preserved: " + record.PlanRevision + Environment.NewLine +
                "Previous authority invalidated: " + (record.PreviousAuthorityInvalidated ? "yes" : "no") + Environment.NewLine +
                "Interrupted step recovery: " + record.StepDecisions[0].Disposition + Environment.NewLine +
                "Unknown operation host review: " + (record.StepDecisions[0].Disposition == AiPlanRecoveryStepDisposition.RequiresHostReview ? "yes" : "no") + Environment.NewLine +
                "Completed step revived: no");

            return Task.CompletedTask;
        }
    }
}
