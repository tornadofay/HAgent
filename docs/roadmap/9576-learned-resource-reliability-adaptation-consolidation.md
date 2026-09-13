# Phase 0.9576 — Learned Resource Reliability, Adaptation + Forgetting

## Status

**VERIFIED — all five slices complete.**

## Purpose

Provide the post-promotion reliability layer for learned Skills, Knowledge, and other learned resources.

0.9575 governs how experience becomes a validated candidate and how that candidate is promoted. 0.9576 governs whether the promoted resource remains safe, applicable, useful, and appropriately retained afterward.

This phase is deliberately practical: it provides lifecycle/reliability controls needed by production V1 without turning HAgent into a research system for large-scale knowledge consolidation or neural continual learning.

## V1 outcome

A promoted resource is never trusted forever merely because it was once approved.

HAgent distinguishes:

```text
Authorization      = may this resource be used?
Applicability      = does it fit this situation?
Reliability        = how much evidence supports continued trust?
Lifecycle          = is it active, stale, quarantined, archived, retired?
Retention          = should it remain available, be archived, or be retired?
```

These dimensions remain separate.

## Delivery slices

### Slice 1 — Applicability and validity — VERIFIED

Verified by the user on 2026-09-12 on .NET Framework 4.8.1 and .NET 9.

- provider-neutral applicability results: `Applicable`, `NotApplicable`, `Uncertain`, `Invalidated`;
- bounded applicability conditions, preconditions, scope, and evidence references;
- deterministic evaluation before model reasoning when sufficient evidence exists;
- applicability independent from authorization/capability state;
- applicability decisions preserve resource-version identity, condition results, and evidence references;
- dedicated tests and Example verification.

### Slice 2 — Reliability evidence and outcome feedback — VERIFIED

Verified by the user on 2026-09-13 on .NET Framework 4.8.1 and .NET 9. The user also reported the full `.NET 9` `HAgent.Tests` regression result as 242/242 passed, 0 failed, 0 skipped.

- `AiResourceReliabilityIdentity` keys evidence to exact resource type, ID, published version, and scope;
- promotion evidence remains distinct from operational outcome evidence;
- only host-validated outcomes can change reliability;
- bounded reliability score and outcome counters;
- review and quarantine recommendations remain separate from lifecycle mutation;
- provider-neutral compare-and-swap persistence;
- policy-controlled reliability updates;
- success/failure/invalid-precondition/contradiction evidence with deterministic score deltas;
- execution/runtime/agent/evaluation provenance preserved;
- no authoritative resource version mutation.

### Slice 3 — Staleness, contradiction, and revalidation — VERIFIED

User verified `HAgent.Example → Cognition → Learning → LEARNED RESOURCE ADAPTATION` on .NET Framework 4.8.1 and .NET 9 on 2026-09-13. The user also reported the full `.NET 9` `HAgent.Tests` regression suite at **252/252 passed, 0 failed, 0 skipped**.

Implemented and verified:

- deterministic distinction between age-based staleness, observed degradation, contextual drift, and direct contradiction;
- bounded lifecycle states `Active`, `UnderReview`, `Quarantined`, and `Retired`;
- explicit automatic-use gate requiring `Active + Current`;
- existing Evaluation and Applicability contracts are consumed rather than duplicated;
- lifecycle transitions are policy-controlled and compare-and-swap revision-safe;
- clean revalidation can recover review/quarantine state while retired resources remain retired;
- replacement/revision is represented as a new typed learning candidate;
- lifecycle history, reliability/applicability/evaluation signals, and resource-version identity remain bounded and preserved;
- published resource versions remain unchanged.

### Slice 4 — Forgetting and archival — VERIFIED

User verified `HAgent.Example → Cognition → Learning → LEARNED RESOURCE RETENTION` on .NET Framework 4.8.1 and .NET 9 on 2026-09-13. The user reported the full `.NET 9` `HAgent.Tests` result at **256/256 passed, 0 failed, 0 skipped** after correcting the test's invalid timeline fixture.

Implemented and verified:

- bounded utility, validated-use, freshness, supersession, contradiction, retention, retirement, and authority signals;
- recoverable `Archived` lifecycle state;
- deterministic archive/retire/restore decisions;
- higher-authority preservation;
- policy operation `resource.lifecycle.retention`;
- compare-and-swap lifecycle mutation and bounded retention provenance;
- authoritative resource definitions remain unchanged;
- matching Example registered through the existing Learning registration path.

### Slice 5 — Runtime integration and verification — VERIFIED

User verified `HAgent.Example → Cognition → Learning → LEARNED RESOURCE RUNTIME INTEGRATION` on .NET Framework 4.8.1 and .NET 9 on 2026-09-13.

The user reported the full `.NET 9` `HAgent.Tests` result as **260/260 passed, 0 failed, 0 skipped**.

Implemented and verified:

- reliability/applicability/retention outcomes exposed through the established resource and policy boundaries;
- execution-owned learned-resource admission state captured in an immutable execution snapshot;
- exact resource identity and reliability/lifecycle revision captured for deterministic reproducibility;
- stale reliability revisions rejected rather than overwriting newer state;
- uncertain learned-resource applicability falls back to a host-owned deterministic/fallback decision rather than unsafe automatic use;
- runtime use is policy-controlled;
- published resource versions remain unchanged.

## V1 safety rules

1. Reliability never grants authorization.
2. An authorized resource can still be inapplicable or invalidated.
3. `Uncertain` never means `Applicable`.
4. Published Skill and Knowledge versions are not silently mutated.
5. Reliability and retention updates are revision-safe and auditable.
6. Learned-resource failure never forces an unsafe fallback.
7. Reliability remains usable without GPU, embeddings, or vector databases.
8. Reliability operates on already-promoted resource versions; it does not bypass the 0.9575 candidate and promotion boundary.
9. Replacement, adaptation, retention, and retirement produce governed state/candidates rather than hidden in-place mutation.
10. Retention protects higher-authority resources when lower-utility duplicates exist.

## Ownership boundary

0.9575 owns candidate creation, review, authorization, and authoritative promotion.

0.9576 owns post-promotion applicability, reliability evidence, degradation/quarantine, revalidation, forgetting, archival, replacement signals, and runtime reliability integration.

0.958 owns runtime lifecycle/health; it may consume reliability evidence but does not become the learned-resource evaluator.

0.97 consumes reliable learned behavior during cognition but does not create a second reliability or learning architecture.

## Dependency chain

```text
0.9575 governed learning + promotion
        ↓
0.9576 learned resource reliability + adaptation + retention
        ↓
0.958 lifecycle + health
        ↓
0.97 deterministic learned behavior + deliberation
```

## Exit criterion

HAgent can determine whether promoted learned behavior is applicable and trustworthy, refuse or quarantine stale/contradictory behavior, incorporate validated outcomes into reliability evidence, produce replacement candidates without mutating authoritative versions, safely forget/archive learned resources, and integrate learned-resource state into runtime admission and execution snapshots without confusing reliability with authorization or bypassing governed promotion.
