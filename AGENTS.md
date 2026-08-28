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

## UI rules

The WinForms UI is designer-free by design.

Use:

- clear hierarchy
- short field labels
- one-sentence descriptions beside important fields
- obvious primary actions
- disabled/empty states that explain what the user should do next
- consistent spacing
- DPI-aware sizing
- keyboard-friendly focus order

Avoid:

- giant property grids
- unexplained icons
- nested modal dialogs for routine navigation
- hidden provider/agent relationships
- putting secret values into ListView/DataGridView text

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

## Compatibility

Build all target frameworks before declaring a change complete. The repository may be edited on machines that have only Visual Studio or only the .NET CLI, so avoid relying on machine-local generated files.

The CI matrix should eventually cover:

- .NET Framework 4.8.1
- .NET 9

.NET 10 is a future upgrade target, not a current build target.

## Testing

Test domain behavior independently from WinForms. Network-provider tests should use a fake `HttpMessageHandler` or local test server; do not make unit tests call a real AI vendor.
