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
        private static readonly Color Error = Color.FromArgb(185, 28, 28);

        private readonly string _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HAgent");
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

        public MainForm()
            : base("HAgent Example", "Manual integration and feature-verification host", new Size(1280, 820), new Size(1000, 680))
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

            root.Controls.Add(heading, 0, 0);
            root.Controls.Add(promptPanel, 0, 1);
            root.Controls.Add(actions, 0, 2);
            root.Controls.Add(_tabs, 0, 3);
            root.Controls.Add(outputPanel, 0, 4);
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
            return (FlowLayoutPanel)root.GetControlFromPosition(0, 2);
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

        private void AddFeatureTabs()
        {
            AddApiTab("Messaging", "Send message", "Calls HAgentClient.SendAsync with the selected agent.", "A conversational model should return exactly MESSAGE-OK.", "Reply with exactly MESSAGE-OK and nothing else.", SendMessageAsync, "Provider/model warning", "Choose a conversational model; model discovery can also return guard, classification, embedding, and other non-chat models.");
            AddApiTab("Session", "Run session test", "Creates one AgentSession, sends the editable first message, then asks the fixed recall question. The second request receives the session history.", "The response should identify HAgent-session-42 and the output should show the retained transcript.", "Store this temporary test value in our conversation: HAgent-session-42.", TestSessionAsync, "Memory boundary", "This validates in-session history forwarding. Durable memory is tested separately.");
            AddApiTab("Runtime 0.2", "Run runtime test", "Uses the 0.2 execution pipeline with timeout, provider-attempt, retry, lifecycle, and diagnostics behavior.", "Execution should reach Succeeded and show execution ID, state, provider, model, and response.", "Reply with the word RUNTIME-OK and nothing else.", TestRuntimeAsync, "Runtime boundary", "The runtime orchestrates execution but does not yet infer model suitability.");
            AddApiTab("Configuration", "Read configuration", "Reads providers and agents directly from the local file store.", "The output should show the settings path, provider count, agent count, and relationships.", "No AI request is sent by this example.", ReadConfigurationAsync, "Storage boundary", "This verifies host-side configuration reading.");
            AddApiTab("Memory", "Run memory test", "Writes explicit memory to a persistent file store, closes it, opens a second instance, recalls the entry, and removes it.", "The second store instance should recall the same memory ID and content, proving persistence outside the original object.", "Remember exactly this test value: HAgent-memory-42.", TestMemoryAsync, "Memory boundary", "Provider-independent and no AI request. Tests explicit durable memory operations.");
        }

        private void AddApiTab(string title, string buttonText, string description, string expected, string initialMessage, Func<string, Task> test, string noteTitle, string noteText)
        {
            var page = new TabPage(title) { BackColor = Surface, Padding = new Padding(0) };
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
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));

            var input = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = title != "Configuration",
                ScrollBars = title == "Configuration" ? ScrollBars.None : ScrollBars.Vertical,
                Text = initialMessage,
                Font = new Font("Segoe UI", 9.2f),
                BackColor = Color.White,
                ForeColor = Text,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 2, 0, 4)
            };

            var runButton = CreateButton(buttonText, 190);
            runButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            runButton.Click += async delegate { await RunExampleAsync(delegate { return test(input.Text); }); };
            _testButtons.Add(runButton);

            layout.Controls.Add(runButton, 0, 0);
            layout.Controls.Add(new Label
            {
                Text = "Sent message / input  — editable",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(1, 3, 0, 0)
            }, 0, 1);
            layout.Controls.Add(input, 0, 2);
            layout.Controls.Add(new Label
            {
                Text = "Description\r\n" + description + "\r\n\r\nExpected result\r\n" + expected,
                Dock = DockStyle.Fill,
                ForeColor = Text,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(1, 4, 20, 0)
            }, 0, 3);
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

                if (!string.IsNullOrWhiteSpace(previousId)) SelectAgent(previousId);
                if (_agentSelector.SelectedIndex < 0 && _agentSelector.Items.Count > 0) _agentSelector.SelectedIndex = 0;
                await UpdateSelectedAgentAsync();
            }
            catch (Exception ex)
            {
                _globalStatus.Text = "Agent list could not be loaded";
                _globalStatus.ForeColor = Error;
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

        private async Task UpdateSelectedAgentAsync()
        {
            var agent = GetSelectedAgent();
            if (agent == null)
            {
                _globalStatus.Text = "No agent selected";
                _globalStatus.ForeColor = Muted;
                ClearPromptPreview();
                return;
            }

            _globalStatus.Text = agent.Enabled ? "Selected: " + agent.Name : "Selected: " + agent.Name + " (disabled)";
            _globalStatus.ForeColor = agent.Enabled ? Muted : Error;

            try
            {
                var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
                var providers = await store.GetProvidersAsync();
                var providerIds = new List<string>();
                if (!string.IsNullOrWhiteSpace(agent.ProviderId)) providerIds.Add(agent.ProviderId);
                if (agent.ProviderIds != null) providerIds.AddRange(agent.ProviderIds.Where(x => !string.IsNullOrWhiteSpace(x)));
                var provider = providerIds.Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(id => providers.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)))
                    .FirstOrDefault(p => p != null);

                _providerPrompt.Text = provider == null ? "No configured provider." : (provider.DefaultSystemPrompt ?? string.Empty);
                _agentPrompt.Text = agent.SystemPrompt ?? string.Empty;

                if (provider == null)
                    _promptResolution.Text = "Provider prompt unavailable.";
                else if (agent.UseProviderSystemPrompt && !string.IsNullOrWhiteSpace(provider.DefaultSystemPrompt) && !string.IsNullOrWhiteSpace(agent.SystemPrompt))
                    _promptResolution.Text = "Provider + Agent prompts are used; agent inherits the provider prompt.";
                else if (agent.UseProviderSystemPrompt && !string.IsNullOrWhiteSpace(provider.DefaultSystemPrompt))
                    _promptResolution.Text = "Provider system prompt is used.";
                else if (!string.IsNullOrWhiteSpace(agent.SystemPrompt))
                    _promptResolution.Text = "Agent system prompt is used; provider prompt is not inherited.";
                else
                    _promptResolution.Text = "No system prompt is configured.";
            }
            catch
            {
                ClearPromptPreview();
            }
        }

        private void ClearPromptPreview()
        {
            _providerPrompt.Clear();
            _agentPrompt.Clear();
            _promptResolution.Text = "No prompt information available.";
        }

        private async Task<ClientSelection> CreateClientAndAgentAsync()
        {
            var agent = GetSelectedAgent();
            if (agent == null) throw new InvalidOperationException("Select an agent first.");
            if (!agent.Enabled) throw new InvalidOperationException("The selected agent is disabled. Enable it in Configuration first.");

            var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            var providers = await store.GetProvidersAsync();
            var providerIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(agent.ProviderId)) providerIds.Add(agent.ProviderId);
            if (agent.ProviderIds != null) providerIds.AddRange(agent.ProviderIds.Where(x => !string.IsNullOrWhiteSpace(x)));

            var provider = providerIds.Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(id => providers.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault(p => p != null && p.Enabled);
            if (provider == null) throw new InvalidOperationException("The selected agent has no enabled provider. Agent='" + agent.Name + "'.");

            var model = string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model;
            if (string.IsNullOrWhiteSpace(model)) throw new InvalidOperationException("No model is configured for agent '" + agent.Name + "' or provider '" + provider.Name + "'.");

            return new ClientSelection(new HAgentClient(store, secrets, new[] { new OpenAICompatibleProviderAdapter() }), agent, provider, model);
        }

        private async Task SendMessageAsync(string message)
        {
            var selection = await CreateClientAndAgentAsync();
            var response = await selection.Client.SendAsync(selection.Agent.Id, RequireInput(message));
            Write("SEND MESSAGE", "Agent: " + selection.Agent.Name + Environment.NewLine + "Provider: " + selection.Provider.Name + Environment.NewLine + "Model: " + selection.Model + Environment.NewLine + "Request: " + message + Environment.NewLine + "Response: " + response.Text);
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
            var execution = await selection.Client.ExecuteAsync(selection.Agent.Id, RequireInput(message), new AgentExecutionOptions { Timeout = TimeSpan.FromSeconds(30), MaxProviderAttempts = 2, MaxRetriesPerProvider = 1 }, CancellationToken.None);
            Write("RUNTIME", "Execution: " + execution.Id + Environment.NewLine + "State: " + execution.State + Environment.NewLine + "Failure: " + execution.FailureKind + Environment.NewLine + "Provider: " + selection.Provider.Name + " (" + execution.Response.ProviderId + ")" + Environment.NewLine + "Model: " + selection.Model + Environment.NewLine + "Response: " + execution.Response.Text);
        }

        private async Task ReadConfigurationAsync(string unused)
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
            var firstStore = new FileMemoryStore(memoryPath);
            string memoryId;
            var originalInput = RequireInput(message);
            var storedContent = ExtractMemorySearchText(originalInput);
            try
            {
                var entry = new MemoryEntry
                {
                    Scope = MemoryScope.Application,
                    OwnerId = "HAgent.Example",
                    Content = storedContent,
                    Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "source", "HAgent.Example" }, { "test", "persistent-memory" } },
                    CreatedAt = DateTimeOffset.UtcNow
                };
                memoryId = entry.Id;
                await firstStore.AddAsync(entry, CancellationToken.None);
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
                    Text = storedContent,
                    MaxResults = 10
                }, CancellationToken.None);

                var found = recalled.FirstOrDefault(x => string.Equals(x.Id, memoryId, StringComparison.OrdinalIgnoreCase));
                if (found == null) throw new InvalidOperationException("The second memory-store instance could not recall the persisted entry.");
                await secondStore.RemoveAsync(found.Id, CancellationToken.None);
                Write("MEMORY", "Store path: " + memoryPath + Environment.NewLine + "Persistence test succeeded." + Environment.NewLine + "Memory ID: " + found.Id + Environment.NewLine + "Content: " + found.Content + Environment.NewLine + "Scope: " + found.Scope + Environment.NewLine + "Owner: " + found.OwnerId);
            }
            finally
            {
                secondStore.Dispose();
            }
        }

        private static string ExtractMemorySearchText(string input)
        {
            var value = input ?? string.Empty;
            var marker = "test value";
            var index = value.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var colon = value.IndexOf(':', index);
                if (colon >= 0 && colon + 1 < value.Length)
                    return value.Substring(colon + 1).Trim().TrimEnd('.', '!', '?');
            }
            return value.Trim();
        }

        private static string RequireInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("The example input cannot be empty.", nameof(input));
            return input;
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
            _ = UpdateSelectedAgentAsync();
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
            _output.Text = "[" + title + "]" + Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + value + Environment.NewLine;
        }

        private sealed class AgentItem
        {
            public AgentItem(AiAgent agent) { Agent = agent; }
            public AiAgent Agent { get; private set; }
            public override string ToString() { return Agent.Enabled ? Agent.Name : Agent.Name + " (Disabled)"; }
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
