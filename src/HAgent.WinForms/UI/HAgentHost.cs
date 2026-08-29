using System;
using System.Windows.Forms;
using HAgent.Abstractions;

namespace HAgent.WinForms.UI
{
    public sealed class HAgentHost : IDisposable
    {
        private readonly IToolRegistry _tools;
        private readonly WinFormsUiContext _context;
        private bool _disposed;

        private HAgentHost(IWin32Window owner, IToolRegistry tools, WinFormsUiContext context)
        {
            Owner = owner;
            _tools = tools;
            _context = context;
        }

        public IWin32Window Owner { get; private set; }
        public IUiContext Context { get { return _context; } }

        public static HAgentHost Attach(Form form, IToolRegistry tools, bool registerUiTools = true)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (tools == null) throw new ArgumentNullException(nameof(tools));

            var context = new WinFormsUiContext(form);
            var host = new HAgentHost(form, tools, context);
            if (registerUiTools) WinFormsUiTools.RegisterDefaultTools(tools, context);
            return host;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _context.Dispose();
        }
    }
}
