# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.95 Generic External Host Integration

### Objective
Complete the generic host boundary required for a broad class of LLM-driven software. HAgent must accept host-owned execution input/context and correlation identity, preserve them in immutable execution state, support structured-output contracts with real validation, and remain independent of any host domain, UI, scheduler, persistence model, or side-effect system.

### Current slices

- [x] Reusable runtime-agent profiles and live runtime instances remain separate and verified through the 0.9 runtime foundation.
- [x] Provider-neutral workspace/routing foundation exists for later host coordination; workspace UI/execution remains deferred to Phase 0.10.
- [x] Canonical `AgentExecutionRequest` supports multiple messages, host correlation identity, bounded host context, and normal execution options.
- [ ] Preserve host correlation identity through all relevant tool execution/correlation metadata.
- [ ] Define host-owned structured-output request and validation contract.
- [ ] Validate structured output independently of provider claims.
- [ ] Strengthen terminal-state protection against late provider completion after cancellation, timeout, retirement, shutdown, or another terminal outcome.
- [ ] Verify execution snapshots fully isolate mutable runtime overrides and host context.
- [ ] Add deterministic Example verification for the generic host execution request.
- [ ] Add deterministic concurrent external-consumer verification without introducing HWorld-specific dependencies.

### Generic host boundary

`AgentExecutionRequest` is the canonical provider-neutral execution request. It identifies the target reusable agent profile, carries an ordered message list, accepts a host-supplied correlation identity, carries a bounded host context dictionary, and reuses `AgentExecutionOptions` for execution policies and runtime overrides.

Host correlation is distinct from HAgent execution correlation and runtime-instance identity. It is never embedded into prompt text.

Host context is copied into the immutable execution snapshot. HAgent does not assign domain meaning to the context and does not use it as an authorization mechanism.

The existing string-based `ExecuteAsync(agentId, message, ...)` API remains a compatibility convenience and delegates to the canonical request boundary.

### HWorld boundary

HWorld is an external consumer, not an HAgent dependency. HWorld should reference HAgent normally and use the public canonical request/runtime APIs. HAgent must not add HWorld-specific adapters, world types, physics, simulation scheduling, or action authority.

### Deferred work

Skills, Wiki/content integration, Learning governance, and their management UI remain intentionally deferred. Phase 0.10 workspace execution/chat also remains deferred until this cross-cutting host boundary is stable.

## Verification rule

A 0.95 slice becomes complete only after its implementation exists, its matching `HAgent.Example` verification passes locally, and the authoritative documentation reflects the result.
