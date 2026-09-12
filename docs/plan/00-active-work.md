# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9576 Learned Resource Reliability + Adaptation
- **Status:** Slice 2 in progress — Reliability evidence and outcome feedback
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Record host-validated post-promotion reliability evidence against exact resource versions, with policy control and revision-safe updates, without mutating the authoritative resource.

## 0.9575 checkpoint closed

The 0.9575 Knowledge, Skills, Memory Governance + Learning phase is complete through the Learning Review and Authoritative Resource Inventory + Detail Inspection management increment.

User verification completed on 2026-09-12, followed by 0.9576 Slice 1 verification and a 234/234 full-suite result reported by the user.

## 0.9576 Slice 1 checkpoint closed

User verified `HAgent.Example → Learned Resource Applicability` on .NET Framework 4.8.1 and .NET 9. The reported full .NET 9 regression result was 234/234 passed, 0 failed, 0 skipped.

## Current Slice 2 boundary

`0.9576 Slice 2 — Reliability evidence and outcome feedback` establishes:

- `AiResourceReliabilityIdentity` keyed by resource type, resource ID, published version, and scope;
- distinct promotion evidence and operational outcome evidence;
- `AiValidatedResourceOutcome` requiring explicit host validation before it can change reliability;
- bounded reliability score and outcome counters;
- review and quarantine recommendations as evidence-derived state, not lifecycle authorization;
- provider-neutral `IAiResourceReliabilityStore` with revision-safe updates;
- deterministic `InMemoryAiResourceReliabilityStore` reference implementation;
- policy-controlled outcome updates through `IAiPolicyEngine` operation `resource.reliability.record-outcome`;
- success reinforcement, failure weakening, invalid-precondition weakening, and contradiction weakening;
- execution/runtime/agent/evaluation provenance preservation;
- no mutation of the promoted resource version;
- no model, GPU, embeddings, vector database, or host-domain dependency.

## Files and verification

- Architecture: `docs/architecture/98-learned-resource-reliability.md`.
- Tests: `tests/HAgent.Tests/LearnedResourceReliabilityTests.cs` / `LearnedResourceReliabilityTests`.
- Example: `src/HAgent.Example/MainForm.LearnedResourceReliability.cs` / `Learned Resource Reliability`.

**Example to run:** `HAgent.Example → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `HAgent.Tests → LearnedResourceReliabilityTests.cs` focused first, then the full `HAgent.Tests` regression suite.

Verification is pending user execution.

## Do not advance

Do not advance to 0.9576 Slice 3 staleness/drift/revalidation, or later quarantine/forgetting/runtime integration, until this Slice 2 Example succeeds on both supported targets and the focused/full tests remain green.
