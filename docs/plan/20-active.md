# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — VERIFIED

Phase 0.957 is complete through Slice 6. User verification on 2026-09-09 recorded 139/139 tests passed and the required evaluation Example succeeded on .NET Framework 4.8.1 and .NET 9.

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT

Phase 0.9575 consumes the canonical resource, identity, policy, instruction, context, observability, evaluation, and runtime foundations. It must not introduce a parallel resource architecture.

### Slices 1–10 — VERIFIED

Slices 1–6 were verified on 2026-09-09. Slice 7, Slice 8, Slice 9, and Slice 10 were verified by the user on 2026-09-11. Their recorded test counts are 146, 153, 158, 167, 175, 181, 187, 194, 200, and 205 respectively; all required Examples succeeded on both supported frameworks.

### Slice 11 — Context, Instruction, Runtime, and Observability Integration — VERIFIED

Verified by user on 2026-09-11.

- `HAgent.Example → Cognition → Learning → Learning Execution Integration` succeeded on .NET Framework 4.8.1 and .NET 9.
- Both Examples verified provider-neutral learned instruction, policy/capability-gated learned context, bounded execution context snapshots, authoritative runtime outcome observation capture, non-creation of candidates from observations, and non-authoritative prompt text.
- Full `HAgent.Tests`: **208/208 passed, 0 failed, 0 skipped** on .NET 9.

Architecture: `docs/architecture/90-learning-execution-integration.md`.

Slice 11 is closed.

### Slice 12 — Management UI — CURRENT

The management surface is being added under the existing WinForms configuration architecture. `AISettingsForm` remains a composition shell; feature behavior lives under `src/HAgent.WinForms/UI/Configuration/`.

Current increment:

- `Learning Review` configuration page for durable `PendingReview` candidates;
- explicit reviewer identity fields;
- Approve/Reject actions routed through `AiLearningCandidateReviewService` and the unified policy engine;
- bounded candidate metadata projection without copying candidate payload into the list;
- durable candidate store dependency exposed through `ConfigurationContext`;
- no authoritative Memory/Knowledge/Skill publication from the UI.

Architecture: `docs/architecture/93-learning-review-management-ui.md`.

**Example to run:** `HAgent.Example → Configuration → Learning Review`.

**Tests to run:** full `HAgent.Tests` remains the phase regression gate; Slice 12 UI verification is primarily the supported WinForms Example on .NET Framework 4.8.1 and .NET 9.

## Run rule

Complete the current implementation increment and record its verification before moving to the next management-UI increment. Do not combine multiple numbered slices in one run.
