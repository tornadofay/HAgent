using System;
using System.Collections.Generic;

namespace HAgent.WinForms.UI
{
    /// <summary>
    /// Holds live, application-owned context objects for an HAgent host.
    /// Object instances are references only and are never serialized by HAgent.
    /// </summary>
    public sealed class HAgentApplicationContext
    {
        private sealed class Entry
        {
            public object Instance { get; set; }
            public int MaxDepth { get; set; }
            public int MaxCollectionItems { get; set; }
        }

        private readonly object _sync = new object();
        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        private readonly ReflectionApplicationObjectDiscovery _discovery = new ReflectionApplicationObjectDiscovery();

        /// <summary>
        /// Attaches a live application-owned object for bounded structural discovery.
        /// <para>
        /// The object instance is required because HAgent inspects the live object at runtime;
        /// HAgent does not know the application's concrete type and does not reconstruct it from metadata.
        /// </para>
        /// <paramref name="maxDepth"/> limits recursive traversal. The attached root object is depth 0;
        /// child objects and collection items are inspected only while the remaining depth is greater than 0.
        /// A value of 0 inspects only the root object's direct properties and does not recursively inspect child objects.
        /// <paramref name="maxCollectionItems"/> limits how many non-scalar items are inspected from each collection.
        /// These bounds keep CPU, memory, and output size predictable for large or cyclic application object graphs.
        /// </summary>
        public void Attach(string id, object instance, int maxDepth = 2, int maxCollectionItems = 20)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Object ID is required.", nameof(id));
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (maxDepth < 0) throw new ArgumentOutOfRangeException(nameof(maxDepth));
            if (maxCollectionItems < 1) throw new ArgumentOutOfRangeException(nameof(maxCollectionItems));

            lock (_sync)
            {
                _entries[id.Trim()] = new Entry
                {
                    Instance = instance,
                    MaxDepth = maxDepth,
                    MaxCollectionItems = maxCollectionItems
                };
            }
        }

        public bool Detach(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            lock (_sync) return _entries.Remove(id.Trim());
        }

        public ApplicationObjectDescriptor Describe(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Object ID is required.", nameof(id));

            Entry entry;
            lock (_sync)
            {
                if (!_entries.TryGetValue(id.Trim(), out entry))
                    throw new ArgumentException("Application object was not found: " + id, nameof(id));
            }

            return _discovery.Describe(entry.Instance, id.Trim(), entry.MaxDepth, entry.MaxCollectionItems);
        }

        public IReadOnlyList<ApplicationObjectDescriptor> DescribeAll()
        {
            List<KeyValuePair<string, Entry>> entries;
            lock (_sync) entries = new List<KeyValuePair<string, Entry>>(_entries);

            var result = new List<ApplicationObjectDescriptor>(entries.Count);
            foreach (var pair in entries)
                result.Add(_discovery.Describe(pair.Value.Instance, pair.Key, pair.Value.MaxDepth, pair.Value.MaxCollectionItems));
            return result.AsReadOnly();
        }
    }
}
