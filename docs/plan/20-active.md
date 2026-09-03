# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.96 Capability-Aware Execution — NEXT

Phase 0.96 is the current hardening target. Phase 0.10 Workspaces, Routing + Chat is paused after its verified provider-neutral routing and coordinator/specialist role-policy foundation. Workspace implementation must resume only after execution target capability, quota, admission, and long-running execution behavior are robust enough to support heterogeneous environments.

### Objective

Make HAgent select and execute against the right concrete provider/model deployment for each request without permanently binding reusable Agent profiles to one provider/model, while respecting capabilities, request-specific constraints, permissions, quotas, rate limits, concurrency capacity, availability, latency, fallback policy, and cancellation/timeout semantics.

### Current slices

- [ ] Separate reusable Agent profile requirements/preferences from provider/model transport binding.
- [ ] Model Provider, account/project/endpoint, logical model, and concrete execution target as distinct concepts.
- [ ] Support the same logical model through multiple providers without collapsing their operational identities.
- [ ] Extend tri-state capability knowledge to exact execution targets: Supported / Unsupported / Unknown.
- [ ] Model input/output modalities and future task types explicitly.
- [ ] Separate capability, constraint, permission, quota/rate capacity, concurrency/capacity, availability/health, and latency.
- [ ] Record capability evidence, confidence, source, observation time, and expiration/refresh state.
- [ ] Add provider/discovery/probe evidence and capability caching without hard-coded provider matrices in Core.
- [ ] Define request-side capability requirements using Required / Preferred / Optional / Forbidden semantics.
- [ ] Distinguish native capability from emulated/degraded behavior, especially structured output.
- [ ] Define explicit fallback/degradation policy rather than silently degrading requirements.
- [ ] Add capability-aware Execution Planner and candidate scoring/filtering.
- [ ] Validate manually selected provider/model/deployment against the same compatibility policy as automatic selection.
- [ ] Keep provider-native transport behind `ProviderExecutionRequest`.
- [ ] Introduce generic quota/rate dimensions including request count, input tokens, output tokens, total tokens, and concurrency.
- [ ] Support arbitrary time windows including minute/day/provider-specific windows.
- [ ] Support limits scoped to organization, account, project, endpoint, model/deployment, or provider-defined scope.
- [ ] Reconcile configured limits with provider-reported remaining/reset information and runtime observations.
- [ ] Implement proactive rate/quota admission before provider transport.
- [ ] Add atomic concurrent reservations and post-execution usage reconciliation.
- [ ] Support Wait / TryNextCandidate / Fail / policy-approved degraded behavior with bounded admission wait.
- [ ] Treat 429/throttling/quota failures as operational feedback, not the primary capability discovery mechanism.
- [ ] Track latency and execution duration separately from quota/rate state.
- [ ] Support long-running targets with multi-minute requests without treating slow inference as automatic failure.
- [ ] Separate quota availability from execution capacity and support bounded concurrency/serialization where required.
- [ ] Preserve async execution, cancellation, timeout, and stale-result safety while waiting, executing, retrying, or falling back.
- [ ] Add transient health/backoff state without permanent blacklisting from temporary failures.
- [ ] Support arbitrary OpenAI-compatible endpoints with partial or unknown capability information.
- [ ] Allow provider-specific capability/usage/rate-limit discovery adapters behind normalized Core contracts.
- [ ] Add planner diagnostics explaining candidate acceptance/rejection, waiting, fallback, or degradation.
- [ ] Add deterministic Example verification for multi-provider same-model routing, capability requirements, unknown capabilities, manual incompatibility, structured-output native/fallback, rate/quota admission, concurrent reservation, 429 feedback, long-running requests, cancellation, timeout, stale results, and fallback.
- [ ] Update management UI to show execution-target identity, capabilities, constraints, quota/rate state, availability, latency, and request compatibility.
- [ ] Ensure Phase 0.10 Workspace provider/model selection consumes the planner and cannot bypass capability/admission policy.

### Agent configuration direction

A reusable Agent profile remains first-class and contains the agent's identity and cognitive configuration plus requirements/preferences:

```text
Agent
    identity / instructions
    tools
    memory / knowledge / skills policy
    required capabilities
    preferred capabilities
    preferred logical model (optional)
    preferred provider (optional)
    fallback / degradation policy
```

Persistent Agent profiles do not permanently bind transport to one provider/model. A host or runtime may select a concrete execution target for a particular execution, but HAgent validates that target against the request and agent requirements before transport. Runtime/conversation overrides remain non-mutating.

### Execution-target direction

```text
Provider
    service integration

Provider account / project / endpoint
    operational environment

Logical Model
    provider-independent model identity where reliably established

Model Deployment / Execution Target
    provider + account/project/endpoint + model deployment

Execution Planner
    compatibility + policy + capacity + latency decision
```

The same logical model may exist at several providers or endpoints. Each concrete deployment remains independently characterized by capability, limits, quota, health, latency, permissions, and routing behavior.

### Capability and admission direction

```text
AgentExecutionRequest
        |
        v
Requirements + preferences
        |
        v
Capability/constraint compatibility
        |
        v
Candidate execution targets
        |
        v
Policy/preference scoring
        |
        v
Quota/rate/concurrency admission
        |
        +--> Wait
        +--> Try another target
        +--> Fail
        +--> Explicit degraded mode
        |
        v
ProviderExecutionRequest
        |
        v
Provider adapter
        |
        v
Observed usage / limits / health / latency
        |
        v
Reconcile planner state
```

### Long-running provider direction

Providers may offer abundant or effectively unlimited daily quota while having low concurrency capacity and multi-minute inference. HAgent must represent these independently:

```text
quota available
    !=
execution capacity available

free/high-quota
    !=
fast/high-throughput
```

Long-running inference remains asynchronous. A slow target may still be the correct target when the request's latency policy permits it. Unrelated runtime executions must remain able to proceed.

Cloudflare Workers AI currently exposes multiple task families, model-specific rate limits, and a daily free Neuron allocation; some frontier models have distinct per-account/per-model limits. NVIDIA's current model catalog contains free/downloadable endpoints and multimodal/reasoning/tool-use models, while hosted NVIDIA services can still produce rate-limit responses. These environments are direct validation cases for this architecture. citeturn147698search0turn147698search2turn513772search0turn720373search0

### Boundaries

0.96 is generic runtime hardening. No Groq, Cloudflare, NVIDIA, OpenRouter, or other provider model matrix belongs in HAgent.Core. Provider-specific knowledge remains in provider/discovery adapters and normalized runtime records.

HAgent provides generic planning and admission primitives but does not become the source of billing truth or replace host scheduling policy.

### Verification rule

A 0.96 slice becomes complete only after its implementation exists, matching deterministic Example verification passes locally, and the authoritative documentation reflects the result. Do not claim local build/test success unless it was actually performed.
