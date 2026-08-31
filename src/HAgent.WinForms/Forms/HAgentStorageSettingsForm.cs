using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Storage.File;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.Forms
{
    public sealed class HAgentStorageSettingsForm : HAgentForm
    {
        private const string SqlServerPasswordSecretId = "hagent-storage-sqlserver-password";
        private const string MySqlPasswordSecretId = "hagent-storage-mysql-password";
        private const string LegacyDatabasePasswordSecretId = "hagent.storage.database.password";
        private const string LegacyCurrentDatabasePasswordSecretId = "hagent-storage-database-password";

        private readonly FileHAgentStorageConfigurationStore _store;
        private readonly ISecretStore _secrets;
        private readonly ComboBox _storageType = new ComboBox();
        private readonly TextBox _applicationName = new TextBox();
        private readonly TextBox _rootPath = new TextBox();
        private readonly TextBox _serverName = new TextBox();
        private readonly TextBox _port = new TextBox();
        private readonly TextBox _userName = new TextBox();
        private readonly TextBox _password = new TextBox();
        private readonly Label _resolvedDatabase = new Label();
        private readonly Label _status = new Label();
        private HAgentStorageOptions _loadedOptions;
        private bool _loadingProfile;
        private HAgentStorageType _lastSelectedStorageType = HAgentStorageType.File;

        private static readonly Color Surface = Color.FromArgb(248, 248, 252);
        private static readonly Color Text = Color.FromArgb(68, 62, 88);
        private static readonly Color Muted = Color.FromArgb(100, 92, 120);
        private static readonly Color Accent = Color.FromArgb(116, 76, 210);

        public HAgentStorageSettingsForm(string basePath, string applicationName, ISecretStore secrets)
            : base("HAgent Storage", "HAgent-owned providers, agents, memory, tools, skills, wiki, and runtime storage", new Size(900, 690), new Size(760, 560))
        {
            if (secrets == null) throw new ArgumentNullException(nameof(secrets));

            var root = string.IsNullOrWhiteSpace(basePath) ? AppContext.BaseDirectory : basePath;
            var configurationPath = Path.Combine(root, "HAgentData", "configuration", "storage.json");
            _store = new FileHAgentStorageConfigurationStore(configurationPath);
            _secrets = secrets;

            BuildShell();
            _applicationName.Text = applicationName ?? string.Empty;
            _rootPath.Text = AppContext.BaseDirectory;
            _loadingProfile = true;
            _storageType.SelectedItem = HAgentStorageType.File;
            _loadingProfile = false;
            UpdateVisibility();
            UpdateDefaultPort(false);
            Shown += async delegate { await LoadAsync(); };
        }

        /// <summary>
        /// Indicates that the active runtime backend or its connection target changed and the host should reload its runtime configuration.
        /// </summary>
        public bool RuntimeStorageChanged { get; private set; }

        private void BuildShell()
        {
            BodyPanel.Padding = new Padding(26);
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9,
                BackColor = Surface,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 8; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _storageType.DropDownStyle = ComboBoxStyle.DropDownList;
            _storageType.Items.AddRange(new object[] { HAgentStorageType.File, HAgentStorageType.SqlServer, HAgentStorageType.MySql });
            _storageType.SelectedIndexChanged += async delegate
            {
                if (_loadingProfile) return;

                var previousType = _lastSelectedStorageType;
                var nextType = (HAgentStorageType)_storageType.SelectedItem;

                try
                {
                    CaptureProfileFromControls(previousType);
                    await PersistProfileAsync(previousType);
                    _lastSelectedStorageType = nextType;
                    LoadSelectedProfileIntoControls();
                }
                catch (Exception ex)
                {
                    _loadingProfile = true;
                    try { _storageType.SelectedItem = previousType; }
                    finally { _loadingProfile = false; }
                    _lastSelectedStorageType = previousType;
                    LoadSelectedProfileIntoControls();
                    _status.Text = "The storage profile could not be switched.";
                    _status.ForeColor = Color.FromArgb(185, 28, 28);
                    HMessage.ShowException(this, "The HAgent storage profile could not be switched.", "HAgent Storage", ex);
                }
            };
            AddRow(layout, 0, "Storage type", _storageType);

            ConfigureText(_applicationName);
            AddRow(layout, 1, "Host application name", _applicationName);

            ConfigureText(_rootPath);
            AddRow(layout, 2, "File root", _rootPath);

            ConfigureText(_serverName);
            AddRow(layout, 3, "Server name", _serverName);

            ConfigureText(_port);
            _port.KeyPress += delegate(object sender, KeyPressEventArgs e)
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
            };
            AddRow(layout, 4, "Port", _port);

            ConfigureText(_userName);
            AddRow(layout, 5, "User name", _userName);

            ConfigureText(_password);
            _password.UseSystemPasswordChar = true;
            AddRow(layout, 6, "Password", _password);

            _resolvedDatabase.Dock = DockStyle.Fill;
            _resolvedDatabase.ForeColor = Accent;
            _resolvedDatabase.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _resolvedDatabase.TextAlign = ContentAlignment.MiddleLeft;
            AddRow(layout, 7, "HAgent database", _resolvedDatabase);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Surface,
                Padding = new Padding(0, 8, 0, 0)
            };
            var save = CreateButton("Save settings", 145);
            save.Click += async delegate { await SaveAsync(); };
            actions.Controls.Add(save);

            var clear = CreateButton("Clear password", 145);
            clear.Click += async delegate
            {
                var type = (HAgentStorageType)_storageType.SelectedItem;
                var secretId = GetPasswordSecretId(type);
                _password.Clear();
                try { await _secrets.DeleteAsync(secretId); } catch { }
                var profile = type == HAgentStorageType.File ? null : _loadedOptions.GetDatabaseProfile(type);
                if (profile != null) profile.PasswordSecretId = secretId;
                _status.Text = "The selected database password secret was cleared.";
                _status.ForeColor = Muted;
            };
            actions.Controls.Add(clear);

            _status.AutoSize = true;
            _status.ForeColor = Muted;
            _status.Margin = new Padding(12, 11, 0, 0);
            actions.Controls.Add(_status);
            layout.Controls.Add(actions, 1, 8);

            BodyPanel.Controls.Add(layout);
        }

        private static void AddRow(TableLayoutPanel layout, int row, string label, Control control)
        {
            layout.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            }, 0, row);
            control.Margin = new Padding(0, 6, 0, 6);
            layout.Controls.Add(control, 1, row);
        }

        private static void ConfigureText(TextBox box)
        {
            box.Dock = DockStyle.Fill;
            box.Font = new Font("Segoe UI", 9.2f);
            box.BackColor = Color.White;
            box.ForeColor = Text;
            box.BorderStyle = BorderStyle.FixedSingle;
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
                Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
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
                ButtonDownBorderColor = Color.FromArgb(104, 79, 176)
            };
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var options = await _store.LoadAsync();
            if (options == null)
            {
                options = new HAgentStorageOptions
                {
                    StorageType = HAgentStorageType.File,
                    ApplicationName = _applicationName.Text,
                    RootPath = AppContext.BaseDirectory
                };
            }

            _loadedOptions = options;
            _loadingProfile = true;
            try
            {
                _storageType.SelectedItem = options.StorageType;
                _lastSelectedStorageType = options.StorageType;
                _applicationName.Text = options.ApplicationName ?? string.Empty;
                _rootPath.Text = string.IsNullOrWhiteSpace(options.RootPath) ? AppContext.BaseDirectory : options.RootPath;
            }
            finally
            {
                _loadingProfile = false;
            }

            await MigrateLegacyProfilesAsync();
            LoadSelectedProfileIntoControls();
        }

        private async System.Threading.Tasks.Task MigrateLegacyProfilesAsync()
        {
            if (_loadedOptions == null) return;

            foreach (var type in new[] { HAgentStorageType.SqlServer, HAgentStorageType.MySql })
            {
                var profile = _loadedOptions.GetDatabaseProfile(type);
                await MigrateLegacyPasswordAsync(profile, type);
            }
        }

        private void CaptureProfileFromControls(HAgentStorageType type)
        {
            if (_loadedOptions == null) return;

            _loadedOptions.ApplicationName = _applicationName.Text.Trim();
            _loadedOptions.RootPath = _rootPath.Text.Trim();

            if (type == HAgentStorageType.File) return;

            var profile = _loadedOptions.GetDatabaseProfile(type);
            profile.ServerName = _serverName.Text.Trim();
            profile.Port = ParsePortOrZero();
            profile.UserName = _userName.Text.Trim();
            profile.PasswordSecretId = GetPasswordSecretId(type);
        }

        private async System.Threading.Tasks.Task PersistProfileAsync(HAgentStorageType type)
        {
            if (_loadedOptions == null) return;

            if (type != HAgentStorageType.File)
            {
                var profile = _loadedOptions.GetDatabaseProfile(type);
                if (!string.IsNullOrWhiteSpace(profile.ServerName))
                {
                    var password = _password.Text;
                    if (!string.IsNullOrEmpty(password))
                        await _secrets.SetAsync(GetPasswordSecretId(type), password);

                    var originalStorageType = _loadedOptions.StorageType;
                    _loadedOptions.StorageType = type;
                    try
                    {
                        await _store.SaveAsync(_loadedOptions);
                    }
                    finally
                    {
                        _loadedOptions.StorageType = originalStorageType;
                    }

                    _password.Clear();
                }
            }
            else
            {
                await _store.SaveAsync(_loadedOptions);
            }
        }

        private void LoadSelectedProfileIntoControls()
        {
            if (_loadedOptions == null) return;

            var type = (HAgentStorageType)_storageType.SelectedItem;
            _loadingProfile = true;
            try
            {
                _applicationName.Text = _loadedOptions.ApplicationName ?? string.Empty;
                _rootPath.Text = string.IsNullOrWhiteSpace(_loadedOptions.RootPath) ? AppContext.BaseDirectory : _loadedOptions.RootPath;

                if (type == HAgentStorageType.File)
                {
                    _serverName.Clear();
                    _port.Clear();
                    _userName.Clear();
                    _password.Clear();
                }
                else
                {
                    var profile = _loadedOptions.GetDatabaseProfile(type);
                    _serverName.Text = profile.ServerName ?? string.Empty;
                    _port.Text = profile.GetEffectivePort(type).ToString();
                    _userName.Text = profile.UserName ?? string.Empty;
                    _password.Clear();
                    _status.Text = string.IsNullOrWhiteSpace(profile.ServerName)
                        ? "No saved connection profile exists for this database type yet."
                        : "Saved connection profile loaded. Password is retained securely and is not displayed.";
                    _status.ForeColor = Muted;
                }

                UpdateVisibility();
                UpdateDefaultPort(false);
                UpdateResolvedDatabase();
            }
            finally
            {
                _loadingProfile = false;
            }
        }

        private int ParsePortOrZero()
        {
            int port;
            if (string.IsNullOrWhiteSpace(_port.Text)) return 0;
            if (!int.TryParse(_port.Text, out port) || port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(_port), "Database port must be between 1 and 65535.");
            return port;
        }

        private static string GetPasswordSecretId(HAgentStorageType type)
        {
            switch (type)
            {
                case HAgentStorageType.SqlServer: return SqlServerPasswordSecretId;
                case HAgentStorageType.MySql: return MySqlPasswordSecretId;
                default: return string.Empty;
            }
        }

        private async System.Threading.Tasks.Task MigrateLegacyPasswordAsync(HAgentDatabaseStorageOptions profile, HAgentStorageType type)
        {
            if (profile == null || !string.IsNullOrWhiteSpace(profile.PasswordSecretId)) return;

            var oldSecret = await _secrets.GetAsync(LegacyCurrentDatabasePasswordSecretId);
            if (string.IsNullOrEmpty(oldSecret))
                oldSecret = await _secrets.GetAsync(LegacyDatabasePasswordSecretId);
            if (string.IsNullOrEmpty(oldSecret)) return;

            var newId = GetPasswordSecretId(type);
            await _secrets.SetAsync(newId, oldSecret);
            profile.PasswordSecretId = newId;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            try
            {
                if (_loadedOptions == null)
                    throw new InvalidOperationException("Storage settings have not been loaded.");

                var before = CloneForComparison(_loadedOptions);
                var selectedType = (HAgentStorageType)_storageType.SelectedItem;
                CaptureProfileFromControls(selectedType);
                _loadedOptions.StorageType = selectedType;
                _loadedOptions.ApplicationName = _applicationName.Text.Trim();
                _loadedOptions.RootPath = _rootPath.Text.Trim();

                if (selectedType != HAgentStorageType.File)
                {
                    var profile = _loadedOptions.GetDatabaseProfile(selectedType);
                    profile.PasswordSecretId = GetPasswordSecretId(selectedType);
                    await MigrateLegacyPasswordAsync(profile, selectedType);

                    if (string.IsNullOrEmpty(_password.Text))
                    {
                        var existing = await _secrets.GetAsync(profile.PasswordSecretId);
                        if (string.IsNullOrEmpty(existing))
                            throw new ArgumentException("Enter the database password before saving database storage settings.", nameof(_password));
                    }
                }

                await _store.SaveAsync(_loadedOptions);

                if (selectedType != HAgentStorageType.File && !string.IsNullOrEmpty(_password.Text))
                    await _secrets.SetAsync(GetPasswordSecretId(selectedType), _password.Text);

                _lastSelectedStorageType = selectedType;
                _password.Clear();
                _status.Text = "Settings saved. Storage changes can now be applied without restarting the application.";
                _status.ForeColor = Accent;
                RuntimeStorageChanged = HasActiveRuntimeChanges(before, _loadedOptions);
            }
            catch (Exception ex)
            {
                _status.Text = "Storage settings were not saved.";
                _status.ForeColor = Color.FromArgb(185, 28, 28);
                HMessage.ShowException(this, "The HAgent storage settings could not be saved.", "HAgent Storage", ex);
            }
        }

        private static HAgentStorageOptions CloneForComparison(HAgentStorageOptions source)
        {
            var copy = new HAgentStorageOptions
            {
                StorageType = source.StorageType,
                ApplicationName = source.ApplicationName,
                RootPath = source.RootPath,
                DatabaseName = source.DatabaseName,
                ServerName = source.ServerName,
                Port = source.Port,
                UserName = source.UserName,
                PasswordSecretId = source.PasswordSecretId,
                SqlServer = CloneProfile(source.SqlServer),
                MySql = CloneProfile(source.MySql)
            };
            return copy;
        }

        private static HAgentDatabaseStorageOptions CloneProfile(HAgentDatabaseStorageOptions source)
        {
            if (source == null) return new HAgentDatabaseStorageOptions();
            return new HAgentDatabaseStorageOptions
            {
                ServerName = source.ServerName,
                Port = source.Port,
                UserName = source.UserName,
                PasswordSecretId = source.PasswordSecretId
            };
        }

        private static bool HasActiveRuntimeChanges(HAgentStorageOptions before, HAgentStorageOptions after)
        {
            if (before == null || after == null) return before != after;
            if (before.StorageType != after.StorageType) return true;

            if (!string.Equals(before.ApplicationName, after.ApplicationName, StringComparison.Ordinal)) return true;

            if (after.StorageType == HAgentStorageType.File)
                return !string.Equals(before.RootPath, after.RootPath, StringComparison.Ordinal);

            return !ProfilesEqual(before.GetDatabaseProfile(after.StorageType), after.GetDatabaseProfile(after.StorageType));
        }

        private static bool ProfilesEqual(HAgentDatabaseStorageOptions left, HAgentDatabaseStorageOptions right)
        {
            if (left == null || right == null) return left == right;
            return string.Equals(left.ServerName, right.ServerName, StringComparison.Ordinal)
                && left.Port == right.Port
                && string.Equals(left.UserName, right.UserName, StringComparison.Ordinal)
                && string.Equals(left.PasswordSecretId, right.PasswordSecretId, StringComparison.Ordinal);
        }

        private void UpdateVisibility()
        {
            var isFile = _storageType.SelectedItem is HAgentStorageType type && type == HAgentStorageType.File;
            _rootPath.Enabled = isFile;
            _serverName.Enabled = !isFile;
            _port.Enabled = !isFile;
            _userName.Enabled = !isFile;
            _password.Enabled = !isFile;
        }

        private void UpdateDefaultPort(bool overwriteCurrent)
        {
            if (!(_storageType.SelectedItem is HAgentStorageType type) || type == HAgentStorageType.File)
            {
                if (overwriteCurrent) _port.Clear();
                _port.Enabled = false;
                return;
            }

            _port.Enabled = true;
            if (overwriteCurrent || string.IsNullOrWhiteSpace(_port.Text))
                _port.Text = (type == HAgentStorageType.MySql ? 3306 : 1433).ToString();
        }

        private void UpdateResolvedDatabase()
        {
            _resolvedDatabase.Text = HAgentStorageOptions.BuildDatabaseName(_applicationName.Text);
        }
    }
}