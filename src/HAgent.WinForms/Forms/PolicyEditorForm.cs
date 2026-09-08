using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.Forms
{
    internal sealed class PolicyEditorForm : HAgentForm
    {
        private readonly IAiStore _store;
        private readonly IReadOnlyList<AiAgent> _agents;
        private AiPolicySet _policy = new AiPolicySet();
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
        private readonly Label _policyVersion = new Label();

        private static readonly Color Surface = Color.FromArgb(248, 248, 252);
        private static readonly Color Heading = Color.FromArgb(31, 24, 69);
        private static readonly Color Muted = Color.FromArgb(100, 92, 120);
        private static readonly Color Accent = Color.FromArgb(116, 76, 210);

        public PolicyEditorForm(IAiStore store, IEnumerable<AiAgent> agents)
            : base("Policy", "Manage unified policy rules, inspect effective decisions, and review agent resource capability state", new Size(1180, 820), new Size(980, 680))
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _agents = (agents ?? new List<AiAgent>()).Where(x => x != null).ToList().AsReadOnly();
            Build();
            Shown += async delegate { await ReloadAsync().ConfigureAwait(true); };
        }

        private void Build()
        {
            BodyPanel.Padding = new Padding(20);
            _tabs.Dock = DockStyle.Fill;
            _tabs.Font = new Font("Segoe UI", 9f);
            var rulesTab = new TabPage("Policy Rules") { BackColor = Surface, Padding = new Padding(14) };
            rulesTab.Controls.Add(BuildRulesPage());
            var evaluationTab = new TabPage("Effective Decision") { BackColor = Surface, Padding = new Padding(14) };
            evaluationTab.Controls.Add(BuildEvaluationPage());
            var resourcesTab = new TabPage("Agent Capabilities") { BackColor = Surface, Padding = new Padding(14) };
            resourcesTab.Controls.Add(BuildResourcesPage());
            _tabs.TabPages.Add(rulesTab);
            _tabs.TabPages.Add(evaluationTab);
            _tabs.TabPages.Add(resourcesTab);
            BodyPanel.Controls.Add(_tabs);
        }

        private Control BuildRulesPage()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            var top = new Panel { Dock = DockStyle.Top, Height = 82, BackColor = Surface };
            top.Controls.Add(new Label { Text = "Unified policy rules", AutoSize = true, Left = 0, Top = 0, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Heading });
            top.Controls.Add(new Label { Text = "Rules are evaluated by explicit precedence. Empty constraint lists remain unrestricted.", AutoSize = true, Left = 1, Top = 36, Font = new Font("Segoe UI", 8.8f), ForeColor = Muted });
            _policyVersion.AutoSize = true;
            _policyVersion.Left = 1;
            _policyVersion.Top = 60;
            _policyVersion.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            _policyVersion.ForeColor = Accent;
            top.Controls.Add(_policyVersion);
            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Surface, Padding = new Padding(0, 2, 0, 0) };
            AddAction(actions, "+  Add rule", false, async delegate { await EditRuleAsync(null).ConfigureAwait(true); });
            AddAction(actions, "Edit selected", false, async delegate { await EditSelectedRuleAsync().ConfigureAwait(true); });
            AddAction(actions, "Delete selected", true, async delegate { await DeleteSelectedRuleAsync().ConfigureAwait(true); });
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

        private Control BuildEvaluationPage()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            var intro = new Label { Text = "Evaluate the current persisted policy with explicit request fields. This shows the same decision/provenance the runtime receives.", Dock = DockStyle.Top, Height = 42, ForeColor = Muted, Font = new Font("Segoe UI", 8.8f) };
            var grid = new TableLayoutPanel { Dock = DockStyle.Top, Height = 220, ColumnCount = 2, RowCount = 5, BackColor = Surface };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 5; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            AddEvalField(grid, 0, "Operation", _operation);
            AddEvalField(grid, 1, "Resource type", _resourceType);
            AddEvalField(grid, 2, "Resource ID", _resourceId);
            AddEvalField(grid, 3, "Tool ID", _toolId);
            AddEvalField(grid, 4, "Provider / target", CreateProviderTargetHost());
            var button = CreateActionButton("Evaluate policy", 150, 36, false);
            button.Top = 272;
            button.Left = 170;
            button.Click += async delegate { await EvaluateAsync().ConfigureAwait(true); };
            _decision.Left = 0;
            _decision.Top = 322;
            _decision.Width = 850;
            _decision.Height = 120;
            _decision.BorderStyle = BorderStyle.FixedSingle;
            _decision.BackColor = Color.White;
            _decision.Padding = new Padding(12);
            _decision.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _decision.ForeColor = Heading;
            ConfigureText(_operation, "tool.invoke");
            ConfigureText(_resourceType, "tool");
            ConfigureText(_resourceId, string.Empty);
            ConfigureText(_toolId, string.Empty);
            root.Controls.Add(_decision);
            root.Controls.Add(button);
            root.Controls.Add(grid);
            root.Controls.Add(intro);
            return root;
        }

        private Control BuildResourcesPage()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            var heading = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Surface };
            heading.Controls.Add(new Label { Text = "Agent resource capabilities", AutoSize = true, Left = 0, Top = 0, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Heading });
            heading.Controls.Add(new Label { Text = "Profile states are persistent. Effective state includes the profile policy and the default-enabled fallback.", AutoSize = true, Left = 1, Top = 36, Font = new Font("Segoe UI", 8.8f), ForeColor = Muted });
            _agent.DropDownStyle = ComboBoxStyle.DropDownList;
            _agent.Dock = DockStyle.Top;
            _agent.Height = 30;
            _agent.SelectedIndexChanged += delegate { RefreshResources(); };
            foreach (var agent in _agents.OrderBy(x => x.Name)) _agent.Items.Add(new AgentItem(agent));
            var agentHost = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(0, 6, 0, 8), BackColor = Surface };
            agentHost.Controls.Add(_agent);
            ConfigureGrid(_resources);
            _resources.Columns.Add("Type", "Resource Type");
            _resources.Columns.Add("ResourceId", "Resource ID");
            _resources.Columns.Add("Configured", "Profile State");
            _resources.Columns.Add("Effective", "Effective State");
            _resources.Dock = DockStyle.Fill;
            root.Controls.Add(_resources);
            root.Controls.Add(agentHost);
            root.Controls.Add(heading);
            return root;
        }

        private async Task ReloadAsync()
        {
            _policy = await _store.GetPolicySetAsync().ConfigureAwait(true) ?? new AiPolicySet();
            _policy.Validate();
            _policyVersion.Text = "Policy version: " + _policy.Version + "   |   Rules: " + (_policy.Rules == null ? 0 : _policy.Rules.Count);
            RefreshRules();
            if (_agent.Items.Count > 0 && _agent.SelectedIndex < 0) _agent.SelectedIndex = 0;
            RefreshResources();
        }

        private void RefreshRules()
        {
            _rules.Rows.Clear();
            foreach (var rule in (_policy.Rules ?? new List<AiPolicyRule>()).Where(x => x != null).OrderByDescending(x => x.Priority).ThenBy(x => x.Id, StringComparer.OrdinalIgnoreCase))
                _rules.Rows.Add(rule.Id, rule.Name, rule.Scope.ToString(), rule.ScopeId, rule.Priority, rule.Outcome.ToString(), string.Join(", ", rule.Operations ?? new List<string>()), BuildTargetText(rule));
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
            var row = _rules.CurrentRow;
            if (row == null) return;
            var id = Convert.ToString(row.Cells[0].Value);
            var rule = (_policy.Rules ?? new List<AiPolicyRule>()).FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            if (rule != null) await EditRuleAsync(rule).ConfigureAwait(true);
        }

        private async Task EditRuleAsync(AiPolicyRule source)
        {
            var rule = source == null ? new AiPolicyRule() : source.Clone();
            using (var form = new PolicyRuleDialog(rule))
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;
                if (source == null) _policy.Rules.Add(form.Rule); else ReplaceRule(source, form.Rule);
                _policy.Validate();
                await _store.SavePolicySetAsync(_policy).ConfigureAwait(true);
                RefreshRules();
                _policyVersion.Text = "Policy version: " + _policy.Version + "   |   Rules: " + _policy.Rules.Count;
            }
        }

        private void ReplaceRule(AiPolicyRule source, AiPolicyRule replacement)
        {
            var index = _policy.Rules.IndexOf(source);
            if (index >= 0) _policy.Rules[index] = replacement;
        }

        private async Task DeleteSelectedRuleAsync()
        {
            var row = _rules.CurrentRow;
            if (row == null) return;
            var id = Convert.ToString(row.Cells[0].Value);
            var rule = (_policy.Rules ?? new List<AiPolicyRule>()).FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            if (rule == null) return;
            if (HMessage.ShowQuestion(this, "Delete the selected policy rule?", "Policy") != DialogResult.Yes) return;
            _policy.Rules.Remove(rule);
            _policy.Validate();
            await _store.SavePolicySetAsync(_policy).ConfigureAwait(true);
            RefreshRules();
            _policyVersion.Text = "Policy version: " + _policy.Version + "   |   Rules: " + _policy.Rules.Count;
        }

        private async Task EvaluateAsync()
        {
            try
            {
                _policy = await _store.GetPolicySetAsync().ConfigureAwait(true) ?? new AiPolicySet();
                _policy.Validate();
                var engine = new DefaultAiPolicyEngine(_policy);
                var result = engine.Evaluate(new AiPolicyEvaluationContext
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
            catch (Exception ex) { HMessage.ShowException(this, "The policy could not be evaluated.", "Policy", ex); }
        }

        private void RefreshResources()
        {
            _resources.Rows.Clear();
            var item = _agent.SelectedItem as AgentItem;
            if (item == null || item.Agent == null) return;
            var profile = item.Agent.ResourceCapabilities;
            var snapshot = AiResourceCapabilitySnapshot.Resolve(profile, null);
            foreach (var entry in (profile == null ? new List<AiResourceCapabilityEntry>() : profile.Entries).Where(x => x != null))
                _resources.Rows.Add(entry.ResourceType, entry.ResourceId, entry.State.ToString(), snapshot.GetState(entry.ResourceType, entry.ResourceId).ToString());
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

        private static void ConfigureText(TextBox box, string value)
        {
            box.Text = value;
            box.Dock = DockStyle.Fill;
            box.Height = 28;
        }

        private static void AddEvalField(TableLayoutPanel grid, int row, string label, Control control)
        {
            grid.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Muted, Font = new Font("Segoe UI", 9f, FontStyle.Bold) }, 0, row);
            control.Dock = DockStyle.Fill;
            grid.Controls.Add(control, 1, row);
        }

        private Control CreateProviderTargetHost()
        {
            var host = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            _providerId.Dock = DockStyle.Fill;
            _targetId.Dock = DockStyle.Fill;
            host.Controls.Add(_providerId, 0, 0);
            host.Controls.Add(_targetId, 1, 0);
            return host;
        }

        private static HButton CreateActionButton(string text, int width, int height, bool destructive)
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

        private static void AddAction(Control host, string text, bool destructive, Func<Task> action)
        {
            var button = CreateActionButton(text, text.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) ? 140 : 128, 36, destructive);
            button.Margin = new Padding(0, 2, 8, 0);
            button.Click += async delegate { await action().ConfigureAwait(true); };
            host.Controls.Add(button);
        }

        private sealed class AgentItem
        {
            public AgentItem(AiAgent agent) { Agent = agent; }
            public AiAgent Agent { get; private set; }
            public override string ToString() { return Agent == null ? string.Empty : Agent.Name + " (" + Agent.Id + ")"; }
        }
    }

    internal sealed class PolicyRuleDialog : HAgentForm
    {
        private readonly TableLayoutPanel _layout = new TableLayoutPanel();
        private readonly TextBox _name = new TextBox();
        private readonly ComboBox _scope = new ComboBox();
        private readonly TextBox _scopeId = new TextBox();
        private readonly NumericUpDown _priority = new NumericUpDown();
        private readonly ComboBox _outcome = new ComboBox();
        private readonly TextBox _operations = new TextBox();
        private readonly TextBox _resourceTypes = new TextBox();
        private readonly TextBox _resourceIds = new TextBox();
        private readonly TextBox _toolIds = new TextBox();
        private readonly TextBox _providerIds = new TextBox();
        private readonly TextBox _targetIds = new TextBox();
        private readonly TextBox _reason = new TextBox();
        public AiPolicyRule Rule { get; private set; }

        public PolicyRuleDialog(AiPolicyRule rule)
            : base("Policy rule", "Define one deterministic rule in the unified HAgent policy", new Size(900, 760), new Size(760, 650))
        {
            Rule = rule == null ? new AiPolicyRule() : rule.Clone();
            Build();
        }

        private void Build()
        {
            BodyPanel.Padding = new Padding(22);
            _layout.Dock = DockStyle.Fill;
            _layout.ColumnCount = 2;
            _layout.RowCount = 13;
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 13; i++) _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 12 ? 100 : 44));
            AddField(0, "Name", _name);
            AddField(1, "Scope", _scope);
            AddField(2, "Scope ID", _scopeId);
            AddField(3, "Priority", _priority);
            AddField(4, "Outcome", _outcome);
            AddField(5, "Operations", _operations);
            AddField(6, "Resource types", _resourceTypes);
            AddField(7, "Resource IDs", _resourceIds);
            AddField(8, "Tool IDs", _toolIds);
            AddField(9, "Provider IDs", _providerIds);
            AddField(10, "Execution target IDs", _targetIds);
            AddField(11, "Provenance / reason", _reason);
            foreach (var value in Enum.GetValues(typeof(AiPolicyScopeKind))) _scope.Items.Add(value);
            foreach (var value in Enum.GetValues(typeof(AiPolicyOutcome))) _outcome.Items.Add(value);
            _scope.DropDownStyle = ComboBoxStyle.DropDownList;
            _outcome.DropDownStyle = ComboBoxStyle.DropDownList;
            _scope.SelectedItem = Rule.Scope;
            _outcome.SelectedItem = Rule.Outcome;
            _priority.Minimum = -1000000;
            _priority.Maximum = 1000000;
            _priority.Value = Math.Max(_priority.Minimum, Math.Min(_priority.Maximum, Rule.Priority));
            _name.Text = Rule.Name;
            _scopeId.Text = Rule.ScopeId;
            _operations.Text = string.Join(",", Rule.Operations ?? new List<string>());
            _resourceTypes.Text = string.Join(",", Rule.ResourceTypes ?? new List<string>());
            _resourceIds.Text = string.Join(",", Rule.ResourceIds ?? new List<string>());
            _toolIds.Text = string.Join(",", Rule.ToolIds ?? new List<string>());
            _providerIds.Text = string.Join(",", Rule.ProviderIds ?? new List<string>());
            _targetIds.Text = string.Join(",", Rule.ExecutionTargetIds ?? new List<string>());
            _reason.Text = Rule.Reason;
            _reason.Multiline = true;
            _reason.ScrollBars = ScrollBars.Vertical;
            _reason.Height = 82;
            var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            var save = CreateButton("Save rule", 125, 36);
            var cancel = CreateButton("Cancel", 105, 36);
            save.Click += delegate { SaveRule(); };
            cancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            footer.Controls.Add(save);
            footer.Controls.Add(cancel);
            BodyPanel.Controls.Add(_layout);
            BodyPanel.Controls.Add(footer);
        }

        private void AddField(int row, string label, Control control)
        {
            _layout.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(100, 92, 120), Font = new Font("Segoe UI", 9f, FontStyle.Bold) }, 0, row);
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0, 7, 0, 5);
            _layout.Controls.Add(control, 1, row);
        }

        private void SaveRule()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_name.Text)) throw new InvalidOperationException("Rule name is required.");
                Rule.Name = _name.Text.Trim();
                Rule.Scope = (AiPolicyScopeKind)_scope.SelectedItem;
                Rule.ScopeId = _scopeId.Text.Trim();
                Rule.Priority = (int)_priority.Value;
                Rule.Outcome = (AiPolicyOutcome)_outcome.SelectedItem;
                Replace(Rule.Operations, Parse(_operations.Text));
                Replace(Rule.ResourceTypes, Parse(_resourceTypes.Text));
                Replace(Rule.ResourceIds, Parse(_resourceIds.Text));
                Replace(Rule.ToolIds, Parse(_toolIds.Text));
                Replace(Rule.ProviderIds, Parse(_providerIds.Text));
                Replace(Rule.ExecutionTargetIds, Parse(_targetIds.Text));
                Rule.Reason = _reason.Text;
                Rule.Validate();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { HMessage.ShowException(this, "The policy rule could not be saved.", "Policy rule", ex); }
        }

        private static List<string> Parse(string value)
        {
            return (value ?? string.Empty).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void Replace(IList<string> target, IEnumerable<string> values)
        {
            target.Clear();
            foreach (var value in values) target.Add(value);
        }

        private static HButton CreateButton(string text, int width, int height)
        {
            var button = new HButton { Text = text, Width = width, Height = height, RoundButton = true, Edge = 10, TextMargin = 8, Font = new Font("Segoe UI", 9.2f, FontStyle.Bold) };
            button.ButtonLeaveBackGroundColor1 = Color.FromArgb(92, 67, 168);
            button.ButtonLeaveBackGroundColor2 = Color.FromArgb(57, 40, 108);
            button.ButtonLeaveForeColor = Color.White;
            button.ButtonLeaveBorderColor = Color.FromArgb(116, 76, 210);
            button.ButtonEnterBackGroundColor1 = Color.FromArgb(126, 94, 214);
            button.ButtonEnterBackGroundColor2 = Color.FromArgb(79, 54, 145);
            button.ButtonEnterForeColor = Color.White;
            button.ButtonEnterBorderColor = Color.FromArgb(146, 118, 232);
            return button;
        }
    }
}
