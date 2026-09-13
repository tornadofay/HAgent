using System;
using System.IO;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Storage.File;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddRuntimeHealthTab()
        {
            AddApiTab(
                "RUNTIME HEALTH",
                "Run runtime health test",
                "Verifies provider-neutral runtime health as a separate dimension from lifecycle: Unknown, Healthy, Degraded, Failed, bounded evidence, source metadata, slow-but-valid inference, and persisted restoration.",
                "Health should remain descriptive evidence, not lifecycle authorization. Degradation is transient, terminal failure is explicit, metadata is bounded, and valid slow inference remains healthy.",
                "Runtime health verification.",
                TestRuntimeHealthAsync,
                "Normalized runtime health and evidence",
                "Uses only the local runtime-state file store; no external provider is contacted.");
        }

        private async Task TestRuntimeHealthAsync(string message)
        {
            var profile = new AiAgent
            {
                Id = "runtime-health-profile-42",
                Name = "Runtime Health Test Profile",
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.TryNextCandidate,
                    CostPolicy = AiCostPolicy.NoRestriction
                },
                CapabilityRequirements = new AiCapabilityRequirements(),
                Enabled = true
            };
            var instance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Application);

            if (instance.Health.Status != AiRuntimeHealthStatus.Unknown)
                throw new InvalidOperationException("New runtimes must start with Unknown health.");

            var lifecycleRevision = instance.CurrentLifecycleRevision;
            instance.SetHealth(new AiRuntimeHealth(
                AiRuntimeHealthStatus.Degraded,
                AiRuntimeHealthSource.RecoveryResult,
                AiRuntimeHealthFailureKind.Transient,
                "Recovery is retryable.",
                string.IsNullOrWhiteSpace(message) ? "attempt=1" : message,
                DateTimeOffset.UtcNow));
            if (instance.Health.Status != AiRuntimeHealthStatus.Degraded ||
                instance.Health.FailureKind != AiRuntimeHealthFailureKind.Transient)
                throw new InvalidOperationException("Degraded health was not recorded as transient.");
            if (instance.CurrentLifecycleRevision != lifecycleRevision || instance.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Health mutation changed lifecycle authority.");

            var slowObservation = DateTimeOffset.UtcNow;
            instance.SetHealth(new AiRuntimeHealth(
                AiRuntimeHealthStatus.Healthy,
                AiRuntimeHealthSource.RuntimeObservation,
                AiRuntimeHealthFailureKind.None,
                "Inference completed successfully despite high latency.",
                "elapsed_ms=30000; response_valid=yes",
                slowObservation));
            if (instance.Health.Status != AiRuntimeHealthStatus.Healthy || instance.Health.FailureKind != AiRuntimeHealthFailureKind.None)
                throw new InvalidOperationException("Slow but valid inference was incorrectly classified as failed or degraded.");

            instance.SetHealth(new AiRuntimeHealth(
                AiRuntimeHealthStatus.Failed,
                AiRuntimeHealthSource.HostSignal,
                AiRuntimeHealthFailureKind.Terminal,
                "Host reported terminal runtime invalidation.",
                "signal=retire",
                DateTimeOffset.UtcNow));
            if (instance.Health.Status != AiRuntimeHealthStatus.Failed || instance.Health.FailureKind != AiRuntimeHealthFailureKind.Terminal)
                throw new InvalidOperationException("Terminal runtime failure was not recorded as terminal.");

            var record = AgentRuntimeStateRecord.FromInstance(instance, "example-host", "example-user", "example-workspace", "example-session");
            var path = Path.Combine(Path.GetTempPath(), "hagent-example-runtime-health-" + Guid.NewGuid().ToString("N") + ".jsonl");
            try
            {
                using (var store = new FileAgentRuntimeStateStore(path))
                {
                    await store.SaveAsync(record).ConfigureAwait(true);
                    var saved = await store.GetAsync(instance.InstanceId).ConfigureAwait(true);
                    if (saved == null || saved.Health.Status != AiRuntimeHealthStatus.Failed ||
                        saved.Health.FailureKind != AiRuntimeHealthFailureKind.Terminal ||
                        !string.Equals(saved.Health.Reason, instance.Health.Reason, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException("Persisted runtime health evidence did not round-trip.");
                    }

                    var restored = AgentRuntimeInstance.Restore(profile, saved);
                    if (restored.Health.Status != instance.Health.Status ||
                        restored.Health.Source != instance.Health.Source ||
                        restored.Health.FailureKind != instance.Health.FailureKind ||
                        !string.Equals(restored.Health.Reason, instance.Health.Reason, StringComparison.Ordinal) ||
                        !string.Equals(restored.Health.Evidence, instance.Health.Evidence, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException("Restored runtime health evidence did not match the persisted state.");
                    }
                }
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }

            Write("RUNTIME HEALTH",
                "Contract test succeeded." + Environment.NewLine +
                "Initial health: Unknown" + Environment.NewLine +
                "Degraded health: transient" + Environment.NewLine +
                "Lifecycle revision unchanged by health: " + instance.CurrentLifecycleRevision + Environment.NewLine +
                "Slow valid inference: Healthy" + Environment.NewLine +
                "Terminal failure: Failed" + Environment.NewLine +
                "Persisted health restored: yes" + Environment.NewLine +
                "Health source: " + instance.Health.Source + Environment.NewLine +
                "Health reason preserved: yes");
        }
    }
}
