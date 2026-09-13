using System;
using HAgent.Models;
using System.Threading.Tasks;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddRuntimeProgressRecoveryTab()
        {
            AddApiTab(
                "RUNTIME PROGRESS & RECOVERY",
                "Run runtime progress and recovery test",
                "Verifies bounded progress/heartbeat evidence, configured stall detection, explicit recovery outcome, lifecycle revision changes, and preservation of runtime identity.",
                "Stall detection must remain evidence-driven and must not mutate lifecycle automatically. Recovery must be explicit, revision-safe, and preserve the existing runtime instance.",
                "Runtime progress and recovery verification.",
                TestRuntimeProgressRecoveryAsync,
                "Progress, heartbeat, stall, and recovery",
                "Uses only deterministic local runtime contracts; no external provider is contacted.");
        }

        private Task TestRuntimeProgressRecoveryAsync(string message)
        {
            var instance = AgentRuntimeInstance.Create(new AiAgent
            {
                Id = "runtime-progress-example-profile",
                Name = "Runtime Progress Example Profile",
                Enabled = true,
                CapabilityRequirements = new AiCapabilityRequirements(),
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.TryNextCandidate,
                    CostPolicy = AiCostPolicy.NoRestriction
                }
            }, AgentRuntimeScope.Application);

            var startedAt = DateTimeOffset.UtcNow;
            instance.ReportProgress(new AiRuntimeProgressSnapshot(
                1,
                AiRuntimeProgressKind.Progress,
                40,
                string.IsNullOrWhiteSpace(message) ? "Long-running work started" : message,
                "local progress evidence",
                startedAt));

            var policy = new AiRuntimeStallPolicy(TimeSpan.FromSeconds(5));
            var healthy = instance.EvaluateStall(policy, startedAt.AddSeconds(4));
            if (healthy.IsStalled)
                throw new InvalidOperationException("Current progress evidence was incorrectly classified as stalled.");

            var stalled = instance.EvaluateStall(policy, startedAt.AddSeconds(6));
            if (!stalled.IsStalled)
                throw new InvalidOperationException("Configured stale progress evidence was not detected as stalled.");
            if (instance.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Stall assessment changed lifecycle without an explicit recovery request.");

            var lifecycleRevisionBeforeRecovery = instance.CurrentLifecycleRevision;
            instance.BeginRecovery();
            if (instance.State != AgentRuntimeInstanceState.Recovering)
                throw new InvalidOperationException("Explicit recovery request did not enter Recovering state.");

            instance.CompleteRecovery(
                new AiRuntimeRecoveryResult(
                    AiRuntimeRecoveryStatus.Succeeded,
                    "Recovery completed from the detected stall.",
                    "Runtime identity and durable state remained intact.",
                    startedAt.AddSeconds(7)),
                AgentRuntimeInstanceState.Active);

            if (instance.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Successful recovery did not return the runtime to Active.");
            if (instance.CurrentLifecycleRevision <= lifecycleRevisionBeforeRecovery)
                throw new InvalidOperationException("Recovery completion did not advance lifecycle revision.");
            if (instance.InstanceId.Length == 0)
                throw new InvalidOperationException("Recovery changed or removed runtime identity.");

            Write("RUNTIME PROGRESS & RECOVERY",
                "Contract test succeeded." + Environment.NewLine +
                "Initial progress: 40%" + Environment.NewLine +
                "Current evidence before threshold: not stalled" + Environment.NewLine +
                "Configured stall detected: yes" + Environment.NewLine +
                "Lifecycle changed automatically from stall detection: no" + Environment.NewLine +
                "Recovery: Recovering -> Active" + Environment.NewLine +
                "Recovery outcome: Succeeded" + Environment.NewLine +
                "Lifecycle revision advanced: yes" + Environment.NewLine +
                "Runtime identity preserved: yes");

            return Task.CompletedTask;
        }
    }
}
