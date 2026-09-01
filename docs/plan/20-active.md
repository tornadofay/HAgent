# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.95 Generic External Host Integration

### Objective
Complete the generic host boundary required for a broad class of LLM-driven software. HAgent must accept host-owned execution input/context and correlation identity, preserve them in immutable execution state, support structured-output contracts with real validation, and remain independent of any host domain, UI, scheduler, persistence model, or side-effect system.

### Current slices

- [x] Reusable runtime-agent profiles and live runtime instances remain separate and verified through the 0.9 runtime foundation.
- [x] Provider-neutral workspace/routing foundation exists for later host coordination; workspace UI/execution remains deferred to Phase 0.10.
- [x] Canonical `AgentExecutionRequest` supports multiple messages, host correlation identity, bounded host context, runtime overrides, and normal execution options.
- [x] Preserve host correlation identity through relevant tool execution/correlation metadata.
- [x] Define host-owned structured-output request and validation contract.
- [x] Validate structured output independently of provider claims.
- [x] Strengthen terminal-state protection against late provider completion after cancellation, timeout, retirement, shutdown, or another terminal outcome. Implementation and deterministic Example verification are complete.
- [x] Verify execution snapshots fully isolate mutable runtime overrides and host context.
- [x] Add deterministic Example verification for the generic host execution request.
- [x] Add deterministic Example verification for structured-output schema validation.
- [x] Define a distinct provider-facing `ProviderExecutionRequest` boundary.
- [x] Route normal, tool-calling, and streaming provider interfaces through `ProviderExecutionRequest`.
- [x] Migrate in-repository Example and Core test adapters to the `ProviderExecutionRequest` contract.
- [x] Add deterministic Example verification that provider-facing requests preserve host-owned structured-output requirements.
- [ ] Use provider-facing structured-output requirements for provider-native constrained generation where supported. Implementation landed; Example native/fallback transport verification pending.
- [ ] Add deterministic concurrent external-consumer verification without introducing HWorld-specific dependencies.
- [ ] Run the full 0.95 verification pass and close the phase only after all cross-cutting slices pass.

### Generic host boundary

`AgentExecutionRequest` is the canonical host-facing provider-neutral execution request. It identifies the target reusable agent profile, carries an ordered message list, accepts a host-supplied correlation identity, carries a bounded host context dictionary, optional structured-output requirements, and reuses `AgentExecutionOptions` for execution policies and runtime overrides.

Host correlation is distinct from HAgent execution correlation and runtime-instance identity. It is never embedded into prompt text and is retained through tool execution metadata as a separate identity.

Host context is copied into the immutable execution snapshot. HAgent does not assign domain meaning to the context and does not use it as an authorization mechanism.

### Provider boundary

`ProviderExecutionRequest` is the provider-facing contract. It contains the resolved provider, execution agent snapshot, secret material needed only for the adapter call, composed system prompt, bounded messages, optional structured-output requirement, optional tools, and optional streaming progress sink.

This boundary is intentionally separate from `AgentExecutionRequest`. Host callers do not need to know provider transport details, while provider adapters can evolve their supported capabilities without expanding the host-facing model.

The normal, tool-calling, and streaming provider adapter contracts consume `ProviderExecutionRequest`. All in-repository adapter implementations and deterministic test doubles now use the request-object contract. The current OpenAI-compatible implementation retains legacy overloads only as internal compatibility helpers; the adapter interfaces themselves use the request object.

Provider adapters must normalize responses into `AIResponse`, and HAgent remains responsible for provider-neutral validation of host-owned structured-output contracts after normalization. The OpenAI-compatible adapter now attempts native `response_format`/JSON Schema transport when structured output is requested; if the endpoint explicitly reports that feature as unsupported, it retries using the ordinary request shape and marks the fallback in provider metadata.

### Runtime terminal outcome

`AgentExecution` owns the first-terminal-outcome-wins gate. HAgent may finish caller-visible cancellation or timeout before a non-cooperative provider task returns. Such a late provider task cannot replace the terminal execution state or response, and provider faults from detached late work are observed so they do not surface as unobserved task failures.

### HWorld boundary

HWorld is an external consumer, not an HAgent dependency. HWorld should reference HAgent normally and use the public canonical request/runtime APIs. HAgent must not add HWorld-specific adapters, world types, physics, simulation scheduling, or action authority.

### Next phases

Workspace execution/chat belongs to Phase 0.10. Knowledge, Skills, Wiki/content management, and Learning governance belong to the dedicated Phase 0.11 and are no longer treated as part of the 0.95 implementation milestone.

## Verification rule

A 0.95 slice becomes complete only after its implementation exists, its matching `HAgent.Example` verification passes locally, and the authoritative documentation reflects the result.
