using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;

namespace HAgent.WinForms.UI.Configuration.Resources
{
    internal sealed class AuthoritativeResourceInventoryPage : ConfigurationPageBase
    {
        private readonly ConfigurationContext _context;
        private readonly ListView _list = new ListView();
        private readonly ComboBox _typeFilter = new ComboBox();
        private readonly ComboBox _scopeFilter = new ComboBox();
        private readonly CheckBox _authoritativeOnly = new CheckBox();
        private readonly TextBox _search = new TextBox();
        private readonly Label _status = new Label();
        private readonly Label _details = new Label();

        public AuthoritativeResourceInventoryPage(ConfigurationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Build();
            ClearDetails();
        }

        public async Task RefreshDataAsync()
        {
            _list.Items.Clear();
            ClearDetails();
            if (_context.ResourceInventory == null)
            {
                _status.Text = "Authoritative resource inventory is not configured.";
                return;
            }

            var query = new AiResourceInventoryQuery
            {
                AuthoritativeOnly = _authoritativeOnly.Checked,
                SearchText = string.IsNullOrWhiteSpace(_search.Text) ? null : _search.Text.Trim(),
                MaxResults = 500
            };
            if (_typeFilter.SelectedIndex > 0)
                query.ResourceTypes.Add(_typeFilter.SelectedItem.ToString());
            if (_scopeFilter.SelectedIndex > 0)
                query.Scope = (AgentResourceScope)Enum.Parse(typeof(AgentResourceScope), _scopeFilter.SelectedItem.ToString());

            var items = await _context.ResourceInventory.ListAsync(query);
            foreach (var item in items)
            {
                var row = new ListViewItem(new[]
                {
                    item.ResourceType,
                    item.ResourceId,
                    item.DisplayName ?? string.Empty,
                    item.LifecycleStatus ?? string.Empty,
                    item.Scope.ToString(),
                    item.OwnerId ?? string.Empty,
                    item.Version.HasValue ? item.Version.Value.ToString() : "",
                    item.UpdatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                });
                row.Tag = item;
                _list.Items.Add(row);
            }
            _status.Text = items.Count == 0 ? "No authoritative resources match the current filters." : items.Count + " resource(s) match the current filters.";
        }

        private void Build()
        {
            var root = CreateListPageRoot();
            root.Controls.Add(CreateHeader("Authoritative Resources", "Inspect authoritative Memory, Knowledge/Wiki, and Skill resources through the provider-neutral inventory contract."));
            root.Controls.Add(CreateActionBar());
            root.Controls.Add(CreateWorkspace());
            Controls.Add(root);
            Load += async delegate { await RefreshDataAsync(); };
        }

        private Control CreateActionBar()
        {
            var outer = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            var actions = CreateActionPanel();
            ConfigureFilter(_typeFilter);
            _typeFilter.Items.Add("All");
            _typeFilter.Items.Add("memory");
            _typeFilter.Items.Add("knowledge");
            _typeFilter.Items.Add("wiki");
            _typeFilter.Items.Add("skill");
            _typeFilter.SelectedIndex = 0;
            _typeFilter.SelectedIndexChanged += async delegate { await RefreshDataAsync(); };

            ConfigureFilter(_scopeFilter);
            _scopeFilter.Items.Add("All");
            foreach (var value in Enum.GetValues(typeof(AgentResourceScope))) _scopeFilter.Items.Add(value.ToString());
            _scopeFilter.SelectedIndex = 0;
            _scopeFilter.SelectedIndexChanged += async delegate { await RefreshDataAsync(); };

            _search.Width = 180;
            _search.Margin = new Padding(8, 5, 8, 0);
            _search.KeyDown += async delegate(object sender, KeyEventArgs args)
            {
                if (args.KeyCode != Keys.Enter) return;
                args.SuppressKeyPress = true;
                await RefreshDataAsync();
            };
            _authoritativeOnly.Text = "Authoritative only";
            _authoritativeOnly.Checked = true;
            _authoritativeOnly.AutoSize = true;
            _authoritativeOnly.ForeColor = Muted;
            _authoritativeOnly.Margin = new Padding(8, 8, 8, 0);
            _authoritativeOnly.CheckedChanged += async delegate { await RefreshDataAsync(); };
            var refresh = CreateActionButton("Refresh", 90);
            refresh.Click += async delegate { await RefreshDataAsync(); };

            actions.Controls.Add(new Label { Text = "Type:", AutoSize = true, ForeColor = Muted, Margin = new Padding(0, 10, 5, 0) });
            actions.Controls.Add(_typeFilter);
            actions.Controls.Add(new Label { Text = "Scope:", AutoSize = true, ForeColor = Muted, Margin = new Padding(10, 10, 5, 0) });
            actions.Controls.Add(_scopeFilter);
            actions.Controls.Add(_authoritativeOnly);
            actions.Controls.Add(new Label { Text = "Search:", AutoSize = true, ForeColor = Muted, Margin = new Padding(8, 10, 4, 0) });
            actions.Controls.Add(_search);
            actions.Controls.Add(refresh);
            actions.Controls.Add(_status);
            outer.Controls.Add(actions);
            return outer;
        }

        private Control CreateWorkspace()
        {
            ConfigureList(_list);
            _list.Columns.Add("Type", 80);
            _list.Columns.Add("Resource", 185);
            _list.Columns.Add("Name", 190);
            _list.Columns.Add("Status", 95);
            _list.Columns.Add("Scope", 90);
            _list.Columns.Add("Owner", 130);
            _list.Columns.Add("Version", 65);
            _list.Columns.Add("Updated", 145);
            _list.SelectedIndexChanged += delegate { DisplayDetails(); };
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, BackColor = Surface, SplitterDistance = 720 };
            split.Panel1.Padding = new Padding(0, 0, 6, 0);
            split.Panel2.Padding = new Padding(6, 0, 0, 0);
            split.Panel1.Controls.Add(_list);
            split.Panel2.Controls.Add(CreateDetails());
            return split;
        }

        private Control CreateDetails()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(14) };
            _details.Dock = DockStyle.Fill;
            _details.ForeColor = Text;
            _details.Font = new Font("Segoe UI", 9f);
            _details.TextAlign = ContentAlignment.TopLeft;
            panel.Controls.Add(_details);
            return panel;
        }

        private void DisplayDetails()
        {
            if (_list.SelectedItems.Count == 0) { ClearDetails(); return; }
            var item = _list.SelectedItems[0].Tag as AiResourceInventoryItem;
            if (item == null) { ClearDetails(); return; }
            _details.Text =
                "Resource type: " + item.ResourceType + Environment.NewLine +
                "Resource ID: " + item.ResourceId + Environment.NewLine +
                "Display name: " + (item.DisplayName ?? "") + Environment.NewLine +
                "Lifecycle: " + (item.LifecycleStatus ?? "") + Environment.NewLine +
                "Authoritative: " + (item.IsAuthoritative ? "Yes" : "No") + Environment.NewLine +
                "Scope: " + item.Scope + Environment.NewLine +
                "Owner: " + (item.OwnerId ?? "") + Environment.NewLine +
                "Version: " + (item.Version.HasValue ? item.Version.Value.ToString() : "N/A") + Environment.NewLine +
                "Updated: " + item.UpdatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine +
                "Source: " + (item.Source ?? "");
        }

        private void ClearDetails() { _details.Text = "Select a resource to inspect its bounded inventory metadata."; }
    }
}
