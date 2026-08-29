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
- [x] Per-agent tool assignment persisted through `AiAgent.ToolIds`.
- [x] Agent assignment Example verification.

### Remaining
- [ ] Per-session temporary tools.
- [ ] Built-in tool handlers.
- [ ] Application tool registration guidance/API conventions.
- [ ] Declarative execution engine.
- [ ] SQL Server tool execution.
- [ ] MySQL tool execution.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool audit/history.
- [ ] Tool budgets and stronger loop detection.
- [ ] More capability negotiation around tool calling.

## 0.6 Safety
- [x] General permission configuration UI for the current WinForms policy.
- [x] Persist current UI permission policy through the public settings path.
- [ ] Read/write/invoke/export permissions across all tool types.
- [ ] Host authorization callbacks.
- [ ] Human approval lifecycle.
- [ ] Input/output/tool guardrails.
- [ ] Budgets and observability.
- [ ] Sensitive-data redaction.

## 0.7 WinForms UI Context

### Implemented foundation
- [x] `IUiContext` and `WinFormsUiContext`.
- [x] Stable control lookup by WinForms control name.
- [x] UI-thread dispatch for context reads.
- [x] UI lifecycle-safe same-thread inspection before a native handle exists.
- [x] `UiControlSnapshot` inspection model.
- [x] `ui.inspect`, `ui.read_control`, and `ui.read_data` read-only tools.
- [x] Bound/native `DataGridView` source preference.
- [x] Bounded data extraction with lazy adaptation.
- [x] `DataTable` treated as optional rather than the default representation.
- [x] Coarse `UiAutomationPermissions` policy.
- [x] Built-in UI tools enforce the permission policy.
- [x] Explicit permission-aware `HAgentHost.Attach(...)` overload.
- [x] Semantic descriptor model for logical name, role, binding, data role, and permitted capabilities.
- [x] Automatic semantic discovery for standard WinForms controls.
- [x] Optional developer-supplied `IUiSemanticProvider` hook for custom controls/BaseForm/domain semantics.
- [x] Read-only `ui.discover` tool, gated by automatic-discovery permission.

### Current design
- [x] Automatic UI discovery is optional convenience behavior, never implicit authority.
- [x] Developers may replace/enrich automatic semantics with application-specific authorization or semantic logic.
- [x] “Form serialization” is treated as UI context/adapter behavior, not arbitrary object serialization.
- [x] Explicit developer abstractions remain a supported path for domain concepts such as Customer, Invoice, and Contact.
- [x] Automatic semantic discovery can identify useful standard controls and bound data sources without forcing wrappers.
- [ ] Automatic data querying against application/SQL sources requires explicit permissions and restricted query tools; never give the model arbitrary database access by default.
- [ ] Cross-form discovery/memory requires explicit scope and policy.
- [x] The UI boundary remains outside `HAgent.Core`, allowing future HControl/BaseForm, GDI, DirectX, and Unity-style adapters.

### Next
- [ ] Form/UserControl attachment and stable logical form identity.
- [ ] BindingSource/CurrencyManager/IList/native collection adapters.
- [ ] Semantic relationship discovery between controls and data.
- [ ] Safe data projection/query abstraction.
- [ ] SQL Server/MySQL read/query tools under separate restricted permissions.
- [ ] UI write/invoke tools only after permission/approval foundation.
- [ ] Floating assistant/flyout.

## Example host
- [x] Every current Example tab has editable input and expected-output guidance.
- [x] Every current Example tab has a copyable C# reproduction snippet beside its input.
- [ ] Every public-API snippet should become self-contained or link to a clearly identified shared setup snippet.
- [ ] Keep snippets synchronized whenever a public API used by an example changes.
- [ ] Maintain focused partial test files instead of returning to one monolithic `MainForm` implementation.

## Documentation workflow
- [x] Small source files under `docs/plan/` and `docs/roadmap/`.
- [x] Generated root `plan.md` and `roadmap.md` workflow.
- [x] Documentation source changes are part of implementation state.

## 0.8 Chat + scopes
- [ ] Agent profile separated from runtime binding.
- [ ] Application/global, form, session, task, and ephemeral scopes.
- [ ] User ↔ agent chat with global/form agent selector.
- [ ] Persistent conversations and conversation switching/search.
- [ ] Streaming UI and tool activity visualization.
- [ ] Reasoning visibility policy.
- [ ] Cancel/stop and simultaneous conversations.
- [ ] Safe handling of deleted/disabled agents.
- [ ] Cross-form memory governed by scope and authorization policy.

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
