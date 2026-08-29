using System;
using System.Drawing;
using System.Windows.Forms;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;
using HAgent.WinForms.UI;

namespace HAgent.WinForms.Forms
{
    public sealed class UiPermissionsForm : HAgentForm
    {
        private readonly UiAutomationPermissions _permissions;
        private readonly CheckBox _automaticDiscovery = new CheckBox();
        private readonly CheckBox _readControls = new CheckBox();
        private readonly CheckBox _readData = new CheckBox();
        private readonly CheckBox _writeControls = new CheckBox();
        private readonly CheckBox _invokeControls = new CheckBox();
        private readonly Label _status = new Label();

        private static readonly Color Surface = Color.FromArgb(248, 248, 252);
        private static readonly Color Text = Color.FromArgb(68, 62, 88);
        private static readonly Color Muted = Color.FromArgb(100, 92, 120);

        public UiPermissionsForm(UiAutomationPermissions permissions)
            : base("AI Permissions", "Controls what automatic HAgent UI behavior may inspect or perform.", new Size(820, 620), new Size(700, 520))
        {
            _permissions = (permissions ?? new UiAutomationPermissions()).Clone();
            BuildUi();
            LoadValues();
        }

        private void BuildUi()
        {
            BodyPanel.Padding = new Padding(24);
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Surface
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

            root.Controls.Add(new Label
            {
                Text = "These permissions govern automatic HAgent UI behavior.\r\nThey do not grant arbitrary code execution.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Muted,
                Padding = new Padding(0, 2, 0, 0)
            }, 0, 0);

            var options = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Surface
            };
            options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 5; i++)
                options.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));

            AddOption(options, 0, _automaticDiscovery, "Automatic discovery", "Discover controls, forms, bindings, and useful data sources automatically.");
            AddOption(options, 1, _readControls, "Read controls", "Read control values and relevant UI properties.");
            AddOption(options, 2, _readData, "Read bound data", "Read tabular data exposed through DataGridView and related data sources.");
            AddOption(options, 3, _writeControls, "Write controls", "Allow future tools to change control values. Disabled by default.");
            AddOption(options, 4, _invokeControls, "Invoke controls", "Allow future tools to trigger buttons or commands. Disabled by default.");
            root.Controls.Add(options, 0, 1);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Surface
            };

            var save = CreateActionButton("Save permissions", 160, 38);
            save.Click += Save;
            actions.Controls.Add(save);

            var reset = CreateActionButton("Reset safe defaults", 175, 38);
            reset.Click += delegate
            {
                _permissions.AutomaticDiscovery = false;
                _permissions.ReadControls = true;
                _permissions.ReadData = true;
                _permissions.WriteControls = false;
                _permissions.InvokeControls = false;
                LoadValues();
            };
            actions.Controls.Add(reset);
            root.Controls.Add(actions, 0, 2);

            _status.Dock = DockStyle.Fill;
            _status.ForeColor = Muted;
            _status.Font = new Font("Segoe UI", 8.5f);
            root.Controls.Add(_status, 0, 3);
            BodyPanel.Controls.Add(root);
        }

        private static void AddOption(TableLayoutPanel host, int row, CheckBox checkBox, string title, string description)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            checkBox.AutoSize = true;
            checkBox.Text = title;
            checkBox.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            checkBox.ForeColor = Text;
            checkBox.Location = new Point(0, 2);
            panel.Controls.Add(checkBox);
            panel.Controls.Add(new Label
            {
                Text = description,
                AutoSize = false,
                Left = 24,
                Top = 28,
                Width = 700,
                Height = 38,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.6f)
            });
            host.Controls.Add(panel, 0, row);
        }

        private static HButton CreateActionButton(string text, int width, int height)
        {
            return new HButton
            {
                Text = text,
                Width = width,
                Height = height,
                RoundButton = true,
                Edge = 10,
                TextAlign = ContentAlignment.MiddleCenter,
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

        private void LoadValues()
        {
            _automaticDiscovery.Checked = _permissions.AutomaticDiscovery;
            _readControls.Checked = _permissions.ReadControls;
            _readData.Checked = _permissions.ReadData;
            _writeControls.Checked = _permissions.WriteControls;
            _invokeControls.Checked = _permissions.InvokeControls;
            _status.Text = "Safe defaults keep automatic discovery, writing, and invoking disabled.";
        }

        private void Save(object sender, EventArgs e)
        {
            _permissions.AutomaticDiscovery = _automaticDiscovery.Checked;
            _permissions.ReadControls = _readControls.Checked;
            _permissions.ReadData = _readData.Checked;
            _permissions.WriteControls = _writeControls.Checked;
            _permissions.InvokeControls = _invokeControls.Checked;
            try
            {
                _permissions.Validate();
                AISettings.SaveUiPermissions(_permissions);
                _status.Text = "Permissions saved.";
            }
            catch (Exception ex)
            {
                HMessage.ShowException(this, "The permission policy could not be saved.", "AI Permissions", ex);
            }
        }
    }
}
