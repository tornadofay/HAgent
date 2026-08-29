# Active and near-term roadmap

## 0.5 Tools + Agent Loop

Tool definitions, validation, persistence, per-agent assignment, provider tool transport, deterministic tool loops, and live Groq tool calling are implemented.

Remaining tool work:

- [ ] Per-session temporary tools.
- [ ] Built-in tool handlers.
- [ ] Declarative execution engine.
- [ ] SQL Server tool execution layer.
- [ ] MySQL tool execution layer.
- [ ] Tool aliases/versioning.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool audit/history and budgets.
- [ ] Stronger loop detection and provider/tool capability negotiation.

## 0.6 Safety + Permissions

- [ ] General permission configuration UI.
- [ ] Read/write/invoke/export permissions.
- [ ] Host authorization callbacks.
- [ ] Human approval lifecycle.
- [ ] Input/output/tool guardrails.
- [ ] Budgets and observability.
- [ ] Sensitive-data redaction.

## 0.7 WinForms UI Context + Automation

The WinForms subsystem uses **UI Context**, not generic “form serialization”. Two supported development modes are intentional:

1. **Explicit domain abstraction** — the host can expose typed concepts such as Customer, Contact, Invoice, or a custom view-model/tool instead of allowing HAgent to inspect arbitrary controls.
2. **Automatic UI Context** — HAgent can discover controls, bound data, and useful relationships automatically when the host explicitly enables the appropriate permission policy.

Automatic discovery is convenience, not authority. Attaching a form must never automatically grant write or invoke access.

### Implemented foundation

- [x] `IUiContext` / `WinFormsUiContext`.
- [x] Stable control lookup by WinForms control name.
- [x] UI-thread-safe inspection/read operations.
- [x] `UiControlSnapshot`.
- [x] `ui.inspect`.
- [x] `ui.read_control`.
- [x] `ui.read_data`.
- [x] Bound/native DataGridView source preference.
- [x] Bounded data extraction.
- [x] Light-weight representation rule: avoid unnecessary `DataTable` materialization.
- [x] `UiAutomationPermissions` with safe defaults.
- [x] Read-only UI tools enforce the permission policy.

### Automatic data understanding

- [ ] Semantic discovery of common WinForms controls and relationships.
- [ ] Domain-friendly labels/semantic IDs in addition to raw control names.
- [ ] BindingSource/CurrencyManager/IList/collection adapters.
- [ ] Lazy/native data projections.
- [ ] Safe identification of tabular data without scraping visible cells when a bound source exists.
- [ ] Optional application-defined semantic adapters for Customer/Invoice/etc.
- [ ] Restricted query abstraction for application/SQL data rather than arbitrary SQL execution.

### Permission model

The initial coarse permission groups are:

- Automatic discovery.
- Read controls.
- Read data.
- Write controls.
- Invoke controls.

Developers may disable automatic behavior and implement their own authorization/abstraction path. Future SQL Server/MySQL query permissions must remain separate from UI permissions.

### UI automation

- [ ] Permission configuration tab in the main HAgent settings UI.
- [ ] Persist permission policy.
- [ ] Form/UserControl attachment and stable logical identity.
- [ ] `HAgentHost.Attach(ai, form)` bridge.
- [ ] Floating assistant/flyout.
- [ ] `ui.write_control`.
- [ ] `ui.move_control`.
- [ ] `ui.resize_control`.
- [ ] `ui.invoke` / approved click.
- [ ] `ui.enable_control` / `ui.disable_control`.
- [ ] Batch operations.
- [ ] Dry-run/preview.
- [ ] Human approval.
- [ ] Per-control permissions.
- [ ] Optional undo/rollback hooks.
