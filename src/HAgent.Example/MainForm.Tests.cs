using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Providers.OpenAICompatible;
using HAgent.Runtime;
using HAgent.Storage.File;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task<ClientSelection> CreateClientAndAgentAsync()
        {
            var agent = GetSelectedAgent();
            if (agent == null)
                throw new InvalidOperationException("Select an agent first.");
            if (!agent.Enabled)
                throw new InvalidOperationException("The selected agent is disabled. Enable it in Configuration first.");

            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            var providers = await store.GetProvidersAsync();

            var providerIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(agent.ProviderId))
                providerIds.Add(agent.ProviderId);
            if (agent.ProviderIds != null)
                providerIds.AddRange(agent.ProviderIds.Where(x => !string.IsNullOrWhiteSpace(x)));

            var provider = providerIds
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(id => providers.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault(p => p != null && p.Enabled);

            if (provider == null)
                throw new InvalidOperationException("The selected agent has no enabled provider. Agent='" + agent.Name + "'.");

            var model = string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model;
            if (string.IsNullOrWhiteSpace(model))
                throw new InvalidOperationException("No model is configured for agent '" + agent.Name + "' or provider '" + provider.Name + "'.");

            return new ClientSelection(
                new HAgentClient(store, secrets, new[] { new OpenAICompatibleProviderAdapter() }),
                agent,
                provider,
                model);
        }

        private async Task SendMessageAsync(string message)
        {
            var selection = await CreateClientAndAgentAsync();
            var request = RequireInput(message);
            var response = await selection.Client.SendAsync(selection.Agent.Id, request);
            Write("SEND MESSAGE", "Agent: " + selection.Agent.Name + Environment.NewLine +
                                 "Provider: " + selection.Provider.Name + Environment.NewLine +
                                 "Model: " + selection.Model + Environment.NewLine +
                                 "Request: " + request + Environment.NewLine +
                                 "Response: " + response.Text);
        }

        private async Task TestSessionAsync(string firstMessage)
        {
            var selection = await CreateClientAndAgentAsync();
            var session = selection.Client.CreateSession(selection.Agent.Id);
            var request = RequireInput(firstMessage);
            await session.SendAsync(request);
            var response = await session.SendAsync("What temporary test value did I just give you? Reply with only the value.");
            var read = await session.ReadAsync();

            Write("SESSION", "Agent: " + selection.Agent.Name + Environment.NewLine +
                             "Provider: " + selection.Provider.Name + Environment.NewLine +
                             "Model: " + selection.Model + Environment.NewLine +
                             "First request: " + request + Environment.NewLine +
                             "Second response: " + response.Text + Environment.NewLine +
                             "Messages retained: " + read.Messages.Count + Environment.NewLine +
                             "Transcript:" + Environment.NewLine +
                             string.Join(Environment.NewLine, read.Messages.Select(x => "  " + x.Role + ": " + x.Content)));
        }

        private async Task TestPersistentSessionAsync(string firstMessage)
        {
            var selection = await CreateClientAndAgentAsync();
            var request = RequireInput(firstMessage);
            var options = await LoadStorageOptionsAsync().ConfigureAwait(true);
            var sessionId = "example-" + Guid.NewGuid().ToString("N");
            var firstConversationStore = await CreateConfiguredConversationStoreAsync().ConfigureAwait(true);
            string originalTranscript;

            try
            {
                var firstStore = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
                var firstSecrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
                var firstClient = new HAgentClient(
                    firstStore,
                    firstSecrets,
                    new[] { new OpenAICompatibleProviderAdapter() },
                    null,
                    null,
                    firstConversationStore);

                var session = firstClient.CreateSession(selection.Agent.Id, sessionId);
                await session.SendAsync(request);
                var firstRead = await session.ReadAsync();
                originalTranscript = string.Join(Environment.NewLine, firstRead.Messages.Select(x => "  " + x.Role + ": " + x.Content));
            }
            finally
            {
                var disposable = firstConversationStore as IDisposable;
                if (disposable != null) disposable.Dispose();
            }

            var secondConversationStore = await CreateConfiguredConversationStoreAsync().ConfigureAwait(true);
            try
            {
                var secondStore = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
                var secondSecrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
                var secondClient = new HAgentClient(
                    secondStore,
                    secondSecrets,
                    new[] { new OpenAICompatibleProviderAdapter() },
                    null,
                    null,
                    secondConversationStore);

                var reopened = await secondClient.OpenSessionAsync(selection.Agent.Id, sessionId, CancellationToken.None);
                var reopenedRead = await reopened.ReadAsync();
                var retained = reopenedRead.Messages.Any(x => string.Equals(x.Content, request, StringComparison.Ordinal));

                if (!retained)
                    throw new InvalidOperationException("The reopened session did not retain the original user message.");

                var internalConversationTool = new HAgentInternalConversationTool(secondConversationStore);
                var internalRead = await internalConversationTool.ExecuteAsync(new ToolExecutionContext
                {
                    AgentId = selection.Agent.Id,
                    ToolCallId = "persistent-session-conversation-read-42",
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    Arguments = new Dictionary<string, object>
                    {
                        { "sessionId", sessionId },
                        { "maxMessages", 1 }
                    },
                    CancellationToken = CancellationToken.None
                }).ConfigureAwait(false);

                if (!internalRead.Succeeded)
                    throw new InvalidOperationException("The internal conversation read tool failed: " + internalRead.Error);
                if (internalRead.Output.IndexOf("Content | " + request.Trim(), StringComparison.Ordinal) < 0)
                    throw new InvalidOperationException("The internal conversation read tool did not return the persisted user message.");
                if (internalRead.Output.IndexOf("Additional messages omitted by maxMessages.", StringComparison.Ordinal) < 0)
                    throw new InvalidOperationException("The internal conversation read tool did not enforce maxMessages.");

                var conversationStoreType = secondConversationStore.GetType().Name;
                var persistenceLocation = options.StorageType == HAgentStorageType.File
                    ? Path.Combine(options.GetEffectiveRootPath(), "conversations", sessionId + ".json")
                    : "HAgentConversations in " + options.GetEffectiveDatabaseName();

                Write("PERSISTENT SESSION", "Session ID: " + sessionId + Environment.NewLine +
                                          "Storage backend: " + options.StorageType + Environment.NewLine +
                                          "Conversation store: " + conversationStoreType + Environment.NewLine +
                                          "Persistence location: " + persistenceLocation + Environment.NewLine +
                                          "Agent: " + selection.Agent.Name + Environment.NewLine +
                                          "Provider: " + selection.Provider.Name + Environment.NewLine +
                                          "Model: " + selection.Model + Environment.NewLine +
                                          "Persistence test succeeded." + Environment.NewLine +
                                          "Messages retained after reopening: " + reopenedRead.Messages.Count + Environment.NewLine +
                                          "Internal conversation read: succeeded." + Environment.NewLine +
                                          "Internal conversation message bound: maxMessages=1." + Environment.NewLine +
                                          "Original transcript:" + Environment.NewLine + originalTranscript + Environment.NewLine +
                                          "Reopened transcript:" + Environment.NewLine +
                                          string.Join(Environment.NewLine, reopenedRead.Messages.Select(x => "  " + x.Role + ": " + x.Content)));

                await reopened.DeleteAsync(CancellationToken.None);
            }
            finally
            {
                var disposable = secondConversationStore as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
        }

        private async Task TestRuntimeAsync(string message)
        {
            var selection = await CreateClientAndAgentAsync();
            var request = RequireInput(message);
            var execution = await selection.Client.ExecuteAsync(
                selection.Agent.Id,
                request,
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(30),
                    MaxProviderAttempts = 2,
                    MaxRetriesPerProvider = 1
                },
                CancellationToken.None);

            Write("RUNTIME", "Execution: " + execution.Id + Environment.NewLine +
                             "State: " + execution.State + Environment.NewLine +
                             "Failure: " + execution.FailureKind + Environment.NewLine +
                             "Provider error: " + execution.ProviderErrorKind + Environment.NewLine +
                             "Provider: " + selection.Provider.Name + " (" + (execution.Response == null ? string.Empty : execution.Response.ProviderId) + ")" + Environment.NewLine +
                             "Model: " + selection.Model + Environment.NewLine +
                             "Request: " + request + Environment.NewLine +
                             "Response: " + (execution.Response == null ? string.Empty : execution.Response.Text));
        }

        private async Task ReadConfigurationAsync(string unused)
        {
            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            var providers = await store.GetProvidersAsync();
            var agents = await store.GetAgentsAsync();

            Write("CONFIGURATION", "Settings: " + StorageConfigurationPath + Environment.NewLine +
                                  "Providers: " + providers.Count + Environment.NewLine +
                                  string.Join(Environment.NewLine, providers.Select(p => "  - " + p.Name + " [" + p.Kind + "] model=" + p.DefaultModel)) + Environment.NewLine +
                                  "Agents: " + agents.Count + Environment.NewLine +
                                  string.Join(Environment.NewLine, agents.Select(a => "  - " + a.Name + " -> " + a.ProviderId)));

            await Task.CompletedTask;
        }

        private async Task TestMemoryAsync(string message)
        {
            var originalInput = RequireInput(message);
            var storedContent = ExtractMemorySearchText(originalInput);
            var options = await LoadStorageOptionsAsync().ConfigureAwait(true);
            var firstStore = await CreateConfiguredMemoryStoreAsync().ConfigureAwait(true);
            string memoryId;

            try
            {
                var entry = new MemoryEntry
                {
                    Scope = MemoryScope.Application,
                    OwnerId = "HAgent.Example",
                    Content = storedContent,
                    Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "source", "HAgent.Example" },
                        { "test", "persistent-memory" }
                    },
                    CreatedAt = DateTimeOffset.UtcNow
                };

                memoryId = entry.Id;
                await firstStore.AddAsync(entry, CancellationToken.None);
            }
            finally
            {
                var disposable = firstStore as IDisposable;
                if (disposable != null) disposable.Dispose();
            }

            var secondStore = await CreateConfiguredMemoryStoreAsync().ConfigureAwait(true);
            try
            {
                var recalled = await secondStore.SearchAsync(new MemoryQuery
                {
                    OwnerId = "HAgent.Example",
                    Scope = MemoryScope.Application,
                    Text = storedContent,
                    MaxResults = 10
                }, CancellationToken.None);

                var found = recalled.FirstOrDefault(x => string.Equals(x.Id, memoryId, StringComparison.OrdinalIgnoreCase));
                if (found == null)
                    throw new InvalidOperationException("The second memory-store instance could not recall the persisted entry.");

                await secondStore.RemoveAsync(found.Id, CancellationToken.None);
                var persistenceLocation = options.StorageType == HAgentStorageType.File
                    ? Path.Combine(options.GetEffectiveRootPath(), "memory", "memory.jsonl")
                    : "HAgentMemoryEntries in " + options.GetEffectiveDatabaseName();

                Write("MEMORY", "Storage backend: " + options.StorageType + Environment.NewLine +
                              "Persistence location: " + persistenceLocation + Environment.NewLine +
                              "Persistence test succeeded." + Environment.NewLine +
                              "Memory ID: " + found.Id + Environment.NewLine +
                              "Content: " + found.Content + Environment.NewLine +
                              "Scope: " + found.Scope + Environment.NewLine +
                              "Owner: " + found.OwnerId);
            }
            finally
            {
                var disposable = secondStore as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
        }

        private static string ExtractMemorySearchText(string input)
        {
            var value = input ?? string.Empty;
            var marker = "test value";
            var index = value.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var colon = value.IndexOf(':', index);
                if (colon >= 0 && colon + 1 < value.Length)
                    return value.Substring(colon + 1).Trim().TrimEnd('.', '!', '?');
            }

            return value.Trim();
        }
    }
}
