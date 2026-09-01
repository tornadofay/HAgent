# Phase 0.95 — Generic External Host Integration

## Goal
Make HAgent a complete generic LLM cognition/execution boundary for different project types without introducing application-specific domain concepts into Core.

## Requirements

1. [ ] Introduce a canonical generic `AgentExecutionRequest` that can carry host-supplied input/context without requiring plain string message as the fundamental execution model.
2. [ ] Keep plain string message execution as a convenience overload over the generic request.
3. [ ] Add host-supplied correlation identity distinct from `AgentExecution.Id` and `AgentRuntimeInstance.InstanceId`.
4. [ ] Propagate execution, runtime-instance, and host correlation identity into tool execution context.
5. [ ] Define a generic structured-output request contract with host-owned schema input.
6. [ ] Request structured output through capable provider adapters rather than merely detecting valid JSON after generation.
7. [ ] Validate returned structured output against the requested schema and expose normalized validation/result metadata.
8. [ ] Ensure provider capability reporting distinguishes structured output `Supported`, `Unsupported`, and `Unknown`.
9. [ ] Make execution terminal-state transitions race-safe so late provider completion cannot overwrite cancellation, timeout, retirement, shutdown, or another terminal outcome.
10. [ ] Preserve independent runtime-instance identity, overrides, execution state, and private memory ownership when multiple instances originate from one profile.
11. [ ] Snapshot runtime overrides at the instance boundary so mutable caller-owned override objects cannot create cross-instance state coupling.
12. [ ] Keep host scheduling external; HAgent may provide focused admission-control primitives only.
13. [ ] Preserve host ownership of domain state, persistence, authorization, scheduling policy, and side effects.
14. [ ] Add deterministic Example verification for generic execution input, host correlation, structured output, late completion protection, tool identity propagation, concurrent runtime instances, and memory isolation.

## API direction

The intended public integration shape is:

```text
Host
  -> AgentExecutionRequest
  -> AgentRuntimeInstance
  -> HAgentClient.ExecuteAsync(...)
  -> AgentExecution
```

The request should remain generic. HAgent must not define host-domain schemas, event types, command types, domain objects, or lifecycle policy.

## Runtime invariant

One reusable profile may produce many long-lived runtime instances. Every runtime instance remains independently addressable and owns its own runtime lifecycle, execution revision, override snapshot, shutdown signaling, and private memory ownership.

## Completion invariant

An execution that has reached a terminal outcome cannot later publish a conflicting outcome because a provider completed late.

## Structured-output invariant

A structured-output response is valid only when the requested contract is successfully honored and validated. Arbitrary JSON text is not sufficient evidence that the contract was satisfied.

## Exit criterion

A generic external host can provide arbitrary bounded context, correlate requests, request validated structured output, expose host-owned tools, run multiple long-lived runtime instances concurrently, control cancellation/timeout/lifecycle, and safely ignore stale or late results without introducing host-specific concepts into HAgent.Core.
