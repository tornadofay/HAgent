# Phase 0.96 — Capability-Aware Execution

## Status

**Planned next — provider/runtime hardening before resuming Phase 0.10.**

## Goal

Make HAgent safe and useful across heterogeneous providers, accounts, deployments, models, modalities, quotas, rate limits, concurrency limits, and very different inference latencies without binding an agent profile to one provider/model.

Real provider testing exposed that a provider may expose many models with different capabilities and limits, while the same logical model may be available through multiple providers. HAgent must therefore reason about the **actual execution target** for each request rather than treating provider or model names as sufficient capability descriptions.

## Core model

The phase separates these concepts:

```text
Agent Profile
    = what the agent is, what it requires, and what it prefers

Provider
    = a provider/service integration

Provider Account / Project / Endpoint
    = an operational execution environment

Model / Logical Model
    = provider-independent model identity where it can be established

Model Deployment / Execution Target
    = a concrete provider + account/project/endpoint + model deployment

Capability
    = what the execution target can do

Constraint
    = request/model limits such as context, output, image count, or schema size

Quota / Rate Limit
    = operational consumption limits over windows

Availability / Health
    = whether the target can accept work now

Execution Planner
    = selects the best currently compatible execution target
```

**Architectural terminology rule:** the **Execution Planner** is not the cognitive planner. The Execution Planner answers *where/how should an already-requested inference execute?* The cognitive **Planner** in Phase 0.97 answers *what should the agent do?* These layers must remain independent even when both perform candidate selection and scoring.

## Requirements

