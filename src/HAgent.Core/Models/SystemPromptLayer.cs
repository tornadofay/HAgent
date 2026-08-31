using System;

namespace HAgent.Models
{
    /// <summary>
    /// One additive system-prompt layer. Layers are composed in priority order
    /// and none may replace or erase another layer.
    /// </summary>
    public sealed class SystemPromptLayer
    {
        public SystemPromptLayer()
        {
            Id = string.Empty;
            Name = string.Empty;
            Text = string.Empty;
        }

        public SystemPromptLayer(string id, string name, string text, int priority)
        {
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            Text = text ?? string.Empty;
            Priority = priority;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public int Priority { get; set; }
    }
}
