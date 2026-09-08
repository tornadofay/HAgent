using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.WinForms.Forms;
using HAgent.WinForms.Helpers;

namespace HAgent.WinForms.UI.Configuration.Tools
{
    internal sealed class ToolsPage : ConfigurationPageBase
    {
        private readonly ConfigurationContext _context;
        private readonly ListView _list = new ListView();

        public ToolsPage(ConfigurationContext context)
        {
            _context = context;
            Build();
        }

        public void RefreshData()
        {
            _list.Items.Clear();
            foreach (var tool in _context.Tools.GetDefinitions())
            {
                _list.Items.Add(new ListViewItem(new[]
                {
                    tool.Name,
                    tool.Category,
                    tool.IsBuiltIn ? "Predefined" : "Custom",
                    tool.Enabled ? "Enabled" : "Disabled",
                    tool.Description
                }) { Tag = tool });
            }
        }

        private void Build()
        {
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            root.Controls.Add(CreateHeader("Tools", "Predefined and custom capability definitions. The host application owns actual execution."));
            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, WrapContents = false, BackColor = Surface, Padding = new Padding(0, 2, 0, 0) };
            var add = CreateActionButton("+  Add custom tool", 158);
            var delete = CreateActionButton("Delete selected", 132, true);
            add.Click += async delegate { await EditAsync(null); };
            delete.Click += async delegate { await DeleteSelectedAsync(); };
            actions.Controls.Add(add);
            actions.Controls.Add(delete);

            ConfigureList(_list);
            _list.Columns.Add("Tool", 220);
            _list.Columns.Add("Category", 130);
            _list.Columns.Add("Kind", 110);
            _list.Columns.Add("Status", 90);
            _list.Columns.Add("Description", 420);
            _list.DoubleClick += async delegate
            {
                if (_list.SelectedItems.Count > 0) await EditAsync((AiTool)_list.SelectedItems[0].Tag);
            };
            root.Controls.Add(_list);
            root.Controls.Add(actions);
            Controls.Add(root);
        }

        private async Task EditAsync(AiTool existing)
        {
            if (existing != null && existing.IsBuiltIn)
            {
                HMessage.ShowInformation(FindForm(), "This is a predefined tool. Its definition is supplied by the host application and cannot be edited here.", "Tool");
                return;
            }
            var editor = new ToolEditorForm(existing == null ? new AiTool() : existing);
            if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;
            _context.Tools.Register(new DelegateAgentTool(editor.Tool, delegate(ToolExecutionContext context)
            {
                return Task.FromResult(ToolExecutionResult.Failure("This custom tool has a definition but no execution handler. Register an IAgentTool implementation in the host application."));
            }));
            RefreshData();
        }

        private async Task DeleteSelectedAsync()
        {
            if (_list.SelectedItems.Count == 0) return;
            var tool = _list.SelectedItems[0].Tag as AiTool;
            if (tool == null) return;
            if (tool.IsBuiltIn)
            {
                HMessage.ShowInformation(FindForm(), "Predefined tools are supplied by the host and cannot be deleted here.", "Tool");
                return;
            }
            if (HMessage.ShowDelete(FindForm(), "Delete tool '" + tool.Name + "'?", "Delete tool") != DialogResult.Yes) return;
            _context.Tools.Unregister(tool.Id);
            RefreshData();
            await Task.CompletedTask;
        }
    }
}
