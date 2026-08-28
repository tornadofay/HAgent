using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.WinForms.Forms;

namespace HAgent.WinForms
{
    public static class AISettings
    {
        public static void ShowMainAISettingsForm(IWin32Window owner = null)
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HAgent");
            Directory.CreateDirectory(basePath);
            var store = new HAgent.Storage.File.FileAiStore(Path.Combine(basePath, "settings.json"));
            var secrets = new HAgent.Storage.File.ProtectedDataSecretStore(Path.Combine(basePath, "secrets"));
            ShowMainAISettingsForm(store, secrets, owner);
        }

        public static void ShowMainAISettingsForm(IAiStore store, ISecretStore secrets, IWin32Window owner = null, IEnumerable<IAiProviderAdapter> adapters = null)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (secrets == null) throw new ArgumentNullException(nameof(secrets));
            using (var form = new AISettingsForm(store, secrets, adapters))
                form.ShowDialog(owner);
        }
    }
}
