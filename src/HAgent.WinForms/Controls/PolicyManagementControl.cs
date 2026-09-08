using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.Controls
{
    internal sealed class PolicyManagementControl : UserControl
    {
        private readonly IAiStore _store;
        private readonly IReadOnlyList<AiAgent> _agents;
        private readonly TabControl _tabs = new TabControl();
        private readonly DataGridView _rules = new DataGridView();
        private readonly ComboBox _agent = new ComboBox();
        private readonly DataGridView _resources = new DataGridView();
        private readonly TextBox _operation = new TextBox();
        private readonly TextBox _resourceType = new TextBox();
        private readonly TextBox _resourceId = new TextBox();
        private readonly TextBox _toolId = new TextBox();
        private readonly TextBox _providerId = new TextBox();
        private readonly TextBox _targetId = new TextBox();
        private readonly Label _decision = new Label();
        private readonly Label _version = new Label();

        private AiPolicySet _policy = new AiPolicySet();

        private static readonly Color Surface = Color.FromArgb(248, 248, 252);
        private static readonly Color Heading = Color.FromArgb(31, 24, 69);
        private static readonly Color Muted = Color.FromArgb(100, 92, 120);
        private static readonly Color Accent = Color.FromArgb(116, 76, 210);

        public PolicyManagementControl(IAiStore store, IEnumerable<AiAgent> agents)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _agents = (agents ?? new List<AiAgent>()).Where(x => x != null).ToList().AsReadOnly();
            Dock = DockStyle.Fill;
            BackColor = Surface;
            Build();
        }

        public async Task ReloadAsync()
        {
            _policy = await _store.GetPolicySetAsync().ConfigureAwait(true) ?? new AiPolicySet();
            _policy.Validate();
            _version.Text = "Policy version: " + _policy.Version + "   |   Rules: " + (_policy.Rules == null ? 0 : _policy.Rules.Count);
            RefreshRules();
            if (_agent.Items.Count > 0 && _agent.SelectedIndex < 0) _agent.SelectedIndex = 0;
            RefreshResources();
        }

        private void Build()
        {
            _tabs.Dock = DockStyle.Fill;
            _tabs.Font = new Font("Segoe UI", 9f);

            var rules = new TabPage("Policy Rules") { BackColor = Surface, Padding = new Padding(14) };
            rules.Controls.Add(BuildRules());
            var evaluation = new TabPage("Effective Decision") { BackColor = Surface, Padding = new Padding(14) };
            evaluation.Controls.Add(BuildEvaluation());
            var resources = new TabPage("Agent Capabilities") { BackColor = Surface, Padding = new Padding(14) };
            resources.Controls.Add(BuildResources());

            _tabs.TabPages.Add(rules);
            _tabs.TabPages.Add(evaluation);
            _tabs.TabPages.Add(resources);
            Controls.Add(_tabs);
        }

        private Control BuildRules()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            var top = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = Surface };
            top.Controls.Add(new Label { Text = "Unified policy rules", AutoSize = true, Left = 0, Top = 0, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Heading });
            top.Controls.Add(new Label { Text = "Rules are evaluated by deterministic precedence. Empty constraints are unrestricted.", AutoSize = true, Left = 1, Top = 35, Font = new Font("Segoe UI", 8.8f), ForeColor = Muted });
            _version.AutoSize = true;
            _version.Left = 1;
            _version.Top = 57;
            _version.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            _version.ForeColor = Accent;
            top.Controls.Add(_version);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, WrapContents = false, BackColor = Surface, Padding = new Padding(0, 2, 0, 0) };
            AddAction(actions, "+  Add rule", false, async delegate { await EditRuleAsync(null); });
            AddAction(actions, "Edit selected", false, async delegate { await EditSelectedRuleAsync(); });
            AddAction(actions, "Delete selected", true, async delegate { await DeleteSelectedRuleAsync(); });

            ConfigureGrid(_rules);
            _rules.Columns.Add("Id", "ID");
            _rules.Columns.Add("Name", "Name");
            _rules.Columns.Add("Scope", "Scope");
            _rules.Columns.Add("ScopeId", "Scope ID");
            _rules.Columns.Add("Priority", "Priority");
            _rules.Columns.Add("Outcome", "Outcome");
            _rules.Columns.Add("Operation", "Operation");
            _rules.Columns.Add("Target", "Resource / Tool");
            _rules.Dock = DockStyle.Fill;

            root.Controls.Add(_rules);
            root.Controls.Add(actions);
            root.Controls.Add(top);
            return root;
        }

        private Control BuildEvaluation()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            root.Controls.Add(new Label { Text = "Evaluate the persisted policy using the same decision engine used by HAgent runtime enforcement.", Dock = DockStyle.Top, Height = 42, ForeColor = Muted, Font = new Font("Segoe UI", 8.8f) });

            var grid = new TableLayoutPanel { Dock = DockStyle.Top, Height = 220, ColumnCount = 2, RowCount = 6, BackColor = Surface };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 6; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            AddEvalField(grid, 0, "Operation", _operation);
            AddEvalField(grid, 1, "Resource type", _resourceType);
            AddEvalField(grid, 2, "Resource ID", _resourceId);
            AddEvalField(grid, 3, "Tool ID", _toolId);
            AddEvalField(grid, 4, "Provider ID", _providerId);
            AddEvalField(grid, 5, "Execution target", _targetId);
            ConfigureText(_operation, "tool.invoke");
            ConfigureText(_resourceType, "tool");

            var button = CreateButton("Evaluate policy", 150, 36);
            button.Left = 170;
            button.Top = 270;
            button.Click += async delegate { await EvaluateAsync(); };

            _decision.Left = 0;
            _decision.Top = 320;
            _decision.Width = 900;
            _decision.Height = 125;
            _decision.BorderStyle = BorderStyle.FixedSingle;
            _decision.BackColor = Color.White;
            _decision.Padding = new Padding(12);
            _decision.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _decision.ForeColor = Heading;

            root.Controls.Add(_decision);
            root.Controls.Add(button);
            root.Controls.Add(grid);
            return root;
        }

        private Control BuildResources()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            var heading = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Surface };
            heading.Controls.Add(new Label { Text = "Agent resource capabilities", AutoSize = true, Left = 0, Top = 0, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Heading });
            heading.Controls.Add(new Label { Text = "Persistent profile state plus effective default resolution.", AutoSize = true, Left = 1, Top = 36, Font = new Font("Segoe UI", 8.8f), ForeColor = Muted });

            _agent.DropDownStyle = ComboBoxStyle.DropDownList;
            _agent.Dock = DockStyle.Fill;
            foreach (var agent in _agents.OrderBy(x => x.Name)) _agent.Items.Add(new AgentItem(agent));
            _agent.SelectedIndexChanged += delegate { RefreshResources(); };
            var host = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(0, 6, 0, 8), BackColor = Surface };
            host.Controls.Add(_agent);

            ConfigureGrid(_resources);
            _resources.Columns.Add("Type", "Resource Type");
            _resources.Columns.Add("ResourceId", "Resource ID");
            _resources.Columns.Add("Configured", "Profile State");
            _resources.Columns.Add("Effective", "Effective State");
            _resources.Dock = DockStyle.Fill;

            root.Controls.Add(_resources);
            root.Controls.Add(host);
            root.Controls.Add(heading);
            return root;
        }

        private async Task EvaluateAsync()
        {
            try
            {
                var policy = await _store.GetPolicySetAsync().ConfigureAwait(true) ?? new AiPolicySet();
                policy.Validate();
                var result = new DefaultAiPolicyEngine(policy).Evaluate(new AiPolicyEvaluationContext
                {
                    Operation = _operation.Text.Trim(),
                    ResourceType = _resourceType.Text.Trim(),
                    ResourceId = _resourceId.Text.Trim(),
                    ToolId = _toolId.Text.Trim(),
                    ProviderId = _providerId.Text.Trim(),
                    ExecutionTargetId = _targetId.Text.Trim(),
                    Identity = new AgentIdentityContext()
                });
                _decision.Text = "Outcome: " + result.Outcome + Environment.NewLine +
                                  "Rule: " + (string.IsNullOrWhiteSpace(result.RuleId) ? "(none)" : result.RuleId) + Environment.NewLine +
                                  "Scope: " + result.Scope + "   Priority: " + result.Priority + Environment.NewLine +
                                  "Policy version: " + result.PolicyVersion + Environment.NewLine +
                                  "Reason: " + result.Reason;
            }
            catch (Exception ex)
            {
                HMessage.ShowException(FindForm(), "The policy could not be evaluated.", "Policy", ex);
            }
        }

        private void RefreshRules()
        {
            _rules.Rows.Clear();
            foreach (var rule in (_policy.Rules ?? new List<AiPolicyRule>()).Where(x => x != null).OrderByDescending(x => x.Priority).ThenBy(x => x.Id, StringComparer.OrdinalIgnoreCase))
                _rules.Rows.Add(rule.Id, rule.Name, rule.Scope, rule.ScopeId, rule.Priority, rule.Outcome, string.Join(", ", rule.Operations ?? new List<string>()), BuildTargetText(rule));
        }

        private static string BuildTargetText(AiPolicyRule rule)
        {
            var values = new List<string>();
            if (rule.ResourceTypes != null && rule.ResourceTypes.Count > 0) values.Add("type=" + string.Join(",", rule.ResourceTypes));
            if (rule.ResourceIds != null && rule.ResourceIds.Count > 0) values.Add("resource=" + string.Join(",", rule.ResourceIds));
            if (rule.ToolIds != null && rule.ToolIds.Count > 0) values.Add("tool=" + string.Join(",", rule.ToolIds));
            if (rule.ProviderIds != null && rule.ProviderIds.Count > 0) values.Add("provider=" + string.Join(",", rule.ProviderIds));
            if (rule.ExecutionTargetIds != null && rule.ExecutionTargetIds.Count > 0) values.Add("target=" + string.Join(",", rule.ExecutionTargetIds));
            return values.Count == 0 ? "(unrestricted)" : string.Join("; ", values);
        }

        private async Task EditSelectedRuleAsync()
        {
            if (_rules.CurrentRow == null) return;
            var id = Convert.ToString(_rules.CurrentRow.Cells[0].Value);
            var rule = (_policy.Rules ?? new List<AiPolicyRule>()).FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            if (rule != null) await EditRuleAsync(rule);
        }

        private async Task EditRuleAsync(AiPolicyRule source)
        {
            var rule = source == null ? new AiPolicyRule() : source.Clone();
            using (var dialog = new PolicyRuleDialog(rule))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                if (source == null) _policy.Rules.Add(dialog.Rule); else _policy.Rules[_policy.Rules.IndexOf(source)] = dialog.Rule;
                _policy.Validate();
                await _store.SavePolicySetAsync(_policy);
                RefreshRules();
                _version.Text = "Policy version: " + _policy.Version + "   |   Rules: " + _policy.Rules.Count;
            }
        }

        private async Task DeleteSelectedRuleAsync()
        {
            if (_rules.CurrentRow == null) return;
            var id = Convert.ToString(_rules.CurrentRow.Cells[0].Value);
            var rule = (_policy.Rules ?? new List<AiPolicyRule>()).FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            if (rule == null) return;
            if (HMessage.ShowQuestion(FindForm(), "Delete the selected policy rule?", "Policy") != DialogResult.Yes) return;
            _policy.Rules.Remove(rule);
            _policy.Validate();
            await _store.SavePolicySetAsync(_policy);
            RefreshRules();
            _version.Text = "Policy version: " + _policy.Version + "   |   Rules: " + _policy.Rules.Count;
        }

        private void RefreshResources()
        {
            _resources.Rows.Clear();
            var item = _agent.SelectedItem as AgentItem;
            if (item == null) return;
            var profile = item.Agent.ResourceCapabilities;
            var snapshot = AiResourceCapabilitySnapshot.Resolve(profile, null);
            foreach (var entry in (profile == null ? new List<AiResourceCapabilityEntry>() : profile.Entries).Where(x => x != null))
                _resources.Rows.Add(entry.ResourceType, entry.ResourceId, entry.State, snapshot.GetState(entry.ResourceType, entry.ResourceId));
            if (_resources.Rows.Count == 0) _resources.Rows.Add("*", "", "Inherit", "Enabled (default)");
        }

        private static void ConfigureGrid(DataGridView grid)
        {
            grid.AutoGenerateColumns = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.Font = new Font("Segoe UI", 8.8f);
        }

        private static void ConfigureText(TextBox box, string value) { box.Text = value; box.Dock = DockStyle.Fill; box.Height = 28; }

        private static void AddEvalField(TableLayoutPanel grid, int row, string label, Control control)
        {
            grid.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Muted, Font = new Font("Segoe UI", 9f, FontStyle.Bold) }, 0, row);
            control.Dock = DockStyle.Fill;
            grid.Controls.Add(control, 1, row);
        }

        private static void AddAction(Control host, string text, bool destructive, Func<Task> action)
        {
            var button = CreateButton(text, text.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) ? 140 : 128, 36, destructive);
            button.Margin = new Padding(0, 2, 8, 0);
            button.Click += async delegate { await action(); };
            host.Controls.Add(button);
        }

        private static HButton CreateButton(string text, int width, int height, bool destructive = false)
        {
            var button = new HButton { Text = text, Width = width, Height = height, RoundButton = true, Edge = 10, TextMargin = 8, Font = new Font("Segoe UI", 9.2f, FontStyle.Bold), Cursor = Cursors.Hand };
            button.ButtonLeaveBackGroundColor1 = destructive ? Color.FromArgb(183, 61, 89) : Color.FromArgb(92, 67, 168);
            button.ButtonLeaveBackGroundColor2 = destructive ? Color.FromArgb(119, 38, 62) : Color.FromArgb(57, 40, 108);
            button.ButtonLeaveForeColor = Color.White;
            button.ButtonLeaveBorderColor = destructive ? Color.FromArgb(207, 80, 105) : Accent;
            button.ButtonEnterBackGroundColor1 = destructive ? Color.FromArgb(214, 75, 106) : Color.FromArgb(126, 94, 214);
            button.ButtonEnterBackGroundColor2 = destructive ? Color.FromArgb(150, 43, 74) : Color.FromArgb(79, 54, 145);
            button.ButtonEnterForeColor = Color.White;
            button.ButtonEnterBorderColor = destructive ? Color.FromArgb(231, 105, 131) : Color.FromArgb(146, 118, 232);
            return button;
        }

        private sealed class AgentItem
        {
            public AgentItem(AiAgent agent) { Agent = agent; }
            public AiAgent Agent { get; private set; }
            public override string ToString() { return Agent == null ? string.Empty : Agent.Name + " (" + Agent.Id + ")"; }
        }
    }
}
