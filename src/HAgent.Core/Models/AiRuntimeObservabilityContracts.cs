using System;

namespace HAgent.Models
{
    public enum AiRuntimeObservationKind { LifecycleChanged, HealthChanged, ProgressChanged, RecoveryCompleted }

    public sealed class AiRuntimeObservation
    {
        public AiRuntimeObservation(AiRuntimeObservationKind kind, AgentRuntimeInstance instance, AgentRuntimeInstanceState previousLifecycleState, AiRuntimeHealth previousHealth, AiRuntimeProgressSnapshot progress, AiRuntimeRecoveryResult recovery, DateTimeOffset observedAt)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            Kind = kind; RuntimeInstanceId = instance.InstanceId; ProfileId = instance.ProfileId;
            LifecycleState = instance.State; PreviousLifecycleState = previousLifecycleState; LifecycleRevision = instance.CurrentLifecycleRevision;
            Health = instance.Health; PreviousHealth = previousHealth == null ? null : previousHealth.Clone();
            Progress = progress == null ? null : progress.Clone(); Recovery = recovery == null ? null : recovery.Clone(); ObservedAt = observedAt;
        }
        public AiRuntimeObservationKind Kind { get; private set; }
        public string RuntimeInstanceId { get; private set; }
        public string ProfileId { get; private set; }
        public AgentRuntimeInstanceState LifecycleState { get; private set; }
        public AgentRuntimeInstanceState PreviousLifecycleState { get; private set; }
        public long LifecycleRevision { get; private set; }
        public AiRuntimeHealth Health { get; private set; }
        public AiRuntimeHealth PreviousHealth { get; private set; }
        public AiRuntimeProgressSnapshot Progress { get; private set; }
        public AiRuntimeRecoveryResult Recovery { get; private set; }
        public DateTimeOffset ObservedAt { get; private set; }
    }

    public sealed class AiRuntimeDiagnosticsSnapshot
    {
        public AiRuntimeDiagnosticsSnapshot(AgentRuntimeInstance instance, DateTimeOffset capturedAt)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            RuntimeInstanceId = instance.InstanceId; ProfileId = instance.ProfileId; LifecycleState = instance.State;
            LifecycleRevision = instance.CurrentLifecycleRevision; ExecutionRevision = instance.CurrentExecutionRevision;
            Health = instance.Health; Progress = instance.Progress; CapturedAt = capturedAt;
        }
        public string RuntimeInstanceId { get; private set; }
        public string ProfileId { get; private set; }
        public AgentRuntimeInstanceState LifecycleState { get; private set; }
        public long LifecycleRevision { get; private set; }
        public long ExecutionRevision { get; private set; }
        public AiRuntimeHealth Health { get; private set; }
        public AiRuntimeProgressSnapshot Progress { get; private set; }
        public DateTimeOffset CapturedAt { get; private set; }
    }
}
