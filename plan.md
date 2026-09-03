# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

## Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is to provide reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.96 Capability-Aware Execution — planned next / investigation and hardening target**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

0.8 Data Access + Authorization + Internal Storage foundations are substantially implemented and manually verified across supported storage backends; remaining internal repository parity is intentionally deferred.

0.9 Runtime Agent Instances is complete for the generic runtime contract and manually verified through deterministic Example coverage. HWorld is an external consumer rather than an HAgent dependency.

0.95 Generic External Host Integration is complete and verified on .NET Framework 4.8.1 and .NET 9, including canonical generic execution requests, provider-facing request isolation, structured-output validation/native transport, terminal-state protection, runtime snapshot isolation, external-consumer verification, and composition of long-lived runtime instances with canonical execution requests.

0.10 Workspaces, Routing + Chat has a verified routing and role-policy foundation and is intentionally paused. The remaining workspace product work is deferred while the provider/model capability and execution-planning layer is hardened.

0.96 is the next hardening target. It addresses heterogeneous capabilities, the same logical model exposed by multiple providers, provider/account/project restrictions, model/task-specific constraints, quotas, RPM/RPD/TPM/TPD and future quota dimensions, concurrency capacity, operational availability, long-running inference, capability-aware candidate selection, fallback/degradation, and proactive admission control.

The later Knowledge + Skills + Memory Governance + Learning layer remains planned after the runtime capability model is mature enough to support heterogeneous environments safely.

## Verified implementation

The repository currently contains verified foundations for:

- provider/agent configuration and routing;
- execution lifecycle, timeout, cancellation, retries, diagnostics, and failure reporting;
- memory, persistent sessions, context budgeting, automatic/episodic/task memory;
- capability discovery and response normalization;
- streaming contracts and live streaming;
- tool definitions, registry, schema validation, provider transport, bounded tool loops, persistence, and per-agent assignment;
- WinForms UI Context with Form/UserControl attachment;
- semantic control and bound/native data-source discovery;
- CurrencyManager/current-item/source relationships;
- control-to-source relationship discovery;
- convention-based custom control adaptation;
- bounded application-object discovery;
- provider-neutral structured data projection/query contracts;
- HAgent-owned storage configuration for File, SQL Server, and MySQL backends;
- application-specific File storage layout;
- HAgent-owned SQL Server/MySQL database bootstrap foundations;
- bounded internal inventory, memory, conversation, and execution-audit read tools;
- automatic payload-free execution auditing with configurable bounded retention;
- runtime-instance identity, scope, runtime-only overrides, independent memory ownership, concurrent execution, stale-result protection, host-controlled scheduling, shutdown semantics, and optional runtime-state persistence;
- provider-neutral workspace participants, message metadata, default-recipient routing, and coordinator/specialist role policy;
- generic host execution requests with multiple messages, host correlation identity, bounded host context, provider-facing request isolation, native structured-output transport/fallback, terminal-state protection, runtime snapshot isolation, verified external-consumer coverage on both supported target frameworks, and verified runtime-instance + canonical-request composition.

## Paused Workspace target

Phase 0.10 initially provides one default persisted workspace per host user. The host supplies a stable `UserId`, display identity, and `IsAdmin` identity. Database-backed persistence is partitioned by host application identity and user identity; File storage remains local to the host installation.

Workspace visibility is always explicit: the workspace is hidden until the host opens it. `Create`, `Open/Show`, `Hide`, and `Close` are separate lifecycle operations, and closing the UI never destroys persisted workspace state. The model remains extensible to multiple named workspaces later.

The workspace product target includes a shared Lobby, distinct user-to-agent Private Chats, agent join/leave, coordinator/specialist defaults, permitted provider/agent/model selection and runtime overrides, integrated approval requests/resolution, safe activity/statistics, unread/last-seen state, bounded presentation of tables/charts/graphs and popup/detail results, and modern WinForms presentation through a public host-facing workspace facade.

