using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Runtime;
using HAgent.Providers.OpenAICompatible;
using HAgent.Storage.File;
using HAgent.WinForms;

namespace HAgent.Sample
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly string _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HAgent");
        private readonly Label _result = new Label();

        public MainForm()
        {
            Text = "HAgent Sample"; Size = new System.Drawing.Size(760, 430); StartPosition = FormStartPosition.CenterScreen; Font = new System.Drawing.Font("Segoe UI", 10f);
            var settings = new Button { Text = "Open AI Settings", Left = 32, Top = 32, Width = 180, Height = 42 };
            settings.Click += (_,__) => AISettings.ShowMainAISettingsForm(this);
            var send = new Button { Text = "Send test message", Left = 230, Top = 32, Width = 180, Height = 42 };
            send.Click += async (_,__) => await SendAsync();
            _result.Text = "Configure a provider and an agent, then send a test message."; _result.Left = 32; _result.Top = 100; _result.Width = 670; _result.Height = 240; _result.AutoEllipsis = true;
            Controls.Add(settings); Controls.Add(send); Controls.Add(_result);
        }

        private async Task SendAsync()
        {
            try
            {
                var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
                var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
                var agents = await store.GetAgentsAsync();
                var agent = agents.FirstOrDefault(a => a.Enabled);
                if (agent == null) { _result.Text = "No enabled agent exists."; return; }
                var client = new HAgentClient(store, secrets, new[] { new OpenAICompatibleProviderAdapter() });
                _result.Text = "Sending...";
                var response = await client.SendAsync(agent.Id, "Reply with a concise confirmation that HAgent is connected.");
                _result.Text = response.Text;
            }
            catch (Exception ex) { _result.Text = ex.Message; }
        }
    }
}
