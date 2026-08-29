using System.Collections.Generic;

namespace HAgent.WinForms.UI
{
    public sealed class UiDataSourceDescriptor
    {
        public string ControlId { get; set; }
        public string ControlType { get; set; }
        public string SourceType { get; set; }
        public string DataMember { get; set; }
        public string ItemType { get; set; }
        public int? Count { get; set; }
        public IReadOnlyList<string> FieldNames { get; set; }
        public IReadOnlyDictionary<string, object> Metadata { get; set; }
    }
}