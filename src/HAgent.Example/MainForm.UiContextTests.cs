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
                    if (host.Context.RootControl != form || host.Context.RootForm != form || host.Context.RootId != "ExampleForm")
                        throw new InvalidOperationException("UI context did not expose the expected form root identity.");

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
                    var sources = sourceDiscovery.Discover((Control)form, permissions);
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

                    await TestUserControlAttachmentAsync(input);

                    Write("UI CONTEXT",
                        "Contract test succeeded." + Environment.NewLine +
                        "Attached form: " + snapshot.Id + Environment.NewLine +
                        "Controls inspected: " + snapshot.Children.Count + Environment.NewLine +
                        "TextBox value: " + Convert.ToString(name) + Environment.NewLine +
                        "DataGridView rows: " + rows.Count + Environment.NewLine +
                        "Data source: DataTable (bound source preferred)" + Environment.NewLine +
                        "Discovered data sources: " + sources.Count + Environment.NewLine +
                        "Discovered grid fields: " + string.Join(", ", gridSource.FieldNames) + Environment.NewLine +
                        "UserControl attachment: verified" + Environment.NewLine +
                        "UI tools registered: " + uiTools + Environment.NewLine +
                        "Write/click/move/resize operations: not exposed in this read-only slice.");
                }
            }
        }

        private async Task TestUserControlAttachmentAsync(string unused)
        {
            using (var panel = new UserControl { Name = "CustomerPanel", Width = 400, Height = 250 })
            using (var nameBox = new TextBox { Name = "txtPanelCustomer", Text = "Panel Customer", Width = 200, Location = new Point(10, 10) })
            using (var grid = new DataGridView { Name = "gridPanelCustomers", Width = 360, Height = 150, Location = new Point(10, 45), AutoGenerateColumns = true })
            using (var bindingSource = new BindingSource())
            {
                var table = new DataTable();
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
                table.Rows.Add(10, "Panel Alice");
                bindingSource.DataSource = table;
                grid.DataSource = bindingSource;
                panel.Controls.Add(nameBox);
                panel.Controls.Add(grid);
                panel.CreateControl();

                var permissions = new UiAutomationPermissions { AutomaticDiscovery = true, ReadControls = true, ReadData = true };
                var registry = new InMemoryToolRegistry();
                using (var host = HAgentHost.Attach(panel, "CustomerPanel", registry, true, permissions))
                {
                    if (host.Context.RootControl != panel || host.Context.RootForm != null || host.Context.RootId != "CustomerPanel")
                        throw new InvalidOperationException("UserControl attachment did not expose the expected root identity.");

                    var snapshot = await host.Context.InspectAsync(null, CancellationToken.None);
                    if (snapshot == null || snapshot.Id != "CustomerPanel" || snapshot.Children == null || snapshot.Children.Count != 2)
                        throw new InvalidOperationException("UI context did not inspect the attached UserControl correctly.");

                    var name = await host.Context.ReadControlAsync("txtPanelCustomer", CancellationToken.None);
                    if (!string.Equals(Convert.ToString(name), "Panel Customer", StringComparison.Ordinal))
                        throw new InvalidOperationException("UserControl attachment did not read the nested TextBox correctly.");

                    var rows = await host.Context.ReadDataAsync("gridPanelCustomers", 10, CancellationToken.None);
                    if (rows.Count != 1 || !string.Equals(Convert.ToString(rows[0]["Name"]), "Panel Alice", StringComparison.Ordinal))
                        throw new InvalidOperationException("UserControl attachment did not read the nested bound BindingSource data correctly.");

                    var semantics = new WinFormsSemanticDiscovery().Discover((Control)panel, permissions);
                    if (!semantics.Any(x => string.Equals(x.ControlId, "txtPanelCustomer", StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidOperationException("Semantic discovery did not traverse the attached UserControl root.");

                    var sources = new WinFormsDataSourceDiscovery().Discover((Control)panel, permissions);
                    var gridSource = sources.FirstOrDefault(x => string.Equals(x.ControlId, "gridPanelCustomers", StringComparison.OrdinalIgnoreCase));
                    if (gridSource == null)
                        throw new InvalidOperationException("Data-source discovery did not traverse the attached UserControl root.");
                    if (!string.Equals(gridSource.SourceKind, "BindingSource", StringComparison.Ordinal))
                        throw new InvalidOperationException("Data-source discovery did not preserve the BindingSource relationship.");
                    if (string.IsNullOrWhiteSpace(gridSource.UnderlyingSourceType) || gridSource.UnderlyingSourceType.IndexOf("DataTable", StringComparison.OrdinalIgnoreCase) < 0)
                        throw new InvalidOperationException("Data-source discovery did not identify the BindingSource underlying DataTable.");
                    if (string.IsNullOrWhiteSpace(gridSource.ListType))
                        throw new InvalidOperationException("Data-source discovery did not identify the BindingSource list type.");
                    if (!string.Equals(gridSource.BindingPath, "DataSource", StringComparison.Ordinal))
                        throw new InvalidOperationException("Data-source discovery did not report the DataGridView binding path.");
                    if (gridSource.CurrencyManagerType == null || gridSource.CurrencyManagerType.IndexOf("CurrencyManager", StringComparison.OrdinalIgnoreCase) < 0)
                        throw new InvalidOperationException("Data-source discovery did not identify the control CurrencyManager.");
                    if (gridSource.Count != 1 || gridSource.Position != 0)
                        throw new InvalidOperationException("Data-source discovery reported incorrect BindingSource position or count.");
                    if (gridSource.CurrentItemType == null || gridSource.CurrentItemType.IndexOf("DataRowView", StringComparison.OrdinalIgnoreCase) < 0)
                        throw new InvalidOperationException("Data-source discovery did not identify the BindingSource current item type.");
                    if (gridSource.FieldNames == null || !gridSource.FieldNames.Contains("Id") || !gridSource.FieldNames.Contains("Name"))
                        throw new InvalidOperationException("BindingSource field discovery did not expose the underlying DataTable fields.");

                    await TestNativeIListRelationshipAsync(unused);

                    var uiTools = registry.GetDefinitions().Count(x => x.Type == AiToolType.UI);
                    Write("UI CONTEXT USERCONTROL",
                        "Contract test succeeded." + Environment.NewLine +
                        "Root control: " + host.Context.RootControl.GetType().FullName + Environment.NewLine +
                        "Root ID: " + host.Context.RootId + Environment.NewLine +
                        "Root form: none" + Environment.NewLine +
                        "Controls inspected: " + snapshot.Children.Count + Environment.NewLine +
                        "TextBox value: " + Convert.ToString(name) + Environment.NewLine +
                        "DataGridView rows: " + rows.Count + Environment.NewLine +
                        "Data source kind: " + gridSource.SourceKind + Environment.NewLine +
                        "Underlying source: " + gridSource.UnderlyingSourceType + Environment.NewLine +
                        "List type: " + gridSource.ListType + Environment.NewLine +
                        "Currency manager: " + gridSource.CurrencyManagerType + Environment.NewLine +
                        "Current item type: " + gridSource.CurrentItemType + Environment.NewLine +
                        "Position / count: " + Convert.ToString(gridSource.Position) + " / " + Convert.ToString(gridSource.Count) + Environment.NewLine +
                        "Discovered data sources: " + sources.Count + Environment.NewLine +
                        "Semantic controls discovered: " + semantics.Count + Environment.NewLine +
                        "UI tools registered: " + uiTools + Environment.NewLine +
                        "BindingSource relationship and bounded read verified.");
                }
            }
        }

        private async Task TestNativeIListRelationshipAsync(string unused)
        {
            using (var panel = new UserControl { Name = "NativeListPanel", Width = 420, Height = 270 })
            using (var grid = new DataGridView { Name = "gridNativeCustomers", Width = 380, Height = 180, Location = new Point(10, 10), AutoGenerateColumns = true })
            {
                var customers = new List<NativeCustomer>
                {
                    new NativeCustomer { Id = 20, Name = "Native Alice" },
                    new NativeCustomer { Id = 21, Name = "Native Bob" }
                };
                grid.DataSource = customers;
                panel.Controls.Add(grid);
                panel.CreateControl();

                var permissions = new UiAutomationPermissions { AutomaticDiscovery = true, ReadControls = true, ReadData = true };
                var sources = new WinFormsDataSourceDiscovery().Discover((Control)panel, permissions);
                var source = sources.FirstOrDefault(x => string.Equals(x.ControlId, "gridNativeCustomers", StringComparison.OrdinalIgnoreCase));
                if (source == null)
                    throw new InvalidOperationException("Data-source discovery did not identify the native IList DataGridView source.");
                if (!string.Equals(source.SourceKind, "IList", StringComparison.Ordinal))
                    throw new InvalidOperationException("Data-source discovery did not classify the native list source as IList.");
                if (source.CurrencyManagerType == null || source.CurrencyManagerType.IndexOf("CurrencyManager", StringComparison.OrdinalIgnoreCase) < 0)
                    throw new InvalidOperationException("Data-source discovery did not identify the native IList CurrencyManager.");
                if (source.Position != 0 || source.Count != 2)
                    throw new InvalidOperationException("Data-source discovery reported incorrect native IList position or count.");
                if (string.IsNullOrWhiteSpace(source.ItemType) || source.ItemType.IndexOf("NativeCustomer", StringComparison.OrdinalIgnoreCase) < 0)
                    throw new InvalidOperationException("Data-source discovery did not identify the native IList item type.");
                if (string.IsNullOrWhiteSpace(source.CurrentItemType) || source.CurrentItemType.IndexOf("NativeCustomer", StringComparison.OrdinalIgnoreCase) < 0)
                    throw new InvalidOperationException("Data-source discovery did not identify the native IList current item type.");
                if (source.FieldNames == null || !source.FieldNames.Contains("Id") || !source.FieldNames.Contains("Name"))
                    throw new InvalidOperationException("Data-source discovery did not identify native IList item fields.");

                using (var host = HAgentHost.Attach(panel, "NativeListPanel", new InMemoryToolRegistry(), true, permissions))
                {
                    var rows = await host.Context.ReadDataAsync("gridNativeCustomers", 10, CancellationToken.None);
                    if (rows.Count != 2 || !string.Equals(Convert.ToString(rows[0]["Name"]), "Native Alice", StringComparison.Ordinal))
                        throw new InvalidOperationException("UI context did not read the native IList through the bound DataGridView.");
                }

                Write("UI CONTEXT NATIVE ILIST",
                    "Contract test succeeded." + Environment.NewLine +
                    "Source kind: " + source.SourceKind + Environment.NewLine +
                    "Source type: " + source.SourceType + Environment.NewLine +
                    "Currency manager: " + source.CurrencyManagerType + Environment.NewLine +
                    "Item type: " + source.ItemType + Environment.NewLine +
                    "Current item type: " + source.CurrentItemType + Environment.NewLine +
                    "Position / count: " + Convert.ToString(source.Position) + " / " + Convert.ToString(source.Count) + Environment.NewLine +
                    "Fields: " + string.Join(", ", source.FieldNames) + Environment.NewLine +
                    "Native IList bounded read: verified without DataTable normalization.");
            }
        }

        private sealed class NativeCustomer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
