using System.Drawing;
using System.Windows.Forms;
using HAgent.WinForms.Forms;

namespace HAgent.WinForms.UI.Configuration.Policy
{
    internal sealed class PolicyPage : ConfigurationPageBase
    {
        private readonly ConfigurationContext _context;

        public PolicyPage(ConfigurationContext context)
        {
            _context = context;
            Build();
        }

        private void Build()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(0, 0, 10, 0) };
            root.Controls.Add(CreateHeader("Policy", "Unified rules, deterministic precedence, effective decisions, and resource capability state."));

            var card = new Panel { Dock = DockStyle.Top, Height = 190, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(20) };
            card.Controls.Add(new Label
            {
                Text = "Policy management",
                AutoSize = true,
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Heading
            });
            card.Controls.Add(new Label
            {
                Text = "Edit policy rules, inspect decision provenance, and review effective agent resource capability state. The same canonical policy engine used by runtime enforcement is used for evaluation here.",
                Dock = DockStyle.Top,
                Height = 70,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9f)
            });
            var open = CreateActionButton("Open policy management", 190);
            open.Location = new Point(20, 118);
            open.Click += delegate
            {
                using (var form = new PolicyEditorForm(_context.Store, _context.Agents))
                    form.ShowDialog(FindForm());
            };
            card.Controls.Add(open);
            root.Controls.Add(card);
            Controls.Add(root);
        }
    }
}
