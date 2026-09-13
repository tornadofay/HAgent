# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9576 Learned Resource Reliability + Adaptation
- **Status:** Slice 4 implementation in progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Define governed retention/utility assessment for already-promoted learned resources, including archival, retirement eligibility, authority protection, provenance, recovery, and revision-safe policy-controlled state changes.

## 0.9575 checkpoint closed

The 0.9575 Knowledge, Skills, Memory Governance + Learning phase is complete through the Learning Review and Authoritative Resource Inventory + Detail Inspection management increment.

User verification completed on 2026-09-12, followed by 0.9576 Slice 1 verification and a 234/234 full-suite result reported by the user.

## 0.9576 Slice 1 checkpoint closed

User verified `HAgent.Example → Learned Resource Applicability` on .NET Framework 4.8.1 and .NET 9. The reported full .NET 9 regression result was 234/234 passed, 0 failed, 0 skipped.

## 0.9576 Slice 2 checkpoint closed

User verified `HAgent.Example → Cognition → Learning → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9 on 2026-09-13.

The user also reported the full `.NET 9` `HAgent.Tests` result: **242/242 passed, 0 failed, 0 skipped**.

Verified boundary:

- `AiResourceReliabilityIdentity` keyed by resource type, resource ID, published version, and scope;
- distinct promotion evidence and operational outcome evidence;
- host-validated outcomes only;
- bounded reliability score and outcome counters;
- review and quarantine recommendations as evidence-derived state;
- revision-safe provider-neutral reliability persistence;
- policy-controlled reliability updates;
- success reinforcement, failure weakening, invalid-precondition weakening, and contradiction weakening;
- execution/runtime/agent/evaluation provenance preservation;
- no mutation of the promoted resource version.

Example organization was also corrected: learned-resource capability scenarios are explicitly classified, and unknown feature/subgroup classification fails closed instead of silently falling into Diagnostics.

## 0.9576 Slice 3 checkpoint closed

User verified `HAgent.Example → Cognition → Learning → LEARNED RESOURCE ADAPTATION` on .NET Framework 4.8.1 and .NET 9 on 2026-09-13.

The user also reported the full `.NET 9` `HAgent.Tests` result: **252/252 passed, 0 failed, 0 skipped**.

Verified boundary:

- `Active`, `UnderReview`, `Quarantined`, and `Retired` lifecycle states;
- `Current`, `Stale`, `Degraded`, `Drifted`, and `Contradicted` conditions;
- explicit automatic-use gate requiring `Active + Current`;
- exact-version lifecycle state and bounded transition history;
- age-based staleness, observed degradation, contextual drift, and direct contradiction detection;
- policy-controlled, compare-and-swap lifecycle mutation;
- terminal `Retired` preservation;
- governed quarantine and clean recovery;
- replacement proposals emitted as typed learning candidates;
- published resource versions remain unchanged.

## 0.9576 Slice 4 implementation checkpoint

Current objective: build the provider-neutral retention/utility boundary without deleting or mutating authoritative learned resources.

Entry condition: Slice 3 is user-verified and closed.

Required outcomes:

- bounded utility and retention signals;
- deterministic eligibility for archive/retire decisions;
- protection against removing a higher-authority resource because a lower-utility duplicate exists;
- bounded provenance for retention decisions;
- recoverable `Archived` state where policy allows;
- policy-controlled and compare-and-swap state changes;
- no authoritative resource-version mutation;
- focused tests and matching Example.

**Tests:** `HAgent.Tests → LearnedResourceRetentionTests.cs`.

**Example:** `HAgent.Example → Cognition → Learning → LEARNED RESOURCE RETENTION` on .NET Framework 4.8.1 and .NET 9.
