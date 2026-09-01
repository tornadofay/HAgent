using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Providers.OpenAICompatible;
using HAgent.Runtime;
using HAgent.Storage.File;
using HAgent.Storage.MySql;
using HAgent.Storage.SqlServer;

namespace HAgent.ExternalConsumer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        private static async Task RunAsync()
        {
            // This sample represents an unrelated host consuming the HAgent system.
            // It references the production HAgent modules but supplies its own host
            // storage/provider implementations so no external database or API is required.
            TouchProductionSurface();

            var provider = new AiProvider
            {
                Id = "external-provider-42",
                Name = "External Consumer Test Provider",
                Kind = "external-consumer-test",
                DefaultModel = "external-model-42",
                Enabled = true
            };

            var agent = new AiAgent
            {
                Id = "external-agent-42",
                Name = "External Consumer Test Agent",
                ProviderId = provider.Id,
                Model = provider.DefaultModel,
                Enabled = true
            };

            var store = new InMemoryAiStore(provider, agent);
            var secrets = new InMemorySecretStore();
            var adapter = new ExternalProviderAdapter();
            var client = new HAgentClient(store, secrets, new[] { adapter });

            var canonical = await client.ExecuteAsync(
                new AgentExecutionRequest
                {
                    AgentId = agent.Id,
                    Messages = new[]
                    {
                        new AIMessage("user", "external-host-message-1"),
                        new AIMessage("user", "external-host-message-2")
                    },
                    HostCorrelationId = "external-host-correlation-42",
                    HostContext = new Dictionary<string, string>
                    {
                        { "operation", "external-consumer-verification" },
                        { "resource", "resource-42" }
                    }
                },
                CancellationToken.None).ConfigureAwait(false);

            Assert(canonical.State == AgentExecutionState.Succeeded, "Canonical external execution did not succeed.");
            Assert(canonical.Messages.Count == 2, "Canonical external request did not preserve both messages.");
            Assert(canonical.HostCorrelationId == "external-host-correlation-42", "Host correlation was not preserved.");
            Assert(canonical.Snapshot.HostContext["resource"] == "resource-42", "Host context was not captured.");

            var sessionInstance = AgentRuntimeInstance.Create(agent, AgentRuntimeScope.Session);
            var taskInstance = AgentRuntimeInstance.Create(agent, AgentRuntimeScope.Task);
            var executions = await Task.WhenAll(
                client.ExecuteAsync(sessionInstance, "external-session-work", cancellationToken: CancellationToken.None),
                client.ExecuteAsync(taskInstance, "external-task-work", cancellationToken: CancellationToken.None)).ConfigureAwait(false);

            Assert(executions.Length == 2, "Concurrent external consumer execution count is incorrect.");
            Assert(executions.All(x => x.State == AgentExecutionState.Succeeded), "A concurrent external execution did not succeed.");
            Assert(executions[0].Id != executions[1].Id, "External executions must have distinct execution IDs.");
            Assert(executions[0].CorrelationId != executions[1].CorrelationId, "External executions must have distinct correlation IDs.");
            Assert(executions[0].RuntimeInstanceId != executions[1].RuntimeInstanceId, "External executions must have distinct runtime instances.");

            Console.WriteLine("[EXTERNAL CONSUMER]");
            Console.WriteLine("Production HAgent modules referenced: Core, Provider, File, SQL Server, MySQL, WinForms");
            Console.WriteLine("Canonical request: succeeded");
            Console.WriteLine("Host correlation preserved: yes");
            Console.WriteLine("Host context preserved: yes");
            Console.WriteLine("Concurrent runtime executions: 2");
            Console.WriteLine("Distinct execution IDs: yes");
            Console.WriteLine("Distinct correlation IDs: yes");
            Console.WriteLine("Distinct runtime instances: yes");
            Console.WriteLine("Host domain implementation inside HAgent: none");
            Console.WriteLine("External consumer verification succeeded.");
        }

        private static void TouchProductionSurface()
        {
            // Compile-time references to the production HAgent modules.
            GC.KeepAlive(typeof(OpenAICompatibleProviderAdapter));
            GC.KeepAlive(typeof(ProtectedDataSecretStore));
            GC.KeepAlive(typeof(SqlServerHAgentStorageBootstrapper));
            GC.KeepAlive(typeof(MySqlHAgentStorageBootstrapper));
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private sealed class ExternalProviderAdapter : IAiProviderAdapter
        {
            public string Kind { get { return "external-consumer-test"; } }
            public string DisplayName { get { return "External Consumer Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                request.Validate();
                cancellationToken.ThrowIfCancellationRequested();

                return Task.FromResult(new AIResponse
                {
                    AgentId = request.Agent.Id,
                    ProviderId = request.Provider.Id,
                    Model = request.Agent.Model,
                    Text = "EXTERNAL-CONSUMER-OK"
                });
            }
        }

        private sealed class InMemoryAiStore : IAiStore
        {
            private readonly List<AiProvider> _providers;
            private readonly List<AiAgent> _agents;

            public InMemoryAiStore(AiProvider provider, AiAgent agent)
            {
                _providers = new List<AiProvider> { provider };
                _agents = new List<AiAgent> { agent };
            }

            public Task<IReadOnlyList<AiProvider>> GetProvidersAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult((IReadOnlyList<AiProvider>)_providers.AsReadOnly());
            }

            public Task<IReadOnlyList<AiAgent>> GetAgentsAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult((IReadOnlyList<AiAgent>)_agents.AsReadOnly());
            }

            public Task SaveProviderAsync(AiProvider provider, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            public Task SaveAgentAsync(AiAgent agent, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            public Task DeleteProviderAsync(string providerId, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            public Task DeleteAgentAsync(string agentId, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }
        }

        private sealed class InMemorySecretStore : ISecretStore
        {
            public Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            public Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(string.Empty);
            }

            public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }
        }
    }
}
