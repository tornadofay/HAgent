using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HAgent.WinForms.UI
{
    /// <summary>
    /// Bounded, read-only reflection over application-owned objects.
    /// It inspects public readable properties only and never invokes arbitrary methods.
    /// </summary>
    public sealed class ReflectionApplicationObjectDiscovery
    {
        public ApplicationObjectDescriptor Describe(object instance, string id, int maxDepth = 2, int maxCollectionItems = 20)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Object ID is required.", nameof(id));
            if (maxDepth < 0) throw new ArgumentOutOfRangeException(nameof(maxDepth));
            if (maxCollectionItems < 1) throw new ArgumentOutOfRangeException(nameof(maxCollectionItems));

            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            return DescribeObject(instance, id.Trim(), maxDepth, maxCollectionItems, visited);
        }

        private static ApplicationObjectDescriptor DescribeObject(
            object instance,
            string id,
            int depth,
            int maxCollectionItems,
            HashSet<object> visited)
        {
            if (instance == null) return null;
            if (!IsSafeObject(instance)) return null;
            if (!visited.Add(instance)) return null;

            var properties = new List<ApplicationObjectPropertyDescriptor>();
            foreach (var property in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead || property.GetIndexParameters().Length != 0) continue;

                object value;
                try { value = property.GetValue(instance, null); }
                catch { continue; }

                properties.Add(DescribeProperty(property.Name, property.PropertyType, value, depth, maxCollectionItems, visited));
            }

            return new ApplicationObjectDescriptor
            {
                Id = id,
                Type = instance.GetType().FullName,
                Properties = properties.AsReadOnly()
            };
        }

        private static ApplicationObjectPropertyDescriptor DescribeProperty(
            string name,
            Type declaredType,
            object value,
            int depth,
            int maxCollectionItems,
            HashSet<object> visited)
        {
            var descriptor = new ApplicationObjectPropertyDescriptor
            {
                Name = name,
                Type = declaredType == null ? null : declaredType.FullName,
                Kind = "scalar"
            };

            if (value == null)
            {
                descriptor.Value = null;
                return descriptor;
            }

            if (IsSafeScalar(value))
            {
                descriptor.Value = value;
                return descriptor;
            }

            var enumerable = value as IEnumerable;
            if (enumerable != null && !(value is string))
            {
                descriptor.Kind = "collection";
                descriptor.Count = TryGetCount(value);
                descriptor.ItemType = ResolveItemType(value);

                if (depth <= 0) return descriptor;

                var items = new List<ApplicationObjectDescriptor>();
                foreach (var item in enumerable)
                {
                    if (items.Count >= maxCollectionItems) break;
                    if (item == null || IsSafeScalar(item)) continue;
                    var itemDescriptor = DescribeObject(item, name + "[" + items.Count + "]", depth - 1, maxCollectionItems, visited);
                    if (itemDescriptor != null) items.Add(itemDescriptor);
                }
                descriptor.Items = items.AsReadOnly();
                return descriptor;
            }

            descriptor.Kind = "object";
            if (depth > 0)
                descriptor.Object = DescribeObject(value, name, depth - 1, maxCollectionItems, visited);
            return descriptor;
        }

        private static bool IsSafeObject(object value)
        {
            var type = value.GetType();
            return !(type.IsPointer || type.IsByRef || value is Delegate);
        }

        private static bool IsSafeScalar(object value)
        {
            var type = value.GetType();
            return type.IsPrimitive || type.IsEnum || value is string || value is decimal ||
                   value is DateTime || value is DateTimeOffset || value is TimeSpan || value is Guid;
        }

        private static int? TryGetCount(object value)
        {
            var collection = value as ICollection;
            if (collection != null) return collection.Count;

            var property = value.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanRead || property.GetIndexParameters().Length != 0) return null;
            try { return Convert.ToInt32(property.GetValue(value, null)); }
            catch { return null; }
        }

        private static string ResolveItemType(object value)
        {
            var type = value.GetType();
            if (type.IsArray)
                return type.GetElementType().FullName;

            var enumerable = type.GetInterfaces()
                .Concat(new[] { type })
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return enumerable == null ? null : enumerable.GetGenericArguments()[0].FullName;
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

            public new bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
