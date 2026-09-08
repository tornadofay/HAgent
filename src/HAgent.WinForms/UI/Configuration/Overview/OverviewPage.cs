using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HAgent.WinForms.UI.Configuration.Overview
{
    internal sealed class OverviewPage : ConfigurationPageBase
    {
        private readonly ConfigurationContext _context;
        private readonly FlowLayoutPanel _cards = new FlowLayoutPanel();

        public OverviewPage(ConfigurationContext context)
        {
            _context = context;
            Build();
        }

        public void RefreshData()
        {
            _cards.Controls.Clear();
            _cards.Controls.Add(Card("Providers", _context.Providers.Count.ToString(), "Connection definitions"));
            _cards.Controls.Add(Card("Agents", _context.Agents.Count.ToString(), "Configured behaviors"));
            _cards.Controls.Add(Card("Tools", _context.Tools.GetDefinitions().Count().ToString(), "Available capabilities"));
        }

        private void Build()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            root.Controls.Add(CreateHeader("Workspace", "Manage providers, agents, tools, policy, permissions, and storage from one configuration surface."));
            _cards.Dock = DockStyle.Top;
            _cards.Height = 116;
            _cards.FlowDirection = FlowDirection.LeftToRight;
            _cards.WrapContents = false;
            _cards.Padding = new Padding(0, 6, 0, 0);
            _cards.BackColor = Surface;
            root.Controls.Add(_cards);
            Controls.Add(root);
            RefreshData();
        }

        private Control Card(string name, string value, string description)
        {
            var panel = new Panel { Width = 220, Height = 96, BackColor = Color.White, Margin = new Padding(0, 0, 14, 0), BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = Accent, AutoSize = true, Left = 16, Top = 12 });
            panel.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Heading, AutoSize = true, Left = 88, Top = 18 });
            panel.Controls.Add(new Label { Text = description, Font = new Font("Segoe UI", 8.5f), ForeColor = Muted, AutoSize = true, Left = 88, Top = 43 });
            return panel;
        }
    }
}
