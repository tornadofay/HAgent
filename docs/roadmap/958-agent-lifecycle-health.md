# Phase 0.958 — Agent Lifecycle and Health Management

## Status

**Planned architectural foundation before persistent cognitive runtime.**

## Goal

Make agent/runtime lifecycle and health explicit, observable, recoverable, and controllable for both request-oriented and persistent agents.

## Requirements

1. [ ] Define normalized lifecycle states for runtime agents and persistent cognitive agents.
2. [ ] Distinguish lifecycle state from health state and execution state.
3. [ ] Support at least active, sleeping/idle, waiting, blocked, deliberating, executing, degraded, failed, retired, recovering, and shutdown semantics where applicable.
4. [ ] Define health/status reasons and safe transitions rather than exposing only a Boolean healthy flag.
5. [ ] Prevent retired/shutdown agents from originating new executions.
6. [ ] Support suspension/resume without deleting durable state.
7. [ ] Expose lifecycle and health changes through events and tracing.
8. [ ] Define heartbeat/progress or equivalent signals for long-running persistent runtimes where needed.
9. [ ] Detect stalled or repeatedly failing progress without confusing slow legitimate inference with failure.
10. [ ] Support operator-visible diagnostics explaining why an agent is blocked, waiting, degraded, or recovering.
11. [ ] Add deterministic Example verification for lifecycle transitions, suspension/resume, unhealthy/degraded states, stalled work, and shutdown safety.

## Architectural rule

Lifecycle state answers "what is the agent doing?" Health state answers "is the agent operating normally?" Execution state answers "what is this specific operation doing?" These concerns remain separate.