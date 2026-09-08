# Phase 0.955 — Context Engineering

## Status

**Verified and complete — the provider-neutral context pipeline, policy/capability admission, host authorization boundary, execution integration, and deterministic Example/test matrix are verified.**

## Goal

Make context assembly a first-class HAgent subsystem that selects, ranks, bounds, compresses, and explains the information sent to an execution instead of treating prompt construction as string concatenation.

## Requirements

1. [x] Define provider-neutral context items with source, type, provenance, trust, importance, freshness, scope, and estimated size.
2. [x] Define context budgets for tokens/characters/items and other applicable resource dimensions.
3. [x] Separate context retrieval from context assembly and from cognitive attention.
4. [x] Support relevance ranking using goal relevance, attention, recency, importance, trust, redundancy, and estimated cost where available.
5. [x] Support bounded memory, knowledge, skill, conversation, host-context, tool-description, and instruction retrieval through the provider-neutral multi-resource retrieval plan.
6. [x] Support compaction, summarization, deduplication, and truncation strategies without silently discarding required policy or provenance.
7. [x] Preserve source/provenance metadata for assembled context and expose safe diagnostics explaining inclusion/exclusion.
8. [x] Support reusable and cacheable context components when configuration/version rules permit.
9. [x] Keep provider-specific tokenization behind optional adapters; Core must not require a particular tokenizer.
10. [x] Ensure context assembly respects policy, permissions, disabled resources, and instruction authority. HAgent policy and resource-capability state are enforced by the canonical context admission boundary; protected data-backed context sources additionally compose the existing host-owned `IDataAccessAuthorizer`; instruction authority remains owned by the canonical 0.954 instruction-governance subsystem.
11. [x] Capture the resulting bounded context in immutable execution snapshots.
12. [x] Add deterministic Example verification for budgets, ranking, prioritization, compaction, source provenance, cache reuse, and policy-enforced exclusion through the verified Context Example matrix, including end-to-end assembly and host authorization.

## Verified slices

- Slice 2: provider-neutral context contracts and budgets — verified 2026-09-08 on .NET Framework 4.8.1 and .NET 9 Example execution.
- Slice 3: bounded acquisition and execution-owned context snapshots — verified 2026-09-08 with 20/20 HAgent.Tests and deterministic Example coverage.
- Slice 4: ranking, deterministic prioritization, and deduplication — verified 2026-09-08 with 24/24 HAgent.Tests and deterministic Example coverage.
- Slice 5: deterministic compaction/truncation and provenance-preserving diagnostics — verified 2026-09-09 with 29/29 HAgent.Tests and deterministic Example coverage.
- Slice 6: cache-safe reusable context components — verified 2026-09-09 with 34/34 HAgent.Tests and deterministic public-API Example coverage.
- Slice 7: execution/provider integration and request isolation — verified 2026-09-09 with 37/37 HAgent.Tests and deterministic public-API Context Execution Integration Example coverage.
- Slice 8: bounded multi-resource retrieval — verified 2026-09-09 with 41/41 HAgent.Tests and deterministic public-API Example coverage on .NET Framework 4.8.1 and .NET 9.
- Slice 9: policy/capability-aware context assembly admission — verified 2026-09-09 with 46/46 HAgent.Tests and deterministic public-API Context Policy Assembly Example coverage on .NET Framework 4.8.1 and .NET 9.
- Slice 10: end-to-end bounded context assembly pipeline — verified 2026-09-09 with 50/50 HAgent.Tests and deterministic public-API Context Assembly Example coverage on .NET Framework 4.8.1 and .NET 9.
- Slice 11: host authorization-aware context admission — verified 2026-09-09 with 56/56 HAgent.Tests and deterministic public-API Context Host Authorization Example coverage on .NET Framework 4.8.1 and .NET 9.

## Final implementation outcome

- The canonical context pipeline is provider-neutral and bounded:
  `retrieval plan → policy/capability admission → host authorization where required → ranking/deduplication → final compaction → ContextSnapshot`.
- Host authorization remains an explicit host-owned boundary. A policy allow does not imply host authorization, and protected sources fail closed when the host authorization boundary is unavailable or denies access.
- The context subsystem does not create a second policy engine or instruction precedence system. Instruction authority and conflict resolution remain owned by the canonical instruction-governance subsystem.
- Admission and compaction diagnostics remain metadata-only and preserve safe provenance without copying context payloads.
- The complete required deterministic Context Example/test matrix for 0.955 is covered by the verified slices above.

## Architectural outcome

```text
Available information
        ↓
Policy + permissions
        ↓
Attention / relevance
        ↓
Acquisition / retrieval
        ↓
Policy/capability admission
        ↓
Host authorization when the source requires it
        ↓
Ranking / deduplication
        ↓
Compression / compaction / truncation
        ↓
Bounded Context Snapshot
        ↓
Execution Request / Provider Adapter
```

Context engineering remains distinct from cognitive decision making: cognition decides what matters; context engineering constructs the bounded evidence supplied to an execution. Instruction authority remains owned by the canonical instruction-governance subsystem.
