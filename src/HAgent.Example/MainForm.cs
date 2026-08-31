using HAgent.Models;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace HAgent.Example
{
    internal sealed partial class MainForm : HAgentForm
    {
        private static readonly Color Surface = Color.FromArgb(248, 248, 252);
        private static readonly Color Heading = Color.FromArgb(31, 24, 69);
        private static readonly Color Text = Color.FromArgb(68, 62, 88);
        private static readonly Color Muted = Color.FromArgb(100, 92, 120);
        private static readonly Color Accent = Color.FromArgb(116, 76, 210);
        private static readonly Color Error = Color.FromArgb(185, 28, 28);

        private readonly string _basePath = new HAgentStorageOptions
        {
            ApplicationName = string.IsNullOrWhiteSpace(Process.GetCurrentProcess().ProcessName) ? "HAgent" : Process.GetCurrentProcess().ProcessName,
            RootPath = AppContext.BaseDirectory
        }.GetEffectiveRootPath();

        private readonly HButton _configurationButton;
        private readonly HButton _clearOutputButton;
        private readonly ComboBox _agentSelector = new ComboBox();
        private readonly Label _globalStatus = new Label();
        private readonly TextBox _providerPrompt = new TextBox();
        private readonly TextBox _agentPrompt = new TextBox();
        private readonly Label _promptResolution = new Label();
        private readonly TextBox _output = new TextBox();
        private readonly TabControl _tabs = new TabControl();
        private readonly List<HButton> _testButtons = new List<HButton>();
        private readonly List<AiAgent> _agents = new List<AiAgent>();
        private CancellationTokenSource _streamingCts;

        public MainForm()
            : base(
                "HAgent Example",
                "Manual integration and feature-verification host",
                new Size(1280, 820),
                new Size(1000, 680))
        {
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BuildShell();

            _configurationButton = CreateButton("Configuration", 150);
            _configurationButton.Click += delegate { OpenConfiguration(); };
            GetActionsPanel().Controls.Add(_configurationButton);

            GetActionsPanel().Controls.Add(new Label
            {
                Text = "Agent:",
                AutoSize = true,
                ForeColor = Text,
                Font = new Font("Segoe UI", 9.1f, FontStyle.Bold),
                Margin = new Padding(8, 11, 5, 0)
            });
            ConfigureAgentSelector();

            _clearOutputButton = CreateButton("Clear output", 120);
            _clearOutputButton.Click += delegate { _output.Clear(); SetReady(); };
            GetActionsPanel().Controls.Add(_clearOutputButton);

            AddFeatureTabs();
            Shown += async delegate
            {
                try
                {
                    await RefreshExampleAgentsAsync();
                }
                catch (Exception ex)
                {
                    _globalStatus.Text = "Storage unavailable";
                    _globalStatus.ForeColor = Error;
                    Write("STORAGE", "HAgent could not open the configured internal storage backend." + Environment.NewLine +
                                      "Open Configuration → Storage to review the settings, then restart if the backend is changed." + Environment.NewLine +
                                      "Detail: " + ex.Message);
                    HMessage.ShowException(
                        this,
                        "HAgent could not open the configured internal storage backend. Review Configuration → Storage and restart the application after changing storage settings.",
                        "HAgent Storage",
                        ex);
                    SetButtonsEnabled(true);
                }
            };
            FormClosed += delegate
            {
                if (_streamingCts != null)
                {
                    _streamingCts.Cancel();
                    _streamingCts.Dispose();
                    _streamingCts = null;
                }
            };
        }

        private void BuildShell()
        {
            BodyPanel.Padding = new Padding(22);
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Surface,
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 12));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 8));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            var promptPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                BackColor = Surface,
                Padding = new Padding(0, 4, 0, 4)
            };
            promptPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            promptPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            promptPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            promptPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            promptPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            promptPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

            promptPanel.Controls.Add(CreatePromptFieldLabel("Provider system prompt"), 0, 0);
            promptPanel.Controls.Add(CreateReadOnlyPromptBox(_providerPrompt), 1, 0);
            promptPanel.Controls.Add(CreatePromptFieldLabel("Agent system prompt"), 2, 0);
            promptPanel.Controls.Add(CreateReadOnlyPromptBox(_agentPrompt), 3, 0);

            promptPanel.Controls.Add(new Label
            {
                Text = "System prompt used:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding = new Padding(0, 0, 4, 0)
            }, 0, 1);
            _promptResolution.Dock = DockStyle.Fill;
            _promptResolution.TextAlign = ContentAlignment.MiddleLeft;
            _promptResolution.ForeColor = Accent;
            _promptResolution.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _promptResolution.AutoEllipsis = true;
            promptPanel.Controls.Add(_promptResolution, 1, 1);
            promptPanel.SetColumnSpan(_promptResolution, 3);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 3, 0, 0),
                BackColor = Surface
            };

            _globalStatus.Text = "Ready";
            _globalStatus.AutoSize = true;
            _globalStatus.ForeColor = Muted;
            _globalStatus.Margin = new Padding(12, 11, 0, 0);
            actions.Controls.Add(_globalStatus);

            _tabs.Dock = DockStyle.Fill;
            _tabs.Font = new Font("Segoe UI", 9f);
            _tabs.Padding = new Point(12, 5);

            var outputPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.FromArgb(236, 234, 245),
                Padding = new Padding(10)
            };
            outputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            outputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            outputPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            outputPanel.Controls.Add(new Label
            {
                Text = "Global output",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Heading,
                Margin = new Padding(0)
            }, 0, 0);

            _output.Dock = DockStyle.Fill;
            _output.Multiline = true;
            _output.ReadOnly = true;
            _output.ScrollBars = ScrollBars.Both;
            _output.Font = new Font("Consolas", 9f);
            _output.BackColor = Color.White;
            _output.BorderStyle = BorderStyle.FixedSingle;
            _output.WordWrap = false;
            outputPanel.Controls.Add(_output, 0, 1);

            root.Controls.Add(promptPanel, 0, 0);
            root.Controls.Add(actions, 0, 1);
            root.Controls.Add(_tabs, 0, 2);
            root.Controls.Add(outputPanel, 0, 3);
            BodyPanel.Controls.Add(root);
        }

        private static Label CreatePromptFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.4f, FontStyle.Bold),
                Padding = new Padding(0, 0, 4, 0)
            };
        }

        private static TextBox CreateReadOnlyPromptBox(TextBox box)
        {
            box.ReadOnly = true;
            box.Multiline = true;
            box.ScrollBars = ScrollBars.Vertical;
            box.Dock = DockStyle.Fill;
            box.Font = new Font("Segoe UI", 8.3f);
            box.BackColor = Color.White;
            box.BorderStyle = BorderStyle.FixedSingle;
            box.Margin = new Padding(0, 1, 4, 2);
            return box;
        }

        private FlowLayoutPanel GetActionsPanel()
        {
            var root = BodyPanel.Controls.OfType<TableLayoutPanel>().First();
            return (FlowLayoutPanel)root.GetControlFromPosition(0, 1);
        }

        private void ConfigureAgentSelector()
        {
            _agentSelector.Width = 240;
            _agentSelector.Height = 30;
            _agentSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            _agentSelector.Font = new Font("Segoe UI", 9.1f);
            _agentSelector.Margin = new Padding(0, 5, 8, 0);
            _agentSelector.SelectedIndexChanged += delegate { _ = UpdateSelectedAgentAsync(); };
            GetActionsPanel().Controls.Add(_agentSelector);
        }
    }
}
