using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;

namespace HAgent.WinForms.UI.Configuration.Resources
{
    internal sealed class AuthoritativeResourceInventoryPage : ConfigurationPageBase
    {
        private const int PageSize = 50;
        private readonly ConfigurationContext _context;
        private readonly ListView _list = new ListView();
        private readonly ComboBox _typeFilter = new ComboBox();
        private readonly ComboBox _scopeFilter = new ComboBox();
        private readonly ComboBox _lifecycleFilter = new ComboBox();
        private readonly ComboBox _updatedFilter = new ComboBox();
        private readonly TextBox _ownerFilter = new TextBox();
        private readonly TextBox _versionFilter = new TextBox();
        private readonly TextBox _search = new TextBox();
        private readonly CheckBox _authoritativeOnly = new CheckBox();
        private readonly Label _status = new Label();
        private readonly Label _pageStatus = new Label();
        private readonly HAgent.WinForms.Helpers.Button.HButton _previousPage;
        private readonly HAgent.WinForms.Helpers.Button.HButton _nextPage;
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
        private int _refreshRequest;
        private int _pageNumber;
        private bool _hasNextPage;

        public AuthoritativeResourceInventoryPage(ConfigurationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _previousPage = CreateActionButton("Previous", 90);
            _nextPage = CreateActionButton("Next", 75);
            Build();
            ClearDetails();
            UpdatePagingState();
        }

        public async Task RefreshDataAsync()
        {
            await RefreshDataAsync(true).ConfigureAwait(true);
        }

        private async Task RefreshDataAsync(bool resetPage)
        {
            if (resetPage) _pageNumber = 0;
            var request = ++_refreshRequest;
            _list.Items.Clear();
            ClearDetails();
            if (_context.ResourceInventory == null)
            {
                _hasNextPage = false;
                _status.Text = "Authoritative resource inventory is not configured.";
                UpdatePagingState();
                return;
            }

            var query = BuildQuery();
            query.SkipResults = _pageNumber * PageSize;
            query.MaxResults = PageSize + 1;

            _status.Text = "Loading resources...";
            UpdatePagingState();
            try
            {
                var items = await _context.ResourceInventory.ListAsync(query).ConfigureAwait(true);
                if (request != _refreshRequest) return;

                _hasNextPage = items.Count > PageSize;
                var displayCount = Math.Min(PageSize, items.Count);
                for (var index = 0; index < displayCount; index++)
                {
                    var item = items[index];
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

                _status.Text = displayCount == 0
                    ? "No resources match the current filters."
                    : displayCount + (_hasNextPage ? "+" : "") + " resource(s) on this page.";
                _pageStatus.Text = "Page " + (_pageNumber + 1);
                UpdatePagingState();
            }
            catch (Exception ex)
            {
                if (request != _refreshRequest) return;
                _hasNextPage = false;
                _status.Text = "Unable to load resources: " + ex.Message;
                UpdatePagingState();
            }
        }

        private AiResourceInventoryQuery BuildQuery()
        {
            var query = new AiResourceInventoryQuery
            {
                AuthoritativeOnly = _authoritativeOnly.Checked,
                SearchText = string.IsNullOrWhiteSpace(_search.Text) ? null : _search.Text.Trim(),
                OwnerId = string.IsNullOrWhiteSpace(_ownerFilter.Text) ? null : _ownerFilter.Text.Trim(),
                LifecycleStatus = GetFilterValue(_lifecycleFilter),
                Version = ParseVersionFilter(),
                UpdatedAfterUtc = ResolveUpdatedAfterUtc()
            };
            if (_typeFilter.SelectedIndex > 0)
                query.ResourceTypes.Add(_typeFilter.SelectedItem.ToString());
            if (_scopeFilter.SelectedIndex > 0)
                query.Scope = (AgentResourceScope)Enum.Parse(typeof(AgentResourceScope), _scopeFilter.SelectedItem.ToString());
            return query;
        }

        private long? ParseVersionFilter()
        {
            if (string.IsNullOrWhiteSpace(_versionFilter.Text)) return null;
            long version;
            return long.TryParse(_versionFilter.Text.Trim(), out version) && version > 0 ? (long?)version : null;
        }

        private static string GetFilterValue(ComboBox combo)
        {
            var value = combo.Text == null ? string.Empty : combo.Text.Trim();
            return string.IsNullOrWhiteSpace(value) || string.Equals(value, "All", StringComparison.OrdinalIgnoreCase) ? null : value;
        }

        private DateTimeOffset? ResolveUpdatedAfterUtc()
        {
            switch (_updatedFilter.SelectedIndex)
            {
                case 1: return DateTimeOffset.UtcNow.Date;
                case 2: return DateTimeOffset.UtcNow.AddDays(-7);
                case 3: return DateTimeOffset.UtcNow.AddDays(-30);
                case 4: return DateTimeOffset.UtcNow.AddDays(-90);
                default: return null;
            }
        }

        private void Build()
        {
            var root = CreateListPageRoot(94);
            root.Controls.Add(CreateHeader("Authoritative Resources", "Browse authoritative resources through a bounded master list and inspect the selected resource in a resizable detail pane."));
            root.Controls.Add(CreateActionBar());
            root.Controls.Add(CreateWorkspace());
            Controls.Add(root);
        }

        private Control CreateActionBar()
        {
            var outer = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(0) };
            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                BackColor = Surface,
                Padding = new Padding(0, 2, 0, 2),
                Margin = new Padding(0)
            };

