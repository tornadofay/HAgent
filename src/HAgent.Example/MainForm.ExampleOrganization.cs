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
                    RowCount = pagesInGroup.Count,
                    BackColor = Color.FromArgb(248, 248, 252),
                    Padding = new Padding(8)
                };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                for (var i = 0; i < pagesInGroup.Count; i++)
                {
                    layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / pagesInGroup.Count));
                    layout.Controls.Add(pagesInGroup[i].Controls.Count == 0
                        ? new Label { Text = pagesInGroup[i].Text, Dock = DockStyle.Fill }
                        : pagesInGroup[i].Controls[0], 0, i);
                }

                featurePage.Controls.Add(layout);
                _tabs.TabPages.Add(featurePage);
            }
        }
    }
}
