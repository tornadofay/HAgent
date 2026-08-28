using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.WinForms.Controls;

namespace HAgent.WinForms.Forms
{
    internal sealed class ProviderEditorForm : HAgentForm
    {
        public AiProvider Provider { get; }
        private readonly ISecretStore _secrets;
        private readonly TableLayoutPanel _layout = new TableLayoutPanel();
        private readonly TextBox _name = new TextBox();
        private readonly ComboBox _kind = new ComboBox();
        private readonly TextBox _baseUrl = new TextBox();
        private readonly TextBox _apiKey = new TextBox();
        private readonly TextBox _model = new TextBox();
        private readonly TextBox _prompt = new TextBox();
        private readonly CheckBox _enabled = new CheckBox();

        public ProviderEditorForm(AiProvider provider, ISecretStore secrets)
            : base("Provider connection", "Define how HAgent connects to an AI service", new Size(760, 650), new Size(650, 580))
        {
            Provider = provider;
            _secrets = secrets;
            Build();
        }

        private void Build()
        {
            BodyPanel.Padding = new Padding(24);
            _layout.Dock = DockStyle.Fill;
            _layout.ColumnCount = 2;
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _layout.RowCount = 7;
            for (var i = 0; i < 6; i++) _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _layout.BackColor = Color.FromArgb(248, 250, 252);

            AddField(0, "Name", "Friendly name shown throughout HAgent.", _name);
            AddField(1, "Provider type", "Adapter responsible for understanding this API.", _kind);
            AddField(2, "Base URL", "API root, for example https://api.openai.com/v1", _baseUrl);
            AddField(3, "API key", "Stored separately with Windows DPAPI; it is not written into settings.json.", _apiKey);
            AddField(4, "Default model", "Used when an agent does not specify its own model.", _model);
            AddField(5, "Shared instruction", "Optional default instruction available to agents that enable inheritance.", _prompt);

            _name.Text = Provider.Name;
            _baseUrl.Text = Provider.BaseUrl;
            _model.Text = Provider.DefaultModel;
            _prompt.Text = Provider.DefaultSystemPrompt;
            _apiKey.UseSystemPasswordChar = true;
            _kind.DropDownStyle = ComboBoxStyle.DropDownList;
            _kind.Items.Add("OpenAI-compatible");
            _kind.SelectedIndex = 0;
            _prompt.Multiline = true;
            _prompt.Height = 58;
            _prompt.ScrollBars = ScrollBars.Vertical;

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            var save = new HFlatButton { Text = "Save provider", Width = 140, Height = 36, Margin = new Padding(8, 4, 0, 0) };
            save.Click += async delegate { await SaveAsync(); };
            _enabled.Text = "Provider is enabled";
            _enabled.Checked = Provider.Enabled;
            _enabled.AutoSize = true;
            _enabled.Margin = new Padding(0, 10, 18, 0);
            footer.Controls.Add(save);
            footer.Controls.Add(_enabled);

            BodyPanel.Controls.Add(_layout);
            BodyPanel.Controls.Add(footer);
        }

        private void AddField(int row, string title, string description, Control control)
        {
            var labelPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 10, 0) };
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

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(_name.Text)) { MessageBox.Show(this, "Provider name is required.", "HAgent", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(_baseUrl.Text)) { MessageBox.Show(this, "Base URL is required.", "HAgent", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            Provider.Name = _name.Text.Trim();
            Provider.Kind = "openai-compatible";
            Provider.BaseUrl = _baseUrl.Text.Trim();
            Provider.DefaultModel = _model.Text.Trim();
            Provider.DefaultSystemPrompt = _prompt.Text;
            Provider.Enabled = _enabled.Checked;
            if (string.IsNullOrWhiteSpace(Provider.SecretId)) Provider.SecretId = "provider-" + Provider.Id;
            if (!string.IsNullOrWhiteSpace(_apiKey.Text)) await _secrets.SetAsync(Provider.SecretId, _apiKey.Text);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
