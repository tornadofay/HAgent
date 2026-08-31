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
        private const string DatabasePasswordSecretId = "hagent-storage-database-password";
        private const string LegacyDatabasePasswordSecretId = "hagent.storage.database.password";

        private readonly FileHAgentStorageConfigurationStore _store;
        private readonly ISecretStore _secrets;
        private readonly ComboBox _storageType = new ComboBox();
        private readonly TextBox _applicationName = new TextBox();
        private readonly TextBox _rootPath = new TextBox();
        private readonly TextBox _databaseName = new TextBox();
        private readonly TextBox _serverName = new TextBox();
        private readonly TextBox _userName = new TextBox();
        private readonly TextBox _password = new TextBox();
        private readonly Label _resolvedDatabase = new Label();
        private readonly Label _status = new Label();
        private HAgentStorageOptions _loadedOptions;

        private static readonly Color Surface = Color.FromArgb(248, 248, 252);
        private static readonly Color Text = Color.FromArgb(68, 62, 88);
        private static readonly Color Muted = Color.FromArgb(100, 92, 120);
        private static readonly Color Accent = Color.FromArgb(116, 76, 210);

        public HAgentStorageSettingsForm(string basePath, string applicationName, ISecretStore secrets)
            : base("HAgent Storage", "HAgent-owned providers, agents, memory, tools, skills, wiki, and runtime storage", new Size(900, 640), new Size(760, 520))
        {
            if (secrets == null) throw new ArgumentNullException(nameof(secrets));

            var root = string.IsNullOrWhiteSpace(basePath) ? AppContext.BaseDirectory : basePath;
            var configurationPath = Path.Combine(root, "HAgentData", "configuration", "storage.json");
            _store = new FileHAgentStorageConfigurationStore(configurationPath);
            _secrets = secrets;

            BuildShell();
            _applicationName.Text = applicationName ?? string.Empty;
            _rootPath.Text = AppContext.BaseDirectory;
            _storageType.SelectedItem = HAgentStorageType.File;
            UpdateVisibility();
            Shown += async delegate { await LoadAsync(); };
        }

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
            _storageType.SelectedIndexChanged += delegate { UpdateVisibility(); UpdateResolvedDatabase(); };
            AddRow(layout, 0, "Storage type", _storageType);

            ConfigureText(_applicationName);
            _applicationName.TextChanged += delegate { UpdateResolvedDatabase(); };
            AddRow(layout, 1, "Host application name", _applicationName);

            ConfigureText(_rootPath);
            AddRow(layout, 2, "File root", _rootPath);

            ConfigureText(_databaseName);
            _databaseName.TextChanged += delegate { UpdateResolvedDatabase(); };
            AddRow(layout, 3, "Database name", _databaseName);

            ConfigureText(_serverName);
            AddRow(layout, 4, "Server name", _serverName);

            ConfigureText(_userName);
            AddRow(layout, 5, "User name", _userName);

            ConfigureText(_password);
            _password.UseSystemPasswordChar = true;
            AddRow(layout, 6, "Password", _password);

            _resolvedDatabase.Dock = DockStyle.Fill;
            _resolvedDatabase.ForeColor = Accent;
            _resolvedDatabase.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _resolvedDatabase.TextAlign = ContentAlignment.MiddleLeft;
            AddRow(layout, 7, "Effective database", _resolvedDatabase);

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
                _password.Clear();
                try { await _secrets.DeleteAsync(DatabasePasswordSecretId); } catch { }
                _status.Text = "Database password secret cleared.";
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
                _loadedOptions = null;
                UpdateResolvedDatabase();
                return;
            }

            _loadedOptions = options;
            _storageType.SelectedItem = options.StorageType;
            _applicationName.Text = options.ApplicationName;
            _rootPath.Text = string.IsNullOrWhiteSpace(options.RootPath) ? AppContext.BaseDirectory : options.RootPath;
            _databaseName.Text = options.DatabaseName ?? string.Empty;
            _serverName.Text = options.ServerName ?? string.Empty;
            _userName.Text = options.UserName ?? string.Empty;
            _password.Clear();
            UpdateVisibility();
            UpdateResolvedDatabase();

            if (string.Equals(options.PasswordSecretId, DatabasePasswordSecretId, StringComparison.Ordinal))
            {
                var existing = await _secrets.GetAsync(DatabasePasswordSecretId);
                _status.Text = string.IsNullOrWhiteSpace(existing)
                    ? "No database password secret is configured."
                    : "Database password is stored separately in the secret store.";
                _status.ForeColor = Muted;
            }
            else if (string.Equals(options.PasswordSecretId, LegacyDatabasePasswordSecretId, StringComparison.Ordinal))
            {
                _status.Text = "A previous storage setting used an invalid secret ID. Enter the password again and save to migrate it.";
                _status.ForeColor = Color.FromArgb(185, 28, 28);
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            try
            {
                var options = new HAgentStorageOptions
                {
                    StorageType = (HAgentStorageType)_storageType.SelectedItem,
                    ApplicationName = _applicationName.Text.Trim(),
                    RootPath = _rootPath.Text.Trim(),
                    DatabaseName = _databaseName.Text.Trim(),
                    ServerName = _serverName.Text.Trim(),
                    UserName = _userName.Text.Trim(),
                    PasswordSecretId = _storageType.SelectedItem is HAgentStorageType selectedType && selectedType != HAgentStorageType.File
                        ? DatabasePasswordSecretId
                        : string.Empty
                };
                options.Validate();

                if (options.StorageType != HAgentStorageType.File && string.IsNullOrEmpty(_password.Text))
                {
                    var existing = await _secrets.GetAsync(DatabasePasswordSecretId);
                    if (string.IsNullOrEmpty(existing))
                        throw new ArgumentException("Enter the database password before saving database storage settings.", nameof(_password));
                }

                var restartRequired = HasRestartRelevantChanges(options);

                await _store.SaveAsync(options);

                if (options.StorageType != HAgentStorageType.File && !string.IsNullOrEmpty(_password.Text))
                    await _secrets.SetAsync(DatabasePasswordSecretId, _password.Text);

                _loadedOptions = options;
                _password.Clear();
                _status.Text = options.StorageType == HAgentStorageType.File
                    ? "Settings saved."
                    : "Settings saved. Database password is stored separately in the secret store.";
                _status.ForeColor = Accent;

                if (restartRequired)
                {
                    HMessage.ShowInformation(
                        this,
                        "The storage configuration has changed. Restart the application for the new storage backend or connection settings to take effect.",
                        "HAgent Storage");
                }
            }
            catch (Exception ex)
            {
                _status.Text = "Storage settings were not saved.";
                _status.ForeColor = Color.FromArgb(185, 28, 28);
                HMessage.ShowException(this, "The HAgent storage settings could not be saved.", "HAgent Storage", ex);
            }
        }

        private bool HasRestartRelevantChanges(HAgentStorageOptions options)
        {
            if (_loadedOptions == null)
                return false;

            return _loadedOptions.StorageType != options.StorageType
                || !string.Equals(_loadedOptions.ApplicationName, options.ApplicationName, StringComparison.Ordinal)
                || !string.Equals(_loadedOptions.RootPath, options.RootPath, StringComparison.Ordinal)
                || !string.Equals(_loadedOptions.DatabaseName, options.DatabaseName, StringComparison.Ordinal)
                || !string.Equals(_loadedOptions.ServerName, options.ServerName, StringComparison.Ordinal)
                || !string.Equals(_loadedOptions.UserName, options.UserName, StringComparison.Ordinal)
                || !string.Equals(_loadedOptions.PasswordSecretId, options.PasswordSecretId, StringComparison.Ordinal);
        }

        private void UpdateVisibility()
        {
            var isFile = _storageType.SelectedItem is HAgentStorageType type && type == HAgentStorageType.File;
            _rootPath.Enabled = isFile;
            _serverName.Enabled = !isFile;
            _userName.Enabled = !isFile;
            _password.Enabled = !isFile;
            _databaseName.Enabled = !isFile;
        }

        private void UpdateResolvedDatabase()
        {
            var name = string.IsNullOrWhiteSpace(_databaseName.Text)
                ? HAgentStorageOptions.BuildDatabaseName(_applicationName.Text)
                : _databaseName.Text.Trim();
            _resolvedDatabase.Text = name;
        }
    }
}
