using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.WinForms;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;
using HAgent.Storage.File;
using System.IO;
using System.Linq;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
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
            foreach (var button in _testButtons)
                button.Enabled = enabled;
        }

        private void Write(string title, string value)
        {
            _output.Text = "[" + title + "]" + Environment.NewLine +
                           DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine +
                           value + Environment.NewLine;
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

        private async Task RefreshExampleAgentsAsync()
        {
            var configurationPath = Path.Combine(_basePath, "configuration", "settings.json");
            var store = new FileAiStore(configurationPath);
            var agents = await store.GetAgentsAsync();

            _agents.Clear();
            _agents.AddRange(agents);

            _agentSelector.BeginUpdate();
            try
            {
                _agentSelector.Items.Clear();
                foreach (var agent in _agents)
                    _agentSelector.Items.Add(new AgentItem(agent));

                if (_agentSelector.Items.Count > 0)
                    _agentSelector.SelectedIndex = 0;
            }
            finally
            {
                _agentSelector.EndUpdate();
            }
        }

        private void OpenConfiguration()
        {
            var store = new HAgent.Storage.File.FileAiStore(System.IO.Path.Combine(_basePath, "configuration", "settings.json"));
            var secrets = new HAgent.Storage.File.ProtectedDataSecretStore(System.IO.Path.Combine(_basePath, "secrets"));
            AISettings.ShowMainAISettingsForm(
                store,
                secrets,
                this,
                new[] { new HAgent.Providers.OpenAICompatible.OpenAICompatibleProviderAdapter() });
            _ = RefreshExampleAgentsAsync();
        }

        private sealed class AgentItem
        {
            public AgentItem(AiAgent agent)
            {
                Agent = agent;
            }

            public AiAgent Agent { get; private set; }

            public override string ToString()
            {
                return Agent.Enabled ? Agent.Name : Agent.Name + " (Disabled)";
            }
        }

        private sealed class ClientSelection
        {
            public ClientSelection(HAgent.Runtime.HAgentClient client, AiAgent agent, AiProvider provider, string model)
            {
                Client = client;
                Agent = agent;
                Provider = provider;
                Model = model;
            }

            public HAgent.Runtime.HAgentClient Client { get; private set; }
            public AiAgent Agent { get; private set; }
            public AiProvider Provider { get; private set; }
            public string Model { get; private set; }
        }
    }
}