1. [ ] Remove permanent provider/model binding from reusable Agent profiles. Existing provider/model configuration must be migrated to preference/requirement semantics without losing backward compatibility unnecessarily.
2. [ ] Preserve Agent profiles as first-class configuration containing identity, instructions, tools, memory/knowledge/skills policy, capability requirements, execution preferences, and fallback/degradation policy.
3. [ ] Represent Provider independently from Model and from concrete execution endpoint/account/project/deployment.
4. [ ] Introduce provider-independent logical model identity where reliably known, while preserving provider-native model identifiers and deployment identity.
5. [ ] Allow the same logical model to be exposed by multiple providers, including different endpoints/accounts/projects, without treating those executions as equivalent.
6. [ ] Define normalized execution-target identity covering provider, endpoint/account/project, model identifier, model version/revision where available, and relevant routing/deployment identity.
7. [ ] Extend the existing tri-state capability system to exact execution targets: `Supported`, `Unsupported`, `Unknown`.
8. [ ] Separate capability from operational state, account/project permission, quota, rate limiting, health, and request-specific constraints.
9. [ ] Model input/output modalities explicitly rather than using one generic vision/image flag. Support extensibility for text, image, audio, video, embeddings, generation, understanding, and future modalities.
10. [ ] Represent capability evidence, confidence, source, observation time, and expiration/refresh information.
11. [ ] Support capability evidence from provider metadata, provider documentation supplied through adapters, discovery APIs, controlled probes, successful executions, failures, and response metadata.
12. [ ] Cache capability knowledge without treating stale or undocumented capability data as authoritative.
13. [ ] Define request-side capability requirements independently from agent identity. Requirements must support at least required, preferred, optional, and forbidden semantics.
14. [ ] Support requirements for structured output, strict structured output, tool calling, reasoning, modalities, streaming, embeddings, and future capabilities.
15. [ ] Distinguish native capability from emulated/degraded behavior. Do not report prompt-based JSON fallback as equivalent to native constrained structured output.
16. [ ] Make fallback/degradation policy explicit: fail, wait, try another candidate, or use an explicitly permitted degraded mode.
17. [ ] Validate manually selected provider/model/execution targets against request and agent requirements before sending provider requests.
18. [ ] Introduce a capability-aware Execution Planner that evaluates candidate execution targets before transport.
19. [ ] Score/filter candidates by required capabilities, preferred logical model, preferred provider, explicit host/runtime selection, policy, availability, limits, expected latency, and other execution preferences without mutating the Agent profile.
20. [ ] Keep provider-native transport behind the existing `ProviderExecutionRequest` boundary.
21. [ ] Introduce normalized generic rate/quota dimensions rather than hard-coding only RPM/RPD/TPM/TPD.
22. [ ] Support at minimum request count, input tokens, output tokens, total tokens, concurrency, and future dimensions such as audio duration, image count, bytes, spend, or provider-specific units.
23. [ ] Support arbitrary windows including per-minute, per-day, and provider-specific/custom windows.
24. [ ] Support limits at the scope actually enforced by a provider, including account, organization, project, endpoint, model/deployment, or other documented scope.
25. [ ] Distinguish configured limits from observed remaining capacity and provider-reported reset information.
26. [ ] Parse provider rate-limit and retry metadata where available and reconcile observed state with HAgent's admission state.
27. [ ] Implement proactive rate/quota admission before provider transport so HAgent does not intentionally discover ordinary limits by sending doomed requests.
28. [ ] Implement atomic reservation/admission for concurrent executions so two requests cannot both consume the same remaining budget.
29. [ ] Reconcile reservations with actual provider usage after execution, including partial/unknown usage when provider telemetry is incomplete.
30. [ ] Support `Wait`, `TryNextCandidate`, `Fail`, and explicitly policy-controlled degraded behavior when capacity is insufficient.
31. [ ] Support maximum queue/admission wait so a theoretically available future target does not cause unbounded waiting.
32. [ ] Treat provider `429`, throttling, exhaustion, and quota failures as feedback for the operational state rather than as the only capability discovery mechanism.
33. [ ] Track request latency and execution duration separately from rate/quota state.
34. [ ] Model long-running inference targets where a single request may legitimately take minutes without treating slow response as provider failure.
35. [ ] Ensure caller cancellation, timeout, and late-result protection remain correct while a long-running request is waiting, executing, or being retried/fallback-routed.
36. [ ] Support explicit concurrency capacity such as one-at-a-time or bounded in-flight requests for providers/models that have limited serving capacity even when daily quota is high or unlimited.
37. [ ] Distinguish `quota available` from `execution capacity available`. A target may have abundant daily quota but still require serialization or waiting because inference is slow or concurrency-limited.
38. [ ] Provide target health/availability state and backoff hints without permanently blacklisting a provider because of transient failures.
39. [ ] Preserve provider neutrality: do not hard-code Groq, Cloudflare, NVIDIA, OpenRouter, or any other provider's model matrix into HAgent.Core.
40. [ ] Allow providers to supply provider-specific discovery/capability adapters while HAgent.Core consumes only normalized contracts.
41. [ ] Support arbitrary OpenAI-compatible endpoints whose capabilities may be partially known or completely unknown.
42. [ ] Handle providers with multiple task families and model catalogs, including text generation, image generation, image-to-text, embeddings, speech, classification, and future task types.
43. [ ] Preserve independent provider/model capability snapshots for multiple environments even when the logical model name is identical.
44. [ ] Expose enough planner diagnostics for a host/UI to explain why a candidate was accepted, rejected, delayed, or degraded.
45. [ ] Add deterministic Example verification for identical logical models exposed through multiple providers, required/preferred/optional capabilities, unknown capabilities, incompatible manual selection, structured-output native vs fallback behavior, proactive rate limiting, daily quota, token windows, atomic concurrent reservations, 429 feedback, long-running requests, cancellation, timeout, stale-result protection, and candidate fallback.
46. [ ] Update management UI targets so provider/model selection shows effective capabilities, constraints, quota/rate state, availability, and compatibility with the active request rather than only listing model names.
47. [ ] Ensure the Workspace/provider/model selection planned for Phase 0.10 consumes this capability planner rather than bypassing it.

## Agent configuration direction

A reusable Agent profile remains first-class. Its execution configuration becomes requirements and preferences rather than a permanent transport binding:

```text
Agent
    Identity / instructions
    Tools
    Memory / Knowledge / Skills policy

    Required capabilities
    Preferred capabilities

    Preferred logical model (optional)
    Preferred provider (optional)

    Fallback / degradation policy
```

A host or runtime may explicitly choose a provider/model/deployment for one execution, but HAgent validates compatibility before transport. Runtime and conversation overrides do not mutate the persistent profile.

## Same model, multiple providers

The same logical model may appear through different providers:

```text
Logical Model: M

Groq       -> Deployment A
OpenRouter -> Deployment B
Local      -> Deployment C
```

The deployments are separate execution targets because they may differ in capabilities, limits, pricing, routing, permissions, latency, availability, and operational state. HAgent may use the logical model as a preference while selecting among compatible concrete deployments.

## Capability and limitation layers

HAgent must keep these dimensions separate:

```text
Capability
    Can it do the operation?

Constraint
    Can it do this size/shape/version of the operation?

Permission
    Is this account/project allowed to use it?

Quota / Rate Limit
    Is there remaining budget in the applicable window?

Concurrency / Capacity
    Can it accept another request now?

Availability / Health
    Is the target currently usable?

Latency
    How long may the operation reasonably take?
```

A target may therefore be capable but temporarily unavailable, or available but incompatible with a required feature.

