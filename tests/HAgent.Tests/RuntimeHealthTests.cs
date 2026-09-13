using System;
using System.IO;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Storage.File;
using Xunit;

namespace HAgent.Tests
{
    public sealed class RuntimeHealthTests
    {
        [Fact]
        public void RuntimeHealth_DefaultIsUnknownAndDoesNotChangeLifecycleRevision()
        {
            var instance = CreateInstance();
            Assert.Equal(AiRuntimeHealthStatus.Unknown, instance.Health.Status);
            Assert.Equal(AgentRuntimeInstanceState.Active, instance.State);
            Assert.Equal(0L, instance.CurrentLifecycleRevision);

            instance.SetHealth(CreateHealthy());

            Assert.Equal(AiRuntimeHealthStatus.Healthy, instance.Health.Status);
            Assert.Equal(AgentRuntimeInstanceState.Active, instance.State);
            Assert.Equal(0L, instance.CurrentLifecycleRevision);
        }

        [Fact]
        public void RuntimeHealth_DegradedAndFailedRequireDistinctFailureKinds()
        {
            var instance = CreateInstance();

            instance.SetHealth(new AiRuntimeHealth(
                AiRuntimeHealthStatus.Degraded,
                AiRuntimeHealthSource.RecoveryResult,
                AiRuntimeHealthFailureKind.Transient,
                "Recovery is retryable.",
                "attempt=1",
                DateTimeOffset.UtcNow));

            Assert.Equal(AiRuntimeHealthStatus.Degraded, instance.Health.Status);
            Assert.Equal(AiRuntimeHealthFailureKind.Transient, instance.Health.FailureKind);

            Assert.Throws<ArgumentException>(() => new AiRuntimeHealth(
                AiRuntimeHealthStatus.Degraded,
                AiRuntimeHealthSource.RecoveryResult,
                AiRuntimeHealthFailureKind.Terminal,
                "invalid",
                string.Empty,
                DateTimeOffset.UtcNow).Validate());

            instance.SetHealth(new AiRuntimeHealth(
                AiRuntimeHealthStatus.Failed,
                AiRuntimeHealthSource.HostSignal,
                AiRuntimeHealthFailureKind.Terminal,
                "Runtime ownership was invalidated by the host.",
                "signal=retire",
                DateTimeOffset.UtcNow));

            Assert.Equal(AiRuntimeHealthStatus.Failed, instance.Health.Status);
            Assert.Equal(AiRuntimeHealthFailureKind.Terminal, instance.Health.FailureKind);
        }

        [Fact]
        public void RuntimeHealth_BoundsReasonAndEvidenceAndClonesSnapshots()
        {
            var reason = new string('r', AiRuntimeHealth.MaxReasonLength);
            var evidence = new string('e', AiRuntimeHealth.MaxEvidenceLength);
            var health = new AiRuntimeHealth(
                AiRuntimeHealthStatus.Degraded,
                AiRuntimeHealthSource.ExternalEvidence,
                AiRuntimeHealthFailureKind.Transient,
                reason,
                evidence,
                DateTimeOffset.UtcNow);
            var instance = CreateInstance();

            instance.SetHealth(health);
            var snapshot = instance.Health;

            Assert.Equal(reason, snapshot.Reason);
            Assert.Equal(evidence, snapshot.Evidence);
            Assert.NotSame(health, snapshot);

            Assert.Throws<ArgumentException>(() => new AiRuntimeHealth(
                AiRuntimeHealthStatus.Degraded,
                AiRuntimeHealthSource.ExternalEvidence,
                AiRuntimeHealthFailureKind.Transient,
                new string('r', AiRuntimeHealth.MaxReasonLength + 1),
                string.Empty,
                DateTimeOffset.UtcNow).Validate());
            Assert.Throws<ArgumentException>(() => new AiRuntimeHealth(
                AiRuntimeHealthStatus.Degraded,
                AiRuntimeHealthSource.ExternalEvidence,
                AiRuntimeHealthFailureKind.Transient,
                "bounded",
                new string('e', AiRuntimeHealth.MaxEvidenceLength + 1),
                DateTimeOffset.UtcNow).Validate());
        }

