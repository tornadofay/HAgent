using System;
using System.Collections.Generic;
using System.Linq;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class InMemoryToolRegistry : IToolRegistry
    {
        private readonly object _sync = new object();
        private readonly Dictionary<string, IAgentTool> _tools = new Dictionary<string, IAgentTool>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<AiTool> GetDefinitions()
        {
            lock (_sync)
                return _tools.Values.Select(x => x.Definition).ToList().AsReadOnly();
        }

        public bool TryGet(string toolId, out IAgentTool tool)
        {
            lock (_sync) return _tools.TryGetValue(toolId, out tool);
        }

        public void Register(IAgentTool tool)
        {
            if (tool == null) throw new ArgumentNullException(nameof(tool));
            if (tool.Definition == null || string.IsNullOrWhiteSpace(tool.Definition.Id))
                throw new ArgumentException("A tool must have a definition with an id.", nameof(tool));
            lock (_sync) _tools[tool.Definition.Id] = tool;
        }

        public void Unregister(string toolId)
        {
            if (string.IsNullOrWhiteSpace(toolId)) return;
            lock (_sync) _tools.Remove(toolId);
        }
    }

    public sealed class DelegateAgentTool : IAgentTool
    {
        private readonly ToolExecutionHandler _handler;
        public AiTool Definition { get; private set; }

        public DelegateAgentTool(AiTool definition, ToolExecutionHandler handler)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public System.Threading.Tasks.Task<ToolExecutionResult> ExecuteAsync(ToolExecutionContext context)
        {
            return _handler(context);
        }
    }
}
