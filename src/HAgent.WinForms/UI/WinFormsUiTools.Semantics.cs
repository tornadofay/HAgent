using System;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Newtonsoft.Json;

namespace HAgent.WinForms.UI
{
    public static partial class WinFormsUiTools
    {
        public static void RegisterSemanticDiscoveryTool(IToolRegistry registry, WinFormsUiContext context)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (context == null) throw new ArgumentNullException(nameof(context));

            registry.Register(new DelegateAgentTool(new AiTool
            {
                Id = "ui.discover",
                Name = "Discover UI semantics",
                Description = "Discover semantic descriptions of controls, roles, bindings, data roles, and permitted capabilities on the attached WinForms UI root.",
                Category = "UI",
                Type = AiToolType.UI,
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}",
                IsBuiltIn = true,
                Enabled = true
            }, async execution =>
            {
                RequireDiscovery(context);
                var descriptors = await context.DiscoverSemanticsAsync(execution.CancellationToken).ConfigureAwait(false);
                return ToolExecutionResult.Success(JsonConvert.SerializeObject(descriptors));
            }));
        }

        private static void RequireDiscovery(WinFormsUiContext context)
        {
            if (context.Permissions == null || !context.Permissions.AutomaticDiscovery)
                throw new InvalidOperationException("Automatic UI discovery is disabled by the current permission policy.");
        }
    }
}
