using System.Collections.Generic;

namespace HAgent.WinForms.UI
{
    public sealed class ApplicationObjectDescriptor
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public IReadOnlyList<ApplicationObjectPropertyDescriptor> Properties { get; set; }
    }

    public sealed class ApplicationObjectPropertyDescriptor
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Kind { get; set; }
        public object Value { get; set; }
        public int? Count { get; set; }
        public string ItemType { get; set; }
        public ApplicationObjectDescriptor Object { get; set; }
        public IReadOnlyList<ApplicationObjectDescriptor> Items { get; set; }
    }
}
