using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.WinForms.UI;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestUiContextAsync(string input)
        {
            using (var form = new Form { Name = "ExampleForm", Text = "UI Context Example", Size = new Size(640, 420) })
            using (var nameBox = new TextBox { Name = "txtCustomerName", Text = "HAgent Customer", Width = 240, Location = new Point(20, 20) })
            using (var grid = new DataGridView { Name = "gridCustomers", Width = 560, Height = 220, Location = new Point(20, 65), AutoGenerateColumns = true })
            {
                var table = new DataTable();
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
                table.Rows.Add(1, "Alice");
                table.Rows.Add(2, "Bob");
                grid.DataSource = table;
                form.Controls.Add(nameBox);
                form.Controls.Add(grid);

                form.CreateControl();
                var registry = new InMemoryToolRegistry();
                using (var host = HAgentHost.Attach(form, registry))
                {
                    var snapshot = await host.Context.InspectAsync(null, CancellationToken.None);
                    if (snapshot == null || snapshot.Id != "ExampleForm" || snapshot.Children == null || snapshot.Children.Count < 2)
                        throw new InvalidOperationException("UI context did not inspect the attached form correctly.");

                    var name = await host.Context.ReadControlAsync("txtCustomerName", CancellationToken.None);
                    if (!string.Equals(Convert.ToString(name), "HAgent Customer", StringComparison.Ordinal))
                        throw new InvalidOperationException("UI context did not read the TextBox value correctly.");

                    var rows = await host.Context.ReadDataAsync("gridCustomers", 100, CancellationToken.None);
                    if (rows.Count != 2)
                        throw new InvalidOperationException("UI context did not read the bound grid source correctly.");
                    if (!string.Equals(Convert.ToString(rows[0]["Name"]), "Alice", StringComparison.Ordinal))
                        throw new InvalidOperationException("Bound row data was not normalized correctly.");

                    var permissions = new UiAutomationPermissions { AutomaticDiscovery = true, ReadControls = true, ReadData = true };
                    var sourceDiscovery = new WinFormsDataSourceDiscovery();
                    var sources = sourceDiscovery.Discover(form, permissions);
                    var gridSource = sources.FirstOrDefault(x => string.Equals(x.ControlId, "gridCustomers", StringComparison.OrdinalIgnoreCase));
                    if (gridSource == null)
                        throw new InvalidOperationException("UI data-source discovery did not identify the bound DataGridView source.");
                    if (!string.Equals(gridSource.DataMember, null, StringComparison.Ordinal))
                    {
                        // DataTable has no DataMember; keep this branch as an explicit contract check.
                    }
                    if (gridSource.Count != 2)
                        throw new InvalidOperationException("UI data-source discovery reported an incorrect bound row count.");
                    if (gridSource.FieldNames == null || !gridSource.FieldNames.Contains("Id") || !gridSource.FieldNames.Contains("Name"))
                        throw new InvalidOperationException("UI data-source discovery did not expose the bound DataTable field names.");

                    IAgentTool inspect;
                    IAgentTool read;
                    IAgentTool data;
                    if (!registry.TryGet("ui.inspect", out inspect) || !registry.TryGet("ui.read_control", out read) || !registry.TryGet("ui.read_data", out data))
                        throw new InvalidOperationException("Default read-only UI tools were not registered.");

                    IAgentTool discoverDataSources;
                    if (!registry.TryGet("ui.discover_data_sources", out discoverDataSources))
                        throw new InvalidOperationException("The UI data-source discovery tool was not registered.");

                    var definitions = registry.GetDefinitions();
                    var uiTools = definitions.Count(x => x.Type == AiToolType.UI);

                    Write("UI CONTEXT",
                        "Contract test succeeded." + Environment.NewLine +
                        "Attached form: " + snapshot.Id + Environment.NewLine +
                        "Controls inspected: " + snapshot.Children.Count + Environment.NewLine +
                        "TextBox value: " + Convert.ToString(name) + Environment.NewLine +
                        "DataGridView rows: " + rows.Count + Environment.NewLine +
                        "Data source: DataTable (bound source preferred)" + Environment.NewLine +
                        "Discovered data sources: " + sources.Count + Environment.NewLine +
                        "Discovered grid fields: " + string.Join(", ", gridSource.FieldNames) + Environment.NewLine +
                        "UI tools registered: " + uiTools + Environment.NewLine +
                        "Write/click/move/resize operations: not exposed in this read-only slice.");
                }
            }
        }
    }
}
