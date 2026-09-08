using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.WinForms.Forms;
using HAgent.WinForms.Helpers;

namespace HAgent.WinForms.UI.Configuration.Providers
{
    internal sealed class ProvidersPage : ConfigurationPageBase
    {
        private readonly ConfigurationContext _context;
        private readonly ListView _list = new ListView();

        public ProvidersPage(ConfigurationContext context)
        {
            _context = context;
            Build();
        }

        public void RefreshData()
        {
            _list.Items.Clear();
            foreach (var provider in _context.Providers)
            {
                var used = _context.Agents.Where(a => UsesProvider(a, provider.Id)).Select(a => a.Name).ToArray();
                var item = new ListViewItem(new[]
                {
                    provider.Name,
                    provider.Kind,
                    provider.DefaultModel,
                    used.Length.ToString(),
                    string.Join(", ", used),
                    provider.Enabled ? "Enabled" : "Disabled"
                }) { Tag = provider };
                _list.Items.Add(item);
            }
        }

        private void Build()
        {
            var root = CreateListPageRoot();
            root.Controls.Add(CreateListContent());
            root.Controls.Add(CreateActionBar());
            root.Controls.Add(CreateHeader("Providers", "Connection, authentication, model defaults, and shared provider settings."));
            Controls.Add(root);
        }

        private Control CreateActionBar()
        {
            var actions = CreateActionPanel();
            var add = CreateActionButton("+  Add provider", 148);
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
            _list.Columns.Add("Provider", 190);
            _list.Columns.Add("Type", 145);
            _list.Columns.Add("Model", 180);
            _list.Columns.Add("Agents", 70);
            _list.Columns.Add("Used by", 260);
            _list.Columns.Add("Status", 85);
            _list.DoubleClick += async delegate
            {
                if (_list.SelectedItems.Count > 0) await EditAsync((AiProvider)_list.SelectedItems[0].Tag);
            };
            return _list;
        }

        private async Task EditAsync(AiProvider existing)
        {
            var editor = new ProviderEditorForm(existing == null ? new AiProvider() : existing, _context.Secrets, _context.Adapters);
            if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;
            await _context.Store.SaveProviderAsync(editor.Provider);
            _context.Providers = await _context.Store.GetProvidersAsync();
            RefreshData();
        }

        private async Task DeleteSelectedAsync()
        {
            if (_list.SelectedItems.Count == 0) return;
            var provider = _list.SelectedItems[0].Tag as AiProvider;
            if (provider == null) return;
            var users = _context.Agents.Where(a => UsesProvider(a, provider.Id)).Select(a => a.Name).ToArray();
            if (users.Length > 0)
            {
                HMessage.ShowError(FindForm(), "This provider cannot be deleted because these agents still use it:\r\n\r\n" + string.Join(", ", users), "Provider in use");
                return;
            }
            if (HMessage.ShowDelete(FindForm(), "Delete provider '" + provider.Name + "'?", "Delete provider") != DialogResult.Yes) return;
            await _context.Store.DeleteProviderAsync(provider.Id);
            _context.Providers = await _context.Store.GetProvidersAsync();
            RefreshData();
        }

        private static bool UsesProvider(AiAgent agent, string providerId)
        {
            if (agent == null || string.IsNullOrWhiteSpace(providerId) || agent.ExecutionSelection == null) return false;
            var selection = agent.ExecutionSelection;
            if (string.Equals(selection.PreferredProviderId, providerId, StringComparison.OrdinalIgnoreCase)) return true;
            var target = selection.PreferredTargetId ?? string.Empty;
            var separator = target.IndexOf("::", StringComparison.Ordinal);
            return separator > 0 && string.Equals(target.Substring(0, separator), providerId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
