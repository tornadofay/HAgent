using System.Collections.Generic;

namespace HAgent.WinForms.UI
{
    public sealed class UiControlSnapshot
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ControlType { get; set; }
        public string Text { get; set; }
        public bool Enabled { get; set; }
        public bool Visible { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string ValueType { get; set; }
        public object Value { get; set; }
        public IReadOnlyList<UiControlSnapshot> Children { get; set; }
    }
}
