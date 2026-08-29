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
        public IReadOnlyList<UiSemanticDescriptor> Discover(Form form, UiAutomationPermissions permissions)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (permissions == null) throw new ArgumentNullException(nameof(permissions));
            permissions.Validate();
            if (!permissions.AutomaticDiscovery)
                throw new InvalidOperationException("Automatic UI discovery is disabled by the current permission policy.");

            var result = new List<UiSemanticDescriptor>();
            Visit(form, result, permissions);
            return result.AsReadOnly();
        }

        private static void Visit(Control control, List<UiSemanticDescriptor> result, UiAutomationPermissions permissions)
        {
            var descriptor = Describe(control, permissions);
            if (descriptor != null) result.Add(descriptor);

            foreach (Control child in control.Controls)
                Visit(child, result, permissions);
        }

        private static UiSemanticDescriptor Describe(Control control, UiAutomationPermissions permissions)
        {
            var logicalName = FirstNonEmpty(
                control.AccessibleName,
                control.Tag as string,
                control.Name,
                control.Text);

            if (string.IsNullOrWhiteSpace(logicalName))
                return null;

            var role = InferRole(control);
            var dataRole = InferDataRole(control);
            var binding = FindBinding(control);

            return new UiSemanticDescriptor
            {
                ControlId = control.Name,
                LogicalName = NormalizeLogicalName(logicalName),
                Role = role,
                Description = control.AccessibleDescription,
                DataRole = dataRole,
                DataMember = binding == null ? null : binding.DataMember,
                BindingPath = binding == null ? null : binding.BindingPath,
                Readable = permissions.ReadControls && IsReadable(control),
                Writable = permissions.WriteControls && IsWritable(control),
                Invokable = permissions.InvokeControls && IsInvokable(control),
                Metadata = BuildMetadata(control)
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

        private static string InferDataRole(Control control)
        {
            if (control is DataGridView)
            {
                var grid = (DataGridView)control;
                if (grid.DataSource is DataTable) return "tabular-data";
                if (grid.DataSource is DataView) return "tabular-view";
                if (grid.DataSource != null) return "bound-data";
            }

            var binding = FindBinding(control);
            return binding == null ? null : "bound-value";
        }

        private static BindingInfo FindBinding(Control control)
        {
            var bindings = control.DataBindings;
            foreach (Binding binding in bindings)
            {
                if (binding == null) continue;
                return new BindingInfo
                {
                    BindingPath = binding.PropertyName,
                    DataMember = binding.BindingManagerBase == null ? null : binding.BindingManagerBase.BindingPath
                };
            }

            var grid = control as DataGridView;
            if (grid != null && grid.DataSource != null)
            {
                return new BindingInfo { BindingPath = "DataSource", DataMember = TryGetDataMember(grid.DataSource) };
            }

            return null;
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
