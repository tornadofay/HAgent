using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using System;
using System.Collections.Generic;

namespace HAgent.WinForms
{
    internal sealed class PersistentToolRegistry : IToolRegistry
    {
        private readonly IToolStore _store;
        private readonly InMemoryToolRegistry _inner = new InMemoryToolRegistry();
        private readonly object _sync = new object();

        public PersistentToolRegistry(IToolStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            Load();
        }

        public IReadOnlyList<AiTool> GetDefinitions()
        {
            return _inner.GetDefinitions();
        }

        public bool TryGet(string toolId, out IAgentTool tool)
        {
            return _inner.TryGet(toolId, out tool);
        }

        public void Register(IAgentTool tool)
        {
            if (tool == null) throw new ArgumentNullException(nameof(tool));
            lock (_sync)
            {
                _inner.Register(tool);
                PersistDefinition(tool.Definition);
            }
        }

        public void Unregister(string toolId)
        {
            if (string.IsNullOrWhiteSpace(toolId)) return;
            lock (_sync)
            {
                _inner.Unregister(toolId);
                _store.DeleteToolAsync(toolId, default(System.Threading.CancellationToken)).GetAwaiter().GetResult();
            }
        }

        private void Load()
        {
            var tools = _store.GetToolsAsync(default(System.Threading.CancellationToken)).GetAwaiter().GetResult();
            foreach (var tool in tools ?? new List<AiTool>())
            {
                if (tool == null || string.IsNullOrWhiteSpace(tool.Id)) continue;
                _inner.Register(CreateDefinitionOnlyTool(tool));
            }
        }

        private void PersistDefinition(AiTool definition)
        {
            _store.SaveToolAsync(Clone(definition), default(System.Threading.CancellationToken)).GetAwaiter().GetResult();
        }

        private static IAgentTool CreateDefinitionOnlyTool(AiTool definition)
        {
            var copy = Clone(definition);
            return new DelegateAgentTool(copy, delegate(ToolExecutionContext context)
            {
                return System.Threading.Tasks.Task.FromResult(
                    ToolExecutionResult.Failure(
                        "This tool has a persisted definition but no executable handler is registered by the host application."));
            });
        }

        private static AiTool Clone(AiTool source)
        {
            return new AiTool
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
                InputSchemaJson = source.InputSchemaJson,
                Category = source.Category,
                Type = source.Type,
                IsBuiltIn = source.IsBuiltIn,
                Enabled = source.Enabled
            };
        }
    }
}
