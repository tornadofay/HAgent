using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            registry.Register(new DelegateAgentTool(new AiTool
            {
                Id = "ui.project_data",
                Name = "Project UI data",
                Description = "Read a bounded allow-listed field projection from a bound DataGridView data source. No SQL, expressions, arbitrary property paths, writes, or sorting are supported.",
                Category = "UI",
                Type = AiToolType.UI,
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"controlId\":{\"type\":\"string\"},\"fields\":{\"type\":\"array\",\"minItems\":1,\"maxItems\":50,\"items\":{\"type\":\"string\"}},\"skip\":{\"type\":\"integer\",\"minimum\":0,\"maximum\":100000},\"take\":{\"type\":\"integer\",\"minimum\":1,\"maximum\":1000}},\"required\":[\"controlId\",\"fields\"],\"additionalProperties\":false}",
                IsBuiltIn = true,
                Enabled = true
            }, async execution =>
            {
                Require(context, delegate(UiAutomationPermissions permissions) { return permissions.ReadData; }, "Reading UI data is disabled by the current permission policy.");
                object controlValue;
                execution.Arguments.TryGetValue("controlId", out controlValue);
                var controlId = Convert.ToString(controlValue);
                if (string.IsNullOrWhiteSpace(controlId))
                    throw new ArgumentException("controlId is required.", nameof(controlId));

                object fieldsValue;
                execution.Arguments.TryGetValue("fields", out fieldsValue);
                var fields = new List<string>();
                var enumerable = fieldsValue as System.Collections.IEnumerable;
                if (enumerable != null && !(fieldsValue is string))
                {
                    foreach (var item in enumerable)
                        fields.Add(Convert.ToString(item));
                }

                object skipValue;
                execution.Arguments.TryGetValue("skip", out skipValue);
                object takeValue;
                execution.Arguments.TryGetValue("take", out takeValue);
                var request = new DataProjectionRequest
                {
                    Fields = fields,
                    Skip = skipValue == null ? 0 : Convert.ToInt32(skipValue),
                    Take = takeValue == null ? 100 : Convert.ToInt32(takeValue)
                };

                var result = await context.ProjectDataAsync(controlId, request, execution.CancellationToken).ConfigureAwait(false);
                return ToolExecutionResult.Success(JsonConvert.SerializeObject(result));
            }));
        }
    }
}
