# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.9591 Goal/Plan Persistence and Recovery — CURRENT

Phase 0.958 Agent Lifecycle and Health Management is fully verified and closed after Slice 4 verification on both supported Example targets, with the user's full `.NET 9` regression checkpoint at **286/286 passed, 0 failed, 0 skipped**.

### Slice 1 — Durable goal/intention contracts — VERIFIED / CLOSED

Built ahead of roadmap and then formally entered/verified with phase review. The contracts keep goal identity separate from intention identity, distinguish `HostSupplied` from `AgentInferred` authority, preserve provenance, timestamps, revision metadata, adoption metadata, and intention status-change reasons.

Verification checkpoint recorded on 2026-09-13:

- .NET Framework 4.8.1 Example: `GOAL & INTENTION CONTRACTS` succeeded.
- .NET 9 Example: `GOAL & INTENTION CONTRACTS` succeeded.
- Full `.NET 9` `HAgent.Tests`: **270/270 passed, 0 failed, 0 skipped**.

### Slice 2 — Durable plans and steps — IMPLEMENTED / VERIFICATION PENDING

Implemented the canonical provider-neutral plan surface in `src/HAgent.Core/Models/AiPlanContracts.cs`:

- `AiPlan` keeps goal identity, intention identity, status, revision metadata, provenance, preconditions, assumptions, expected effects, failure conditions, completion criteria, and owned steps separate from runtime execution machinery.
- `AiPlanStep` carries status, explicit sequence, dependency IDs, preconditions, assumptions, expected effects, completion criteria, failure conditions, provenance, and its own revision.
- Plan validation rejects duplicate step IDs, foreign plan ownership, self-dependencies, and dangling dependencies.
- Plan cloning detaches nested collections and step state.
- Focused tests: `tests/HAgent.Tests/PlanContractsTests.cs`.
- Matching Example scenario exists in `src/HAgent.Example/MainForm.PlanContractsTests.cs`; Example organization/registration still needs to be completed before the Slice 2 verification checkpoint is handed to the user.

### Verification checkpoint

**Example to run:** not yet released for verification. Intended path: `HAgent.Example → Cognition → Goals & Plans → PLAN CONTRACTS` once Example registration is completed.

**Tests to run:** `tests/HAgent.Tests/PlanContractsTests.cs` focused first, then the full `HAgent.Tests` regression suite required by the phase checkpoint.

**Run rule:** Do not begin Slice 3 until Slice 2 Example registration and verification are complete and recorded.
