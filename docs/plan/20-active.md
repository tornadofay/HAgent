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

### Current work
- [ ] Complete settings UI persistence wiring and refresh behavior.
- [ ] Per-agent tool assignment UI verified locally.
- [ ] Per-session temporary tools.
- [ ] Built-in tool handlers.
- [ ] Declarative execution engine.
- [ ] SQL Server tool execution.
- [ ] MySQL tool execution.
- [ ] Tool timeout/cancellation/progress.
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
- [ ] Form/UserControl attachment and stable identity.
- [ ] UI state snapshots and provider-neutral context.
- [ ] Native/bound data-source adapters.
- [ ] DataGridView, BindingSource, CurrencyManager, IList and collection adapters.
- [ ] `HAgentHost.Attach(ai, form)` bridge.
- [ ] Floating assistant/flyout.
- [ ] `ui.inspect`, read, write, move, resize, invoke, enable/disable, batch.
- [ ] UI thread dispatch, dry-run, undo hooks and per-control permissions.
- [ ] Always prefer the lightest native representation; `DataTable` is optional.

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
