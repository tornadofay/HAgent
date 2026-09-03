# HAgent Roadmap

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/roadmap`.

## HAgent Roadmap

The roadmap is the ordered implementation path toward the HAgent master plan. Stable architectural definitions belong under `docs/architecture/`, and current work belongs under `docs/plan/`.

## Current position

- 0.1–0.2 — foundations complete
- 0.3–0.4 — memory/context and provider-capability foundations complete
- 0.5 — tool foundation complete; hardening remains
- 0.6 — safety/permission foundation complete; broader authorization remains
- 0.7 — WinForms UI Context + Data Discovery complete and locally verified
- 0.8 — Data Access + Authorization + Internal Storage foundations substantially implemented; Skills/Wiki management and broader knowledge governance were deferred
- 0.9 — Runtime Agent Instances complete and locally verified
- 0.95 — Generic External Host Integration **complete and verified on .NET Framework 4.8.1 and .NET 9**
- 0.96 — Capability-Aware Execution **planned next — provider/runtime hardening**
- 0.97 — Persistent Cognitive Runtime **planned after 0.96 and before resuming higher-level autonomous-agent features**
- 0.10 — Workspaces, Routing + Chat — paused until the generic runtime/capability/cognitive foundations are sufficient
- 0.11 — Knowledge, Skills, Memory Governance + Learning — planned platform feature layer
- 1.0 — Collaboration + Workflows
- Later — provider ecosystem, extensibility, developer platform, release hardening

Phase 0.11 converts existing memory/skill/wiki foundations into a coherent scoped resource model and adds controlled learning, review, capability inheritance, runtime overrides, and management UI. It must consume the generic runtime contracts rather than create project-specific exceptions.

Phase 0.95 is a completed cross-cutting runtime/API hardening phase. It established the generic execution boundary for arbitrary hosts: host input/context, host correlation, structured output contracts and validation, provider-facing request isolation, execution terminality, tool identity propagation, runtime snapshot isolation, provider-native structured-output transport, and external-consumer verification. It does not introduce any host-specific domain dependency.

Phase 0.96 makes execution-target selection and admission capability-aware. It must complete before the persistent cognitive layer depends on heterogeneous-provider routing, quota/capacity management, and long-running execution semantics.

Phase 0.97 adds the missing higher-level runtime above individual executions: a long-lived cognitive runtime that owns persistent cognitive state, receives environment events, manages attention, goals, intentions and plans, decides when reactive handling is sufficient, and activates deliberative executions only when needed. It is generic and must not contain HWorld- or business-application-specific domain logic.

The roadmap distinguishes feature phases from generic runtime hardening. Higher-level features may continue later, but they must consume the generic contracts rather than create project-specific exceptions.

External consumers use HAgent through public provider-neutral APIs. HAgent does not contain consumer-specific dependencies or domain logic.

## Foundations — 0.1 through 0.7

This file records the completed foundation path and the remaining hardening work that grows directly out of those phases. It is the roadmap's implementation history; the master plan does not repeat these checklists.

## 0.1 Foundation — complete

Implemented:

- Multi-target .NET Framework 4.8.1 and .NET 9 where supported.
- Provider and agent configuration with multi-provider relationships.
- OpenAI-compatible provider adapter.
- File, SQL Server, and MySQL persistence foundations.
- Protected local secrets.
- Provider/agent/tool management UI.
- Model discovery and connection testing.
- Dependency-aware deletion behavior.
- `HAgent.Example` integration host and modular examples.
- Global agent selection and output handling.

## 0.2 Runtime — complete

Implemented:

- Execution lifecycle and stable execution IDs.
- Provider routing and ordered candidates.
- Retries, timeout, and cancellation.
- Diagnostics and structured failure categories.
- Actionable provider/model/account error reporting.
- System-prompt resolution.
- Execution snapshots so active work is isolated from later configuration changes.
- Low-RAM/no-GPU design constraints.

## 0.3 Memory + Context — foundation complete

Implemented:

- Persistent JSONL memory and bounded search.
- Explicit remember/recall/forget.
- Memory scopes.
- Typed Fact/Preference/Task/Event records.
- Persistent conversations and sessions.
- Context budgets and tokenizer-free estimation.
- Conservative automatic memory.
- Lightweight relevance ranking.
- Episodic memory with provenance.

Deferred maturation:

- Memory upsert/update semantics.
- Retention/expiration policies.
- Context compaction/summarization.
- Larger-store indexing improvements.
- SQL Server/MySQL memory stores.
- Conversation listing/search/metadata management.
- Optional vector-memory adapters and remote embeddings.

## 0.4 Provider Capabilities + Response Normalization — foundation complete

Implemented:

- Tri-state capability reporting with evidence/confidence.
- Capability caching.
- Normalized text, reasoning, raw text, structured output, tool calls, usage, and provider metadata.
- Separate reasoning handling and `<think>` diagnostics.
- Provider error classification/advice.
- Streaming delta contract.
- OpenAI-compatible SSE streaming.
- Streaming cancellation.
- Live streaming verification.

## 0.5 Tools + Agent Loop — foundation complete

Implemented:

- Six initial tool categories: BuiltIn, Application, Declarative, UI, SqlServer, MySql.
- Tool definition/handler separation.
- Tool registry and application-registered handlers.
- JSON Schema validation.
- Provider tool-definition transport.
- Bounded multi-turn tool loops.
- Persisted tool definitions.
- Per-agent tool assignment.
- Live Groq tool-loop verification.

Hardening remains:

- Per-session temporary tools.
- Built-in tool handlers.
- Declarative execution engine.
- Tool aliases/versioning.
- Tool timeout/cancellation/progress.
- Tool audit/history and budgets.
- Stronger loop detection and capability negotiation.

## 0.6 Safety + Permissions — foundation complete

Implemented:

- General permission configuration UI.
- Persisted current WinForms permission policy.
- Safe defaults for automatic discovery/read/write/invoke behavior.

Remaining platform safety work:

- Read/write/invoke/export authorization across tool categories.
- Host authorization callbacks.
- Human approval lifecycle.
- Input/output/tool guardrails.
- Execution/tool/memory budgets.
- Tracing and observability.
- Sensitive-data redaction.

## 0.7 WinForms UI Context + Data Discovery — complete

Implemented and locally verified:

- Form and arbitrary control-tree/UserControl attachment with stable root identity.
- Read-only inspection and control reads.
- Semantic control discovery.
- Bound/native data-source discovery for DataTable, DataView, BindingSource, IList, arrays, and compatible collections.
- CurrencyManager/current-item/position/count metadata.
- Control-to-source relationships based on actual bindings/source identity.
- Convention-based control adapters, including external `IHyperControl`-style controls using members such as `DbFieldName`, `GetValue()`, and `SetValue(object)`.
- Live application-object attachment and bounded structural inspection.
- `maxDepth` and `maxCollectionItems` resource limits.
- Provider-neutral structured data projection and query contracts with explicit fields, scalar filters, sorting, and bounded paging.
- Example verification for UI Context, UserControl, native IList, data relationships, custom control adaptation, application-object context, and query semantics.

## Foundation exit

These phases establish the base on which the remaining roadmap is built. The next work is not another UI discovery feature; it is safe real data access followed by the runtime-agent model required for multi-agent hosts and HWorld.

## Phase 0.8 — Data Access + Authorization + Internal Storage

## Goal

Provide bounded structured data contracts and establish HAgent-owned persistence across File, SQL Server, and MySQL backends without ever using HAgent storage as access to a host application's business database.

## Steps

1. [x] Application-owned structured-query contract and authoritative field schema.
2. [x] Separate data-operation permissions and request-specific host authorization contracts.
3. [x] Query/result limits, cancellation, timeout, and resource budgets.
4. [x] HAgent internal storage backend configuration for File, SQL Server, and MySQL.
5. [x] Application-specific File storage layout.
6. [x] SQL Server HAgent database creation and initial schema bootstrap.
7. [x] MySQL HAgent database creation and initial schema bootstrap.
8. [ ] Wire providers, agents, tools, memory, conversations, skills, wiki/content, learning candidates, and runtime repositories to the selected backend.
9. [x] Versioned schema migrations beyond the initial bootstrap version.
10. [ ] Read-only HAgent internal data tools, audit/correlation metadata, and live Example verification before any internal writes beyond repository persistence.

The read-only foundation now includes bounded provider/agent/tool inventory, memory inspection with scope/owner isolation, explicit-session conversation inspection, and execution-audit inspection. Execution audit persistence is available through File, SQL Server, and MySQL using a secret-safe payload-free record.

## Internal database naming

The default HAgent database name is derived from the host application name using `<application-name>-ai`, for example `nap-ai` or `hworld-ai`. The database name is controlled by HAgent storage naming rules and is not a user-editable field in the Storage UI.

## File backend

File storage is application-specific and rooted beneath the host executable directory in `HAgentData`, with dedicated areas for configuration, providers, agents, tools, skills, memory, conversations, wiki, runtime, cache, logs, and audit data. Future learning/knowledge-governance records remain HAgent-owned and must use additive schema/layout changes rather than host databases.

## Database backends

SQL Server and MySQL storage providers receive server name and username as persisted configuration metadata and a password through the secret/runtime boundary. They connect to the server, create the HAgent-owned database if it does not exist, and initialize only HAgent-owned tables. Schema version metadata supports deterministic migrations.

The internal database schema includes provider, agent, tool, memory, conversation, execution-audit, skill, wiki document/chunk, and schema metadata areas. Phase 0.11 will add the HAgent-owned persistence required for knowledge resources, skill versions/relationships, learning candidates/review state, capability assignments/overrides, and extensible memory-type policy.

## Audit foundation

`AgentExecution` carries an execution-level correlation ID. `AgentExecutionAuditRecord` projects only execution/correlation identity, agent/provider/model metadata, lifecycle timing, state, and classified failure metadata. Prompts, responses, provider secrets, secret IDs, connection strings, raw exceptions, and other payloads are excluded.

`IExecutionAuditStore` provides bounded append/search persistence. File uses an HAgent-owned `audit/executions.jsonl` file; SQL Server and MySQL use the HAgent-owned `HAgentExecutionAudits` table.

## Live Example

The Example storage verification will exercise File, SQL Server, and MySQL initialization where the corresponding backend is configured. It will verify database creation when absent, idempotent initialization when present, schema version reporting, persistence through the HAgent repositories, execution-audit round trips, and strict separation from host application data.

## Boundaries

- No raw SQL from model input.
- No implicit access to the host application's business database.
- HAgent storage providers are internal persistence providers, not host database adapters.
- Database passwords remain in the secret/runtime boundary.
- UI discovery, object provenance, and model instructions do not grant database authorization.
- Learning candidates and knowledge/skill management remain HAgent-owned data and do not grant host-business-data access.

## Exit criterion

A host can select an HAgent-owned storage backend, initialize or upgrade it deterministically, and use HAgent repositories against it without HAgent gaining access to the host application's business database.

## Phase 0.9 — Runtime Agent Instances

## Goal
Make live agents first-class runtime objects separate from reusable agent profiles.

## Steps

1. [x] Introduce a provider-neutral runtime agent instance with its own stable instance ID and profile reference.
2. [x] Define explicit provider-neutral runtime scopes.
3. [x] Allow runtime-specific context and provider/model overrides without mutating stored profiles.
4. [x] Give each runtime instance an independent memory owner.
5. [x] Support multiple runtime instances executing concurrently.
6. [x] Expose asynchronous scheduling, cancellation, timeout, correlation, and stale-result protection foundations.
7. [x] Define explicit active/retired/shutdown lifecycle behavior.
8. [x] Keep dynamically created agents out of persistent configuration by default.
9. [x] Add optional runtime-state persistence for recovery, collaboration, or multi-process deployments.
10. [x] Verify the runtime contract with deterministic Example coverage.
11. [ ] Complete generic external-host execution boundary hardening in Phase 0.95.

## Runtime rule

One configured profile can produce many live instances. Runtime roles are host policy over the same generic runtime model, not separate agent classes.

Runtime-only provider, model, generation, system-prompt, context, and capability overrides are applied to execution snapshots created from the persistent profile. They never mutate the stored profile. Runtime configuration remains distinct from per-execution host input.

Each runtime instance owns private memory through its `MemoryOwnerId`, keeping private agent-scoped memory separate across instances created from the same profile. Shared memory is possible only through an explicit shared scope and authorization policy.

Each instance-bound execution receives a monotonically increasing instance revision. Hosts can use `AgentRuntimeInstance.IsExecutionCurrent(execution)` to reject late results after a newer execution starts or the instance is retired. The generic execution hardening phase additionally ensures late provider completion cannot overwrite a terminal execution outcome.

Retirement and shutdown are separate lifecycle operations. Retirement prevents new executions and invalidates result authority while allowing already-running work to finish or be cancelled by the host. Shutdown is terminal, prevents new executions, invalidates result authority, and requests cancellation of outstanding instance-bound executions.

`IAgentExecutionScheduler` and the default `AgentExecutionScheduler` provide an optional host-controlled admission boundary with a configurable concurrency limit. The scheduler does not own host timing or replace runtime execution semantics.

Capability policy and learning are layered above this runtime foundation. Phase 0.11 resolves profile capability defaults plus runtime tri-state overrides (`Inherit`, `Enabled`, `Disabled`) into each execution snapshot. Learning operates on execution experience and never mutates runtime identity directly.

## External-host relationship

Phase 0.9 establishes the runtime-instance foundation. Phase 0.95 completes the generic execution boundary required for external hosts: arbitrary host input/context, host correlation, structured output contracts, terminal execution semantics, and tool identity propagation. Phase 0.11 consumes these runtime guarantees for scoped knowledge, Skills, Memory, Learning, and management UI.

## Exit criterion

A host can create, run, cancel, and retire multiple independent runtime agents from reusable profiles without identity, private-memory, or execution-state collisions. Later phases may layer reusable Skills, Knowledge/Wiki, Memory governance, and Learning without weakening runtime isolation.

## Phase 0.95 — Generic External Host Integration

## Status

**Complete — verified on .NET Framework 4.8.1 and .NET 9.**

## Goal

Complete the provider-neutral execution boundary required by arbitrary host applications without coupling HAgent.Core to a host domain, UI framework, scheduler, persistence model, or side-effect system.

## Requirements

1. [x] Introduce a canonical `AgentExecutionRequest` carrying multiple messages, host-supplied bounded context, runtime overrides, and execution options.
2. [x] Preserve plain string message execution as a convenience overload over the canonical request boundary.
3. [x] Preserve host-supplied correlation identity separately from `AgentExecution.Id` and `AgentRuntimeInstance.InstanceId`.
4. [x] Propagate execution, runtime-instance, tool-call, tool, and host correlation identity through tool execution metadata.
5. [x] Define a host-owned `StructuredOutputOptions` request contract with HAgent-side validation.
6. [x] Request structured output through capable provider adapters rather than relying on post-generation JSON detection alone.
7. [x] Validate normalized structured output against the requested schema and expose normalized validation/result metadata.
8. [x] Preserve provider capability distinction for structured output (`Supported`, `Unsupported`, `Unknown`).
9. [x] Make execution terminal-state transitions race-safe so late provider completion cannot overwrite cancellation, timeout, retirement, shutdown, or another terminal outcome.
10. [x] Preserve independent runtime-instance identity, overrides, execution state, shutdown lifecycle, and private memory ownership when multiple instances originate from one profile.
11. [x] Snapshot runtime overrides and host context at the execution/instance boundary so mutable caller-owned state cannot create cross-instance coupling.
12. [x] Keep host scheduling external; HAgent provides only focused admission-control primitives.
13. [x] Preserve host ownership of domain state, persistence, authorization, scheduling policy, and side effects.
14. [x] Add deterministic Example verification covering generic execution input, host correlation, structured output, late completion protection, tool identity propagation, concurrent runtime instances, and memory isolation.
15. [x] Define and verify a provider-facing `ProviderExecutionRequest` boundary separate from the host-facing request.
16. [x] Route normal, tool-calling, and streaming provider adapter contracts through `ProviderExecutionRequest`.
17. [x] Use provider-facing structured-output requirements for provider-native constrained generation where supported, with controlled fallback and continued HAgent validation.
18. [x] Add and verify an external-consumer smoke sample representing a host consuming the HAgent production surface on both supported target frameworks.
19. [x] Compose a long-lived `AgentRuntimeInstance` with the canonical `AgentExecutionRequest` through `HAgentClient.ExecuteAsync(instance, request, cancellationToken)`, preserving request input/context/correlation/structured-output semantics while the instance supplies runtime identity, revision, overrides, lifecycle, and private-memory ownership. Verified by `RUNTIME INSTANCE REQUEST` on the Example application.

## API direction

The public integration shape is:

```text
Host
  -> AgentRuntimeInstance (optional long-lived execution identity)
  + AgentExecutionRequest (execution input/context)
  -> HAgentClient.ExecuteAsync(instance, request, ...)
  -> AgentExecution
