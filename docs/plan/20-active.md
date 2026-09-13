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

### Slice 5 — Restart and recovery — VERIFIED / CLOSED

Implemented in:
- `src/HAgent.Core/Models/AiPlanRecoveryContracts.cs`
- `tests/HAgent.Tests/PlanRecoveryContractsTests.cs`
- Example: `HAgent.Example -> Cognition -> Goals & Plans -> RESTART & RECOVERY`

The provider-neutral recovery boundary preserves plan identity/revision, requires a new runtime instance and advancing execution revision, invalidates previous authority, keeps terminal steps terminal, and requires host review for requested/unknown external outcomes.

**Verified on 2026-09-13:** .NET Framework 4.8.1 and .NET 9 Example scenarios both succeeded. Full `.NET 9` `HAgent.Tests`: **306/306 passed, 0 failed, 0 skipped**.

### Slice 6 — Persistence backends and verification — CURRENT / IMPLEMENTING

Use the existing storage architecture rather than introducing a second persistence model.

Current implementation investigation confirms these provider-neutral Core boundaries already exist:
- `IAiStore`
- `IAgentRuntimeStateStore`
- `IExecutionAuditStore`

Existing provider assemblies implement these boundaries outside Core, including File, SQL Server, and MySQL stores. Slice 6 will extend that pattern for the canonical goal/plan/checkpoint/retry/recovery contracts.

Initial implementation target:
- define the focused provider-neutral durable cognition store contract;
- keep canonical `AiGoal`, `AiIntention`, `AiPlan`, checkpoint/outcome, operation, and recovery models as the persisted domain objects;
- enforce plan/revision/authority consistency at the store boundary;
- add aligned File, SQL Server, and MySQL adapters without putting provider details in Core;
- add deterministic tests and a matching Example scenario before backend verification.

### Verification checkpoint

**Example to run:** `HAgent.Example -> Cognition -> Goals & Plans -> PERSISTENT GOAL/PLAN RECOVERY` once Slice 6 Example is implemented, on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** the focused Slice 6 persistence test class first, then the full `HAgent.Tests` regression suite.

**Run rule:** do not start Slice 7 or unrelated roadmap work until the Slice 6 checkpoint is recorded.
