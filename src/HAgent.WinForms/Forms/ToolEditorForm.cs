using System;
using System.Drawing;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.Forms
{
    internal sealed class ToolEditorForm : HAgentForm
    {
        public AiTool Tool { get; private set; }
        private readonly TextBox _name = new TextBox();
        private readonly TextBox _description = new TextBox();
        private readonly ComboBox _type = new ComboBox();
        private readonly TextBox _category = new TextBox();
        private readonly TextBox _schema = new TextBox();
        private readonly CheckBox _enabled = new CheckBox();
        private readonly Label _typeHint = new Label();

        public ToolEditorForm(AiTool tool)
            : base("Tool definition", "Describe a capability that an agent can request from the host application", new Size(820, 700), new Size(700, 620))
        {
            Tool = tool ?? new AiTool();
            Build();
        }

        private void Build()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5, BackColor = Color.FromArgb(248, 248, 252) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            AddField(layout, 0, "Name", "Stable tool name exposed to the model.", _name);
            AddField(layout, 1, "Description", "Explain what the tool does and when the agent should use it.", _description);
            AddTypeField(layout, 2);
            AddField(layout, 3, "Category", "Human-facing grouping such as UI, Database, or Application.", _category);
            AddField(layout, 4, "Input schema", "JSON Schema describing the arguments the model may send to the tool.", _schema);

            _name.Text = Tool.Name;
            _description.Text = Tool.Description;
            _category.Text = Tool.Category;
            _schema.Text = Tool.InputSchemaJson;
            _description.Multiline = true;
            _description.ScrollBars = ScrollBars.Vertical;
            _schema.Multiline = true;
            _schema.ScrollBars = ScrollBars.Both;
            _schema.Font = new Font("Consolas", 9f);

            _enabled.Text = "Tool definition is enabled";
            _enabled.Checked = Tool.Enabled;
            _enabled.AutoSize = true;
            _enabled.Margin = new Padding(0, 12, 10, 0);

            var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.FromArgb(248, 248, 252) };
            var save = CreateButton("Save tool", 120, 36);
            save.Margin = new Padding(8, 4, 0, 0);
            save.Click += delegate { Save(); };
            footer.Controls.Add(save);
            footer.Controls.Add(_enabled);

            BodyPanel.Padding = new Padding(24);
            BodyPanel.Controls.Add(layout);
            BodyPanel.Controls.Add(footer);
            UpdateTypeHint();
        }

        private void AddTypeField(TableLayoutPanel layout, int row)
        {
            var labels = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 12, 0) };
            labels.Controls.Add(new Label
            {
                Text = "How the implementation is supplied. This is separate from the human-facing Category.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.1f),
                ForeColor = Color.FromArgb(100, 92, 120)
            });
            labels.Controls.Add(new Label
            {
                Text = "Tool type",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 24, 69)
            });
            layout.Controls.Add(labels, 0, row);

            var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            _type.DropDownStyle = ComboBoxStyle.DropDownList;
            _type.Dock = DockStyle.Top;
            _type.Height = 30;

            AddType(AiToolType.BuiltIn, "Built-in");
            AddType(AiToolType.Application, "Application tool");
            AddType(AiToolType.Declarative, "Declarative tool");
            AddType(AiToolType.UI, "UI tool");
            AddType(AiToolType.SqlServer, "SQL Server tool");
            AddType(AiToolType.MySql, "MySQL tool");

            var selectedType = Tool.Type;
            if (Tool.IsBuiltIn && selectedType == AiToolType.Application)
                selectedType = AiToolType.BuiltIn;
            SelectType(selectedType);
            _type.SelectedIndexChanged += delegate { UpdateTypeHint(); };

            _typeHint.Text = "";
            _typeHint.Dock = DockStyle.Top;
            _typeHint.Height = 28;
            _typeHint.Font = new Font("Segoe UI", 8.2f);
            _typeHint.ForeColor = Color.FromArgb(100, 92, 120);
            _typeHint.Padding = new Padding(1, 5, 0, 0);

            host.Controls.Add(_typeHint);
            host.Controls.Add(_type);
            layout.Controls.Add(host, 1, row);
        }

        private void SelectType(AiToolType type)
        {
            for (var i = 0; i < _type.Items.Count; i++)
            {
                var item = _type.Items[i] as ToolTypeItem;
                if (item != null && item.Type == type)
                {
                    _type.SelectedIndex = i;
                    return;
                }
            }
            if (_type.Items.Count > 1) _type.SelectedIndex = 1;
        }

        private void AddType(AiToolType type, string name)
        {
            _type.Items.Add(new ToolTypeItem(type, name));
        }

        private void UpdateTypeHint()
        {
            var item = _type.SelectedItem as ToolTypeItem;
            if (item == null)
            {
                _typeHint.Text = string.Empty;
                return;
            }

            switch (item.Type)
            {
                case AiToolType.BuiltIn:
                    _typeHint.Text = "Implemented by HAgent itself; no application handler is supplied in this form.";
                    break;
                case AiToolType.Application:
                    _typeHint.Text = "The application supplies the executable handler through HAgent's tool registry.";
                    break;
                case AiToolType.Declarative:
                    _typeHint.Text = "Configured behavior backed by a safe declarative operation; it is not arbitrary code execution.";
                    break;
                case AiToolType.UI:
                    _typeHint.Text = "Implemented by HAgent.WinForms control adapters and UI capability handlers.";
                    break;
                case AiToolType.SqlServer:
                    _typeHint.Text = "Implemented by the SQL Server tool layer with explicit query/operation restrictions.";
                    break;
                case AiToolType.MySql:
                    _typeHint.Text = "Implemented by the MySQL tool layer with explicit query/operation restrictions.";
                    break;
                default:
                    _typeHint.Text = string.Empty;
                    break;
            }
        }

        private sealed class ToolTypeItem
        {
            public ToolTypeItem(AiToolType type, string name) { Type = type; Name = name; }
            public AiToolType Type { get; private set; }
            private string Name { get; set; }
            public override string ToString() { return Name; }
        }

        private static HButton CreateButton(string text, int width, int height)
        {
            return new HButton
            {
                Text = text,
                Width = width,
                Height = height,
                RoundButton = true,
                Edge = 10,
                TextMargin = 8,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
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
                ButtonDownBorderColor = Color.FromArgb(104, 79, 176)
            };
        }

        private static void AddField(TableLayoutPanel layout, int row, string title, string description, Control control)
        {
            var labels = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 12, 0) };
            labels.Controls.Add(new Label { Text = description, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.1f), ForeColor = Color.FromArgb(100, 92, 120) });
            labels.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(31, 24, 69) });
            layout.Controls.Add(labels, 0, row);
            var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 9, 0, 0) };
            control.Dock = DockStyle.Fill;
            host.Controls.Add(control);
            layout.Controls.Add(host, 1, row);
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_name.Text)) throw new InvalidOperationException("Tool name is required.");
                if (string.IsNullOrWhiteSpace(_description.Text)) throw new InvalidOperationException("Tool description is required.");
                if (string.IsNullOrWhiteSpace(_schema.Text)) throw new InvalidOperationException("Input schema is required.");

                var typeItem = _type.SelectedItem as ToolTypeItem;
                var type = typeItem == null ? AiToolType.Application : typeItem.Type;

                Tool.Name = _name.Text.Trim();
                Tool.Description = _description.Text.Trim();
                Tool.Category = string.IsNullOrWhiteSpace(_category.Text) ? "Custom" : _category.Text.Trim();
                Tool.Type = type;
                Tool.IsBuiltIn = type == AiToolType.BuiltIn;
                Tool.InputSchemaJson = _schema.Text.Trim();
                Tool.Enabled = _enabled.Checked;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                HMessage.ShowException(this, "The tool could not be saved.", "Tool", ex);
            }
        }
    }
}