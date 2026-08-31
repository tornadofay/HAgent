using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.WinForms;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;
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
            HAgentStorageOptions options = null;
            try
            {
                options = await LoadStorageOptionsAsync().ConfigureAwait(true);
                var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
                var agents = await store.GetAgentsAsync().ConfigureAwait(true);

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

                _globalStatus.Text = "Ready — HAgent storage: " + options.StorageType;
                _globalStatus.ForeColor = Muted;
            }
            catch (Exception ex)
            {
                _agents.Clear();
                _agentSelector.BeginUpdate();
                try
                {
                    _agentSelector.Items.Clear();
                }
                finally
                {
                    _agentSelector.EndUpdate();
                }

                _globalStatus.Text = "HAgent storage unavailable — open Configuration to repair it.";
                _globalStatus.ForeColor = Error;

                var details = options == null
                    ? "The HAgent storage configuration could not be loaded or validated."
                    : BuildStorageDiagnostic(options);

                Write("STORAGE UNAVAILABLE", details + Environment.NewLine +
                                          "Exception:" + Environment.NewLine +
                                          ex);
            }
        }

        private static string BuildStorageDiagnostic(HAgentStorageOptions options)
        {
            if (options == null)
                return "No HAgent storage configuration is available.";

            switch (options.StorageType)
            {
                case HAgentStorageType.File:
                    return "Backend: File" + Environment.NewLine +
                           "Root: " + options.GetEffectiveRootPath();

                case HAgentStorageType.SqlServer:
                case HAgentStorageType.MySql:
                {
                    var profile = options.GetDatabaseProfile(options.StorageType);
                    return "Backend: " + options.StorageType + Environment.NewLine +
                           "Server: " + (profile == null ? "<missing>" : (string.IsNullOrWhiteSpace(profile.ServerName) ? "<missing>" : profile.ServerName)) + Environment.NewLine +
                           "Port: " + (profile == null ? "<missing>" : profile.GetEffectivePort(options.StorageType).ToString()) + Environment.NewLine +
                           "User: " + (profile == null || string.IsNullOrWhiteSpace(profile.UserName) ? "<missing>" : profile.UserName) + Environment.NewLine +
                           "Database: " + options.GetEffectiveDatabaseName() + Environment.NewLine +
                           "Password secret: " + (profile == null || string.IsNullOrWhiteSpace(profile.PasswordSecretId) ? "<missing>" : profile.PasswordSecretId);
                }

                default:
                    return "Backend: " + options.StorageType;
            }
        }

        private async void OpenConfiguration()
        {
            try
            {
                var runtimeOptions = await LoadStorageOptionsAsync().ConfigureAwait(true);
                var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
                var secrets = new HAgent.Storage.File.ProtectedDataSecretStore(System.IO.Path.Combine(_basePath, "secrets"));
                var toolStore = await CreateConfiguredToolStoreAsync().ConfigureAwait(true);

                AISettings.ShowMainAISettingsForm(
                    store,
                    secrets,
                    this,
                    new[] { new HAgent.Providers.OpenAICompatible.OpenAICompatibleProviderAdapter() },
                    toolStore);

                var updatedOptions = await LoadStorageOptionsAsync().ConfigureAwait(true);
                if (HasRuntimeStorageChanges(runtimeOptions, updatedOptions))
                {
                    _globalStatus.Text = "Storage settings changed. Restart HAgent to apply the new storage configuration.";
                    _globalStatus.ForeColor = Accent;
                    Write("STORAGE RESTART REQUIRED", "Storage settings were changed. The current runtime remains on the previous storage backend until the application is restarted.");
                    return;
                }

                await RefreshExampleAgentsAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Write("STORAGE CONFIGURATION", "The configured HAgent storage backend could not be opened." + Environment.NewLine +
                                                   ex.Message + Environment.NewLine +
                                                   "Opening the Storage settings directly so the backend can be repaired without requiring a database connection.");
                HMessage.ShowException(this,
                    "The configured HAgent storage backend could not be opened. The Storage settings will be opened so you can repair the connection.",
                    "HAgent Storage",
                    ex);

                OpenStorageSettingsDirectly();
            }
        }

        private static bool HasRuntimeStorageChanges(HAgentStorageOptions before, HAgentStorageOptions after)
        {
            if (before == null || after == null) return before != after;

            return before.StorageType != after.StorageType
                || !string.Equals(before.ApplicationName, after.ApplicationName, StringComparison.Ordinal)
                || !string.Equals(before.RootPath, after.RootPath, StringComparison.Ordinal)
                || !ProfilesEqual(before.GetDatabaseProfile(HAgentStorageType.SqlServer), after.GetDatabaseProfile(HAgentStorageType.SqlServer))
                || !ProfilesEqual(before.GetDatabaseProfile(HAgentStorageType.MySql), after.GetDatabaseProfile(HAgentStorageType.MySql));
        }

        private static bool ProfilesEqual(HAgentDatabaseStorageOptions left, HAgentDatabaseStorageOptions right)
        {
            if (left == null || right == null) return left == right;
            return string.Equals(left.ServerName, right.ServerName, StringComparison.Ordinal)
                && left.Port == right.Port
                && string.Equals(left.UserName, right.UserName, StringComparison.Ordinal)
                && string.Equals(left.PasswordSecretId, right.PasswordSecretId, StringComparison.Ordinal);
        }

        private void OpenStorageSettingsDirectly()
        {
            var secrets = new HAgent.Storage.File.ProtectedDataSecretStore(System.IO.Path.Combine(_basePath, "secrets"));
            using (var form = new HAgent.WinForms.Forms.HAgentStorageSettingsForm(AppContext.BaseDirectory, System.Diagnostics.Process.GetCurrentProcess().ProcessName, secrets))
            {
                form.ShowDialog(this);
            }
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
