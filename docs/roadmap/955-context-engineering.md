# Phase 0.955 — Context Engineering

## Status

**In progress — context contracts, acquisition, ranking, deterministic compaction, reusable caching, execution/provider integration, and bounded multi-resource retrieval are verified; policy/permission-aware assembly and the complete phase verification matrix remain.**

## Goal

Make context assembly a first-class HAgent subsystem that selects, ranks, bounds, compresses, and explains the information sent to an execution instead of treating prompt construction as string concatenation.

## Requirements

1. [x] Define provider-neutral context items with source, type, provenance, trust, importance, freshness, scope, and estimated size.
2. [x] Define context budgets for tokens/characters/items and other applicable resource dimensions.
3. [x] Separate context retrieval from context assembly and from cognitive attention.
4. [x] Support relevance ranking using goal relevance, attention, recency, importance, trust, redundancy, and estimated cost where available.
5. [ ] Support bounded memory, knowledge, skill, conversation, host-context, tool-description, and instruction retrieval.
6. [x] Support compaction, summarization, deduplication, and truncation strategies without silently discarding required policy or provenance.
7. [x] Preserve source/provenance metadata for assembled context and expose safe diagnostics explaining inclusion/exclusion.
8. [x] Support reusable and cacheable context components when configuration/version rules permit.
9. [x] Keep provider-specific tokenization behind optional adapters; Core must not require a particular tokenizer.
10. [ ] Ensure context assembly respects policy, permissions, disabled resources, and instruction authority.
11. [x] Capture the resulting bounded context in immutable execution snapshots.
12. [ ] Add deterministic Example verification for budgets, ranking, prioritization, compaction, source provenance, cache reuse, and policy-enforced exclusion.

## Verified slices

- Slice 2: provider-neutral context contracts and budgets — verified 2026-09-08 on .NET Framework 4.8.1 and .NET 9 Example execution.
- Slice 3: bounded acquisition and execution-owned context snapshots — verified 2026-09-08 with 20/20 HAgent.Tests and deterministic Example coverage.
- Slice 4: ranking, deterministic prioritization, and deduplication — verified 2026-09-08 with 24/24 HAgent.Tests and deterministic Example coverage.
- Slice 5: deterministic compaction/truncation and provenance-preserving diagnostics — verified 2026-09-09 with 29/29 HAgent.Tests and deterministic Example coverage.
- Slice 6: cache-safe reusable context components — verified 2026-09-09 with 34/34 HAgent.Tests and deterministic public-API Example coverage.
- Slice 7: execution/provider integration and request isolation — verified 2026-09-09 with 37/37 HAgent.Tests and deterministic public-API Context Execution Integration Example coverage.
- Slice 8: bounded multi-resource retrieval — verified 2026-09-09 with 41/41 HAgent.Tests and deterministic public-API Example coverage on .NET Framework 4.8.1 and .NET 9.

## Current implementation checkpoint

- Slice 9: policy/permission-aware context assembly — implementation checkpoint; local verification pending.
- The implementation composes the existing unified `IAiPolicyEngine` with the execution's effective `AiResourceCapabilitySnapshot`.
- Denied/approval-required/deferred sources are excluded before their source callback is queried; disabled sources are excluded before retrieval.
- Candidates are evaluated before global budget assembly so denied candidates cannot consume the bounded context budget.
- Admission decisions contain bounded metadata only and do not carry context payloads or translate policy into prompt text.

## Architectural outcome

```text
Available information
        ↓
Policy + permissions
        ↓
Attention / relevance
        ↓
Retrieval
        ↓
Policy-aware admission
        ↓
Ranking / deduplication
        ↓
Compression / compaction
        ↓
Bounded Context
        ↓
Execution Request
```

Context engineering remains distinct from cognitive decision making: cognition decides what matters; context engineering constructs the bounded evidence supplied to an execution.
