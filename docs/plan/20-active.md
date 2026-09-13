# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.9591 Goal/Plan Persistence and Recovery — CURRENT

Phase 0.958 is closed/verified. The final user-reported full .NET 9 regression checkpoint was **286/286 passed, 0 failed, 0 skipped**.

### Slice 1 — Durable goal/intention contracts — VERIFIED / CLOSED

Verified on both supported Example targets. Full .NET 9 checkpoint: **270/270 passed, 0 failed, 0 skipped**.

Example: `HAgent.Example -> Cognition -> Goals & Plans -> GOAL & INTENTION CONTRACTS`.

### Slice 2 — Durable plans and steps — VERIFIED / CLOSED

Implemented in `src/HAgent.Core/Models/AiPlanContracts.cs` with focused tests in `tests/HAgent.Tests/PlanContractsTests.cs` and matching Example `src/HAgent.Example/MainForm.PlanContractsTests.cs`.

`AiPlan` covers goal/intention linkage, status, revision metadata, provenance, preconditions, assumptions, expected effects, failure conditions, completion criteria, and owned steps. `AiPlanStep` covers explicit sequence, dependencies, status, preconditions, assumptions, expected effects, completion criteria, failure conditions, provenance, and revision. Validation covers duplicate identities, plan ownership, and self-dependencies; cloning detaches nested state.

**Verified on 2026-09-13:** .NET Framework 4.8.1 Example and .NET 9 Example both succeeded. Full `.NET 9` `HAgent.Tests` reported **290/290 passed, 0 failed, 0 skipped**.

### Slice 3 — Checkpoints and outcome semantics — VERIFIED / CLOSED

Implemented the provider-neutral checkpoint/outcome contract surface in `src/HAgent.Core/Models/AiCheckpointOutcomeContracts.cs` with focused tests in `tests/HAgent.Tests/CheckpointOutcomeContractsTests.cs` and matching Example `src/HAgent.Example/MainForm.CheckpointOutcomeContractsTests.cs`.

The contracts provide explicit checkpoint identity, plan/step linkage, plan revision, boundary/evidence metadata, and checkpoint status. Outcome semantics distinguish `Completed`, `Failed`, `UnknownOutcome`, `Cancelled`, and `Superseded`; completed outcomes require evidence and unknown external outcomes remain explicitly non-success.

**Verified on 2026-09-13:** .NET Framework 4.8.1 Example and .NET 9 Example both succeeded. Full `.NET 9` `HAgent.Tests` reported **294/294 passed, 0 failed, 0 skipped**.

### Slice 4 — Retry and idempotency — VERIFIED / CLOSED

Implemented in `src/HAgent.Core/Models/AiPlanRetryIdempotencyContracts.cs` with focused tests in `tests/HAgent.Tests/PlanRetryIdempotencyContractsTests.cs` and matching Example `HAgent.Example -> Cognition -> Goals & Plans -> RETRY & IDEMPOTENCY`.

The durable contract keeps operation identity stable across retry attempts, requires explicit host confirmation before retrying a failed operation, requires reconciliation for requested and unknown outcomes, and prevents completed, cancelled, and superseded operations from being retried. HAgent does not claim exactly-once execution for arbitrary host side effects.

**Verified on 2026-09-13:** .NET Framework 4.8.1 Example and .NET 9 Example both succeeded. Full `.NET 9` `HAgent.Tests` reported **300/300 passed, 0 failed, 0 skipped**.

### Slice 5 — Restart and recovery — CURRENT / IMPLEMENTING

Define safe recovery semantics across process restart or crash before persistence backend implementation.

Initial boundary:
- Recover the latest durable goal/plan revision without reviving obsolete execution/provider authority.
- Invalidate work owned by the previous process/runtime execution.
- Reconcile incomplete steps into safe states such as retryable, unknown, blocked, or requiring host review.
- Preserve evidence explaining the recovery decision.
- Keep recovery deterministic and host-neutral; backend persistence remains Slice 6.

### Implementation checkpoint

Slice 5 should add focused tests and a matching Example before any persistence backend work begins. Do not start Slice 6 until the Slice 5 Example and regression results are recorded.
