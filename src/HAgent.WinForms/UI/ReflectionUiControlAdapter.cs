using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace HAgent.WinForms.UI
{
    /// <summary>
    /// Convention-based adapter for application-owned controls such as an external IHyperControl implementation.
    /// It requires a readable DbFieldName property and a parameterless GetValue method, avoiding an assembly dependency.
    /// </summary>
    public sealed class ReflectionUiControlAdapter : IUiControlAdapter
    {
        private static readonly string[] MetadataProperties =
        {
            "DbFieldName", "DisplayName", "TitleEn", "TitleAr", "DataSourceName", "DataSourceIndex",
            "DataType", "DataTypeLength", "DataTypePrecision", "DataTypeScale", "DataTypeSize",
            "IsPrimaryKey", "GenerationMode", "IsNullable", "SqlDataType", "IsConcurrencyToken",
            "IsBinding", "IsSearchField", "PropertyName", "IsRequired", "IsUnique", "IsLogged",
            "LogCaption", "Encrypt"
        };

        public bool CanAdapt(Control control)
        {
            if (control == null) return false;
            return FindProperty(control, "DbFieldName", canRead: true, canWrite: false) != null &&
                   FindGetValueMethod(control) != null;
        }

        public bool CanRead(Control control)
        {
            return CanAdapt(control) && FindGetValueMethod(control) != null;
        }

        public bool CanWrite(Control control)
        {
            return CanAdapt(control) && FindSetValueMethod(control) != null;
        }

        public object ReadValue(Control control)
        {
            var method = FindGetValueMethod(control);
            if (method == null) throw new InvalidOperationException("The adapted control does not expose GetValue().");
            return Invoke(method, control, null);
        }

        public void WriteValue(Control control, object value)
        {
            var method = FindSetValueMethod(control);
            if (method == null) throw new InvalidOperationException("The adapted control does not expose SetValue(object).");
            Invoke(method, control, new[] { value });
        }

        public IReadOnlyDictionary<string, object> GetMetadata(Control control)
        {
            if (!CanAdapt(control)) return new Dictionary<string, object>();

            var metadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["adapter"] = GetType().Name,
                ["contract"] = "DbFieldName + GetValue/SetValue convention",
                ["canReadValue"] = CanRead(control),
                ["canWriteValue"] = CanWrite(control)
            };

            foreach (var name in MetadataProperties)
            {
                var property = FindProperty(control, name, canRead: true, canWrite: false);
                if (property == null) continue;
                try
                {
                    var value = property.GetValue(control, null);
                    if (IsMetadataValue(value))
                        metadata[ToCamelCase(name)] = value is Enum ? Convert.ToString(value, CultureInfo.InvariantCulture) : value;
                }
                catch
                {
                }
            }

            return metadata;
        }

        public string GetLogicalName(Control control)
        {
            if (!CanAdapt(control)) return null;
            return FirstNonEmpty(
                ReadStringProperty(control, "DisplayName"),
                ReadStringProperty(control, "TitleEn"),
                ReadStringProperty(control, "TitleAr"),
                ReadStringProperty(control, "DbFieldName"),
                control.Name,
                control.Text);
        }

        public string GetDataRole(Control control)
        {
            return CanAdapt(control) && !string.IsNullOrWhiteSpace(ReadStringProperty(control, "DbFieldName"))
                ? "database-field"
                : null;
        }

        private static MethodInfo FindGetValueMethod(Control control)
        {
            return control.GetType().GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
        }

        private static MethodInfo FindSetValueMethod(Control control)
        {
            foreach (var method in control.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!string.Equals(method.Name, "SetValue", StringComparison.Ordinal) || method.GetParameters().Length != 1)
                    continue;
                return method;
            }
            return null;
        }

        private static PropertyInfo FindProperty(Control control, string name, bool canRead, bool canWrite)
        {
            var property = control.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (property == null) return null;
            if (canRead && !property.CanRead) return null;
            if (canWrite && !property.CanWrite) return null;
            return property;
        }

        private static object Invoke(MethodInfo method, object target, object[] arguments)
        {
            try
            {
                return method.Invoke(target, arguments);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        }

        private static string ReadStringProperty(Control control, string name)
        {
            var property = FindProperty(control, name, canRead: true, canWrite: false);
            if (property == null) return null;
            try { return Convert.ToString(property.GetValue(control, null), CultureInfo.InvariantCulture); }
            catch { return null; }
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (var value in values)
                if (!string.IsNullOrWhiteSpace(value)) return value;
            return null;
        }

        private static bool IsMetadataValue(object value)
        {
            if (value == null) return true;
            var type = value.GetType();
            return type.IsPrimitive || value is string || value is decimal || value is DateTime || type.IsEnum;
        }

        private static string ToCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }
    }
}