        [Fact]
        public void RuntimeHealth_SlowButValidInferenceRemainsHealthy()
        {
            var observedAt = DateTimeOffset.UtcNow;
            var instance = CreateInstance();

            instance.SetHealth(new AiRuntimeHealth(
                AiRuntimeHealthStatus.Healthy,
                AiRuntimeHealthSource.RuntimeObservation,
                AiRuntimeHealthFailureKind.None,
                "Inference completed successfully despite high latency.",
                "elapsed_ms=30000; response_valid=yes",
                observedAt));

            Assert.Equal(AiRuntimeHealthStatus.Healthy, instance.Health.Status);
            Assert.Equal(AiRuntimeHealthFailureKind.None, instance.Health.FailureKind);
            Assert.Equal(observedAt, instance.Health.ObservedAt);
        }

        [Fact]
        public void RuntimeHealth_InvalidCombinationsAreRejected()
        {
            Assert.Throws<ArgumentException>(() => new AiRuntimeHealth(
                AiRuntimeHealthStatus.Unknown,
                AiRuntimeHealthSource.RuntimeObservation,
                AiRuntimeHealthFailureKind.Terminal,
                "invalid",
                string.Empty,
                null).Validate());
            Assert.Throws<ArgumentException>(() => new AiRuntimeHealth(
                AiRuntimeHealthStatus.Failed,
                AiRuntimeHealthSource.RuntimeObservation,
                AiRuntimeHealthFailureKind.Terminal,
                string.Empty,
                string.Empty,
                DateTimeOffset.UtcNow).Validate());
        }

        [Fact]
        public async Task RuntimeHealth_RestoreAndFilePersistencePreserveHealthEvidence()
        {
            var path = Path.Combine(Path.GetTempPath(), "hagent-runtime-health-" + Guid.NewGuid().ToString("N") + ".jsonl");
            try
            {
                var profile = CreateProfile();
                var instance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Session);
                instance.SetHealth(new AiRuntimeHealth(
                    AiRuntimeHealthStatus.Degraded,
                    AiRuntimeHealthSource.RecoveryResult,
                    AiRuntimeHealthFailureKind.Transient,
                    "Recovery requires another attempt.",
                    "attempt=2",
                    DateTimeOffset.UtcNow));

                var record = AgentRuntimeStateRecord.FromInstance(instance, "host", "user", "workspace", "session");
                var restored = AgentRuntimeInstance.Restore(profile, record);
                Assert.Equal(instance.Health.Status, restored.Health.Status);
                Assert.Equal(instance.Health.Source, restored.Health.Source);
                Assert.Equal(instance.Health.FailureKind, restored.Health.FailureKind);
                Assert.Equal(instance.Health.Reason, restored.Health.Reason);
                Assert.Equal(instance.Health.Evidence, restored.Health.Evidence);

                using (var store = new FileAgentRuntimeStateStore(path))
                {
                    await store.SaveAsync(record).ConfigureAwait(false);
                    var saved = await store.GetAsync(instance.InstanceId).ConfigureAwait(false);
                    Assert.NotNull(saved);
                    Assert.Equal(instance.Health.Status, saved.Health.Status);
                    Assert.Equal(instance.Health.Reason, saved.Health.Reason);
                    Assert.Equal(instance.Health.Evidence, saved.Health.Evidence);
                }
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private static AgentRuntimeInstance CreateInstance()
        {
            return AgentRuntimeInstance.Create(CreateProfile(), AgentRuntimeScope.Task);
        }

        private static AiRuntimeHealth CreateHealthy()
        {
            return new AiRuntimeHealth(
                AiRuntimeHealthStatus.Healthy,
                AiRuntimeHealthSource.RuntimeObservation,
                AiRuntimeHealthFailureKind.None,
                "Runtime observation succeeded.",
                "response=valid",
                DateTimeOffset.UtcNow);
        }

        private static AiAgent CreateProfile()
        {
            return new AiAgent
            {
                Id = "runtime-health-profile",
                Name = "Runtime Health Profile",
                Enabled = true,
                CapabilityRequirements = new AiCapabilityRequirements(),
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.TryNextCandidate,
                    CostPolicy = AiCostPolicy.NoRestriction
                }
            };
        }
    }
}
