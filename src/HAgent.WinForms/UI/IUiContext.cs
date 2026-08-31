using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;

namespace HAgent.WinForms.UI
{
    public interface IUiContext
    {
        /// <summary>
        /// The attached WinForms root when the attachment target is a Form; otherwise null for a UserControl/control-tree attachment.
        /// </summary>
        Form RootForm { get; }

        /// <summary>
        /// The actual attached control-tree root. This may be a Form, UserControl, or another application-owned Control.
        /// </summary>
        Control RootControl { get; }

        /// <summary>
        /// Stable host-defined logical identity for the attached UI root.
        /// </summary>
        string RootId { get; }

        Task<UiControlSnapshot> InspectAsync(string controlId = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<object> ReadControlAsync(string controlId, CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<IReadOnlyDictionary<string, object>>> ReadDataAsync(string controlId, int maxRows = 100, CancellationToken cancellationToken = default(CancellationToken));
        Task<DataProjectionResult> ProjectDataAsync(string controlId, DataProjectionRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }
}
