# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — VERIFIED

Phase 0.957 is complete through Slice 6. User verification on 2026-09-09 recorded 139/139 tests passed and the required evaluation Example succeeded on .NET Framework 4.8.1 and .NET 9.

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT

Phase 0.9575 consumes the canonical resource, identity, policy, instruction, context, observability, evaluation, and runtime foundations. It must not introduce a parallel resource architecture.

### Slice 1 — Resource capability governance — VERIFIED

Verified on 2026-09-09. Resource governance composes canonical ownership, effective capability state, and unified policy authorization. User reported 146/146 tests passed and the required Example succeeded on both supported frameworks.

### Slice 2 — Knowledge/Wiki governed resource contract — VERIFIED

Verified on 2026-09-09. Managed Knowledge/Wiki resources provide explicit identity, scope/ownership, provenance, lifecycle/versioning, bounded metadata, relationships, chunks, and provider/index-neutral governed retrieval. User reported 153/153 tests passed and the required Example succeeded on both supported frameworks.

### Slice 3 — Stable/versioned Skill definition and reference contract — VERIFIED

Verified by user on 2026-09-09.

- Example: `HAgent.Example → Cognition → Skills → Skill Definitions` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: 158/158 passed, 0 failed, 0 skipped on .NET 9.

The Skill architecture includes versioned reusable definitions/references, explicit scope/ownership, lifecycle/provenance, bounded contracts, preconditions, ordered procedure steps, Knowledge/Tool dependencies, constraints, execution snapshot isolation, governed resolution, and runtime-owned executable handlers outside persisted definitions.

Slice 3 is closed.

### Slice 4 — Memory family/type and provenance contract — VERIFIED

Verified by user on 2026-09-09.

- Example: `HAgent.Example → Memory → Memory Families` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: 167/167 passed, 0 failed, 0 skipped on .NET 9.

Slice 4 is closed.

### Slice 5 — Memory governance and retention — VERIFIED

Verified by user on 2026-09-09.

- Example: `HAgent.Example → Memory → Memory Governance` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: 175/175 passed, 0 failed, 0 skipped on .NET 9.

Slice 5 is closed.

### Slice 6 — Learning Mode foundation — VERIFIED

Verified by user on 2026-09-09.

- Example: `HAgent.Example → Cognition → Learning → Learning Mode` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: 181/181 passed, 0 failed, 0 skipped on .NET 9.

`AiLearningMode` remains distinct from resource capability enablement, with persistent profile state, runtime-only override, immutable execution-snapshot capture, and the existing single learning-candidate contract.

Slice 6 is closed.

### Slice 7 — Learning Policy + Typed Candidates — CURRENT

**Objective:** define one provider-neutral learning policy contract and typed Memory/Knowledge/Skill candidate payload contracts while reusing the existing canonical `AiLearningCandidate` lifecycle and promotion boundary.

**Complete within this slice:**

- learning policy covering candidate type, scope, confidence/evidence, provenance, contradiction checks, retention, evaluation requirements, and promotion authorization;
- typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` contracts;
- source execution/runtime/profile identity, proposed scope, provenance, and evidence/confidence preservation where available;
- deterministic code-derived learning signals without requiring an LLM;
- optional model-assisted extraction/evaluation that remains non-authoritative;
- candidate creation kept separate from candidate promotion.

**Out of scope:** candidate persistence, promotion orchestration, Learning Review UI, Knowledge/Skill management UI, context integration, and the broader 0.9576 roadmap restructuring.

**Completion checkpoint:** affected projects build; focused learning-policy/typed-candidate tests pass; full .NET 9 `HAgent.Tests` passes; matching Example succeeds on .NET Framework 4.8.1 and .NET 9.

**Example to run:** `HAgent.Example → Cognition → Learning → Learning Policy` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/LearningPolicyTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Run rule

Complete the current slice and record its verification before selecting the next slice. Do not combine multiple numbered slices in one run.
