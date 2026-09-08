using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Providers.OpenAICompatible;
using HAgent.Runtime;
using HAgent.WinForms.Forms;

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
            var adapters = new List<IAiProviderAdapter> { new OpenAICompatibleProviderAdapter() };
            var tools = new InMemoryToolRegistry();
            tools.Register(new DelegateAgentTool(
                new AiTool
                {
                    Id = "built-in-app-info",
                    Name = "application_info",
                    Description = "Returns basic information supplied by the host application.",
                    Category = "System",
                    IsBuiltIn = true,
                    InputSchemaJson = "{\"type\":\"object\",\"properties\":{}}"
                },
                delegate(ToolExecutionContext context)
                {
                    return Task.FromResult(ToolExecutionResult.Success("HAgent WinForms development host"));
                }));

            Application.Run(new AISettingsForm(store, secrets, adapters, tools));
        }
    }
}
