# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — VERIFIED

Phase 0.957 is complete through Slice 6. User verification on 2026-09-09 recorded 139/139 tests passed and the required evaluation Example succeeded on .NET Framework 4.8.1 and .NET 9.

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CLOSED

Phase 0.9575 is closed through Learning Review and the Authoritative Resource Inventory + Detail Inspection management increment.

- Learning Review was user-verified on both supported targets.
- Authoritative Resource Inventory was user-verified on 2026-09-12 on .NET Framework 4.8.1 and .NET 9.
- The inventory Example verified real `InMemoryMemoryStore` projection, provider-neutral Knowledge enumeration, authoritative-only filtering, deterministic ordering/paging, filtering, and readable Memory/Knowledge/Skill details.
- Skill storage enumeration remains deferred until its existing source contract gains a supported authoritative enumeration boundary.
- Governed resource editing/version-creation/lifecycle workflows remain later management work.

## 0.9576 Learned Resource Reliability + Adaptation — CURRENT

### Slice 1 — Applicability and validity — CURRENT

This slice establishes the provider-neutral decision boundary that determines whether a promoted resource version is applicable to a bounded context, or whether evidence is insufficient or the resource is invalidated.

Implemented:

- `AiApplicabilityOutcome`: `Applicable`, `NotApplicable`, `Uncertain`, `Invalidated`;
- `AiApplicabilityTarget` with resource type/ID/version/scope and explicit invalidation state;
- `AiApplicabilityCondition` with bounded `Exists`, `Equals`, `NotEquals`, and `OneOf` operators;
- `AiApplicabilityContext` with bounded host-supplied facts and optional scope;
- `AiApplicabilityEvidenceReference` plus condition-level evidence references;
- `AiApplicabilityDecision` retaining outcome, version identity, bounded condition results, evidence references, reason, and evaluation timestamp;
- provider-neutral `IAiApplicabilityEvaluator`;
- deterministic `AiDeterministicApplicabilityEvaluator` that evaluates available deterministic evidence before any model reasoning;
- missing deterministic evidence yields `Uncertain` and never proves applicability;
- applicability remains independent from authorization/capability policy;
- invalidation is terminal for this evaluator and does not mutate the published resource;
- focused tests and a dedicated manual Example.

Architecture: `docs/architecture/97-learned-resource-applicability.md`.

**Example to run:** `HAgent.Example → Learned Resource Applicability` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `HAgent.Tests → LearnedResourceApplicabilityTests.cs` focused first, then the full `HAgent.Tests` regression suite.

## Run rule

Complete Slice 1 and record user verification before starting 0.9576 Slice 2 reliability evidence. Do not combine numbered 0.9576 slices in one run.
