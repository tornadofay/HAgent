# HAgent Active Roadmap

## Current

### 0.9576 — Learned Resource Reliability + Adaptation

**Slice 2 — Reliability evidence and validated outcome feedback — VERIFIED.**

The 0.9575 Knowledge, Skills, Memory Governance + Learning phase is closed through its Learning Review and Authoritative Resource Inventory management increments.

0.9576 establishes the governance needed after promotion and before broader reliability/adaptation changes resource behavior:

- determine whether a learned resource version is applicable to a bounded context;
- distinguish `Applicable`, `NotApplicable`, `Uncertain`, and `Invalidated`;
- preserve applicability evidence and evaluation results for observability;
- record post-promotion reliability evidence separately from promotion evidence;
- accept only host-validated operational outcomes as reliability feedback;
- strengthen or weaken reliability through a policy-controlled, revision-safe boundary;
- never treat reliability as authorization;
- never silently mutate a published resource version because of reliability evaluation.

#### Slice 1 — Applicability and validity — VERIFIED

User-verified on 2026-09-12 on .NET Framework 4.8.1 and .NET 9.

**Example:** `HAgent.Example → Learned Resource Applicability`.

**Tests:** `HAgent.Tests → LearnedResourceApplicabilityTests.cs`, followed by the full suite; the user reported **234/234 passed** on .NET 9 after the slice.

#### Slice 2 — Reliability evidence and outcome feedback — VERIFIED

User-verified on 2026-09-13 on .NET Framework 4.8.1 and .NET 9.

The user reported **242/242 passed, 0 failed, 0 skipped** for the full `.NET 9` `HAgent.Tests` suite.

Verified implementation:

- `AiResourceReliabilityIdentity` keyed by resource type/ID/version/scope;
- separate promotion and operational evidence collections;
- `AiValidatedResourceOutcome` requiring explicit host validation metadata;
- bounded reliability score, outcome counters, review flag, and quarantine recommendation;
- deterministic outcome deltas for success, failure, invalid precondition, and contradiction;
- `IAiResourceReliabilityStore` with revision-safe compare-and-swap updates;
- deterministic `InMemoryAiResourceReliabilityStore` implementation;
- `AiResourceReliabilityService` with policy-controlled outcome updates;
- execution/runtime/agent/evaluation provenance preservation;
- no mutation of the promoted resource version.

**Example verified:** `HAgent.Example → Cognition → Learning → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9.

**Focused tests:** `HAgent.Tests → LearnedResourceReliabilityTests.cs`.

The Example organization was also hardened so feature/subgroup classification is explicit and an unclassified new example fails closed instead of silently entering Diagnostics.

## 0.9575 — Knowledge, Skills, Memory Governance + Learning — CLOSED

Completed and user-verified through 2026-09-12:

- resource capability governance;
- Knowledge/Wiki and Skills contracts;
- Memory families/types, provenance, governance, and retention;
- Learning Mode, policy, typed candidates, lifecycle, persistence, review, and promotion;
- learning Context/Instruction/Runtime/Observability integration;
- Learning Review management UI;
- Authoritative Resource Inventory + Detail Inspection.

The final inventory increment was verified on 2026-09-12 on both .NET Framework 4.8.1 and .NET 9, including real Memory inventory adaptation, provider-neutral Knowledge enumeration, authoritative-only filtering, deterministic ordering/paging, and readable Memory/Knowledge/Skill resource details.

## Planned order

### 0.9576 remaining slices

- **Slice 3:** staleness, contradiction, drift, and revalidation.
- **Slice 4:** quarantine, retirement, archival, and forgetting.
- **Slice 5:** governed runtime integration and end-to-end verification.

### 0.958 — Agent Lifecycle + Health

- Extend runtime lifecycle for long-lived agents.
- Add runtime health state.
- Add bounded progress and recovery signals.
- Add lifecycle/health observability and verification.

### 0.9591 — Goal/Plan Persistence + Recovery

- Durable goals and intentions.
- Durable plans and plan steps.
- Checkpoints and outcome states.
- Retry/idempotency records.
- Restart recovery and stale-revision protection.
- File, SQL Server, and MySQL verification.
