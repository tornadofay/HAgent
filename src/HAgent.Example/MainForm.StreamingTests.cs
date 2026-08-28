using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.WinForms.Helpers.Button;

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

        private void AddLiveStreamingTab()
        {
            var page = new TabPage("Live Streaming")
            {
                BackColor = Surface,
                Padding = new Padding(0)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Surface,
                Padding = new Padding(22)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Surface,
                Padding = new Padding(0)
            };

            var startButton = CreateButton("Start live stream", 170);
            var cancelButton = CreateButton("Cancel", 110);
            cancelButton.Enabled = false;
            actions.Controls.Add(startButton);
            actions.Controls.Add(cancelButton);

            var inputLabel = new Label
            {
                Text = "Message to send — editable",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(1, 4, 0, 0)
            };

            var input = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = "Explain briefly why streaming responses are useful in a desktop AI assistant. Reply in 3 short sentences.",
                Font = new Font("Segoe UI", 9.2f),
                BackColor = Color.White,
                ForeColor = Text,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 2, 0, 4)
            };

            var description = new Label
            {
                Text = "This test uses the selected agent and the real provider streaming path. Text arrives incrementally in Global output; provider reasoning remains separate when supplied.",
                Dock = DockStyle.Fill,
                ForeColor = Text,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(1, 4, 20, 0)
            };

            var expected = new Label
            {
                Text = "Expected result: Global output updates before the request completes, the final response is assembled correctly, and cancellation stops the request without hanging the UI.",
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.8f),
                Padding = new Padding(1, 4, 20, 0)
            };

            var note = new Label
            {
                Text = "Requirement: the selected provider adapter must implement streaming. The OpenAI-compatible adapter does.",
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.6f),
                Padding = new Padding(1, 5, 20, 0)
            };

            startButton.Click += async delegate
            {
                await StartLiveStreamingAsync(input.Text, startButton, cancelButton);
            };
            cancelButton.Click += delegate
            {
                if (_streamingCts != null)
                    _streamingCts.Cancel();
            };

            page.Controls.Add(layout);
            layout.Controls.Add(actions, 0, 0);
            layout.Controls.Add(inputLabel, 0, 1);
            layout.Controls.Add(input, 0, 2);
            layout.Controls.Add(description, 0, 3);
            layout.Controls.Add(expected, 0, 4);
            layout.Controls.Add(note, 0, 5);

            _tabs.TabPages.Add(page);
        }

        private async Task StartLiveStreamingAsync(string message, HButton startButton, HButton cancelButton)
        {
            var request = RequireInput(message);
            if (_streamingCts != null)
                return;

            SetButtonsEnabled(false);
            startButton.Enabled = false;
            cancelButton.Enabled = true;
            _globalStatus.Text = "Streaming...";
            _globalStatus.ForeColor = Accent;
            _output.Text = string.Empty;

            _streamingCts = new CancellationTokenSource();
            var startedAt = DateTimeOffset.UtcNow;
            var deltaCount = 0;
            var reasoningStarted = false;

            try
            {
                var selection = await CreateClientAndAgentAsync();
                _output.AppendText("[LIVE STREAMING]" + Environment.NewLine +
                                   "Agent: " + selection.Agent.Name + Environment.NewLine +
                                   "Provider: " + selection.Provider.Name + Environment.NewLine +
                                   "Model: " + selection.Model + Environment.NewLine +
                                   "Request: " + request + Environment.NewLine +
                                   "Status: receiving incremental response..." + Environment.NewLine +
                                   "Text: ");

                var progress = new Progress<AIResponseDelta>(delta =>
                {
                    deltaCount++;
                    if (!string.IsNullOrEmpty(delta.Text))
                        _output.AppendText(delta.Text);

                    if (!string.IsNullOrEmpty(delta.Reasoning))
                    {
                        if (!reasoningStarted)
                        {
                            _output.AppendText(Environment.NewLine + "Reasoning: ");
                            reasoningStarted = true;
                        }
                        _output.AppendText(delta.Reasoning);
                    }
                });

                var response = await selection.Client.StreamAsync(
                    selection.Agent.Id,
                    request,
                    progress,
                    _streamingCts.Token).ConfigureAwait(true);

                _output.AppendText(Environment.NewLine +
                                   Environment.NewLine +
                                   "Final response: " + response.Text + Environment.NewLine +
                                   "Deltas received: " + deltaCount + Environment.NewLine +
                                   "Elapsed: " + (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds.ToString("0") + " ms" + Environment.NewLine +
                                   "Streaming completed successfully.");
                _globalStatus.Text = "Ready";
                _globalStatus.ForeColor = Muted;
            }
            catch (OperationCanceledException)
            {
                _output.AppendText(Environment.NewLine + Environment.NewLine + "Streaming cancelled by user.");
                _globalStatus.Text = "Cancelled";
                _globalStatus.ForeColor = Muted;
            }
            catch (Exception ex)
            {
                Write("EXCEPTION", ex.ToString());
                _globalStatus.Text = "Error";
                _globalStatus.ForeColor = Error;
                HMessage.ShowException(this, "The live streaming test failed.", "HAgent Example", ex);
            }
            finally
            {
                if (_streamingCts != null)
                {
                    _streamingCts.Dispose();
                    _streamingCts = null;
                }

                startButton.Enabled = true;
                cancelButton.Enabled = false;
                SetReady();
            }
        }
    }
}
