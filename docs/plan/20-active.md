# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.9591 Goal/Plan Persistence and Recovery — CURRENT

Phase 0.958 is closed/verified. The final user-reported full .NET 9 regression checkpoint was **286/286 passed, 0 failed, 0 skipped**.

### Slice 1 — Durable goal/intention contracts — VERIFIED / CLOSED

Verified on both supported Example targets. Full .NET 9 checkpoint: **270/270 passed, 0 failed, 0 skipped**.

Example: `HAgent.Example -> Cognition -> Goals & Plans -> GOAL & INTENTION CONTRACTS`.

### Slice 2 — Durable plans and steps — IMPLEMENTED / VERIFICATION PENDING

Implemented in `src/HAgent.Core/Models/AiPlanContracts.cs` with focused tests in `tests/HAgent.Tests/PlanContractsTests.cs` and a matching Example scenario in `src/HAgent.Example/MainForm.PlanContractsTests.cs`.

`AiPlan` covers goal/intention linkage, status, revision metadata, provenance, preconditions, assumptions, expected effects, failure conditions, completion criteria, and owned steps. `AiPlanStep` covers explicit sequence, dependencies, status, preconditions, assumptions, expected effects, completion criteria, failure conditions, provenance, and revision. Validation covers duplicate identities, plan ownership, and self-dependencies; cloning detaches nested state.

The Example shell now explicitly registers `PLAN CONTRACTS` under `Cognition -> Goals & Plans`.

### Verification checkpoint

**Example to run:** `HAgent.Example -> Cognition -> Goals & Plans -> PLAN CONTRACTS` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/PlanContractsTests.cs` focused first, then the full `HAgent.Tests` regression suite.

**Run rule:** Do not begin Slice 3 until Slice 2 Example verification and regression results are recorded.
