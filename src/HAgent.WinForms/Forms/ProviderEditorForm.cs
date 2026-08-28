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
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.Forms
{
    internal sealed class ProviderEditorForm : HAgentForm
    {
        public AiProvider Provider { get; }
        private readonly ISecretStore _secrets;
        private readonly IReadOnlyList<IAiProviderAdapter> _adapters;
        private readonly TableLayoutPanel _layout = new TableLayoutPanel();
        private readonly TextBox _name = new TextBox();
        private readonly ComboBox _kind = new ComboBox();
        private readonly TextBox _baseUrl = new TextBox();
        private readonly TextBox _apiKey = new TextBox();
        private readonly ComboBox _model = new ComboBox();
        private readonly TextBox _prompt = new TextBox();
        private readonly CheckBox _enabled = new CheckBox();
        private readonly HButton _test = new HButton();
        private readonly Label _status = new Label();

        public ProviderEditorForm(AiProvider provider, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters)
            : base("Provider connection", "Define how HAgent connects to an AI service", new Size(800, 690), new Size(680, 620))
        {
            Provider = provider ?? new AiProvider();
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? new List<IAiProviderAdapter>()).ToList().AsReadOnly();
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
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _layout.BackColor = Color.FromArgb(248, 248, 252);

            AddField(0, "Name", "Friendly name shown throughout HAgent.", _name);
            AddField(1, "Provider type", "Adapter responsible for understanding this service API.", _kind);
            AddField(2, "Base URL", "API root, for example https://api.openai.com/v1", _baseUrl);
            AddField(3, "API key", "Stored separately using the configured secret store.", _apiKey);
            AddModelField(4);
            AddField(5, "Shared instruction", "Optional instruction inherited by agents that enable shared provider instructions.", _prompt);

            _name.Text = Provider.Name;
            _baseUrl.Text = Provider.BaseUrl;
            _prompt.Text = Provider.DefaultSystemPrompt;
            _apiKey.UseSystemPasswordChar = true;
            _kind.DropDownStyle = ComboBoxStyle.DropDownList;
            _kind.Items.Add("OpenAI-compatible");
            _kind.SelectedIndex = 0;
            _prompt.Multiline = true;
            _prompt.ScrollBars = ScrollBars.Vertical;
            _prompt.Height = 108;
            _model.DropDownStyle = ComboBoxStyle.DropDown;
            _model.Text = Provider.DefaultModel;

            var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.FromArgb(248, 248, 252) };
            var save = CreateButton("Save provider", 140, 36);
            save.Margin = new Padding(8, 4, 0, 0);
            save.Click += async delegate { await SaveAsync(); };
            ConfigureButton(_test, "Test connection", 130, 36);
            _test.Margin = new Padding(8, 4, 0, 0);
            _test.Click += async delegate { await TestAsync(); };
            _status.AutoSize = true;
            _status.ForeColor = Color.FromArgb(100, 92, 120);
            _status.Margin = new Padding(8, 15, 12, 0);
            _enabled.Text = "Provider is enabled";
            _enabled.Checked = Provider.Enabled;
            _enabled.AutoSize = true;
            _enabled.Margin = new Padding(0, 10, 8, 0);
            footer.Controls.Add(save);
            footer.Controls.Add(_test);
            footer.Controls.Add(_status);
            footer.Controls.Add(_enabled);

            BodyPanel.Controls.Add(_layout);
            BodyPanel.Controls.Add(footer);
        }

        private void AddModelField(int row)
        {
            var labelPanel = CreateLabelPanel("Default model", "Used when an agent does not specify its own model.");
            _layout.Controls.Add(labelPanel, 0, row);
            var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 9, 0, 0) };
            _model.Dock = DockStyle.Fill;
            host.Controls.Add(_model);
            var refresh = CreateButton("Refresh models", 112, 30);
            refresh.Dock = DockStyle.Right;
            refresh.Margin = new Padding(8, 0, 0, 0);
            refresh.Click += async delegate { await LoadModelsAsync(true); };
            host.Controls.Add(refresh);
            _layout.Controls.Add(host, 1, row);
        }

        private static HButton CreateButton(string text, int width, int height)
        {
            var button = new HButton
            {
                Text = text,
                Width = width,
                Height = height,
                RoundButton = true,
                Edge = 10,
                ButtonLeaveBackGroundColor1 = Color.FromArgb(92, 67, 168),
                ButtonLeaveBackGroundColor2 = Color.FromArgb(57, 40, 108),
                ButtonLeaveForeColor = Color.White,
                ButtonLeaveBorderColor = Color.FromArgb(116, 76, 210),
                ButtonEnterBackGroundColor1 = Color.FromArgb(126, 94, 214),
                ButtonEnterBackGroundColor2 = Color.FromArgb(79, 54, 145),
                ButtonEnterForeColor = Color.White,
                ButtonEnterBorderColor = Color.FromArgb(146, 118, 232),
                ButtonDownBackGroundColor1 = Color.FromArgb(72, 52, 132),
                ButtonDownBackGroundColor2 = Color.FromArgb(45, 31, 88),
                ButtonDownForeColor = Color.White,
                ButtonDownBorderColor = Color.FromArgb(104, 79, 176),
                TextMargin = 8
            };
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

        private static Panel CreateLabelPanel(string title, string description)
        {
            var labelPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 12, 0) };
            labelPanel.Controls.Add(new Label { Text = description, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.1f), ForeColor = Color.FromArgb(100, 92, 120) });
            labelPanel.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(31, 24, 69) });
            return labelPanel;
        }

        private IAiProviderAdapter FindAdapter()
        {
            return _adapters.FirstOrDefault(x => x.CanHandle(CreateWorkingProvider()));
        }

        private AiProvider CreateWorkingProvider()
        {
            return new AiProvider
            {
                Id = Provider.Id,
                Name = _name.Text.Trim(),
                Kind = "openai-compatible",
                BaseUrl = _baseUrl.Text.Trim(),
                DefaultModel = _model.Text.Trim(),
                DefaultSystemPrompt = _prompt.Text,
                SecretId = Provider.SecretId,
                Enabled = _enabled.Checked
            };
        }

        private async Task<string> GetApiKeyAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(_apiKey.Text)) return _apiKey.Text;
            if (!string.IsNullOrWhiteSpace(Provider.SecretId)) return await _secrets.GetAsync(Provider.SecretId, cancellationToken).ConfigureAwait(true);
            return string.Empty;
        }

        private async Task TestAsync()
        {
            try
            {
                ValidateConnectionFields();
                var adapter = FindAdapter();
                var tester = adapter as IProviderConnectionTester;
                if (tester == null) throw new InvalidOperationException("This provider adapter does not support connection testing.");
                _test.Enabled = false;
                _status.Text = "Testing…";
                _status.ForeColor = Color.FromArgb(100, 92, 120);
                await tester.TestConnectionAsync(CreateWorkingProvider(), await GetApiKeyAsync(), CancellationToken.None).ConfigureAwait(true);
                _status.Text = "Connection successful";
                _status.ForeColor = Color.FromArgb(22, 101, 52);
                await LoadModelsAsync(false).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _status.Text = "Connection failed";
                _status.ForeColor = Color.FromArgb(185, 28, 28);
                HMessage.ShowException(this, "The provider connection test failed.", "Provider test", ex);
            }
            finally { _test.Enabled = true; }
        }

        private async Task LoadModelsAsync(bool showErrors)
        {
            try
            {
                ValidateConnectionFields();
                var adapter = FindAdapter();
                var catalog = adapter as IProviderModelCatalog;
                if (catalog == null) throw new InvalidOperationException("This provider adapter does not expose a model catalog.");
                var models = await catalog.GetModelsAsync(CreateWorkingProvider(), await GetApiKeyAsync(), CancellationToken.None).ConfigureAwait(true);
                var current = _model.Text;
                _model.Items.Clear();
                foreach (var model in models) _model.Items.Add(model);
                _model.Text = current;
                if (models.Count > 0) _status.Text = models.Count + " model(s) available";
            }
            catch (Exception ex)
            {
                if (showErrors) HMessage.ShowException(this, "The model list could not be loaded.", "Load models", ex);
            }
        }

        private void ValidateConnectionFields()
        {
            if (string.IsNullOrWhiteSpace(_name.Text)) throw new InvalidOperationException("Provider name is required.");
            if (string.IsNullOrWhiteSpace(_baseUrl.Text)) throw new InvalidOperationException("Base URL is required.");
            if (FindAdapter() == null) throw new InvalidOperationException("No installed adapter can handle this provider type.");
        }

        private async Task SaveAsync()
        {
            try
            {
                ValidateConnectionFields();
                Provider.Name = _name.Text.Trim();
                Provider.Kind = "openai-compatible";
                Provider.BaseUrl = _baseUrl.Text.Trim();
                Provider.DefaultModel = _model.Text.Trim();
                Provider.DefaultSystemPrompt = _prompt.Text;
                Provider.Enabled = _enabled.Checked;
                if (string.IsNullOrWhiteSpace(Provider.SecretId)) Provider.SecretId = "provider-" + Provider.Id;
                if (!string.IsNullOrWhiteSpace(_apiKey.Text)) await _secrets.SetAsync(Provider.SecretId, _apiKey.Text).ConfigureAwait(true);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { HMessage.ShowException(this, "The provider could not be saved.", "Provider", ex); }
        }
    }
}