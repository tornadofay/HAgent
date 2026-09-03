# Capability-Aware Execution

## Purpose

HAgent is provider-neutral, but provider environments are not equivalent. A provider can expose multiple models and task families, the same logical model can be reachable through multiple providers, and each concrete deployment can impose different capabilities, constraints, permissions, quotas, concurrency limits, availability, and latency.

HAgent therefore must select an **execution target** for a request rather than treating an Agent profile, provider name, or model name as the execution decision.

## Identity layers

```text
Agent Profile
    What the agent is, what it requires, and what it prefers.

Provider
    Service/provider integration.

Provider Account / Project / Endpoint
    Operational environment and policy boundary.

Logical Model
    Provider-independent model identity where reliably established.

Model Deployment / Execution Target
    Concrete provider + account/project/endpoint + model deployment.
```

The same logical model may be exposed by multiple execution targets. They must remain independently characterized because they may differ in capabilities, permissions, limits, quotas, routing behavior, health, and latency.

## Agent profile semantics

Reusable Agent profiles remain first-class. They should not permanently bind an agent to one provider/model transport target.

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

A host/runtime may explicitly select a concrete execution target for an execution, but HAgent validates the choice against request requirements and policy before transport. Runtime and conversation overrides do not mutate the persistent profile.

## Capability

Capability answers:

> Can this exact execution target perform this operation at all?

The normalized state is:

```text
Supported
Unsupported
Unknown
```

Capability is distinct from:

```text
Constraint      Can it satisfy this request's size/shape?
Permission      Is this account/project allowed to use it?
Quota           Is budget remaining in the applicable window?
Concurrency     Can another request be admitted now?
Availability    Is the target currently healthy/usable?
Latency         How long is the operation expected to take?
```

## Modalities

HAgent must model input and output modalities explicitly. A single `Vision=true` property is insufficient.

Examples:

```text
Text -> Text
Image + Text -> Text
Audio -> Text
Video -> Text
Text -> Image
Text -> Audio
Text -> Embedding
```

The contract must be extensible for future modalities and task families such as image classification, object detection, summarization, speech recognition, translation, and generation.

## Capability evidence

Capability records should retain evidence rather than pretending every fact is permanent:

```text
Capability
State
Confidence
Evidence source
Observed at
Expires / refresh at
```

Evidence can come from:

- provider metadata/discovery;
- provider-specific capability endpoints;
- documented provider/model metadata supplied by an adapter;
- controlled compatibility probes;
- successful executions;
- provider response metadata;
- failures or incompatibility responses.

No provider-specific model matrix belongs in HAgent.Core.

## Request requirements

Requests express requirements independently from agent identity.

Each capability requirement may be:

```text
Required
Preferred
Optional
Forbidden
```

Required means an incompatible target cannot execute the request. Preferred influences scoring. Optional does not block execution. Forbidden removes candidates that provide an unwanted capability when policy requires it.

A structured-output requirement must distinguish native constrained generation from weaker emulation. Prompt-generated JSON must not be represented as equivalent to provider-native schema-constrained output.

## Execution planner

The planner resolves a request to a concrete target:

```text
AgentExecutionRequest
        |
        v
Requirements + preferences
        |
        v
Capability / constraint compatibility
        |
        v
Candidate execution targets
        |
        v
Policy + preference scoring
        |
        v
Quota / rate / concurrency admission
        |
        +---- wait
        +---- select another candidate
        +---- fail
        +---- explicit degraded mode
        |
        v
ProviderExecutionRequest
        |
        v
Provider adapter
```

Candidate scoring can consider:

- required capability satisfaction;
- preferred capabilities;
- preferred logical model;
- preferred provider;
- explicit host/runtime selection;
- account/project policy;
- request constraints;
- availability/health;
- current quota/rate capacity;
- concurrency capacity;
- expected latency;
- configured cost/quality preferences when such metadata is available.

The planner chooses deployments, not abstract model names.

## Quota and rate limiting

Rate limiting is proactive admission control, not merely retry logic.

The normalized model must support arbitrary dimensions and windows. Minimum dimensions include:

