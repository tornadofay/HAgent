# Phase 0.95 — Generic External Host Integration

## Goal
Complete the provider-neutral execution boundary required by arbitrary host applications without coupling HAgent.Core to a host domain, UI framework, scheduler, persistence model, or side-effect system.

## Steps

1. [x] Define a canonical `AgentExecutionRequest` carrying multiple messages, host correlation identity, bounded host context, runtime overrides, and execution options.
2. [x] Preserve host correlation identity through tool execution and correlation metadata.
3. [x] Define a host-owned `StructuredOutputOptions` request contract and HAgent validation boundary.
4. [x] Validate structured output independently of provider capability claims.
5. [x] Strengthen terminal-state protection against late provider completion after cancellation, timeout, retirement, shutdown, or another terminal outcome.
6. [x] Verify mutable runtime overrides and host context remain isolated in immutable execution snapshots.
7. [x] Verify the canonical request through deterministic Example coverage.
8. [ ] Verify concurrent external consumption through a provider-neutral consumer scenario.
9. [x] Define a distinct provider-facing `ProviderExecutionRequest` boundary separate from the host-facing request.
10. [x] Route normal, tool-calling, and streaming provider adapter contracts through `ProviderExecutionRequest`.
11. [x] Verify provider-facing propagation of host-owned structured-output requirements with a deterministic Example adapter.
12. [ ] Use provider-facing structured-output requirements for provider-native constrained generation where supported. Native/fallback implementation and deterministic Example transport verification are pending local test.
13. [ ] Run the full 0.95 verification pass and close the phase only after all cross-cutting slices pass.

## Boundary rule

External hosts consume HAgent through the public provider-neutral API. `AgentExecutionRequest` is the host-facing boundary; `ProviderExecutionRequest` is the provider-facing boundary created by HAgent after agent/provider resolution.

Host correlation identity is distinct from HAgent execution and runtime-instance identities and is never encoded into model prompt text. It remains distinct through tool execution metadata.

Host context is arbitrary host-owned data with bounded size. HAgent preserves it as context but does not assign domain meaning or treat it as authorization.

Structured-output schemas are owned by the host. HAgent validates normalized provider output regardless of provider capability claims. Provider adapters receive the structured-output requirement through `ProviderExecutionRequest`, enabling provider-native constrained generation without leaking transport details into the host-facing contract.

The OpenAI-compatible adapter currently attempts the native `response_format`/JSON Schema request shape when structured output is requested. If the endpoint explicitly reports `response_format` as unsupported or unknown, the adapter retries using the ordinary request shape. The normalized response remains subject to the same HAgent validation in either case, and provider metadata records whether native transport or fallback was used.

The legacy string-message execution overload remains a convenience API and delegates to the canonical host-facing request boundary.

## HWorld boundary

HWorld is an external consumer. HAgent does not contain an HWorld dependency, adapter, physics, rendering, simulation-time, or action-authority code. HWorld references HAgent and owns its own domain lifecycle, scheduling, state, authorization, and side effects.

## Exit criterion

A host can submit a complete provider-neutral execution request with bounded context, host correlation, and optional structured-output requirements; HAgent resolves it into a provider-facing request, invokes an adapter, normalizes the response, validates host-owned contracts, and preserves execution identity without coupling to host or provider-specific domain models.
