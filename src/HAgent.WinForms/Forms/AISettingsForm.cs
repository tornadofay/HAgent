using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.WinForms.Controls;

namespace HAgent.WinForms.Forms
{
    public sealed class AISettingsForm : HAgentForm
    {
        private readonly IAiStore _store;
        private readonly ISecretStore _secrets;
        private readonly IEnumerable<IAiProviderAdapter> _adapters;
        private readonly Panel _content = new Panel();
        private IReadOnlyList<AiProvider> _providers = new List<AiProvider>();
        private IReadOnlyList<AiAgent> _agents = new List<AiAgent>();

        private static readonly Color Navy = Color.FromArgb(15, 23, 42);
        private static readonly Color Surface = Color.FromArgb(248, 250, 252);
        private static readonly Color Muted = Color.FromArgb(100, 116, 139);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        public AISettingsForm(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters)
            : base("AI Configuration", "Providers, agents, and shared AI workspace settings", new Size(1120, 720), new Size(900, 600))
        {
            _store = store;
            _secrets = secrets;
            _adapters = adapters ?? new List<IAiProviderAdapter>();
            BuildShell();
            Shown += async delegate { await ReloadAsync(); };
        }

        private void BuildShell()
        {
            var nav = new Panel
            {
                Dock = DockStyle.Left,
                Width = 188,
                BackColor = Navy,
                Padding = new Padding(10, 18, 10, 10)
            };
            AddNavButton(nav, "Overview", ShowOverview, 0);
            AddNavButton(nav, "Providers", ShowProviders, 1);
            AddNavButton(nav, "Agents", ShowAgents, 2);
            AddNavButton(nav, "About", ShowAbout, 3);

            _content.Dock = DockStyle.Fill;
            _content.BackColor = Surface;
            _content.Padding = new Padding(26);

            BodyPanel.Padding = new Padding(0);
            BodyPanel.Controls.Add(_content);
            BodyPanel.Controls.Add(nav);
        }

        private void AddNavButton(Control host, string text, Action click, int index)
        {
            var b = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = Navy,
                ForeColor = Color.FromArgb(226, 232, 240),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand,
                Padding = new Padding(12, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 6)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += delegate { click(); };
            host.Controls.Add(b);
            if (index == 3) host.Controls.SetChildIndex(b, 0);
        }

        private async Task ReloadAsync()
        {
            _providers = await _store.GetProvidersAsync();
            _agents = await _store.GetAgentsAsync();
            ShowOverview();
        }

        private void ClearContent() { _content.Controls.Clear(); }

        private Panel CreatePage(string title, string description, Action addAction, string addText)
        {
            var page = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            var heading = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Surface };
            var titleLabel = new Label { Text = title, AutoSize = true, Left = 0, Top = 0, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Navy };
            var descLabel = new Label { Text = description, AutoSize = true, Left = 1, Top = 35, ForeColor = Muted, Font = new Font("Segoe UI", 8.8f) };
            heading.Controls.Add(titleLabel);
            heading.Controls.Add(descLabel);
            page.Controls.Add(heading);

