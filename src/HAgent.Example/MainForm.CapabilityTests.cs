using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestCapabilitiesAsync(string unused)
        {
            var selection = await CreateClientAndAgentAsync();
            var capabilities = await selection.Client.GetModelCapabilitiesAsync(
                selection.Provider.Id,
                selection.Model,
                CancellationToken.None);

            var lines = new string[]
            {
                "Provider: " + selection.Provider.Name,
                "Model: " + capabilities.Model,
                "",
                "Chat: " + capabilities.Get(AiCapability.Chat),
                "Streaming: " + capabilities.Get(AiCapability.Streaming),
                "Structured Output: " + capabilities.Get(AiCapability.StructuredOutput),
                "Tool Calling: " + capabilities.Get(AiCapability.ToolCalling),
                "Vision: " + capabilities.Get(AiCapability.Vision),
                "Audio Input: " + capabilities.Get(AiCapability.AudioInput),
                "Audio Output: " + capabilities.Get(AiCapability.AudioOutput),
                "Embeddings: " + capabilities.Get(AiCapability.Embeddings),
                "Reasoning: " + capabilities.Get(AiCapability.Reasoning),
                "",
                "Unknown means the adapter/provider has not established support; HAgent must not assume it."
            };

            Write("CAPABILITIES", string.Join(Environment.NewLine, lines));
        }
    }
}
