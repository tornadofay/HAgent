# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — VERIFIED

Phase 0.957 is complete through Slice 6. User verification on 2026-09-09 recorded 139/139 tests passed and the required evaluation Example succeeded on .NET Framework 4.8.1 and .NET 9.

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CLOSED

Phase 0.9575 is closed through Learning Review and the Authoritative Resource Inventory + Detail Inspection management increment. User verification was completed on 2026-09-12 on .NET Framework 4.8.1 and .NET 9.

## 0.9576 Learned Resource Reliability + Adaptation — CURRENT

### Slice 2 — VERIFIED

User verified Learned Resource Reliability on both supported targets on 2026-09-13. Full .NET 9 suite: 242/242 passed.

### Slice 3 — VERIFIED

User verified `HAgent.Example → Cognition → Learning → LEARNED RESOURCE ADAPTATION` on .NET Framework 4.8.1 and .NET 9 on 2026-09-13. Full .NET 9 suite: **252/252 passed, 0 failed, 0 skipped**.

Verified boundary: bounded lifecycle state, deterministic condition assessment, automatic-use gate, policy-controlled revalidation, revision-safe lifecycle persistence, governed recovery, typed replacement candidates, and unchanged published resource identity.

Architecture: `docs/architecture/99-learned-resource-lifecycle.md`.

### Slice 4 — CURRENT

Retention/archival boundary for already-promoted learned resources.

Implemented in this run:

- bounded utility, validated-use, freshness, supersession, contradiction, retention, retirement, and authority signals;
- recoverable `Archived` lifecycle state;
- deterministic archive/retire/restore decisions;
- higher-authority preservation;
- policy operation `resource.lifecycle.retention`;
- compare-and-swap lifecycle mutation and bounded retention provenance;
- focused tests in `LearnedResourceRetentionTests`;
- matching Example registered through the existing Learning registration path.

Architecture: `docs/architecture/100-learned-resource-retention.md`.

**Example to run:** `HAgent.Example → Cognition → Learning → LEARNED RESOURCE RETENTION` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `HAgent.Tests → LearnedResourceRetentionTests.cs` focused first, then the full `HAgent.Tests` suite.

### Run rule

Slice 4 is the only active implementation slice. Do not begin Slice 5 until Slice 4 has its focused tests, both required Example targets, and required regression verification recorded as complete.