```

The runtime instance and execution request are orthogonal. The runtime instance answers **who is executing**; the request answers **what is being executed**. The request must not absorb the runtime instance because runtime ownership, lifecycle, revision, overrides, shutdown signaling, and private-memory ownership belong to the instance boundary.

After HAgent resolves the agent/provider/runtime state, the provider adapter receives:

```text
AgentExecutionRequest + runtime-derived execution options
  -> ProviderExecutionRequest
  -> provider adapter
  -> normalized AIResponse
```

The request boundaries remain generic. HAgent does not define host-domain schemas, event types, command types, domain objects, or lifecycle policy.

## Runtime invariants

One reusable profile may produce many long-lived runtime instances. Every runtime instance remains independently addressable and owns its own runtime lifecycle, execution revision, override snapshot, shutdown signaling, and private memory ownership.

When an execution is started from a runtime instance, `request.AgentId` must match `instance.ProfileId`. The caller's request/options objects are not mutated to attach runtime identity; HAgent creates the effective execution request/options internally.

An execution that has reached a terminal outcome cannot later publish a conflicting outcome because a provider completed late. Non-cooperative providers may continue executing after HAgent has completed cancellation/timeout handling, but their late results cannot regain authority over the terminal execution state.

## Structured-output invariant

A structured-output response is valid only when the requested contract is successfully honored and validated. Arbitrary JSON text is not sufficient evidence that the contract was satisfied.

Provider-native constrained generation is opportunistic. When an OpenAI-compatible endpoint supports the native `response_format`/JSON Schema request shape, the adapter uses it. If the endpoint explicitly reports that feature as unsupported or unknown, the adapter may retry without the native field. HAgent validation remains authoritative in either path.

## External consumer verification

`samples/HAgent.ExternalConsumer` is a standalone host sample that references the broad HAgent production surface available to an application: Core, the OpenAI-compatible provider transport, File storage, SQL Server storage, MySQL storage, and WinForms. It owns its own host-side test data/provider and does not introduce HWorld-specific domain logic into HAgent. The sample has been executed successfully on both `.NET Framework 4.8.1` and `.NET 9`.

A real host is not required to reference every HAgent assembly in production; it selects the modules it needs. The sample is intentionally broad so this milestone verifies the public HAgent system surface rather than only `HAgent.Core`.

## HWorld boundary

HWorld is an external consumer. HAgent does not contain an HWorld dependency, adapter, physics, rendering, simulation-time, or action-authority code. HWorld references the HAgent modules it needs and owns its own domain lifecycle, scheduling, state, authorization, and side effects.

## Exit criterion

A host can submit a complete provider-neutral execution request with bounded context, host correlation, and optional structured-output requirements; HAgent can execute that request either directly or through a long-lived runtime instance without losing request semantics or runtime ownership. HAgent resolves the request into a provider-facing request, invokes an adapter, normalizes the response, validates host-owned contracts, preserves execution identity, protects terminal state, and isolates runtime snapshots without coupling to host or provider-specific domain models. A standalone external consumer representing the HAgent production surface demonstrated the public boundary on both supported target frameworks, and runtime-instance execution composes the canonical request through the verified instance/request API.

## Phase 0.96 — Capability-Aware Execution

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

## Phase 0.97 — Persistent Cognitive Runtime

## Status

**Planned after Phase 0.96 and before resuming higher-level autonomous-agent features.**

## Goal

Add a provider-neutral, long-lived cognitive runtime above individual HAgent executions while preserving the existing execution engine as the reusable foundation.

A runtime agent instance identifies and owns the lifecycle of a live agent. The Persistent Cognitive Runtime adds the higher-level machinery that lets that agent continuously exist as an autonomous process: it receives environmental events, maintains cognitive state, manages attention, owns goals and intentions, maintains plans, chooses between reactive and deliberative behavior, and decides when an LLM execution is warranted.

The runtime is generic. It must work for business applications, automation, research systems, simulations, games, and other hosts without embedding domain-specific world models, schedulers, persistence engines, UI frameworks, or side-effect authority.

## Architectural position

HAgent must support both request-oriented execution and persistent cognition. Persistent cognition is a layer above, not a replacement for, `ExecuteAsync`.

```text
HAgent
│
├── Execution Engine
│   ├── provider/model execution
│   ├── structured output
│   ├── tools
│   ├── memory/context integration
│   ├── retries / timeout / cancellation
│   └── capability-aware execution planning
│
├── Runtime Agent Instance
│   ├── identity
│   ├── lifecycle
│   ├── runtime overrides
│   ├── revision / stale-result protection
│   └── private-memory ownership
│
└── Persistent Cognitive Runtime
    ├── event intake
    ├── attention / salience
    ├── working cognitive state
    ├── goals
    ├── intentions
    ├── plans
    ├── activation / wake-sleep policy
    ├── reactive policies
    └── deliberative execution requests
             │
             ▼
       Existing Execution Engine
