using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Storage.File;
using HAgent.Storage.MySql;
using HAgent.Storage.SqlServer;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.Forms
{
    public sealed class HAgentStorageConnectionTestForm : HAgentForm
    {
        private readonly FileHAgentStorageConfigurationStore _store;
        private readonly ISecretStore _secrets;
        private readonly Label _backend = new Label();
        private readonly Label _server = new Label();
        private readonly Label _port = new Label();
        private readonly Label _user = new Label();
        private readonly Label _database = new Label();
        private readonly Label _status = new Label();
        private readonly HButton _test = CreateButton("Test connection", 150);

        private HAgentStorageOptions _options;

        private static readonly Color Surface = Color.FromArgb(248, 248, 252);
        private static readonly Color Text = Color.FromArgb(68, 62, 88);
        private static readonly Color Muted = Color.FromArgb(100, 92, 120);
        private static readonly Color Accent = Color.FromArgb(116, 76, 210);

        public HAgentStorageConnectionTestForm(string basePath, ISecretStore secrets)
            : base("Storage Connection Test", "Non-destructive validation of the configured HAgent storage endpoint", new Size(760, 500), new Size(650, 420))
        {
            if (secrets == null) throw new ArgumentNullException(nameof(secrets));

            var root = string.IsNullOrWhiteSpace(basePath) ? AppContext.BaseDirectory : basePath;
            _store = new FileHAgentStorageConfigurationStore(Path.Combine(root, "HAgentData", "configuration", "storage.json"));
            _secrets = secrets;

            BuildShell();
            Shown += async delegate { await LoadAsync(); };
        }

        private void BuildShell()
        {
            BodyPanel.Padding = new Padding(26);
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                BackColor = Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddValueRow(layout, 0, "Storage backend", _backend);
            AddValueRow(layout, 1, "Server name", _server);
            AddValueRow(layout, 2, "Port", _port);
            AddValueRow(layout, 3, "User name", _user);
            AddValueRow(layout, 4, "HAgent database", _database);

            _status.Dock = DockStyle.Fill;
            _status.ForeColor = Muted;
            _status.Font = new Font("Segoe UI", 9.2f);
            _status.AutoEllipsis = true;
            AddValueRow(layout, 5, "Status", _status);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Surface,
                Padding = new Padding(0, 8, 0, 0)
            };
            _test.Click += async delegate { await TestAsync(); };
            actions.Controls.Add(_test);
            layout.Controls.Add(actions, 1, 6);

            BodyPanel.Controls.Add(layout);
        }

        private static void AddValueRow(TableLayoutPanel layout, int row, string caption, Label value)
        {
            layout.Controls.Add(new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            }, 0, row);

            value.Dock = DockStyle.Fill;
            value.TextAlign = ContentAlignment.MiddleLeft;
            value.ForeColor = Text;
            value.Font = new Font("Segoe UI", 9.2f);
            layout.Controls.Add(value, 1, row);
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
            _options = await _store.LoadAsync();
            if (_options == null)
            {
                _status.Text = "No HAgent storage configuration has been saved yet.";
                _status.ForeColor = Color.FromArgb(185, 28, 28);
                _test.Enabled = false;
                return;
            }

            _backend.Text = _options.StorageType.ToString();
            _database.Text = _options.GetEffectiveDatabaseName();

            if (_options.StorageType == HAgentStorageType.File)
            {
                _server.Text = "Local file system";
                _port.Text = "N/A";
                _user.Text = "N/A";
                _status.Text = "File storage does not require a database connection.";
                _test.Text = "Verify file storage";
                return;
            }

            var profile = _options.GetDatabaseProfile(_options.StorageType);
            _server.Text = profile == null ? string.Empty : profile.ServerName ?? string.Empty;
            _port.Text = profile == null ? string.Empty : profile.GetEffectivePort(_options.StorageType).ToString();
            _user.Text = profile == null ? string.Empty : profile.UserName ?? string.Empty;
            _status.Text = "Ready to test the saved endpoint. Password is retrieved only for the connection attempt.";
        }

        private async System.Threading.Tasks.Task TestAsync()
        {
            try
            {
                if (_options == null)
                    throw new InvalidOperationException("HAgent storage configuration has not been loaded.");

                if (_options.StorageType == HAgentStorageType.File)
                {
                    var root = _options.GetEffectiveRootPath();
                    Directory.CreateDirectory(root);
                    _status.Text = "File storage path is available: " + root;
                    _status.ForeColor = Accent;
                    return;
                }

                var profile = _options.GetDatabaseProfile(_options.StorageType);
                if (profile == null || string.IsNullOrWhiteSpace(profile.ServerName))
                    throw new ArgumentException("Server name is required for the selected database storage backend.");

                var port = profile.GetEffectivePort(_options.StorageType);
                if (port < 1 || port > 65535)
                    throw new ArgumentOutOfRangeException(nameof(port));

                var secretId = profile.PasswordSecretId;
                var password = string.IsNullOrWhiteSpace(secretId)
                    ? string.Empty
                    : await _secrets.GetAsync(secretId).ConfigureAwait(true);

                if (_options.StorageType == HAgentStorageType.SqlServer)
                {
                    await SqlServerHAgentStorageBootstrapper.TestConnectionAsync(profile.ServerName, port, profile.UserName, password).ConfigureAwait(true);
                }
                else if (_options.StorageType == HAgentStorageType.MySql)
                {
                    await MySqlHAgentStorageBootstrapper.TestConnectionAsync(profile.ServerName, port, profile.UserName, password).ConfigureAwait(true);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported HAgent storage backend: " + _options.StorageType);
                }

                _status.Text = "Connection succeeded. No HAgent database or schema changes were made.";
                _status.ForeColor = Accent;
            }
            catch (Exception ex)
            {
                _status.Text = "Connection test failed: " + ex.Message;
                _status.ForeColor = Color.FromArgb(185, 28, 28);
                HMessage.ShowException(this, "The HAgent storage connection test failed.", "HAgent Storage", ex);
            }
        }
    }
}