            if (addAction != null)
            {
                var actions = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Surface };
                var add = new HFlatButton { Text = addText, Width = 148, Height = 36, Left = 0, Top = 4 };
                add.Click += delegate { addAction(); };
                actions.Controls.Add(add);
                page.Controls.Add(actions);
            }
            return page;
        }

        private void ShowOverview()
        {
            ClearContent();
            var page = CreatePage("Workspace", "A single place to manage providers and agents.", null, null);
            var cards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 116,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Surface,
                Padding = new Padding(0, 6, 0, 0)
            };
            cards.Controls.Add(Card("Providers", _providers.Count.ToString(), "Connection definitions"));
            cards.Controls.Add(Card("Agents", _agents.Count.ToString(), "Configured behaviors"));
            page.Controls.Add(cards);

            var actionRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Surface,
                Padding = new Padding(0, 8, 0, 0)
            };
            var add = new HFlatButton { Text = "+  Add provider", Width = 150, Height = 36, Margin = new Padding(0, 0, 10, 0) };
            add.Click += async delegate { await EditProviderAsync(null); };
            var addAgent = new HFlatButton { Text = "+  Add agent", Width = 138, Height = 36, BackColor = Color.FromArgb(14, 116, 144) };
            addAgent.Click += async delegate { await EditAgentAsync(null); };
            actionRow.Controls.Add(add);
            actionRow.Controls.Add(addAgent);
            page.Controls.Add(actionRow);

            var tip = new Label
            {
                Dock = DockStyle.Top,
                Height = 54,
                Text = "Provider settings describe connectivity and shared defaults. Agent settings describe behavior and can inherit the provider's shared instruction.",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(2, 8, 0, 0)
            };
            page.Controls.Add(tip);
            _content.Controls.Add(page);
        }

        private Control Card(string name, string value, string description)
        {
            var p = new Panel { Width = 250, Height = 96, BackColor = Color.White, Margin = new Padding(0, 0, 14, 0), BorderStyle = BorderStyle.FixedSingle };
            var v = new Label { Text = value, Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = Accent, AutoSize = true, Left = 16, Top = 12 };
            var n = new Label { Text = name, Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Navy, AutoSize = true, Left = 92, Top = 18 };
            var d = new Label { Text = description, Font = new Font("Segoe UI", 8.5f), ForeColor = Muted, AutoSize = true, Left = 92, Top = 43 };
            p.Controls.Add(v); p.Controls.Add(n); p.Controls.Add(d); return p;
        }

        private void ShowProviders()
        {
            ClearContent();
            var page = CreatePage("Providers", "Connection endpoint, authentication, model defaults, and optional shared instructions.", async delegate { await EditProviderAsync(null); }, "+  Add provider");
            var list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                GridLines = false,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f)
            };
            list.Columns.Add("Provider", 210); list.Columns.Add("Type", 155); list.Columns.Add("Model", 180); list.Columns.Add("Agents", 70); list.Columns.Add("Used by", 250); list.Columns.Add("Status", 85);
            foreach (var p in _providers)
            {
                var used = _agents.Where(a => a.ProviderId == p.Id).Select(a => a.Name).ToArray();
                var item = new ListViewItem(new[] { p.Name, p.Kind, p.DefaultModel, used.Length.ToString(), string.Join(", ", used), p.Enabled ? "Enabled" : "Disabled" });
                item.Tag = p;
                list.Items.Add(item);
            }
            list.DoubleClick += async delegate { if (list.SelectedItems.Count > 0) await EditProviderAsync((AiProvider)list.SelectedItems[0].Tag); };
            page.Controls.Add(list);
            _content.Controls.Add(page);
            list.BringToFront();
        }

        private void ShowAgents()
        {
            ClearContent();
            var page = CreatePage("Agents", "Choose a provider and model, then define the agent's behavior with a focused system instruction.", async delegate { await EditAgentAsync(null); }, "+  Add agent");
            var list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                GridLines = false,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f)
            };
            list.Columns.Add("Agent", 220); list.Columns.Add("Provider", 210); list.Columns.Add("Model", 200); list.Columns.Add("Status", 85);
            foreach (var a in _agents)
            {
                var provider = _providers.FirstOrDefault(p => p.Id == a.ProviderId);
                var item = new ListViewItem(new[] { a.Name, provider == null ? "Missing provider" : provider.Name, string.IsNullOrWhiteSpace(a.Model) ? "Provider default" : a.Model, a.Enabled ? "Enabled" : "Disabled" });
                item.Tag = a;
                list.Items.Add(item);
            }
            list.DoubleClick += async delegate { if (list.SelectedItems.Count > 0) await EditAgentAsync((AiAgent)list.SelectedItems[0].Tag); };
            page.Controls.Add(list);
            _content.Controls.Add(page);
            list.BringToFront();
        }

        private void ShowAbout()
        {
            ClearContent();
            var page = CreatePage("About HAgent", "Lightweight AI provider and agent management for desktop .NET applications.", null, null);
            var label = new Label
            {
                Dock = DockStyle.Top,
                Height = 200,
                Text = "HAgent keeps the application API intentionally small. Configure providers and agents once, then call SendAsync(agentId, message) or create a session and use SendAsync + ReadAsync.\r\n\r\nThe default file store uses JSON for structured settings and Windows DPAPI for secret values. SQL Server and MySQL stores are separate adapters so database dependencies are not forced onto every application.",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9.5f),
                Padding = new Padding(2, 8, 0, 0)
            };
            page.Controls.Add(label);
            _content.Controls.Add(page);
        }

        private async Task EditProviderAsync(AiProvider existing)
        {
            var editor = new ProviderEditorForm(existing == null ? new AiProvider() : existing, _secrets);
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                await _store.SaveProviderAsync(editor.Provider);
                await ReloadAsync();
                ShowProviders();
            }
        }

        private async Task EditAgentAsync(AiAgent existing)
        {
            if (_providers.Count == 0)
            {
                MessageBox.Show(this, "Add a provider first. An agent must belong to a provider.", "HAgent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowProviders();
                return;
            }
            var editor = new AgentEditorForm(existing == null ? new AiAgent() : existing, _providers);
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                await _store.SaveAgentAsync(editor.Agent);
                await ReloadAsync();
                ShowAgents();
            }
        }
    }
}
