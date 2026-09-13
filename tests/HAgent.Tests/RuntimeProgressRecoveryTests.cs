using System;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class RuntimeProgressRecoveryTests
    {
        [Fact]
        public void RuntimeProgress_RejectsInvalidBoundsAndStaleSequence()
        {
            var instance = CreateInstance();
            var now = DateTimeOffset.UtcNow;

            Assert.Throws<ArgumentOutOfRangeException>(() => new AiRuntimeProgressSnapshot(0, AiRuntimeProgressKind.Progress, 10, "x", "", now).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new AiRuntimeProgressSnapshot(1, AiRuntimeProgressKind.Progress, 101, "x", "", now).Validate());

            instance.ReportProgress(new AiRuntimeProgressSnapshot(1, AiRuntimeProgressKind.Progress, 10, "started", "", now));
            Assert.Throws<InvalidOperationException>(() => instance.ReportProgress(new AiRuntimeProgressSnapshot(1, AiRuntimeProgressKind.Heartbeat, null, "", "", now.AddSeconds(1))));
            Assert.Equal(1L, instance.Progress.Sequence);
        }

        [Fact]
        public void RuntimeProgress_ReportsHeartbeatAndProgressAsDetachedSnapshots()
        {
            var instance = CreateInstance();
            var now = DateTimeOffset.UtcNow;
            instance.ReportProgress(new AiRuntimeProgressSnapshot(1, AiRuntimeProgressKind.Progress, 25, "running", "evidence", now));

            var snapshot = instance.Progress;
            Assert.NotNull(snapshot);
            Assert.Equal(AiRuntimeProgressKind.Progress, snapshot.Kind);
            Assert.Equal(25, snapshot.PercentComplete);
            Assert.Equal("running", snapshot.Detail);

            var secondRead = instance.Progress;
            Assert.NotSame(snapshot, secondRead);
        }

        [Fact]
        public void RuntimeProgress_StallDetectionRequiresConfiguredEvidence()
        {
            var instance = CreateInstance();
            var policy = new AiRuntimeStallPolicy(TimeSpan.FromSeconds(5));
            var now = DateTimeOffset.UtcNow;

            var noEvidence = instance.EvaluateStall(policy, now);
            Assert.False(noEvidence.IsStalled);

            instance.ReportProgress(new AiRuntimeProgressSnapshot(1, AiRuntimeProgressKind.Heartbeat, null, "", "heartbeat", now));
            var current = instance.EvaluateStall(policy, now.AddSeconds(4));
            Assert.False(current.IsStalled);

            var stalled = instance.EvaluateStall(policy, now.AddSeconds(6));
            Assert.True(stalled.IsStalled);
            Assert.Equal(1L, stalled.LastProgress.Sequence);
        }

        [Fact]
        public void RuntimeProgress_StallAssessmentDoesNotMutateLifecycle()
        {
            var instance = CreateInstance();
            var now = DateTimeOffset.UtcNow;
            instance.ReportProgress(new AiRuntimeProgressSnapshot(1, AiRuntimeProgressKind.Progress, 50, "long-running", "", now));

            var assessment = instance.EvaluateStall(new AiRuntimeStallPolicy(TimeSpan.FromSeconds(1)), now.AddSeconds(2));

            Assert.True(assessment.IsStalled);
            Assert.Equal(AgentRuntimeInstanceState.Active, instance.State);
            Assert.Equal(0L, instance.CurrentLifecycleRevision);
        }

        [Fact]
        public void RuntimeRecovery_SuccessCompletesToActiveWithNewLifecycleRevision()
        {
            var instance = CreateInstance();
            var originalId = instance.InstanceId;
            var originalRevision = instance.CurrentLifecycleRevision;

            instance.BeginRecovery();
            Assert.Equal(AgentRuntimeInstanceState.Recovering, instance.State);
            var recoveryRevision = instance.CurrentLifecycleRevision;

            instance.CompleteRecovery(
                new AiRuntimeRecoveryResult(
                    AiRuntimeRecoveryStatus.Succeeded,
                    "Recovery checks passed.",
                    "runtime-state preserved",
                    DateTimeOffset.UtcNow),
                AgentRuntimeInstanceState.Active);

            Assert.Equal(AgentRuntimeInstanceState.Active, instance.State);
            Assert.Equal(recoveryRevision + 1L, instance.CurrentLifecycleRevision);
            Assert.True(instance.CurrentLifecycleRevision > originalRevision);
            Assert.Equal(originalId, instance.InstanceId);
        }

        [Fact]
        public void RuntimeRecovery_FailureCannotReturnToActiveAndPreservesRuntimeIdentity()
        {
            var instance = CreateInstance();
            var originalId = instance.InstanceId;
            instance.BeginRecovery();

            var failed = new AiRuntimeRecoveryResult(
                AiRuntimeRecoveryStatus.Failed,
                "Recovery dependency remained unavailable.",
                "durable runtime state retained",
                DateTimeOffset.UtcNow);

            Assert.Throws<InvalidOperationException>(() => instance.CompleteRecovery(failed, AgentRuntimeInstanceState.Active));
            instance.CompleteRecovery(failed, AgentRuntimeInstanceState.Suspended);

            Assert.Equal(AgentRuntimeInstanceState.Suspended, instance.State);
            Assert.Equal(originalId, instance.InstanceId);
        }

        [Fact]
        public void RuntimeRecovery_FailedAndCancelledResultsRequireReasons()
        {
            var now = DateTimeOffset.UtcNow;
            Assert.Throws<ArgumentException>(() => new AiRuntimeRecoveryResult(AiRuntimeRecoveryStatus.Failed, "", "", now).Validate());
            Assert.Throws<ArgumentException>(() => new AiRuntimeRecoveryResult(AiRuntimeRecoveryStatus.Cancelled, "", "", now).Validate());

            new AiRuntimeRecoveryResult(AiRuntimeRecoveryStatus.Succeeded, "ok", "", now).Validate();
        }

        private static AgentRuntimeInstance CreateInstance()
        {
            var profile = new AiAgent
            {
                Id = "runtime-progress-profile",
                Name = "Runtime Progress Profile",
                Enabled = true,
                CapabilityRequirements = new AiCapabilityRequirements(),
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.TryNextCandidate,
                    CostPolicy = AiCostPolicy.NoRestriction
                }
            };
            return AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);
        }
    }
}
