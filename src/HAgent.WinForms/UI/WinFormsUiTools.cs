using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.WinForms.UI
{
    public static class WinFormsUiTools
    {
        public static void RegisterDefaultTools(IToolRegistry registry, IUiContext context)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (context == null) throw new ArgumentNullException(nameof(context));

            registry.Register(new DelegateAgentTool(new AiTool
            {
                Id = "ui.inspect",
                Name = "Inspect UI",
                Description = "Inspect the attached WinForms form or a named control without changing it.",
                Category = "UI",
                Type = AiToolType.UI,
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"controlId\":{\"type\":\"string\"}},\"additionalProperties\":false}",
                IsBuiltIn = true,
                Enabled = true
            }, async execution =>
            {
                object value;
                execution.Arguments.TryGetValue("controlId", out value);
                var id = value as string;
                var snapshot = await context.InspectAsync(string.IsNullOrWhiteSpace(id) ? null : id, execution.CancellationToken).ConfigureAwait(false);
                return ToolExecutionResult.Success(snapshot);
            }));

            registry.Register(new DelegateAgentTool(new AiTool
            {
                Id = "ui.read_control",
                Name = "Read UI control",
                Description = "Read the current value of a named WinForms control without changing it.",
                Category = "UI",
                Type = AiToolType.UI,
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"controlId\":{\"type\":\"string\"}},\"required\":[\"controlId\"],\"additionalProperties\":false}",
                IsBuiltIn = true,
                Enabled = true
            }, async execution =>
            {
                object value;
                execution.Arguments.TryGetValue("controlId", out value);
                var id = Convert.ToString(value);
                var result = await context.ReadControlAsync(id, execution.CancellationToken).ConfigureAwait(false);
                return ToolExecutionResult.Success(result);
            }));

            registry.Register(new DelegateAgentTool(new AiTool
            {
                Id = "ui.read_data",
                Name = "Read UI data",
                Description = "Read bounded tabular data from a DataGridView using its bound data source when available.",
                Category = "UI",
                Type = AiToolType.UI,
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"controlId\":{\"type\":\"string\"},\"maxRows\":{\"type\":\"integer\",\"minimum\":1,\"maximum\":1000}},\"required\":[\"controlId\"],\"additionalProperties\":false}",
                IsBuiltIn = true,
                Enabled = true
            }, async execution =>
            {
                object value;
                execution.Arguments.TryGetValue("controlId", out value);
                var id = Convert.ToString(value);
                object rowsValue;
                execution.Arguments.TryGetValue("maxRows", out rowsValue);
                var maxRows = rowsValue == null ? 100 : Math.Max(1, Math.Min(1000, Convert.ToInt32(rowsValue)));
                var result = await context.ReadDataAsync(id, maxRows, execution.CancellationToken).ConfigureAwait(false);
                return ToolExecutionResult.Success(result);
            }));
        }
    }
}
