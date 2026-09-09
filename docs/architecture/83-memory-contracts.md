# Memory Contracts

## Purpose

This document defines the normalized provider-neutral contract for HAgent Memory records. It extends the existing `MemoryEntry` model rather than introducing a second persisted memory object.

## Families and types

`AiMemoryFamily` identifies the broad semantic role of a record:

```text
Working    execution-local working state
Episodic   experiences/events that happened
Semantic   generalized facts/preferences/knowledge derived from experience
Procedural  learned procedures/strategies
Custom     application-specific future memory families
```

`TypeId` is a stable bounded identifier within a family namespace. Built-in families use reserved prefixes (`working.`, `episodic.`, `semantic.`, `procedural.`). Custom types use an application-specific namespace so future memory types do not require a new persisted C# class.

Family and type are descriptive contract state. They do not by themselves grant retrieval or mutation authority.

## Provenance and confidence

Each memory record carries `AiMemoryProvenance`. Provenance identifies source kind, source identity, optional URI/creator information, originating execution/runtime identities, evidence, and confidence.

Model-generated provenance is explicitly descriptive. The fact that a memory came from a model does not make it authoritative.

Confidence is bounded to the inclusive range 0..1. Evidence and source identifiers are bounded strings so malformed host/model data cannot create unbounded records.

## Expiration

`MemoryEntry.ExpiresAt` is optional metadata. `IsExpired(...)` provides deterministic point-in-time evaluation. This slice defines the metadata contract only; retention-policy enforcement remains a later governance slice.

An occurrence may legitimately precede record creation because imported or reconstructed experience can be historical. The contract therefore does not require `OccurredAt >= CreatedAt`.

## Validation and ownership

`MemoryEntry.Validate()` validates family/type namespaces, required identity/content metadata, bounded metadata/provenance, confidence, timestamps, and expiration state. It does not perform authorization.

The existing memory ownership/scope mechanism remains separate from the family/type model. Later governance uses the generic resource-authorization boundary rather than embedding authorization rules in the memory record.

## Immutability and cloning

`MemoryEntry.Clone()` creates an independent copy of nested metadata and provenance. Execution/runtime and governance layers may use this boundary to prevent caller-owned mutable objects from changing captured memory state.

## Storage and provider neutrality

File, SQL Server, MySQL, and in-memory implementations continue to persist and transport the same `MemoryEntry` representation. No provider SDK, vector database, embedding model, or GPU dependency is introduced by the family/type contract.

The existing `IMemoryStore` remains the storage boundary. Later slices may add family/type filtering, governed retrieval, retention, and capability enforcement to that boundary without creating a parallel store abstraction.

## Security rule

Memory family/type, provenance, confidence, and expiration are metadata and evidence. They are never authorization grants. Retrieval, exposure, deletion, and mutation remain subject to the applicable resource governance and host authorization boundaries.