```

The execution engine remains usable directly:

```text
Host
  -> AgentExecutionRequest
  -> HAgentClient.ExecuteAsync(...)
  -> AgentExecution
```

A persistent cognitive host uses the higher layer:

```text
Environment
  -> Event / Observation
  -> Cognitive Runtime
  -> Attention / Policy
  -> Reactive action OR deliberation
  -> AgentExecutionRequest
  -> Execution Engine
  -> Plan / Decision / Tool result
  -> Host-owned side effect or environment action
```

## Core principles

1. [ ] Preserve request-oriented `ExecuteAsync` as a first-class public API. It remains the simplest integration path for callers that do not need autonomous cognition.
2. [ ] Build persistent cognition as an optional higher-level runtime rather than changing the semantics of ordinary execution.
3. [ ] Separate environment/simulation time, cognitive scheduling time, and LLM execution latency. None may be conflated.
4. [ ] Make event-driven activation the default. The runtime must not require an LLM call on every host tick or every incoming event.
5. [ ] Allow deterministic/reactive policies to resolve routine situations without model execution.
6. [ ] Allow salience, novelty, uncertainty, goal relevance, urgency, and policy to determine whether an event should activate deliberation.
7. [ ] Preserve the host boundary: HAgent may decide what it wants to do, but the host remains authoritative over domain state, authorization, scheduling policy, and side effects.
8. [ ] Keep runtime state provider-neutral and serializable where persistence is explicitly enabled.
9. [ ] Ensure cognitive state changes are versioned and concurrency-safe so stale asynchronous executions cannot overwrite newer intent or plan state.
10. [ ] Make activation, planning, execution, memory retrieval, tool usage, and outcomes observable without requiring prompt inspection.

## Cognitive runtime model

A persistent cognitive runtime should conceptually maintain:

```text
Identity
  Agent profile + runtime instance

