using System;
using System.Collections.Generic;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.Providers.OpenAICompatible;

namespace HAgent.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var basePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HAgent");
            System.IO.Directory.CreateDirectory(basePath);

            IAiStore store = new HAgent.Storage.File.FileAiStore(
                System.IO.Path.Combine(basePath, "settings.json"));
            ISecretStore secrets = new HAgent.Storage.File.ProtectedDataSecretStore(
                System.IO.Path.Combine(basePath, "secrets"));

            var adapters = new List<IAiProviderAdapter>
            {
                new OpenAICompatibleProviderAdapter()
            };

            Application.Run(new Forms.AISettingsForm(store, secrets, adapters));
        }
    }
}
