# AGENTS.md

This repository is designed to be worked on by both human developers and coding agents.

## Architecture rules

1. Keep `HAgent.Core` dependency-light and provider-neutral.
2. Do not put WinForms types in Core.
3. Do not put SQL/MySQL implementation details in Core.
4. Provider-specific HTTP logic belongs in provider adapter assemblies.
5. Secrets must use `ISecretStore`; never add an API-key property to persistent normal configuration objects.
6. A provider defines connection concerns and optional shared defaults. An agent defines behavior.
7. Avoid hidden configuration precedence. Any inheritance should be represented explicitly in the model and UI.
8. Public APIs should support cancellation tokens and remain async for network/database operations.
9. Preserve .NET Framework 4.8.1 compatibility for projects that currently target it.
10. Do not introduce a framework-sized dependency for a feature that can be implemented in a small adapter.
11. Runtime execution must operate from an execution snapshot so mutable/deleted configuration cannot invalidate active work.
12. Provider routing must be provider-neutral and must never assume OpenAI semantics in Core.
13. Memory must work without a local GPU and must not require a large resident RAM footprint. Embeddings/vector search are optional adapters, never core requirements.
14. Capabilities are discovered/declared explicitly. Never assume a discovered model supports chat, tools, vision, structured output, reasoning, embeddings, or streaming merely because the provider returned its model ID.
15. Capability claims should preserve provenance/evidence when practical: support state, source, confidence, observation time, and an optional explanatory note. A capability value without evidence must remain distinguishable from a provider-verified capability.
16. Tool execution is a host capability boundary. Models may request registered tools, but they never receive arbitrary reflection, process, file, database, control-tree, or memory access.
17. Guardrails, permissions, approval, budgets, and cancellation must be enforced outside the model's own instructions. A prompt saying “you are not allowed to do X” is not a security boundary.
18. Provider responses must have a provider-neutral representation that can carry text, structured output, tool calls/results, reasoning metadata when explicitly exposed, usage, and raw provider metadata without forcing every provider to implement every field.
19. Streaming is optional. Core contracts must support providers that stream and providers that do not.
20. Observability must be possible without logging secrets or full sensitive payloads by default. Diagnostics should use correlation IDs and configurable redaction.
21. Agent lifetime/scope is separate from agent profile. Do not create incompatible “agent types” merely for global/form/session/task use. Prefer explicit scope/binding concepts such as application, form, session, task, or ephemeral.
22. WinForms integration belongs in `HAgent.WinForms`, not Core. It should expose a host bridge that can attach AI capabilities to a `Form` or control tree without making `HAgentClient` depend on WinForms.
23. The WinForms integration feature is called **UI Context / Control Adapters**, not generic “form serialization.” Serialization is one operation produced by an adapter; reading and changing controls are explicit capabilities/tools.
24. UI adapters must understand common WinForms controls and data sources without requiring callers to manually convert them: `DataGridView`, `DataTable`, `BindingSource`, `CurrencyManager`, common list sources, `TextBox`, `ComboBox`, `Button`, `CheckBox`, `RadioButton`, `DateTimePicker`, `NumericUpDown`, `ListBox`, `TreeView`, and custom/user controls where an explicit adapter is available.
25. UI context must expose stable semantic identities and safe summaries, not raw object graphs. DataGridView access should prefer data-source extraction (`DataTable`, `BindingSource`, `IList`, etc.) before falling back to visible rows.
26. The WinForms host bridge must support an attached/flyout UI, but attaching an agent to a form must not automatically grant write/execute permission. Read, write, invoke, and data-export capabilities are separately controllable.
27. Cross-form memory must use memory ownership/scope and provenance. A form identifier may be metadata; it is not a substitute for a user/session/application security boundary.
28. Active runtime work must survive configuration edits/deletions safely. UI attachment removal, agent deletion, or form closure must have explicit behavior and must never silently invalidate running work.
29. `HAgent.Example` is the manual verification surface for every meaningful completed capability, including provider capability discovery, response normalization, tool execution, UI context, permissions, approvals, and agent collaboration.
30. The initial tool taxonomy is explicit: `BuiltIn`, `Application`, `Declarative`, `UI`, `SqlServer`, and `MySql`. `Extension` tools are deferred to a future extensibility milestone and must not be introduced into the initial runtime path.
31. Tool configuration defines the public contract and binding metadata; it must never interpret arbitrary configuration text as executable code. Executable behavior comes from a trusted built-in handler, application-registered handler, or dedicated restricted subsystem.

## Documentation is part of project state

`README.md`, `roadmap.md`, `plan.md`, and `AGENTS.md` are maintained project artifacts, not disposable documentation.

Whenever a meaningful feature, architecture, compatibility target, UI convention, milestone, or public API changes:

- update `plan.md` so the implementation state remains accurate;
- update `roadmap.md` when the long-term ordering/scope changes;
- update `README.md` when user-facing capabilities, usage, architecture, or supported targets change;
- update `AGENTS.md` when repository engineering rules or non-negotiable conventions change.

Never mark a feature complete in documentation unless the repository actually contains the implementation. Keep deferred/partial work explicitly marked as such.

## UI rules

The WinForms UI is designer-free by design, except for application-provided shared controls/helpers that are intentionally maintained separately.

Use:

