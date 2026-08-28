using System;
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
        private readonly HButton _clearButton;
        private readonly HButton _sendButton;
        private readonly HButton _sessionButton;
        private readonly HButton _runtimeButton;
        private readonly HButton _readButton;
        private readonly Label _status = new Label();
        private readonly TextBox _output = new TextBox();

        private OpenAICompatibleProviderAdapter CreateAdapter()
        {
            return new OpenAICompatibleProviderAdapter();
        }

        public MainForm()
            : base(
                "HAgent Example",
                "Manual integration host for testing completed HAgent features",
                new Size(980, 650),
                new Size(820, 560))
        {
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            Build();

            _configurationButton = CreateButton("Configuration", 150);
            _configurationButton.Click += delegate { OpenConfiguration(); };
            ((FlowLayoutPanel)BodyPanel.Controls[1]).Controls.Add(_configurationButton);

            _clearButton = CreateButton("Clear output", 120);
            _clearButton.Click += delegate { _output.Clear(); _status.Text = "Ready"; };
            ((FlowLayoutPanel)BodyPanel.Controls[1]).Controls.Add(_clearButton);

            _sendButton = AddAction("Send message", 150);
            _sendButton.Click += async delegate { await SendMessageAsync(); };

            _sessionButton = AddAction("Test session", 150);
            _sessionButton.Click += async delegate { await TestSessionAsync(); };

            _runtimeButton = AddAction("Test runtime", 150);
            _runtimeButton.Click += async delegate { await TestRuntimeAsync(); };

            _readButton = AddAction("Read configuration", 170);
            _readButton.Click += async delegate { await ReadConfigurationAsync(); };
        }

        private void Build()
        {
            BodyPanel.Padding = new Padding(28);

            var intro = new Label
            {
                Text = "Use this application as the manual test bench for HAgent. Configure providers and agents, then run each example. New library features should add a corresponding example here.",
                Dock = DockStyle.Top,
                Height = 52,
                ForeColor = Color.FromArgb(100, 92, 120),
                Font = new Font("Segoe UI", 9.5f)
            };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 0)
            };

            _status.Text = "Ready";
            _status.AutoSize = true;
            _status.ForeColor = Color.FromArgb(100, 92, 120);
            _status.Margin = new Padding(14, 16, 0, 0);
            actions.Controls.Add(_status);

            _output.Multiline = true;
            _output.ReadOnly = true;
            _output.ScrollBars = ScrollBars.Both;
            _output.Dock = DockStyle.Fill;
            _output.Font = new Font("Consolas", 9f);
            _output.BackColor = Color.White;
            _output.BorderStyle = BorderStyle.FixedSingle;

            BodyPanel.Controls.Add(_output);
            BodyPanel.Controls.Add(actions);
            BodyPanel.Controls.Add(intro);
        }

        private HButton AddAction(string text, int width)
        {
            var button = CreateButton(text, width);
            ((FlowLayoutPanel)BodyPanel.Controls[1]).Controls.Add(button);
            return button;
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
                Margin = new Padding(0, 6, 10, 0)
            };
        }

        private void OpenConfiguration()
        {
            var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            var adapters = new[] { CreateAdapter() };
            AISettings.ShowMainAISettingsForm(store, secrets, this, adapters);
        }

        private async Task SendMessageAsync()
        {
            try
            {
                SetBusy("Sending message...");
                var (client, agent) = await CreateClientAndAgentAsync();
                var response = await client.SendAsync(agent.Id, "Reply with a concise confirmation that HAgent is connected.");
                Write("SEND MESSAGE", response.Text);
                SetReady();
            }
            catch (Exception ex)
            {
                ShowException("Send message failed.", ex);
            }
        }

        private async Task TestSessionAsync()
        {
            try
            {
                SetBusy("Running session example...");
                var (client, agent) = await CreateClientAndAgentAsync();
                var session = client.CreateSession(agent.Id);
                await session.SendAsync("Remember this test value: HAgent-session-42.");
                await session.SendAsync("What test value did I ask you to remember?");
                var read = await session.ReadAsync();
                Write("SESSION", string.Join(Environment.NewLine, read.Messages.Select(x => x.Role + ": " + x.Content)));
                SetReady();
            }
            catch (Exception ex)
            {
                ShowException("Session test failed.", ex);
            }
        }

        private async Task TestRuntimeAsync()
        {
            try
            {
                SetBusy("Running runtime example...");
                var (client, agent) = await CreateClientAndAgentAsync();
                var execution = await client.ExecuteAsync(
                    agent.Id,
                    "Reply with the word RUNTIME and nothing else.",
                    new AgentExecutionOptions
                    {
                        Timeout = TimeSpan.FromSeconds(30),
                        MaxProviderAttempts = 2,
                        MaxRetriesPerProvider = 1
                    },
                    CancellationToken.None);

                Write("RUNTIME", "Execution: " + execution.Id + Environment.NewLine +
                                 "State: " + execution.State + Environment.NewLine +
                                 "Provider: " + execution.Response.ProviderId + Environment.NewLine +
                                 "Model: " + execution.Response.Model + Environment.NewLine +
                                 "Response: " + execution.Response.Text);
                SetReady();
            }
            catch (Exception ex)
            {
                ShowException("Runtime test failed.", ex);
            }
        }

        private async Task ReadConfigurationAsync()
        {
            try
            {
                SetBusy("Reading configuration...");
                var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
                var providers = await store.GetProvidersAsync();
                var agents = await store.GetAgentsAsync();
                Write("CONFIGURATION", "Providers: " + providers.Count + Environment.NewLine +
                                       string.Join(Environment.NewLine, providers.Select(p => "  - " + p.Name + " [" + p.Kind + "]")) +
                                       Environment.NewLine +
                                       "Agents: " + agents.Count + Environment.NewLine +
                                       string.Join(Environment.NewLine, agents.Select(a => "  - " + a.Name + " -> " + a.ProviderId)));
                SetReady();
            }
            catch (Exception ex)
            {
                ShowException("Configuration read failed.", ex);
            }
        }

        private async Task<(HAgentClient Client, AiAgent Agent)> CreateClientAndAgentAsync()
        {
            var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            var agents = await store.GetAgentsAsync();
            var agent = agents.FirstOrDefault(a => a.Enabled);
            if (agent == null)
                throw new InvalidOperationException("No enabled agent exists. Open Configuration and create an agent first.");

            var client = new HAgentClient(
                store,
                secrets,
                new[] { CreateAdapter() });

            return (client, agent);
        }

        private void SetBusy(string text)
        {
            SetButtonsEnabled(false);
            _status.Text = text;
            _status.ForeColor = Color.FromArgb(116, 76, 210);
        }

        private void SetReady()
        {
            SetButtonsEnabled(true);
            _status.Text = "Ready";
            _status.ForeColor = Color.FromArgb(100, 92, 120);
        }

        private void SetButtonsEnabled(bool enabled)
        {
            _configurationButton.Enabled = enabled;
            _clearButton.Enabled = enabled;
            _sendButton.Enabled = enabled;
            _sessionButton.Enabled = enabled;
            _runtimeButton.Enabled = enabled;
            _readButton.Enabled = enabled;
        }

        private void Write(string title, string text)
        {
            _output.Text = "[" + title + "]" + Environment.NewLine +
                           DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine +
                           text + Environment.NewLine;
        }

        private void ShowException(string message, Exception ex)
        {
            SetReady();
            HMessage.ShowException(this, message, "HAgent Example", ex);
        }
    }
}
