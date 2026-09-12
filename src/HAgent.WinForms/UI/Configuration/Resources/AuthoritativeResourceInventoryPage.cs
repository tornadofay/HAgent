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
        private readonly Label _detailTitle = new Label();
        private readonly Label _detailType = new Label();
        private readonly Label _detailSummary = new Label();
        private readonly TableLayoutPanel _properties = new TableLayoutPanel();
        private readonly TabControl _detailTabs = new TabControl();
        private readonly TabPage _overviewTab = new TabPage("Overview");
        private readonly TabPage _contentTab = new TabPage("Content");
        private readonly Panel _contentHost = new Panel();
        private readonly Label _detailState = new Label();
        private int _detailRequest;

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
            root.Controls.Add(CreateHeader("Authoritative Resources", "Browse authoritative resources and inspect their actual readable content through provider-neutral contracts."));
            root.Controls.Add(CreateActionBar());
            root.Controls.Add(CreateWorkspace());
            Controls.Add(root);
        }

        private Control CreateActionBar()
        {
            var outer = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            var actions = CreateActionPanel();
            ConfigureComboBox(_typeFilter);
            _typeFilter.Items.Add("All");
            _typeFilter.Items.Add("memory");
            _typeFilter.Items.Add("knowledge");
            _typeFilter.Items.Add("wiki");
            _typeFilter.Items.Add("skill");
            _typeFilter.SelectedIndex = 0;
            _typeFilter.SelectedIndexChanged += async delegate { await RefreshDataAsync(); };

            ConfigureComboBox(_scopeFilter);
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

        private static void ConfigureComboBox(ComboBox combo)
        {
            combo.Width = 120;
            combo.Height = 28;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Margin = new Padding(0, 5, 0, 0);
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
            _list.SelectedIndexChanged += async delegate { await DisplayDetailsAsync(); };

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

            _detailTitle.Dock = DockStyle.Top;
            _detailTitle.AutoSize = false;
            _detailTitle.Height = 28;
            _detailTitle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            _detailTitle.ForeColor = Color.FromArgb(48, 43, 70);

            _detailType.Dock = DockStyle.Top;
            _detailType.Height = 24;
            _detailType.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            _detailType.ForeColor = Muted;

            _detailState.Dock = DockStyle.Bottom;
            _detailState.Height = 30;
            _detailState.TextAlign = ContentAlignment.MiddleLeft;
            _detailState.ForeColor = Muted;

            _detailTabs.Dock = DockStyle.Fill;
            _detailTabs.TabPages.Add(_overviewTab);
            _detailTabs.TabPages.Add(_contentTab);

            ConfigureOverviewTab();
            ConfigureContentTab();

            panel.Controls.Add(_detailTabs);
            panel.Controls.Add(_detailState);
            panel.Controls.Add(_detailType);
            panel.Controls.Add(_detailTitle);
            return panel;
        }

        private void ConfigureOverviewTab()
        {
            _overviewTab.BackColor = Color.White;
            _overviewTab.Padding = new Padding(8);

            var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            _properties.Dock = DockStyle.Top;
            _properties.AutoSize = true;
            _properties.ColumnCount = 2;
            _properties.RowCount = 0;
            _properties.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105f));
            _properties.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.Controls.Add(_properties);

            _detailSummary.AutoSize = true;
            _detailSummary.Dock = DockStyle.Top;
            _detailSummary.Padding = new Padding(0, 14, 0, 0);
            _detailSummary.Font = new Font("Segoe UI", 9f);
            _detailSummary.ForeColor = Color.FromArgb(68, 62, 88);
            root.Controls.Add(_detailSummary);
            _overviewTab.Controls.Add(root);
        }

        private void ConfigureContentTab()
        {
            _contentTab.BackColor = Color.White;
            _contentTab.Padding = new Padding(8);
            _contentHost.Dock = DockStyle.Fill;
            _contentHost.AutoScroll = true;
            _contentHost.BackColor = Color.White;
            _contentTab.Controls.Add(_contentHost);
        }

        private static Label CreatePropertyLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(112, 105, 130),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding = new Padding(0, 2, 8, 2)
            };
        }

        private static Label CreatePropertyValue(string text)
        {
            return new Label
            {
                Text = text ?? string.Empty,
                Dock = DockStyle.Fill,
                AutoSize = false,
                ForeColor = Color.FromArgb(55, 50, 70),
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(0, 2, 0, 2)
            };
        }

        private async Task DisplayDetailsAsync()
        {
            if (_list.SelectedItems.Count == 0) { ClearDetails(); return; }
            var item = _list.SelectedItems[0].Tag as AiResourceInventoryItem;
            if (item == null) { ClearDetails(); return; }

            var request = ++_detailRequest;
            SetInventoryMetadata(item);
            ClearContentHost();

            if (_context.ResourceDetails == null)
            {
                _detailState.Text = "Read-only inventory metadata is available; this host has not supplied a resource detail source.";
                return;
            }

            _detailState.Text = "Loading resource content...";
            try
            {
                var detail = await _context.ResourceDetails.GetAsync(item);
                if (request != _detailRequest) return;
                if (detail == null) throw new InvalidOperationException("The resource detail source returned no detail projection.");
                detail.Validate();
                if (!string.Equals(detail.InventoryItem.ResourceId, item.ResourceId, StringComparison.Ordinal) ||
                    !string.Equals(detail.InventoryItem.ResourceType, item.ResourceType, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("The resource detail source returned a different resource than the selected inventory item.");

                _detailSummary.Text = string.IsNullOrWhiteSpace(detail.Summary) ? "" : "Summary: " + detail.Summary;
                foreach (var field in detail.Fields)
                {
                    AddProperty(field.Name, field.Value);
                }
                AddContentBlock("Content", detail.Content);
                foreach (var section in detail.Sections)
                {
                    AddContentBlock(section.Title, section.Content);
                }
                _detailState.Text = "Read-only inspection. Editing, deletion, publishing, and lifecycle actions are separate governed operations.";
            }
            catch (Exception ex)
            {
                if (request != _detailRequest) return;
                _detailState.Text = "Unable to read this resource: " + ex.Message;
            }
        }

        private void SetInventoryMetadata(AiResourceInventoryItem item)
        {
            _detailTitle.Text = string.IsNullOrWhiteSpace(item.DisplayName) ? item.ResourceId : item.DisplayName;
            _detailType.Text = item.ResourceType + "  •  " + (item.LifecycleStatus ?? "") + "  •  " + (item.IsAuthoritative ? "Authoritative" : "Non-authoritative");
            _properties.SuspendLayout();
            _properties.Controls.Clear();
            _properties.RowStyles.Clear();
            _properties.RowCount = 0;
            AddProperty("Resource ID", item.ResourceId);
            AddProperty("Type", item.ResourceType);
            AddProperty("Lifecycle", item.LifecycleStatus);
            AddProperty("Scope", item.Scope.ToString());
            AddProperty("Owner", item.OwnerId);
            AddProperty("Version", item.Version.HasValue ? item.Version.Value.ToString() : "N/A");
            AddProperty("Updated", item.UpdatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            AddProperty("Source", item.Source);
            _detailSummary.Text = "";
            _properties.ResumeLayout();
        }

        private void AddProperty(string name, string value)
        {
            var row = _properties.RowCount++;
            _properties.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _properties.Controls.Add(CreatePropertyLabel(name), 0, row);
            _properties.Controls.Add(CreatePropertyValue(value), 1, row);
        }

        private void AddContentBlock(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;
            var group = new GroupBox
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = title.Equals("Content", StringComparison.OrdinalIgnoreCase) ? 230 : 170,
                Padding = new Padding(8),
                Margin = new Padding(0, 0, 0, 10),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            var text = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9f),
                ForeColor = Color.FromArgb(48, 44, 64),
                Text = content
            };
            group.Controls.Add(text);
            _contentHost.Controls.Add(group);
            group.BringToFront();
        }

        private void ClearContentHost()
        {
            foreach (Control control in _contentHost.Controls) control.Dispose();
            _contentHost.Controls.Clear();
        }

        private void ClearDetails()
        {
            ++_detailRequest;
            _detailTitle.Text = "No resource selected";
            _detailType.Text = "Select a resource from the list.";
            _detailState.Text = "";
            _properties.Controls.Clear();
            _properties.RowStyles.Clear();
            _properties.RowCount = 0;
            _detailSummary.Text = "";
            ClearContentHost();
        }
    }
}