using System;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class LearningModeTests
    {
        [Fact]
        public void LearningMode_DefaultsToDisabledAndIsIndependentFromCapabilities()
        {
            var agent = new AiAgent();
            Assert.Equal(AiLearningMode.Disabled, agent.LearningMode);
            Assert.NotNull(agent.ResourceCapabilities);
            Assert.Equal(AiResourceCapabilityState.Inherit, agent.ResourceCapabilities.GetState("memory"));
        }

        [Fact]
        public void LearningMode_ProducesCandidatesOnlyWhenNotDisabled()
        {
            Assert.False(AiLearningModePolicy.ProducesCandidates(AiLearningMode.Disabled));
            Assert.True(AiLearningModePolicy.ProducesCandidates(AiLearningMode.SuggestOnly));
            Assert.True(AiLearningModePolicy.ProducesCandidates(AiLearningMode.AutomaticWithPolicy));
            Assert.True(AiLearningModePolicy.ProducesCandidates(AiLearningMode.FullyAutomatic));
        }

        [Fact]
        public void LearningMode_MapsReviewAndPromotionSemanticsDeterministically()
        {
            Assert.True(AiLearningModePolicy.RequiresReview(AiLearningMode.SuggestOnly));
            Assert.False(AiLearningModePolicy.RequiresReview(AiLearningMode.AutomaticWithPolicy));
            Assert.False(AiLearningModePolicy.AllowsUnreviewedPromotion(AiLearningMode.AutomaticWithPolicy));
            Assert.True(AiLearningModePolicy.AllowsPolicyDrivenPromotion(AiLearningMode.AutomaticWithPolicy));
            Assert.True(AiLearningModePolicy.AllowsUnreviewedPromotion(AiLearningMode.FullyAutomatic));
            Assert.True(AiLearningModePolicy.AllowsPolicyDrivenPromotion(AiLearningMode.FullyAutomatic));
        }

        [Fact]
        public void InvalidLearningModeIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AiLearningModePolicy.Validate((AiLearningMode)99));
        }

        [Fact]
        public void RuntimeLearningModeOverrideIsCapturedWithoutMutatingProfile()
        {
            var profile = new AiAgent { LearningMode = AiLearningMode.SuggestOnly };
            var overrides = new AgentRuntimeOverrides { LearningMode = AiLearningMode.FullyAutomatic };
            var snapshot = new AgentExecutionSnapshot(profile, new AiProvider[0], overrides);

            Assert.Equal(AiLearningMode.SuggestOnly, profile.LearningMode);
            Assert.Equal(AiLearningMode.FullyAutomatic, snapshot.LearningMode);
            Assert.Equal(AiLearningMode.FullyAutomatic, snapshot.Agent.LearningMode);

            overrides.LearningMode = AiLearningMode.Disabled;
            profile.LearningMode = AiLearningMode.Disabled;
            Assert.Equal(AiLearningMode.FullyAutomatic, snapshot.LearningMode);
        }

        [Fact]
        public void PromotionRequestRejectsUnknownLearningModeText()
        {
            var request = new AiLearningPromotionRequest
            {
                CandidateId = "candidate-42",
                CandidateType = AiLearningCandidateType.Memory,
                ProposedScope = "Agent",
                ConfidenceBand = "high",
                EvidenceState = "present",
                ProvenanceState = "preserved",
                ContradictionState = "clear",
                RetentionClass = "normal",
                LearningMode = "invalid-mode"
            };

            Assert.Throws<ArgumentException>(() => request.Validate());
        }
    }
}
