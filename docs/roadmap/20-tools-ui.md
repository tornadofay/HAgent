# Active and near-term roadmap

## 0.5 Tools + Agent Loop

### Completed

- Definition/handler separation.
- Registry and direct execution.
- JSON Schema validation before execution.
- Provider tool-definition transport.
- Bounded multi-turn tool loop.
- File tool-definition persistence.
- Six initial tool categories: BuiltIn, Application, Declarative, UI, SqlServer, MySql.
- Per-agent tool selection using persisted `ToolIds`.
- Agent/tool persistence verification.
- Live Groq tool-loop verification.

### Next

- Per-session temporary tools.
- Built-in tool handlers beyond the initial UI read-only tools.
- Declarative execution engine.
- SQL Server tool execution layer.
- MySQL tool execution layer.
- Tool timeout/cancellation/progress policies.
- Tool audit/history and budgets.
- Stronger loop detection and policy controls.
- Provider/tool capability negotiation beyond basic tool calling.

Extension tools are deliberately deferred to the later extensibility milestone.

## 0.7 WinForms UI Context + Automation

### Implemented foundation

- `IUiContext` contract.
- `WinFormsUiContext` attach/inspect/read operations.
- Stable lookup by WinForms control name.
- Lightweight form/control snapshots.
- Common scalar-control value extraction.
- DataGridView bound-source extraction.
- DataTable/DataView/enumerable handling when naturally available.
- Bounded row reads.
- Read-only `ui.inspect`, `ui.read_control`, and `ui.read_data` tools.
- `HAgentHost.Attach(form, registry)` bridge.
- Example verification for the UI Context layer.

### Next

- Rich BindingSource/CurrencyManager/IList adapters.
- Better semantic identities for UserControl/custom controls.
- Public attach/detach lifetime management.
- Floating assistant/flyout attached to a form.
- `ui.write_control`.
- `ui.move_control`.
- `ui.resize_control`.
- `ui.invoke` / approved click.
- Enable/disable operations.
- Batch operations.
- UI-thread dispatch with real host cancellation.
- Dry-run/preview and undo hooks.
- Per-control permissions and human approval.

### Data representation rule

Always use the lightest representation that preserves the information required by the current operation. Prefer bound/native sources; adapt lazily; avoid unnecessary copies/materialization. `DataTable` is optional, never the architectural default. Large sources should use paging, projection, or streaming when appropriate.
