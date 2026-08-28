using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Helpers;

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

        private static readonly Color Navy = Color.FromArgb(31, 24, 69);
        private static readonly Color Surface = Color.FromArgb(248, 248, 252);
        private static readonly Color Muted = Color.FromArgb(100, 92, 120);
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
                BackColor = Navy,
                Padding = new Padding(10, 18, 10, 10),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false
            };

            AddNavButton(nav, "Overview", ShowOverview);
            AddNavButton(nav, "Agents", ShowAgents);
            AddNavButton(nav, "Providers", ShowProviders);
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
            var b = new Button
            {
                Text = text,
                Width = 166,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = Navy,
                ForeColor = Color.FromArgb(239, 234, 250),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand,
                Padding = new Padding(12, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 6)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += delegate { click(); };
            host.Controls.Add(b);
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
            heading.Controls.Add(new Label { Text = title, AutoSize = true, Left = 0, Top = 0, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Navy });
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
                var add = new HFlatButton { Text = addText, Width = 148, Height = 36, Margin = new Padding(0, 0, 8, 0) };
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
            cards.Controls.Add(Card("Agents", _agents.Count.ToString(), "Configured behaviors"));
            cards.Controls.Add(Card("Providers", _providers.Count.ToString(), "Connection definitions"));
            cards.Controls.Add(Card("Tools", _tools.GetDefinitions().Count().ToString(), "Available capabilities"));
            page.Controls.Add(cards);
            _content.Controls.Add(page);
        }

        private Control Card(string name, string value, string description)
        {
            var p = new Panel { Width = 220, Height = 96, BackColor = Color.White, Margin = new Padding(0, 0, 14, 0), BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = Accent, AutoSize = true, Left = 16, Top = 12 });
            p.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Navy, AutoSize = true, Left = 88, Top = 18 });
            p.Controls.Add(new Label { Text = description, Font = new Font("Segoe UI", 8.5f), ForeColor = Muted, AutoSize = true, Left = 88, Top = 43 });
            return p;
        }

        private void ShowProviders()
        {
            ClearContent();
            Panel listHost;
            var page = CreatePage("Providers", "Connection, authentication, model defaults, and optional shared instruction.", async delegate { await EditProviderAsync(null); }, "+  Add provider", out listHost);

            var actions = (FlowLayoutPanel)page.Controls[1];
            var delete = new HFlatButton { Text = "Delete selected", Width = 130, Height = 36, BackColor = Color.FromArgb(185, 28, 28) };
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
            delete.Click += async delegate { if (list.SelectedItems.Count > 0) await DeleteProviderAsync((AiProvider)list.SelectedItems[0].Tag); };
            listHost.Controls.Add(list);
            _content.Controls.Add(page);
        }

        private void ShowAgents()
        {
            ClearContent();
            Panel listHost;
            var page = CreatePage("Agents", "Choose providers and models, then define each agent's behavior and runtime settings.", async delegate { await EditAgentAsync(null); }, "+  Add agent", out listHost);
            var actions = (FlowLayoutPanel)page.Controls[1];
            var delete = new HFlatButton { Text = "Delete selected", Width = 130, Height = 36, BackColor = Color.FromArgb(185, 28, 28) };
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
            delete.Click += async delegate { if (list.SelectedItems.Count > 0) await DeleteAgentAsync((AiAgent)list.SelectedItems[0].Tag); };
            listHost.Controls.Add(list);
            _content.Controls.Add(page);
        }

        private void ShowTools()
        {
            ClearContent();
            Panel listHost;
            var page = CreatePage("Tools", "Predefined and custom capability definitions. The host application owns actual execution.", async delegate { await EditToolAsync(null); }, "+  Add custom tool", out listHost);
            var list = CreateListView();
            list.Columns.Add("Tool", 220); list.Columns.Add("Category", 130); list.Columns.Add("Kind", 110); list.Columns.Add("Status", 90); list.Columns.Add("Description", 420);
            foreach (var tool in _tools.GetDefinitions())
            {
                var item = new ListViewItem(new[] { tool.Name, tool.Category, tool.IsBuiltIn ? "Predefined" : "Custom", tool.Enabled ? "Enabled" : "Disabled", tool.Description });
                item.Tag = tool;
                list.Items.Add(item);
            }
            list.DoubleClick += async delegate { if (list.SelectedItems.Count > 0) { var selected = (AiTool)list.SelectedItems[0].Tag; if (!selected.IsBuiltIn) await EditToolAsync(selected); } };
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
            page.Controls.Add(new Label { Dock = DockStyle.Top, Height = 200, Text = "HAgent keeps the application API intentionally small. Providers define connections and shared defaults. Agents define behavior and may use multiple providers. Tools describe capabilities that the host application explicitly exposes for execution.\r\n\r\nThe runtime direction includes durable conversation memory, long-term memory, tool execution, agent-to-agent collaboration, routing, and execution policies.", ForeColor = Muted, Font = new Font("Segoe UI", 9.5f), Padding = new Padding(2, 8, 0, 0) });
            _content.Controls.Add(page);
        }
    }
}
