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
            AddObservabilityTracingTab();
            AddObservabilityRuntimeInstrumentationTab();
            AddObservabilitySamplingRetentionTab();
            AddObservabilitySinksTab();
            AddObservabilityDiagnosticProjectionTab();
            AddObservabilityTracePropagationTab();
            AddObservabilityOutcomeTracingTab();
            AddEvaluationContractsTab();
            AddDeterministicEvaluationTab();
            AddSuppliedEvaluationTab();
            AddModelAssistedEvaluationTab();
            AddEvaluationAggregationTab();
            AddEvaluationRegressionTab();
            // This example was implemented ahead of the roadmap order. Keep its public-API
            // verification available without changing the ordered roadmap milestone.
            AddLearningCandidateInterventionTab();
            OrganizeExampleTabs();
            NormalizeExampleTabContentLayouts();
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
                   string.Equals(group, "Runtime", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(group, "Diagnostics", StringComparison.OrdinalIgnoreCase);
        }

        private static Control CreateExampleSubGroups(List<TabPage> pages)
        {
            string[] subgroupOrder;
            var group = GetExampleFeatureGroup(pages[0].Text);
            if (string.Equals(group, "Context", StringComparison.OrdinalIgnoreCase))
                subgroupOrder = new[] { "Context Core", "UI Context", "Data Access Context" };
            else if (string.Equals(group, "Diagnostics", StringComparison.OrdinalIgnoreCase))
                subgroupOrder = new[] { "Observability", "Evaluation", "Other Diagnostics" };
            else
                subgroupOrder = new[] { "Runtime Instances", "Execution", "Intervention", "Planning & Capacity", "Diagnostics" };

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

        private void NormalizeExampleTabContentLayouts()
        {
            foreach (TabPage featurePage in _tabs.TabPages)
                NormalizeControlsRecursive(featurePage);
        }

        private static void NormalizeControlsRecursive(Control root)
        {
            var page = root as TabPage;
            var layout = root as TableLayoutPanel;
            if (page != null && page.Controls.Count == 1)
                layout = page.Controls[0] as TableLayoutPanel;

            if (layout != null && layout.RowCount == 4 && layout.Controls.Count >= 4)
            {
                bool deeplyNested = IsDeeplyNestedExamplePage(page);

                layout.RowStyles[0] = new RowStyle(SizeType.Absolute, 44);
                layout.RowStyles[1] = new RowStyle(SizeType.Percent, deeplyNested ? 45 : 52);
                layout.RowStyles[2] = new RowStyle(SizeType.Absolute, deeplyNested ? 126 : 0);
                layout.RowStyles[3] = new RowStyle(SizeType.Absolute, 48);

                if (!deeplyNested)
                    layout.RowStyles[2] = new RowStyle(SizeType.Percent, 31);

                var details = layout.GetControlFromPosition(0, 2) as Label;
                if (details != null)
                {
                    details.AutoEllipsis = false;
                    details.AutoSize = false;
                    details.Padding = new Padding(1, 8, 20, 4);
                }

                var note = layout.GetControlFromPosition(0, 3) as Label;
                if (note != null)
                {
                    note.AutoEllipsis = false;
                    note.AutoSize = false;
                    note.Padding = new Padding(1, 5, 20, 2);
                }
            }

            foreach (Control child in root.Controls)
                NormalizeControlsRecursive(child);
        }

        private static bool IsDeeplyNestedExamplePage(TabPage page)
        {
            if (page == null)
                return false;

            int tabControlAncestors = 0;
            Control current = page.Parent;
            while (current != null)
            {
                if (current is TabControl)
                    tabControlAncestors++;
                current = current.Parent;
            }

            return tabControlAncestors >= 2 &&
                   (GetExampleFeatureGroup(page.Text) == "Runtime" ||
                    GetExampleFeatureGroup(page.Text) == "Context");
        }

        private static string GetExampleSubGroup(string title)
        {
            var key = (title ?? string.Empty).Trim().ToUpperInvariant();

            if (key == "CONTEXT CONTRACTS" || key == "CONTEXT ACQUISITION" || key == "CONTEXT BUDGET" || key == "CONTEXT RANKING" || key == "CONTEXT COMPACTION" || key == "CONTEXT CACHE" || key == "CONTEXT EXECUTION INTEGRATION" || key == "CONTEXT MULTI-RESOURCE RETRIEVAL" || key == "CONTEXT POLICY ASSEMBLY" || key == "CONTEXT ASSEMBLY" || key == "CONTEXT HOST AUTHORIZATION")
                return "Context Core";
            if (key.StartsWith("UI ", StringComparison.Ordinal) || key == "APPLICATION OBJECT CONTEXT")
                return "UI Context";
            if (key == "DATA QUERY CONTRACT")
                return "Data Access Context";
            if (key.StartsWith("OBSERVABILITY", StringComparison.Ordinal))
                return "Observability";
            if (key == "EVALUATION CONTRACTS" || key == "DETERMINISTIC EVALUATION" || key == "SUPPLIED EVALUATION RATINGS" || key == "MODEL-ASSISTED EVALUATION" || key == "EVALUATION AGGREGATION" || key == "EVALUATION REGRESSION SUITES")
                return "Evaluation";

            if (key == "RUNTIME INSTANCES" || key == "RUNTIME OVERRIDES" || key == "RUNTIME SHUTDOWN" || key == "RUNTIME SCHEDULING" || key == "RUNTIME CONCURRENCY")
                return "Runtime Instances";
            if (key == "EXECUTION INTERVENTION" || key == "INTERVENTION HARDENING")
                return "Intervention";
            if (key == "RUNTIME TERMINAL STATE" || key == "RESOURCE CAPABILITY" || key == "RUNTIME EXECUTION")
                return "Execution";
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

            if (key == "CONTEXT CONTRACTS" || key == "CONTEXT ACQUISITION" || key == "CONTEXT RANKING" || key == "CONTEXT COMPACTION" || key == "CONTEXT CACHE" || key == "CONTEXT EXECUTION INTEGRATION" || key == "CONTEXT MULTI-RESOURCE RETRIEVAL" || key == "CONTEXT POLICY ASSEMBLY" || key == "CONTEXT ASSEMBLY" || key == "CONTEXT HOST AUTHORIZATION" || key == "CONTEXT BUDGET" ||
                key.StartsWith("UI ", StringComparison.Ordinal) || key == "APPLICATION OBJECT CONTEXT" || key == "DATA QUERY CONTRACT")
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

            if (key.Contains("TRACE") || key.Contains("OBSERVABILITY"))
                return "Diagnostics";

            if (key.Contains("EVALUATION"))
                return "Diagnostics";

            if (key.Contains("RUNTIME") || key.Contains("EXECUTION") || key == "RESOURCE CAPABILITY" || key == "QUOTA ADMISSION")
                return "Runtime";

            if (key.Contains("WORKSPACE"))
                return "Workspace";

            if (key.Contains("LEARNING") || key.Contains("COGNITION"))
                return "Cognition";

            if (key.Contains("CONFIGURATION"))
                return "Configuration";

            return "Diagnostics";
        }
    }
}
