using System;
using System.Collections.Generic;

namespace HAgent.WinForms.UI
{
    public sealed class UiSemanticDescriptor
    {
        public string ControlId { get; set; }
        public string LogicalName { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
        public string DataRole { get; set; }
        public string DataMember { get; set; }
        public string BindingPath { get; set; }
        public bool Readable { get; set; }
        public bool Writable { get; set; }
        public bool Invokable { get; set; }
        public IReadOnlyDictionary<string, object> Metadata { get; set; }
    }

    public interface IUiSemanticProvider
    {
        UiSemanticDescriptor Describe(System.Windows.Forms.Control control);
    }
}
