using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private static readonly string[] ExampleFeatureOrder =
        {
            "Core",
            "Memory",
            "Context",
            "Tools",
            "Providers",
            "Policy",
            "Events",
            "Identity",
            "Runtime",
            "Workspace",
            "Cognition",
            "Configuration",
            "Diagnostics"
        };

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AddIdentityFeatureTabs();
            // This example was implemented ahead of the roadmap order. Keep its public-API
            // verification available without changing the ordered roadmap milestone.
            AddLearningCandidateInterventionTab();
            OrganizeExampleTabs();
        }

        private void OrganizeExampleTabs()
        {
            if (_tabs == null || _tabs.TabPages.Count == 0)
                return;

            var pages = _tabs.TabPages.Cast<TabPage>().ToList();
            var grouped = new Dictionary<string, List<TabPage>>(StringComparer.OrdinalIgnoreCase);
            foreach (var group in ExampleFeatureOrder)
                grouped[group] = new List<TabPage>();

            foreach (var page in pages)
            {
                var group = GetExampleFeatureGroup(page.Text);
                List<TabPage> target;
                if (!grouped.TryGetValue(group, out target))
                    target = grouped["Core"];
                target.Add(page);
            }

            _tabs.TabPages.Clear();
            _tabs.Padding = new Point(14, 6);
            _tabs.Multiline = false;

            foreach (var group in ExampleFeatureOrder)
            {
                List<TabPage> pagesInGroup = grouped[group];
                if (pagesInGroup.Count == 0)
                    continue;

                var featurePage = new TabPage(group)
                {
                    BackColor = Color.FromArgb(248, 248, 252),
                    Padding = new Padding(0)
                };

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    BackColor = Color.FromArgb(248, 248, 252),
                    Padding = new Padding(14)
                };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                var title = new Label
                {
                    Text = group + " examples",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.FromArgb(31, 24, 69),
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    Padding = new Padding(2, 0, 0, 0)
                };
                layout.Controls.Add(title, 0, 0);

                var nested = new TabControl
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9f),
                    Padding = new Point(12, 5),
                    Multiline = false
                };
                foreach (var page in pagesInGroup)
                    nested.TabPages.Add(page);

                layout.Controls.Add(nested, 0, 1);
                featurePage.Controls.Add(layout);
                _tabs.TabPages.Add(featurePage);
            }

            if (_tabs.TabPages.Count > 0)
                _tabs.SelectedIndex = 0;
        }

        private static string GetExampleFeatureGroup(string title)
        {
            var key = (title ?? string.Empty).Trim().ToUpperInvariant();

            if (key == "MESSAGING" || key == "SESSION" || key == "PERSISTENT SESSION")
                return "Core";

            if (key.Contains("MEMORY") || key == "AUTOMATIC MEMORY" || key == "TASK / EVENT MEMORY" || key == "EPISODIC MEMORY")
                return "Memory";

            if (key == "CONTEXT BUDGET" || key.StartsWith("UI ", StringComparison.Ordinal) ||
                key == "APPLICATION OBJECT CONTEXT" || key == "DATA QUERY CONTRACT")
                return "Context";

            if (key.Contains("TOOL"))
                return "Tools";

            if (key.Contains("PROVIDER") || key == "CAPABILITIES" || key == "RESPONSE NORMALIZATION" ||
                key == "STREAMING" || key == "LIVE STREAMING")
                return "Providers";

            if (key.Contains("POLICY") || key == "APPROVAL WORKFLOW")
                return "Policy";

            if (key.Contains("EVENT"))
                return "Events";

            if (key.Contains("IDENTITY"))
                return "Identity";

            if (key.Contains("WORKSPACE"))
                return "Workspace";

            if (key.Contains("LEARNING") || key.Contains("COGNITION"))
                return "Cognition";

            if (key == "CONFIGURATION")
                return "Configuration";

            if (key == "INTERNAL INVENTORY")
                return "Diagnostics";

            if (key.Contains("RUNTIME") || key.Contains("EXECUTION") || key.Contains("INTERVENTION") ||
                key.Contains("QUOTA") || key.Contains("TARGET PLANNING") || key.Contains("TARGET CATALOG") ||
                key.Contains("RESOURCE CAPABILITY") || key.Contains("AUDIT"))
                return "Runtime";

            return "Core";
        }
    }
}