Provider secrets, connection strings, live provider tasks, live `CancellationToken` state, runtime synchronization primitives, raw HTTP requests, raw provider payloads, and temporary execution objects remain outside persisted workspace state; these exclusions were established by 0.95.

## Phase 0.96 capability-aware execution target

Reusable Agent profiles remain first-class. They describe what the agent is and what capabilities it requires or prefers rather than permanently binding the agent to one provider/model transport target.

```text
Agent Profile
    identity / instructions / tools / memory / knowledge / skills
    required capabilities
    preferred capabilities
    preferred logical model (optional)
    preferred provider (optional)
    fallback / degradation policy

Provider
    service integration

Provider account / project / endpoint
    operational execution environment

Model / logical model
    provider-independent identity when reliably known

Model deployment / execution target
    concrete provider + account/project/endpoint + model deployment

Execution Planner
    selects the best currently compatible target
```

The same logical model may be exposed by Groq, OpenRouter, a direct vendor endpoint, or a local OpenAI-compatible server. These are separate execution targets because capability, provider policy, limits, quota, health, latency, routing behavior, and permissions may differ.

Capability and operational state remain separate. Effective capability state is `Supported`, `Unsupported`, or `Unknown`. Separate records represent request/model constraints, account/project permission, quota/rate capacity, concurrency capacity, current availability/health, and expected latency.

The capability model supports explicit input/output modalities rather than a single vision flag, including text, image, audio, video, embeddings, generation, understanding, and future task types. Native and emulated/degraded behavior remain distinguishable, especially for structured output.

Execution requests express capability requirements independently from agent identity, using required/preferred/optional/forbidden semantics. A manually selected provider/model target passes the same compatibility and policy checks as an automatically selected candidate.

The Execution Planner evaluates candidate targets against requirements, preferences, policy, current availability, limits, and operational capacity before transport. Provider adapters may supply provider-specific discovery and telemetry, but HAgent.Core must consume normalized contracts and must not hard-code Groq, Cloudflare, NVIDIA, OpenRouter, or other provider model matrices.

### Admission and quotas

HAgent provides proactive admission control rather than relying on retries after ordinary limit failures. Rate/quota dimensions are generic and extensible. Minimum dimensions include request count, input tokens, output tokens, total tokens, and concurrent requests, with future dimensions such as audio duration, image count, bytes, spend, or provider-specific units.

Windows may be per-minute, per-day, or provider-specific. Limits may apply at organization, account, project, endpoint, model/deployment, or another provider-defined scope.

Concurrent executions require atomic reservation before provider transport and reconciliation with actual observed usage after execution. Provider headers, retry metadata, 429/throttling responses, and other telemetry are feedback that updates operational state rather than the sole capability-discovery mechanism.

Capacity decisions may be `Wait`, `TryNextCandidate`, `Fail`, or an explicitly policy-permitted degraded path. Admission waiting has a bounded maximum wait.

### Long-running providers

A provider can have abundant or effectively unlimited daily quota while still having low concurrency capacity and multi-minute inference latency. HAgent must not equate quota availability with execution capacity. Long-running requests remain asynchronous, respect cancellation and timeout semantics, and do not block unrelated runtime executions. Slow targets may be valid candidates when the request's latency policy permits them.

Cloudflare Workers AI demonstrates why task/model-specific limits, daily Neuron allocation, and distinct frontier-model limits must be modeled independently. NVIDIA's hosted model catalog demonstrates why free/downloadable endpoints, multimodal capabilities, reasoning/tool-use, rate limiting, and long-running inference must coexist in the generic target model. citeturn147698search0turn147698search2turn513772search0turn720373search0

## Management UI target for 0.96

Provider/model administration should eventually show execution-target identity, capabilities, constraints, quota/rate state, availability, latency observations, and compatibility with the current request. Workspace provider/model selection must consume the same planner rather than bypassing it.

## Boundaries

0.96 is provider-neutral runtime hardening. It does not add provider-specific model matrices to Core, replace host scheduling policy, or become the source of billing truth. It supplies generic discovery, compatibility, admission, and planning primitives that provider adapters and hosts can use.

