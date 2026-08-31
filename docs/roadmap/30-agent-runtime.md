# Phase 0.9 — Agent Runtime Instances

## Goal
Separate reusable agent profiles from live runtime agent identities so one configured profile can produce many independent agents concurrently.

## Sequence

- [ ] Runtime agent instance model with stable instance ID and profile ID.
- [ ] Explicit scope: Application, Workspace, Context/Form, Session, Task, Ephemeral.
- [ ] Runtime-specific context and provider/model overrides.
- [ ] Runtime-instance memory ownership.
- [ ] Independent concurrent execution.
- [ ] External scheduling, cancellation, timeout, and stale-result protection.
- [ ] Explicit retirement and shutdown behavior.
- [ ] Optional persistence for recovery/collaboration.
- [ ] HWorld adapter verification.

## Boundaries

Agent roles are configuration/binding concepts, not separate agent classes. Dynamically created specialists are not permanent configuration records by default.

## Exit criterion

A host can create multiple independent runtime agents from configured profiles, execute them concurrently with isolated identity/context/memory, and safely retire or persist them according to host policy.
