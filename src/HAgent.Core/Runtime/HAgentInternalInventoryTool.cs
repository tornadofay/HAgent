using System;
using System.Text;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Read-only trusted tool for inspecting bounded HAgent-owned inventory metadata.
    /// It never exposes provider secrets or arbitrary storage records and has no write operation.
    /// </summary>
    public sealed class HAgentInternalInventoryTool : IAgentTool
    {
        private readonly IAiStore _aiStore;
        private readonly IToolStore _toolStore;

        public HAgentInternalInventoryTool(IAiStore aiStore, IToolStore toolStore)
        {
            if (aiStore == null) throw new ArgumentNullException(nameof(aiStore));
            if (toolStore == null) throw new ArgumentNullException(nameof(toolStore));

            _aiStore = aiStore;
            _toolStore = toolStore;
            Definition = CreateDefinition();
        }

        public AiTool Definition { get; private set; }

        public async Task<ToolExecutionResult> ExecuteAsync(ToolExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.CancellationToken.ThrowIfCancellationRequested();

            var providers = await _aiStore.GetProvidersAsync(context.CancellationToken).ConfigureAwait(false);
            var agents = await _aiStore.GetAgentsAsync(context.CancellationToken).ConfigureAwait(false);
            var tools = await _toolStore.GetToolsAsync(context.CancellationToken).ConfigureAwait(false);

            var result = new StringBuilder();
            result.AppendLine("HAgent internal inventory");
            result.AppendLine("Providers: " + providers.Count);
            foreach (var provider in providers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                result.AppendLine("Provider | " + Safe(provider.Id) + " | " + Safe(provider.Name) + " | Enabled=" + provider.Enabled);
            }

            result.AppendLine("Agents: " + agents.Count);
            foreach (var agent in agents)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                result.AppendLine("Agent | " + Safe(agent.Id) + " | " + Safe(agent.Name) + " | Provider=" + Safe(agent.ProviderId) + " | Enabled=" + agent.Enabled);
            }

            result.AppendLine("Tools: " + tools.Count);
            foreach (var tool in tools)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                result.AppendLine("Tool | " + Safe(tool.Id) + " | " + Safe(tool.Name) + " | Type=" + tool.Type + " | Enabled=" + tool.Enabled);
            }

            return ToolExecutionResult.Success(result.ToString().TrimEnd());
        }

        private static AiTool CreateDefinition()
        {
            return new AiTool
            {
                Id = "hagent.internal.inventory",
                Name = "HAgent Internal Inventory",
                Description = "Read-only inventory of HAgent-owned providers, agents, and registered tool definitions. Does not expose secrets or modify data.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}",
                Category = "BuiltIn",
                Type = AiToolType.BuiltIn,
                IsBuiltIn = true,
                Enabled = true
            };
        }

        private static string Safe(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\r", " ").Replace("\n", " ");
        }
    }
}
