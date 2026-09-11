using System;
using System.Collections.Generic;
using System.IO;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.Storage.File;

namespace HAgent.WinForms.UI.Configuration
{
    internal sealed class ConfigurationContext
    {
        public ConfigurationContext(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IToolRegistry tools,
            IAiLearningCandidateStore learningCandidates = null,
            AgentIdentityContext reviewerIdentity = null,
            AiLearningPromotionService learningPromotion = null)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
            Secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            Adapters = new List<IAiProviderAdapter>(adapters ?? new List<IAiProviderAdapter>()).AsReadOnly();
            Tools = tools ?? new InMemoryToolRegistry();
            LearningPromotion = learningPromotion;

            if (learningCandidates != null)
            {
                LearningCandidates = learningCandidates;
            }
            else
            {
                var root = new HAgentStorageOptions
                {
                    ApplicationName = "HAgent",
                    RootPath = AppContext.BaseDirectory
                }.GetEffectiveRootPath();
                LearningCandidates = new FileLearningCandidateStore(Path.Combine(root, "learning", "candidates.jsonl"));
            }

            ReviewerIdentity = reviewerIdentity == null
                ? new AgentIdentityContext(userId: AISettings.DefaultSystemAdminUserId)
                : reviewerIdentity.Clone();

            if (string.IsNullOrWhiteSpace(ReviewerIdentity.UserId))
                ReviewerIdentity = new AgentIdentityContext(userId: AISettings.DefaultSystemAdminUserId);
        }

        public IAiStore Store { get; private set; }
        public ISecretStore Secrets { get; private set; }
        public IReadOnlyList<IAiProviderAdapter> Adapters { get; private set; }
        public IToolRegistry Tools { get; private set; }
        public IAiLearningCandidateStore LearningCandidates { get; private set; }
        public AgentIdentityContext ReviewerIdentity { get; private set; }
        public AiLearningPromotionService LearningPromotion { get; private set; }
        public IReadOnlyList<AiProvider> Providers { get; set; } = new List<AiProvider>();
        public IReadOnlyList<AiAgent> Agents { get; set; } = new List<AiAgent>();
    }
}
