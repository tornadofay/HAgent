using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Composes system-prompt layers without replacement semantics.
    /// Lower layers are additive and must not be treated as authorization overrides.
    /// </summary>
    public static class SystemPromptComposer
    {
        public static string Compose(IEnumerable<SystemPromptLayer> layers)
        {
            if (layers == null) return string.Empty;

            var ordered = layers
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Text))
                .OrderBy(x => x.Priority)
                .ToList();

            if (ordered.Count == 0) return string.Empty;

            var builder = new StringBuilder();
            foreach (var layer in ordered)
            {
                if (builder.Length > 0)
                    builder.AppendLine().AppendLine();

                var title = string.IsNullOrWhiteSpace(layer.Name)
                    ? layer.Id
                    : layer.Name;

                if (!string.IsNullOrWhiteSpace(title))
                {
                    builder.Append("[System Prompt Layer: ")
                        .Append(title.Trim())
                        .AppendLine("]");
                }

                builder.Append(layer.Text.Trim());
            }

            return builder.ToString();
        }

        public static SystemPromptLayer Create(string id, string name, string text, int priority)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Prompt layer text is required.", nameof(text));

            return new SystemPromptLayer(id, name, text, priority);
        }
    }
}
