# HAgent Active Roadmap

## Current

### 0.9591 — Goal/Plan Persistence + Recovery

**Slice 2 — Durable plans and steps — IMPLEMENTED / VERIFICATION PENDING.**

0.958 Agent Lifecycle + Health is CLOSED / VERIFIED. Its four slices are complete; the final full `.NET 9` `HAgent.Tests` checkpoint reported **286/286 passed, 0 failed, 0 skipped**.

#### Slice 1 — Durable goal/intention contracts — CLOSED / VERIFIED

- Stable goal/intention identities, status, priority, constraints, provenance, revision metadata, adoption metadata, and status-change reasons.
- Goal authority distinguishes `HostSupplied` from `AgentInferred`.
- Example: `HAgent.Example → Cognition → Goals & Plans → GOAL & INTENTION CONTRACTS`.
- Full `.NET 9` `HAgent.Tests`: **270/270 passed, 0 failed, 0 skipped** at the Slice 1 checkpoint.

#### Slice 2 — Durable plans and steps — IMPLEMENTED / VERIFICATION PENDING

- `AiPlan` provides goal/intention linkage, status, revision metadata, provenance, preconditions, assumptions, expected effects, failure conditions, completion criteria, and owned steps.
- `AiPlanStep` provides explicit sequence, dependencies, status, preconditions, assumptions, expected effects, completion criteria, failure conditions, provenance, and revision.
- Validation rejects duplicate step identities, foreign ownership, self-dependencies, and dangling dependencies.
- Focused tests: `tests/HAgent.Tests/PlanContractsTests.cs`.
- Matching Example scenario file: `src/HAgent.Example/MainForm.PlanContractsTests.cs`.
- Example organization/registration still needs to expose `PLAN CONTRACTS` under `Cognition → Goals & Plans` before user verification.

**Verification to run after Example registration:** `HAgent.Example → Cognition → Goals & Plans → PLAN CONTRACTS` on .NET Framework 4.8.1 and .NET 9 Windows, then `tests/HAgent.Tests/PlanContractsTests.cs` focused first and the full regression suite.

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
