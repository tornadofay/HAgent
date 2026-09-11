using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.WinForms.Helpers;

namespace HAgent.WinForms.UI.Configuration.Learning
{
    internal sealed class LearningReviewPage : ConfigurationPageBase
    {
        private readonly ConfigurationContext _context;
        private readonly ListView _list = new ListView();
        private readonly Label _userId = new Label { AutoSize = true, ForeColor = Text };
        private readonly Label _tenantId = new Label { AutoSize = true, ForeColor = Text };
        private readonly Label _workspaceId = new Label { AutoSize = true, ForeColor = Text };
        private readonly Label _status = new Label { AutoSize = true, ForeColor = Muted };

        public LearningReviewPage(ConfigurationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Build();
            ApplyIdentityDisplay();
        }

        public async Task RefreshDataAsync()
        {
            _list.Items.Clear();
            ApplyIdentityDisplay();

            if (_context.LearningCandidates == null)
            {
                _status.Text = "Learning candidate storage is not configured.";
                return;
            }

            var candidates = await _context.LearningCandidates.QueryAsync(new AiLearningCandidateQuery
            {
                Status = AiLearningCandidateStatus.PendingReview,
                MaxResults = 100
            });

            foreach (var candidate in candidates)
            {
                var item = new ListViewItem(new[]
                {
                    candidate.CandidateType.ToString(),
                    candidate.CandidateId,
                    candidate.ProposedScope,
                    candidate.SourceAgentProfileId,
                    candidate.Confidence.HasValue ? candidate.Confidence.Value.ToString("0.00") : "",
                    candidate.Revision.ToString(),
                    candidate.UpdatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                });
                item.Tag = candidate;
                _list.Items.Add(item);
            }

            _status.Text = candidates.Count == 0 ? "No PendingReview candidates." : candidates.Count + " candidate(s) pending review.";
        }

        private void Build()
        {
            var root = CreateListPageRoot();
            root.Controls.Add(CreateHeader("Learning Review", "Review durable learning candidates through the existing unified policy boundary."));
            root.Controls.Add(CreateActionBar());
            root.Controls.Add(CreateListContent());
            Controls.Add(root);
            Load += async delegate { await RefreshDataAsync(); };
        }

        private void ApplyIdentityDisplay()
        {
            var identity = _context.ReviewerIdentity;
            _userId.Text = string.IsNullOrWhiteSpace(identity.UserId) ? AISettings.DefaultSystemAdminUserId : identity.UserId;
            _tenantId.Text = string.IsNullOrWhiteSpace(identity.TenantId) ? "Not supplied" : identity.TenantId;
            _workspaceId.Text = string.IsNullOrWhiteSpace(identity.WorkspaceId) ? "Not supplied" : identity.WorkspaceId;
        }

        private Control CreateActionBar()
        {
            var outer = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            var actions = CreateActionPanel();
            var approve = CreateActionButton("Approve", 100);
            var reject = CreateActionButton("Reject", 100, true);
            var refresh = CreateActionButton("Refresh", 100);
            approve.Click += async delegate { await ReviewAsync(AiLearningCandidateReviewAction.Approve); };
            reject.Click += async delegate { await ReviewAsync(AiLearningCandidateReviewAction.Reject); };
            refresh.Click += async delegate { await RefreshDataAsync(); };

            actions.Controls.Add(new Label { Text = "Reviewer user:", AutoSize = true, ForeColor = Muted, Margin = new Padding(0, 10, 4, 0) });
            actions.Controls.Add(_userId);
            actions.Controls.Add(new Label { Text = "Tenant:", AutoSize = true, ForeColor = Muted, Margin = new Padding(10, 10, 4, 0) });
            actions.Controls.Add(_tenantId);
            actions.Controls.Add(new Label { Text = "Workspace:", AutoSize = true, ForeColor = Muted, Margin = new Padding(10, 10, 4, 0) });
            actions.Controls.Add(_workspaceId);
            actions.Controls.Add(approve);
            actions.Controls.Add(reject);
            actions.Controls.Add(refresh);
            actions.Controls.Add(_status);
            outer.Controls.Add(actions);
            return outer;
        }

        private Control CreateListContent()
        {
            ConfigureList(_list);
            _list.Columns.Add("Type", 90);
            _list.Columns.Add("Candidate", 220);
            _list.Columns.Add("Scope", 90);
            _list.Columns.Add("Source Agent", 150);
            _list.Columns.Add("Confidence", 90);
            _list.Columns.Add("Revision", 70);
            _list.Columns.Add("Updated", 150);
            return _list;
        }

        private async Task ReviewAsync(AiLearningCandidateReviewAction action)
        {
            if (_list.SelectedItems.Count == 0)
            {
                HMessage.ShowInformation(FindForm(), "Select a PendingReview candidate first.", "Learning Review");
                return;
            }

            var record = _list.SelectedItems[0].Tag as AiLearningCandidateRecord;
            if (record == null || _context.LearningCandidates == null) return;

            var identity = _context.ReviewerIdentity;
            var policy = new DefaultAiPolicyEngine(await _context.Store.GetPolicySetAsync());
            var reviewer = new AiLearningCandidateReviewService(_context.LearningCandidates, policy);

            try
            {
                var updated = await reviewer.ReviewAsync(
                    record.CandidateId,
                    action,
                    identity,
                    action == AiLearningCandidateReviewAction.Approve ? "Approved from Learning Review UI." : "Rejected from Learning Review UI.");
                _status.Text = "Candidate " + updated.CandidateId + " -> " + updated.Status + ".";
                await RefreshDataAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                HMessage.ShowError(FindForm(), ex.Message, "Learning Review");
            }
            catch (Exception ex)
            {
                HMessage.ShowException(FindForm(), "The learning review operation failed.", "Learning Review", ex);
            }
        }
    }
}
