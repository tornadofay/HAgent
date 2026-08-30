using System.Collections.Generic;

namespace HAgent.WinForms.UI
{
    public sealed class UiDataRelationshipDescriptor
    {
        public string ControlId { get; set; }
        public string ControlType { get; set; }
        public string RelationshipKind { get; set; }
        public string SourceKind { get; set; }
        public string SourceType { get; set; }
        public string UnderlyingSourceType { get; set; }
        public string BindingPath { get; set; }
        public string DataMember { get; set; }
        public string CurrencyManagerType { get; set; }
        public int? Position { get; set; }
        public string CurrentItemType { get; set; }
        public IReadOnlyList<string> RelatedControlIds { get; set; }
        public IReadOnlyDictionary<string, object> Metadata { get; set; }
    }
}
