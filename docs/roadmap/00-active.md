# HAgent Active Roadmap

## Current

### 0.9591 — Goal/Plan Persistence + Recovery

**Slice 5 — Restart and recovery — IMPLEMENTING.**

0.958 Agent Lifecycle + Health is CLOSED / VERIFIED. Its four slices are complete; the final full `.NET 9` `HAgent.Tests` checkpoint reported **286/286 passed, 0 failed, 0 skipped**.

#### Slice 1 — Durable goal/intention contracts — CLOSED / VERIFIED

- Stable goal/intention identities, status, priority, constraints, provenance, revision metadata, adoption metadata, and status-change reasons.
- Goal authority distinguishes `HostSupplied` from `AgentInferred`.
- Example: `HAgent.Example → Cognition → Goals & Plans → GOAL & INTENTION CONTRACTS`.
- Full `.NET 9` `HAgent.Tests`: **270/270 passed, 0 failed, 0 skipped**.

#### Slice 2 — Durable plans and steps — CLOSED / VERIFIED

- `AiPlan` provides goal/intention linkage, status, revision metadata, provenance, preconditions, assumptions, expected effects, failure conditions, completion criteria, and owned steps.
- `AiPlanStep` provides explicit sequence, dependencies, status, preconditions, assumptions, expected effects, completion criteria, failure conditions, provenance, and revision.
- Validation rejects duplicate step identities, foreign ownership, and self-dependencies; cloning detaches nested state.
- Focused tests: `tests/HAgent.Tests/PlanContractsTests.cs`.
- Matching Example: `HAgent.Example → Cognition → Goals & Plans → PLAN CONTRACTS`.
- Full `.NET 9` regression at checkpoint: **290/290 passed, 0 failed, 0 skipped**.

#### Slice 3 — Checkpoints and outcome semantics — CLOSED / VERIFIED

- Explicit checkpoint boundaries and provider-neutral safe-point evidence.
- Distinguishes `Completed`, `Failed`, `UnknownOutcome`, `Cancelled`, and `Superseded`.
- Never converts timeout/provider failure into success without evidence.
- Implementation: `src/HAgent.Core/Models/AiCheckpointOutcomeContracts.cs`.
- Focused tests: `tests/HAgent.Tests/CheckpointOutcomeContractsTests.cs`.
- Matching Example: `HAgent.Example → Cognition → Goals & Plans → CHECKPOINT & OUTCOME CONTRACTS`.
- Verified on both .NET Framework 4.8.1 and .NET 9 Example targets.
- Full `.NET 9` regression at checkpoint: **294/294 passed, 0 failed, 0 skipped**.

#### Slice 4 — Retry and idempotency — CLOSED / VERIFIED

- Stable operation identity is linked to plan, step, and plan revision; retry attempts do not create a new operation identity.
- Failed operations are retryable only when the host explicitly establishes retry safety.
- Requested and unknown external outcomes require reconciliation before retry.
- Completed, cancelled, and superseded operations are not retryable.
- Implementation: `src/HAgent.Core/Models/AiPlanRetryIdempotencyContracts.cs`.
- Focused tests: `tests/HAgent.Tests/PlanRetryIdempotencyContractsTests.cs`.
- Matching Example: `HAgent.Example → Cognition → Goals & Plans → RETRY & IDEMPOTENCY`.
- Verified on both .NET Framework 4.8.1 and .NET 9 Example targets.
- Full `.NET 9` regression at checkpoint: **300/300 passed, 0 failed, 0 skipped**.

#### Slice 5 — Restart and recovery — CURRENT / IMPLEMENTING

- Recover the latest durable goal/plan revision after process restart or crash without reviving obsolete execution/provider authority.
- Invalidate in-flight work owned by the previous process/runtime execution.
- Reconcile incomplete steps into safe states such as retryable, unknown, blocked, or requiring host review.
- Preserve evidence explaining the recovery decision.
- Keep the recovery boundary host-neutral; persistence backend implementation remains Slice 6.

**Current implementation target:** define the provider-neutral restart/recovery contract, focused tests, and matching Example before any persistence backend work.

## Planned order

### 0.959 — Human-in-the-Loop + Intervention

Consumes the canonical intervention boundary while 0.959 owns runtime lifecycle transitions caused by authorized intervention.

### 0.9592 — Provider Ecosystem + Adapter Lifecycle

Provider/adapter lifecycle and provider operational evidence remain separate from runtime-agent lifecycle and health.

### 0.9593 — Reasoning Requirement + Boundary Foundation

Provider-neutral reasoning requirement contract before later capability-aware execution and reasoning engineering.

### Later V1 order

`0.96.x → 0.96 → 0.97 → 0.98 → 0.10 → 1.0`

The ordered dependency chain and full historical roadmap remain authoritative in the numbered `docs/roadmap/` phase documents and `docs/roadmap/00-overview.md`.
