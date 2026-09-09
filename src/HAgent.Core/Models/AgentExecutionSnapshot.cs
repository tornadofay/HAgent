using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HAgent.Models
{
    public sealed class AgentExecutionSnapshot
    {
        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers)
            : this(agent, providers, null, null, null, null, null, null, null)
        {
        }

        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers, AgentRuntimeOverrides overrides)
            : this(agent, providers, overrides, null, null, null, null, null, null)
        {
        }

        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers, AgentRuntimeOverrides overrides, IReadOnlyDictionary<string, string> hostContext)
            : this(agent, providers, overrides, hostContext, null, null, null, null, null)
        {
        }

        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers, AgentRuntimeOverrides overrides, IReadOnlyDictionary<string, string> hostContext, AgentIdentityContext identity)
            : this(agent, providers, overrides, hostContext, identity, null, null, null, null)
        {
        }

        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers, AgentRuntimeOverrides overrides, IReadOnlyDictionary<string, string> hostContext, AgentIdentityContext identity, AiPolicySet effectivePolicy)
            : this(agent, providers, overrides, hostContext, identity, effectivePolicy, null, null, null)
        {
        }

        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers, AgentRuntimeOverrides overrides, IReadOnlyDictionary<string, string> hostContext, AgentIdentityContext identity, AiPolicySet effectivePolicy, AiInstructionSnapshot instructionSnapshot)
            : this(agent, providers, overrides, hostContext, identity, effectivePolicy, instructionSnapshot, null, null)
        {
        }

        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers, AgentRuntimeOverrides overrides, IReadOnlyDictionary<string, string> hostContext, AgentIdentityContext identity, AiPolicySet effectivePolicy, AiInstructionSnapshot instructionSnapshot, ContextSnapshot contextSnapshot)
            : this(agent, providers, overrides, hostContext, identity, effectivePolicy, instructionSnapshot, contextSnapshot, null)
        {
        }

        public AgentExecutionSnapshot(AiAgent agent, IReadOnlyList<AiProvider> providers, AgentRuntimeOverrides overrides, IReadOnlyDictionary<string, string> hostContext, AgentIdentityContext identity, AiPolicySet effectivePolicy, AiInstructionSnapshot instructionSnapshot, ContextSnapshot contextSnapshot, AiSkillExecutionSnapshot skillSnapshot)
        {
            var sourceAgent = agent ?? throw new ArgumentNullException(nameof(agent));
            Agent = CloneAgent(sourceAgent, overrides);
            Providers = CloneProviders(providers ?? throw new ArgumentNullException(nameof(providers)));
            RuntimeContext = CloneContext(overrides == null ? null : overrides.Context);
            HostContext = CloneContext(hostContext);
            Identity = identity == null ? new AgentIdentityContext() : identity.Clone();
            EffectivePolicy = effectivePolicy == null ? new AiPolicySet() : effectivePolicy.Clone();
            EffectiveResourceCapabilities = AiResourceCapabilitySnapshot.Resolve(sourceAgent.ResourceCapabilities, overrides == null ? null : overrides.ResourceCapabilityOverrides);
            InstructionSnapshot = instructionSnapshot == null ? new AiInstructionSnapshot(null, null) : instructionSnapshot.Clone();
            Context = contextSnapshot == null ? null : contextSnapshot.Clone();
            Skills = skillSnapshot == null ? new AiSkillExecutionSnapshot(new AiSkillBinding[0]) : skillSnapshot.Clone();
            EffectivePolicy.Validate();
            EffectiveResourceCapabilities.Validate();
            CreatedAt = DateTimeOffset.UtcNow;
            Identity.Validate();
        }

        public AiAgent Agent { get; private set; }
        public IReadOnlyList<AiProvider> Providers { get; private set; }
        public IReadOnlyDictionary<string, string> RuntimeContext { get; private set; }
        public IReadOnlyDictionary<string, string> HostContext { get; private set; }
        public AgentIdentityContext Identity { get; private set; }
        public AiPolicySet EffectivePolicy { get; private set; }
        public AiResourceCapabilitySnapshot EffectiveResourceCapabilities { get; private set; }
        public AiInstructionSnapshot InstructionSnapshot { get; private set; }
        public AiSkillExecutionSnapshot Skills { get; private set; }

        /// <summary>
        /// Optional execution-owned provider-neutral context snapshot captured from the canonical request.
        /// </summary>
        public ContextSnapshot Context { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        internal void CaptureInstructionSnapshot(AiInstructionSnapshot instructionSnapshot)
        {
            if (instructionSnapshot == null) throw new ArgumentNullException(nameof(instructionSnapshot));
            InstructionSnapshot = instructionSnapshot.Clone();
        }

        internal void CaptureSkillSnapshot(AiSkillExecutionSnapshot skillSnapshot)
        {
            if (skillSnapshot == null) throw new ArgumentNullException(nameof(skillSnapshot));
            Skills = skillSnapshot.Clone();
        }

        private static AiAgent CloneAgent(AiAgent source, AgentRuntimeOverrides overrides)
        {
            var clone = new AiAgent
            {
                Id = source.Id,
                Name = source.Name,
                SystemPrompt = source.SystemPrompt,
                UseProviderSystemPrompt = source.UseProviderSystemPrompt,
                Temperature = source.Temperature,
                MaxOutputTokens = source.MaxOutputTokens,
                Enabled = source.Enabled,
                ToolIds = source.ToolIds == null ? new List<string>() : new List<string>(source.ToolIds),
                ExecutionSelection = source.ExecutionSelection == null ? new AiExecutionSelectionPolicy() : source.ExecutionSelection.Clone(),
                CapabilityRequirements = source.CapabilityRequirements == null ? new AiCapabilityRequirements() : source.CapabilityRequirements.Clone(),
                ResourceCapabilities = source.ResourceCapabilities == null ? new AiResourceCapabilityPolicy() : source.ResourceCapabilities.Clone(),
                Skills = source.Skills == null ? new AiSkillSet { Name = "Default skills" } : source.Skills.Clone()
            };

            if (overrides == null) return clone;
            if (overrides.Temperature.HasValue) clone.Temperature = overrides.Temperature;
            if (overrides.MaxOutputTokens.HasValue) clone.MaxOutputTokens = overrides.MaxOutputTokens;
            if (!string.IsNullOrWhiteSpace(overrides.SystemPrompt)) clone.SystemPrompt = overrides.SystemPrompt;
            return clone;
        }

        private static IReadOnlyDictionary<string, string> CloneContext(IEnumerable<KeyValuePair<string, string>> source)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (source != null) foreach (var item in source) result[item.Key] = item.Value;
            return new ReadOnlyDictionary<string, string>(result);
        }

        private static IReadOnlyList<AiProvider> CloneProviders(IReadOnlyList<AiProvider> source)
        {
            var result = new List<AiProvider>();
            foreach (var provider in source)
            {
                if (provider == null) continue;
                result.Add(new AiProvider { Id = provider.Id, Name = provider.Name, Kind = provider.Kind, BaseUrl = provider.BaseUrl, DefaultModel = provider.DefaultModel, DefaultSystemPrompt = provider.DefaultSystemPrompt, SecretId = provider.SecretId, Enabled = provider.Enabled });
            }
            return result.AsReadOnly();
        }
    }
}