- clear hierarchy
- short field labels
- one-sentence descriptions beside important fields
- obvious primary actions
- disabled/empty states that explain what the user should do next
- consistent spacing
- DPI-aware sizing
- keyboard-friendly focus order
- the shared `HAgent.WinForms.Helpers.Header` for HAgent form chrome
- the shared `HAgent.WinForms.Helpers.HMessage` API for user-facing dialogs
- the shared `HAgent.WinForms.Helpers.Button.HButton` for application buttons

### HMessage is mandatory for HAgent dialogs

Do not use `System.Windows.Forms.MessageBox` directly anywhere in HAgent.WinForms.

Use:

- `HMessage.ShowDelete(...)` for delete confirmations
- `HMessage.ShowQuestion(...)` for confirmations/questions
- `HMessage.ShowInformation(...)` for informational messages
- `HMessage.ShowError(...)` for user-facing errors
- `HMessage.ShowException(...)` when an exception should be presented with technical details

Keep destructive-operation confirmation at the UI boundary and enforce important data-integrity rules again in Core/storage.

### Header

Do not recreate another custom title bar for HAgent forms. Use `Header` through `HAgentForm`.

The HAgent `Header` is intentionally self-contained and must not depend on the larger `HLibraries` application framework. It should contain only window-header responsibilities: title/subtitle rendering, optional icon, dragging, and optional close/minimize/help actions.

### HButton

Use `HButton` for all HAgent action buttons. Do not introduce another button wrapper or fall back to the standard WinForms `Button` for application actions.

Keep the HAgent button palette aligned with the current AI visual identity: deep indigo/violet primary states, brighter violet hover/focus states, restrained red for destructive actions, and white text with sufficient contrast.

Avoid:

- giant property grids
- unexplained icons
- nested modal dialogs for routine navigation
- hidden provider/agent relationships
- putting secret values into ListView/DataGridView text
- copying unrelated application-framework dependencies into HAgent

## Manual example host

`HAgent.Example` is the manual integration and feature-verification application. It is not a replacement for `HAgent.Tests`.

The example host should:

- provide an obvious Configuration entry point
- exercise every meaningful completed feature with a small runnable example
- use the real public APIs, not internal test-only shortcuts
- remain usable as a developer smoke-test application as the runtime evolves
- avoid depending on external services unless the example explicitly says so

The Example form is intentionally split by responsibility as the test suite grows:

- `Program.cs` — application entry point only.
- `MainForm.cs` — form fields, constructor, main shell/layout, and global controls.
- `MainForm.Tabs.cs` — feature-tab construction and per-tab presentation.
- `MainForm.Context.cs` — agent selection, provider/agent prompt context, and context refresh.
- `MainForm.Tests.cs` — manual feature execution against the public HAgent APIs.
- `MainForm.Ui.cs` — shared Example UI helpers, execution wrapper, output handling, and small view models.

Do not grow `MainForm.cs` into a monolithic test harness. New feature examples should normally be added to the appropriate partial file or a new focused Example component when a feature becomes large enough to justify one.

When a new capability becomes complete, add a corresponding example to `HAgent.Example` before considering the developer experience complete.

## Provider adapter contract

Implement `IAiProviderAdapter`.

The adapter must:

- expose a stable `Kind` string
- accept provider and agent configuration
- receive the already-resolved secret
- support cancellation
- return `AIResponse`
- avoid mutating the stored provider or agent objects

## Storage contract

Implement `IAiStore` for persisted providers and agents.

If a backend needs secrets, prefer composing it with `ISecretStore` rather than storing raw secrets in the normal configuration table.

Provider deletion must not silently delete dependent agents. Data-integrity restrictions must be enforced by the storage implementation, not only by the UI.

## Runtime contract

Runtime responsibilities include:

- execution lifecycle/state
- provider routing/fallback
- capability selection/compatibility
- cancellation and timeout boundaries
- execution snapshots
- memory/context integration
- tool execution integration
- guardrails/permissions/approval boundaries
- budgets and loop limits
- structured failure reporting
- correlation/observability hooks

The runtime must not hide cancellation, timeout, or tool failures as ordinary provider fallbacks.

## WinForms host bridge contract

The future WinForms host bridge must remain outside Core and should expose concepts such as:

```text
Attach(Form)
Detach()
InspectControl(...)
ReadControl(...)
WriteControl(...)
InvokeControl(...)
ReadData(...)
ShowAssistant(...)
```

Exact names may change, but the separation must remain: **UI context/introspection** describes what exists; **tools/capabilities** define what may be done.

The bridge should support form-bound AI assistants and cross-form memory without forcing every attached form to share one global conversation. Form, session, task, and application relationships should be explicit.

## Compatibility

Build all target frameworks before declaring a change complete. The repository may be edited on machines that have only Visual Studio or only the .NET CLI, so avoid relying on machine-local generated files.

The CI matrix should eventually cover:

- .NET Framework 4.8.1
- .NET 9

.NET 10 is a future upgrade target, not a current build target.

## Testing

Test domain behavior independently from WinForms. Network-provider tests should use a fake `HttpMessageHandler` or local test server; do not make automated tests call a real AI vendor.

For this development workflow, do not claim local build/test success unless it was actually executed. The developer machine is the authoritative VS 2022 build/test environment when local tool access is unavailable.
