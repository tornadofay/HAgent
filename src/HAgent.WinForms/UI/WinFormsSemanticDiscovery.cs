using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace HAgent.WinForms.UI
{
    public sealed class WinFormsSemanticDiscovery
    {
        private static readonly IUiControlAdapter DefaultControlAdapter = new ReflectionUiControlAdapter();

        public IReadOnlyList<UiSemanticDescriptor> Discover(Control root, UiAutomationPermissions permissions, IUiSemanticProvider customProvider = null)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (permissions == null) throw new ArgumentNullException(nameof(permissions));
            permissions.Validate();
            if (!permissions.AutomaticDiscovery)
                throw new InvalidOperationException("Automatic UI discovery is disabled by the current permission policy.");

            var result = new List<UiSemanticDescriptor>();
            Visit(root, result, permissions, customProvider);
            return result.AsReadOnly();
        }

        public IReadOnlyList<UiSemanticDescriptor> Discover(Form form, UiAutomationPermissions permissions, IUiSemanticProvider customProvider = null)
        {
            return Discover((Control)form, permissions, customProvider);
        }

        private static void Visit(Control control, List<UiSemanticDescriptor> result, UiAutomationPermissions permissions, IUiSemanticProvider customProvider)
        {
            var descriptor = customProvider == null ? null : customProvider.Describe(control);
            if (descriptor == null)
                descriptor = Describe(control, permissions);
            if (descriptor != null) result.Add(descriptor);

            foreach (Control child in control.Controls)
                Visit(child, result, permissions, customProvider);
        }

        private static UiSemanticDescriptor Describe(Control control, UiAutomationPermissions permissions)
        {
            var binding = FindBinding(control);
            var adapterName = DefaultControlAdapter.CanAdapt(control)
                ? DefaultControlAdapter.GetLogicalName(control)
                : null;
            var logicalName = FirstNonEmpty(
                adapterName,
                control.AccessibleName,
                control.Name,
                control.Text);
            if (string.IsNullOrWhiteSpace(logicalName)) return null;

            var adapterCanRead = DefaultControlAdapter.CanRead(control);
            var adapterCanWrite = DefaultControlAdapter.CanWrite(control);
            var adapterDataRole = DefaultControlAdapter.GetDataRole(control);
            var adapterMetadata = DefaultControlAdapter.CanAdapt(control)
                ? DefaultControlAdapter.GetMetadata(control)
                : new Dictionary<string, object>();

            return new UiSemanticDescriptor
            {
                ControlId = control.Name,
                LogicalName = NormalizeLogicalName(logicalName),
                Role = adapterDataRole == "database-field" ? "database-field" : InferRole(control),
                Description = control.AccessibleDescription,
                DataRole = adapterDataRole ?? InferDataRole(control, binding),
                DataMember = binding == null ? null : binding.DataMember,
                BindingPath = binding == null ? null : binding.BindingPath,
                Readable = permissions.ReadControls && (adapterCanRead || IsReadable(control)),
                Writable = permissions.WriteControls && (adapterCanWrite || IsWritable(control)),
                Invokable = permissions.InvokeControls && IsInvokable(control),
                Metadata = MergeMetadata(BuildMetadata(control), adapterMetadata)
            };
        }

        private static string InferRole(Control control)
        {
            if (control is Label) return "label";
            if (control is TextBoxBase) return "text-input";
            if (control is ComboBox) return "selection";
            if (control is CheckBox) return "boolean";
            if (control is RadioButton) return "option";
            if (control is NumericUpDown) return "numeric-input";
            if (control is DateTimePicker) return "date-input";
            if (control is DataGridView) return "data-grid";
            if (control is ListView) return "list";
            if (control is ListBox) return "selection-list";
            if (control is ButtonBase) return "action";
            return control.GetType().Name;
        }

        private static string InferDataRole(Control control, BindingInfo binding)
        {
            if (control is DataGridView)
            {
                var grid = (DataGridView)control;
                if (grid.DataSource is DataTable) return "tabular-data";
                if (grid.DataSource is DataView) return "tabular-view";
                if (grid.DataSource != null) return "bound-data";
            }
            return binding == null ? null : "bound-value";
        }

        private static BindingInfo FindBinding(Control control)
        {
            foreach (Binding binding in control.DataBindings)
            {
                if (binding == null) continue;
                return new BindingInfo
                {
                    BindingPath = binding.PropertyName,
                    DataMember = TryGetBindingDataMember(binding)
                };
            }

            var grid = control as DataGridView;
            if (grid != null && grid.DataSource != null)
                return new BindingInfo { BindingPath = "DataSource", DataMember = TryGetDataMember(grid.DataSource) };

            return null;
        }

        private static string TryGetBindingDataMember(Binding binding)
        {
            if (binding == null || binding.DataSource == null)
                return null;

            var source = binding.DataSource;
            var member = source as BindingSource;
            if (member != null)
                return string.IsNullOrWhiteSpace(member.DataMember) ? null : member.DataMember;

            return TryGetDataMember(source);
        }

        private static string TryGetDataMember(object source)
        {
            if (source == null) return null;
            var property = source.GetType().GetProperty("DataMember", BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanRead) return null;
            try { return Convert.ToString(property.GetValue(source, null)); }
            catch { return null; }
        }

        private static Dictionary<string, object> BuildMetadata(Control control)
        {
            var metadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["controlType"] = control.GetType().FullName,
                ["enabled"] = control.Enabled,
                ["visible"] = control.Visible,
                ["tabIndex"] = control.TabIndex
            };

            var grid = control as DataGridView;
            if (grid != null)
            {
                metadata["columnCount"] = grid.Columns.Count;
                metadata["rowCount"] = grid.Rows.Cast<DataGridViewRow>().Count(r => !r.IsNewRow);
                metadata["hasDataSource"] = grid.DataSource != null;
            }
            return metadata;
        }

        private static Dictionary<string, object> MergeMetadata(
            IReadOnlyDictionary<string, object> standard,
            IReadOnlyDictionary<string, object> adapter)
        {
            var merged = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (standard != null)
            {
                foreach (var pair in standard)
                    merged[pair.Key] = pair.Value;
            }
            if (adapter != null)
            {
                foreach (var pair in adapter)
                    merged[pair.Key] = pair.Value;
            }
            return merged;
        }

        private static bool IsReadable(Control control)
        {
            return control is TextBoxBase || control is ComboBox || control is CheckBox ||
                   control is RadioButton || control is NumericUpDown || control is DateTimePicker ||
                   control is ListBox || control is ListView || control is Label || control is DataGridView;
        }

        private static bool IsWritable(Control control)
        {
            return control is TextBoxBase || control is ComboBox || control is CheckBox ||
                   control is RadioButton || control is NumericUpDown || control is DateTimePicker;
        }

        private static bool IsInvokable(Control control)
        {
            return control is ButtonBase;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        }

        private static string NormalizeLogicalName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return value.Trim().Replace("_", " ");
        }

        private sealed class BindingInfo
        {
            public string BindingPath { get; set; }
            public string DataMember { get; set; }
        }
    }
}
