using System;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class RuntimeObservabilityTests
    {
        [Fact]
        public void RuntimeDiagnostics_CapturesDetachedCurrentState()
        {
            var instance = CreateInstance();
            instance.SetHealth(new AiRuntimeHealth(
                AiRuntimeHealthStatus.Degraded,
                AiRuntimeHealthSource.HostSignal,
                AiRuntimeHealthFailureKind.Transient,
                "host pressure",
                "diagnostic evidence",
                DateTimeOffset.UtcNow));
            instance.ReportProgress(new AiRuntimeProgressSnapshot(1, AiRuntimeProgressKind.Progress, 50, "half", "", DateTimeOffset.UtcNow));

            var diagnostics = AiRuntimeDiagnosticsService.Capture(instance);

            Assert.Equal(instance.InstanceId, diagnostics.RuntimeInstanceId);
            Assert.Equal(AgentRuntimeInstanceState.Active, diagnostics.LifecycleState);
            Assert.Equal(AiRuntimeHealthStatus.Degraded, diagnostics.Health.Status);
            Assert.Equal(50, diagnostics.Progress.PercentComplete);

            var originalHealth = diagnostics.Health;
            instance.SetHealth(new AiRuntimeHealth(AiRuntimeHealthStatus.Healthy, AiRuntimeHealthSource.RuntimeObservation, AiRuntimeHealthFailureKind.None, "ok", "", DateTimeOffset.UtcNow));
            Assert.Equal(AiRuntimeHealthStatus.Degraded, originalHealth.Status);
            Assert.Equal(AiRuntimeHealthStatus.Healthy, instance.Health.Status);
        }

        [Fact]
        public void RuntimeObservation_ContractCarriesLifecycleHealthProgressAndRecoveryEvidence()
        {
            var instance = CreateInstance();
            var progress = new AiRuntimeProgressSnapshot(2, AiRuntimeProgressKind.Heartbeat, null, "alive", "heartbeat", DateTimeOffset.UtcNow);
            var recovery = new AiRuntimeRecoveryResult(AiRuntimeRecoveryStatus.Succeeded, "recovered", "runtime state retained", DateTimeOffset.UtcNow);
            var health = new AiRuntimeHealth(AiRuntimeHealthStatus.Healthy, AiRuntimeHealthSource.RecoveryResult, AiRuntimeHealthFailureKind.None, "recovered", "checks passed", DateTimeOffset.UtcNow);
            instance.SetHealth(health);

            var observation = new AiRuntimeObservation(AiRuntimeObservationKind.RecoveryCompleted, instance, AgentRuntimeInstanceState.Recovering, null, progress, recovery, DateTimeOffset.UtcNow);
            var args = new AiRuntimeObservationEventArgs(observation);

            Assert.Same(observation, args.Observation);
            Assert.Equal(instance.InstanceId, observation.RuntimeInstanceId);
            Assert.Equal(AiRuntimeObservationKind.RecoveryCompleted, observation.Kind);
            Assert.Equal(AiRuntimeHealthStatus.Healthy, observation.Health.Status);
            Assert.Equal(2L, observation.Progress.Sequence);
            Assert.Equal(AiRuntimeRecoveryStatus.Succeeded, observation.Recovery.Status);
        }

        private static AgentRuntimeInstance CreateInstance()
        {
            return AgentRuntimeInstance.Create(new AiAgent
            {
                Id = "runtime-observability-profile",
                Name = "Runtime Observability Profile",
                Enabled = true,
                CapabilityRequirements = new AiCapabilityRequirements(),
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.TryNextCandidate,
                    CostPolicy = AiCostPolicy.NoRestriction
                }
            }, AgentRuntimeScope.Task);
        }
    }
}
