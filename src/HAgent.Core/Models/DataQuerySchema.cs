using System;

namespace HAgent.Models
{
    /// <summary>
    /// Authoritative field allow-list for a structured data source.
    /// The schema is host-owned configuration and is independent of any model-generated query request.
    /// </summary>
    public sealed class DataQuerySchema
    {
        private readonly System.Collections.Generic.IReadOnlyDictionary<string, string> _fields;

        public DataQuerySchema(System.Collections.Generic.IEnumerable<string> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            var map = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in fields)
            {
                if (string.IsNullOrWhiteSpace(field))
                    throw new ArgumentException("Schema field names cannot be empty.", nameof(fields));

                if (map.ContainsKey(field))
                    throw new ArgumentException("Schema field names cannot contain duplicates.", nameof(fields));

                map.Add(field, field);
            }

            if (map.Count == 0)
                throw new ArgumentException("At least one schema field is required.", nameof(fields));

            _fields = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(map);
        }

        public System.Collections.Generic.IReadOnlyCollection<string> Fields
        {
            get { return new System.Collections.Generic.List<string>(_fields.Values).AsReadOnly(); }
        }

        public bool Contains(string field)
        {
            return !string.IsNullOrWhiteSpace(field) && _fields.ContainsKey(field);
        }

        public void ValidateRequest(DataQueryRequest request,
            int maximumTake = 1000,
            int maximumSkip = 100000,
            int maximumFilters = 16,
            int maximumSorts = 8)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate(maximumTake, maximumSkip, maximumFilters, maximumSorts);

            foreach (var field in request.Fields)
                EnsureAllowed(field, "project");

            foreach (var filter in request.Filters)
                EnsureAllowed(filter.Field, "filter");

            foreach (var sort in request.Sorts)
                EnsureAllowed(sort.Field, "sort");
        }

        private void EnsureAllowed(string field, string operation)
        {
            if (!Contains(field))
                throw new ArgumentException(
                    "The requested field is not present in the authoritative data-query schema for the " + operation + " operation: " + field,
                    nameof(DataQueryRequest));
        }
    }
}
