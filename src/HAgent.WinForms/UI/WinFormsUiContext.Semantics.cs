using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.WinForms.UI
{
    public sealed partial class WinFormsUiContext
    {
        private readonly IUiSemanticProvider _semanticProvider;

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
                var discovered = discovery.Discover(_form, Permissions);
                if (_semanticProvider == null)
                    return discovered;

                var result = new List<UiSemanticDescriptor>(discovered.Count);
                foreach (var item in discovered)
                {
                    var control = FindControlForSemantic(item.ControlId);
                    var custom = control == null ? null : _semanticProvider.Describe(control);
                    result.Add(custom ?? item);
                }
                return (IReadOnlyList<UiSemanticDescriptor>)result.AsReadOnly();
            });
        }

        private Control FindControlForSemantic(string controlId)
        {
            if (string.IsNullOrWhiteSpace(controlId)) return null;
            return FindControl(_form, controlId);
        }
    }
}
