using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.WinForms;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.Forms
{
    internal sealed class AgentEditorForm : HAgentForm
    {
        public AiAgent Agent { get; }
        private readonly IReadOnlyList<AiProvider> _providers;
        private readonly ISecretStore _secrets;
        private readonly IReadOnlyList<IAiProviderAdapter> _adapters;
        private readonly IReadOnlyList<AiTool> _tools;
        private readonly TableLayoutPanel _layout = new TableLayoutPanel();
        private readonly TextBox _name = new TextBox();
        private readonly ComboBox _provider = new ComboBox();
        private readonly ComboBox _model = new ComboBox();
        private readonly TextBox _prompt = new TextBox();
        private readonly CheckBox _inherit = new CheckBox();
        private readonly CheckBox _enabled = new CheckBox();
        private readonly NumericUpDown _temperature = new NumericUpDown();
        private readonly NumericUpDown _tokens = new NumericUpDown();
        private readonly CheckedListBox _toolList = new CheckedListBox();
        private readonly HButton _test = new HButton();
        private readonly Label _status = new Label();

        public AgentEditorForm(
            AiAgent agent,
            IReadOnlyList<AiProvider> providers,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IEnumerable<AiTool> tools = null)
            : base("Agent behavior", "Define this agent's role, provider/model preferences, tools, and runtime settings", new Size(860, 790), new Size(720, 680))
        {
            Agent = agent ?? new AiAgent();
            _providers = providers ?? new List<AiProvider>();
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? new List<IAiProviderAdapter>()).ToList().AsReadOnly();
            _tools = (tools ?? new List<AiTool>()).Where(x => x != null).ToList().AsReadOnly();
            Build();
        }

        private void Build()
        {
            BodyPanel.Padding = new Padding(24);
            _layout.Dock = DockStyle.Fill;
            _layout.ColumnCount = 2;
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 195));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _layout.RowCount = 8;
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            _layout.BackColor = Color.FromArgb(248, 248, 252);

            AddField(0, "Name", "How this agent appears in your application.", _name);
            AddField(1, "Provider", "The preferred provider. Provider IDs can also be extended for future fallback/routing.", _provider);
            AddField(2, "Model", "Choose from models reported by the selected provider, or type one manually.", _model);
            AddField(3, "System instruction", "Define role, rules, output style, and task-specific behavior.", _prompt);
            AddCheckField(4, "Instruction inheritance", "Also include the selected provider's shared instruction.", _inherit);
            AddField(5, "Temperature", "Optional sampling control. Empty keeps the provider/model default.", _temperature);
            AddField(6, "Max output tokens", "Optional upper limit for generated output.", _tokens);
            AddToolField(7);

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
            _provider.SelectedIndexChanged += async delegate { await LoadModelsAsync(); };

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

            var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.FromArgb(248, 248, 252) };
            var save = CreateButton("Save agent", 130, 36);
            save.Margin = new Padding(8, 4, 0, 0);
            save.Click += delegate { Save(); };
            ConfigureButton(_test, "Test agent", 110, 36);
            _test.Margin = new Padding(8, 4, 0, 0);
            _test.Click += async delegate { await TestAsync(); };
            _status.AutoSize = true;
            _status.ForeColor = Color.FromArgb(100, 92, 120);
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
            Shown += async delegate { await LoadModelsAsync(); };
        }

        private static HButton CreateButton(string text, int width, int height)
        {
            var button = new HButton { Text = text, Width = width, Height = height, RoundButton = true, Edge = 10, TextMargin = 8 };
            ConfigureButton(button, text, width, height);
            return button;
        }

        private static void ConfigureButton(HButton button, string text, int width, int height)
        {
            button.Text = text;
            button.Width = width;
            button.Height = height;
            button.RoundButton = true;
            button.Edge = 10;
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
            button.TextMargin = 8;
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

        private void AddToolField(int row)
        {
            _layout.Controls.Add(CreateLabelPanel("Tools", "Select the capabilities this agent is allowed to request. Assignment controls availability; handler registration controls execution."), 0, row);
            var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0) };
            _toolList.Dock = DockStyle.Fill;
            _toolList.BorderStyle = BorderStyle.FixedSingle;
            _toolList.CheckOnClick = true;
            _toolList.Font = new Font("Segoe UI", 8.8f);
            _toolList.BackColor = Color.White;
            _toolList.ForeColor = Color.FromArgb(68, 62, 88);

            var assigned = new HashSet<string>(Agent.ToolIds ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            foreach (var tool in _tools.OrderBy(x => x.Name))
            {
                var index = _toolList.Items.Add(new ToolItem(tool));
                _toolList.SetItemChecked(index, assigned.Contains(tool.Id));
            }

            host.Controls.Add(_toolList);
            _layout.Controls.Add(host, 1, row);
        }

        private static Panel CreateLabelPanel(string title, string description)
        {
            var p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 12, 0) };
            p.Controls.Add(new Label { Text = description, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.1f), ForeColor = Color.FromArgb(100, 92, 120) });
            p.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(31, 24, 69) });
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

        private async Task LoadModelsAsync()
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
                await ShowCapabilitiesAsync(provider, adapter, _model.Text).ConfigureAwait(true);
            }
            catch { }
        }

        private async Task ShowCapabilitiesAsync(AiProvider provider, IAiProviderAdapter adapter, string model)
        {
            var capabilityAdapter = adapter as IProviderModelCapabilities;
            if (capabilityAdapter == null)
            {
                _status.Text = "Capabilities: unavailable";
                return;
            }
            if (string.IsNullOrWhiteSpace(model))
            {
                _status.Text = "Capabilities: select a model";
                return;
            }
            var capabilities = await capabilityAdapter.GetCapabilitiesAsync(provider, model.Trim(), await GetApiKeyAsync(provider), CancellationToken.None).ConfigureAwait(true);
            _status.Text = CapabilityDisplay.BuildSummary(capabilities);
            CapabilityDisplay.AttachToolTip(_status, capabilities);
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
                await ShowCapabilitiesAsync(provider, adapter, _model.Text).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _status.Text = "Test failed";
                _status.ForeColor = Color.FromArgb(185, 28, 28);
                HMessage.ShowException(this, "The agent test failed.", "Agent test", ex);
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
                Agent.ToolIds = new List<string>();
                foreach (var item in _toolList.CheckedItems)
                {
                    var tool = item as ToolItem;
                    if (tool != null && !string.IsNullOrWhiteSpace(tool.Tool.Id)) Agent.ToolIds.Add(tool.Tool.Id);
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { HMessage.ShowException(this, "The agent could not be saved.", "Agent", ex); }
        }

        private sealed class ProviderItem
        {
            public ProviderItem(AiProvider provider) { Provider = provider; }
            public AiProvider Provider { get; private set; }
            public override string ToString() { return Provider.Name; }
        }

        private sealed class ToolItem
        {
            public ToolItem(AiTool tool) { Tool = tool; }
            public AiTool Tool { get; private set; }
            public override string ToString() { return Tool.Name + "  —  " + Tool.Description; }
        }
    }
}