## Active implementation

The active implementation plan remains `docs/plan/20-active.md`, but Phase 0.10 is paused. Phase 0.96 is the current investigation/planning target before workspace implementation resumes.

## Verification rule

A capability becomes complete only after its implementation exists, its matching `HAgent.Example` verification passes locally, and the project documentation reflects the result.

Do not claim local build/test success unless it was actually performed.

## Documentation ownership

- `README.md` — public introduction and quick start.
- `AGENTS.md` — non-negotiable engineering and repository rules.
- `docs/architecture/` — stable architectural design and boundaries.
- `docs/plan/` — master direction, current state, and active implementation only.
- `docs/roadmap/` — ordered path from completed foundations and future phases.
- `docs/storage.md` — storage-specific details.

The root `plan.md` and `roadmap.md` are generated from their source directories. They are views, not independent sources of truth.

## HAgent Master Plan

## Purpose

HAgent is a general-purpose, provider-neutral cognition and execution library that makes connecting software to LLMs practical. Its goal is to provide reusable infrastructure for any software project that needs LLM-driven behavior without importing host-specific domain models into HAgent.Core.

A host may be a conversational program, business software, service, game, simulation, automation system, developer tool, or another environment.

## End-state goal

A host should be able to add HAgent and choose how much intelligence it wants to expose. HAgent should provide generic infrastructure for model invocation, context, tools, memory, reusable skills, knowledge/Wiki, controlled learning, structured output, multi-agent coordination, and asynchronous execution.

The host remains authoritative over real domain state, lifecycle, scheduling, host persistence, authorization, and side effects.

## Core model

```text
Provider profile
    -> connection/model configuration

Agent profile
    -> reusable behavior + capability policy defaults

Runtime agent instance
    -> one live agent identity created from a profile
    -> runtime-only capability/memory overrides

Execution request
    -> host input/context + correlation + execution requirements

Skills
    -> reusable executable capabilities/procedures

Knowledge
    -> reusable retrievable information
    -> Wiki is a managed persistent knowledge source

Memory
    -> scoped experience/state
    -> working / episodic / semantic / procedural / future families

Learning
    -> execution experience -> typed candidates -> policy -> promotion

Execution
    -> bounded asynchronous model/tool work with lifecycle and correlation
```

The distinction between persistent profiles and runtime instances remains fundamental. One profile can produce many independent runtime instances. Shared resources are referenced; private runtime state is not copied across instances.

## Knowledge, Skills, Memory, and Learning

HAgent must keep the following distinctions explicit:

```text
Skill     = reusable executable capability/procedure
Knowledge = reusable information
Wiki      = managed persistent knowledge source
Memory    = scoped experience/state
Learning  = controlled transformation of experience into candidates
```

Knowledge and Skills are reusable resources with explicit scope and authorization rather than private copies owned by every agent. Memory has explicit ownership/scope and may be private to a runtime instance, shared at logical-agent/user/tenant level, or execution-local.

Learning is not model-weight training. It may use deterministic code, LLM reasoning, or both. Code controls candidate typing, provenance, policy, authorization, retention, and promotion.

### Learning modes

```text
Disabled
SuggestOnly
AutomaticWithPolicy
FullyAutomatic
```

`SuggestOnly` is the recommended governance mode. `AutomaticWithPolicy` permits promotion only under explicit policy. `FullyAutomatic` is an explicit advanced opt-in and never follows merely from enabling learning.

Learning candidates are typed (`MemoryCandidate`, `KnowledgeCandidate`, `SkillCandidate`) and preserve source execution/runtime identity, proposed scope, provenance, and evidence/confidence where available.

### Promotion rules

LLM output must never write authoritative Wiki/knowledge or mutate a published Skill directly merely because it was generated. Normal promotion is:

```text
experience
  -> candidate
  -> validation / policy / authorization
  -> memory, managed knowledge, or new skill version
```

