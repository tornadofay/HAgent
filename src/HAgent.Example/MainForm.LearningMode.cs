using System;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddLearningModeTab()
        {
            AddApiTab(
                "Learning Mode",
                "Run Learning Mode test",
                "Exercises provider-neutral LearningMode profile state, runtime override precedence, candidate/review semantics, and separation from resource capability enablement.",
                "Disabled must produce no candidates, SuggestOnly must require review, AutomaticWithPolicy must require policy-driven promotion, and FullyAutomatic is the only mode permitting unreviewed promotion.",
                "No AI request is sent by this example.",
                TestLearningModeAsync,
                "Learning governance boundary",
                "Learning Mode controls the learning lifecycle policy. It is deliberately separate from Skills, Knowledge, Memory, or other capability enablement.");
        }

        private Task TestLearningModeAsync(string unused)
        {
            var profile = new AiAgent
            {
                Id = "learning-mode-agent-42",
                Name = "Learning Mode Agent",
                LearningMode = AiLearningMode.SuggestOnly
            };
            profile.ResourceCapabilities.Set(AiMemoryResourceTypes.Memory, AiResourceCapabilityState.Disabled);

            var runtime = new AgentRuntimeOverrides
            {
                LearningMode = AiLearningMode.AutomaticWithPolicy
            };
            var snapshot = new AgentExecutionSnapshot(profile, new AiProvider[0], runtime);

            if (profile.LearningMode != AiLearningMode.SuggestOnly)
                throw new InvalidOperationException("Profile LearningMode was not retained.");
            if (snapshot.LearningMode != AiLearningMode.AutomaticWithPolicy)
                throw new InvalidOperationException("Runtime LearningMode override was not captured.");
            if (snapshot.EffectiveResourceCapabilities.GetState(AiMemoryResourceTypes.Memory) != AiResourceCapabilityState.Disabled)
                throw new InvalidOperationException("Learning Mode incorrectly changed Memory capability state.");
            if (!AiLearningModePolicy.ProducesCandidates(AiLearningMode.SuggestOnly) ||
                !AiLearningModePolicy.RequiresReview(AiLearningMode.SuggestOnly))
                throw new InvalidOperationException("SuggestOnly semantics are incorrect.");
            if (!AiLearningModePolicy.AllowsPolicyDrivenPromotion(AiLearningMode.AutomaticWithPolicy) ||
                AiLearningModePolicy.AllowsUnreviewedPromotion(AiLearningMode.AutomaticWithPolicy))
                throw new InvalidOperationException("AutomaticWithPolicy semantics are incorrect.");
            if (!AiLearningModePolicy.AllowsUnreviewedPromotion(AiLearningMode.FullyAutomatic))
                throw new InvalidOperationException("FullyAutomatic semantics are incorrect.");

            runtime.LearningMode = AiLearningMode.Disabled;
            profile.LearningMode = AiLearningMode.Disabled;
            if (snapshot.LearningMode != AiLearningMode.AutomaticWithPolicy)
                throw new InvalidOperationException("LearningMode execution snapshot changed after source mutation.");

            Write(
                "LEARNING MODE",
                "Contract test succeeded." + Environment.NewLine +
                "Profile LearningMode: SuggestOnly." + Environment.NewLine +
                "Runtime LearningMode override: AutomaticWithPolicy." + Environment.NewLine +
                "Execution snapshot isolation: verified." + Environment.NewLine +
                "Disabled produces candidates: no." + Environment.NewLine +
                "SuggestOnly requires review: yes." + Environment.NewLine +
                "AutomaticWithPolicy requires policy: yes." + Environment.NewLine +
                "FullyAutomatic permits unreviewed promotion: yes." + Environment.NewLine +
                "Learning Mode independent from Memory capability: verified.");

            return Task.CompletedTask;
        }
    }
}
