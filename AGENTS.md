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
- cancellation and timeout boundaries
- execution snapshots
- memory/context integration
- tool execution integration
- structured failure reporting

The runtime must not hide cancellation, timeout, or tool failures as ordinary provider fallbacks.

## Compatibility

Build all target frameworks before declaring a change complete. The repository may be edited on machines that have only Visual Studio or only the .NET CLI, so avoid relying on machine-local generated files.

The CI matrix should eventually cover:

- .NET Framework 4.8.1
- .NET 9

.NET 10 is a future upgrade target, not a current build target.

## Testing

Test domain behavior independently from WinForms. Network-provider tests should use a fake `HttpMessageHandler` or local test server; do not make unit tests call a real AI vendor.
