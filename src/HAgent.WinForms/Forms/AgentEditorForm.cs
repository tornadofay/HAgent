using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.WinForms.Controls;

namespace HAgent.WinForms.Forms
{
    internal sealed class AgentEditorForm : HAgentForm
    {
        public AiAgent Agent { get; }
        private readonly IReadOnlyList<AiProvider> _providers;
        private readonly TableLayoutPanel _layout = new TableLayoutPanel();
        private readonly TextBox _name = new TextBox();
        private readonly ComboBox _provider = new ComboBox();
        private readonly TextBox _model = new TextBox();
        private readonly TextBox _prompt = new TextBox();
        private readonly CheckBox _inherit = new CheckBox();
        private readonly CheckBox _enabled = new CheckBox();
        private readonly NumericUpDown _temperature = new NumericUpDown();
        private readonly NumericUpDown _tokens = new NumericUpDown();

        public AgentEditorForm(AiAgent agent, IReadOnlyList<AiProvider> providers)
            : base("Agent behavior", "Define this agent's role, model preferences, and runtime settings", new Size(800, 700), new Size(680, 610))
        {
            Agent = agent;
            _providers = providers;
            Build();
        }

        private void Build()
        {
            BodyPanel.Padding = new Padding(24);
            _layout.Dock = DockStyle.Fill;
            _layout.ColumnCount = 2;
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _layout.RowCount = 7;
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            _layout.BackColor = Color.FromArgb(248, 250, 252);

            AddField(0, "Name", "How this agent appears in your application.", _name);
            AddField(1, "Provider", "The connection used by this agent. Multiple agents may share one provider.", _provider);
            AddField(2, "Model", "Optional override. Empty means use the provider's default model.", _model);
            AddField(3, "System instruction", "Define the role, rules, output style, and task-specific behavior of this agent.", _prompt);
            AddField(5, "Temperature", "Optional sampling control. Empty keeps the provider/model default.", _temperature);
            AddField(6, "Max output tokens", "Optional upper limit for generated output.", _tokens);

            _name.Text = Agent.Name;
            _model.Text = Agent.Model;
            _prompt.Text = Agent.SystemPrompt;
            _prompt.Multiline = true;
            _prompt.ScrollBars = ScrollBars.Vertical;
            _prompt.Height = 100;
            _provider.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (var provider in _providers) _provider.Items.Add(new ProviderItem(provider));
            var selected = _providers.FirstOrDefault(p => p.Id == Agent.ProviderId);
            if (selected != null)
            {
                for (var i = 0; i < _provider.Items.Count; i++)
                {
                    var item = _provider.Items[i] as ProviderItem;
                    if (item != null && item.Provider.Id == selected.Id) { _provider.SelectedIndex = i; break; }
                }
            }
            else if (_provider.Items.Count > 0) _provider.SelectedIndex = 0;

            _temperature.DecimalPlaces = 2;
            _temperature.Increment = .05m;
            _temperature.Minimum = 0;
            _temperature.Maximum = 2;
            _temperature.Value = Agent.Temperature.HasValue ? (decimal)Math.Max(0, Math.Min(2, Agent.Temperature.Value)) : 0;
            _tokens.Minimum = 0;
            _tokens.Maximum = 1000000;
            _tokens.Value = Agent.MaxOutputTokens.HasValue ? Agent.MaxOutputTokens.Value : 0;

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            var save = new HFlatButton { Text = "Save agent", Width = 130, Height = 36, Margin = new Padding(8, 4, 0, 0) };
            save.Click += delegate { Save(); };
            _enabled.Text = "Agent is enabled";
            _enabled.Checked = Agent.Enabled;
            _enabled.AutoSize = true;
            _enabled.Margin = new Padding(0, 10, 18, 0);
            _inherit.Text = "Also use the provider's shared system instruction";
            _inherit.Checked = Agent.UseProviderSystemPrompt;
            _inherit.AutoSize = true;
            _inherit.Margin = new Padding(0, 10, 18, 0);
            footer.Controls.Add(save);
            footer.Controls.Add(_enabled);

            var inheritLabel = new Label { Text = "Instruction inheritance", Dock = DockStyle.Fill, Padding = new Padding(0, 12, 0, 0), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42) };
            _layout.Controls.Add(inheritLabel, 0, 4);
            var inheritPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            inheritPanel.Controls.Add(_inherit);
            _layout.Controls.Add(inheritPanel, 1, 4);

            BodyPanel.Controls.Add(_layout);
            BodyPanel.Controls.Add(footer);
        }

        private void AddField(int row, string title, string description, Control control)
        {
            var labelPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 12, 0) };
            var titleLabel = new Label { Text = title, Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42) };
            var descriptionLabel = new Label { Text = description, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.1f), ForeColor = Color.FromArgb(100, 116, 139) };
            labelPanel.Controls.Add(descriptionLabel);
            labelPanel.Controls.Add(titleLabel);
            _layout.Controls.Add(labelPanel, 0, row);

            var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 9, 0, 0) };
            control.Dock = DockStyle.Top;
            control.Height = 30;
            host.Controls.Add(control);
            _layout.Controls.Add(host, 1, row);
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(_name.Text)) { MessageBox.Show(this, "Agent name is required.", "HAgent", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var item = _provider.SelectedItem as ProviderItem;
            if (item == null) { MessageBox.Show(this, "Select a provider.", "HAgent", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            Agent.Name = _name.Text.Trim();
            Agent.ProviderId = item.Provider.Id;
            Agent.Model = _model.Text.Trim();
            Agent.SystemPrompt = _prompt.Text;
            Agent.UseProviderSystemPrompt = _inherit.Checked;
            Agent.Temperature = _temperature.Value == 0 ? (double?)null : (double)_temperature.Value;
            Agent.MaxOutputTokens = _tokens.Value == 0 ? (int?)null : (int)_tokens.Value;
            Agent.Enabled = _enabled.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }

        private sealed class ProviderItem
        {
            public ProviderItem(AiProvider provider) { Provider = provider; }
            public AiProvider Provider { get; private set; }
            public override string ToString() { return Provider.Name; }
        }
    }
}
