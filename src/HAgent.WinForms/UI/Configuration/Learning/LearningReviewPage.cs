using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.UI.Configuration.Learning
{
    internal sealed class LearningReviewPage : ConfigurationPageBase
    {
        private readonly ConfigurationContext _context;
        private readonly ListView _list = new ListView();
        private readonly ComboBox _statusFilter = new ComboBox();
        private readonly ComboBox _typeFilter = new ComboBox();
        private readonly Label _userId = new Label { AutoSize = true, ForeColor = Color.FromArgb(68, 62, 88) };
        private readonly Label _tenantId = new Label { AutoSize = true, ForeColor = Color.FromArgb(68, 62, 88) };
        private readonly Label _workspaceId = new Label { AutoSize = true, ForeColor = Color.FromArgb(68, 62, 88) };
        private readonly Label _status = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailCandidateId = new Label { AutoSize = true, ForeColor = Heading };
        private readonly Label _detailStatus = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailType = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailScope = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailSourceAgent = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailRevision = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailConfidence = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailEvidenceState = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailProvenanceState = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailContradictionState = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailRetention = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailEvaluation = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailCreated = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailUpdated = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailExpiry = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailPolicy = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailAuthorization = new Label { AutoSize = true, ForeColor = Muted };
        private readonly Label _detailLastReview = new Label { AutoSize = true, ForeColor = Muted };
        private readonly RichTextBox _detailPayload = CreateReadOnlyTextBox();
        private readonly RichTextBox _detailProvenance = CreateReadOnlyTextBox();
        private readonly RichTextBox _detailEvidence = CreateReadOnlyTextBox();
        private readonly RichTextBox _detailReviewEvidence = CreateReadOnlyTextBox();
        private readonly RichTextBox _detailSource = CreateReadOnlyTextBox();
        private readonly HButton _approveButton;
        private readonly HButton _rejectButton;

        public LearningReviewPage(ConfigurationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _approveButton = CreateActionButton("Approve", 100);
            _rejectButton = CreateActionButton("Reject", 100, true);
            Build();
            ApplyIdentityDisplay();
            ClearDetails();
        }

        public async Task RefreshDataAsync()
        {
            var selectedId = GetSelectedCandidateId();
            _list.Items.Clear();
            ApplyIdentityDisplay();
            ClearDetails();

            if (_context.LearningCandidates == null)
            {
                _status.Text = "Learning candidate storage is not configured.";
                return;
            }

            AiLearningCandidateStatus? status = null;
            if (_statusFilter.SelectedIndex > 0)
                status = (AiLearningCandidateStatus)Enum.Parse(typeof(AiLearningCandidateStatus), _statusFilter.SelectedItem.ToString());

            AiLearningCandidateType? type = null;
            if (_typeFilter.SelectedIndex > 0)
                type = (AiLearningCandidateType)Enum.Parse(typeof(AiLearningCandidateType), _typeFilter.SelectedItem.ToString());

            var candidates = await _context.LearningCandidates.QueryAsync(new AiLearningCandidateQuery
            {
                Status = status,
                CandidateType = type,
                IncludeExpired = false,
                MaxResults = 500
            });

            foreach (var candidate in candidates)
            {
                var item = new ListViewItem(new[]
                {
                    candidate.CandidateType.ToString(),
                    candidate.CandidateId,
                    candidate.Status.ToString(),
                    candidate.ProposedScope,
                    candidate.SourceAgentProfileId,
                    candidate.Confidence.HasValue ? candidate.Confidence.Value.ToString("0.00") : "",
                    candidate.Revision.ToString(),
                    candidate.UpdatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                });
                item.Tag = candidate;
                _list.Items.Add(item);
            }

            _status.Text = candidates.Count == 0
                ? "No candidates match the current filters."
                : candidates.Count + " candidate(s) match the current filters.";

            if (!string.IsNullOrWhiteSpace(selectedId))
                SelectCandidate(selectedId);

            if (_list.SelectedItems.Count == 0 && _list.Items.Count > 0)
                _list.Items[0].Selected = true;
        }

        private void Build()
        {
            var root = CreateListPageRoot();
            root.Controls.Add(CreateHeader("Learning Review", "Inspect governed learning candidates, filter durable review state, and review the selected candidate through the existing policy boundary."));
            root.Controls.Add(CreateActionBar());
            root.Controls.Add(CreateWorkspace());
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
            _approveButton.Click += async delegate { await ReviewAsync(AiLearningCandidateReviewAction.Approve); };
            _rejectButton.Click += async delegate { await ReviewAsync(AiLearningCandidateReviewAction.Reject); };
            var refresh = CreateActionButton("Refresh", 100);
            refresh.Click += async delegate { await RefreshDataAsync(); };

            actions.Controls.Add(new Label { Text = "Reviewer user:", AutoSize = true, ForeColor = Muted, Margin = new Padding(0, 10, 4, 0) });
            actions.Controls.Add(_userId);
            actions.Controls.Add(new Label { Text = "Tenant:", AutoSize = true, ForeColor = Muted, Margin = new Padding(10, 10, 4, 0) });
            actions.Controls.Add(_tenantId);
            actions.Controls.Add(new Label { Text = "Workspace:", AutoSize = true, ForeColor = Muted, Margin = new Padding(10, 10, 4, 0) });
            actions.Controls.Add(_workspaceId);
            actions.Controls.Add(_approveButton);
            actions.Controls.Add(_rejectButton);
            actions.Controls.Add(refresh);
            actions.Controls.Add(_status);
            outer.Controls.Add(actions);
            return outer;
        }

        private Control CreateWorkspace()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Surface,
                Margin = new Padding(0)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Surface,
                Padding = new Padding(0, 3, 0, 3),
                Margin = new Padding(0)
            };

            filters.Controls.Add(new Label { Text = "Status:", AutoSize = true, ForeColor = Muted, Margin = new Padding(0, 9, 5, 0) });
            ConfigureFilter(_statusFilter);
            _statusFilter.Items.Add("All");
            foreach (var value in Enum.GetValues(typeof(AiLearningCandidateStatus)))
                _statusFilter.Items.Add(value.ToString());
            _statusFilter.SelectedIndex = 0;
            _statusFilter.SelectedIndexChanged += async delegate { await RefreshDataAsync(); };
            filters.Controls.Add(_statusFilter);

            filters.Controls.Add(new Label { Text = "Type:", AutoSize = true, ForeColor = Muted, Margin = new Padding(12, 9, 5, 0) });
            ConfigureFilter(_typeFilter);
            _typeFilter.Items.Add("All");
            foreach (var value in Enum.GetValues(typeof(AiLearningCandidateType)))
                _typeFilter.Items.Add(value.ToString());
            _typeFilter.SelectedIndex = 0;
            _typeFilter.SelectedIndexChanged += async delegate { await RefreshDataAsync(); };
            filters.Controls.Add(_typeFilter);

            root.Controls.Add(filters, 0, 0);
            root.Controls.Add(CreateSplitWorkspace(), 0, 1);
            return root;
        }

        private Control CreateSplitWorkspace()
        {
            ConfigureList(_list);
            _list.Columns.Add("Type", 75);
            _list.Columns.Add("Candidate", 210);
            _list.Columns.Add("Status", 105);
            _list.Columns.Add("Scope", 90);
            _list.Columns.Add("Source Agent", 145);
            _list.Columns.Add("Confidence", 80);
            _list.Columns.Add("Revision", 65);
            _list.Columns.Add("Updated", 145);
            _list.SelectedIndexChanged += delegate { DisplaySelectedCandidate(); };
            _list.DoubleClick += delegate { DisplaySelectedCandidate(); };

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = Surface,
                Margin = new Padding(0)
            };
            split.Panel1.Padding = new Padding(0, 0, 6, 0);
            split.Panel2.Padding = new Padding(6, 0, 0, 0);
            split.SizeChanged += delegate { ApplySplitterLayout(split); };
            split.Panel1.Controls.Add(_list);
            split.Panel2.Controls.Add(CreateDetailsPanel());
            ApplySplitterLayout(split);
            return split;
        }

        private static void ApplySplitterLayout(SplitContainer split)
        {
            if (split == null || split.Width <= 1) return;

            const int panel1Minimum = 300;
            const int panel2Minimum = 360;
            var minimumTotal = panel1Minimum + panel2Minimum;

            if (split.Width < minimumTotal)
            {
                split.Panel1MinSize = 0;
                split.Panel2MinSize = 0;
                split.SplitterDistance = Math.Max(0, Math.Min(split.Width - split.SplitterWidth, split.Width / 2));
                return;
            }

            split.Panel1MinSize = panel1Minimum;
            split.Panel2MinSize = panel2Minimum;
            var desired = 720;
            var maximum = split.Width - panel2Minimum;
            split.SplitterDistance = Math.Max(panel1Minimum, Math.Min(desired, maximum));
        }

        private Control CreateDetailsPanel()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(10),
                Margin = new Padding(0)
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var title = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0) };
            title.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            title.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            title.Controls.Add(_detailCandidateId, 0, 0);
            title.Controls.Add(_detailStatus, 0, 1);
            panel.Controls.Add(title, 0, 0);

            var details = new TabControl { Dock = DockStyle.Fill, Margin = new Padding(0) };
            details.TabPages.Add(CreateOverviewTab());
            details.TabPages.Add(CreatePayloadTab());
            details.TabPages.Add(CreateReviewEvidenceTab());
            panel.Controls.Add(details, 0, 1);
            return panel;
        }

        private TabPage CreateOverviewTab()
        {
            var page = new TabPage("Overview") { BackColor = Color.White, Padding = new Padding(6) };
            var table = CreateDetailTable();
            AddDetailRow(table, 0, "Type", _detailType);
            AddDetailRow(table, 1, "Proposed scope", _detailScope);
            AddDetailRow(table, 2, "Source agent", _detailSourceAgent);
            AddDetailRow(table, 3, "Revision", _detailRevision);
            AddDetailRow(table, 4, "Confidence", _detailConfidence);
            AddDetailRow(table, 5, "Evidence state", _detailEvidenceState);
            AddDetailRow(table, 6, "Provenance state", _detailProvenanceState);
            AddDetailRow(table, 7, "Contradiction state", _detailContradictionState);
            AddDetailRow(table, 8, "Retention", _detailRetention);
            AddDetailRow(table, 9, "Evaluation", _detailEvaluation);
            AddDetailRow(table, 10, "Created", _detailCreated);
            AddDetailRow(table, 11, "Updated", _detailUpdated);
            AddDetailRow(table, 12, "Expires", _detailExpiry);
            AddDetailRow(table, 13, "Policy", _detailPolicy);
            AddDetailRow(table, 14, "Promotion authorization", _detailAuthorization);
            AddDetailRow(table, 15, "Last review", _detailLastReview);
            page.Controls.Add(table);
            return page;
        }

        private TabPage CreatePayloadTab()
        {
            var page = new TabPage("Candidate Content") { BackColor = Color.White, Padding = new Padding(6) };
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(CreateSectionLabel("Proposed resource payload"), 0, 0);
            root.Controls.Add(_detailPayload, 0, 1);
            root.Controls.Add(CreateSectionLabel("Provenance"), 0, 2);
            root.Controls.Add(_detailProvenance, 0, 3);
            root.Controls.Add(CreateSectionLabel("Evidence"), 0, 4);
            root.Controls.Add(_detailEvidence, 0, 5);
            page.Controls.Add(root);
            return page;
        }

        private TabPage CreateReviewEvidenceTab()
        {
            var page = new TabPage("Review Evidence") { BackColor = Color.White, Padding = new Padding(6) };
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            root.Controls.Add(CreateSectionLabel("Source execution / runtime"), 0, 0);
            root.Controls.Add(_detailSource, 0, 1);
            root.Controls.Add(CreateSectionLabel("Last review / policy evidence"), 0, 2);
            root.Controls.Add(_detailReviewEvidence, 0, 3);
            page.Controls.Add(root);
            return page;
        }

        private static TableLayoutPanel CreateDetailTable()
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoScroll = true,
                ColumnCount = 2,
                Padding = new Padding(4),
                BackColor = Color.White,
                AutoSize = true
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return table;
        }

        private static void AddDetailRow(TableLayoutPanel table, int row, string caption, Control value)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            table.Controls.Add(new Label { Text = caption, AutoSize = true, ForeColor = Muted, Margin = new Padding(0, 6, 8, 0) }, 0, row);
            value.Margin = new Padding(0, 6, 0, 0);
            table.Controls.Add(value, 1, row);
        }

        private static Label CreateSectionLabel(string text)
        {
            return new Label { Text = text, AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Heading, Margin = new Padding(0, 3, 0, 0) };
        }

        private static RichTextBox CreateReadOnlyTextBox()
        {
            return new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 8.7f),
                DetectUrls = false,
                Margin = new Padding(0)
            };
        }

        private static void ConfigureFilter(ComboBox filter)
        {
            filter.DropDownStyle = ComboBoxStyle.DropDownList;
            filter.Width = 150;
            filter.Height = 30;
            filter.Margin = new Padding(0);
        }

        private void DisplaySelectedCandidate()
        {
            var record = GetSelectedCandidate();
            if (record == null)
            {
                ClearDetails();
                return;
            }

            _detailCandidateId.Text = record.CandidateId;
            _detailStatus.Text = "Status: " + record.Status;
            _detailType.Text = record.CandidateType.ToString();
            _detailScope.Text = record.ProposedScope;
            _detailSourceAgent.Text = record.SourceAgentProfileId;
            _detailRevision.Text = record.Revision.ToString();
            _detailConfidence.Text = record.Confidence.HasValue ? record.Confidence.Value.ToString("0.000") : "Not supplied";
            _detailEvidenceState.Text = record.EvidenceState;
            _detailProvenanceState.Text = record.ProvenanceState;
            _detailContradictionState.Text = record.ContradictionState;
            _detailRetention.Text = record.RetentionClass;
            _detailEvaluation.Text = record.EvaluationState;
            _detailCreated.Text = record.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz");
            _detailUpdated.Text = record.UpdatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz");
            _detailExpiry.Text = record.ExpiresAt.HasValue ? record.ExpiresAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz") : "No expiry";
            _detailPolicy.Text = BuildPolicySummary(record);
            _detailAuthorization.Text = BuildAuthorizationSummary(record);
            _detailLastReview.Text = BuildReviewSummary(record);
            _detailPayload.Text = FormatJson(record.PayloadJson);
            _detailProvenance.Text = string.IsNullOrWhiteSpace(record.Provenance) ? "Not supplied" : record.Provenance;
            _detailEvidence.Text = string.IsNullOrWhiteSpace(record.Evidence) ? "Not supplied" : record.Evidence;
            _detailSource.Text = "Execution ID: " + Safe(record.SourceExecutionId) + Environment.NewLine +
                                  "Runtime instance ID: " + Safe(record.SourceRuntimeInstanceId) + Environment.NewLine +
                                  "Agent profile ID: " + Safe(record.SourceAgentProfileId);
            _detailReviewEvidence.Text = BuildReviewEvidence(record);
            _approveButton.Enabled = record.Status == AiLearningCandidateStatus.PendingReview;
            _rejectButton.Enabled = record.Status == AiLearningCandidateStatus.PendingReview;
        }

        private void ClearDetails()
        {
            _detailCandidateId.Text = "No candidate selected";
            _detailStatus.Text = string.Empty;
            _detailType.Text = "";
            _detailScope.Text = "";
            _detailSourceAgent.Text = "";
            _detailRevision.Text = "";
            _detailConfidence.Text = "";
            _detailEvidenceState.Text = "";
            _detailProvenanceState.Text = "";
            _detailContradictionState.Text = "";
            _detailRetention.Text = "";
            _detailEvaluation.Text = "";
            _detailCreated.Text = "";
            _detailUpdated.Text = "";
            _detailExpiry.Text = "";
            _detailPolicy.Text = "";
            _detailAuthorization.Text = "";
            _detailLastReview.Text = "";
            _detailPayload.Clear();
            _detailProvenance.Clear();
            _detailEvidence.Clear();
            _detailSource.Clear();
            _detailReviewEvidence.Clear();
            _approveButton.Enabled = false;
            _rejectButton.Enabled = false;
        }

        private string GetSelectedCandidateId()
        {
            var selected = GetSelectedCandidate();
            return selected == null ? null : selected.CandidateId;
        }

        private AiLearningCandidateRecord GetSelectedCandidate()
        {
            if (_list.SelectedItems.Count == 0) return null;
            return _list.SelectedItems[0].Tag as AiLearningCandidateRecord;
        }

        private void SelectCandidate(string candidateId)
        {
            foreach (ListViewItem item in _list.Items)
            {
                var record = item.Tag as AiLearningCandidateRecord;
                if (record == null || !string.Equals(record.CandidateId, candidateId, StringComparison.OrdinalIgnoreCase)) continue;
                item.Selected = true;
                item.Focused = true;
                item.EnsureVisible();
                return;
            }
        }

        private static string BuildPolicySummary(AiLearningCandidateRecord record)
        {
            return "Policy ID: " + Safe(record.PolicyId) + Environment.NewLine +
                   "Policy version: " + record.PolicyVersion + Environment.NewLine +
                   "Policy rule: " + Safe(record.PolicyRuleId);
        }

        private static string BuildAuthorizationSummary(AiLearningCandidateRecord record)
        {
            return Safe(record.AuthorizationOutcome) +
                   (string.IsNullOrWhiteSpace(record.AuthorizationReason) ? "" : " — " + record.AuthorizationReason);
        }

        private static string BuildReviewSummary(AiLearningCandidateRecord record)
        {
            if (!record.LastReviewedAt.HasValue) return "Not reviewed";
            return Safe(record.LastReviewAction) + " by recorded reviewer at " + record.LastReviewedAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz");
        }

        private static string BuildReviewEvidence(AiLearningCandidateRecord record)
        {
            if (!record.LastReviewedAt.HasValue && string.IsNullOrWhiteSpace(record.LastReviewAction))
                return "No review evidence recorded.";

            return "Action: " + Safe(record.LastReviewAction) + Environment.NewLine +
                   "Reviewer identity: " + Safe(record.LastReviewerIdentityJson) + Environment.NewLine +
                   "Review policy version: " + Safe(record.LastReviewPolicyVersion) + Environment.NewLine +
                   "Review rule: " + Safe(record.LastReviewRuleId) + Environment.NewLine +
                   "Review outcome: " + Safe(record.LastReviewOutcome) + Environment.NewLine +
                   "Review reason: " + Safe(record.LastReviewReason) + Environment.NewLine +
                   "Reviewed at: " + (record.LastReviewedAt.HasValue ? record.LastReviewedAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz") : "Not supplied");
        }

        private static string FormatJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "Not supplied";
            try
            {
                using (var document = JsonDocument.Parse(json))
                    return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return json;
            }
        }

        private static string Safe(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Not supplied" : value;
        }

        private async Task ReviewAsync(AiLearningCandidateReviewAction action)
        {
            var record = GetSelectedCandidate();
            if (record == null)
            {
                HMessage.ShowInformation(FindForm(), "Select a candidate first.", "Learning Review");
                return;
            }
            if (record.Status != AiLearningCandidateStatus.PendingReview)
            {
                HMessage.ShowInformation(FindForm(), "Only PendingReview candidates can be reviewed.", "Learning Review");
                return;
            }

            if (_context.LearningCandidates == null) return;

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
