# Active implementation plan

## 0.5 Tools + Agent Loop

### Verified
- [x] Definition/handler separation.
- [x] Registry and direct execution.
- [x] JSON Schema validation before execution.
- [x] Provider tool-definition transport.
- [x] Bounded multi-turn loop.
- [x] File definition persistence.
- [x] Six initial tool categories.
- [x] Live Groq tool loop.
- [x] Per-agent tool assignment via persisted `ToolIds`.
- [x] Agent/tool persistence verification.

### Current work
- [ ] Per-session temporary tools.
- [ ] Built-in tool handlers beyond the initial UI read-only tools.
- [ ] Declarative execution engine.
- [ ] SQL Server tool execution.
- [ ] MySQL tool execution.
- [ ] Tool timeout/cancellation/progress policy.
- [ ] Tool audit/history.
- [ ] Tool budgets and stronger loop detection.
- [ ] More capability negotiation around tool calling.

## 0.6 Safety
- [ ] Read/write/invoke/export permissions.
- [ ] Host authorization callbacks.
- [ ] Human approval lifecycle.
- [ ] Input/output/tool guardrails.
- [ ] Budgets and observability.
- [ ] Sensitive-data redaction.

## 0.7 WinForms UI Context

### Implemented foundation
- [x] `IUiContext` contract.
- [x] `WinFormsUiContext` attachment/read/inspection path.
- [x] Stable control lookup by WinForms control name.
- [x] UI state snapshots for form/control trees.
- [x] TextBox/ComboBox/CheckBox/RadioButton/NumericUpDown/DateTimePicker/ListBox/ListView/Label value extraction.
- [x] DataGridView bound-source extraction.
- [x] DataTable/DataView/native enumerable handling where naturally available.
- [x] Bounded row reads and cancellation checks.
- [x] Read-only `ui.inspect`, `ui.read_control`, and `ui.read_data` tools.
- [x] `HAgentHost.Attach(form, registry)` bridge.
- [x] Provider-independent Example UI Context test.

### Current work
- [ ] Data source adapters for BindingSource/CurrencyManager/IList and richer collection types.
- [ ] Public attach/detach lifecycle suitable for application use.
- [ ] Form/UserControl/custom-control semantic identity improvements.
- [ ] Floating assistant/flyout.
- [ ] `ui.write_control`, `ui.move_control`, `ui.resize_control`, `ui.invoke`.
- [ ] UI-thread dispatch integrated with host cancellation.
- [ ] Dry-run/preview and undo hooks.
- [ ] Per-control permissions and human approval integration.

### Mandatory representation rule
- [x] Prefer the lightest representation that preserves required information.
- [x] Prefer bound/native data sources over visible-cell scraping.
- [x] Avoid eager copying/materialization.
- [x] `DataTable` is optional, not the architectural default.
- [ ] Add paging/projection/streaming adapters for large sources where appropriate.

## 0.8 Chat + scopes
- [ ] Global/form/session/task/ephemeral scopes.
- [ ] User ↔ agent chat.
- [ ] Global/form agent selector.
- [ ] Persistent conversations.
- [ ] Streaming and tool activity UI.
- [ ] Reasoning visibility policy.
- [ ] Deleted/disabled agent handling.

## 0.9 Collaboration
- [ ] Agents-as-tools.
- [ ] Handoffs/delegation.
- [ ] Agent-to-agent messaging board/channels.
- [ ] Shared/private memory policies.
- [ ] Parallel execution and collaboration budgets.

## 0.10 Tasks + workflows
- [ ] Task/job lifecycle.
- [ ] Planning/execution/verification.
- [ ] Durable checkpoints and restart recovery.
- [ ] Scheduling/events/background work.
- [ ] Workflow budgets and observability.

## Later
- [ ] More provider adapters and multimodal support.
- [ ] Extension/provider/tool/UI-adapter DLL ecosystem.
- [ ] SQL/MySQL memory stores.
- [ ] Optional vector/MCP integrations.
- [ ] SDKs and developer diagnostics.
- [ ] Stable 1.0 contracts/NuGet.
- [ ] .NET 10 after migration to compatible Visual Studio.
