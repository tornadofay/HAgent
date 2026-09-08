using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HAgent.WinForms.UI.Configuration.Overview
{
    internal sealed class OverviewPage : ConfigurationPageBase
    {
        private readonly ConfigurationContext _context;

        public OverviewPage(ConfigurationContext context)
        {
            _context = context;
            Controls.Add(Build());
        }

        private Control Build()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            root.Controls.Add(CreateHeader("Workspace", "Manage providers, agents, tools, policy, permissions, and storage from one configuration surface."));

            var cards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 116,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 0),
                BackColor = Surface
            };
            cards.Controls.Add(Card("Providers", _context.Providers.Count.ToString(), "Connection definitions"));
            cards.Controls.Add(Card("Agents", _context.Agents.Count.ToString(), "Configured behaviors"));
            cards.Controls.Add(Card("Tools", _context.Tools.GetDefinitions().Count().ToString(), "Available capabilities"));
            root.Controls.Add(cards);
            return root;
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