Working state
  Current situation, active focus, temporary assumptions

Attention
  What currently matters and why

Goals
  Desired persistent outcomes with priority/deadline/status

Intentions
  Goals currently committed to pursue

Plan
  Ordered or partially ordered intended steps

Memory
  Episodic, semantic, procedural, and other HAgent-managed stores

Knowledge / Skills
  Resources selected according to policy and capability

Relationships / collaboration context
  Optional generic references to external participants/workspaces

Activation state
  Awake, waiting, sleeping, blocked, deliberating, executing, or retired

Execution state
  Current execution correlation, revision, cancellation and result authority
```

The exact storage contracts must remain modular. A host may choose an in-memory runtime, persistent runtime state, or another HAgent storage implementation without changing the cognitive semantics.

## Event and activation model

The runtime must accept host-owned events and observations without assuming what the host calls them.

```text
Host event
   |
   v
Event normalization
   |
   v
Attention / salience evaluation
   |
   +---- ignore / retain
   +---- update working state
   +---- run reactive policy
   +---- activate deliberation
             |
             v
       Execution request
```

Requirements:

11. [ ] Define provider-neutral cognitive event/observation contracts with stable identity, source, timestamp, correlation/causation metadata, optional importance, and bounded payload/context.
12. [ ] Support event deduplication and bounded queues.
13. [ ] Support event expiry and retention policy.
14. [ ] Support explicit wake-up, scheduled wake-up, and event-triggered wake-up.
15. [ ] Support sleeping/idle behavior so an agent can remain persistent without continuously consuming model resources.
16. [ ] Support backpressure and bounded activation work under event storms.
17. [ ] Preserve event provenance into cognitive decisions and execution metadata.

## Attention and salience

Attention is a policy layer, not an implicit LLM prompt convention.

Requirements:

18. [ ] Define a generic attention/salience evaluation contract.
19. [ ] Support at least urgency, novelty, goal relevance, uncertainty, relationship relevance, and policy-defined importance as inputs where supplied by the host/runtime.
20. [ ] Produce a bounded set of attended items or an explicit `no-deliberation-needed` outcome.
21. [ ] Allow deterministic attention rules before model execution.
22. [ ] Allow host/runtime policy to cap attention processing cost and queue growth.
23. [ ] Make the reason an event was promoted to deliberation observable.

## Reactive versus deliberative cognition

The runtime must support at least two behavioral paths:

```text
Event
  |
  +--> Reactive policy
  |       |
  |       +--> action / state transition
  |
  +--> Deliberative activation
          |
          +--> context + memory + goals + attention
          |
          +--> LLM execution
          |
          +--> decision / plan update