            ConfigureComboBox(_typeFilter, 105, false);
            _typeFilter.Items.Add("All");
            _typeFilter.Items.Add("memory");
            _typeFilter.Items.Add("knowledge");
            _typeFilter.Items.Add("wiki");
            _typeFilter.Items.Add("skill");
            _typeFilter.SelectedIndex = 0;

            ConfigureComboBox(_scopeFilter, 105, false);
            _scopeFilter.Items.Add("All");
            foreach (var value in Enum.GetValues(typeof(AgentResourceScope))) _scopeFilter.Items.Add(value.ToString());
            _scopeFilter.SelectedIndex = 0;

            ConfigureComboBox(_lifecycleFilter, 105, true);
            _lifecycleFilter.Items.Add("All");
            _lifecycleFilter.Items.Add("Draft");
            _lifecycleFilter.Items.Add("Published");
            _lifecycleFilter.SelectedIndex = 0;

            ConfigureComboBox(_updatedFilter, 105, false);
            _updatedFilter.Items.Add("Any time");
            _updatedFilter.Items.Add("Today");
            _updatedFilter.Items.Add("Last 7 days");
            _updatedFilter.Items.Add("Last 30 days");
            _updatedFilter.Items.Add("Last 90 days");
            _updatedFilter.SelectedIndex = 0;

            ConfigureFilterTextBox(_ownerFilter, 135);
            ConfigureFilterTextBox(_versionFilter, 70);
            ConfigureFilterTextBox(_search, 180);
            _versionFilter.KeyPress += ValidateVersionKeyPress;
            _authoritativeOnly.Text = "Authoritative";
            _authoritativeOnly.AutoSize = true;
            _authoritativeOnly.Checked = true;
            _authoritativeOnly.Margin = new Padding(8, 22, 0, 0);

            filters.Controls.Add(CreateFilterGroup("Type", _typeFilter, 112));
            filters.Controls.Add(CreateFilterGroup("Scope", _scopeFilter, 112));
            filters.Controls.Add(CreateFilterGroup("Lifecycle", _lifecycleFilter, 112));
            filters.Controls.Add(CreateFilterGroup("Agent / Owner", _ownerFilter, 142));
            filters.Controls.Add(CreateFilterGroup("Version", _versionFilter, 80));
            filters.Controls.Add(CreateFilterGroup("Updated", _updatedFilter, 112));
            filters.Controls.Add(CreateFilterGroup("Search", _search, 187));
            filters.Controls.Add(_authoritativeOnly);

