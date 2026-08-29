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
        public IReadOnlyList<UiDataSourceDescriptor> Discover(Control root, UiAutomationPermissions permissions)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (permissions == null) throw new ArgumentNullException(nameof(permissions));
            permissions.Validate();
            if (!permissions.AutomaticDiscovery || !permissions.ReadData)
                throw new InvalidOperationException("Automatic data-source discovery is disabled by the current permission policy.");

            var result = new List<UiDataSourceDescriptor>();
            Visit(root, result);
            return result.AsReadOnly();
        }

        public IReadOnlyList<UiDataSourceDescriptor> Discover(Form form, UiAutomationPermissions permissions)
        {
            return Discover((Control)form, permissions);
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
            if (control == null) return null;

            object source = null;
            string dataMember = null;
            string bindingPath = null;
            Binding binding = null;

            var grid = control as DataGridView;
            if (grid != null && grid.DataSource != null)
            {
                source = grid.DataSource;
                dataMember = TryGetDataMember(source);
                bindingPath = "DataSource";
            }
            else
            {
                foreach (Binding candidate in control.DataBindings)
                {
                    if (candidate == null || candidate.DataSource == null) continue;
                    binding = candidate;
                    source = candidate.DataSource;
                    dataMember = TryGetBindingDataMember(candidate);
                    bindingPath = candidate.PropertyName;
                    break;
                }
            }

            if (source == null) return null;

            var bindingSource = source as BindingSource;
            var effectiveSource = bindingSource == null ? source : (bindingSource.List ?? bindingSource.DataSource);
            var descriptor = new UiDataSourceDescriptor
            {
                ControlId = control.Name,
                ControlType = control.GetType().FullName,
                SourceKind = ResolveSourceKind(source),
                SourceType = source.GetType().FullName,
                UnderlyingSourceType = bindingSource == null || bindingSource.DataSource == null
                    ? null
                    : bindingSource.DataSource.GetType().FullName,
                ListType = bindingSource == null || bindingSource.List == null
                    ? null
                    : bindingSource.List.GetType().FullName,
                DataMember = dataMember,
                BindingPath = bindingPath,
                CurrencyManagerType = ResolveCurrencyManagerType(control, source),
                Position = TryGetPosition(source),
                ItemType = ResolveItemType(effectiveSource),
                Count = TryGetCount(source, effectiveSource),
                FieldNames = ResolveFieldNames(effectiveSource),
                Metadata = BuildMetadata(source, effectiveSource)
            };

            return descriptor;
        }

        private static string ResolveSourceKind(object source)
        {
            if (source is BindingSource) return "BindingSource";
            if (source is DataTable) return "DataTable";
            if (source is DataView) return "DataView";
            if (source is DataSet) return "DataSet";
            if (source is IList) return "IList";
            if (source is IEnumerable && !(source is string)) return "Enumerable";
            return source.GetType().Name;
        }

        private static string ResolveCurrencyManagerType(Control control, object source)
        {
            if (control == null || source == null) return null;
            try
            {
                var manager = control.BindingContext[source];
                return manager == null ? null : manager.GetType().FullName;
            }
            catch
            {
                return null;
            }
        }

        private static int? TryGetPosition(object source)
        {
            var bindingSource = source as BindingSource;
            if (bindingSource != null) return bindingSource.Position < 0 ? (int?)null : bindingSource.Position;

            var currencyManager = source as CurrencyManager;
            if (currencyManager != null) return currencyManager.Position < 0 ? (int?)null : currencyManager.Position;

            return null;
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

        private static int? TryGetCount(object source, object effectiveSource)
        {
            var bindingSource = source as BindingSource;
            if (bindingSource != null) return bindingSource.Count;

            var table = effectiveSource as DataTable;
            if (table != null) return table.Rows.Count;
            var view = effectiveSource as DataView;
            if (view != null) return view.Count;
            var collection = effectiveSource as ICollection;
            if (collection != null) return collection.Count;
            var property = effectiveSource == null ? null : effectiveSource.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanRead) return null;
            try { return Convert.ToInt32(property.GetValue(effectiveSource, null)); }
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

        private static Dictionary<string, object> BuildMetadata(object source, object effectiveSource)
        {
            var metadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["sourceType"] = source.GetType().FullName,
                ["sourceKind"] = ResolveSourceKind(source),
                ["effectiveSourceType"] = effectiveSource == null ? null : effectiveSource.GetType().FullName
            };

            var bindingSource = source as BindingSource;
            if (bindingSource != null)
            {
                metadata["dataMember"] = bindingSource.DataMember;
                metadata["listType"] = bindingSource.List == null ? null : bindingSource.List.GetType().FullName;
                metadata["position"] = bindingSource.Position;
            }

            var dataSet = effectiveSource as DataSet;
            if (dataSet != null)
                metadata["tableCount"] = dataSet.Tables.Count;

            return metadata;
        }

        private static string TryGetBindingDataMember(Binding binding)
        {
            if (binding == null || binding.DataSource == null)
                return null;

            var source = binding.DataSource;
            var member = source as BindingSource;
            if (member != null)
                return string.IsNullOrWhiteSpace(member.DataMember) ? null : member.DataMember;

            try
            {
                var info = binding.BindingMemberInfo;
                return string.IsNullOrWhiteSpace(info.BindingMember) ? null : info.BindingMember;
            }
            catch
            {
                return TryGetDataMember(source);
            }
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
