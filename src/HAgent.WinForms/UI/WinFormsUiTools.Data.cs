using System;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Newtonsoft.Json;

namespace HAgent.WinForms.UI
{
    public static partial class WinFormsUiTools
    {
        private static void RegisterDataSourceDiscoveryTool(IToolRegistry registry, WinFormsUiContext context)
        {
            registry.Register(new DelegateAgentTool(new AiTool
            {
                Id = "ui.discover_data_sources",
                Name = "Discover UI data sources",
                Description = "Discover bound data sources used by the attached WinForms controls without materializing their data.",
                Category = "UI",
                Type = AiToolType.UI,
                InputSchemaJson = "{\"type\":\"object\",\"additionalProperties\":false}",
                IsBuiltIn = true,
                Enabled = true
            }, async execution =>
            {
                Require(context, delegate(UiAutomationPermissions permissions) { return permissions.AutomaticDiscovery && permissions.ReadData; }, "Automatic data-source discovery is disabled by the current permission policy.");
                var discovery = new WinFormsDataSourceDiscovery();
                var sources = discovery.Discover(context.RootControl, context.Permissions);
                return ToolExecutionResult.Success(JsonConvert.SerializeObject(sources));
            }));
        }
    }
}
