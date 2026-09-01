using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public class CoreTests
    {
        [Fact]
        public async Task InMemoryStore_SavesProviderAndAgent()
        {
            var store = new InMemoryAiStore();
            var provider = new AiProvider { Name = "Test" };
            var agent = new AiAgent { Name = "Assistant", ProviderId = provider.Id };
            await store.SaveProviderAsync(provider);
            await store.SaveAgentAsync(agent);
            Assert.Single(await store.GetProvidersAsync());
            Assert.Single(await store.GetAgentsAsync());
        }

        [Fact]
        public async Task Session_ReadReturnsUserAndAssistantMessages()
        {
            var store = new InMemoryAiStore();
            var provider = new AiProvider { Name = "Test", DefaultModel = "demo" };
            var agent = new AiAgent { Name = "Assistant", ProviderId = provider.Id };
            await store.SaveProviderAsync(provider);
            await store.SaveAgentAsync(agent);
            var secrets = new FakeSecretStore();
            var adapter = new EchoAdapter();
            var client = new HAgentClient(store, secrets, new[] { adapter });
            var session = client.CreateSession(agent.Id);
            await session.SendAsync("hello");
            var read = await session.ReadAsync();
            Assert.Equal(2, read.Messages.Count);
            Assert.Equal("hello", read.Messages[0].Content);
            Assert.Equal("echo: hello", read.Messages[1].Content);
        }

        private sealed class EchoAdapter : HAgent.Abstractions.IAiProviderAdapter
        {
            public string Kind => "openai-compatible";
            public string DisplayName => "Echo Adapter";
            public bool CanHandle(AiProvider provider) => provider != null && string.Equals(provider.Kind, Kind, System.StringComparison.OrdinalIgnoreCase);

            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, System.Threading.CancellationToken token)
                => Task.FromResult(new AIResponse
                {
                    AgentId = request.Agent.Id,
                    ProviderId = request.Provider.Id,
                    Model = request.Agent.Model,
                    Text = "echo: " + request.Messages[request.Messages.Count - 1].Content
                });
        }

        private sealed class FakeSecretStore : HAgent.Abstractions.ISecretStore
        {
            public Task SetAsync(string id, string secret, System.Threading.CancellationToken t = default(System.Threading.CancellationToken)) => Task.CompletedTask;
            public Task<string> GetAsync(string id, System.Threading.CancellationToken t = default(System.Threading.CancellationToken)) => Task.FromResult(string.Empty);
            public Task DeleteAsync(string id, System.Threading.CancellationToken t = default(System.Threading.CancellationToken)) => Task.CompletedTask;
        }
    }
}
