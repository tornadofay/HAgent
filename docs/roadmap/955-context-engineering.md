# Phase 0.955 — Context Engineering

## Status

**Planned architectural foundation before advanced persistent cognition.**

## Goal

Make context assembly a first-class HAgent subsystem that selects, ranks, bounds, compresses, and explains the information sent to an execution instead of treating prompt construction as string concatenation.

## Requirements

1. [ ] Define provider-neutral context items with source, type, provenance, trust, importance, freshness, scope, and estimated size.
2. [ ] Define context budgets for tokens/characters/items and other applicable resource dimensions.
3. [ ] Separate context retrieval from context assembly and from cognitive attention.
4. [ ] Support relevance ranking using goal relevance, attention, recency, importance, trust, redundancy, and estimated cost where available.
5. [ ] Support bounded memory, knowledge, skill, conversation, host-context, tool-description, and instruction retrieval.
6. [ ] Support compaction, summarization, deduplication, and truncation strategies without silently discarding required policy or provenance.
7. [ ] Preserve source/provenance metadata for assembled context and expose safe diagnostics explaining inclusion/exclusion.
8. [ ] Support reusable and cacheable context components when configuration/version rules permit.
9. [ ] Keep provider-specific tokenization behind optional adapters; Core must not require a particular tokenizer.
10. [ ] Ensure context assembly respects policy, permissions, disabled resources, and instruction authority.
11. [ ] Capture the resulting bounded context in immutable execution snapshots.
12. [ ] Add deterministic Example verification for budgets, ranking, prioritization, compaction, source provenance, cache reuse, and policy-enforced exclusion.

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
Ranking / deduplication
        ↓
Compression / compaction
        ↓
Bounded Context
        ↓
Execution Request
```

Context engineering remains distinct from cognitive decision making: cognition decides what matters; context engineering constructs the bounded evidence supplied to an execution.