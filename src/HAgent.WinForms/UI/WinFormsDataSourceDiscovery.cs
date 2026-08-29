using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace HAgent.WinForms.UI
{
    public sealed class WinFormsDataSourceDiscovery
    {
        public IReadOnlyList<UiDataSourceDescriptor> Discover(Form form, UiAutomationPermissions permissions)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (permissions == null) throw new ArgumentNullException(nameof(permissions));
            permissions.Validate();
            if (!permissions.AutomaticDiscovery || !permissions.ReadData)
                throw new InvalidOperationException("Automatic data-source discovery is disabled by the current permission policy.");

            var result = new List<UiDataSourceDescriptor>();
            Visit(form, result);
            return result.AsReadOnly();
        }

        private static void Visit(Control control, List<UiDataSourceDescriptor> result)
        {
            var descriptor = Describe(control);
            if (descriptor != null) result.Add(descriptor);
            foreach (Control child in control.Controls)
                Visit(child, result);
        }

        private static UiDataSourceDescriptor Describe(Control control)
        {
            object source = null;
            string dataMember = null;

            var grid = control as DataGridView;
            if (grid != null && grid.DataSource != null)
            {
                source = grid.DataSource;
                dataMember = TryGetDataMember(source);
            }
            else
            {
                foreach (Binding binding in control.DataBindings)
                {
                    if (binding == null || binding.DataSource == null) continue;
                    source = UnwrapBindingSource(binding.DataSource, out dataMember);
                    if (source != null) break;
                }
            }

            if (source == null) return null;

            var descriptor = new UiDataSourceDescriptor
            {
                ControlId = control.Name,
                ControlType = control.GetType().FullName,
                SourceType = source.GetType().FullName,
                DataMember = dataMember,
                ItemType = ResolveItemType(source),
                Count = TryGetCount(source),
                FieldNames = ResolveFieldNames(source),
                Metadata = BuildMetadata(source)
            };

            return descriptor;
        }

        private static object UnwrapBindingSource(object source, out string dataMember)
        {
            dataMember = null;
            var bindingSource = source as BindingSource;
            if (bindingSource == null) return source;
            dataMember = string.IsNullOrWhiteSpace(bindingSource.DataMember) ? null : bindingSource.DataMember;
            return bindingSource.List ?? bindingSource.DataSource;
        }

        private static string ResolveItemType(object source)
        {
            if (source is DataTable)
                return typeof(DataRow).FullName;
            if (source is DataView)
                return typeof(DataRowView).FullName;

            var enumerable = source as IEnumerable;
            if (enumerable == null || source is string) return null;

            var type = source.GetType();
            if (type.IsArray) return type.GetElementType().FullName;
            var generic = type.GetInterfaces()
                .Concat(new[] { type })
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return generic == null ? null : generic.GetGenericArguments()[0].FullName;
        }

        private static int? TryGetCount(object source)
        {
            var table = source as DataTable;
            if (table != null) return table.Rows.Count;
            var view = source as DataView;
            if (view != null) return view.Count;
            var collection = source as ICollection;
            if (collection != null) return collection.Count;
            var property = source.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanRead) return null;
            try { return Convert.ToInt32(property.GetValue(source, null)); }
            catch { return null; }
        }

        private static IReadOnlyList<string> ResolveFieldNames(object source)
        {
            var table = source as DataTable;
            if (table != null)
                return table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList().AsReadOnly();

            var view = source as DataView;
            if (view != null)
                return view.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList().AsReadOnly();

            var itemTypeName = ResolveItemType(source);
            if (string.IsNullOrWhiteSpace(itemTypeName)) return new List<string>().AsReadOnly();
            var itemType = Type.GetType(itemTypeName, false);
            if (itemType == null) return new List<string>().AsReadOnly();

            var fields = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .Select(p => p.Name)
                .ToList();
            return fields.AsReadOnly();
        }

        private static Dictionary<string, object> BuildMetadata(object source)
        {
            var metadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["sourceType"] = source.GetType().FullName
            };

            var bindingSource = source as BindingSource;
            if (bindingSource != null)
            {
                metadata["dataMember"] = bindingSource.DataMember;
                metadata["listType"] = bindingSource.List == null ? null : bindingSource.List.GetType().FullName;
            }

            var dataSet = source as DataSet;
            if (dataSet != null)
                metadata["tableCount"] = dataSet.Tables.Count;

            return metadata;
        }

        private static string TryGetDataMember(object source)
        {
            if (source == null) return null;
            var property = source.GetType().GetProperty("DataMember", BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanRead) return null;
            try { return Convert.ToString(property.GetValue(source, null)); }
            catch { return null; }
        }
    }
}