```

Requirements:

24. [ ] Define a host-neutral reactive policy boundary for deterministic behavior.
25. [ ] Define a deliberation activation policy that can decide when an LLM execution is justified.
26. [ ] Support explicit triggers such as goal failure, novelty, ambiguity, blocked progress, high-priority event, social interaction, or host-requested deliberation.
27. [ ] Allow deliberation to be skipped when an existing plan can safely continue.
28. [ ] Allow re-deliberation when execution results contradict assumptions or invalidate the current plan.
29. [ ] Avoid encoding repeated motor prompts as the canonical persistent-agent architecture.

## Goals, intentions, and planning

Requirements:

30. [ ] Define generic goal identity, status, priority, constraints, provenance, and lifecycle.
31. [ ] Support multiple concurrent goals with explicit priority and conflict policy.
32. [ ] Define intention state separately from goal state.
33. [ ] Define a plan model that can represent ordered steps, dependencies, checkpoints, failure conditions, and completion criteria.
34. [ ] Allow a plan to be partially executed without requiring a new LLM call for every step.
35. [ ] Allow deterministic progress against known plan steps.
36. [ ] Allow plan revision, replacement, suspension, resumption, and abandonment.
37. [ ] Record why a plan changed: new evidence, failure, higher-priority goal, external event, policy change, or deliberate reconsideration.
38. [ ] Support bounded planning depth, step count, context size, and deliberation budget.
39. [ ] Keep plan execution separate from domain side-effect authority; the host decides whether a requested action is valid and executable.

## Memory, knowledge, and skills integration

Persistent cognition must consume HAgent's existing resource layers rather than duplicate them.

Requirements:

40. [ ] Define how working cognitive state differs from persistent Memory records and from conversation context.
41. [ ] Retrieve episodic/semantic/procedural resources selectively based on current attention, goal, plan, and execution requirements.
42. [ ] Avoid rebuilding full memory, wiki, skill, or conversation stores into every LLM request.
43. [ ] Allow runtime policy to choose which memory/knowledge/skill resources are eligible for an execution.
44. [ ] Support bounded retrieval and context budgets.
45. [ ] Integrate Phase 0.11 governance contracts when available without making governance logic a cognitive-runtime-specific exception.
46. [ ] Preserve private versus shared resource boundaries and authorization rules.

## Execution integration

Persistent cognition must use the existing generic execution boundary rather than inventing a second provider path.

Requirements:

47. [ ] Create deliberative work through the canonical `AgentExecutionRequest` boundary.
48. [ ] Reuse Phase 0.96 capability-aware target planning for model/provider selection.
49. [ ] Preserve host correlation, execution identity, runtime-instance identity, cancellation, timeout, and stale-result protection.
50. [ ] Associate an execution with the cognitive activation, attended event set, goal, intention, and plan revision that caused it.
51. [ ] Prevent an obsolete execution from mutating current goals, intentions, plans, or working state after superseding state exists.
52. [ ] Support execution budgets across time, model calls, tokens, tools, and host-defined units where available.
53. [ ] Support independent concurrency limits for cognitive runtimes while allowing unrelated runtimes to continue.
54. [ ] Support long-running deliberation without blocking host event ingestion.

## Context efficiency

The persistent runtime exists partly to eliminate unnecessary repeated prompting and context transfer.

Requirements:

55. [ ] Maintain bounded persistent working context rather than reconstructing the entire cognitive state for every execution.
56. [ ] Prefer state deltas, event summaries, plan changes, and relevant memory retrieval over repeatedly sending unchanged state.
57. [ ] Cache stable execution environment information where safe, including resolved profile/resource configuration, with explicit invalidation/version rules.
58. [ ] Avoid reloading unchanged agent/provider configuration from persistence for every cognitive activation when a valid runtime snapshot already exists.
59. [ ] Rebuild or refresh cached execution configuration when persistent configuration, capability data, permissions, or runtime overrides change.
60. [ ] Expose diagnostics for context size, retrieved resources, estimated token cost, activation frequency, deliberation frequency, latency, and cache refreshes.

## Persistence and recovery

61. [ ] Define an optional persistence contract for cognitive state independent from HAgent provider transport state.
62. [ ] Persist identity, goals, intentions, plan state, activation state, bounded working state, and recovery metadata when enabled.
63. [ ] Do not persist live tasks, cancellation tokens, synchronization primitives, raw provider sessions, secrets, or transient HTTP state as cognitive state.
64. [ ] Support restart recovery with explicit reconciliation of in-flight executions.
65. [ ] On recovery, invalidate obsolete execution authority and rebuild the runtime from the latest durable cognitive revision.
66. [ ] Support optional checkpoints to bound recovery cost.

## Collaboration and social extensibility

The runtime should be usable by applications that have multiple agents without hard-coding social semantics.

67. [ ] Allow an environment to provide participant/reference information to cognition through provider-neutral contracts.
68. [ ] Allow cognitive plans to request communication/delegation through generic tools or collaboration APIs.
69. [ ] Preserve sender, recipient, correlation, causation, and ordering metadata where collaboration systems provide them.
70. [ ] Keep domain-specific social rules outside the generic cognitive runtime.
71. [ ] Allow HAgent Workspaces to use persistent cognitive runtimes without making workspace participation mandatory.

## Observability

72. [ ] Add lifecycle events for cognitive activation, attention selection, goal changes, plan creation/update/completion/failure, sleep/wake, and deliberation start/end.
73. [ ] Expose a structured cognitive trace separate from raw provider prompt/response logging.
74. [ ] Track activation-to-execution relationships and end-to-end latency.
75. [ ] Track reactive versus deliberative decisions and the percentage of events handled without model execution.
76. [ ] Expose budget consumption and reasons for skipped/delayed/failed deliberation.
77. [ ] Make stale-result rejection and plan supersession observable.

## Safety and policy

78. [ ] Ensure cognitive autonomy never bypasses HAgent permission or host authorization boundaries.
79. [ ] Require explicit policy for autonomous tool invocation and consequential actions.
80. [ ] Support host-controlled maximum deliberation frequency and budget.
81. [ ] Support suspension, retirement, and shutdown of a cognitive runtime independently from persistent state deletion.
82. [ ] Ensure sleeping/retired runtimes do not continue to originate new executions.

## Business and simulation neutrality

This phase must explicitly support both ordinary business agents and simulated/artificial-world agents.

Examples:

```text
Business
  Customer-support agent
    Event: customer reply
    Attention: unresolved issue
    Goal: resolve ticket within SLA
    Plan: inspect -> diagnose -> respond/escalate

