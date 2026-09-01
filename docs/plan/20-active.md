# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.95 Generic External Host Integration

### Objective
Complete the generic host boundary required for a broad class of LLM-driven software. HAgent must accept host-owned execution input/context and correlation identity, preserve them in immutable execution state, support structured-output contracts with real validation, and remain independent of any host domain, UI, scheduler, persistence model, or side-effect system.

### Current slices

- [x] Reusable runtime-agent profiles and live runtime instances remain separate and verified through the 0.9 runtime foundation.
- [x] Provider-neutral workspace/routing foundation exists for later host coordination; workspace UI/execution remains deferred to Phase 0.10.
- [x] Canonical `AgentExecutionRequest` supports multiple messages, host correlation identity, bounded host context, and normal execution options.
- [x] Preserve host correlation identity through relevant tool execution/correlation metadata.
- [x] Define host-owned structured-output request and validation contract.
- [x] Validate structured output independently of provider claims.
- [ ] Strengthen terminal-state protection against late provider completion after cancellation, timeout, retirement, shutdown, or another terminal outcome.
- [ ] Verify execution snapshots fully isolate mutable runtime overrides and host context.
- [x] Add deterministic Example verification for the generic host execution request.
- [x] Add deterministic Example verification for structured-output schema validation.
- [ ] Add deterministic concurrent external-consumer verification without introducing HWorld-specific dependencies.
- [ ] Run the full 0.95 verification pass and close the phase only after all cross-cutting slices pass.

### Generic host boundary

`AgentExecutionRequest` is the canonical provider-neutral execution request. It identifies the target reusable agent profile, carries an ordered message list, accepts a host-supplied correlation identity, carries a bounded host context dictionary, and reuses `AgentExecutionOptions` for execution policies and runtime overrides.

Host correlation is distinct from HAgent execution correlation and runtime-instance identity. It is never embedded into prompt text and is retained through tool execution metadata as a separate identity.

Host context is copied into the immutable execution snapshot. HAgent does not assign domain meaning to the context and does not use it as an authorization mechanism.

Structured output is a host-owned optional execution contract. `StructuredOutputOptions` carries the schema, and `StructuredOutputValidator` validates the normalized provider output against that schema independently of provider capability claims. The runtime rejects a response that does not satisfy the requested structured-output contract.

The existing string-based `ExecuteAsync(agentId, message, ...)` API remains a compatibility convenience and delegates to the canonical request boundary.

### HWorld boundary

HWorld is an external consumer, not an HAgent dependency. HWorld should reference HAgent normally and use the public canonical request/runtime APIs. HAgent must not add HWorld-specific adapters, world types, physics, simulation scheduling, or action authority.

### Next phases

Workspace execution/chat belongs to Phase 0.10. Knowledge, Skills, Wiki/content management, and Learning governance belong to the dedicated Phase 0.11 and are no longer treated as part of the 0.95 implementation milestone.

## Verification rule

A 0.95 slice becomes complete only after its implementation exists, its matching `HAgent.Example` verification passes locally, and the authoritative documentation reflects the result.
