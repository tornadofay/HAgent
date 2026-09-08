using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace HAgent.WinForms.UI.Configuration.About
{
    internal sealed class AboutPage : ConfigurationPageBase
    {
        public AboutPage()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            root.Controls.Add(CreateHeader("About HAgent", "Configuration host and reusable AI runtime library."));
            root.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 150,
                Text = "HAgent keeps provider connections, agent behavior, tools, policy, permissions, and storage as separate configuration concerns. The host application remains responsible for authentication, authorization, and executable side effects.",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9.5f),
                Padding = new Padding(2, 8, 0, 0)
            });
            var link = new LinkLabel { Text = "GitHub repository", AutoSize = true, LinkColor = Accent, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Padding = new Padding(2, 6, 0, 0) };
            link.Click += delegate
            {
                Process.Start(new ProcessStartInfo("https://github.com/tornadofay/HAgent") { UseShellExecute = true });
            };
            root.Controls.Add(link);
            Controls.Add(root);
        }
    }
}
