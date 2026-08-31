using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Read-only trusted tool for inspecting bounded HAgent-owned memory for one explicit scope and owner.
    /// </summary>
    public sealed class HAgentInternalMemoryTool : IAgentTool
    {
        private const int DefaultMaxItems = 20;
        private const int MaximumMaxItems = 50;
        private const int MaximumContentLength = 4000;

        private readonly IMemoryStore _memoryStore;

        public HAgentInternalMemoryTool(IMemoryStore memoryStore)
        {
            if (memoryStore == null) throw new ArgumentNullException(nameof(memoryStore));

            _memoryStore = memoryStore;
            Definition = CreateDefinition();
        }

        public AiTool Definition { get; private set; }

        public async Task<ToolExecutionResult> ExecuteAsync(ToolExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.CancellationToken.ThrowIfCancellationRequested();

            var arguments = context.Arguments;
            var scope = ResolveRequiredString(arguments, "scope");
            var ownerId = ResolveRequiredString(arguments, "ownerId");
            var maxItems = ResolveMaxItems(arguments);
            var text = ResolveOptionalString(arguments, "text");
            var taskId = ResolveOptionalString(arguments, "taskId");
            var kind = ResolveOptionalKind(arguments);

            MemoryScope parsedScope;
            if (!Enum.TryParse(scope, true, out parsedScope))
                throw new ArgumentException("scope must be one of: Session, Task, Agent, User, Application, Shared.", nameof(arguments));

            var query = new MemoryQuery
            {
                Scope = parsedScope,
                OwnerId = ownerId,
                TaskId = taskId,
                Text = text,
                Kind = kind,
                MaxResults = maxItems
            };

            var entries = await _memoryStore.SearchAsync(query, context.CancellationToken).ConfigureAwait(false);

            var output = new StringBuilder();
            output.AppendLine("HAgent internal memory");
            output.AppendLine("Scope: " + parsedScope);
            output.AppendLine("Owner: " + Safe(ownerId));
            output.AppendLine("Returned: " + entries.Count);
            output.AppendLine("Max items: " + maxItems);

            foreach (var entry in entries)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                output.AppendLine("Memory | " + Safe(entry.Id) +
                                  " | Kind=" + entry.Kind +
                                  " | Task=" + Safe(entry.TaskId) +
                                  " | Created=" + entry.CreatedAt.ToString("O") +
                                  " | Occurred=" + entry.OccurredAt.ToString("O"));
                output.AppendLine("Content: " + SafeContent(entry.Content));
                AppendSafeMetadata(output, entry.Metadata);
            }

            return ToolExecutionResult.Success(output.ToString().TrimEnd());
        }

        private static MemoryKind? ResolveOptionalKind(IReadOnlyDictionary<string, object> arguments)
        {
            var value = ResolveOptionalString(arguments, "kind");
            if (string.IsNullOrWhiteSpace(value)) return null;

            MemoryKind kind;
            if (!Enum.TryParse(value, true, out kind))
                throw new ArgumentException("kind must be one of: Fact, Preference, Task, Event.", nameof(arguments));
            return kind;
        }

        private static string ResolveRequiredString(IReadOnlyDictionary<string, object> arguments, string name)
        {
            var value = ResolveOptionalString(arguments, name);
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", nameof(arguments));
            return value.Trim();
        }

        private static string ResolveOptionalString(IReadOnlyDictionary<string, object> arguments, string name)
        {
            object value;
            if (arguments == null || !arguments.TryGetValue(name, out value) || value == null)
                return string.Empty;
            return Convert.ToString(value) ?? string.Empty;
        }

        private static int ResolveMaxItems(IReadOnlyDictionary<string, object> arguments)
        {
            object rawValue;
            if (arguments == null || !arguments.TryGetValue("maxItems", out rawValue) || rawValue == null)
                return DefaultMaxItems;

            int value;
            try
            {
                value = Convert.ToInt32(rawValue);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("maxItems must be an integer between 1 and " + MaximumMaxItems + ".", nameof(arguments), ex);
            }

            if (value < 1 || value > MaximumMaxItems)
                throw new ArgumentOutOfRangeException(nameof(arguments), "maxItems must be between 1 and " + MaximumMaxItems + ".");

            return value;
        }

        private static void AppendSafeMetadata(StringBuilder output, IDictionary<string, string> metadata)
        {
            if (metadata == null || metadata.Count == 0) return;

            foreach (var item in metadata)
            {
                if (IsSensitiveKey(item.Key))
                {
                    output.AppendLine("Metadata | " + Safe(item.Key) + " | <redacted>");
                    continue;
                }

                output.AppendLine("Metadata | " + Safe(item.Key) + " | " + SafeContent(item.Value));
            }
        }

        private static bool IsSensitiveKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            var normalized = key.Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).ToLowerInvariant();
            return normalized.IndexOf("password", StringComparison.Ordinal) >= 0 ||
                   normalized.IndexOf("apikey", StringComparison.Ordinal) >= 0 ||
                   normalized.IndexOf("secret", StringComparison.Ordinal) >= 0 ||
                   normalized.IndexOf("connectionstring", StringComparison.Ordinal) >= 0 ||
                   normalized.IndexOf("credential", StringComparison.Ordinal) >= 0 ||
                   normalized.IndexOf("token", StringComparison.Ordinal) >= 0;
        }

        private static string SafeContent(string value)
        {
            var safe = Safe(value);
            if (safe.Length <= MaximumContentLength) return safe;
            return safe.Substring(0, MaximumContentLength) + "… (truncated)";
        }

        private static string Safe(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\r", " ").Replace("\n", " ");
        }

        private static AiTool CreateDefinition()
        {
            return new AiTool
            {
                Id = "hagent.internal.memory",
                Name = "HAgent Internal Memory",
                Description = "Read-only bounded inspection of HAgent-owned memory for one explicit scope and owner. Secrets in memory metadata are redacted; no memory write operation is exposed.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"scope\":{\"type\":\"string\",\"enum\":[\"Session\",\"Task\",\"Agent\",\"User\",\"Application\",\"Shared\"]},\"ownerId\":{\"type\":\"string\",\"minLength\":1},\"kind\":{\"type\":\"string\",\"enum\":[\"Fact\",\"Preference\",\"Task\",\"Event\"]},\"taskId\":{\"type\":\"string\"},\"text\":{\"type\":\"string\"},\"maxItems\":{\"type\":\"integer\",\"minimum\":1,\"maximum\":50}},\"required\":[\"scope\",\"ownerId\"],\"additionalProperties\":false}",
                Category = "BuiltIn",
                Type = AiToolType.BuiltIn,
                IsBuiltIn = true,
                Enabled = true
            };
        }
    }
}
