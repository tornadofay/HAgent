# Phase 0.95 — Generic External Host Integration

## Goal
Complete the provider-neutral execution boundary required by arbitrary host applications without coupling HAgent.Core to a host domain, UI framework, scheduler, persistence model, or side-effect system.

## Steps

1. [x] Define a canonical `AgentExecutionRequest` carrying multiple messages, host correlation identity, bounded host context, and execution options.
2. [ ] Preserve host correlation identity through tool execution and correlation metadata.
3. [ ] Define a host-owned structured-output request/validation contract.
4. [ ] Validate structured output independently of provider capability claims.
5. [ ] Strengthen terminal-state protection against late provider completion after cancellation, timeout, retirement, shutdown, or another terminal outcome.
6. [ ] Verify mutable runtime overrides and host context remain isolated in immutable execution snapshots.
7. [ ] Verify the canonical request through deterministic Example coverage.
8. [ ] Verify concurrent external consumption through a provider-neutral consumer scenario.

## Boundary rule

External hosts consume HAgent through the public provider-neutral API. Host correlation identity is distinct from HAgent execution and runtime-instance identities and is never encoded into model prompt text.

Host context is arbitrary host-owned data with bounded size. HAgent preserves it as context but does not assign domain meaning or treat it as authorization.

The legacy string-message execution overload remains a convenience API and delegates to the canonical request boundary.

## HWorld boundary

HWorld is an external consumer. HAgent does not contain an HWorld dependency, adapter, physics, rendering, simulation-time, or action-authority code. HWorld references HAgent and owns its own domain lifecycle, scheduling, state, authorization, and side effects.

## Exit criterion

A host can submit a complete provider-neutral execution request with bounded context and host correlation, receive an immutable execution snapshot, and rely on HAgent to keep host identity distinct from runtime and execution identity.