Simulation
  World agent
    Event: nearby entity / danger / conversation
    Attention: threat or social opportunity
    Goal: survive / trade / reach destination
    Plan: navigate -> interact -> re-evaluate
```

The cognitive runtime sees both as events, goals, plans, memory, skills, tools, and execution requests. The environment supplies the domain semantics.

## Explicit non-goals

- No HWorld types, physics, simulation clocks, actors, factions, or world rules.
- No CRM/ERP/helpdesk domain model.
- No renderer/UI dependency.
- No mandatory LLM usage for every event or tick.
- No requirement that cognition and execution share a thread.
- No replacement of the existing `AgentExecution` lifecycle.
- No provider-specific cognitive implementation.

## Example verification

Add deterministic Example coverage for:

1. [ ] Request-oriented execution remains unchanged and does not require a cognitive runtime.
2. [ ] A persistent cognitive runtime can receive events while multiple executions run asynchronously.
3. [ ] Routine events are handled reactively without an LLM execution.
4. [ ] Salient/novel events activate deliberation.
5. [ ] Existing plans continue through deterministic steps without repeated model calls.
6. [ ] Plan failure triggers bounded re-deliberation.
7. [ ] Higher-priority goals can supersede lower-priority intentions.
8. [ ] Persistent state survives runtime restart where persistence is enabled.
9. [ ] Late/stale executions cannot overwrite newer cognitive revisions.
10. [ ] Execution configuration is reused safely and refreshed when invalidated.
11. [ ] Cognitive trace reports why an event was ignored, reacted to, or deliberated.
12. [ ] Two independent cognitive runtimes can operate concurrently without state or memory leakage.
13. [ ] The same cognitive runtime model works with a business-style event host and a simulation-style event host using only public generic APIs.

## Exit criterion

A host can create a long-lived cognitive runtime for a runtime agent, feed it events/observations, maintain persistent goals and plans, selectively retrieve memory/knowledge/skills, handle routine situations without an LLM, activate deliberation only when justified, execute through the existing provider-neutral HAgent execution engine, recover safely after restart, and observe the full cognitive lifecycle — without HAgent gaining ownership of host-domain state or side effects.

## Phase 0.10 — Workspaces, Routing + Chat

**Status: PAUSED after the provider-neutral workspace routing and role-policy foundation.**

Phase 0.10 remains intentionally unfinished. The remaining workspace product work is deferred while earlier provider/runtime capability gaps are investigated and corrected.

## Goal
Provide an optional shared conversation where one authenticated host user and multiple runtime agents can visibly work together while every model request is routed deliberately and the user's workspace state survives application restarts.

## Steps

1. [x] Introduce a workspace abstraction independent of WinForms.
2. [x] Register users and runtime-agent participants with explicit lifecycle state.
3. [x] Define one workspace default recipient for unaddressed user messages.
4. [x] Define direct user-to-agent addressing.
5. [x] Define addressed agent-to-agent delegation and responses.
6. [x] Define coordinator/specialist behavior as a role/policy over generic runtime agents. `WORKSPACE ROLES` Example verification complete.
7. [ ] Allow specialists to represent whole domains, tables, subsystems, or other host responsibilities.
8. [x] Preserve sender, recipient, correlation, causation, ordering, and routing metadata.
9. [ ] Execute routed workspace messages through runtime agents and make agent-to-agent work visible in the workspace lobby when enabled.
10. [ ] Add configurable addressing syntax at the host/UI layer without making prompt text the authoritative routing mechanism.
11. [ ] Add loop protection and collaboration budgets.
12. [ ] Add optional persistent workspace state and explicit shared-memory policy.
13. [ ] Add the WinForms workspace surface and global agent selection.
14. [ ] Add stable host user identity input, including `UserId` and `IsAdmin`, with database-safe user/workspace partitioning.
15. [ ] Add create/open/show/hide/close workspace lifecycle APIs where UI close never destroys persisted workspace state.
16. [ ] Persist user-owned lobby chat, private-agent chat, participant membership/roles, approval state, safe statistics, and selected workspace UX state according to storage policy.
17. [ ] Add user-facing workspace configuration with a single `Enable Workspace` setting; workspace remains hidden until explicitly opened.
18. [ ] Add default manager/coordinator agent configuration.
19. [ ] Add default specialist agent configuration and specialist responsibility metadata.
20. [ ] Add workspace/private-chat provider and agent selection/override controls without mutating persistent agent profiles.
21. [ ] Add configurable default approval type/policy and integrate approval requests/resolution into workspace UI and conversation history.
22. [ ] Add professional modern WinForms UI with lobby chat, participant/agent list, private chats, approval presentation, and explicit open/close/show/hide controls.
23. [ ] Add Example controls and tests for create, open/show, hide, close UI, agent join/leave, lobby chat, private chat, approval flow, persistence across restart, and user/workspace restoration.
24. [ ] Verify SQL Server, MySQL, and File workspace persistence and user partitioning.

### Current foundation

The provider-neutral foundation contains `AgentWorkspace`, `WorkspaceParticipant`, `WorkspaceMessage`, `IWorkspaceRouter`, and `WorkspaceRouter`. Participants are either users or runtime agents and have explicit Active/Suspended/Retired state. An active default recipient may be defined for unaddressed user messages. Routing does not invoke providers, mutate agent profiles, or perform host side effects.

`IWorkspaceRolePolicy` and `WorkspaceRolePolicy` allow coordinator/specialist behavior to be expressed as policy over ordinary agent participants. `WorkspaceAgentRoleAssignment` describes `Participant`, `Coordinator`, or `Specialist` role, optional responsibility metadata, user-message eligibility, and allowed delegation target roles. No separate coordinator/specialist agent classes are introduced. The `WORKSPACE ROLES` Example verification confirms the policy works.

The workspace user identity contract must carry a stable `UserId` and an `IsAdmin` flag supplied by the host. The identity is an input to workspace authorization and persistence partitioning; `IsAdmin` does not itself grant tool, memory, data, or host-business permissions.

Phase 0.10 initially provides one default persisted workspace per user. The model should remain extensible to multiple named workspaces later without forcing a second workspace into the first implementation.

## Workspace lifecycle

The workspace UI is hidden until explicitly opened by the host. There is no automatic-show workspace behavior in Phase 0.10.

`Create`/ensure obtains the user's default workspace. `Open`/`Show` makes the UI visible. `Hide` hides it without changing workspace state. `Close` closes the UI without deleting workspace state. Destructive archive/deletion is a separate explicit operation and is not implied by closing the UI.

Application shutdown or computer shutdown must not cause user work to disappear. Reopening the application with the same stable `UserId` restores the persisted workspace state from the selected HAgent storage backend.

## Conversations

The workspace contains a shared Lobby conversation where the user and joined agents can visibly communicate. It also contains distinct Private Chats between the user and selected agents. Private chat content is not automatically exposed to other workspace participants.

Visible messages identify their author and role clearly. System and approval events are first-class workspace-visible events alongside ordinary conversation messages.

## Agent configuration

The host/application administrator can configure default manager/coordinator and specialist agents for the workspace. A specialist has descriptive responsibility metadata that can represent a domain, table, subsystem, process, capability, or other host-owned responsibility without requiring HAgent-specific domain classes.

The workspace UI can let a user switch the active provider/model or selected agent for an allowed conversation or private chat. These are execution/runtime selections and do not silently mutate the stored `AiAgent` profile.

The default approval type/policy is a workspace/host policy default. Approval handling remains subject to HAgent authorization and does not bypass permission checks.

## Routing rules

- Unaddressed user message: send only to the workspace default recipient.
- Explicitly addressed user message: send to that participant.
- Agent delegation: send only to the addressed participant unless an explicit role policy allows the sender's role to delegate to the recipient's role.
- Broadcast: explicit opt-in operation, never the default.
- Agent-to-agent routed work becomes visible in the Lobby when workspace execution policy permits it.

The authoritative routing decision is represented by workspace messages and routing APIs; human-friendly addressing syntax is a UI/host convenience.

## Persistence

Persisted workspace state is partitioned by host application identity and the stable `UserId`. File storage remains local to the host installation; SQL Server and MySQL must prevent users in the same host application from reading or mutating another user's workspace state unless an explicit host/admin policy permits it.

Persisted workspace state includes workspace metadata, participant membership/roles/lifecycle state, lobby and private-chat history, approval requests/resolution state, safe statistics/activity metadata, selected workspace UX state where appropriate, and explicit workspace/shared-memory records.

Provider secrets, connection strings, live provider tasks, live `CancellationToken` state, runtime synchronization primitives, raw HTTP requests, raw provider payloads, and temporary execution objects are not workspace persistence concerns; the generic runtime phase already establishes those boundaries.

Agent private memory remains private unless explicit shared-memory policy grants workspace visibility.

## Workspace UI

The WinForms workspace is a compact, professional collaboration surface rather than a large dashboard. It contains a Lobby, participant/agent selection, access to private chats, approval presentation, and message composition. It must provide clear authorship and state without exposing implementation details.

The host interacts with the workspace through a public HAgent workspace facade rather than direct manipulation of internal WinForms controls. The facade provides lifecycle and communication operations conceptually equivalent to create, open/show, hide, close, agent join/leave, lobby send, private-chat open/send, and workspace state observation.

## Example verification

The Example application will expose explicit controls for creating, showing/opening, hiding, and closing the workspace UI, plus controlled agent join/leave and communication tests. Verification must confirm that closing the UI and restarting the application preserve the user's lobby/private chats, membership, approvals, selected state, and safe statistics for File, SQL Server, and MySQL storage.

## HWorld boundary

HWorld remains an external consumer. It references HAgent normally and uses public runtime/workspace APIs. HAgent does not add an HWorld-specific dependency, adapter, world type, physics, simulation scheduling, or action authority.

## Exit criterion

A host can identify a user, create/open the user's persisted default workspace, show and close the UI without losing work, join runtime agents, conduct visible lobby and private conversations, configure/select permitted manager/specialist agents and provider/model overrides, present and resolve approvals, and restore the same state after application restart while routing remains bounded and traceable.

## Phase 0.11 — Knowledge, Skills, Memory Governance + Learning

## Goal

Turn the existing memory/skill/wiki storage foundations into a coherent, provider-neutral knowledge and learning subsystem with explicit scope, capability inheritance, runtime overrides, and WinForms management surfaces.

## Architecture outcome

```text
Skills    = reusable executable capabilities/procedures
Knowledge = reusable retrievable information
Wiki      = managed persistent knowledge source
Memory    = scoped experience/state
Learning  = execution experience -> typed candidates -> policy -> promotion
```

Resources are shared/reusable by default; ownership and access are explicit through scope and authorization. Runtime instances inherit profile configuration but may override individual capability/resource states without mutating the profile.

## Learning modes

1. [ ] Add provider-neutral `LearningMode`: `Disabled`, `SuggestOnly`, `AutomaticWithPolicy`, `FullyAutomatic`.
2. [ ] Add learning policy contract covering candidate type, scope, confidence/evidence, provenance, contradiction checks, retention, and promotion authorization.
3. [ ] Add typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` contracts.
4. [ ] Preserve execution/runtime/agent provenance on candidates.
5. [ ] Support deterministic code-derived learning signals without requiring an LLM.
6. [ ] Allow optional model-assisted extraction/classification without making the model the authority.
7. [ ] Keep promotion separate from candidate creation and keep published Skills versioned.

