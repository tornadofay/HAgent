# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.958 Agent Lifecycle and Health Management — CURRENT

Phase 0.9576 Learned Resource Reliability + Adaptation is now VERIFIED through all five slices.

### Entry condition

0.9576 is complete and user-verified on .NET Framework 4.8.1 and .NET 9, including runtime integration. Full `.NET 9` regression was reported at 260/260 passed.

### Slice 1 — Lifecycle state extension

Extend the existing runtime lifecycle only where long-lived operation requires it.

- Preserve the existing authoritative runtime-instance identity and execution lifecycle.
- Add only the required long-lived operational states: `Active`, `Suspended`, `Recovering`, `Retired`, and `Shutdown`.
- Define valid/invalid lifecycle transitions and terminal behavior.
- Prevent suspended, recovering, retired, or shutdown runtimes from originating work that policy/lifecycle rules disallow.
- Preserve revision and stale-result protection.
- Add focused tests and a matching Example.

Architecture: `docs/roadmap/958-agent-lifecycle-health.md`.

**Example to run:** the new Slice 1 lifecycle Example on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** the focused Slice 1 lifecycle test class, then the required regression suite.

### Run rule

Slice 1 is the only active implementation slice. Do not begin Slice 2 until Slice 1 has its focused tests, both required Example targets, and required regression verification recorded as complete.
