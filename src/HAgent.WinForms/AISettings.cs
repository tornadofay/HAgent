using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.WinForms.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace HAgent.WinForms
{
    public static class AISettings
    {
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

            using (var form = new Forms.AISettingsForm(store, secrets, adapters, tools))
                form.ShowDialog(owner);
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
    }
}
