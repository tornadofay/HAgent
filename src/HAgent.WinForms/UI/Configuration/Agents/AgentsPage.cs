using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.WinForms.Forms;
using HAgent.WinForms.Helpers;

namespace HAgent.WinForms.UI.Configuration.Agents
{
    internal sealed class AgentsPage : ConfigurationPageBase
    {
        private readonly ConfigurationContext _context;
        private readonly ListView _list = new ListView();

        public AgentsPage(ConfigurationContext context)
        {
            _context = context;
            Build();
        }

        public void RefreshData()
        {
            _list.Items.Clear();
            foreach (var agent in _context.Agents)
            {
                var providerId = agent.ExecutionSelection == null ? string.Empty : agent.ExecutionSelection.PreferredProviderId;
                var provider = _context.Providers.FirstOrDefault(x => string.Equals(x.Id, providerId, StringComparison.OrdinalIgnoreCase));
                var model = agent.ExecutionSelection == null ? string.Empty : agent.ExecutionSelection.PreferredLogicalModelId;
                var item = new ListViewItem(new[]
                {
                    agent.Name,
                    provider == null ? (string.IsNullOrWhiteSpace(providerId) ? "Auto" : "Missing provider") : provider.Name,
                    string.IsNullOrWhiteSpace(model) ? "Auto" : model,
                    agent.ToolIds == null ? "0" : agent.ToolIds.Count.ToString(),
                    agent.Enabled ? "Enabled" : "Disabled"
                }) { Tag = agent };
                _list.Items.Add(item);
            }
        }

        private void Build()
        {
            var root = CreateListPageRoot();
            root.Controls.Add(CreateListContent());
            root.Controls.Add(CreateActionBar());
            root.Controls.Add(CreateHeader("Agents", "Choose providers and models, then define each agent's behavior and runtime settings."));
            Controls.Add(root);
        }

        private Control CreateActionBar()
        {
            var actions = CreateActionPanel();
            var add = CreateActionButton("+  Add agent", 130);
            var delete = CreateActionButton("Delete selected", 132, true);
            add.Click += async delegate { await EditAsync(null); };
            delete.Click += async delegate { await DeleteSelectedAsync(); };
            actions.Controls.Add(add);
            actions.Controls.Add(delete);
            return actions;
        }

        private Control CreateListContent()
        {
            ConfigureList(_list);
            _list.Columns.Add("Agent", 220);
            _list.Columns.Add("Provider", 180);
            _list.Columns.Add("Model", 190);
            _list.Columns.Add("Tools", 70);
            _list.Columns.Add("Status", 90);
            _list.DoubleClick += async delegate
            {
                if (_list.SelectedItems.Count > 0) await EditAsync((AiAgent)_list.SelectedItems[0].Tag);
            };
            return _list;
        }

        private async Task EditAsync(AiAgent existing)
        {
            if (_context.Providers.Count == 0)
            {
                HMessage.ShowInformation(FindForm(), "Add a provider first. An agent needs at least one provider.", "HAgent");
                return;
            }
            var editor = new AgentEditorForm(existing == null ? new AiAgent() : existing, _context.Providers, _context.Secrets, _context.Adapters, _context.Tools.GetDefinitions());
            if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;
            await _context.Store.SaveAgentAsync(editor.Agent);
            _context.Agents = await _context.Store.GetAgentsAsync();
            RefreshData();
        }

        private async Task DeleteSelectedAsync()
        {
            if (_list.SelectedItems.Count == 0) return;
            var agent = _list.SelectedItems[0].Tag as AiAgent;
            if (agent == null) return;
            if (HMessage.ShowDelete(FindForm(), "Delete agent '" + agent.Name + "'?", "Delete agent") != DialogResult.Yes) return;
            await _context.Store.DeleteAgentAsync(agent.Id);
            _context.Agents = await _context.Store.GetAgentsAsync();
            RefreshData();
        }
    }
}
