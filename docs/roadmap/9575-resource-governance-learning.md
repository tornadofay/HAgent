# Phase 0.9575 — Knowledge, Skills, Memory Governance + Learning

## Status

**CLOSED — Slice 13 phase completion verification — 2026-09-12.**

Slices 1–13 are verified. Phase 0.9576 is now the current implementation phase.

## Goal

Complete HAgent's production V1 governance and learning boundary for first-class Skills, Knowledge/Wiki, Memory, and Learning resources without introducing a parallel resource architecture.

The canonical model remains:

```text
Skills    = reusable executable capability definitions
Knowledge = reusable retrievable information
Memory    = scoped experience/state
Learning  = governed process that turns experience into typed candidates
            and, when permitted, promotes them into authoritative state
```

Model output is never an authority. Learning creates typed candidates before authoritative promotion.

## Verified slices

### Slice 1 — Resource capability governance — VERIFIED 2026-09-09
Canonical ownership, effective capability state, and unified policy authorization.

Verification: 146/146 tests and required Examples on .NET Framework 4.8.1 and .NET 9.

### Slice 2 — Knowledge/Wiki governed resource contract — VERIFIED 2026-09-09
Explicit identity, scope/ownership, provenance, lifecycle/versioning, bounded metadata, relationships, chunks, and provider/index-neutral retrieval.

Verification: 153/153 tests and required Examples on both supported targets.

### Slice 3 — Versioned Skill definitions — VERIFIED 2026-09-09
Reusable versioned Skill definitions, explicit scope/ownership, lifecycle/provenance, dependencies, constraints, snapshot semantics, governed resolution, and runtime-owned handlers.

Verification: 158/158 tests and required Examples on both supported targets.

### Slice 4 — Memory family/type and provenance — VERIFIED 2026-09-09
Canonical family/type, provenance, expiration metadata, cloning/validation, and aligned persistence contracts.

Verification: 167/167 tests and required Examples on both supported targets.

### Slice 5 — Memory governance and retention — VERIFIED 2026-09-09
Generic resource capability governance, deterministic bounded retrieval/expiration filtering, retention caps, and governed memory-store decoration.

Verification: 175/175 tests and required Examples on both supported targets.

### Slice 6 — Learning Mode — VERIFIED 2026-09-09
Provider-neutral Learning Mode, persistent profile state, runtime override, and immutable execution-snapshot capture.

Verification: 181/181 tests and required Examples on both supported targets.

### Slice 7 — Learning Policy + Typed Candidates — VERIFIED 2026-09-11
One canonical Learning Policy and typed Memory/Knowledge/Skill learning candidates.

Verification: 187/187 tests; Learning Policy Example succeeded on both supported targets.

### Slice 8 — Canonical Learning Lifecycle Gate — VERIFIED 2026-09-11
Canonical admission gate combining candidate validation, Learning Policy, Learning Mode, and promotion authorization.

Verification: 194/194 tests; Learning Lifecycle Example succeeded on both supported targets.

### Slice 9 — Candidate persistence, retention, and review — VERIFIED 2026-09-11
Durable typed candidate records, File/InMemory stores, revision-safe review, retention/expiry, restart recovery, reviewer identity, policy evidence, and no authoritative publication at the review boundary.

Verification: 200/200 tests; Learning Candidate Persistence Example succeeded on both supported targets.

### Slice 10 — Authoritative promotion and version-safe resource creation — VERIFIED 2026-09-11
Canonical promotion service for Memory, Knowledge, and Skill, fresh authorization, publication-before-lifecycle transition, version conflict protection, provenance, and immutable published versions.

Verification: 205/205 tests; Learning Candidate Promotion Example succeeded on both supported targets.

### Slice 11 — Context, instruction, runtime, and observability integration — VERIFIED 2026-09-11
Governed learned instruction/context, bounded execution snapshots, authoritative runtime observations, and separation between observations, candidates, authorization, and prompt text.

Verification: 208/208 tests; Learning Execution Integration Example succeeded on both supported targets.

### Slice 12 — Management UI — VERIFIED 2026-09-12
Learning Review management workflow plus Authoritative Resource Inventory + Detail Inspection. The inventory increment used a real `InMemoryMemoryStore`, a provider-neutral Knowledge enumeration source, and a deterministic Skill projection; it provided bounded filters, deterministic ordering/paging, and readable Memory/Knowledge/Skill details. Skill generic storage enumeration remains intentionally deferred because its current contract is lookup-only.

Verification: `HAgent.Example → Authoritative Resource Inventory` succeeded on .NET Framework 4.8.1 and .NET 9. The management increment was regression-checked with the full test suite; the subsequent repository-wide result was 234/234 passed after the first 0.9576 slice.

### Slice 13 — Phase completion verification — VERIFIED 2026-09-12
Final phase verification confirmed evidence across the required 0.9575 boundaries: both supported targets for required Examples, candidate persistence/recovery, governed Memory/Knowledge/Skill promotion, immutable resource-version behavior, execution/runtime isolation and stale-result protection, context/instruction integration, runtime observations and policy/audit evidence, Learning Review management UI, and the final Authoritative Resource Inventory + Detail Inspection increment.

The 234/234 full `HAgent.Tests` result is green after the first 0.9576 implementation slice and provides the current regression gate over the completed 0.9575 implementation.

## Phase exit decision

**0.9575 is CLOSED.** Post-promotion reliability, adaptation, staleness, contradiction, quarantine, forgetting, replacement, and runtime reliability integration are owned by **0.9576** and are not backfilled into 0.9575.

## Phase exit criterion

Knowledge, Skills, Memory, and Learning are first-class production V1 resources with explicit identity, scope, ownership, provenance, capability policy, runtime overrides, immutable execution snapshots, governed retrieval, typed learning candidates, durable lifecycle, version-safe authoritative promotion, context/runtime integration, management UI, and auditable policy boundaries.
