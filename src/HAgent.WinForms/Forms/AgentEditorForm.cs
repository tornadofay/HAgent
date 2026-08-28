using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.WinForms.Controls;

namespace HAgent.WinForms.Forms
{
    internal sealed class AgentEditorForm : HAgentForm
    {
        public AiAgent Agent { get; }
        private readonly IReadOnlyList<AiProvider> _providers;
        private readonly ISecretStore _secrets;
        private readonly IReadOnlyList<IAiProviderAdapter> _adapters;
        private readonly TableLayoutPanel _layout = new TableLayoutPanel();
        private readonly TextBox _name = new TextBox();
        private readonly ComboBox _provider = new ComboBox();
        private readonly ComboBox _model = new ComboBox();
        private readonly TextBox _prompt = new TextBox();
        private readonly CheckBox _inherit = new CheckBox();
        private readonly CheckBox _enabled = new CheckBox();
        private readonly NumericUpDown _temperature = new NumericUpDown();
        private readonly NumericUpDown _tokens = new NumericUpDown();
        private readonly Button _test = new HFlatButton();
        private readonly Label _status = new Label();

        public AgentEditorForm(AiAgent agent, IReadOnlyList<AiProvider> providers, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters)
            : base("Agent behavior", "Define this agent's role, provider/model preferences, and runtime settings", new Size(820, 730), new Size(700, 650))
        {
            Agent = agent ?? new AiAgent();
            _providers = providers ?? new List<AiProvider>();
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? new List<IAiProviderAdapter>()).ToList().AsReadOnly();
            Build();
        }

