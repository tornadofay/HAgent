# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.9591 Goal/Plan Persistence + Recovery — CURRENT

### Entry condition

Phase 0.958 Agent Lifecycle and Health Management Slice 1 is verified on .NET Framework 4.8.1 and .NET 9 Windows, including lifecycle Example execution and the full .NET 9 HAgent.Tests regression suite at 266/266 passed.

### Slice 1 — Durable goal/intention contracts — IMPLEMENTED, VERIFICATION PENDING

The phase establishes durable provider-neutral goal/intention authority before plans, checkpoints, retries, and restart recovery.

Implemented:

- Stable goal identity independent from intention identity.
- Explicit goal and intention statuses and priority metadata.
- Constraint collections on both goals and intentions.
- Provenance with explicit `HostSupplied` versus `AgentInferred` authority.
- Goal/intention creation and update timestamps plus monotonic revision metadata.
- Intention adoption timestamp.
- Immutable intention status-change records containing previous/new status, revision, mandatory reason, evidence, timestamp, and authority.
- Validation for required identity, metadata, revision, timestamp, provenance, and adoption ordering.
- Matching focused tests and public-API Example through the normal Example registration/classification path.

No durable Plan/PlanStep, checkpoint, retry/idempotency, restart recovery, or persistence backend work is included in Slice 1.

### Authoritative architecture

`docs/roadmap/9591-goal-plan-persistence-recovery.md` defines the phase delivery boundary. `docs/architecture/103-goal-plan-persistence-recovery.md` defines the detailed Slice 1 contract architecture. `docs/architecture/10-runtime.md` and `docs/architecture/102-runtime-lifecycle-health.md` remain authoritative for runtime identity, execution revision, lifecycle, cancellation, and stale-result authority.

### Example and verification checkpoint

**Example to run:** `HAgent.Example → Cognition → Goals & Plans → GOAL & INTENTION CONTRACTS` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/GoalIntentionContractsTests.cs` focused first, then the full `HAgent.Tests` regression suite.

**Verification status:** implementation is committed directly to `master`; Slice 1 is not closed until focused tests, full regression, and both Example targets are actually executed and recorded.

### Run rule

Slice 1 is the only active implementation slice. Do not begin Slice 2 (durable plans and steps) until Slice 1 verification is complete.
