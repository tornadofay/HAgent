using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.Storage.File;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task RefreshAgentsAsync()
        {
            try
            {
                var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
                var agents = await store.GetAgentsAsync();
                var previousId = GetSelectedAgentId();
                _agents.Clear();
                _agents.AddRange(agents);

                _agentSelector.BeginUpdate();
                try
                {
                    _agentSelector.Items.Clear();
                    foreach (var agent in _agents)
                        _agentSelector.Items.Add(new AgentItem(agent));
                }
                finally
                {
                    _agentSelector.EndUpdate();
                }

                if (!string.IsNullOrWhiteSpace(previousId))
                    SelectAgent(previousId);
                if (_agentSelector.SelectedIndex < 0 && _agentSelector.Items.Count > 0)
                    _agentSelector.SelectedIndex = 0;

                await UpdateSelectedAgentAsync();
            }
            catch (Exception ex)
            {
                _globalStatus.Text = "Agent list could not be loaded";
                _globalStatus.ForeColor = Error;
                HMessage.ShowException(this, "The agent list could not be loaded.", "HAgent Example", ex);
            }
        }

        private string GetSelectedAgentId()
        {
            var item = _agentSelector.SelectedItem as AgentItem;
            return item == null ? string.Empty : item.Agent.Id;
        }

        private AiAgent GetSelectedAgent()
        {
            var item = _agentSelector.SelectedItem as AgentItem;
            return item == null ? null : item.Agent;
        }

        private void SelectAgent(string agentId)
        {
            for (var i = 0; i < _agentSelector.Items.Count; i++)
            {
                var item = _agentSelector.Items[i] as AgentItem;
                if (item != null && string.Equals(item.Agent.Id, agentId, StringComparison.OrdinalIgnoreCase))
                {
                    _agentSelector.SelectedIndex = i;
                    return;
                }
            }
        }

        private async Task UpdateSelectedAgentAsync()
        {
            var agent = GetSelectedAgent();
            if (agent == null)
            {
                _globalStatus.Text = "No agent selected";
                _globalStatus.ForeColor = Muted;
                ClearPromptPreview();
                return;
            }

            _globalStatus.Text = agent.Enabled ? "Selected: " + agent.Name : "Selected: " + agent.Name + " (disabled)";
            _globalStatus.ForeColor = agent.Enabled ? Muted : Error;

            try
            {
                var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
                var providers = await store.GetProvidersAsync();
                var providerIds = new List<string>();
                if (!string.IsNullOrWhiteSpace(agent.ProviderId))
                    providerIds.Add(agent.ProviderId);
                if (agent.ProviderIds != null)
                    providerIds.AddRange(agent.ProviderIds.Where(x => !string.IsNullOrWhiteSpace(x)));

                var provider = providerIds
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(id => providers.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)))
                    .FirstOrDefault(p => p != null);

                _providerPrompt.Text = provider == null ? "No configured provider." : (provider.DefaultSystemPrompt ?? string.Empty);
                _agentPrompt.Text = agent.SystemPrompt ?? string.Empty;

                if (provider == null)
                    _promptResolution.Text = "Provider prompt unavailable.";
                else if (agent.UseProviderSystemPrompt && !string.IsNullOrWhiteSpace(provider.DefaultSystemPrompt) && !string.IsNullOrWhiteSpace(agent.SystemPrompt))
                    _promptResolution.Text = "Provider + Agent prompts are used; agent inherits the provider prompt.";
                else if (agent.UseProviderSystemPrompt && !string.IsNullOrWhiteSpace(provider.DefaultSystemPrompt))
                    _promptResolution.Text = "Provider system prompt is used.";
                else if (!string.IsNullOrWhiteSpace(agent.SystemPrompt))
                    _promptResolution.Text = "Agent system prompt is used; provider prompt is not inherited.";
                else
                    _promptResolution.Text = "No system prompt is configured.";
            }
            catch
            {
                ClearPromptPreview();
            }
        }

        private void ClearPromptPreview()
        {
            _providerPrompt.Clear();
            _agentPrompt.Clear();
            _promptResolution.Text = "No prompt information available.";
        }
    }
}
