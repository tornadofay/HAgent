using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestInternalMemoryAsync(string input)
        {
            var options = await LoadStorageOptionsAsync(CancellationToken.None).ConfigureAwait(false);
            var memoryStore = await CreateConfiguredMemoryStoreAsync(CancellationToken.None).ConfigureAwait(false);
            var ownerId = "example-internal-memory-42";
            var hiddenOwnerId = "example-internal-memory-hidden";
            var content = string.IsNullOrWhiteSpace(input) ? "HAgent-internal-memory-42" : input.Trim();
            var visibleId = Guid.NewGuid().ToString("N");
            var hiddenId = Guid.NewGuid().ToString("N");

            await memoryStore.AddAsync(new MemoryEntry
            {
                Id = visibleId,
                Scope = MemoryScope.Application,
                Kind = MemoryKind.Fact,
                OwnerId = ownerId,
                Content = content,
                Metadata = new Dictionary<string, string>
                {
                    { "source", "Example" },
                    { "apiKey", "must-not-be-returned" },
                    { "normal", "visible-metadata" }
                }
            }, CancellationToken.None).ConfigureAwait(false);

            await memoryStore.AddAsync(new MemoryEntry
            {
                Id = hiddenId,
                Scope = MemoryScope.Application,
                Kind = MemoryKind.Fact,
                OwnerId = hiddenOwnerId,
                Content = "Hidden-memory-value",
                Metadata = new Dictionary<string, string>()
            }, CancellationToken.None).ConfigureAwait(false);

            try
            {
                var tool = new HAgentInternalMemoryTool(memoryStore);
                var result = await tool.ExecuteAsync(new ToolExecutionContext
                {
                    AgentId = "example-agent",
                    ToolCallId = "memory-call-42",
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    Arguments = new Dictionary<string, object>
                    {
                        { "scope", "Application" },
                        { "ownerId", ownerId },
                        { "maxItems", 10 }
                    },
                    CancellationToken = CancellationToken.None
                }).ConfigureAwait(false);

                if (!result.Succeeded)
                    throw new InvalidOperationException("Internal memory tool failed: " + result.Error);
                if (result.Output.IndexOf(content, StringComparison.Ordinal) < 0)
                    throw new InvalidOperationException("Internal memory tool did not return the expected scoped memory content.");
                if (result.Output.IndexOf("Hidden-memory-value", StringComparison.Ordinal) >= 0 ||
                    result.Output.IndexOf(hiddenOwnerId, StringComparison.Ordinal) >= 0)
                    throw new InvalidOperationException("Internal memory tool returned memory outside the requested owner scope.");
                if (result.Output.IndexOf("must-not-be-returned", StringComparison.Ordinal) >= 0)
                    throw new InvalidOperationException("Internal memory tool exposed a sensitive metadata value.");
                if (result.Output.IndexOf("apiKey | <redacted>", StringComparison.OrdinalIgnoreCase) < 0)
                    throw new InvalidOperationException("Internal memory tool did not redact the sensitive metadata key as expected.");
                if (result.Output.IndexOf("normal | visible-metadata", StringComparison.Ordinal) < 0)
                    throw new InvalidOperationException("Internal memory tool did not preserve non-sensitive metadata.");

                var invalidMax = false;
                try
                {
                    await tool.ExecuteAsync(new ToolExecutionContext
                    {
                        AgentId = "example-agent",
                        ToolCallId = "memory-call-invalid",
                        CorrelationId = Guid.NewGuid().ToString("N"),
                        Arguments = new Dictionary<string, object>
                        {
                            { "scope", "Application" },
                            { "ownerId", ownerId },
                            { "maxItems", 51 }
                        },
                        CancellationToken = CancellationToken.None
                    }).ConfigureAwait(false);
                }
                catch (ArgumentOutOfRangeException)
                {
                    invalidMax = true;
                }

                if (!invalidMax)
                    throw new InvalidOperationException("Internal memory tool did not reject maxItems above the hard limit.");

                Write("INTERNAL MEMORY",
                    "Contract test succeeded." + Environment.NewLine +
                    "Storage backend: " + options.StorageType + Environment.NewLine +
                    "Tool: " + tool.Definition.Name + Environment.NewLine +
                    "Tool ID: " + tool.Definition.Id + Environment.NewLine +
                    "Scope: Application" + Environment.NewLine +
                    "Owner: " + ownerId + Environment.NewLine +
                    "Returned scoped memory: yes" + Environment.NewLine +
                    "Cross-owner memory: rejected" + Environment.NewLine +
                    "Sensitive metadata: redacted" + Environment.NewLine +
                    "maxItems=51: rejected" + Environment.NewLine +
                    "Write operations exposed by tool: none.");
            }
            finally
            {
                await memoryStore.RemoveAsync(visibleId, CancellationToken.None).ConfigureAwait(false);
                await memoryStore.RemoveAsync(hiddenId, CancellationToken.None).ConfigureAwait(false);
            }
        }
    }
}
