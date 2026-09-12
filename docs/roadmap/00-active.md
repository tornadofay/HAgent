# HAgent Active Roadmap

## Current

### 0.9576 — Learned Resource Reliability + Adaptation

**Current milestone — Slice 1: Applicability and validity.**

The 0.9575 Knowledge, Skills, Memory Governance + Learning phase is closed through its Learning Review and Authoritative Resource Inventory management increments.

0.9576 establishes the governance needed after promotion and before reliability/adaptation changes resource behavior:

- determine whether a learned resource version is applicable to a bounded context;
- distinguish `Applicable`, `NotApplicable`, `Uncertain`, and `Invalidated`;
- preserve applicability evidence and evaluation results for observability;
- never treat applicability as authorization;
- never silently mutate a published resource version because of applicability evaluation;
- evaluate deterministic evidence before model reasoning whenever sufficient evidence exists;
- require governed replacement/new versions rather than unsafe in-place repair.

#### Slice 1 — Applicability and validity

Implemented on `master`:

- bounded provider-neutral applicability target/context/condition/evidence contracts;
- `AiApplicabilityOutcome` with four explicit outcomes;
- deterministic `IAiApplicabilityEvaluator` implementation;
- scope-aware applicability independent from capability authorization;
- missing deterministic evidence produces `Uncertain`, never `Applicable`;
- explicit invalidation outcome with bounded reason;
- decision records preserve resource version identity, condition results, evidence references, and evaluation time;
- focused tests and a dedicated WinForms Example scenario.

**Example to run:** `HAgent.Example → Learned Resource Applicability` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `HAgent.Tests → LearnedResourceApplicabilityTests.cs` focused, then the full suite.

Do not advance to Slice 2 until Slice 1 is user-verified on both supported targets.

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

- **Slice 2:** reliability evidence and validated outcome feedback.
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
