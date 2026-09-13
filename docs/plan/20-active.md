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

### Slice 2 — Reliability evidence and outcome feedback — VERIFIED

User verified `HAgent.Example → Cognition → Learning → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9 on 2026-09-13. The user also reported the full `.NET 9` `HAgent.Tests` regression suite at **242/242 passed, 0 failed, 0 skipped**.

Slice 2 establishes the post-promotion reliability evidence boundary without creating a second resource architecture.

Implemented and verified:

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
- dedicated focused tests and matching manual Example were added;
- Example organization now classifies new capability scenarios explicitly and fails closed instead of silently placing an unclassified example in Diagnostics.

Architecture: `docs/architecture/98-learned-resource-reliability.md`.

**Example:** `HAgent.Example → Cognition → Learning → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9.

**Tests:** `HAgent.Tests → LearnedResourceReliabilityTests.cs`, plus the full regression suite reported at 242/242 on .NET 9.

## Run rule

Slice 2 is closed. Do not advance to 0.9576 Slice 3 until a new run explicitly starts that numbered slice.
