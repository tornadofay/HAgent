using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Data;

namespace HAgent.WinForms.UI
{
    public sealed class WinFormsDataRelationshipDiscovery
    {
        public IReadOnlyList<UiDataRelationshipDescriptor> Discover(Control root, UiAutomationPermissions permissions)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (permissions == null) throw new ArgumentNullException(nameof(permissions));
            permissions.Validate();
            if (!permissions.AutomaticDiscovery || !permissions.ReadData)
                throw new InvalidOperationException("Automatic data relationship discovery is disabled by the current permission policy.");

            var bindings = new List<BindingObservation>();
            Visit(root, bindings);

            var groups = bindings
                .GroupBy(x => x.Source, ReferenceEqualityComparer.Instance)
                .ToDictionary(x => x.Key, x => x.ToList(), ReferenceEqualityComparer.Instance);

            var result = new List<UiDataRelationshipDescriptor>();
            foreach (var observation in bindings)
            {
                var group = groups[observation.Source];
                var related = group
                    .Where(x => !string.Equals(x.ControlId, observation.ControlId, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.ControlId)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                result.Add(new UiDataRelationshipDescriptor
                {
                    ControlId = observation.ControlId,
                    ControlType = observation.ControlType,
                    RelationshipKind = related.Count == 0 ? "bound-to-source" : "shares-data-source",
                    SourceKind = ResolveSourceKind(observation.Source),
                    SourceType = observation.Source.GetType().FullName,
                    UnderlyingSourceType = ResolveUnderlyingSourceType(observation.Source),
                    BindingPath = observation.BindingPath,
                    DataMember = observation.DataMember,
                    CurrencyManagerType = observation.CurrencyManagerType,
                    Position = observation.Position,
                    CurrentItemType = observation.CurrentItemType,
                    RelatedControlIds = related.AsReadOnly(),
                    Metadata = BuildMetadata(observation.Source, related.Count)
                });
            }

            return result.AsReadOnly();
        }

        private static void Visit(Control control, List<BindingObservation> result)
        {
            var observation = Describe(control);
            if (observation != null) result.Add(observation);
            foreach (Control child in control.Controls)
                Visit(child, result);
        }

        private static BindingObservation Describe(Control control)
        {
            object source = null;
            string bindingPath = null;
            string dataMember = null;

            var grid = control as DataGridView;
            if (grid != null && grid.DataSource != null)
            {
                source = grid.DataSource;
                bindingPath = "DataSource";
                dataMember = TryGetDataMember(source);
            }
            else
            {
                foreach (Binding binding in control.DataBindings)
                {
                    if (binding == null || binding.DataSource == null) continue;
                    source = binding.DataSource;
                    bindingPath = binding.PropertyName;
                    dataMember = TryGetBindingDataMember(binding);
                    break;
                }
            }

            if (source == null) return null;

            var bindingSource = source as BindingSource;
            var currencyManager = ResolveCurrencyManager(control, source);
            return new BindingObservation
            {
                ControlId = control.Name,
                ControlType = control.GetType().FullName,
                Source = source,
                BindingPath = bindingPath,
                DataMember = dataMember,
                CurrencyManagerType = currencyManager == null ? null : currencyManager.GetType().FullName,
                Position = bindingSource != null
                    ? NormalizePosition(bindingSource.Position)
                    : currencyManager == null ? (int?)null : NormalizePosition(currencyManager.Position),
                CurrentItemType = ResolveCurrentItemType(bindingSource, currencyManager)
            };
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

        private static string ResolveUnderlyingSourceType(object source)
        {
            var bindingSource = source as BindingSource;
            if (bindingSource == null || bindingSource.DataSource == null) return null;
            return bindingSource.DataSource.GetType().FullName;
        }

        private static CurrencyManager ResolveCurrencyManager(Control control, object source)
        {
            try { return control.BindingContext[source] as CurrencyManager; }
            catch { return null; }
        }

        private static int? NormalizePosition(int position)
        {
            return position < 0 ? (int?)null : position;
        }

        private static string ResolveCurrentItemType(BindingSource bindingSource, CurrencyManager currencyManager)
        {
            object current = null;
            try
            {
                if (bindingSource != null) current = bindingSource.Current;
                else if (currencyManager != null) current = currencyManager.Current;
            }
            catch { current = null; }
            return current == null ? null : current.GetType().FullName;
        }

        private static string TryGetBindingDataMember(Binding binding)
        {
            if (binding == null || binding.DataSource == null) return null;
            var source = binding.DataSource as BindingSource;
            if (source != null)
                return string.IsNullOrWhiteSpace(source.DataMember) ? null : source.DataMember;

            try
            {
                var info = binding.BindingMemberInfo;
                return string.IsNullOrWhiteSpace(info.BindingMember) ? null : info.BindingMember;
            }
            catch { return null; }
        }

        private static string TryGetDataMember(object source)
        {
            if (source == null) return null;
            var property = source.GetType().GetProperty("DataMember", BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanRead) return null;
            try { return Convert.ToString(property.GetValue(source, null)); }
            catch { return null; }
        }

        private static Dictionary<string, object> BuildMetadata(object source, int relatedCount)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["sourceKind"] = ResolveSourceKind(source),
                ["sourceType"] = source.GetType().FullName,
                ["relatedControlCount"] = relatedCount
            };
        }

        private sealed class BindingObservation
        {
            public string ControlId { get; set; }
            public string ControlType { get; set; }
            public object Source { get; set; }
            public string BindingPath { get; set; }
            public string DataMember { get; set; }
            public string CurrencyManagerType { get; set; }
            public int? Position { get; set; }
            public string CurrentItemType { get; set; }
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
