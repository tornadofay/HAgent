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

### Slice 2 — Reliability evidence and outcome feedback — CURRENT

Slice 1 applicability/validity was user-verified on 2026-09-12 on both supported targets and the user subsequently reported 234/234 tests passed on .NET 9.

Slice 2 establishes the post-promotion reliability evidence boundary without creating a second resource architecture.

Implemented:

- `AiResourceReliabilityIdentity` captures resource type, resource ID, version, and explicit scope;
- `AiReliabilityEvidence` separates promotion evidence from later operational outcome evidence;
- `AiValidatedResourceOutcome` requires explicit host validation metadata before reliability can change;
- `AiResourceReliabilityRecord` tracks bounded score, outcome counts, review state, quarantine recommendation, and evidence history;
- `IAiResourceReliabilityStore` provides provider-neutral persistence with compare-and-swap revision updates;
- `InMemoryAiResourceReliabilityStore` provides deterministic reference storage for tests and Example verification;
- `AiResourceReliabilityService` applies bounded deterministic outcome feedback and evaluates `IAiPolicyEngine` using the operation `resource.reliability.record-outcome`;
- success reinforces by `+0.05`, failure weakens by `-0.10`, invalid precondition by `-0.15`, and contradiction by `-0.25`, with score clamped to `[0,1]`;
- reliability below `0.50` requests review; contradiction or score at/below `0.25` recommends quarantine without changing lifecycle state;
- execution/runtime/agent-profile/evaluation provenance is preserved in operational evidence;
- stale revision writes are rejected rather than applied last-write-wins;
- dedicated focused tests and a matching manual Example were added.

Architecture: `docs/architecture/98-learned-resource-reliability.md`.

**Example to run:** `HAgent.Example → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `HAgent.Tests → LearnedResourceReliabilityTests.cs` focused first, then the full `HAgent.Tests` regression suite.

Verification is pending user execution. Do not advance to 0.9576 Slice 3 until Slice 2 is verified on both supported targets.

## Run rule

Work only on the current numbered 0.9576 slice. Do not combine reliability feedback with staleness, contradiction detection, forgetting, archival, or runtime integration in the same run.
