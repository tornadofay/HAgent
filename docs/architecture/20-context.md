# Context and host environment understanding

Context is information explicitly supplied or exposed by the host. HAgent does not assume that it owns a complete application or environment state.

## Context boundary

A host may expose bounded information from any environment through generic context mechanisms. Context may represent observations, state snapshots, events, records, objects, resources, or other information relevant to an LLM-driven execution.

HAgent must not require the host to convert every piece of context into a plain string message.

## Explicit host semantics

The host may deliberately expose semantic concepts through generic extension/adaptation boundaries. The host remains responsible for deciding what concepts are meaningful and what information may be exposed.

## Automatic discovery/adaptation

Optional adapters may inspect supported host surfaces through bounded structural discovery. Discovery can describe available structure and evidence without requiring Core to reference the host's concrete types.

Discovery is evidence about what exists. It is never authorization and it must not silently grant access to perform an operation.

## Data context

Data may be represented by native collections, projections, records, structured values, query results, or other supported representations. Use the lightest representation that preserves the information required for execution.

Structured data queries contain explicit fields, scalar filters, sorting, and bounded paging. They are intent, not raw model-generated SQL.

## Live application objects

Host-owned live objects may be attached or adapted through stable runtime identity and bounded structural discovery when the host explicitly enables the capability. HAgent does not need compile-time knowledge of application classes.

Live object access must remain non-executable unless an explicit tool/capability boundary authorizes an operation.

## Resource bounds

Automatic inspection is bounded. Object discovery uses explicit depth and collection limits; data reads and queries use explicit limits and paging; context passed to a model should be bounded to what the host has chosen to expose.

## Future contexts

The same generic context boundary must work across different project types and host technologies without changing HAgent.Core into a platform-specific implementation.

Examples of host context are categories of information, not required HAgent domain models. HAgent should provide the mechanisms for ingestion, bounding, normalization, provenance, and model-facing representation while the host retains semantic authority.
