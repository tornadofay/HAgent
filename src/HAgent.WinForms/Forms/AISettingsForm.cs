using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.Forms
{
    public sealed class AISettingsForm : HAgentForm
    {
        private readonly IAiStore _store;
        private readonly ISecretStore _secrets;
        private readonly IReadOnlyList<IAiProviderAdapter> _adapters;
        private readonly IToolRegistry _tools;
        private readonly Panel _content = new Panel();
        private IReadOnlyList<AiProvider> _providers = new List<AiProvider>();
        private IReadOnlyList<AiAgent> _agents = new List<AiAgent>();

        private static readonly Color NavigationBackground = Color.FromArgb(31, 24, 69);
        private static readonly Color Surface = Color.FromArgb(248, 248, 252);
        private static readonly Color Muted = Color.FromArgb(100, 92, 120);
        private static readonly Color Heading = Color.FromArgb(31, 24, 69);
        private static readonly Color Accent = Color.FromArgb(116, 76, 210);

        public AISettingsForm(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters, IToolRegistry tools = null)
            : base("AI Configuration", "Providers, agents, tools, and shared AI workspace settings", new Size(1120, 720), new Size(900, 600))
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? new List<IAiProviderAdapter>()).ToList().AsReadOnly();
            _tools = tools ?? new InMemoryToolRegistry();
            BuildShell();
            Shown += async delegate { await ReloadAsync(); };
        }

        private void BuildShell()
        {
            var nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                Width = 188,
                BackColor = NavigationBackground,
                Padding = new Padding(10, 18, 10, 10),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false
            };

            AddNavButton(nav, "Overview", ShowOverview);
            AddNavButton(nav, "Providers", ShowProviders);
            AddNavButton(nav, "Agents", ShowAgents);
            AddNavButton(nav, "Tools", ShowTools);
            AddNavButton(nav, "About", ShowAbout);

            _content.Dock = DockStyle.Fill;
            _content.BackColor = Surface;
            _content.Padding = new Padding(26);

            BodyPanel.Padding = new Padding(0);
            BodyPanel.Controls.Add(_content);
            BodyPanel.Controls.Add(nav);
        }

        private void AddNavButton(Control host, string text, Action click)
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
                ButtonLeaveForeColor = Color.FromArgb(239, 234, 250),
                ButtonLeaveBorderColor = Color.FromArgb(55, 45, 94),
                ButtonEnterBackGroundColor1 = Color.FromArgb(76, 54, 132),
                ButtonEnterBackGroundColor2 = Color.FromArgb(55, 39, 100),
                ButtonEnterForeColor = Color.White,
                ButtonEnterBorderColor = Color.FromArgb(116, 76, 210),
                ButtonDownBackGroundColor1 = Color.FromArgb(61, 43, 110),
                ButtonDownBackGroundColor2 = Color.FromArgb(42, 29, 78),
                ButtonDownForeColor = Color.White,
                ButtonDownBorderColor = Color.FromArgb(104, 76, 170),
                Font = new Font("Segoe UI", 9.5f)
            };
            button.Click += delegate { click(); };
            host.Controls.Add(button);
        }

        private static HButton CreateActionButton(string text, int width, int height, bool destructive = false)
        {
            var button = new HButton
            {
                Text = text,
                Width = width,
                Height = height,
                RoundButton = true,
                Edge = 10,
                TextAlign = ContentAlignment.MiddleCenter,
                TextMargin = 8,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            if (destructive)
            {
                button.ButtonLeaveBackGroundColor1 = Color.FromArgb(183, 61, 89);
                button.ButtonLeaveBackGroundColor2 = Color.FromArgb(119, 38, 62);
                button.ButtonLeaveForeColor = Color.White;
                button.ButtonLeaveBorderColor = Color.FromArgb(207, 80, 105);
                button.ButtonEnterBackGroundColor1 = Color.FromArgb(214, 75, 106);
                button.ButtonEnterBackGroundColor2 = Color.FromArgb(150, 43, 74);
                button.ButtonEnterForeColor = Color.White;
                button.ButtonEnterBorderColor = Color.FromArgb(231, 105, 131);
                button.ButtonDownBackGroundColor1 = Color.FromArgb(151, 45, 70);
                button.ButtonDownBackGroundColor2 = Color.FromArgb(99, 29, 51);
                button.ButtonDownForeColor = Color.White;
                button.ButtonDownBorderColor = Color.FromArgb(190, 63, 91);
            }
            else
            {
                button.ButtonLeaveBackGroundColor1 = Color.FromArgb(92, 67, 168);
                button.ButtonLeaveBackGroundColor2 = Color.FromArgb(57, 40, 108);
                button.ButtonLeaveForeColor = Color.White;
                button.ButtonLeaveBorderColor = Color.FromArgb(116, 76, 210);
                button.ButtonEnterBackGroundColor1 = Color.FromArgb(126, 94, 214);
                button.ButtonEnterBackGroundColor2 = Color.FromArgb(79, 54, 145);
                button.ButtonEnterForeColor = Color.White;
                button.ButtonEnterBorderColor = Color.FromArgb(146, 118, 232);
                button.ButtonDownBackGroundColor1 = Color.FromArgb(72, 52, 132);
                button.ButtonDownBackGroundColor2 = Color.FromArgb(45, 31, 88);
                button.ButtonDownForeColor = Color.White;
                button.ButtonDownBorderColor = Color.FromArgb(104, 79, 176);
            }

            return button;
        }

        private async Task ReloadAsync()
        {
            _providers = await _store.GetProvidersAsync();
            _agents = await _store.GetAgentsAsync();
            ShowOverview();
        }

        private void ClearContent() { _content.Controls.Clear(); }

        private Panel CreatePage(string title, string description, Action addAction, string addText, out Panel listHost)
        {
            var page = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            var heading = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Surface };
            heading.Controls.Add(new Label { Text = title, AutoSize = true, Left = 0, Top = 0, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Heading });
            heading.Controls.Add(new Label { Text = description, AutoSize = true, Left = 1, Top = 35, ForeColor = Muted, Font = new Font("Segoe UI", 8.8f) });

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Surface,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 4, 0, 0)
            };
            if (addAction != null)
            {
                var add = CreateActionButton(addText, 148, 36);
                add.Click += delegate { addAction(); };
                actions.Controls.Add(add);
            }

            listHost = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(0, 4, 0, 0) };
            page.Controls.Add(listHost);
            page.Controls.Add(actions);
            page.Controls.Add(heading);
            return page;
        }

        private void ShowOverview()
        {
            ClearContent();
            Panel unused;
            var page = CreatePage("Workspace", "A single place to manage your AI providers, agents, and tools.", null, null, out unused);
            var cards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 116, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Surface, Padding = new Padding(0, 6, 0, 0) };
            cards.Controls.Add(Card("Providers", _providers.Count.ToString(), "Connection definitions"));
            cards.Controls.Add(Card("Agents", _agents.Count.ToString(), "Configured behaviors"));
            cards.Controls.Add(Card("Tools", _tools.GetDefinitions().Count().ToString(), "Available capabilities"));
            page.Controls.Add(cards);
            _content.Controls.Add(page);
        }

        private Control Card(string name, string value, string description)
        {
            var p = new Panel { Width = 220, Height = 96, BackColor = Color.White, Margin = new Padding(0, 0, 14, 0), BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = Accent, AutoSize = true, Left = 16, Top = 12 });
            p.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Heading, AutoSize = true, Left = 88, Top = 18 });
            p.Controls.Add(new Label { Text = description, Font = new Font("Segoe UI", 8.5f), ForeColor = Muted, AutoSize = true, Left = 88, Top = 43 });
            return p;
        }

        private void ShowProviders()
        {
            ClearContent();
            Panel listHost;
            var page = CreatePage("Providers", "Connection, authentication, model defaults, and optional shared instruction.", async delegate { await EditProviderAsync(null); }, "+  Add provider", out listHost);
            var actions = (FlowLayoutPanel)page.Controls[1];
            var delete = CreateActionButton("Delete selected", 130, 36, true);
            delete.Click += async delegate { await TryDeleteSelectedProviderAsync(listHost); };
            actions.Controls.Add(delete);

            var list = CreateListView();
            list.Columns.Add("Provider", 190);
            list.Columns.Add("Type", 145);
            list.Columns.Add("Model", 180);
            list.Columns.Add("Agents", 65);
            list.Columns.Add("Used by", 260);
            list.Columns.Add("Status", 85);
            foreach (var p in _providers)
            {
                var used = _agents.Where(a => UsesProvider(a, p.Id)).Select(a => a.Name).ToArray();
                var item = new ListViewItem(new[] { p.Name, p.Kind, p.DefaultModel, used.Length.ToString(), string.Join(", ", used), p.Enabled ? "Enabled" : "Disabled" });
                item.Tag = p;
                list.Items.Add(item);
            }
            list.DoubleClick += async delegate { if (list.SelectedItems.Count > 0) await EditProviderAsync((AiProvider)list.SelectedItems[0].Tag); };
            listHost.Controls.Add(list);
            _content.Controls.Add(page);
        }

        private async Task TryDeleteSelectedProviderAsync(Control host)
        {
            var list = host.Controls.OfType<ListView>().FirstOrDefault();
            if (list != null && list.SelectedItems.Count > 0)
                await DeleteProviderAsync((AiProvider)list.SelectedItems[0].Tag);
        }

        private void ShowAgents()
        {
            ClearContent();
            Panel listHost;
            var page = CreatePage("Agents", "Choose providers and models, then define each agent's behavior and runtime settings.", async delegate { await EditAgentAsync(null); }, "+  Add agent", out listHost);
            var actions = (FlowLayoutPanel)page.Controls[1];
            var delete = CreateActionButton("Delete selected", 130, 36, true);
            delete.Click += async delegate { await TryDeleteSelectedAgentAsync(listHost); };
            actions.Controls.Add(delete);

            var list = CreateListView();
            list.Columns.Add("Agent", 220);
            list.Columns.Add("Provider", 180);
            list.Columns.Add("Model", 190);
            list.Columns.Add("Tools", 70);
            list.Columns.Add("Status", 90);
            foreach (var a in _agents)
            {
                var provider = _providers.FirstOrDefault(p => p.Id == a.ProviderId);
                var toolCount = a.ToolIds == null ? 0 : a.ToolIds.Count;
                var item = new ListViewItem(new[] { a.Name, provider == null ? "Missing provider" : provider.Name, string.IsNullOrWhiteSpace(a.Model) ? (provider == null ? "" : provider.DefaultModel) : a.Model, toolCount.ToString(), a.Enabled ? "Enabled" : "Disabled" });
                item.Tag = a;
                list.Items.Add(item);
            }
            list.DoubleClick += async delegate { if (list.SelectedItems.Count > 0) await EditAgentAsync((AiAgent)list.SelectedItems[0].Tag); };
            listHost.Controls.Add(list);
            _content.Controls.Add(page);
        }

        private async Task TryDeleteSelectedAgentAsync(Control host)
        {
            var list = host.Controls.OfType<ListView>().FirstOrDefault();
            if (list != null && list.SelectedItems.Count > 0)
                await DeleteAgentAsync((AiAgent)list.SelectedItems[0].Tag);
        }

        private void ShowTools()
        {
            ClearContent();
            Panel listHost;
            var page = CreatePage("Tools", "Predefined and custom capability definitions. The host application owns actual execution.", async delegate { await EditToolAsync(null); }, "+  Add custom tool", out listHost);
            var actions = (FlowLayoutPanel)page.Controls[1];
            var delete = CreateActionButton("Delete selected", 130, 36, true);
            delete.Click += delegate
            {
                var list = listHost.Controls.OfType<ListView>().FirstOrDefault();
                if (list == null || list.SelectedItems.Count == 0) return;
                var selected = list.SelectedItems[0].Tag as AiTool;
                if (selected == null) return;
                if (selected.IsBuiltIn)
                {
                    HMessage.ShowInformation(this, "Predefined tools are supplied by the host and cannot be deleted here.", "Tool");
                    return;
                }
                if (HMessage.ShowDelete(this, "Delete tool '" + selected.Name + "'?", "Delete tool") != DialogResult.Yes) return;
                _tools.Unregister(selected.Id);
                ShowTools();
            };
            actions.Controls.Add(delete);

            var list = CreateListView();
            list.Columns.Add("Tool", 220);
            list.Columns.Add("Category", 130);
            list.Columns.Add("Kind", 110);
            list.Columns.Add("Status", 90);
            list.Columns.Add("Description", 420);
            foreach (var tool in _tools.GetDefinitions())
            {
                var item = new ListViewItem(new[] { tool.Name, tool.Category, tool.IsBuiltIn ? "Predefined" : "Custom", tool.Enabled ? "Enabled" : "Disabled", tool.Description });
                item.Tag = tool;
                list.Items.Add(item);
            }
            list.DoubleClick += async delegate
            {
                if (list.SelectedItems.Count == 0) return;
                var selected = (AiTool)list.SelectedItems[0].Tag;
                if (selected.IsBuiltIn)
                {
                    HMessage.ShowInformation(this, "This is a predefined tool. Its definition is supplied by the host application and cannot be edited here.", "Tool");
                    return;
                }
                await EditToolAsync(selected);
            };
            list.MouseDoubleClick += async delegate(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left || list.SelectedItems.Count == 0) return;
                var hit = list.HitTest(e.Location).Item;
                if (hit == null) return;
                var selected = hit.Tag as AiTool;
                if (selected != null && !selected.IsBuiltIn) await EditToolAsync(selected);
            };
            listHost.Controls.Add(list);
            _content.Controls.Add(page);
        }

        private static ListView CreateListView()
        {
            return new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, GridLines = false, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f) };
        }

        private static bool UsesProvider(AiAgent agent, string providerId)
        {
            if (agent == null || string.IsNullOrWhiteSpace(providerId)) return false;
            if (string.Equals(agent.ProviderId, providerId, StringComparison.OrdinalIgnoreCase)) return true;
            return agent.ProviderIds != null && agent.ProviderIds.Any(x => string.Equals(x, providerId, StringComparison.OrdinalIgnoreCase));
        }

        private async Task DeleteProviderAsync(AiProvider provider)
        {
            if (provider == null) return;
            var users = _agents.Where(a => UsesProvider(a, provider.Id)).Select(a => a.Name).ToArray();
            if (users.Length > 0)
            {
                HMessage.ShowError(this, "This provider cannot be deleted because these agents still use it:\r\n\r\n" + string.Join(", ", users) + "\r\n\r\nEdit or delete those agents first.", "Provider in use");
                return;
            }

            if (HMessage.ShowDelete(this, "Delete provider '" + provider.Name + "'? This removes its saved configuration.", "Delete provider") != DialogResult.Yes)
                return;

            try
            {
                await _store.DeleteProviderAsync(provider.Id);
                await ReloadAsync();
                ShowProviders();
            }
            catch (Exception ex)
            {
                HMessage.ShowException(this, "The provider could not be deleted.", "Delete provider", ex);
            }
        }

        private async Task DeleteAgentAsync(AiAgent agent)
        {
            if (agent == null) return;
            if (HMessage.ShowDelete(this, "Delete agent '" + agent.Name + "'? Existing running work is not cancelled by this configuration deletion.", "Delete agent") != DialogResult.Yes)
                return;

            try
            {
                await _store.DeleteAgentAsync(agent.Id);
                await ReloadAsync();
                ShowAgents();
            }
            catch (Exception ex)
            {
                HMessage.ShowException(this, "The agent could not be deleted.", "Delete agent", ex);
            }
        }

        private async Task EditProviderAsync(AiProvider existing)
        {
            var editor = new ProviderEditorForm(existing == null ? new AiProvider() : existing, _secrets, _adapters);
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    await _store.SaveProviderAsync(editor.Provider);
                    await ReloadAsync();
                    ShowProviders();
                }
                catch (Exception ex)
                {
                    HMessage.ShowException(this, "The provider could not be saved.", "Provider", ex);
                }
            }
        }

        private async Task EditAgentAsync(AiAgent existing)
        {
            if (_providers.Count == 0)
            {
                HMessage.ShowInformation(this, "Add a provider first. An agent needs at least one provider.", "HAgent");
                ShowProviders();
                return;
            }
            var editor = new AgentEditorForm(existing == null ? new AiAgent() : existing, _providers, _secrets, _adapters);
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    await _store.SaveAgentAsync(editor.Agent);
                    await ReloadAsync();
                    ShowAgents();
                }
                catch (Exception ex)
                {
                    HMessage.ShowException(this, "The agent could not be saved.", "Agent", ex);
                }
            }
        }

        private async Task EditToolAsync(AiTool existing)
        {
            var editor = new ToolEditorForm(existing == null ? new AiTool() : existing);
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _tools.Register(new DelegateAgentTool(editor.Tool, delegate(ToolExecutionContext token)
                    {
                        return Task.FromResult(ToolExecutionResult.Failure("This custom tool has a definition but no execution handler. Register an IAgentTool implementation in the host application."));
                    }));
                    ShowTools();
                }
                catch (Exception ex)
                {
                    HMessage.ShowException(this, "The tool could not be registered.", "Tool", ex);
                }
            }
            await Task.CompletedTask;
        }

        private void ShowAbout()
        {
            ClearContent();
            Panel unused;
            var page = CreatePage("About HAgent", "Lightweight provider and agent management for desktop .NET applications.", null, null, out unused);
            var description = new Label
            {
                Dock = DockStyle.Top,
                Height = 150,
                Text = "HAgent keeps the application API intentionally small. Providers define connections and shared defaults. Agents define behavior and may use multiple providers. Tools describe capabilities that the host application explicitly exposes for execution.\r\n\r\nThe runtime direction includes durable conversation memory, long-term memory, tool execution, agent-to-agent collaboration, routing, and execution policies.",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9.5f),
                Padding = new Padding(2, 8, 0, 0)
            };
            page.Controls.Add(description);

            var github = new LinkLabel
            {
                Text = "GitHub: github.com/tornadofay/HAgent",
                AutoSize = true,
                Dock = DockStyle.Top,
                LinkColor = Accent,
                ActiveLinkColor = Accent,
                VisitedLinkColor = Accent,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Padding = new Padding(2, 6, 0, 0)
            };
            github.LinkClicked += delegate
            {
                try
                {
                    Process.Start(new ProcessStartInfo("https://github.com/tornadofay/HAgent") { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    HMessage.ShowException(this, "The GitHub repository could not be opened.", "GitHub", ex);
                }
            };
            page.Controls.Add(github);
            _content.Controls.Add(page);
        }
    }
}
