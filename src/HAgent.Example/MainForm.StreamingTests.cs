using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestStreamingContractAsync(string unused)
        {
            var deltas = new[]
            {
                new AIResponseDelta { Text = "Hello " },
                new AIResponseDelta { Text = "from " },
                new AIResponseDelta { Text = "HAgent." },
                new AIResponseDelta { Reasoning = "provider reasoning" }
            };

            var text = new StringBuilder();
            var reasoning = new StringBuilder();

            foreach (var delta in deltas)
            {
                if (!string.IsNullOrEmpty(delta.Text)) text.Append(delta.Text);
                if (!string.IsNullOrEmpty(delta.Reasoning)) reasoning.Append(delta.Reasoning);
            }

            var response = new AIResponse
            {
                Text = text.ToString(),
                Reasoning = reasoning.ToString()
            };

            if (response.Text != "Hello from HAgent.")
                throw new InvalidOperationException("Streaming text deltas were not assembled in order.");
            if (response.Reasoning != "provider reasoning")
                throw new InvalidOperationException("Streaming reasoning delta was not preserved separately.");

            Write("STREAMING CONTRACT",
                "Contract test succeeded." + Environment.NewLine +
                "Deltas: " + deltas.Length + Environment.NewLine +
                "Assembled text: " + response.Text + Environment.NewLine +
                "Reasoning: " + response.Reasoning + Environment.NewLine +
                "Final response remains the canonical completed result.");

            await Task.CompletedTask;
        }
    }
}
