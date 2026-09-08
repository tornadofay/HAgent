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

                Control content;
                if (RequiresExampleSubGroups(group))
                    content = CreateExampleSubGroups(pagesInGroup);
                else
                    content = CreateExampleTabControl(pagesInGroup);

                layout.Controls.Add(content, 0, 1);
                featurePage.Controls.Add(layout);
                _tabs.TabPages.Add(featurePage);
            }

            if (_tabs.TabPages.Count > 0)
                _tabs.SelectedIndex = 0;
        }

        private static bool RequiresExampleSubGroups(string group)
        {
            return string.Equals(group, "Context", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(group, "Runtime", StringComparison.OrdinalIgnoreCase);
        }

        private static Control CreateExampleSubGroups(List<TabPage> pages)
        {
            var subgroupOrder = string.Equals(GetExampleFeatureGroup(pages[0].Text), "Context", StringComparison.OrdinalIgnoreCase)
                ? new[] { "Context Core", "UI Context", "Data Access Context" }
                : new[] { "Runtime Instances", "Execution", "Intervention", "Planning & Capacity", "Diagnostics" };

            var grouped = new Dictionary<string, List<TabPage>>(StringComparer.OrdinalIgnoreCase);
            foreach (var subgroup in subgroupOrder)
                grouped[subgroup] = new List<TabPage>();

            foreach (var page in pages)
            {
                var subgroup = GetExampleSubGroup(page.Text);
                List<TabPage> target;
                if (!grouped.TryGetValue(subgroup, out target))
                    target = grouped[subgroupOrder[0]];
                target.Add(page);
            }

            var subTabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9f),
                Padding = new Point(12, 5),
                Multiline = false
            };

            foreach (var subgroup in subgroupOrder)
            {
                List<TabPage> childPages = grouped[subgroup];
                if (childPages.Count == 0)
                    continue;

                var subgroupPage = new TabPage(subgroup)
                {
                    BackColor = Color.FromArgb(248, 248, 252),
                    Padding = new Padding(8)
                };
                var examples = CreateExampleTabControl(childPages);
                subgroupPage.Controls.Add(examples);
                subTabs.TabPages.Add(subgroupPage);
            }

            return subTabs;
        }

        private static TabControl CreateExampleTabControl(List<TabPage> pages)
        {
            var nested = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9f),
                Padding = new Point(12, 5),
                Multiline = false
            };
            foreach (var page in pages)
                nested.TabPages.Add(page);
            return nested;
        }

        private static string GetExampleSubGroup(string title)
        {
            var key = (title ?? string.Empty).Trim().ToUpperInvariant();

            if (key == "CONTEXT BUDGET")
                return "Context Core";
            if (key.StartsWith("UI ", StringComparison.Ordinal) || key == "APPLICATION OBJECT CONTEXT")
                return "UI Context";
            if (key == "DATA QUERY CONTRACT")
                return "Data Access Context";

            if (key == "RUNTIME INSTANCES" || key == "RUNTIME OVERRIDES" || key == "RUNTIME SHUTDOWN" || key == "RUNTIME SCHEDULING" || key == "RUNTIME CONCURRENCY")
                return "Runtime Instances";
            if (key == "RUNTIME TERMINAL STATE" || key == "RESOURCE CAPABILITY" || key == "EXECUTION INTERVENTION" || key == "INTERVENTION HARDENING")
                return key.Contains("INTERVENTION") ? "Intervention" : "Execution";
            if (key == "EXECUTION TARGET PLANNING" || key == "EXECUTION TARGET CATALOG" || key == "QUOTA ADMISSION")
                return "Planning & Capacity";
            if (key == "EXECUTION AUDIT" || key == "INTERNAL INVENTORY")
                return "Diagnostics";

            return "Execution";
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
