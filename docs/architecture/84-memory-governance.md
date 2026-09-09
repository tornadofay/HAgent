# Memory Governance

## Purpose

Memory governance composes the existing generic resource capability system with a provider-neutral memory policy. It does not introduce a second authorization model or a second persistence model.

## Capability identity

Memory uses the generic tri-state capability contract with these canonical resource types:

```text
memory
memory.family
memory.type
```

The `memory` resource gates the capability as a whole. `memory.family` gates one `AiMemoryFamily`, and `memory.type` gates one stable `MemoryEntry.TypeId`. The existing `AiResourceCapabilitySnapshot` remains the immutable effective state for an execution/runtime boundary.

A disabled memory family/type is never made authoritative merely because a model requested it. The governed memory store evaluates the effective capability snapshot before writes and before requested family/type exposure.

## Query boundary

`MemoryQuery` supports explicit:

- `Family`
- `TypeId`
- `IncludeExpired`
- bounded `MaxResults`

Stores remain provider-neutral. The existing File, SQL Server, MySQL, and InMemory stores continue to use `IMemoryStore` and `MemoryEntry`; the governance decorator provides consistent cross-backend enforcement where a governed boundary is required.

## Retrieval policy

`AiMemoryGovernancePolicy` provides:

```text
Global MaxResults
ExcludeExpired
Family/type rules
```

A rule may target a family or an exact TypeId. Exact TypeId rules take precedence over family rules, and the global limit remains the final upper bound. Limits are bounded to 1..1000.

The governed store queries the underlying store through the existing contract with a bounded upper retrieval window, then applies capability and policy filtering before returning results. Unauthorized or expired records are never returned. Broad queries filter each returned record against its own effective family/type capability.

## Retention policy

A policy rule can define `RetentionDays`. At write time the governed store applies the policy as a maximum retention boundary:

```text
effective expiration = min(existing ExpiresAt, CreatedAt + policy retention)
```

An explicit shorter expiration is preserved. A policy never extends an existing expiration. Retention is expressed as metadata on `MemoryEntry`; physical deletion/compaction remains an implementation concern outside this slice.

## Separation of concerns

```text
AiAgent.ResourceCapabilities
        -> AiResourceCapabilitySnapshot

AiMemoryGovernancePolicy
        -> retrieval bounds + expiration bounds

AiGovernedMemoryStore
        -> compose both at the IMemoryStore boundary
```

Capability authorization and retention policy are separate concerns. The capability snapshot answers whether the memory resource/family/type is enabled; the memory policy answers how much may be retrieved and how long the record may remain valid.

## Provider neutrality

No GPU, vector database, embedding model, or large resident index is required. Governance operates on the existing `IMemoryStore` abstraction and the canonical `MemoryEntry` model.
