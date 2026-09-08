using System;
using System.Collections.Generic;
using System.Linq;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Compatibility-shaped facade over the canonical provider-neutral instruction composer.
    /// Lower layers remain additive and are never treated as authorization overrides.
    /// </summary>
    public static class SystemPromptComposer
    {
        public static string Compose(IEnumerable<SystemPromptLayer> layers)
        {
            var sources = new List<AiInstructionSource>();
            foreach (var layer in layers ?? Enumerable.Empty<SystemPromptLayer>())
            {
                if (layer == null || string.IsNullOrWhiteSpace(layer.Text))
                    continue;

                sources.Add(new AiInstructionSource
                {
                    Id = string.IsNullOrWhiteSpace(layer.Id) ? Guid.NewGuid().ToString("N") : layer.Id,
                    Name = layer.Name,
                    SourceType = ResolveSourceType(layer.Id),
                    Authority = ResolveAuthority(layer.Id),
                    TrustLevel = AiInstructionTrustLevel.HAgentTrusted,
                    Scope = new AiInstructionScope(),
                    Lifecycle = AiInstructionLifecycleState.Active,
                    Priority = layer.Priority,
                    ConflictKey = string.Empty,
                    Version = "1",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    Content = layer.Text,
                    Provenance = new AiInstructionProvenance
                    {
                        SourceKind = "system-prompt-layer",
                        SourceId = layer.Id,
                        SourceVersion = "1",
                        CapturedAt = DateTimeOffset.UtcNow
                    }
                });
            }

            return AiInstructionComposer.Compose(sources).ComposedText;
        }

        public static SystemPromptLayer Create(string id, string name, string text, int priority)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Prompt layer text is required.", nameof(text));

            return new SystemPromptLayer(id, name, text, priority);
        }

        private static AiInstructionSourceType ResolveSourceType(string id)
        {
            switch ((id ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "PROVIDER": return AiInstructionSourceType.SystemPolicy;
                case "AGENT": return AiInstructionSourceType.Agent;
                case "RUNTIME": return AiInstructionSourceType.RuntimeContext;
                case "CONTEXT": return AiInstructionSourceType.HostContext;
                default: return AiInstructionSourceType.SystemPolicy;
            }
        }

        private static AiInstructionAuthority ResolveAuthority(string id)
        {
            switch ((id ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "AGENT": return AiInstructionAuthority.Agent;
                case "RUNTIME": return AiInstructionAuthority.Runtime;
                case "CONTEXT": return AiInstructionAuthority.Runtime;
                default: return AiInstructionAuthority.SystemPolicy;
            }
        }
    }
}
