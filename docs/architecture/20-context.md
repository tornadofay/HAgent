# Context and application understanding

Context is information explicitly supplied by the host. HAgent does not assume that it owns a complete application or environment state.

## UI Context

`HAgent.WinForms` can attach to a Form or arbitrary control tree and inspect controls, bindings, data sources, and semantic descriptors.

## Application context

Host-owned live objects can be attached by stable runtime ID and inspected through bounded structural discovery. HAgent does not need compile-time knowledge of application classes.

## Data context

Data sources may be native lists, BindingSource, DataView, DataTable, arrays, or other supported sources. Use the lightest representation that preserves required information.

Structured data queries contain explicit fields, scalar filters, sorting, and bounded paging. They are intent, not SQL.

## Adapters

Application-specific interfaces such as an external `IHyperControl` may be recognized through explicit adapters or supported member shapes. HAgent must not reference application assemblies from Core.

## Semantic truth

A discovered name, binding, property, or relationship is evidence. It is not automatically authorization or guaranteed business meaning. Explicit developer-provided semantics take precedence when available.

## Resource bounds

Automatic inspection is bounded. Object discovery uses `maxDepth` and `maxCollectionItems`; data reads and queries use explicit limits and paging.

## Future contexts

The same context boundary can later support non-WinForms surfaces, simulations, games, images, or other host-provided observations without changing Core into a platform-specific implementation.
