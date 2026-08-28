using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.Providers.OpenAICompatible;
using HAgent.Storage.File;
using HAgent.WinForms;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;
using HAgent.WinForms.Controls;

namespace HAgent.Example
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    internal sealed class MainForm : HAgentForm
    {
        private readonly string _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HAgent");

        private readonly HButton _configurationButton;
        private readonly HButton _clearOutputButton;
        private readonly ComboBox _agentSelector = new ComboBox();
        private readonly Label _agentLabel = new Label();
        private readonly Label _globalStatus = new Label();
        private readonly TextBox _output = new TextBox();
        private readonly TabControl _tabs = new TabControl();
        private readonly List<HButton> _testButtons = new List<HButton>();
        private readonly List<AiAgent> _agents = new List<AiAgent>();

        public MainForm()
            : base(
                "HAgent Example",
                "Manual integration and feature-verification host",
                new Size(1080, 760),
                new Size(900, 620))
        {
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            BuildShell();

            _configurationButton = CreateButton("Configuration", 150);
            _configurationButton.Click += delegate { OpenConfiguration(); };
            AddHeaderAction(_configurationButton);

            _agentLabel.Text = "Agent:";
            _agentLabel.AutoSize = true;
            _agentLabel.ForeColor = Color.FromArgb(68, 62, 88);
            _agentLabel.Font = new Font("Segoe UI", 9.1f, FontStyle.Bold);
            _agentLabel.Margin = new Padding(8, 11, 5, 0);
            ((FlowLayoutPanel)BodyPanel.Controls.OfType<TableLayoutPanel>().First().GetControlFromPosition(0, 1)).Controls.Add(_agentLabel);

            ConfigureAgentSelector();

            _clearOutputButton = CreateButton("Clear output", 120);
            _clearOutputButton.Click += delegate { _output.Clear(); SetReady(); };
            AddHeaderAction(_clearOutputButton);

            _testButtons.Add(AddExampleAction(
                _tabs.TabPages[0],
                "Send message",
                "Calls HAgentClient.SendAsync using the globally selected agent.",
                "A normal conversational model should return exactly MESSAGE-OK.",
                SendMessageAsync));

            _testButtons.Add(AddExampleAction(
                _tabs.TabPages[1],
                "Run session test",
                "Creates one AgentSession for the selected agent and sends two messages. The second request receives the complete session history.",
                "The second response should identify HAgent-session-42 and the global output should contain the retained transcript.",
                TestSessionAsync));

            _testButtons.Add(AddExampleAction(
                _tabs.TabPages[2],
                "Run runtime test",
                "Uses the 0.2 execution pipeline with timeout, provider-attempt, and retry settings for the selected agent.",
                "Execution should reach Succeeded and show an execution ID, provider, model, state, and response.",
                TestRuntimeAsync));

            _testButtons.Add(AddExampleAction(
                _tabs.TabPages[3],
                "Read configuration",
                "Reads providers and agents directly from the local file store.",
                "The output should show the settings path and the current provider/agent counts and relationships.",
                ReadConfigurationAsync));

            AddNote(_tabs.TabPages[0], "Model warning", "A model catalog can contain classification, guard, embedding, or other non-chat models. HAgent does not yet infer model capabilities, so choose a conversational model for this test.");
            AddNote(_tabs.TabPages[1], "Memory boundary", "This validates in-session history forwarding only. Persistent long-term memory is part of milestone 0.3.");
            AddNote(_tabs.TabPages[2], "Runtime boundary", "This validates orchestration behavior. It does not prove that the chosen model is suitable for the requested task.");
            AddNote(_tabs.TabPages[3], "Storage boundary", "This verifies host-side configuration reading, not database persistence.");

            Shown += async delegate { await RefreshAgentsAsync(); };
        }

        private void BuildShell()
        {
            BodyPanel.Padding = new Padding(22);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.FromArgb(248, 248, 252),
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));

            var heading = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 248, 252)
            };
            heading.Controls.Add(new Label
            {
                Text = "Manual feature test bench",
                AutoSize = true,
                Left = 0,
                Top = 0,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 24, 69)
            });
            heading.Controls.Add(new Label
            {
                Text = "Run the real HAgent APIs and compare the output with each feature's expected behavior.",
                AutoSize = true,
                Left = 1,
                Top = 35,
                ForeColor = Color.FromArgb(100, 92, 120),
                Font = new Font("Segoe UI", 9f)
            });

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 3, 0, 0),
                BackColor = Color.FromArgb(248, 248, 252)
            };

            _tabs.Dock = DockStyle.Fill;
            _tabs.Font = new Font("Segoe UI", 9f);
            _tabs.Padding = new Point(12, 5);
            _tabs.TabPages.Add(CreateExampleTab("Messaging"));
            _tabs.TabPages.Add(CreateExampleTab("Session"));
            _tabs.TabPages.Add(CreateExampleTab("Runtime 0.2"));
            _tabs.TabPages.Add(CreateExampleTab("Configuration"));

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
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 24, 69),
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

            _globalStatus.Text = "Ready";
            _globalStatus.AutoSize = true;
            _globalStatus.ForeColor = Color.FromArgb(100, 92, 120);
            _globalStatus.Margin = new Padding(12, 11, 0, 0);
            actions.Controls.Add(_globalStatus);

            root.Controls.Add(heading, 0, 0);
            root.Controls.Add(actions, 0, 1);
            root.Controls.Add(_tabs, 0, 2);
            root.Controls.Add(outputPanel, 0, 3);
            BodyPanel.Controls.Add(root);
        }

        private void ConfigureAgentSelector()
        {
            _agentSelector.Width = 220;
            _agentSelector.Height = 30;
            _agentSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            _agentSelector.Font = new Font("Segoe UI", 9.1f);
            _agentSelector.Margin = new Padding(0, 5, 8, 0);
            _agentSelector.SelectedIndexChanged += delegate { UpdateSelectedAgentStatus(); };

            var actions = (FlowLayoutPanel)BodyPanel.Controls.OfType<TableLayoutPanel>().First().GetControlFromPosition(0, 1);
            actions.Controls.Add(_agentSelector);
        }

        private async Task RefreshAgentsAsync()
        {
            try
            {
                var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
                var agents = await store.GetAgentsAsync();
                var previousId = GetSelectedAgentId();

                _agents.Clear();
                _agents.AddRange(agents);

                _agentSelector.BeginUpdate();
                try
                {
                    _agentSelector.Items.Clear();
                    foreach (var agent in _agents)
                        _agentSelector.Items.Add(new AgentItem(agent));
                }
                finally
                {
                    _agentSelector.EndUpdate();
                }

                if (!string.IsNullOrWhiteSpace(previousId))
                    SelectAgent(previousId);
                if (_agentSelector.SelectedIndex < 0 && _agentSelector.Items.Count > 0)
                    _agentSelector.SelectedIndex = 0;

                if (_agentSelector.Items.Count == 0)
                    _globalStatus.Text = "No agents configured";
                else
                    UpdateSelectedAgentStatus();
            }
            catch (Exception ex)
            {
                _globalStatus.Text = "Agent list could not be loaded";
                HMessage.ShowException(this, "The agent list could not be loaded.", "HAgent Example", ex);
            }
        }

        private string GetSelectedAgentId()
        {
            var item = _agentSelector.SelectedItem as AgentItem;
            return item == null ? string.Empty : item.Agent.Id;
        }

        private AiAgent GetSelectedAgent()
        {
            var item = _agentSelector.SelectedItem as AgentItem;
            return item == null ? null : item.Agent;
        }

        private void SelectAgent(string agentId)
        {
            for (var i = 0; i < _agentSelector.Items.Count; i++)
            {
                var item = _agentSelector.Items[i] as AgentItem;
                if (item != null && string.Equals(item.Agent.Id, agentId, StringComparison.OrdinalIgnoreCase))
                {
                    _agentSelector.SelectedIndex = i;
                    return;
                }
            }
        }

        private void UpdateSelectedAgentStatus()
        {
            var agent = GetSelectedAgent();
            if (agent == null)
            {
                _globalStatus.Text = "No agent selected";
                return;
            }

            _globalStatus.Text = agent.Enabled
                ? "Selected: " + agent.Name
                : "Selected: " + agent.Name + " (disabled)";
            _globalStatus.ForeColor = agent.Enabled
                ? Color.FromArgb(100, 92, 120)
                : Color.FromArgb(185, 28, 28);
        }

        private void AddHeaderAction(HButton button)
        {
            var root = BodyPanel.Controls.OfType<TableLayoutPanel>().First();
            var actions = root.GetControlFromPosition(0, 1) as FlowLayoutPanel;
            actions.Controls.Add(button);
        }

        private static TabPage CreateExampleTab(string title)
        {
            return new TabPage(title)
            {
                BackColor = Color.FromArgb(248, 248, 252),
                Padding = new Padding(22)
            };
        }

        private HButton AddExampleAction(TabPage page, string buttonText, string description, string expected, Func<Task> test)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.FromArgb(248, 248, 252)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var button = CreateButton(buttonText, 180);
            button.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            button.Click += async delegate { await RunExampleAsync(test); };
            layout.Controls.Add(button, 0, 0);
            layout.Controls.Add(new Label
            {
                Text = description,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(68, 62, 88),
                Font = new Font("Segoe UI", 9.2f),
                Padding = new Padding(1, 8, 20, 0)
            }, 0, 1);
            layout.Controls.Add(new Label
            {
                Text = "Expected result\r\n" + expected,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(81, 75, 104),
                Font = new Font("Segoe UI", 8.9f),
                Padding = new Padding(1, 4, 20, 0)
            }, 0, 2);
            page.Controls.Add(layout);
            return button;
        }

        private static void AddNote(TabPage page, string title, string text)
        {
            var table = page.Controls.OfType<TableLayoutPanel>().First();
            table.Controls.Add(new Label
            {
                Text = title + ": " + text,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(100, 92, 120),
                Font = new Font("Segoe UI", 8.6f),
                Padding = new Padding(1, 8, 20, 0)
            }, 0, 3);
        }

        private async Task RunExampleAsync(Func<Task> action)
        {
            SetBusy("Running example...");
            try
            {
                await action();
                SetReady();
            }
            catch (Exception ex)
            {
                Write("EXCEPTION", ex.ToString());
                SetReady();
                HMessage.ShowException(this, "The example failed.", "HAgent Example", ex);
            }
        }

        private void OpenConfiguration()
        {
            var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            AISettings.ShowMainAISettingsForm(store, secrets, this, new[] { new OpenAICompatibleProviderAdapter() });
            _ = RefreshAgentsAsync();
        }

        private async Task SendMessageAsync()
        {
            var selection = await CreateClientAndAgentAsync();
            var response = await selection.Client.SendAsync(selection.Agent.Id, "Reply with exactly MESSAGE-OK and nothing else.");
            Write("SEND MESSAGE", "Agent: " + selection.Agent.Name + Environment.NewLine + "Provider: " + selection.Provider.Name + Environment.NewLine + "Model: " + selection.Model + Environment.NewLine + "Response: " + response.Text);
        }

        private async Task TestSessionAsync()
        {
            var selection = await CreateClientAndAgentAsync();
            var session = selection.Client.CreateSession(selection.Agent.Id);
            await session.SendAsync("Store this temporary test value in our conversation: HAgent-session-42.");
            var response = await session.SendAsync("What temporary test value did I just give you? Reply with only the value.");
            var read = await session.ReadAsync();
            Write("SESSION", "Agent: " + selection.Agent.Name + Environment.NewLine + "Provider: " + selection.Provider.Name + Environment.NewLine + "Model: " + selection.Model + Environment.NewLine + "Second response: " + response.Text + Environment.NewLine + "Messages retained: " + read.Messages.Count + Environment.NewLine + "Transcript:" + Environment.NewLine + string.Join(Environment.NewLine, read.Messages.Select(x => "  " + x.Role + ": " + x.Content)));
        }

        private async Task TestRuntimeAsync()
        {
            var selection = await CreateClientAndAgentAsync();
            var execution = await selection.Client.ExecuteAsync(
                selection.Agent.Id,
                "Reply with the word RUNTIME-OK and nothing else.",
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(30),
                    MaxProviderAttempts = 2,
                    MaxRetriesPerProvider = 1
                },
                CancellationToken.None);

            Write("RUNTIME", "Execution: " + execution.Id + Environment.NewLine + "State: " + execution.State + Environment.NewLine + "Failure: " + execution.FailureKind + Environment.NewLine + "Provider: " + selection.Provider.Name + " (" + execution.Response.ProviderId + ")" + Environment.NewLine + "Model: " + selection.Model + Environment.NewLine + "Response: " + execution.Response.Text);
        }

        private async Task ReadConfigurationAsync()
        {
            var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
            var providers = await store.GetProvidersAsync();
            var agents = await store.GetAgentsAsync();
            Write("CONFIGURATION", "Settings: " + Path.Combine(_basePath, "settings.json") + Environment.NewLine + "Providers: " + providers.Count + Environment.NewLine + string.Join(Environment.NewLine, providers.Select(p => "  - " + p.Name + " [" + p.Kind + "] model=" + p.DefaultModel)) + Environment.NewLine + "Agents: " + agents.Count + Environment.NewLine + string.Join(Environment.NewLine, agents.Select(a => "  - " + a.Name + " -> " + a.ProviderId)));
        }

        private async Task<ClientSelection> CreateClientAndAgentAsync()
        {
            var agent = GetSelectedAgent();
            if (agent == null)
                throw new InvalidOperationException("Select an agent first.");
            if (!agent.Enabled)
                throw new InvalidOperationException("The selected agent is disabled. Enable it in Configuration first.");

            var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            var providers = await store.GetProvidersAsync();

            var providerIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(agent.ProviderId)) providerIds.Add(agent.ProviderId);
            if (agent.ProviderIds != null) providerIds.AddRange(agent.ProviderIds.Where(x => !string.IsNullOrWhiteSpace(x)));

            var provider = providerIds
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(id => providers.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault(p => p != null && p.Enabled);

            if (provider == null)
                throw new InvalidOperationException("The selected agent has no enabled provider. Agent='" + agent.Name + "'.");

            var model = string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model;
            if (string.IsNullOrWhiteSpace(model))
                throw new InvalidOperationException("No model is configured for agent '" + agent.Name + "' or provider '" + provider.Name + "'.");

            return new ClientSelection(
                new HAgentClient(store, secrets, new[] { new OpenAICompatibleProviderAdapter() }),
                agent,
                provider,
                model);
        }

        private static HButton CreateButton(string text, int width)
        {
            return new HButton
            {
                Text = text,
                Width = width,
                Height = 36,
                RoundButton = true,
                Edge = 10,
                Font = new Font("Segoe UI", 9.3f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                ButtonLeaveBackGroundColor1 = Color.FromArgb(92, 67, 168),
                ButtonLeaveBackGroundColor2 = Color.FromArgb(57, 40, 108),
                ButtonLeaveForeColor = Color.White,
                ButtonLeaveBorderColor = Color.FromArgb(116, 76, 210),
                ButtonEnterBackGroundColor1 = Color.FromArgb(126, 94, 214),
                ButtonEnterBackGroundColor2 = Color.FromArgb(79, 54, 145),
                ButtonEnterForeColor = Color.White,
                ButtonEnterBorderColor = Color.FromArgb(146, 118, 232),
                ButtonDownBackGroundColor1 = Color.FromArgb(72, 52, 132),
                ButtonDownBackGroundColor2 = Color.FromArgb(45, 31, 88),
                ButtonDownForeColor = Color.White,
                ButtonDownBorderColor = Color.FromArgb(104, 79, 176),
                Margin = new Padding(0, 0, 10, 0)
            };
        }

        private void SetBusy(string text)
        {
            SetButtonsEnabled(false);
            _globalStatus.Text = text;
            _globalStatus.ForeColor = Color.FromArgb(116, 76, 210);
        }

        private void SetReady()
        {
            SetButtonsEnabled(true);
            UpdateSelectedAgentStatus();
        }

        private void SetButtonsEnabled(bool enabled)
        {
            _configurationButton.Enabled = enabled;
            _clearOutputButton.Enabled = enabled;
            _agentSelector.Enabled = enabled;
            foreach (var button in _testButtons) button.Enabled = enabled;
        }

        private void Write(string title, string text)
        {
            _output.Text = "[" + title + "]" + Environment.NewLine +
                           DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine +
                           text + Environment.NewLine;
        }

        private sealed class AgentItem
        {
            public AgentItem(AiAgent agent) { Agent = agent; }
            public AiAgent Agent { get; private set; }
            public override string ToString()
            {
                return Agent.Enabled ? Agent.Name : Agent.Name + " (Disabled)";
            }
        }

        private sealed class ClientSelection
        {
            public ClientSelection(HAgentClient client, AiAgent agent, AiProvider provider, string model)
            {
                Client = client;
                Agent = agent;
                Provider = provider;
                Model = model;
            }

            public HAgentClient Client { get; private set; }
            public AiAgent Agent { get; private set; }
            public AiProvider Provider { get; private set; }
            public string Model { get; private set; }
        }
    }
}