## Knowledge and Wiki

8. [ ] Define the provider-neutral knowledge resource/source contract and managed Wiki model.
9. [ ] Define resource scope, lifecycle/status, provenance, versioning, metadata, tags/categories, and relationships.
10. [ ] Define bounded retrieval contracts independent of keyword/vector/index implementation.
11. [ ] Support reusable shared knowledge plus agent/runtime scoped resources where authorized.
12. [ ] Prevent model-generated content from silently becoming authoritative knowledge.

## Skills

13. [ ] Define stable/versioned SkillDefinition and SkillSet/reference contracts.
14. [ ] Keep executable handlers separate from persisted definitions.
15. [ ] Support required knowledge, required tools, input/output contracts, preconditions, procedure steps, constraints, and lifecycle.
16. [ ] Preserve snapshot semantics so in-flight executions are not changed by later skill edits.
17. [ ] Support SkillCandidate -> validation -> new skill version workflow.

## Memory

18. [ ] Normalize memory families including working, episodic, semantic, procedural, and future extensible types.
19. [ ] Make memory scope explicit: execution, runtime, logical agent, user, tenant, or host-approved future scope.
20. [ ] Preserve the existing invariant that independent runtime instances never share private mutable memory ownership.
21. [ ] Keep storage implementation independent of memory ownership and retrieval policy.
22. [ ] Add memory-type enable/disable policy to agent profiles and runtime overrides.

