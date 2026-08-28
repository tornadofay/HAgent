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
        private static readonly Color Surface = Color.FromArgb(248, 248, 252);
        private static readonly Color Heading = Color.FromArgb(31, 24, 69);
        private static readonly Color Text = Color.FromArgb(68, 62, 88);
        private static readonly Color Muted = Color.FromArgb(100, 92, 120);
        private static readonly Color Accent = Color.FromArgb(116, 76, 210);

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
        private readonly TextBox _providerPrompt = new TextBox();
        private readonly TextBox _agentPrompt = new TextBox();
        private readonly Label _promptResolution = new Label();
        private readonly Dictionary<TabPage, TextBox> _messageInputs = new Dictionary<TabPage, TextBox>();

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
            AddHeaderAction(_configurationButton);

            _agentLabel.Text = "Agent:";
            _agentLabel.AutoSize = true;
            _agentLabel.ForeColor = Text;
            _agentLabel.Font = new Font("Segoe UI", 9.1f, FontStyle.Bold);
            _agentLabel.Margin = new Padding(8, 11, 5, 0);
            GetActionsPanel().Controls.Add(_agentLabel);

            ConfigureAgentSelector();

            _clearOutputButton = CreateButton("Clear output", 120);
            _clearOutputButton.Click += delegate { _output.Clear(); SetReady(); };
            AddHeaderAction(_clearOutputButton);

            AddFeatureTabs();
            Shown += async delegate { await RefreshAgentsAsync(); };
        }

        private void BuildShell()
        {
            BodyPanel.Padding = new Padding(22);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Surface,
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 260));

            var heading = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            heading.Controls.Add(new Label
            {
                Text = "Manual feature test bench",
                AutoSize = true,
                Left = 0,
                Top = 0,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Heading
            });
            heading.Controls.Add(new Label
            {
                Text = "Run the real HAgent APIs and compare the output with the expected behavior shown for each feature.",
                AutoSize = true,
                Left = 1,
                Top = 35,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9f)
            });

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
            promptPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            promptPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            promptPanel.Controls.Add(CreateInfoLabel("Provider system prompt"), 0, 0);
            promptPanel.Controls.Add(CreatePromptBox(), 1, 0);
            promptPanel.Controls.Add(CreateInfoLabel("Agent system prompt"), 2, 0);
            promptPanel.Controls.Add(_agentPrompt, 3, 0);

            promptPanel.Controls.Add(new Label
            {
                Text = "Effective system prompt",
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.4f, FontStyle.Bold)
            }, 0, 1);
            promptPanel.Controls.Add(new Label
            {
                Text = "",
                Dock = DockStyle.Fill,
                AutoSize = false,
                ForeColor = Accent,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding = new Padding(0, 2, 0, 0)
            }, 1, 1);
            _promptResolution = (Label)promptPanel.GetControlFromPosition(1, 1);
            promptPanel.Controls.Add(new Label
            {
                Text = "",
                Dock = DockStyle.Fill,
                AutoSize = false,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.4f),
                Padding = new Padding(8, 2, 0, 0)
            }, 2, 1);
            promptPanel.Controls.Add(new Label
            {
                Text = "Read-only preview. Provider prompt is used only when the agent enables inheritance.",
                Dock = DockStyle.Fill,
                AutoSize = false,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.4f),
                Padding = new Padding(0, 2, 0, 0)
            }, 3, 1);

            _providerPrompt.ReadOnly = true;
            _providerPrompt.Multiline = true;
            _providerPrompt.ScrollBars = ScrollBars.Vertical;
            _providerPrompt.Dock = DockStyle.Fill;
            _providerPrompt.Font = new Font("Segoe UI", 8.3f);
            _providerPrompt.BackColor = Color.White;
            _providerPrompt.BorderStyle = BorderStyle.FixedSingle;
            promptPanel.Controls[1, 0] = _providerPrompt;

            _agentPrompt.ReadOnly = true;
            _agentPrompt.Multiline = true;
            _agentPrompt.ScrollBars = ScrollBars.Vertical;
            _agentPrompt.Dock = DockStyle.Fill;
            _agentPrompt.Font = new Font("Segoe UI", 8.3f);
            _agentPrompt.BackColor = Color.White;
            _agentPrompt.BorderStyle = BorderStyle.FixedSingle;

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 3, 0, 0),
                BackColor = Surface
            };

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
                AutoSize = false,
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

            _globalStatus.Text = "Ready";
            _globalStatus.AutoSize = true;
            _globalStatus.ForeColor = Muted;
            _globalStatus.Margin = new Padding(12, 11, 0, 0);
            actions.Controls.Add(_globalStatus);

            root.Controls.Add(heading, 0, 0);
            root.Controls.Add(promptPanel, 0, 1);
            root.Controls.Add(actions, 0, 2);
            root.Controls.Add(_tabs, 0, 3);
            root.Controls.Add(outputPanel, 0, 4);
            BodyPanel.Controls.Add(root);
        }

        private static Label CreateInfoLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.4f, FontStyle.Bold),
                Padding = new Padding(0, 1, 4, 0)
            };
        }

        private TextBox CreatePromptBox()
        {
            return _providerPrompt;
        }

        private FlowLayoutPanel GetActionsPanel()
        {
            var root = BodyPanel.Controls.OfType<TableLayoutPanel>().First();
            return (FlowLayoutPanel)root.GetControlFromPosition(0, 2);
        }

        private void AddFeatureTabs()
        {
            AddApiTab("Messaging", "Send message", "Calls HAgentClient.SendAsync with the selected agent.", "A conversational model should return exactly MESSAGE-OK.", "Reply with exactly MESSAGE-OK and nothing else.", SendMessageAsync, "Provider/model warning", "Use a conversational model. The provider model catalog can also contain guard, classification, embedding, and other non-chat models.");
            AddApiTab("Session", "Run session test", "Creates one AgentSession and sends the editable first message followed by a fixed recall question. The second request receives the complete session history.", "The response should identify HAgent-session-42, and the global output should show the retained transcript.", "Store this temporary test value in our conversation: HAgent-session-42.", TestSessionAsync, "Memory boundary", "This validates in-session conversation history. Durable long-term memory is tested separately in the Memory tab.");
            AddApiTab("Runtime 0.2", "Run runtime test", "Uses the 0.2 execution pipeline with timeout, provider-attempt, retry, lifecycle, and diagnostics behavior.", "Execution should reach Succeeded and display the execution ID, state, provider, model, and response.", "Reply with the word RUNTIME-OK and nothing else.", TestRuntimeAsync, "Runtime boundary", "The runtime orchestrates execution; it does not infer whether a selected model is suitable for the requested task.");
            AddApiTab("Configuration", "Read configuration", "Reads providers and agents directly from the local file store.", "The output should show the settings path, provider count, agent count, and provider relationships.", "No AI request is sent by this example.", ReadConfigurationAsync, "Storage boundary", "This verifies host-side configuration reading, not database persistence.");
            AddApiTab("Memory", "Run memory test", "Writes an explicit memory entry to the persistent file memory store, disposes it, opens a new store instance, recalls the entry, and removes it.", "The second store instance should recall HAgent-memory-42, proving persistence beyond the first process object.", "Remember exactly this test value: HAgent-memory-42.", TestMemoryAsync, "Memory boundary", "This is provider-independent and makes no AI request. It validates explicit durable memory operations, not automatic conversation memory.");
        }

        private void AddApiTab(string title, string buttonText, string description, string expected, string initialMessage, Func<string, Task> test, string noteTitle, string noteText)
        {
            var page = CreateExampleTab(title);
            _tabs.TabPages.Add(page);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Surface,
                Padding = new Padding(22)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));

            var messageBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = title == "Configuration" ? false : true,
                ScrollBars = title == "Configuration" ? ScrollBars.None : ScrollBars.Vertical,
                Text = initialMessage,
                Font = new Font("Segoe UI", 9.2f),
                BackColor = Color.White,
                ForeColor = Text,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 4, 0, 4)
            };
            _messageInputs[page] = messageBox;

            var runButton = CreateButton(buttonText, 190);
            runButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            runButton.Click += async delegate { await RunExampleAsync(delegate { return test(messageBox.Text); }); };
            _testButtons.Add(runButton);

            layout.Controls.Add(runButton, 0, 0);
            layout.Controls.Add(CreateSectionLabel("Sent message / input  — editable"), 0, 1);
            layout.Controls.Add(messageBox, 0, 2);
            layout.Controls.Add(CreateSectionLabel("Description\r\n" + description + "\r\n\r\nExpected result\r\n" + expected), 0, 3);
            layout.Controls.Add(new Label
            {
                Text = noteTitle + ": " + noteText,
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.6f),
                Padding = new Padding(1, 8, 20, 0)
            }, 0, 4);

            page.Controls.Add(layout);
        }

        private static Label CreateSectionLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                AutoSize = false,
                ForeColor = Text,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(1, 6, 20, 0)
            };
        }

        private void ConfigureAgentSelector()
        {
            _agentSelector.Width = 240;
            _agentSelector.Height = 30;
            _agentSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            _agentSelector.Font = new Font("Segoe UI", 9.1f);
            _agentSelector.Margin = new Padding(0, 5, 8, 0);
            _agentSelector.SelectedIndexChanged += delegate { UpdateSelectedAgentStatus(); };
            GetActionsPanel().Controls.Add(_agentSelector);
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

                UpdatePromptPreview();
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

        private async Task SendMessageAsync(string message)
        {
            var selection = await CreateClientAndAgentAsync();
            var response = await selection.Client.SendAsync(selection.Agent.Id, RequireInput(message));
            Write("SEND MESSAGE", "Agent: " + selection.Agent.Name + Environment.NewLine + "Provider: " + selection.Provider.Name + Environment.NewLine + "Model: " + selection.Model + Environment.NewLine + "Response: " + response.Text);
        }

        private async Task TestSessionAsync(string firstMessage)
        {
            var selection = await CreateClientAndAgentAsync();
            var session = selection.Client.CreateSession(selection.Agent.Id);
            await session.SendAsync(RequireInput(firstMessage));
            var response = await session.SendAsync("What temporary test value did I just give you? Reply with only the value.");
            var read = await session.ReadAsync();
            Write("SESSION", "Agent: " + selection.Agent.Name + Environment.NewLine + "Provider: " + selection.Provider.Name + Environment.NewLine + "Model: " + selection.Model + Environment.NewLine + "Second response: " + response.Text + Environment.NewLine + "Messages retained: " + read.Messages.Count + Environment.NewLine + "Transcript:" + Environment.NewLine + string.Join(Environment.NewLine, read.Messages.Select(x => "  " + x.Role + ": " + x.Content)));
        }

        private async Task TestRuntimeAsync(string message)
        {
            var selection = await CreateClientAndAgentAsync();
            var execution = await selection.Client.ExecuteAsync(
                selection.Agent.Id,
                RequireInput(message),
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(30),
                    MaxProviderAttempts = 2,
                    MaxRetriesPerProvider = 1
                },
                CancellationToken.None);

            Write("RUNTIME", "Execution: " + execution.Id + Environment.NewLine + "State: " + execution.State + Environment.NewLine + "Failure: " + execution.FailureKind + Environment.NewLine + "Provider: " + selection.Provider.Name + " (" + execution.Response.ProviderId + ")" + Environment.NewLine + "Model: " + selection.Model + Environment.NewLine + "Response: " + execution.Response.Text);
        }

        private async Task ReadConfigurationAsync(string message)
        {
            var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
            var providers = await store.GetProvidersAsync();
            var agents = await store.GetAgentsAsync();
            Write("CONFIGURATION", "Settings: " + Path.Combine(_basePath, "settings.json") + Environment.NewLine + "Providers: " + providers.Count + Environment.NewLine + string.Join(Environment.NewLine, providers.Select(p => "  - " + p.Name + " [" + p.Kind + "] model=" + p.DefaultModel)) + Environment.NewLine + "Agents: " + agents.Count + Environment.NewLine + string.Join(Environment.NewLine, agents.Select(a => "  - " + a.Name + " -> " + a.ProviderId)));
            await Task.CompletedTask;
        }

        private async Task TestMemoryAsync(string message)
        {
            var memoryPath = Path.Combine(_basePath, "memory", "example-memory.jsonl");
            var firstId = string.Empty;
            var firstStore = new FileMemoryStore(memoryPath);
            try
            {
                firstId = await StoreExampleMemoryAsync(firstStore, RequireInput(message));
            }
            finally
            {
                firstStore.Dispose();
            }

            var secondStore = new FileMemoryStore(memoryPath);
            try
            {
                var recalled = await secondStore.SearchAsync(new MemoryQuery
                {
                    OwnerId = "HAgent.Example",
                    Scope = MemoryScope.Application,
                    Text = "HAgent-memory-42",
                    MaxResults = 10
                });

                var found = recalled.FirstOrDefault(x => string.Equals(x.Id, firstId, StringComparison.OrdinalIgnoreCase));
                var text = found == null
                    ? "Persistence test failed: the second store instance could not recall the entry."
                    : "Persistence test succeeded." + Environment.NewLine + "Memory ID: " + found.Id + Environment.NewLine + "Content: " + found.Content + Environment.NewLine + "Scope: " + found.Scope + Environment.NewLine + "Owner: " + found.OwnerId;

                if (found != null)
                    await secondStore.RemoveAsync(found.Id);

                Write("MEMORY", "Store path: " + memoryPath + Environment.NewLine + text);
            }
            finally
            {
                secondStore.Dispose();
            }
        }

        private static async Task<string> StoreExampleMemoryAsync(FileMemoryStore store, string message)
        {
            var value = message.IndexOf("HAgent-memory-42", StringComparison.OrdinalIgnoreCase) >= 0
                ? "HAgent-memory-42"
                : message.Trim();

            var entry = new MemoryEntry
            {
                Scope = MemoryScope.Application,
                OwnerId = "HAgent.Example",
                Content = value,
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "source", "HAgent.Example" },
                    { "test", "persistent-memory" }
                },
                CreatedAt = DateTimeOffset.UtcNow
            };

            await store.AddAsync(entry, CancellationToken.None);
            return entry.Id;
        }

        private void OpenConfiguration()
        {
            var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            AISettings.ShowMainAISettingsForm(store, secrets, this, new[] { new OpenAICompatibleProviderAdapter() });
            _ = RefreshAgentsAsync();
        }

        private void UpdateSelectedAgentStatus()
        {
            var agent = GetSelectedAgent();
            if (agent == null)
            {
                _globalStatus.Text = "No agent selected";
                UpdatePromptPreview();
                return;
            }

            _globalStatus.Text = agent.Enabled
                ? "Selected: " + agent.Name
                : "Selected: " + agent.Name + " (disabled)";
            _globalStatus.ForeColor = agent.Enabled ? Muted : Color.FromArgb(185, 28, 28);
            UpdatePromptPreview();
        }

        private async void UpdatePromptPreview()
        {
            try
            {
                var agent = GetSelectedAgent();
                if (agent == null)
                {
                    _providerPrompt.Clear();
                    _agentPrompt.Clear();
                    _promptResolution.Text = "No agent selected";
                    return;
                }

                var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
                var providers = await store.GetProvidersAsync();
                var provider = providers.FirstOrDefault(p => string.Equals(p.Id, agent.ProviderId, StringComparison.OrdinalIgnoreCase));
                _providerPrompt.Text = provider == null ? "No provider selected / provider not found." : (provider.DefaultSystemPrompt ?? string.Empty);
                _agentPrompt.Text = agent.SystemPrompt ?? string.Empty;

                if (provider == null)
                {
                    _promptResolution.Text = "Provider prompt unavailable."
                    ;
                }
                else if (agent.UseProviderSystemPrompt && !string.IsNullOrWhiteSpace(provider.DefaultSystemPrompt) && !string.IsNullOrWhiteSpace(agent.SystemPrompt))
                {
                    _promptResolution.Text = "Used: Provider system prompt + Agent system prompt (agent inherits provider prompt).";
                }
                else if (agent.UseProviderSystemPrompt && !string.IsNullOrWhiteSpace(provider.DefaultSystemPrompt))
                {
                    _promptResolution.Text = "Used: Provider system prompt only.";
                }
                else if (!string.IsNullOrWhiteSpace(agent.SystemPrompt))
                {
                    _promptResolution.Text = "Used: Agent system prompt only. Provider prompt is not inherited.";
                }
                else
                {
                    _promptResolution.Text = "Used: No system prompt.";
                }
            }
            catch
            {
                _providerPrompt.Clear();
                _agentPrompt.Clear();
                _promptResolution.Text = "Prompt preview unavailable.";
            }
        }

        private static string RequireInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("The example input cannot be empty.", nameof(input));
            return input;
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

        private HButton CreateButton(string text, int width)
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
            _globalStatus.ForeColor = Accent;
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

        private void Write(string title, string value)
        {
            _output.Text = "[" + title + "]" + Environment.NewLine +
                           DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine +
                           value + Environment.NewLine;
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

        private static TabPage CreateExampleTab(string title)
        {
            return new TabPage(title)
            {
                BackColor = Surface,
                Padding = new Padding(0)
            };
        }
    }
}
