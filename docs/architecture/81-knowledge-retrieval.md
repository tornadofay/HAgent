# Knowledge and Wiki Resource Contracts

This document defines the provider-neutral Knowledge/Wiki contract consumed by Phase 0.9575. It complements `docs/architecture/80-knowledge-memory-learning.md` and does not replace the generic resource-governance boundary.

## Resource model

`AiKnowledgeResource` is the managed resource representation for reusable retrievable information. Its `Kind` distinguishes:

```text
Knowledge = general reusable information
Wiki      = managed persistent knowledge source
```

The contract preserves:

- stable resource identity;
- explicit `AgentResourceScope` and owner identity;
- title, summary, and bounded content;
- lifecycle state (`Draft`, `Published`, `Archived`);
- monotonic version value supplied by the authoritative resource owner;
- source identifier and detailed provenance;
- bounded metadata, tags, and categories;
- explicit relationships to other resource identities;
- creation/update timestamps.

A resource is authoritative only when it is explicitly `Published`. New resources default to `Draft`, so model-generated or otherwise unreviewed content cannot become authoritative merely by being represented as a resource.

## Provenance

`AiKnowledgeProvenance` is retained with the resource and copied through clones/retrieval results. It may identify:

- provenance kind (`HostProvided`, `UserProvided`, `Imported`, `ModelGenerated`, `SystemGenerated`);
- source/source ID/source URI;
- creator identity when supplied by the host;
- source execution and runtime identities;
- evidence and bounded confidence.

Provenance is descriptive evidence. It does not grant authorization and does not make model output authoritative.

## Relationships

`AiKnowledgeRelationship` stores a typed relationship to another resource identity rather than embedding the target resource. Relationships are bounded and remain provider-neutral so a future Knowledge Manager can add graph-like navigation without coupling Core to a graph database.

## Retrieval boundary

`IAiKnowledgeRetriever` is the provider/index-neutral retrieval interface:

```text
AiKnowledgeRetrievalRequest
        ↓
IAiKnowledgeRetriever
        ↓
AiKnowledgeRetrievalResult
```

The contract does not prescribe keyword search, semantic search, embeddings, vector indexes, relational queries, or hybrid ranking. Those implementations remain replaceable behind the interface.

Retrieval requests carry an explicit query, optional resource/type filters, host identity, bounded result/character/chunk limits, draft inclusion, and bounded metadata filters. The operation is asynchronous and cancellation-aware.

Retrieval candidates retain the source `AiKnowledgeResource`, the selected `AiKnowledgeChunk`, and a provider-neutral score. The resource and chunk are copied into returned results rather than exposing the retriever's mutable internal state.

## Governance composition

`AiGovernedKnowledgeRetriever` is the composition boundary between resource governance and a retrieval implementation. It does not implement authorization itself. Instead it:

1. applies the caller's request filters;
2. excludes draft resources unless explicitly requested;
3. submits each eligible resource to `AiResourceGovernanceEvaluator` using the caller identity;
4. forwards only admitted resource IDs to the underlying retriever;
5. returns the underlying provider/index-neutral retrieval result.

Therefore the authority chain remains:

```text
Caller identity
    ↓
Canonical resource ownership
    ↓
Effective resource capability
    ↓
Unified policy decision
    ↓
Knowledge retrieval implementation
```

A retrieval implementation must never be treated as an authorization boundary. The governed wrapper exists to make the intended ordering explicit at the resource-to-retrieval boundary.

## Version and snapshot semantics

Knowledge resources are versioned authoritative records. Retrieval returns a detached resource representation so subsequent edits to the source object do not mutate an already-created retrieval result.

Execution snapshot integration remains the responsibility of the higher-level context/runtime pipeline. The Knowledge contract intentionally does not introduce a second execution snapshot model.

## Safety boundaries

- Prompt text and model output are not authorization mechanisms.
- Model-generated knowledge starts as non-authoritative `Draft` state.
- `Published` is an explicit lifecycle state; promotion remains governed by later learning/approval workflows.
- Non-global resource access continues to require canonical owner identity through `AiResourceGovernanceEvaluator`.
- Retrieval limits are explicit and bounded; retrieval implementations remain responsible for honoring those limits.
- Core contains no SQL Server/MySQL, vector-database, embedding, or WinForms implementation details.

## Persistence boundary

Phase 0.9575 Slice 2 defines the contract only. Existing HAgent Wiki document/chunk storage schemas remain persistence substrates. Repository/backend wiring, migrations, relationships persistence, and Knowledge Manager CRUD are later slices and must consume these contracts rather than invent alternate Knowledge models.
