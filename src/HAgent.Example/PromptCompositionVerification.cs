using System;
using System.Collections.Generic;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal static class PromptCompositionVerification
    {
        public static string Run()
        {
            var layers = new List<SystemPromptLayer>
            {
                new SystemPromptLayer("provider", "Provider", "Provider policy: do not disclose secrets.", 100),
                new SystemPromptLayer("agent", "Agent", "Agent role: analyze customer data.", 200),
                new SystemPromptLayer("runtime", "Runtime", "Runtime restriction: read-only access.", 300),
                new SystemPromptLayer("context", "Context", "Context restriction: use only the supplied customer context.", 400)
            };

            var composed = SystemPromptComposer.Compose(layers);
            var providerIndex = composed.IndexOf("Provider policy: do not disclose secrets.", StringComparison.Ordinal);
            var agentIndex = composed.IndexOf("Agent role: analyze customer data.", StringComparison.Ordinal);
            var runtimeIndex = composed.IndexOf("Runtime restriction: read-only access.", StringComparison.Ordinal);
            var contextIndex = composed.IndexOf("Context restriction: use only the supplied customer context.", StringComparison.Ordinal);

            if (providerIndex < 0 || agentIndex < 0 || runtimeIndex < 0 || contextIndex < 0)
                throw new InvalidOperationException("Prompt composition lost one or more system-prompt layers.");

            if (!(providerIndex < agentIndex && agentIndex < runtimeIndex && runtimeIndex < contextIndex))
                throw new InvalidOperationException("System-prompt layers were not composed in priority order.");

            return "Additive system-prompt composition verified: provider, agent, runtime, and context layers were all preserved in order.";
        }
    }
}