Skill improvements produce new versions; already-running executions use their immutable skill/configuration snapshots.

## Capability policy

Agent profiles establish reusable capability defaults. Runtime instances inherit them and can override them without mutating the profile.

The effective state for each capability/resource is tri-state:

```text
Inherit
Enabled
Disabled
```

The policy must support at least:

- skills and individual skill resources;
- Wiki/knowledge and individual knowledge resources;
- memory and individual memory families/types;
- future resource types by stable type/resource identifiers.

Capability enforcement occurs before retrieval or invocation. Prompt instructions are not authorization.

## Runtime and memory target

A host may keep a runtime instance alive and execute against it repeatedly for an arbitrary lifetime. Private runtime memory must remain independent across runtime instances created from the same profile.

Working memory is execution-local. Long-term memory ownership is explicit and may be runtime-, agent-, user-, tenant-, or another host-approved scope. The physical store may be shared when its contract is concurrency-safe.

Effective profile/runtime capability policy and memory access are captured in execution snapshots so configuration changes cannot alter already-running work.

## Generic execution request

The canonical execution boundary accepts generic host input/context, host correlation identity, execution options, and optional structured-output requirements. Plain strings remain convenience APIs.

## System-prompt model

System prompts are additive layers. Lower layers can add narrower instructions/restrictions but cannot erase higher layers. Prompt layering is behavioral composition, not a security boundary.

## Context target

Context remains bounded, generic, and host-supplied. HAgent may normalize/project/serialize host context but does not assign domain meaning.

## Structured output target

A host may define its own schema. HAgent carries it through provider invocation, validates the returned structure, and exposes validation metadata. Valid JSON alone does not prove schema compliance.

## Tool target

Tool definitions describe what may be requested; trusted runtime handlers define what executes. Handler delegates are never serialized. Tool execution preserves execution/runtime/host correlation for authorization and telemetry.

## Learning and management UI target

`HAgent.WinForms` must provide:

```text
Learning Review
    pending candidates
    inspect provenance/evidence/source
    approve / reject

Wiki / Knowledge Manager
    new / edit / delete
    search/filter
    relationships
    which agents use/access it

Skill Manager
    new / edit / delete
    version/status
    relationships
    which agents use it

Agent Configuration
    selected agent -> effective skills
                     -> knowledge/wiki access
                     -> memory families
                     -> any future resource types
    profile enable/disable
    runtime-instance overrides
```

The agent knowledge overview is based on a generic resource inventory. Known types may have specialized panels, but unknown/new types remain visible without adding a new hard-coded Agent property.

## Generic external-host requirement

HAgent must be capable of serving as the generic LLM cognition/execution layer for different project types. Host state, lifecycle, scheduling, persistence, authorization, and side effects remain host-owned.

## Security target

No model instruction is an authorization boundary. Retrieval, memory recall, skill use, learning, and learning promotion are independently enforceable policy boundaries where meaningful.

## Development principles

- Keep Core provider-neutral and dependency-light.
- Preserve .NET Framework 4.8.1 compatibility where targeted and support .NET 9 where supported.
- Design for low RAM and no GPU assumption.
- Keep runtime work cancellable, bounded, correlated, concurrent, and safe against stale results.
- Keep persistent configuration separate from live runtime state.
- Use generic contracts for future extensibility rather than hard-coded host concepts.
- Verify completed capabilities through `HAgent.Example` before marking them complete.
- Keep authoritative documentation synchronized with implementation.

## What success looks like

A developer can start with:

```csharp
await ai.SendAsync("assistant", "Hello");
```

and later grow the integration into:

```text
host
  -> generic execution/context requests
  -> multiple runtime agent instances
  -> private/shared memory
  -> reusable skills
  -> scoped Wiki/knowledge
  -> controlled learning
  -> authorized tools
  -> structured model output
  -> workspace routing
  -> asynchronous background work
```

without replacing HAgent or introducing application-specific types into `HAgent.Core`.

## Active implementation plan

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
