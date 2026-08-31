using System;
using System.Collections.Generic;
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
        private const int DefaultMaxItems = 50;
        private const int MaximumMaxItems = 100;

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

            var maxItems = ResolveMaxItems(context.Arguments);
            var providers = await _aiStore.GetProvidersAsync(context.CancellationToken).ConfigureAwait(false);
            var agents = await _aiStore.GetAgentsAsync(context.CancellationToken).ConfigureAwait(false);
            var tools = await _toolStore.GetToolsAsync(context.CancellationToken).ConfigureAwait(false);

            var result = new StringBuilder();
            result.AppendLine("HAgent internal inventory");
            result.AppendLine("Max items per category: " + maxItems);
            result.AppendLine("Providers: " + providers.Count);
            AppendProviders(result, providers, maxItems, context);
            result.AppendLine("Agents: " + agents.Count);
            AppendAgents(result, agents, maxItems, context);
            result.AppendLine("Tools: " + tools.Count);
            AppendTools(result, tools, maxItems, context);

            return ToolExecutionResult.Success(result.ToString().TrimEnd());
        }

        private static int ResolveMaxItems(IReadOnlyDictionary<string, object> arguments)
        {
            object rawValue;
            if (arguments == null || !arguments.TryGetValue("maxItems", out rawValue) || rawValue == null)
                return DefaultMaxItems;

            int value;
            try
            {
                value = Convert.ToInt32(rawValue);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("maxItems must be an integer between 1 and " + MaximumMaxItems + ".", nameof(arguments), ex);
            }

            if (value < 1 || value > MaximumMaxItems)
                throw new ArgumentOutOfRangeException(nameof(arguments), "maxItems must be between 1 and " + MaximumMaxItems + ".");

            return value;
        }

        private static void AppendProviders(StringBuilder result, IReadOnlyList<AiProvider> providers, int maxItems, ToolExecutionContext context)
        {
            var count = Math.Min(providers.Count, maxItems);
            for (var i = 0; i < count; i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var provider = providers[i];
                result.AppendLine("Provider | " + Safe(provider.Id) + " | " + Safe(provider.Name) + " | Enabled=" + provider.Enabled);
            }
            AppendTruncated(result, providers.Count, count);
        }

        private static void AppendAgents(StringBuilder result, IReadOnlyList<AiAgent> agents, int maxItems, ToolExecutionContext context)
        {
            var count = Math.Min(agents.Count, maxItems);
            for (var i = 0; i < count; i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var agent = agents[i];
                result.AppendLine("Agent | " + Safe(agent.Id) + " | " + Safe(agent.Name) + " | Provider=" + Safe(agent.ProviderId) + " | Enabled=" + agent.Enabled);
            }
            AppendTruncated(result, agents.Count, count);
        }

        private static void AppendTools(StringBuilder result, IReadOnlyList<AiTool> tools, int maxItems, ToolExecutionContext context)
        {
            var count = Math.Min(tools.Count, maxItems);
            for (var i = 0; i < count; i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var tool = tools[i];
                result.AppendLine("Tool | " + Safe(tool.Id) + " | " + Safe(tool.Name) + " | Type=" + tool.Type + " | Enabled=" + tool.Enabled);
            }
            AppendTruncated(result, tools.Count, count);
        }

        private static void AppendTruncated(StringBuilder result, int total, int returned)
        {
            if (returned < total)
                result.AppendLine("Returned: " + returned + " of " + total + " (bounded by maxItems).");
        }

        private static AiTool CreateDefinition()
        {
            return new AiTool
            {
                Id = "hagent.internal.inventory",
                Name = "HAgent Internal Inventory",
                Description = "Read-only bounded inventory of HAgent-owned providers, agents, and registered tool definitions. Does not expose secrets or modify data.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"maxItems\":{\"type\":\"integer\",\"minimum\":1,\"maximum\":100}},\"additionalProperties\":false}",
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
