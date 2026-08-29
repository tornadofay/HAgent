using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HAgent.WinForms.UI
{
    public interface IUiContext
    {
        Form RootForm { get; }
        Task<UiControlSnapshot> InspectAsync(string controlId = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<object> ReadControlAsync(string controlId, CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<IReadOnlyDictionary<string, object>>> ReadDataAsync(string controlId, int maxRows = 100, CancellationToken cancellationToken = default(CancellationToken));
    }
}
