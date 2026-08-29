using HAgent.Abstractions;
using HAgent.Runtime;
using HAgent.WinForms.Forms;
using HAgent.WinForms.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace HAgent.WinForms
{
    public static class AISettings
    {
        private const string RepositoryUrl = "https://github.com/tornadofay/HAgent";

        public static void ShowMainAISettingsForm(IWin32Window owner = null)
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HAgent");
            Directory.CreateDirectory(basePath);
            var store = new HAgent.Storage.File.FileAiStore(Path.Combine(basePath, "settings.json"));
            var toolStore = new HAgent.Storage.File.FileToolStore(Path.Combine(basePath, "tool-definitions", "tools.json"));
            var secrets = new HAgent.Storage.File.ProtectedDataSecretStore(Path.Combine(basePath, "secrets"));
            ShowMainAISettingsForm(store, secrets, owner, null, toolStore);
        }

        public static UiAutomationPermissions LoadUiPermissions()
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HAgent");
            Directory.CreateDirectory(basePath);
            return new HAgent.Storage.File.UiPermissionStore(Path.Combine(basePath, "ui-permissions.json")).Load();
        }

        public static void SaveUiPermissions(UiAutomationPermissions permissions)
        {
            if (permissions == null) throw new ArgumentNullException(nameof(permissions));
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HAgent");
            Directory.CreateDirectory(basePath);
            new HAgent.Storage.File.UiPermissionStore(Path.Combine(basePath, "ui-permissions.json")).Save(permissions);
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
                AttachRepositoryLink(form);
                form.ShowDialog(owner);
            }
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
