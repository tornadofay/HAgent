using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HAgent.Models
{
    public sealed class AgentExecutionSnapshot
    {
        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers)
            : this(agent, providers, null, null)
        {
        }

        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers, AgentRuntimeOverrides overrides)
            : this(agent, providers, overrides, null)
        {
        }

        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers, AgentRuntimeOverrides overrides, IReadOnlyDictionary<string, string> hostContext)
        {
            Agent = CloneAgent(agent ?? throw new ArgumentNullException(nameof(agent)), overrides);
            Providers = CloneProviders(providers ?? throw new ArgumentNullException(nameof(providers)));
            RuntimeContext = CloneContext(overrides == null ? null : overrides.Context);
            HostContext = CloneContext(hostContext);
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public AiAgent Agent { get; private set; }
        public IReadOnlyList<AiProvider> Providers { get; private set; }
        public IReadOnlyDictionary<string, string> RuntimeContext { get; private set; }
        public IReadOnlyDictionary<string, string> HostContext { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private static AiAgent CloneAgent(AiAgent source, AgentRuntimeOverrides overrides)
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

            if (overrides == null) return clone;

            if (!string.IsNullOrWhiteSpace(overrides.ProviderId))
            {
                clone.ProviderId = overrides.ProviderId;
                clone.ProviderIds = new List<string> { overrides.ProviderId };
            }
            if (!string.IsNullOrWhiteSpace(overrides.Model)) clone.Model = overrides.Model;
            if (overrides.Temperature.HasValue) clone.Temperature = overrides.Temperature;
            if (overrides.MaxOutputTokens.HasValue) clone.MaxOutputTokens = overrides.MaxOutputTokens;
            if (!string.IsNullOrWhiteSpace(overrides.SystemPrompt)) clone.SystemPrompt = overrides.SystemPrompt;

            return clone;
        }

        private static IReadOnlyDictionary<string, string> CloneContext(IEnumerable<KeyValuePair<string, string>> source)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (source != null)
            {
                foreach (var item in source)
                    result[item.Key] = item.Value;
            }
            return new ReadOnlyDictionary<string, string>(result);
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
