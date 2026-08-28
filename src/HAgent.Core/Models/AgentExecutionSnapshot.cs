using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class AgentExecutionSnapshot
    {
        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers)
        {
            Agent = CloneAgent(agent ?? throw new ArgumentNullException(nameof(agent)));
            Providers = CloneProviders(providers ?? throw new ArgumentNullException(nameof(providers)));
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public AiAgent Agent { get; private set; }
        public IReadOnlyList<AiProvider> Providers { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private static AiAgent CloneAgent(AiAgent source)
        {
            var clone = new AiAgent
            {
                Id = source.Id,
                Name = source.Name,
                ProviderId = source.ProviderId,
                Model = source.Model,
                SystemPrompt = source.SystemPrompt,
                UseProviderSystemPrompt = source.UseProviderSystemPrompt,
                Temperature = source.Temperature,
                MaxOutputTokens = source.MaxOutputTokens,
                Enabled = source.Enabled,
                ProviderIds = source.ProviderIds == null ? new List<string>() : new List<string>(source.ProviderIds),
                ToolIds = source.ToolIds == null ? new List<string>() : new List<string>(source.ToolIds)
            };
            return clone;
        }

        private static IReadOnlyList<AiProvider> CloneProviders(IReadOnlyList<AiProvider> source)
        {
            var result = new List<AiProvider>();
            foreach (var provider in source)
            {
                if (provider == null) continue;
                result.Add(new AiProvider
                {
                    Id = provider.Id,
                    Name = provider.Name,
                    Kind = provider.Kind,
                    BaseUrl = provider.BaseUrl,
                    DefaultModel = provider.DefaultModel,
                    DefaultSystemPrompt = provider.DefaultSystemPrompt,
                    SecretId = provider.SecretId,
                    Enabled = provider.Enabled
                });
            }
            return result.AsReadOnly();
        }
    }
}
