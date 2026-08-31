using System;
using System.Drawing;
using System.Windows.Forms;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddInternalConversationTab()
        {
            var page = new TabPage("Internal Conversation") { BackColor = Color.White };
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(12),
                BackColor = Color.White
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var description = new Label
            {
                Text = "Reads one HAgent-owned conversation by explicit session ID; verifies message bounds and cross-agent isolation.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(68, 62, 88),
                AutoEllipsis = true
            };
            panel.Controls.Add(description, 0, 0);
            panel.SetColumnSpan(description, 2);

            var runButton = new HButton
            {
                Text = "Run test",
                Dock = DockStyle.Fill,
                Height = 36,
                RoundButton = true,
                Edge = 10,
                Font = new Font("Segoe UI", 9.1f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 3, 0, 3)
            };
            runButton.Click += async delegate
            {
                runButton.Enabled = false;
                try
                {
                    await RunExampleAsync(delegate { return TestInternalConversationAsync("run"); });
                }
                finally
                {
                    runButton.Enabled = true;
                }
            };
            panel.Controls.Add(runButton, 1, 1);

            var note = new Label
            {
                Text = "Uses the currently selected HAgent storage backend and removes its temporary verification session after the check.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                ForeColor = Color.FromArgb(100, 92, 120),
                Padding = new Padding(0, 10, 0, 0)
            };
            panel.Controls.Add(note, 0, 2);
            panel.SetColumnSpan(note, 2);

            page.Controls.Add(panel);
            _tabs.TabPages.Add(page);
        }
    }
}
