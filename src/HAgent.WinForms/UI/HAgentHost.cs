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
            return Attach(form, tools, registerUiTools, null);
        }

        public static HAgentHost Attach(
            Form form,
            IToolRegistry tools,
            bool registerUiTools,
            UiAutomationPermissions permissions)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (tools == null) throw new ArgumentNullException(nameof(tools));

            var rootId = string.IsNullOrWhiteSpace(form.Name) ? form.GetType().FullName : form.Name;
            return Attach((Control)form, rootId, form, tools, registerUiTools, permissions);
        }

        /// <summary>
        /// Attaches HAgent to a UserControl or other WinForms control tree without requiring a Form.
        /// The supplied root ID is the stable logical identity of this attachment.
        /// </summary>
        public static HAgentHost Attach(
            Control rootControl,
            string rootId,
            IToolRegistry tools,
            bool registerUiTools = true,
            UiAutomationPermissions permissions = null)
        {
            if (rootControl == null) throw new ArgumentNullException(nameof(rootControl));
            if (tools == null) throw new ArgumentNullException(nameof(tools));
            if (string.IsNullOrWhiteSpace(rootId)) throw new ArgumentException("Root ID is required.", nameof(rootId));

            return Attach(rootControl, rootId, rootControl as Form, tools, registerUiTools, permissions);
        }

        private static HAgentHost Attach(
            Control rootControl,
            string rootId,
            Form form,
            IToolRegistry tools,
            bool registerUiTools,
            UiAutomationPermissions permissions)
        {
            var context = new WinFormsUiContext(rootControl, rootId, permissions);
            var owner = (IWin32Window)(form ?? rootControl);
            var host = new HAgentHost(owner, tools, context);
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
