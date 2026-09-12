# HAgent Active Roadmap

## Current

### 0.9575 — Knowledge, Skills, Memory Governance + Learning

**Current milestone — Slice 12 management work continues.**

- Complete the canonical learning lifecycle gate.
- Persist learning candidates with retention and expiry.
- Add Learning Review workflow and durable review state.
- Promote approved Memory candidates into authoritative Memory.
- Promote Knowledge candidates as new authoritative versions.
- Promote Skill candidates as new immutable versions.
- Persist promotion provenance, evaluation, and audit records.
- Integrate governed learned resources with Context and Instruction.
- Capture execution outcomes and observations as learning input.
- Add learning/resource observability.
- Add Learning Review, Knowledge/Wiki, Skill, and Agent Configuration UI.
- Verify persistence, restart/recovery, lifecycle, authorization, concurrency, context integration, UI, and audit behavior on both supported targets.

#### Learning lifecycle

```text
Proposed
   ↓ lifecycle / policy evaluation
PendingReview
   ├──→ Rejected
   ↓
Approved
   ↓ governed authoritative promotion
Promoted
```

- `Proposed`: candidate formed; review has not necessarily been required yet.
- `PendingReview`: lifecycle/policy requires the review boundary.
- `Approved`: accepted through the review boundary, but still a candidate rather than an authoritative resource.
- `Rejected`: not accepted for authoritative promotion.
- `Promoted`: approved candidate converted into the authoritative resource through the separate promotion capability.

Learning mode and policy control the permitted paths. Review and promotion are intentionally separate boundaries.

### Slice 12 — Management UI — VERIFIED through Learning Review promotion

Verified by user on 2026-09-11 on both .NET Framework 4.8.1 and .NET 9:

- durable Learning Review list and review actions;
- host-supplied read-only reviewer identity;
- candidate-store injection aligned with host storage configuration;
- filterable candidate workspace by lifecycle status and candidate type;
- read-only candidate details including payload, provenance/evidence, lifecycle, policy, source execution/runtime, and review evidence;
- governed Promote action for selected `Approved` candidates through the injected `AiLearningPromotionService`;
- fresh `learning.promote` authorization remains inside the existing promotion service;
- successful promotion refreshes the candidate to `Promoted` revision `3`;
- `Learning Review Seed` → real WinForms configuration flow → Approve → Promote → `Learning Review Verify` succeeded on both targets;
- fresh candidate-store reopen and persisted reviewer identity/policy evidence were verified on both targets.

The Learning Review management boundary is closed.

### Current Slice 12 management work — Authoritative Resource Inventory + Detail Inspection

Implemented and now extending the inventory foundation into a scalable read/inspect management surface:

- one provider-neutral `IAiResourceInventory` / `IAiResourceInventorySource` boundary;
- bounded resource inventory query/projection contracts;
- deterministic aggregation, filtering, authoritative-only selection, logical-resource normalization, highest-version selection, ordering, and result bounds;
- lifecycle/version/updated-time/owner/agent, type, scope, search, and authoritative-only filtering;
- deterministic offset paging through `SkipResults` + `MaxResults`;
- focused `ResourceInventoryTests` coverage for filtering, normalization, bounds, and paging;
- canonical Example: `HAgent.Example → Authoritative Resource Inventory`;
- WinForms page: `Configuration → Authoritative Resources` consuming the same inventory contract;
- persistent master resource list with selected-resource inspection kept in a separate detail pane;
- user-resizable SplitContainer starting near a 55/45 list/detail balance;
- Overview/Content tabs contained inside the detail pane rather than replacing the resource list;
- filter/action region for Search, Resource type, Agent/Owner, Scope, Lifecycle, Version, Updated window, Authoritative-only, Apply/Reset/Refresh, and bounded page navigation;
- provider-neutral `IAiResourceDetailSource` / `AiResourceDetail` read boundary for actual resource content;
- readable Content view plus bounded type-specific fields/sections for Memory, Knowledge/Wiki, Skill, and future resource families;
- focused `ResourceDetailInspectionTests` coverage;
- Example detail projections wired through the real Configuration composition;
- no SQL Server/MySQL enumeration implementation;
- resource-specific editing, governed version creation, and lifecycle/CRUD workflows remain subsequent management work;
- reliability/adaptation remains separate from this read-management increment.

The inventory foundation was verified by the user on 2026-09-11 on both supported targets, with full `HAgent.Tests` at **213/213**. The current filter/paging and UI redesign increment is pending local verification.

## Planned order

### 0.9576 — Learned Resource Reliability + Adaptation

- Applicability and validity outcomes.
- Reliability evidence from validated outcomes.
- Staleness, contradiction, drift, and revalidation handling.
- Quarantine, retirement, archival, and forgetting.
- Replacement candidates without in-place mutation of published resources.
- Runtime integration and verification.

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
