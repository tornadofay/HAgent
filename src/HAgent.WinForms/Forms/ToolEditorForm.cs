using System;
using System.Drawing;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Helpers;

namespace HAgent.WinForms.Forms
{
    internal sealed class ToolEditorForm : HAgentForm
    {
        public AiTool Tool { get; private set; }
        private readonly TextBox _name = new TextBox();
        private readonly TextBox _description = new TextBox();
        private readonly TextBox _category = new TextBox();
        private readonly TextBox _schema = new TextBox();
        private readonly CheckBox _enabled = new CheckBox();

        public ToolEditorForm(AiTool tool)
            : base("Tool definition", "Describe a capability that an agent can request from the host application", new Size(820, 650), new Size(700, 580))
        {
            Tool = tool ?? new AiTool();
            Build();
        }

        private void Build()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, BackColor = Color.FromArgb(248, 248, 252) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            AddField(layout, 0, "Name", "Stable tool name exposed to the model.", _name);
            AddField(layout, 1, "Description", "Explain what the tool does and when the agent should use it.", _description);
            AddField(layout, 2, "Category", "A human-facing grouping such as UI, Files, Database, or System.", _category);
            AddField(layout, 3, "Input schema", "JSON Schema describing the arguments the model may send to the tool.", _schema);

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
            var save = new HFlatButton { Text = "Save tool", Width = 120, Height = 36, Margin = new Padding(8, 4, 0, 0) };
            save.Click += delegate { Save(); };
            footer.Controls.Add(save);
            footer.Controls.Add(_enabled);

            BodyPanel.Padding = new Padding(24);
            BodyPanel.Controls.Add(layout);
            BodyPanel.Controls.Add(footer);
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

                Tool.Name = _name.Text.Trim();
                Tool.Description = _description.Text.Trim();
                Tool.Category = string.IsNullOrWhiteSpace(_category.Text) ? "Custom" : _category.Text.Trim();
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
