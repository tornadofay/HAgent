# Active and near-term roadmap

## 0.5 Tools + Agent Loop

Complete next:

- [ ] Per-agent tool selection UX backed by persisted definitions.
- [ ] Per-session temporary tools.
- [ ] Built-in tool handlers.
- [ ] Application tool registration guidance/API conventions.
- [ ] Declarative execution engine.
- [ ] Tool aliases/versioning.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool audit/history and budgets.
- [ ] Advanced loop detection and policy controls.
- [ ] Provider/tool capability negotiation beyond basic tool calling.
- [ ] SQL Server and MySQL tool execution layers.
- [ ] Tool configuration refinement and richer predefined/custom behavior.

## 0.7 WinForms UI Context + Automation

- [ ] Attach/detach `Form`, `UserControl`, and custom controls.
- [ ] Stable control identity and semantic discovery.
- [ ] Safe UI state snapshots and provider-neutral context.
- [ ] Lazy/native data-source adapters.
- [ ] DataGridView, BindingSource, CurrencyManager, IList and tabular source support.
- [ ] TextBox/RichTextBox/ComboBox/Button/CheckBox/RadioButton/DateTimePicker/NumericUpDown/ListBox/ListView/TreeView adapters.
- [ ] Public `HAgentHost.Attach(ai, form)`-style bridge.
- [ ] Floating assistant/flyout.
- [ ] UI tools: inspect, read, read_data, write, move, resize, invoke, enable/disable, batch.
- [ ] UI-thread dispatch, dry-run, undo hooks, and per-control permissions.

### Data representation rule

Always use the lightest representation that preserves the information needed by the operation. Prefer bound/native sources; adapt lazily; avoid unnecessary copies. `DataTable` is optional, not the architectural default.