## Execution-target assessment

The planner should expose a normalized assessment for every candidate considered:

```text
ExecutionTargetAssessment
    Target identity
    Compatible / incompatible
    Capability evidence
    Constraint checks
    Permission state
    Quota state
    Capacity state
    Health / availability
    Estimated latency
    Wait-until (optional)
    Degradation available (optional)
    Score / ranking information
    Decision reason
```

This assessment is diagnostic data, not merely logging. It allows hosts and management UI to explain why an execution target was accepted, rejected, delayed, or degraded without knowing provider-specific implementation details.

## Rate limiting and admission

Rate limiting is proactive admission control, not merely retry logic.

The intended flow is:

```text
Execution Request
      |
      v
Capability requirements
      |
      v
Candidate discovery/filtering
      |
      v
Policy/preferences scoring
      |
      v
Quota/rate/concurrency admission
      |
      +---- wait
      +---- try another candidate
      +---- fail
      |
      v
ProviderExecutionRequest
      |
      v
Provider
      |
      v
Observed usage / limits / retry metadata
      |
      v
Reconcile planner state
```

Concurrent requests must reserve capacity atomically before transport. Actual usage then reconciles the reservation. Provider-specific headers and errors are evidence used to improve operational state.

## Long-running execution

A provider with high or effectively uncapped daily quota is not necessarily a high-throughput provider. HAgent must allow:

```text
Daily quota: abundant
Concurrent capacity: 1
Typical latency: 2-5 minutes
```

without incorrectly retrying, timing out, or flooding the provider.

Long-running execution must remain asynchronous to the host. Cancellation and timeout policy remain host/runtime execution concerns. Slow inference must not block unrelated agent/runtime executions, while concurrency admission prevents excessive in-flight work against a slow target.

## External provider examples

The design intentionally covers providers with substantially different operating models.

Cloudflare Workers AI exposes multiple task families with task/model-specific limits and a daily Neuron allocation, while some frontier models have distinct per-account/per-model limits. HAgent must therefore model task capability, per-target constraints, quota units, and scope rather than assuming generic LLM RPM/TPM semantics.

NVIDIA's current model catalog includes free/downloadable endpoints and multimodal/reasoning/tool-use models, while hosted inference may still encounter rate limiting. A free or high-quota endpoint can therefore be represented as a normal execution target with its own capacity, latency, and observed operational state rather than being treated as unlimited throughput.

OpenRouter or another routing provider may itself route across upstream providers. HAgent should treat it as an execution provider/endpoint with its own capabilities and operational limits; HAgent must not assume that its upstream model route is identical to a direct provider deployment.

## Provider implementation boundary

Provider adapters may implement provider-specific discovery and telemetry:

```text
IProviderCapabilitySource
IProviderUsageSource
IProviderRateLimitSource
```

or equivalent provider contracts as the implementation requires. HAgent.Core consumes normalized capability, constraint, quota, and availability records.

No provider-specific model matrix belongs in Core.

## Non-goals

This phase does not make HAgent responsible for provider pricing truth, billing, or host scheduling policy. It does provide normalized planning/admission primitives so HAgent can prevent avoidable incompatible or over-limit executions while leaving final application policy with the host/runtime.

## Exit criterion

A reusable Agent profile can run unchanged across materially different provider environments. For each execution, HAgent can discover or evaluate candidate targets, determine whether required capabilities are supported, enforce request-specific constraints and policy, account for quota/rate/concurrency capacity, wait or select another compatible target when appropriate, tolerate long-running inference, preserve cancellation/timeout/stale-result safety, and explain the final execution-target decision. The same logical model may be exposed by multiple providers without collapsing their operational identities.

## Architectural additions for Phase 0.97 compatibility

0.96 establishes the execution-side planner only. It must expose contracts that 0.97 can consume without coupling cognitive reasoning to provider infrastructure.

The boundary should remain conceptually:

```text
Cognitive Policy / Planner
        |
        | "I need an inference execution with these requirements"
        v
AgentExecutionRequest
        |
        v
Execution Planner
        |
        v
ExecutionTargetAssessment
        |
        v
ProviderExecutionRequest
```

The cognitive layer must not select or depend directly on provider-specific rate-limit implementations. It expresses requirements/preferences; the Execution Planner handles concrete target selection and admission.

The planner should therefore treat execution requirements as data, not as model/provider names embedded in cognitive policies. This allows a future `LlmPolicy` or `LlmPlanner` to request, for example, `StructuredOutput + ToolCalling + LowLatency`, while 0.96 independently decides which concrete deployment can satisfy that request.