            var apply = CreateActionButton("Apply", 75);
            apply.Click += async delegate { await RefreshDataAsync(true); };
            var reset = CreateActionButton("Reset", 75);
            reset.Click += async delegate
            {
                _typeFilter.SelectedIndex = 0;
                _scopeFilter.SelectedIndex = 0;
                _lifecycleFilter.SelectedIndex = 0;
                _updatedFilter.SelectedIndex = 0;
                _ownerFilter.Clear();
                _versionFilter.Clear();
                _search.Clear();
                _authoritativeOnly.Checked = true;
                await RefreshDataAsync(true);
            };
            var refresh = CreateActionButton("Refresh", 82);
            refresh.Click += async delegate { await RefreshDataAsync(false); };
            _previousPage.Margin = new Padding(8, 19, 0, 0);
            _nextPage.Margin = new Padding(4, 19, 0, 0);
            _previousPage.Click += async delegate
            {
                if (_pageNumber == 0) return;
                _pageNumber--;
                await RefreshDataAsync(false);
            };
            _nextPage.Click += async delegate
            {
                if (!_hasNextPage) return;
                _pageNumber++;
                await RefreshDataAsync(false);
            };
            _pageStatus.AutoSize = true;
            _pageStatus.ForeColor = Muted;
            _pageStatus.Margin = new Padding(8, 30, 0, 0);
            _status.AutoSize = true;
            _status.ForeColor = Muted;
            _status.Margin = new Padding(8, 30, 0, 0);
            filters.Controls.Add(WrapTopAligned(apply));
            filters.Controls.Add(WrapTopAligned(reset));
            filters.Controls.Add(WrapTopAligned(refresh));
            filters.Controls.Add(_previousPage);
            filters.Controls.Add(_nextPage);
            filters.Controls.Add(_pageStatus);
            filters.Controls.Add(_status);
            outer.Controls.Add(filters);
            return outer;
        }

        private static Panel WrapTopAligned(Control control)
        {
            var panel = new Panel { Width = control.Width, Height = 44, Margin = new Padding(8, 0, 0, 0) };
            control.Dock = DockStyle.Top;
            control.Margin = new Padding(0);
            panel.Controls.Add(control);
            return panel;
        }

        private static Panel CreateFilterGroup(string caption, Control control, int width)
        {
            var group = new Panel { Width = width, Height = 42, Margin = new Padding(0, 0, 7, 0) };
            var label = new Label { Text = caption, AutoSize = true, ForeColor = Muted, Left = 0, Top = 0 };
            control.Left = 0;
            control.Top = 17;
            control.Width = width;
            group.Controls.Add(control);
            group.Controls.Add(label);
            return group;
        }

        private static void ConfigureComboBox(ComboBox combo, int width, bool editable)
        {
            combo.Width = width;
            combo.Height = 24;
            combo.DropDownStyle = editable ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
            combo.Margin = new Padding(0);
        }

        private static void ConfigureFilterTextBox(TextBox textBox, int width)
        {
            textBox.Width = width;
            textBox.Height = 24;
            textBox.Margin = new Padding(0);
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void ValidateVersionKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        private Control CreateWorkspace()
        {
            ConfigureList(_list);
            _list.Columns.Add("Type", 70);
            _list.Columns.Add("Resource", 160);
            _list.Columns.Add("Name", 160);
            _list.Columns.Add("Status", 85);
            _list.Columns.Add("Scope", 80);
            _list.Columns.Add("Owner", 110);
            _list.Columns.Add("Version", 60);
            _list.Columns.Add("Updated", 125);
            _list.SelectedIndexChanged += async delegate { await DisplayDetailsAsync(); };

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.None,
                Panel1MinSize = 340,
                Panel2MinSize = 320,
                BackColor = Surface,
                SplitterWidth = 6
            };
            split.Panel1.Padding = new Padding(0, 0, 6, 0);
            split.Panel2.Padding = new Padding(6, 0, 0, 0);
            split.Panel1.Controls.Add(_list);
            split.Panel2.Controls.Add(CreateDetails());
            split.Resize += delegate
            {
                var target = (int)(split.ClientSize.Width * 0.55f);
                if (target < split.Panel1MinSize) target = split.Panel1MinSize;
                if (target > split.ClientSize.Width - split.Panel2MinSize) target = split.ClientSize.Width - split.Panel2MinSize;
                if (target > 0 && target < split.ClientSize.Width) split.SplitterDistance = target;
            };
            split.Layout += delegate
            {
                if (split.SplitterDistance <= split.Panel1MinSize)
                    split.SplitterDistance = Math.Max(split.Panel1MinSize, (int)(split.ClientSize.Width * 0.55f));
            };
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
            _detailState.Height = 34;
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

            var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true };
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
                    AddProperty(field.Name, field.Value);
                AddContentBlock("Content", detail.Content);
                foreach (var section in detail.Sections)
                    AddContentBlock(section.Title, section.Content);
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
                Height = string.Equals(title, "Content", StringComparison.OrdinalIgnoreCase) ? 230 : 170,
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

        private void UpdatePagingState()
        {
            _previousPage.Enabled = _pageNumber > 0;
            _nextPage.Enabled = _hasNextPage;
            _pageStatus.Text = "Page " + (_pageNumber + 1);
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