        private void Build()
        {
            BodyPanel.Padding = new Padding(24);
            _layout.Dock = DockStyle.Fill;
            _layout.ColumnCount = 2;
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 195));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _layout.RowCount = 7;
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            _layout.BackColor = Color.FromArgb(248, 250, 252);

            AddField(0, "Name", "How this agent appears in your application.", _name);
            AddField(1, "Provider", "The preferred provider. Provider IDs can also be extended for future fallback/routing.", _provider);
            AddField(2, "Model", "Choose from models reported by the selected provider, or type one manually.", _model);
            AddField(3, "System instruction", "Define role, rules, output style, and task-specific behavior.", _prompt);
            AddCheckField(4, "Instruction inheritance", "Also include the selected provider's shared instruction.", _inherit);
            AddField(5, "Temperature", "Optional sampling control. Empty keeps the provider/model default.", _temperature);
            AddField(6, "Max output tokens", "Optional upper limit for generated output.", _tokens);

            _name.Text = Agent.Name;
            _prompt.Text = Agent.SystemPrompt;
            _prompt.Multiline = true;
            _prompt.ScrollBars = ScrollBars.Vertical;
            _prompt.Height = 126;
            _provider.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (var provider in _providers) _provider.Items.Add(new ProviderItem(provider));
            var selected = _providers.FirstOrDefault(p => p.Id == Agent.ProviderId);
            if (selected != null) SelectProvider(selected.Id);
            else if (_provider.Items.Count > 0) _provider.SelectedIndex = 0;
            _provider.SelectedIndexChanged += async delegate { await LoadModelsAsync(false); };

            _model.DropDownStyle = ComboBoxStyle.DropDown;
            _model.Text = Agent.Model;
            _temperature.DecimalPlaces = 2;
            _temperature.Increment = .05m;
            _temperature.Minimum = 0;
            _temperature.Maximum = 2;
            _temperature.Value = Agent.Temperature.HasValue ? (decimal)Math.Max(0, Math.Min(2, Agent.Temperature.Value)) : 0;
            _tokens.Minimum = 0;
            _tokens.Maximum = 1000000;
            _tokens.Value = Agent.MaxOutputTokens.HasValue ? Agent.MaxOutputTokens.Value : 0;
            _inherit.Text = "Also use the provider's shared instruction";
            _inherit.Checked = Agent.UseProviderSystemPrompt;
            _inherit.AutoSize = true;

            var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.FromArgb(248, 250, 252) };
            var save = new HFlatButton { Text = "Save agent", Width = 130, Height = 36, Margin = new Padding(8, 4, 0, 0) };
            save.Click += delegate { Save(); };
            _test.Text = "Test agent";
            _test.Width = 110;
            _test.Height = 36;
            _test.FlatStyle = FlatStyle.Flat;
            _test.FlatAppearance.BorderSize = 0;
            _test.Margin = new Padding(8, 4, 0, 0);
            _test.Click += async delegate { await TestAsync(); };
            _status.AutoSize = true;
            _status.ForeColor = Color.FromArgb(100, 116, 139);
            _status.Margin = new Padding(8, 15, 12, 0);
            _enabled.Text = "Agent is enabled";
            _enabled.Checked = Agent.Enabled;
            _enabled.AutoSize = true;
            _enabled.Margin = new Padding(0, 10, 8, 0);
            footer.Controls.Add(save);
            footer.Controls.Add(_test);
            footer.Controls.Add(_status);
            footer.Controls.Add(_enabled);

            BodyPanel.Controls.Add(_layout);
            BodyPanel.Controls.Add(footer);
            Shown += async delegate { await LoadModelsAsync(false); };
        }

        private void AddField(int row, string title, string description, Control control)
        {
            _layout.Controls.Add(CreateLabelPanel(title, description), 0, row);
            var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 9, 0, 0) };
            control.Dock = DockStyle.Top;
            control.Height = 30;
            host.Controls.Add(control);
            _layout.Controls.Add(host, 1, row);
        }

        private void AddCheckField(int row, string title, string description, CheckBox control)
        {
            _layout.Controls.Add(CreateLabelPanel(title, description), 0, row);
            var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            host.Controls.Add(control);
            _layout.Controls.Add(host, 1, row);
        }

        private static Panel CreateLabelPanel(string title, string description)
        {
            var p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 12, 0) };
            p.Controls.Add(new Label { Text = description, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.1f), ForeColor = Color.FromArgb(100, 116, 139) });
            p.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42) });
            return p;
        }

        private void SelectProvider(string providerId)
        {
            for (var i = 0; i < _provider.Items.Count; i++)
            {
                var item = _provider.Items[i] as ProviderItem;
                if (item != null && string.Equals(item.Provider.Id, providerId, StringComparison.OrdinalIgnoreCase))
                {
                    _provider.SelectedIndex = i;
                    return;
                }
            }
        }

        private AiProvider SelectedProvider { get { var item = _provider.SelectedItem as ProviderItem; return item == null ? null : item.Provider; } }

        private IAiProviderAdapter FindAdapter(AiProvider provider) { return _adapters.FirstOrDefault(x => provider != null && x.CanHandle(provider)); }

        private async Task<string> GetApiKeyAsync(AiProvider provider, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (provider == null || string.IsNullOrWhiteSpace(provider.SecretId)) return string.Empty;
            return await _secrets.GetAsync(provider.SecretId, cancellationToken).ConfigureAwait(true);
        }

        private async Task LoadModelsAsync(bool showErrors)
        {
            try
            {
                var provider = SelectedProvider;
                var adapter = FindAdapter(provider);
                var catalog = adapter as IProviderModelCatalog;
                if (catalog == null) return;
                var selectedText = _model.Text;
                var models = await catalog.GetModelsAsync(provider, await GetApiKeyAsync(provider), CancellationToken.None).ConfigureAwait(true);
                _model.Items.Clear();
                foreach (var model in models) _model.Items.Add(model);
                _model.Text = selectedText;
                if (models.Count > 0) _status.Text = models.Count + " model(s) available";
            }
            catch (Exception ex)
            {
                if (showErrors) MessageBox.Show(this, ex.Message, "Load models", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task TestAsync()
        {
            try
            {
                var provider = SelectedProvider;
                if (provider == null) throw new InvalidOperationException("Select a provider.");
                var adapter = FindAdapter(provider);
                var tester = adapter as IProviderConnectionTester;
                if (tester == null) throw new InvalidOperationException("This provider adapter does not support connection testing.");
                _test.Enabled = false;
                _status.Text = "Testing…";
                await tester.TestConnectionAsync(provider, await GetApiKeyAsync(provider), CancellationToken.None).ConfigureAwait(true);
                var catalog = adapter as IProviderModelCatalog;
                if (catalog != null && !string.IsNullOrWhiteSpace(_model.Text))
                {
                    var models = await catalog.GetModelsAsync(provider, await GetApiKeyAsync(provider), CancellationToken.None).ConfigureAwait(true);
                    if (models.Count > 0 && !models.Any(x => string.Equals(x, _model.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidOperationException("The provider connection is valid, but model '" + _model.Text.Trim() + "' was not returned by its model catalog.");
                }
                _status.Text = "Agent configuration is valid";
                _status.ForeColor = Color.FromArgb(22, 101, 52);
            }
            catch (Exception ex)
            {
                _status.Text = "Test failed";
                _status.ForeColor = Color.FromArgb(185, 28, 28);
                MessageBox.Show(this, ex.Message, "Agent test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { _test.Enabled = true; }
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_name.Text)) throw new InvalidOperationException("Agent name is required.");
                var provider = SelectedProvider;
                if (provider == null) throw new InvalidOperationException("Select a provider.");
                Agent.Name = _name.Text.Trim();
                Agent.ProviderId = provider.Id;
                if (Agent.ProviderIds == null) Agent.ProviderIds = new List<string>();
                Agent.ProviderIds.RemoveAll(x => string.Equals(x, provider.Id, StringComparison.OrdinalIgnoreCase));
                Agent.Model = _model.Text.Trim();
                Agent.SystemPrompt = _prompt.Text;
                Agent.UseProviderSystemPrompt = _inherit.Checked;
                Agent.Temperature = _temperature.Value == 0 ? (double?)null : (double)_temperature.Value;
                Agent.MaxOutputTokens = _tokens.Value == 0 ? (int?)null : (int)_tokens.Value;
                Agent.Enabled = _enabled.Checked;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Agent", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private sealed class ProviderItem
        {
            public ProviderItem(AiProvider provider) { Provider = provider; }
            public AiProvider Provider { get; private set; }
            public override string ToString() { return Provider.Name; }
        }
    }
}
