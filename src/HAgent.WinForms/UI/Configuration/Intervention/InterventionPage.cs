using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.UI.Configuration.Intervention
{
    internal sealed class InterventionPage : ConfigurationPageBase
    {
        private readonly ConfigurationContext _context;
        private readonly ListView _list = new ListView();
        private readonly Label _status = new Label();
        private readonly System.Windows.Forms.Timer _refreshTimer = new System.Windows.Forms.Timer();

        public InterventionPage(ConfigurationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Build();
            _refreshTimer.Interval = 1000;
            _refreshTimer.Tick += async delegate { await RefreshDataAsync(); };
            _refreshTimer.Start();
            Disposed += delegate { _refreshTimer.Stop(); _refreshTimer.Dispose(); };
        }

        private void Build()
        {
            var root = CreateListPageRoot();
            root.Controls.Add(CreateHeader("Interventions", "Review pending human-control requests and inspect intervention history."), 0, 0);

            var actions = CreateActionPanel();
            actions.Controls.Add(CreateActionButton("Approve", 92));
            actions.Controls.Add(CreateActionButton("Reject", 92, true));
            actions.Controls.Add(CreateActionButton("Cancel", 92, true));
            actions.Controls.Add(CreateActionButton("Apply", 92));
            actions.Controls.Add(CreateActionButton("Refresh", 92));

            var buttons = actions.Controls.OfType<HButton>().ToList();
            buttons[0].Click += async delegate { await ResolveSelectedAsync(AiInterventionRequestStatus.Approved); };
            buttons[1].Click += async delegate { await ResolveSelectedAsync(AiInterventionRequestStatus.Rejected); };
            buttons[2].Click += async delegate { await ResolveSelectedAsync(AiInterventionRequestStatus.Cancelled); };
            buttons[3].Click += async delegate { await ApplySelectedAsync(); };
            buttons[4].Click += async delegate { await RefreshDataAsync(); };

            _status.AutoSize = true;
            _status.Left = 8;
            _status.Margin = new Padding(12, 10, 0, 0);
            _status.ForeColor = Muted;
            actions.Controls.Add(_status);
            root.Controls.Add(actions, 0, 1);

            ConfigureList(_list);
            _list.Columns.Add("Status", 105);
            _list.Columns.Add("Target", 125);
            _list.Columns.Add("Action", 105);
            _list.Columns.Add("Resource", 180);
            _list.Columns.Add("Agent", 150);
            _list.Columns.Add("Reason", 310);
            _list.Columns.Add("Created", 135);
            root.Controls.Add(_list, 0, 2);
            Controls.Add(root);
        }

        public async Task RefreshDataAsync()
        {
            if (IsDisposed) return;
            try
            {
                var items = await _context.InterventionWorkflow.SearchAsync(new AiInterventionQuery { MaxResults = 200 }, CancellationToken.None).ConfigureAwait(true);
                _list.BeginUpdate();
                try
                {
                    _list.Items.Clear();
                    foreach (var request in items)
                    {
                        var item = new ListViewItem(request.Status.ToString());
                        item.SubItems.Add(request.TargetKind.ToString());
                        item.SubItems.Add(request.RequestedAction.ToString());
                        item.SubItems.Add(string.IsNullOrWhiteSpace(request.ResourceId) ? request.ExecutionId : request.ResourceId);
                        item.SubItems.Add(request.AgentProfileId ?? string.Empty);
                        item.SubItems.Add(request.Reason ?? string.Empty);
                        item.SubItems.Add(request.CreatedAt.LocalDateTime.ToString("g"));
                        item.Tag = request;
                        _list.Items.Add(item);
                    }
                }
                finally { _list.EndUpdate(); }
                var pending = items.Count(x => x.Status == AiInterventionRequestStatus.Pending);
                _status.Text = pending + " pending / " + items.Count + " total" +
                               (_context.InterventionCoordinator == null ? " / runtime apply unavailable" : "");
            }
            catch (Exception ex)
            {
                _status.Text = "Unable to load interventions: " + ex.Message;
            }
        }

        private AiInterventionRequest Selected()
        {
            return _list.SelectedItems.Count == 0 ? null : _list.SelectedItems[0].Tag as AiInterventionRequest;
        }

        private async Task ResolveSelectedAsync(AiInterventionRequestStatus resolution)
        {
            var request = Selected();
            if (request == null)
            {
                _status.Text = "Select an intervention request first.";
                return;
            }
            try
            {
                await _context.InterventionWorkflow.ResolveAsync(
                    request.RequestId,
                    resolution,
                    _context.CurrentIdentity,
                    "Resolved from the HAgent intervention management UI.",
                    CancellationToken.None).ConfigureAwait(true);
                await RefreshDataAsync().ConfigureAwait(true);
            }
            catch (Exception ex) { _status.Text = "Resolution failed: " + ex.Message; }
        }

        private async Task ApplySelectedAsync()
        {
            var request = Selected();
            if (request == null)
            {
                _status.Text = "Select an intervention request first.";
                return;
            }
            if (_context.InterventionCoordinator == null)
            {
                _status.Text = "No runtime coordinator is connected to this configuration surface.";
                return;
            }
            try
            {
                var result = await _context.InterventionCoordinator.ApplyAsync(
                    request.RequestId,
                    _context.CurrentIdentity,
                    "Applied from the HAgent intervention management UI.",
                    CancellationToken.None).ConfigureAwait(true);
                _status.Text = result.Reason ?? result.Status.ToString();
                await RefreshDataAsync().ConfigureAwait(true);
            }
            catch (Exception ex) { _status.Text = "Application failed: " + ex.Message; }
        }
    }
}
