using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.Forms
{
    internal sealed class ConfigurationLauncherForm : HAgentForm
    {
        private readonly IAiStore _store;
        private readonly ISecretStore _secrets;
        private readonly IReadOnlyList<IAiProviderAdapter> _adapters;
        private readonly IToolRegistry _tools;

        public ConfigurationLauncherForm(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters, IToolRegistry tools)
            : base("HAgent Configuration", "Choose the configuration surface you want to manage", new Size(760, 460), new Size(680, 400))
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = new List<IAiProviderAdapter>(adapters ?? new List<IAiProviderAdapter>()).AsReadOnly();
            _tools = tools ?? new InMemoryToolRegistry();
            Build();
        }

        private void Build()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.FromArgb(248, 248, 252),
                Padding = new Padding(36)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            root.Controls.Add(new Label
            {
                Text = "Configuration",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 24, 69),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var choices = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(248, 248, 252)
            };
            choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            choices.Controls.Add(CreateChoice("AI Configuration", "Providers, agents, tools, and AI selection settings.", delegate { OpenAiConfiguration(); }), 0, 0);
            choices.Controls.Add(CreateChoice("Policy", "Unified rules, effective decisions, and agent resource capabilities.", delegate { OpenPolicy(); }), 1, 0);
            root.Controls.Add(choices, 0, 1);

            root.Controls.Add(new Label
            {
                Text = "Policy enforcement remains in HAgent.Core; this window only manages its canonical configuration and inspection surfaces.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = Color.FromArgb(100, 92, 120),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 2);

            var close = CreateButton("Close", 110, 36);
            close.Anchor = AnchorStyles.Right;
            close.Click += delegate { Close(); };
            root.Controls.Add(close, 0, 3);
            BodyPanel.Controls.Add(root);
        }

        private static Control CreateChoice(string title, string description, Action click)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                Padding = new Padding(22),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };
            panel.Controls.Add(new Label { Text = description, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(100, 92, 120), Font = new Font("Segoe UI", 8.8f) });
            panel.Controls.Add(CreateTitleLabel(title));
            var button = CreateButton("Open", 100, 34);
            button.Dock = DockStyle.Bottom;
            button.Click += delegate { click(); };
            panel.Controls.Add(button);
            return panel;
        }

        private static Label CreateTitleLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 34,
                ForeColor = Color.FromArgb(31, 24, 69),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold)
            };
        }

        private void OpenAiConfiguration()
        {
            using (var form = new AISettingsForm(_store, _secrets, _adapters, _tools))
            {
                form.ShowDialog(this);
            }
        }

        private async void OpenPolicy()
        {
            try
            {
                var agents = await _store.GetAgentsAsync().ConfigureAwait(true);
                using (var form = new PolicyEditorForm(_store, agents))
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                HMessage.ShowException(this, "The policy configuration could not be opened.", "HAgent Configuration", ex);
            }
        }

        private static HButton CreateButton(string text, int width, int height)
        {
            var button = new HButton { Text = text, Width = width, Height = height, RoundButton = true, Edge = 10, TextMargin = 8, Font = new Font("Segoe UI", 9.2f, FontStyle.Bold), Cursor = Cursors.Hand };
            button.ButtonLeaveBackGroundColor1 = Color.FromArgb(92, 67, 168);
            button.ButtonLeaveBackGroundColor2 = Color.FromArgb(57, 40, 108);
            button.ButtonLeaveForeColor = Color.White;
            button.ButtonLeaveBorderColor = Color.FromArgb(116, 76, 210);
            button.ButtonEnterBackGroundColor1 = Color.FromArgb(126, 94, 214);
            button.ButtonEnterBackGroundColor2 = Color.FromArgb(79, 54, 145);
            button.ButtonEnterForeColor = Color.White;
            button.ButtonEnterBorderColor = Color.FromArgb(146, 118, 232);
            return button;
        }
    }
}
