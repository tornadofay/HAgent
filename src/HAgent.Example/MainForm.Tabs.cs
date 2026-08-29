using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddFeatureTabs()
        {
            AddApiTab("Messaging", "Send message", "Calls HAgentClient.SendAsync with the selected agent.", "A conversational model should return exactly MESSAGE-OK.", "Reply with exactly MESSAGE-OK and nothing else.", SendMessageAsync, "Provider/model warning", "Choose a conversational model; model discovery can also return guard, classification, embedding, and other non-chat models.");
            AddApiTab("Session", "Run session test", "Creates one AgentSession, sends the editable first message, then asks the fixed recall question. The second request receives the session history.", "The response should identify HAgent-session-42 and the output should show the retained transcript.", "Store this temporary test value in our conversation: HAgent-session-42.", TestSessionAsync, "Memory boundary", "This validates in-session history forwarding. Durable memory is tested separately.");
            AddApiTab("Persistent Session", "Run persistence test", "Creates a persistent session, sends an editable message, closes the conversation store, then opens a fresh store and reopens the same session ID.", "The reopened session should retain the message and show the same session ID without using the first store instance.", "Store this persistent test value in this conversation: HAgent-persist-42.", TestPersistentSessionAsync, "Persistence boundary", "This makes an AI request and verifies conversation persistence using the FileConversationStore.");
            AddApiTab("Runtime 0.2", "Run runtime test", "Uses the 0.2 execution pipeline with timeout, provider-attempt, retry, lifecycle, and diagnostics behavior.", "Execution should reach Succeeded and display execution ID, state, provider, model, and response.", "Reply with the word RUNTIME-OK and nothing else.", TestRuntimeAsync, "Runtime boundary", "The runtime orchestrates execution but does not yet infer model suitability.");
            AddApiTab("Configuration", "Read configuration", "Reads providers and agents directly from the local file store.", "The output should show the settings path, provider count, agent count, and relationships.", "No AI request is sent by this example.", ReadConfigurationAsync, "Storage boundary", "This verifies host-side configuration reading.");
            AddApiTab("Memory", "Run memory test", "Writes explicit memory to a persistent file store, closes it, opens a second instance, recalls the entry, and removes it.", "The second store should recall the same memory ID and content, proving persistence outside the original object.", "Remember exactly this test value: HAgent-memory-42.", TestMemoryAsync, "Memory boundary", "Provider-independent and no AI request. Tests explicit durable memory operations.");
            AddApiTab("Automatic Memory", "Run memory policy test", "Sends an editable message through the default explicit memory policy and checks whether an explicit memory request becomes durable memory.", "A message beginning with 'Remember this:' should create one agent-scoped memory; an ordinary message should create none.", "Remember this: HAgent-auto-memory-42.", TestAutomaticMemoryAsync, "Memory policy", "The default policy is intentionally conservative and does not save ordinary conversation automatically.");
            AddApiTab("Context Budget", "Run budget test", "Builds an intentionally oversized conversation and applies bounded message/character limits before provider submission.", "The resulting context should be smaller, remain in original order, preserve the configured leading messages, and keep the newest messages.", "No AI request is sent by this example.", RunContextBudgetTestAsync, "Context boundary", "Tokenizer-free and deterministic; a provider-specific tokenizer can be added later.");
            AddApiTab("Task / Event Memory", "Run task memory test", "Creates a task record and a checkpoint event under one task ID, then recalls only that task's records from the memory store.", "Exactly two records should be returned: one Task and one Event, both with the same task ID and Task scope.", "Import customers from the selected form.", TestTaskMemoryAsync, "Task memory", "Task/event memory is typed and uses the same lightweight memory store; no AI request is sent.");
            AddApiTab("Episodic Memory", "Run episode test", "Stores one compact completed experience with title, summary, outcome, task ID, session ID, and provenance, then recalls it without storing the full conversation.", "The episode should be recalled by task ID and preserve its outcome, task provenance, and session provenance.", "1,284 customers imported successfully; 3 records were rejected because their email addresses were invalid.", TestEpisodicMemoryAsync, "Episodic memory", "An episode is a compact reusable experience record, not a transcript or a replacement for event history.");
            AddApiTab("Capabilities", "Inspect model capabilities", "Queries the selected provider adapter for capabilities of the selected model.", "Chat should be Supported for the OpenAI-compatible adapter; optional capabilities should remain Unknown unless the provider explicitly establishes them.", "No AI request is sent by this example.", TestCapabilitiesAsync, "Capability boundary", "Unknown is intentional. HAgent must not guess that a model supports tools, vision, reasoning, embeddings, or streaming.");
            AddApiTab("Response Normalization", "Run contract test", "Verifies the provider-neutral AIResponse contract can retain ordinary text, explicit reasoning, structured JSON output, and normalized tool calls at the same time.", "The output should show one tool call, valid structured JSON, separate reasoning, and unchanged response text.", "No AI request is sent by this example.", TestResponseNormalizationAsync, "Contract boundary", "This is provider-independent. Actual provider parsing is exercised by the adapter, while the public response contract remains stable.");
            AddApiTab("Streaming", "Run stream contract test", "Verifies the provider-neutral streaming delta contract can assemble incremental text and preserve reasoning separately.", "The output should show ordered text assembly and separate reasoning content. No provider request is required by this example.", "No AI request is sent by this example.", TestStreamingContractAsync, "Streaming boundary", "Actual network/SSE streaming remains provider-specific; non-streaming providers continue to use the normal SendAsync path.");
            AddLiveStreamingTab();
            AddApiTab("Tool Registry", "Run tool registry test", "Registers a deterministic custom tool, validates its JSON Schema arguments, executes it, reads its definition, and unregisters it without making an AI request.", "The tool should echo HAgent-tool-42 and reject invalid/missing/extra arguments before its handler executes.", "HAgent-tool-42", TestToolRegistryAsync, "Tool boundary", "This verifies registration, safe argument validation, execution, discovery, and cleanup before the model tool-call loop is enabled.");
            AddApiTab("Provider Tool Transport", "Run transport test", "Sends a tool definition through the OpenAI-compatible adapter using a local HTTP capture handler and normalizes the returned tool call.", "The captured request should contain the tool name and JSON Schema parameters, and the response should contain one normalized tool call.", "Call the example tool.", TestProviderToolTransportAsync, "Provider boundary", "No external provider is contacted. This verifies tool transport without invoking a live model.");
            AddApiTab("Tool Loop", "Run tool loop test", "Runs a deterministic two-turn agent loop: the model requests a registered tool, HAgent validates and executes it, then returns the tool result to the model.", "Exactly two provider turns and one executed tool call should occur, followed by the expected final answer.", "Use the add tool.", TestToolLoopAsync, "Loop boundary", "The test uses a local HTTP handler and enforces explicit turn/tool-call limits.");
            AddApiTab("Live Tool Loop", "Run live tool loop", "Uses the selected configured provider/model and a temporary application tool to verify real model tool calling, local execution, and the final model response.", "The model should call example_add with two integers, HAgent should execute it locally, and the final response should use the returned sum.", "You must use the example_add tool with a=3 and b=4. After the tool returns, reply with the result in one short sentence.", TestLiveToolLoopAsync, "Live provider", "This test contacts the configured provider. The temporary tool and temporary agent copy are never persisted.");
            AddApiTab("Tool Persistence", "Run persistence test", "Writes one tool definition to the FileToolStore, closes that store, reopens it, verifies the definition, then removes it.", "The second store should restore the same ID, name, type, enabled state, and JSON Schema. Executable handlers must never be serialized.", "No AI request is sent by this example.", TestToolPersistenceAsync, "Storage boundary", "Only the tool definition is persisted. Application-owned executable handlers remain runtime registrations.");
            AddApiTab("Agent Tool Assignment", "Run assignment test", "Persists an agent with a selected tool ID, reopens both the agent store and tool-definition store, and verifies the relationship survived restart.", "The reopened agent should still reference the persisted tool definition by ID.", "No AI request is sent by this example.", TestAgentToolAssignmentAsync, "Configuration boundary", "Assignment determines which capability an agent may request; executable handlers remain application-owned runtime registrations.");
            AddApiTab("UI Context", "Run UI context test", "Attaches a lightweight UI context to a sample WinForms form, inspects controls, reads a TextBox, and reads DataGridView data from its bound source.", "The form and controls should be discovered, the TextBox value should be read, and two rows should be returned from the bound DataTable without requiring a default DataTable architecture.", "No AI request is sent by this example.", TestUiContextAsync, "UI boundary", "Read-only in this slice. UI write, click, move, resize, and approval require the later guardrail/permission layer.");
        }

        private void AddApiTab(string title, string buttonText, string description, string expected, string initialMessage, Func<string, Task> test, string noteTitle, string noteText)
        {
            var page = new TabPage(title) { BackColor = Surface, Padding = new Padding(0) };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Surface,
                Padding = new Padding(22)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 68));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 12));

            var editors = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Surface,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            editors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            editors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            editors.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            editors.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var input = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = initialMessage,
                Font = new Font("Segoe UI", 9.2f),
                BackColor = Color.White,
                ForeColor = Text,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 8, 0)
            };

            var code = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Text = ExampleCodeSnippets.Get(title),
                Font = new Font("Consolas", 9.0f),
                BackColor = Color.FromArgb(244, 243, 248),
                ForeColor = Text,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(8, 0, 0, 0)
            };

            editors.Controls.Add(new Label
            {
                Text = "Test input / message — editable",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            }, 0, 0);
            editors.Controls.Add(new Label
            {
                Text = "C# reproduction snippet — copy",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(8, 0, 0, 0)
            }, 1, 0);
            editors.Controls.Add(input, 0, 1);
            editors.Controls.Add(code, 1, 1);

            var runButton = CreateButton(buttonText, 190);
            runButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            runButton.Click += async delegate
            {
                await RunExampleAsync(delegate { return test(input.Text); });
            };
            _testButtons.Add(runButton);

            layout.Controls.Add(runButton, 0, 0);
            layout.Controls.Add(editors, 0, 1);
            layout.Controls.Add(new Label
            {
                Text = "Description\r\n" + description + "\r\n\r\nExpected result\r\n" + expected,
                Dock = DockStyle.Fill,
                ForeColor = Text,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(1, 10, 20, 0),
                AutoEllipsis = false
            }, 0, 2);
            layout.Controls.Add(new Label
            {
                Text = noteTitle + ": " + noteText,
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.6f),
                Padding = new Padding(1, 6, 20, 0),
                AutoEllipsis = false
            }, 0, 3);

            page.Controls.Add(layout);
            _tabs.TabPages.Add(page);
        }
    }
}
