# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9591 Goal/Plan Persistence and Recovery
- **Status:** Slice 1 implemented; verification pending
- **Primary source:** `docs/plan/20-active.md`
- **Architecture source:** `docs/architecture/103-goal-plan-persistence-recovery.md`
- **Scope:** Establish provider-neutral durable goal/intention contracts with stable identity, explicit status/priority, constraints, provenance, timestamps, revisions, and attributable intention status-change reasons. Do not begin durable plan, checkpoint, retry, or restart-recovery slices in this run.

## 0.958 checkpoint closed

Phase 0.958 Agent Lifecycle and Health Management Slice 1 is fully verified. The user verified `HAgent.Example → Runtime → Runtime Instances → RUNTIME LIFECYCLE` on both .NET Framework 4.8.1 and .NET 9 Windows. The user reported the full `.NET 9` `HAgent.Tests` suite at **266/266 passed, 0 failed, 0 skipped**.

Verified boundaries include lifecycle transitions, lifecycle revision capture, non-active admission rejection, stale-result invalidation, runtime-state persistence/restore, and shutdown cancellation.

## 0.9591 Slice 1 implementation checkpoint

Goal and intention contracts are defined in `src/HAgent.Core/Models/AiGoalContracts.cs`.

The contracts preserve separate goal and intention identities, explicit goal/intention status and priority, constraints, provenance, timestamps, revision metadata, intention adoption metadata, and immutable intention status-change reasons/evidence/authority.

Host-supplied and agent-inferred goal state are explicitly distinguished by `AiGoalAuthority`; inference cannot silently become host authority.

Focused coverage is in `tests/HAgent.Tests/GoalIntentionContractsTests.cs`. The matching public-API Example is `HAgent.Example → Cognition → Goals & Plans → GOAL & INTENTION CONTRACTS`.

## Verification checkpoint

**Example to run:** `HAgent.Example → Cognition → Goals & Plans → GOAL & INTENTION CONTRACTS` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/GoalIntentionContractsTests.cs` focused first, then the full `HAgent.Tests` regression suite required by the active phase.

**Current status:** Slice 1 is implemented directly on `master` but remains open until the focused tests, full regression suite, and both required Example targets are actually executed and recorded.