```text
RequestCount
InputTokens
OutputTokens
TotalTokens
ConcurrentRequests
```

The model remains extensible to:

```text
AudioDuration
ImageCount
Bytes
Spend
ProviderSpecificUnits
```

Windows may be per-minute, per-day, or provider-specific/custom. Limits may apply at organization, account, project, endpoint, deployment, model, or another provider-defined scope.

Configured limits and observed provider capacity are distinct. Where a provider exposes remaining quota, reset times, retry-after values, or rate-limit headers, HAgent should reconcile those observations with its local admission state.

## Atomic admission

Concurrent executions cannot independently observe the same remaining budget and both assume they can consume it.

Admission must therefore support atomic reservation:

```text
TryReserve(request cost)
       |
       +-- denied -> wait / next candidate / fail
       |
       +-- granted -> provider execution
                         |
                         v
                  actual usage observed
                         |
                         v
                  reservation reconciled
```

Actual usage may differ from estimates. The system must reconcile successful provider-reported usage and bounded unknown/partial usage when telemetry is incomplete.

## Waiting and fallback

When capacity is temporarily unavailable, policy may allow:

```text
Wait
TryNextCandidate
Fail
Use explicit degraded mode
```

Admission waiting must have a bounded maximum. A provider should not be blacklisted permanently because of one transient error.

A fallback is valid only when the fallback target independently satisfies required capabilities and request constraints.

## Long-running inference

Quota availability does not imply throughput or low latency.

A valid target may have:

```text
Daily quota: abundant
Concurrency: 1
Typical latency: several minutes
```

HAgent must represent these separately. Long-running inference remains asynchronous, unrelated executions must continue, and concurrency admission prevents excessive in-flight requests against a slow target.

Cancellation and timeout semantics remain active while work is queued, executing, retried, or waiting for capacity. Existing terminal-state and stale-result protection remains authoritative.

## Provider examples

Cloudflare Workers AI is a direct example of task/model heterogeneity: current documentation lists different limits by task type and model, as well as a daily free Neuron allocation and separate limits for certain frontier models. This requires modeling task capability, quota units, rate limits, and scopes independently. citeturn147698search0turn147698search2

NVIDIA's current hosted model catalog contains free/downloadable endpoints across text, reasoning, multimodal, tool-use, and other workloads, and NVIDIA documentation also describes rate limiting for hosted workloads. A free endpoint therefore cannot be interpreted as unlimited throughput or guaranteed low latency. citeturn513772search0turn720373search0

OpenRouter or similar routing services must be treated as providers/endpoints with their own operational identity. HAgent must not assume a route through such a service is operationally identical to direct access to the same underlying model.

## Provider adapter boundary

Provider-specific knowledge may live behind normalized contracts equivalent to:

```text
IProviderCapabilitySource
IProviderUsageSource
IProviderRateLimitSource
```

The exact API can evolve during implementation. Core consumes normalized capability, constraint, quota, availability, and latency records.

## UI and diagnostics

Management surfaces should show:

```text
Execution target
Capabilities
Constraints
Permission status
Quota/rate state
Concurrency capacity
Health/availability
Latency observations
Compatibility with active request
Decision/rejection reason
```

This is particularly important when a user manually selects a provider/model. The UI should explain why the selection is valid, invalid, waiting, or being replaced by another candidate.

## Relationship to HAgent 0.95

Phase 0.95 established the provider-facing `ProviderExecutionRequest`, structured-output transport boundary, runtime identity, request snapshots, cancellation, timeout, and terminal-state safety. Capability-aware execution builds the planning/admission layer in front of that boundary; it does not replace it.

## Relationship to Phase 0.10

The paused Workspace layer must not bypass capability-aware execution. Any future workspace provider/model selector must invoke the same compatibility and admission planner used by ordinary host execution.

## Non-goals

HAgent does not become the billing authority, provider's source of truth for quota, or host scheduler. It maintains normalized planning and admission state and uses provider observations as evidence. Provider adapters remain responsible for provider-specific transport and discovery details.