## Capability policy

23. [ ] Add profile capability defaults for Skills, Knowledge/Wiki, Memory, and individual resources/types.
24. [ ] Add tri-state runtime override: `Inherit`, `Enabled`, `Disabled`.
25. [ ] Compute one effective immutable capability snapshot per execution.
26. [ ] Enforce capability policy before retrieval, exposure, or invocation.
27. [ ] Use stable resource/type identifiers so future knowledge types can be surfaced without changing the agent domain model.

## Management UI

28. [ ] Add Learning Review management surface with pending candidate list, inspection, provenance/evidence, source execution/runtime, target scope, approve, and reject.
29. [ ] Add Wiki/Knowledge Manager with New/Edit/Delete, search/filter, relationships, and "used by/accessed by agents" views.
30. [ ] Add Skill Manager with New/Edit/Delete, version/status, relationships, and "used by agents" views.
31. [ ] Extend Agent Configuration so selecting an agent shows effective Skills, Knowledge/Wiki, Memory families, and all generic future resource types.
32. [ ] Add profile-level capability switches for Skills, Wiki/Knowledge, Memory, and memory types.
33. [ ] Add runtime-instance-level override controls using `Inherit`/`Enabled`/`Disabled`.
34. [ ] Keep known types specialized while rendering unknown/future resource types through the generic inventory view.
35. [ ] Follow existing HAgent.WinForms conventions: `HMessage`, shared `Header`, `HButton`, and preserve existing layouts unless this phase requires a targeted change.

## Storage

36. [ ] Add HAgent-owned storage migrations for candidates, knowledge resources, skill relationships/versioning, capability assignments/overrides, and memory-type policy where required.
37. [ ] Keep File, SQL Server, and MySQL behavior aligned through provider-specific migrations.
38. [ ] Keep learning/review metadata secret-safe and bounded.

## Runtime integration

39. [ ] Bind effective knowledge/skill/memory policy into the runtime execution snapshot.
40. [ ] Capture execution outcomes/observations as learning input without mutating runtime identity.
41. [ ] Preserve runtime-instance isolation, execution correlation, cancellation, timeout, stale-result protection, and concurrent execution behavior.
42. [ ] Ensure runtime-only overrides never write back to the persistent profile.

## Verification

43. [ ] Add deterministic Example verification for scope isolation, inherited/overridden capability state, memory types, knowledge retrieval, skill binding, and learning candidates.
44. [ ] Add Example verification for SuggestOnly review and approval/rejection.
45. [ ] Add tests that a candidate cannot bypass authorization or directly mutate a published Wiki/Skill.
46. [ ] Add tests for future/unknown resource types surviving inventory and UI projection.
47. [ ] Add tests that existing executions retain immutable capability/skill snapshots after later edits.

## Exit criterion

A host can define reusable Skills and managed Wiki/knowledge, keep independent scoped runtime memories, enable/disable individual capability families and resources at profile or runtime level, run automatic or review-based learning through `LearningMode`, and administer all of it through HAgent.WinForms without introducing host-specific domain types into Core.

## Phase 1.0 — Collaboration + Workflows

## Goal
Turn basic workspace messaging into reliable multi-agent collaboration and then into bounded task/workflow execution.

## Collaboration steps

1. First-class delegation/handoff operations.
2. Shared/private workspace context policies.
3. Parallel specialist work with bounded collaboration budgets.
4. Human intervention and approval points.
5. Explicit runtime/participant lifecycle states.
6. Cross-agent memory sharing only through explicit policy.
7. Collaboration history, audit, and traceability.

## Workflow steps

1. Task/job model and lifecycle.
2. Planning, execution, and verification stages.
3. Multi-step and branching workflows.
4. Background execution and scheduling.
5. Pause/resume and durable checkpoints.
6. Event-triggered execution.
7. Per-step timeout, cancellation, retry, approval, and budget policies.

## Boundary

These are generic orchestration facilities. HAgent does not become the authority for business rules, simulation state, or host-side side effects.

## Exit criterion

A host can coordinate multiple agents and long-running work with bounded execution, explicit authority, resumable state where required, and observable collaboration.

## Later — Platform, Extensibility + Release

These capabilities follow the core runtime, data, and collaboration milestones. They should not block the primary host-integration path.

## Provider ecosystem

- [ ] Additional provider adapters such as Azure OpenAI, Anthropic, Google/Gemini, Ollama, LM Studio, and custom HTTP providers where justified.
- [ ] Multimodal and embedding adapters.
- [ ] Provider capability/contract harness.

## Extensibility

- [ ] Provider, tool, UI-adapter, and storage extension model.
- [ ] Extension validation and failure isolation.
- [ ] External secret stores and secret rotation.
- [ ] Optional MCP/vector integrations where they fit the lightweight architecture.

## Developer platform

- [ ] Optional DI/interoperability integrations.
- [ ] Simulation/test mode for external consumers such as HWorld.
- [ ] Diagnostics/trace viewer.
- [ ] Complete Example coverage for meaningful public APIs.
- [ ] SDK guidance for provider, tool, UI-context, and host integrations.

## Release hardening

- [ ] Stable public contracts and compatibility policy.
- [ ] Storage migration/versioning.
- [ ] NuGet packaging and release process.
- [ ] Security/provider/tool/memory integration coverage.
- [ ] Documentation and migration guidance.

`.NET 10` remains a future target after the development environment and compatibility policy are ready.
