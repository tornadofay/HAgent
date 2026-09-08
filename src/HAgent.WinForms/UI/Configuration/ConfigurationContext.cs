using System;
using System.Collections.Generic;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.WinForms.UI.Configuration
{
    internal sealed class ConfigurationContext
    {
        public ConfigurationContext(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IToolRegistry tools,
            IAiInterventionWorkflow interventionWorkflow = null,
            AiInterventionCoordinator interventionCoordinator = null,
            AgentIdentityContext currentIdentity = null)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
            Secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            Adapters = new List<IAiProviderAdapter>(adapters ?? new List<IAiProviderAdapter>()).AsReadOnly();
            Tools = tools ?? new InMemoryToolRegistry();
            InterventionWorkflow = interventionWorkflow ?? new InMemoryAiInterventionWorkflow();
            InterventionCoordinator = interventionCoordinator;
            CurrentIdentity = currentIdentity == null ? new AgentIdentityContext() : currentIdentity.Clone();
        }

        public IAiStore Store { get; private set; }
        public ISecretStore Secrets { get; private set; }
        public IReadOnlyList<IAiProviderAdapter> Adapters { get; private set; }
        public IToolRegistry Tools { get; private set; }
        public IAiInterventionWorkflow InterventionWorkflow { get; private set; }
        public AiInterventionCoordinator InterventionCoordinator { get; private set; }
        public AgentIdentityContext CurrentIdentity { get; private set; }
        public IReadOnlyList<AiProvider> Providers { get; set; } = new List<AiProvider>();
        public IReadOnlyList<AiAgent> Agents { get; set; } = new List<AiAgent>();
    }
}
