using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.WinForms.Forms;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;
using HAgent.WinForms.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HAgent.WinForms
{
    public static class AISettings
    {
        private const string RepositoryUrl = "https://github.com/tornadofay/HAgent";

        public static void ShowMainAISettingsForm(IWin32Window owner = null)
        {
            var basePath = GetDefaultHAgentRootPath();
            Directory.CreateDirectory(basePath);
            var store = new HAgent.Storage.File.FileAiStore(Path.Combine(basePath, "configuration", "settings.json"));
            var toolStore = new HAgent.Storage.File.FileToolStore(Path.Combine(basePath, "configuration", "tools", "tools.json"));
            var secrets = new HAgent.Storage.File.ProtectedDataSecretStore(Path.Combine(basePath, "secrets"));
            ShowMainAISettingsForm(store, secrets, owner, null, toolStore);
        }

        public static UiAutomationPermissions LoadUiPermissions()
        {
            var basePath = GetDefaultHAgentRootPath();
            Directory.CreateDirectory(basePath);
            return new UiPermissionStore(Path.Combine(basePath, "configuration", "ui-permissions.json")).Load();
        }

        public static void SaveUiPermissions(UiAutomationPermissions permissions)
        {
            if (permissions == null) throw new ArgumentNullException(nameof(permissions));
            var basePath = GetDefaultHAgentRootPath();
            Directory.CreateDirectory(basePath);
            new UiPermissionStore(Path.Combine(basePath, "configuration", "ui-permissions.json")).Save(permissions);
        }

        public static void ShowMainAISettingsForm(
            IAiStore store,
            ISecretStore secrets,
            IWin32Window owner = null,
            IEnumerable<IAiProviderAdapter> adapters = null,
            IToolStore toolStore = null)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (secrets == null) throw new ArgumentNullException(nameof(secrets));

            IToolRegistry tools = toolStore == null
                ? new InMemoryToolRegistry()
                : new PersistentToolRegistry(toolStore);

            using (var form = new AISettingsForm(store, secrets, adapters, tools))
            {
                AttachPermissionsNavigation(form);
                AttachStorageNavigation(form, secrets);
                AttachStorageConnectionTestNavigation(form, secrets);
                AttachRepositoryLink(form);
                form.ShowDialog(owner);
            }
        }

        private static string GetDefaultHAgentRootPath()
        {
            var applicationName = Process.GetCurrentProcess().ProcessName;
            return new HAgentStorageOptions
            {
                ApplicationName = string.IsNullOrWhiteSpace(applicationName) ? "HAgent" : applicationName,
                RootPath = AppContext.BaseDirectory
            }.GetEffectiveRootPath();
        }

        private static void AttachPermissionsNavigation(Form form)
        {
            form.Shown += delegate
            {
                var nav = FindControl(form, c => c is FlowLayoutPanel && c.Width == 188 && c.BackColor == Color.FromArgb(31, 24, 69)) as FlowLayoutPanel;
                if (nav == null || FindControl(nav, c => c is HButton && string.Equals(c.Text, "Permissions", StringComparison.Ordinal)) != null)
                    return;

                var button = CreateNavigationButton("Permissions");
                button.Click += delegate
                {
                    using (var permissionsForm = new UiPermissionsForm(LoadUiPermissions()))
                    {
                        permissionsForm.ShowDialog(form);
                    }
                };
                InsertBeforeAbout(nav, button);
            };
        }

        private static void AttachStorageNavigation(Form form, ISecretStore secrets)
        {
            form.Shown += delegate
            {
                var nav = FindControl(form, c => c is FlowLayoutPanel && c.Width == 188 && c.BackColor == Color.FromArgb(31, 24, 69)) as FlowLayoutPanel;
                if (nav == null || FindControl(nav, c => c is HButton && string.Equals(c.Text, "Storage", StringComparison.Ordinal)) != null)
                    return;

                var button = CreateNavigationButton("Storage");
                button.Click += async delegate
                {
                    using (var storageForm = new HAgentStorageSettingsForm(AppContext.BaseDirectory, Process.GetCurrentProcess().ProcessName, secrets))
                    {
                        storageForm.ShowDialog(form);
                        if (storageForm.RuntimeStorageChanged && !form.IsDisposed)
                            form.Close();
                    }
                };
                InsertBeforeAbout(nav, button);
            };
        }

        private static void AttachStorageConnectionTestNavigation(Form form, ISecretStore secrets)
        {
            form.Shown += delegate
            {
                var nav = FindControl(form, c => c is FlowLayoutPanel && c.Width == 188 && c.BackColor == Color.FromArgb(31, 24, 69)) as FlowLayoutPanel;
                if (nav == null || FindControl(nav, c => c is HButton && string.Equals(c.Text, "Storage Test", StringComparison.Ordinal)) != null)
                    return;

                var button = CreateNavigationButton("Storage Test");
                button.Click += delegate
                {
                    using (var testForm = new HAgentStorageConnectionTestForm(AppContext.BaseDirectory, secrets))
                    {
                        testForm.ShowDialog(form);
                    }
                };
                InsertBeforeAbout(nav, button);
            };
        }

        private static HButton CreateNavigationButton(string text)
        {
            return new HButton
            {
                Text = text,
                Width = 166,
                Height = 42,
                RoundButton = true,
                Edge = 10,
                TextAlign = ContentAlignment.MiddleLeft,
                TextMargin = 16,
                Margin = new Padding(0, 0, 0, 6),
                Cursor = Cursors.Hand,
                ButtonLeaveBackGroundColor1 = Color.FromArgb(31, 24, 69),
                ButtonLeaveBackGroundColor2 = Color.FromArgb(25, 20, 54),
                ButtonLeaveForeColor = Color.FromArgb(239, 234, 250),
                ButtonLeaveBorderColor = Color.FromArgb(55, 45, 94),
                ButtonEnterBackGroundColor1 = Color.FromArgb(76, 54, 132),
                ButtonEnterBackGroundColor2 = Color.FromArgb(55, 39, 100),
                ButtonEnterForeColor = Color.White,
                ButtonEnterBorderColor = Color.FromArgb(116, 76, 210),
                ButtonDownBackGroundColor1 = Color.FromArgb(61, 43, 110),
                ButtonDownBackGroundColor2 = Color.FromArgb(42, 29, 78),
                ButtonDownForeColor = Color.White,
                ButtonDownBorderColor = Color.FromArgb(104, 76, 170),
                Font = new Font("Segoe UI", 9.5f)
            };
        }

        private static void InsertBeforeAbout(FlowLayoutPanel nav, Control button)
        {
            var aboutIndex = nav.Controls.Count;
            for (var i = 0; i < nav.Controls.Count; i++)
            {
                if (string.Equals(nav.Controls[i].Text, "About", StringComparison.Ordinal))
                {
                    aboutIndex = i;
                    break;
                }
            }
            nav.Controls.Add(button);
            if (aboutIndex < nav.Controls.Count - 1)
                nav.Controls.SetChildIndex(button, aboutIndex);
        }

        private static void AttachRepositoryLink(Form form)
        {
            form.Shown += delegate
            {
                var aboutTitle = FindControl(form, c => c is Label && string.Equals(c.Text, "About HAgent", StringComparison.Ordinal));
                if (aboutTitle == null || aboutTitle.Parent == null) return;

                var page = aboutTitle.Parent.Parent;
                if (page == null) return;
                if (FindControl(page, c => c is LinkLabel && string.Equals(c.Tag as string, RepositoryUrl, StringComparison.Ordinal)) != null)
                    return;

                var link = new LinkLabel
                {
                    Text = "GitHub repository",
                    Tag = RepositoryUrl,
                    AutoSize = true,
                    LinkColor = Color.FromArgb(95, 65, 190),
                    ActiveLinkColor = Color.FromArgb(126, 94, 214),
                    VisitedLinkColor = Color.FromArgb(95, 65, 190),
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Location = new Point(2, 220),
                    Cursor = Cursors.Hand
                };
                link.LinkClicked += delegate
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = RepositoryUrl,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        HMessage.ShowException(form, "The GitHub repository could not be opened.", "HAgent", ex);
                    }
                };
                page.Controls.Add(link);
            };
        }

        private static Control FindControl(Control root, Func<Control, bool> predicate)
        {
            if (root == null || predicate == null) return null;
            foreach (Control control in root.Controls)
            {
                if (predicate(control)) return control;
                var found = FindControl(control, predicate);
                if (found != null) return found;
            }
            return null;
        }
    }
}
