using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class RuntimeObservationPublisherTests
    {
        [Fact]
        public async Task PublishAsync_MapsObservationToRuntimeScopedEvent()
        {
            var instance = AgentRuntimeInstance.Create(CreateProfile(), AgentRuntimeScope.Task);
            var observation = new AiRuntimeObservation(
                AiRuntimeObservationKind.HealthChanged,
                instance,
                AgentRuntimeInstanceState.Active,
                AiRuntimeHealth.CreateUnknown(),
                null,
                null,
                DateTimeOffset.UtcNow);
            var dispatcher = new RecordingDispatcher();

            var result = await AiRuntimeObservationPublisher.PublishAsync(dispatcher, observation, CancellationToken.None).ConfigureAwait(false);

            Assert.True(result.Accepted);
            Assert.NotNull(dispatcher.Envelope);
            Assert.Equal(EventSource.Runtime, dispatcher.Envelope.Source);
            Assert.Equal(EventScope.Runtime, dispatcher.Envelope.Scope);
            Assert.Equal(instance.InstanceId, dispatcher.Envelope.ScopeId);
            Assert.Equal("runtime.health.changed", dispatcher.Envelope.Type);
        }

        private static AiAgent CreateProfile()
        {
            return new AiAgent
            {
                Id = "runtime-observation-profile",
                Name = "Runtime Observation Profile",
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

        private sealed class RecordingDispatcher : IEventDispatcher
        {
            public EventEnvelope Envelope { get; private set; }
            public Task<EventPublishResult> PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                Envelope = envelope.Clone();
                return Task.FromResult(new EventPublishResult(EventPublishStatus.Accepted, envelope.Id));
            }
            public IEventSubscription Subscribe(EventSubscription subscription) { throw new NotSupportedException(); }
            public void Dispose() { }
        }
    }
}
