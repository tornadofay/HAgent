using System;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class GoalIntentionContractsTests
    {
        [Fact]
        public void Goal_SeparatesHostAuthorityFromAgentInferredProvenance()
        {
            var hostGoal = new AiGoal
            {
                Title = "Host goal",
                Provenance = new AiGoalProvenance
                {
                    Authority = AiGoalAuthority.HostSupplied,
                    Source = "HostApplication",
                    Evidence = "Operator supplied goal."
                }
            };
            var inferredGoal = new AiGoal
            {
                Title = "Inferred goal",
                Provenance = new AiGoalProvenance
                {
                    Authority = AiGoalAuthority.AgentInferred,
                    Source = "Execution observation",
                    SourceExecutionId = "execution-1"
                }
            };

            hostGoal.Validate();
            inferredGoal.Validate();

            Assert.NotEqual(hostGoal.Id, inferredGoal.Id);
            Assert.Equal(AiGoalAuthority.HostSupplied, hostGoal.Provenance.Authority);
            Assert.Equal(AiGoalAuthority.AgentInferred, inferredGoal.Provenance.Authority);
        }

        [Fact]
        public void Intention_RequiresDistinctGoalIdentityAndCapturesAdoptionMetadata()
        {
            var created = DateTimeOffset.UtcNow;
            var intention = new AiIntention
            {
                GoalId = "goal-1",
                Description = "Pursue the host goal",
                Priority = AiGoalPriority.High,
                CreatedAt = created,
                UpdatedAt = created,
                AdoptedAt = created.AddSeconds(1),
                Revision = 1L,
                Status = AiIntentionStatus.Adopted
            };

            intention.Validate();

            Assert.NotEqual(intention.Id, intention.GoalId);
            Assert.Equal(AiIntentionStatus.Adopted, intention.Status);
            Assert.Equal(1L, intention.Revision);
            Assert.Equal(created.AddSeconds(1), intention.AdoptedAt.Value);
        }

        [Fact]
        public void IntentionStatusChange_RequiresReasonAndPreservesRevisionBoundary()
        {
            var changedAt = DateTimeOffset.UtcNow;
            var change = new AiIntentionStatusChange(
                "intention-1",
                2L,
                AiIntentionStatus.Adopted,
                AiIntentionStatus.Suspended,
                "Host requested temporary suspension.",
                "operator-request-42",
                changedAt,
                AiGoalAuthority.HostSupplied);

            Assert.False(string.IsNullOrWhiteSpace(change.Id));
            Assert.Equal("intention-1", change.IntentionId);
            Assert.Equal(2L, change.Revision);
            Assert.Equal(AiIntentionStatus.Adopted, change.PreviousStatus);
            Assert.Equal(AiIntentionStatus.Suspended, change.NewStatus);
            Assert.Equal("Host requested temporary suspension.", change.Reason);
            Assert.Equal(AiGoalAuthority.HostSupplied, change.Authority);
            Assert.Equal(changedAt, change.ChangedAt);
        }

        [Fact]
        public void Intention_RejectsInvalidAdoptionTimestamp()
        {
            var intention = new AiIntention
            {
                GoalId = "goal-1",
                Description = "Invalid timestamp",
                AdoptedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
            };

            intention.CreatedAt = DateTimeOffset.UtcNow;
            intention.UpdatedAt = intention.CreatedAt;

            Assert.Throws<ArgumentException>(() => intention.Validate());
        }
    }
}
