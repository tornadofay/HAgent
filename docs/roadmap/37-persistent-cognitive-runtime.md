# Phase 0.97 — Persistent Cognitive Runtime

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

## Architectural additions and explicit contracts

The following contracts are first-class architectural concepts even when their initial implementations are deliberately small:

### DecisionContext

`DecisionContext` is the bounded, immutable-at-decision-time cognitive input shared by reactive policies, planners, and LLM-backed policies. It should be assembled from current working state, attended events, active goals/intentions, current plan state, relevant retrieved resources, available capabilities, constraints, and execution requirements.

```text
DecisionContext
├── current situation
├── attended events / observations
├── active goals
├── active intention
├── current plan
├── relevant Memory resources
├── relevant Knowledge resources
├── applicable Skills
├── available capabilities
├── constraints / permissions
└── execution requirements
```

The context builder must not automatically include all stored memory, Wiki/Knowledge, Skills, or conversation history. Retrieval and context budgets determine the working set.

### DecisionPolicy

The cognitive runtime should expose a provider-neutral decision-policy boundary rather than embedding decision logic directly in the runtime loop.

Conceptually:

```text
IDecisionPolicy
├── ReactivePolicy
├── UtilityPolicy
├── LlmPolicy
├── HybridPolicy
└── future policy implementations
```

A policy consumes `DecisionContext` and returns a provider-neutral decision result or a bounded request for deliberation. Policies remain host-neutral; domain-specific authority stays with the host.

### Planner

A `Plan` is state/data. A `Planner` creates or revises plans. Keep these concepts separate so future planning strategies can be added without changing plan storage or execution semantics.

Conceptually:

```text
IPlanner
├── DeterministicPlanner
├── UtilityPlanner
├── LlmPlanner
├── HybridPlanner
└── future planner implementations
```

Initial implementations may be minimal. The architectural contract should exist before advanced planning is implemented.

### Cognitive planning versus execution planning

These are deliberately separate:

```text
Cognitive Planner
    Question: "What should the agent do?"
    Input: DecisionContext + goals + knowledge + skills
    Output: Decision / Plan

Execution Planner
    Question: "Where/how should required inference execute?"
    Input: AgentExecutionRequest + capability requirements
    Output: ExecutionTargetAssessment + selected target
```

The cognitive planner must never directly inspect provider-specific rate-limit or transport state. It expresses inference requirements; Phase 0.96 decides the concrete execution target.

### Retrieval and relevance

Resource retrieval must be a reusable subsystem rather than an LLM-only behavior.

Conceptually:

```text
DecisionContext / RetrievalQuery
        |
        v
Resource eligibility filtering
        |
        v
Candidate retrieval
        |
        v
Relevance ranking
        |
        v
Bounded top-K resources
        |
        v
DecisionContext
```

Relevance should remain composable. Initial ranking may use resource type, scope, tags, lexical similarity, goal/task metadata, recency, importance, and other deterministic signals. Optional semantic/vector retrieval or model-assisted ranking can be added later without making it mandatory for Core.

Attention and relevance are different operations: **attention determines what matters now; retrieval/relevance determines what information is useful about it.**

### Validation boundary

Decisions and plans must remain subject to explicit validation before consequential execution. A provider/model response, even when structurally valid, is not authorization. HAgent permissions, tool capability, budgets, and host authorization remain enforcement boundaries.

## Exit criterion

A host can create a long-lived cognitive runtime for a runtime agent, feed it events/observations, maintain persistent goals and plans, selectively retrieve memory/knowledge/skills, handle routine situations without an LLM, activate deliberation only when justified, execute through the existing provider-neutral HAgent execution engine, recover safely after restart, and observe the full cognitive lifecycle — without HAgent gaining ownership of host-domain state or side effects.
