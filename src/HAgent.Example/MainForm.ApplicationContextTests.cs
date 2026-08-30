using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Runtime;
using HAgent.WinForms.UI;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestHyperControlAdapterAsync(string unused)
        {
            using (var panel = new UserControl { Name = "HyperControlPanel", Width = 460, Height = 180 })
            using (var control = new HyperLikeTextBox
            {
                Name = "txtCustomerName",
                Text = "Adapter Customer",
                DbFieldName = "CustomerName",
                DisplayName = "Customer Name",
                TitleEn = "Customer Name",
                IsRequired = true,
                IsSearchField = true
            })
            {
                control.Location = new Point(10, 10);
                control.Width = 260;
                panel.Controls.Add(control);
                panel.CreateControl();

                var permissions = new UiAutomationPermissions
                {
                    AutomaticDiscovery = true,
                    ReadControls = true,
                    ReadData = true,
                    WriteControls = true
                };
                var registry = new InMemoryToolRegistry();
                using (var host = HAgentHost.Attach(panel, "HyperControlPanel", registry, true, permissions))
                {
                    var value = await host.Context.ReadControlAsync("txtCustomerName", CancellationToken.None);
                    if (!string.Equals(Convert.ToString(value), "Adapter Customer", StringComparison.Ordinal))
                        throw new InvalidOperationException("The application control adapter did not use GetValue().");

                    var descriptors = new WinFormsSemanticDiscovery().Discover((Control)panel, permissions);
                    var descriptor = descriptors.FirstOrDefault(x => string.Equals(x.ControlId, "txtCustomerName", StringComparison.OrdinalIgnoreCase));
                    if (descriptor == null)
                        throw new InvalidOperationException("Semantic discovery did not identify the adapted application control.");
                    if (!string.Equals(descriptor.DataRole, "database-field", StringComparison.Ordinal))
                        throw new InvalidOperationException("The adapted control did not expose its database-field role.");
                    if (!string.Equals(descriptor.Role, "database-field", StringComparison.Ordinal))
                        throw new InvalidOperationException("The adapted control did not expose its database-field control role.");
                    if (!string.Equals(Convert.ToString(descriptor.Metadata["dbFieldName"]), "CustomerName", StringComparison.Ordinal))
                        throw new InvalidOperationException("The adapted control did not expose DbFieldName metadata.");
                    if (!string.Equals(Convert.ToString(descriptor.Metadata["displayName"]), "Customer Name", StringComparison.Ordinal))
                        throw new InvalidOperationException("The adapted control did not expose DisplayName metadata.");
                    if (!string.Equals(Convert.ToString(descriptor.Metadata["titleEn"]), "Customer Name", StringComparison.Ordinal))
                        throw new InvalidOperationException("The adapted control did not expose TitleEn metadata.");

                    var adapter = new ReflectionUiControlAdapter();
                    if (!adapter.CanRead(control) || !adapter.CanWrite(control))
                        throw new InvalidOperationException("The reflection adapter did not identify GetValue()/SetValue(object) capabilities.");
                    adapter.WriteValue(control, "Changed Customer");
                    var changed = await host.Context.ReadControlAsync("txtCustomerName", CancellationToken.None);
                    if (!string.Equals(Convert.ToString(changed), "Changed Customer", StringComparison.Ordinal))
                        throw new InvalidOperationException("The application control adapter did not invoke SetValue(object) correctly.");

                    Write("UI CUSTOM CONTROL ADAPTER",
                        "Contract test succeeded." + Environment.NewLine +
                        "Control: " + control.GetType().FullName + Environment.NewLine +
                        "DbFieldName: " + control.DbFieldName + Environment.NewLine +
                        "DisplayName: " + control.DisplayName + Environment.NewLine +
                        "Logical role: " + descriptor.Role + Environment.NewLine +
                        "Data role: " + descriptor.DataRole + Environment.NewLine +
                        "Initial GetValue(): Adapter Customer" + Environment.NewLine +
                        "SetValue() result: " + Convert.ToString(changed) + Environment.NewLine +
                        "Metadata discovery and runtime value adapter verified without referencing the external IHyperControl type.");
                }
            }
        }

        private async Task TestApplicationObjectContextAsync(string unused)
        {
            using (var panel = new UserControl { Name = "ApplicationContextPanel", Width = 460, Height = 180 })
            {
                var child = new TableInfoLike("InvoiceLine", "InvoiceLineId", true)
                {
                    Filter = "InvoiceId = 42",
                    Order = "InvoiceLineId",
                    UseYear = true,
                    Year = 2026,
                    YearColumnName = "InvoiceYear",
                    UseBranch = true,
                    BranchId = "7",
                    BranchColumnName = "BranchId",
                    RelatedTables = new List<string> { "Product", "Tax" }
                };
                var tableInfo = new TableInfoLike("Invoice", "InvoiceId")
                {
                    FieldString = "InvoiceId,CustomerId,InvoiceDate",
                    Filter = "IsDeleted = 0",
                    Order = "InvoiceDate DESC",
                    UseYear = true,
                    Year = 2026,
                    YearColumnName = "InvoiceYear",
                    UseBranch = true,
                    BranchId = "7",
                    BranchColumnName = "BranchId",
                    RelatedTables = new List<string> { "Customer", "InvoiceLine" }
                };
                tableInfo.ChildTable.Add(child);

                panel.CreateControl();
                var permissions = new UiAutomationPermissions { AutomaticDiscovery = true, ReadControls = true, ReadData = true };
                using (var host = HAgentHost.Attach(panel, "ApplicationContextPanel", new InMemoryToolRegistry(), false, permissions))
                {
                    host.Application.Attach("invoiceTable", tableInfo, maxDepth: 2, maxCollectionItems: 10);
                    var descriptor = host.Application.Describe("invoiceTable");
                    if (descriptor == null || descriptor.Type.IndexOf("TableInfoLike", StringComparison.OrdinalIgnoreCase) < 0)
                        throw new InvalidOperationException("Application object discovery did not identify the attached TableInfo-shaped object.");

                    var tableName = descriptor.Properties.FirstOrDefault(x => string.Equals(x.Name, "TableName", StringComparison.OrdinalIgnoreCase));
                    var primaryKey = descriptor.Properties.FirstOrDefault(x => string.Equals(x.Name, "PkName", StringComparison.OrdinalIgnoreCase));
                    var related = descriptor.Properties.FirstOrDefault(x => string.Equals(x.Name, "RelatedTables", StringComparison.OrdinalIgnoreCase));
                    var children = descriptor.Properties.FirstOrDefault(x => string.Equals(x.Name, "ChildTable", StringComparison.OrdinalIgnoreCase));
                    var useYear = descriptor.Properties.FirstOrDefault(x => string.Equals(x.Name, "UseYear", StringComparison.OrdinalIgnoreCase));
                    var branchId = descriptor.Properties.FirstOrDefault(x => string.Equals(x.Name, "BranchId", StringComparison.OrdinalIgnoreCase));

                    if (tableName == null || !string.Equals(Convert.ToString(tableName.Value), "Invoice", StringComparison.Ordinal))
                        throw new InvalidOperationException("Application object discovery did not read TableName.");
                    if (primaryKey == null || !string.Equals(Convert.ToString(primaryKey.Value), "InvoiceId", StringComparison.Ordinal))
                        throw new InvalidOperationException("Application object discovery did not read PkName.");
                    if (related == null || related.Count != 2 || related.ItemType != typeof(string).FullName)
                        throw new InvalidOperationException("Application object discovery did not identify the RelatedTables list.");
                    if (children == null || children.Kind != "collection" || children.Count != 1 || string.IsNullOrWhiteSpace(children.ItemType))
                        throw new InvalidOperationException("Application object discovery did not identify the ChildTable collection.");
                    if (children.Items == null || children.Items.Count != 1)
                        throw new InvalidOperationException("Application object discovery did not inspect the bounded child TableInfo object.");
                    if (useYear == null || !Equals(useYear.Value, true))
                        throw new InvalidOperationException("Application object discovery did not read UseYear.");
                    if (branchId == null || !string.Equals(Convert.ToString(branchId.Value), "7", StringComparison.Ordinal))
                        throw new InvalidOperationException("Application object discovery did not read BranchId.");

                    Write("APPLICATION OBJECT CONTEXT",
                        "Contract test succeeded." + Environment.NewLine +
                        "Attached object ID: invoiceTable" + Environment.NewLine +
                        "Object type: " + descriptor.Type + Environment.NewLine +
                        "TableName: " + Convert.ToString(tableName.Value) + Environment.NewLine +
                        "PkName: " + Convert.ToString(primaryKey.Value) + Environment.NewLine +
                        "RelatedTables count: " + Convert.ToString(related.Count) + Environment.NewLine +
                        "ChildTable count: " + Convert.ToString(children.Count) + Environment.NewLine +
                        "Child object inspected: " + Convert.ToString(children.Items.Count) + Environment.NewLine +
                        "UseYear: " + Convert.ToString(useYear.Value) + Environment.NewLine +
                        "BranchId: " + Convert.ToString(branchId.Value) + Environment.NewLine +
                        "Bounded public-property discovery verified without knowing the TableInfo class at compile time.");
                }
            }
        }

        private sealed class HyperLikeTextBox : TextBox
        {
            public string DbFieldName { get; set; }
            public string DisplayName { get; set; }
            public string TitleEn { get; set; }
            public string TitleAr { get; set; }
            public bool IsRequired { get; set; }
            public bool IsSearchField { get; set; }
            public string DataSourceName { get; set; }
            public short DataSourceIndex { get; set; }
            public object DataType { get; set; }

            public object GetValue()
            {
                return Text;
            }

            public void SetValue(object value)
            {
                Text = value == null ? string.Empty : Convert.ToString(value);
            }
        }

        private sealed class TableInfoLike
        {
            private readonly string _pkFieldName;
            private readonly string _tableName;

            public TableInfoLike(string tableName, string pkFieldName, bool isChild = false)
            {
                _tableName = tableName;
                _pkFieldName = pkFieldName;
                IsChildTable = isChild;
            }

            public List<TableInfoLike> ChildTable { get; } = new List<TableInfoLike>();
            public string BranchColumnName { get; set; }
            public string BranchId { get; set; }
            public string DeleteType { get; set; }
            public string FieldString { get; set; }
            public string Filter { get; set; }
            public bool IsChildTable { get; private set; }
            public string Order { get; set; }
            public string PkName { get { return _pkFieldName; } }
            public List<string> RelatedTables { get; set; } = new List<string>();
            public string TableName { get { return _tableName; } }
            public bool UseBranch { get; set; }
            public bool UseVoidFieldInSelect { get; set; }
            public bool UseYear { get; set; }
            public string VoidFieldName { get; set; }
            public int Year { get; set; }
            public string YearColumnName { get; set; }
        }
    }
}
