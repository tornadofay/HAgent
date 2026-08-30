using System.Collections.Generic;
using System.Windows.Forms;

namespace HAgent.WinForms.UI
{
    /// <summary>
    /// Optional runtime adapter for application-owned WinForms controls.
    /// Implementations remain outside HAgent.Core and are never serialized.
    /// </summary>
    public interface IUiControlAdapter
    {
        bool CanAdapt(Control control);
        bool CanRead(Control control);
        bool CanWrite(Control control);
        object ReadValue(Control control);
        void WriteValue(Control control, object value);
        IReadOnlyDictionary<string, object> GetMetadata(Control control);
        string GetLogicalName(Control control);
        string GetDataRole(Control control);
    }
}
