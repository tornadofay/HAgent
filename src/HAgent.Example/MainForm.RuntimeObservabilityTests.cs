using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddRuntimeObservabilityTab()
        {
            AddApiTab(
                "RUNTIME OBSERVABILITY",
                "Run runtime observability test",
                "Verifies bounded runtime diagnostics and adaptation of lifecycle, health, progress, and recovery observations into the existing event envelope boundary.",
                "Diagnostics should be detached, observation metadata should preserve the runtime identity and revision, and the event publisher should produce a Runtime-scoped event without granting authorization.",
                "Runtime observability verification.",
                TestRuntimeObservabilityAsync,
                "Runtime diagnostics and event-boundary adaptation",
                "Uses only a local in-memory event dispatcher stub; no external provider or network call is used.");
        }

        private async Task TestRuntimeObservabilityAsync(string message)
        {
            var profile = new AiAgent
            {
                Id = "runtime-observability-profile-42",
                Name = "Runtime Observability Profile",
                Enabled = true,
                CapabilityRequirements = new AiCapabilityRequirements(),
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.TryNextCandidate,
                    CostPolicy = AiCostPolicy.NoRestriction
                }
            };
            var instance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);
            var before = AiRuntimeDiagnosticsService.Capture(instance);

            instance.SetHealth(new AiRuntimeHealth(
                AiRuntimeHealthStatus.Degraded,
                AiRuntimeHealthSource.HostSignal,
                AiRuntimeHealthFailureKind.Transient,
                "host pressure",
                "bounded diagnostic evidence",
                DateTimeOffset.UtcNow));
            instance.ReportProgress(new AiRuntimeProgressSnapshot(
                1,
                AiRuntimeProgressKind.Progress,
                40,
                string.IsNullOrWhiteSpace(message) ? "running" : message,
                "progress evidence",
                DateTimeOffset.UtcNow));

            var after = AiRuntimeDiagnosticsService.Capture(instance);
            if (before.Health.Status != AiRuntimeHealthStatus.Unknown)
                throw new InvalidOperationException("Initial diagnostic health was not Unknown.");
            if (after.Health.Status != AiRuntimeHealthStatus.Degraded || after.Progress == null || after.Progress.PercentComplete != 40)
                throw new InvalidOperationException("Runtime diagnostics did not capture current health and progress.");
            if (before.Health.Status != AiRuntimeHealthStatus.Unknown)
                throw new InvalidOperationException("Diagnostics snapshot was not detached.");

            var observation = new AiRuntimeObservation(
                AiRuntimeObservationKind.HealthChanged,
                instance,
                instance.State,
                before.Health,
                after.Progress,
                null,
                DateTimeOffset.UtcNow);
            var dispatcher = new RecordingEventDispatcher();
            var published = await AiRuntimeObservationPublisher.PublishAsync(dispatcher, observation, CancellationToken.None).ConfigureAwait(true);

            if (!published.Accepted || dispatcher.LastEnvelope == null)
                throw new InvalidOperationException("Runtime observation was not accepted by the event boundary.");
            if (dispatcher.LastEnvelope.Source != EventSource.Runtime || dispatcher.LastEnvelope.Scope != EventScope.Runtime)
                throw new InvalidOperationException("Runtime observation did not preserve Runtime source/scope.");
            if (!string.Equals(dispatcher.LastEnvelope.ScopeId, instance.InstanceId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime observation did not preserve runtime scope identity.");

            Write("RUNTIME OBSERVABILITY",
                "Contract test succeeded." + Environment.NewLine +
                "Initial diagnostic health: " + before.Health.Status + Environment.NewLine +
                "Current health: " + after.Health.Status + Environment.NewLine +
                "Progress captured: " + after.Progress.PercentComplete + "%" + Environment.NewLine +
                "Lifecycle revision: " + after.LifecycleRevision + Environment.NewLine +
                "Diagnostics detached: yes" + Environment.NewLine +
                "Observation event source: " + dispatcher.LastEnvelope.Source + Environment.NewLine +
                "Observation event scope: " + dispatcher.LastEnvelope.Scope + Environment.NewLine +
                "Observation event accepted: " + published.Accepted);
        }

        private sealed class RecordingEventDispatcher : IEventDispatcher
        {
            public EventEnvelope LastEnvelope { get; private set; }
            public Task<EventPublishResult> PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                LastEnvelope = envelope == null ? null : envelope.Clone();
                return Task.FromResult(new EventPublishResult(
                    envelope == null ? EventPublishStatus.Expired : EventPublishStatus.Accepted,
                    envelope == null ? string.Empty : envelope.Id));
            }
            public IEventSubscription Subscribe(EventSubscription subscription) { throw new NotSupportedException(); }
            public void Dispose() { }
        }
    }
}
