using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.WinForms.Controls;
using HAgent.WinForms.UI.Configuration;
using HAgent.WinForms.UI.Configuration.About;
using HAgent.WinForms.UI.Configuration.Agents;
using HAgent.WinForms.UI.Configuration.Intervention;
using HAgent.WinForms.UI.Configuration.Overview;
using HAgent.WinForms.UI.Configuration.Policy;
using HAgent.WinForms.UI.Configuration.Providers;
using HAgent.WinForms.UI.Configuration.Tools;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.Forms
{
    public sealed class AISettingsForm : HAgentForm
    {
        private static readonly Color NavigationBackground = Color.FromArgb(31, 24, 69);
        private static readonly Color Surface = Color.FromArgb(248, 248, 252);
        private static readonly Color NavigationText = Color.FromArgb(239, 234, 250);

        private readonly ConfigurationContext _context;
        private readonly Panel _content = new Panel();
        private readonly FlowLayoutPanel _navigation = new FlowLayoutPanel();
        private readonly Dictionary<string, Func<Control>> _pages = new Dictionary<string, Func<Control>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Control> _navigationButtons = new Dictionary<string, Control>(StringComparer.OrdinalIgnoreCase);
        private readonly OverviewPage _overview;
        private readonly ProvidersPage _providers;
        private readonly AgentsPage _agents;
        private readonly ToolsPage _tools;
        private readonly PolicyPage _policy;
        private readonly InterventionPage _interventions;

        public AISettingsForm(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IToolRegistry tools = null,
            IAiInterventionWorkflow interventionWorkflow = null,
            AiInterventionCoordinator interventionCoordinator = null,
            AgentIdentityContext currentIdentity = null)
            : base("AI Configuration", "Providers, agents, tools, policy, interventions, permissions, and storage", new Size(1120, 720), new Size(900, 600))
        {
            _context = new ConfigurationContext(store, secrets, adapters, tools, interventionWorkflow, interventionCoordinator, currentIdentity);
            _overview = new OverviewPage(_context);
            _providers = new ProvidersPage(_context);
            _agents = new AgentsPage(_context);
            _tools = new ToolsPage(_context);
            _policy = new PolicyPage(_context);
            _interventions = new InterventionPage(_context);
            BuildShell();
            RegisterPages();
            Shown += async delegate { await ReloadAsync(); };
        }

        private void BuildShell()
        {
            _navigation.Dock = DockStyle.Left;
            _navigation.Width = 188;
            _navigation.BackColor = NavigationBackground;
            _navigation.Padding = new Padding(10, 18, 10, 10);
            _navigation.FlowDirection = FlowDirection.TopDown;
            _navigation.WrapContents = false;
            _navigation.AutoScroll = true;

            _content.Dock = DockStyle.Fill;
            _content.BackColor = Surface;
            _content.Padding = new Padding(26);

            BodyPanel.Padding = new Padding(0);
            BodyPanel.Controls.Add(_content);
            BodyPanel.Controls.Add(_navigation);
        }

        private void RegisterPages()
        {
            RegisterPage("Overview", delegate { return _overview; });
            RegisterPage("Providers", delegate { return _providers; });
            RegisterPage("Agents", delegate { return _agents; });
            RegisterPage("Tools", delegate { return _tools; });
            RegisterPage("Policy", delegate { return _policy; });
            RegisterPage("Interventions", delegate { return _interventions; });

            RegisterAction("Permissions", delegate
            {
                using (var form = new UiPermissionsForm(AISettings.LoadUiPermissions()))
                {
                    form.ShowDialog(this);
                }
            });

            RegisterAction("Storage", delegate
            {
                using (var form = new HAgentStorageSettingsForm(AppContext.BaseDirectory, ProcessName(), _context.Secrets))
                {
                    form.ShowDialog(this);
                    if (form.RuntimeStorageChanged && !IsDisposed) Close();
                }
            });

            RegisterAction("Storage Test", delegate
            {
                using (var form = new HAgentStorageConnectionTestForm(AppContext.BaseDirectory, _context.Secrets))
                    form.ShowDialog(this);
            });

            RegisterPage("About", delegate { return new AboutPage(); });
        }

        private void RegisterPage(string name, Func<Control> factory)
        {
            _pages[name] = factory;
            AddNavigationButton(name, delegate { ShowPage(name); });
        }

        private void RegisterAction(string name, Action action)
        {
            AddNavigationButton(name, action);
        }

        private void AddNavigationButton(string text, Action action)
        {
            var button = CreateNavigationButton(text);
            button.Click += delegate { action(); };
            _navigationButtons[text] = button;
            _navigation.Controls.Add(button);
        }

        private void ShowPage(string name)
        {
            Func<Control> factory;
            if (!_pages.TryGetValue(name, out factory)) return;
            _content.Controls.Clear();
            var page = factory();
            page.Dock = DockStyle.Fill;
            _content.Controls.Add(page);
            Control active;
            if (_navigationButtons.TryGetValue(name, out active))
                active.Focus();
        }

        private async Task ReloadAsync()
        {
            _context.Providers = await _context.Store.GetProvidersAsync();
            _context.Agents = await _context.Store.GetAgentsAsync();
            _overview.RefreshData();
            _providers.RefreshData();
            _agents.RefreshData();
            _tools.RefreshData();
            await _interventions.RefreshDataAsync();
            ShowPage("Overview");
        }

        private static string ProcessName()
        {
            var value = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            return string.IsNullOrWhiteSpace(value) ? "HAgent" : value;
        }

        private static HButton CreateNavigationButton(string text)
        {
            var button = new HButton
            {
                Text = text,
                Width = 166,
                Height = 42,
                RoundButton = true,
                Edge = 10,
                TextAlign = ContentAlignment.MiddleLeft,
                TextMargin = 16,
                Margin = new Padding(0, 0, 0, 6),
                Cursor = Cursors.Hand,
                ButtonLeaveBackGroundColor1 = NavigationBackground,
                ButtonLeaveBackGroundColor2 = Color.FromArgb(25, 20, 54),
                ButtonLeaveForeColor = NavigationText,
                ButtonLeaveBorderColor = Color.FromArgb(55, 45, 94),
                ButtonEnterBackGroundColor1 = Color.FromArgb(76, 54, 132),
                ButtonEnterBackGroundColor2 = Color.FromArgb(55, 39, 100),
                ButtonEnterForeColor = Color.White,
                ButtonEnterBorderColor = Color.FromArgb(116, 76, 210),
                ButtonDownBackGroundColor1 = Color.FromArgb(61, 43, 110),
                ButtonDownBackGroundColor2 = Color.FromArgb(42, 29, 78),
                ButtonDownForeColor = Color.White,
                ButtonDownBorderColor = Color.FromArgb(104, 76, 176),
                Font = new Font("Segoe UI", 9.5f)
            };
            return button;
        }
    }
}
