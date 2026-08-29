using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HAgent.WinForms.UI
{
    public sealed partial class WinFormsUiContext
    {
        private IUiSemanticProvider _semanticProvider;

        public void SetSemanticProvider(IUiSemanticProvider provider)
        {
            _semanticProvider = provider;
        }

        public Task<IReadOnlyList<UiSemanticDescriptor>> DiscoverSemanticsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return OnUiAsync(delegate
            {
                if (!Permissions.AutomaticDiscovery)
                    throw new InvalidOperationException("Automatic UI discovery is disabled by the current permission policy.");

                var discovery = new WinFormsSemanticDiscovery();
                return discovery.Discover(_form, Permissions, _semanticProvider);
            });
        }
    }
}
