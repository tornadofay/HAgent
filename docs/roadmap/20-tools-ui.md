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

The permission model is a shared authorization concept, not just a WinForms checkbox collection.

- [x] General permission configuration UI.
- [ ] Read/write/invoke/export permissions across all tool categories.
- [ ] Host authorization callbacks.
- [ ] Human approval lifecycle.
- [ ] Input/output/tool guardrails.
- [ ] Budgets and observability.
- [ ] Sensitive-data redaction.

The first WinForms policy UI persists the currently supported coarse permissions with safe defaults. Database-specific permissions and approval workflows remain separate work.

## 0.7 UI Context + Automation

The public concept is **UI Context / Control Adapters**. “Form serialization” is only one possible implementation technique inside a broader system.

### Two development modes

1. **Explicit domain abstraction** — the developer can expose typed application concepts such as Customer, Contact, Invoice, or a custom view-model/tool. This is the preferred option for high-control and sensitive applications.
2. **Automatic UI/data discovery** — HAgent can inspect controls, bindings, and data sources when explicitly enabled by policy. Automatic discovery is convenience, never authority.

### Implemented foundation

- [x] `IUiContext` / `WinFormsUiContext`.
- [x] Stable WinForms control lookup.
- [x] Same-thread lifecycle-safe inspection even before a native form handle exists.
- [x] UI-thread-safe cross-thread reads when a handle exists.
- [x] `UiControlSnapshot`.
- [x] `ui.inspect`.
- [x] `ui.read_control`.
- [x] `ui.read_data`.
- [x] Bound/native `DataGridView` source preference.
- [x] Bounded extraction and lazy adaptation.
- [x] `DataTable` explicitly optional, not the architectural default.
- [x] Coarse `UiAutomationPermissions` with safe defaults.
- [x] Read-only UI tools enforce the policy.
- [x] Explicit permission-aware `HAgentHost.Attach(...)` overload.

### Automatic application understanding

- [ ] Semantic control labels and logical IDs in addition to raw control names.
- [ ] BindingSource/CurrencyManager/IList/native collection adapters.
- [ ] Relationship discovery between controls, bound records, lists, and forms.
- [ ] Lightweight semantic projections without unnecessary copying.
- [ ] Optional application-defined semantic adapters for Customer/Invoice/etc.
- [ ] Restricted data-query abstraction that can target application data, SQL Server, or MySQL without exposing arbitrary SQL by default.
- [ ] Cross-form context and memory under explicit scope/policy.

### Permission design

Automatic UI/data behavior should be configurable at a coarse level first:

```text
Automatic discovery
Read controls
Read data
Write controls
Invoke controls
```

Hosts may disable automatic behavior entirely and provide their own abstractions and authorization logic.

Database permissions are separate from UI permissions. The presence of provenance, a form attachment, or an agent instruction never grants authorization.

### UI automation

- [x] Permissions configuration page in AI Settings.
- [x] Persist permission policy through the public settings path.
- [ ] Form/UserControl/custom-control attachment and stable logical identity.
- [ ] Floating HAgent assistant/flyout.
- [ ] `ui.write_control`.
- [ ] `ui.move_control`.
- [ ] `ui.resize_control`.
- [ ] `ui.invoke` / approved click.
- [ ] Enable/disable operations.
- [ ] Batch operations.
- [ ] Dry-run/preview.
- [ ] Human approval.
- [ ] Per-control permissions.
- [ ] Undo/rollback hooks where host controls support them.

### Cross-platform direction

The boundary should remain provider-neutral so the same concepts can later be implemented by adapters for:

- HControl/BaseForm and custom controls.
- GDI-rendered objects and scenes.
- DirectX interactive objects.
- Unity components/scenes.
- Other interactive application surfaces.

These platform implementations belong outside `HAgent.Core`.

### Data representation rule

Always use the lightest representation that preserves the required information. Prefer bound/native sources, lazy adapters, projections, paging, and streaming. Avoid unnecessary copying/materialization. `DataTable` is valid when naturally present or actually useful, but it is never the mandatory representation.

## Example developer experience

Every Example feature should provide:

- editable input/message;
- expected behavior and explanation;
- copyable C# reproduction snippet beside the input;
- global agent selection where an agent is involved;
- a global output area when the result can be shared across examples;
- a self-contained setup snippet or a clearly identified shared setup section so a new developer can reproduce the example without guessing what `ai`, stores, providers, or adapters represent.

## Future

- [ ] Chat between user and selected agent.
- [ ] Agent-to-agent messaging board.
- [ ] Agent collaboration and handoffs.
- [ ] Tasks/workflows/background execution.
- [ ] Additional provider adapters.
- [ ] Extension/DLL ecosystem.
