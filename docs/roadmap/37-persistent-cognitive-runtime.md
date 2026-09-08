# Phase 0.97 — Persistent Cognitive Runtime

## Status

**Planned after Phase 0.96 and before resuming higher-level autonomous-agent features.**

## Goal

Add a provider-neutral, long-lived cognitive runtime above individual HAgent executions while preserving the existing execution engine as the reusable foundation.

A runtime agent instance identifies and owns the lifecycle of a live agent. The Persistent Cognitive Runtime adds the higher-level machinery that lets that agent continuously exist as an autonomous process: it receives environmental events, maintains cognitive state, manages attention, owns goals and intentions, maintains plans, chooses between reactive and deliberative behavior, and decides when and how probabilistic reasoning is warranted.

The runtime is generic. It must work for business applications, automation, research systems, simulations, games, and other hosts without embedding domain-specific world models, schedulers, persistence engines, UI frameworks, or side-effect authority.

This phase does **not** assume that any single cognitive architecture is the final or correct architecture for LLM-based agents. HAgent must provide a stable cognitive substrate that can host multiple cognitive strategies and evolve as research improves.

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
    ├── Cognitive Kernel
    │   ├── belief state
    │   ├── working state
    │   ├── attention / global workspace
    │   ├── goals / intentions
    │   ├── plans / operators
    │   ├── cognitive actions
    │   └── revision / learning state
    │
    ├── Cognitive Strategy Layer
    │   ├── Adaptive Hybrid Cognition (initial)
    │   └── future / experimental strategies
    │
    ├── event intake / activation
    ├── reactive cognition
    ├── deliberative cognition
    ├── reasoning-requirement assessment
    ├── memory / knowledge / skill integration
    └── execution requests
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
  -> Cognitive Kernel
  -> Cognitive Strategy
  -> Attention / Belief / Goal / Plan evaluation
  -> Reactive action OR reasoning requirement
  -> Execution Planner
  -> Execution Engine
  -> Outcome / Observation
  -> Cognitive revision / Memory / Learning
  -> Host-owned side effect or environment action
```

## Core principles

1. [ ] Preserve request-oriented `ExecuteAsync` as a first-class public API. It remains the simplest integration path for callers that do not need autonomous cognition.
2. [ ] Build persistent cognition as an optional higher-level runtime rather than changing the semantics of ordinary execution.
3. [ ] Separate environment/simulation time, cognitive scheduling time, and LLM execution latency. None may be conflated.
4. [ ] Make event-driven activation the default. The runtime must not require an LLM call on every host tick or every incoming event.
5. [ ] Allow deterministic/reactive cognition to resolve routine situations without model execution.
6. [ ] Allow salience, novelty, uncertainty, goal relevance, urgency, risk, policy, and available evidence to influence whether and how deliberation is activated.
7. [ ] Treat the LLM as a replaceable reasoning component inside the cognitive runtime, not as the cognitive runtime itself.
8. [ ] Keep the cognitive kernel provider-neutral and avoid coupling cognitive state to any one model, provider, or reasoning style.
9. [ ] Preserve the host boundary: HAgent may decide what it wants to do, but the host remains authoritative over domain state, authorization, scheduling policy, and side effects.
10. [ ] Keep runtime state provider-neutral and serializable where persistence is explicitly enabled.
11. [ ] Ensure cognitive state changes are versioned and concurrency-safe so stale asynchronous executions cannot overwrite newer intent, belief, goal, or plan state.
12. [ ] Make activation, cognition, planning, execution, memory retrieval, learning, tool usage, and outcomes observable without requiring prompt inspection.
13. [ ] Do not treat any researched cognitive architecture as canonical. HAgent should adapt proven principles and leave room for new strategies and research-derived implementations.

## Cognitive kernel and strategy separation

The persistent runtime must distinguish a stable **cognitive kernel** from pluggable **cognitive strategies**.

The cognitive kernel owns durable semantics and safety-critical state such as identity, belief state, working state, goals, intentions, plans, revisions, persistence, provenance, resource boundaries, and execution correlation. A cognitive strategy decides how that state should be interpreted and progressed.

```text
Stable Cognitive Kernel
        │
        ├── Beliefs
        ├── Goals
        ├── Intentions
        ├── Plans / Operators
        ├── Attention / Global Workspace
        ├── Memory / Knowledge / Skills
        ├── Cognitive Actions
        └── Revision / Provenance
                │
                ▼
        Cognitive Strategy
                │
        ┌───────┴─────────────────────┐
        │                             │
  deterministic / hybrid        LLM-backed reasoning
        │                             │
        └──────────────┬──────────────┘
                       ▼
                cognitive decision
```

Requirements:

14. [ ] Define a provider-neutral `ICognitiveStrategy` boundary independent from provider/model execution.
15. [ ] Treat cognitive strategy as replaceable configuration/implementation rather than a hard-coded property of the runtime kernel.
16. [ ] Define strategy lifecycle, state compatibility, versioning, diagnostics, and safe replacement semantics.
17. [ ] Permit multiple cognitive strategies to coexist in the codebase without duplicating the runtime identity, persistence, memory, authorization, or execution engine.
18. [ ] Allow a future research-derived strategy to be implemented under its own explicit name without redesigning the cognitive kernel.
19. [ ] Make strategy selection observable and reproducible for evaluation.
20. [ ] Prevent a cognitive strategy from bypassing shared HAgent security, authorization, budget, persistence, or stale-result boundaries.

### Initial strategy: Adaptive Hybrid Cognition (AHC)

The first strategy should be explicitly named **Adaptive Hybrid Cognition (AHC)**.

AHC is not declared to be the final cognitive architecture. It is the first HAgent strategy and an experimental baseline built around the following principle:

> Use deterministic cognition whenever sufficient; escalate to probabilistic reasoning when necessary; select reasoning capability according to uncertainty, novelty, consequence, available evidence, goal relevance, constraints, and required capability; then incorporate validated outcomes back into persistent cognitive state.

Conceptually:

```text
Situation
   ↓
Existing deterministic cognition sufficient?
   │
   ├── yes → act / continue plan
   │
   └── no
        ↓
   progressive assessment
        ↓
   lightweight reasoning if sufficient
        ↓
   deeper deliberation if required
        ↓
   execution / observation
        ↓
   validated learning and cognitive revision
```

Requirements:

21. [ ] Implement AHC as the initial concrete cognitive strategy.
22. [ ] Make AHC capable of deciding that no LLM/model execution is required.
23. [ ] Make AHC capable of escalating from deterministic processing to lightweight or deeper probabilistic reasoning when evidence is insufficient.
24. [ ] Base escalation on multiple signals rather than a single opaque "complexity score".
25. [ ] Preserve a provider-neutral reasoning requirement between cognition and execution planning.
26. [ ] Make AHC's assessment and escalation reasons observable.
27. [ ] Keep AHC replaceable so later strategies can be evaluated against it rather than modifying the kernel to accommodate every new research idea.

## Cognitive runtime model

A persistent cognitive runtime should conceptually maintain:

```text
Identity
  Agent profile + runtime instance

Belief state
  Current beliefs about the environment, entities, agent state, and relevant assumptions

Working state
  Current situation, active focus, temporary assumptions, cognitive scratch state

Global workspace / attention
  Bounded set of information currently made available to cognitive processing

Goals
  Desired persistent outcomes with priority/deadline/status

Intentions
  Goals currently committed to pursue

Plan
  Ordered or partially ordered intended steps and operators

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

## Belief state and belief revision

Persistent cognition requires a first-class distinction between observations and the agent's current beliefs about what those observations mean. A belief is a cognitive state assertion, not a statement of ground truth. Observations remain attributable to their sources and the host remains authoritative over domain truth.

```text
Observation / Event
        ↓
Interpretation
        ↓
Belief candidate
        ↓
Belief validation / confidence / provenance
        ↓
Current Belief State
        ↓
Goal / attention / plan consequences
```

Requirements:

28. [ ] Define provider-neutral belief identity, content/reference, provenance, confidence/quality metadata, timestamps/validity, scope, and revision information.
29. [ ] Distinguish host observations/events from inferred beliefs.
30. [ ] Support belief addition, reinforcement, weakening, contradiction, expiry/staleness, and removal/retraction.
31. [ ] Record why a belief changed: new observation, execution outcome, contradiction, learned evidence, policy, or deliberate revision.
32. [ ] Propagate belief invalidation to dependent goals, intentions, plan assumptions, and working state where configured.
33. [ ] Ensure stale or low-confidence beliefs cannot silently override newer higher-authority evidence.
34. [ ] Preserve belief provenance through downstream decisions and execution metadata.
35. [ ] Keep host-authoritative state outside the belief store when the host provides authoritative domain truth.

## Working memory, global workspace, and cognitive actions

HAgent should distinguish durable memory from bounded cognitive working state. A research-derived **GlobalWorkspaceFrame** may represent the small set of currently attended information exposed to the selected cognitive strategy.

```text
Long-term resources
   │
   ├── Memory
   ├── Knowledge
   └── Skills
          │
          ▼
   retrieval / relevance
          │
          ▼
   GlobalWorkspaceFrame
          │
          ▼
   DecisionContext / cognitive strategy
```

Requirements:

36. [ ] Define a bounded `GlobalWorkspaceFrame` or equivalent provider-neutral attended-state contract.
37. [ ] Make attention/working-state selection explicit rather than treating the LLM prompt as the only working memory mechanism.
38. [ ] Keep the workspace bounded by item count, size, budget, or policy.
39. [ ] Allow cognitive strategies to replace or extend workspace-selection policies.
40. [ ] Define provider-neutral internal cognitive actions distinct from external host/application tools.
41. [ ] At minimum allow conceptual internal actions such as recall/retrieve, create/revise goal, adopt/revise intention, create/revise plan, mark belief stale, request deliberation, wait, sleep, wake, and emit observation.
42. [ ] Ensure internal cognitive actions cannot bypass shared authorization, budget, provenance, or revision semantics.
43. [ ] Do not expose a false claim of machine consciousness; `GlobalWorkspaceFrame` is an engineering abstraction for bounded attended state.

## Event and activation model

The runtime must accept host-owned events and observations without assuming what the host calls them.

```text
Host event
   |
   v
Event normalization
   |
   v
Belief / working-state update
   |
   v
Attention / salience evaluation
   |
   +---- ignore / retain
   +---- update working state
   +---- run reactive cognition
   +---- activate deliberation
             |
             v
      reasoning requirement
             |
             v
       execution request
```

Requirements:

44. [ ] Define provider-neutral cognitive event/observation contracts with stable identity, source, timestamp, correlation/causation metadata, optional importance, and bounded payload/context.
45. [ ] Support event deduplication and bounded queues.
46. [ ] Support event expiry and retention policy.
47. [ ] Support explicit wake-up, scheduled wake-up, and event-triggered wake-up.
48. [ ] Support sleeping/idle behavior so an agent can remain persistent without continuously consuming model resources.
49. [ ] Support backpressure and bounded activation work under event storms.
50. [ ] Preserve event provenance into cognitive decisions and execution metadata.

## Attention and salience

Attention is a policy layer, not an implicit LLM prompt convention.

Requirements:

51. [ ] Define a generic attention/salience evaluation contract.
52. [ ] Support at least urgency, novelty, goal relevance, uncertainty, relationship relevance, risk/consequence, and policy-defined importance as inputs where supplied by the host/runtime.
53. [ ] Produce a bounded attended set or an explicit `no-deliberation-needed` outcome.
54. [ ] Allow deterministic attention rules before model execution.
55. [ ] Allow host/runtime policy to cap attention processing cost and queue growth.
56. [ ] Make the reason an event was promoted to deliberation observable.

## Reactive versus deliberative cognition

The runtime must support at least two behavioral paths:

```text
Event
  |
  +--> Reactive cognition
  |       |
  |       +--> action / state transition
  |
  +--> Deliberative activation
          |
          +--> context + memory + goals + attention
          |
          +--> reasoning requirement
          |
          +--> selected model execution when justified
          |
          +--> decision / plan update
```

Requirements:

57. [ ] Define a host-neutral reactive cognition boundary for deterministic behavior.
58. [ ] Define a deliberation activation policy that can decide when an LLM/model execution is justified.
59. [ ] Support explicit triggers such as goal failure, novelty, ambiguity, blocked progress, high-priority event, social interaction, contradiction, elevated risk, or host-requested deliberation.
60. [ ] Allow deliberation to be skipped when an existing plan can safely continue.
61. [ ] Allow re-deliberation when execution results contradict beliefs, violate plan assumptions, or invalidate the current plan.
62. [ ] Avoid encoding repeated motor prompts as the canonical persistent-agent architecture.

## Reasoning requirement and adaptive model selection

Cognition must not directly choose a provider-specific model. Instead it produces a provider-neutral **reasoning requirement** describing what kind of inference is needed. The existing Phase 0.96 execution-planning system then maps that requirement to an executable target.

The runtime should use progressive assessment rather than pretending it can predict exact problem complexity:

```text
Need inference?
   ↓
Can deterministic cognition resolve it?
   │
   ├── yes → continue / act
   │
   └── no
        ↓
   assess signals:
     uncertainty
     novelty
     evidence availability
     goal relevance
     consequence / risk
     planning horizon
     conflicting beliefs
     tool / context requirements
     budget / latency policy
        ↓
   define ReasoningRequirement
        ↓
   Phase 0.96 Execution Planner
        ↓
   appropriate provider/model/target
```

Requirements:

63. [ ] Define a provider-neutral `ReasoningRequirement` contract separate from `AiExecutionTarget`.
64. [ ] Support required/preferred reasoning capabilities without requiring a single scalar complexity classification.
65. [ ] Allow reasoning requirements to express qualities such as depth, context capacity, tool-use needs, structured-output needs, latency tolerance, cost policy, and other capability constraints supported by HAgent.
66. [ ] Support staged escalation from deterministic cognition to lightweight reasoning to deeper deliberation when configured.
67. [ ] Allow multiple assessments to be combined before selecting the final reasoning requirement.
68. [ ] Reuse Phase 0.96 capability-aware execution planning for concrete provider/model selection, quota, health, capacity, cost, and routing.
69. [ ] Keep provider-specific model knowledge out of the cognitive kernel and cognitive strategies.
70. [ ] Record why a stronger or weaker reasoning path was selected and whether escalation succeeded.
71. [ ] Support cases where the correct choice is explicitly **no model execution**.

## Goals, intentions, plans, operators, and impasses

The runtime should incorporate strong ideas from established cognitive architectures without reproducing any one architecture literally.

### Goals and intentions

Requirements:

72. [ ] Define generic goal identity, status, priority, constraints, provenance, and lifecycle.
73. [ ] Support multiple concurrent goals with explicit priority and conflict policy.
74. [ ] Define intention state separately from goal state.
75. [ ] Allow new goals to originate from host input, environmental triggers, existing goal decomposition, learned candidates, policy, or model proposals subject to validation/admission.
76. [ ] Allow goals to be revised, suspended, completed, failed, abandoned, or superseded with explicit reasons.

### Plans and operators

Requirements:

77. [ ] Define a plan model that can represent ordered steps, dependencies, checkpoints, failure conditions, completion criteria, and assumptions.
78. [ ] Define a provider-neutral operator/action concept with preconditions, intended effects, provenance, and execution requirements where appropriate.
79. [ ] Allow deterministic progress against known plan/operator steps without requiring a new LLM call for every step.
80. [ ] Allow a plan to be partially executed without losing its identity or revision history.
81. [ ] Allow plan revision, replacement, suspension, resumption, and abandonment.
82. [ ] Record why a plan changed: new evidence, failed precondition, failed action, higher-priority goal, external event, policy change, belief revision, or deliberate reconsideration.
83. [ ] Support bounded planning depth, step count, context size, and deliberation budget.
84. [ ] Keep plan/operator execution separate from domain side-effect authority; the host decides whether a requested action is valid and executable.

### Impasse and bounded deliberation

An **Impasse** represents a bounded state where the current deterministic cognition cannot safely or confidently continue, such as a missing operator, conflicting goals, failed precondition, contradictory beliefs, insufficient evidence, or policy/resource blockage.

Requirements:

85. [ ] Define an explicit provider-neutral `Impasse` concept with reason, affected state, provenance, severity, and resolution status.
86. [ ] Distinguish routine progress from genuine impasse rather than invoking an LLM for every uncertainty.
87. [ ] Allow an impasse to trigger bounded deliberation, alternative strategy selection, additional retrieval, waiting, or escalation to the host.
88. [ ] Support nested/bounded deliberative substates where a strategy requires temporary focused reasoning.
89. [ ] Ensure impasse resolution cannot silently invalidate newer cognitive revisions.
90. [ ] Observe impasse frequency, causes, resolution path, and recurrence.

## Cognitive revision and compare-and-apply

LLM proposals, learned candidates, plan revisions, and belief changes must be treated as **proposals against a versioned cognitive state**, not as direct mutation commands.

```text
Current cognitive revision N
          │
          ▼
      reasoning
          │
          ▼
  proposed cognitive changes
          │
          ▼
   compare against revision N
          │
      ┌───┴────┐
      │        │
 compatible  stale/conflicting
      │        │
      ▼        ▼
 validate    reject/re-deliberate
      │
      ▼
 Commit revision N+1
```

Requirements:

91. [ ] Define a provider-neutral cognitive revision identifier/version.
92. [ ] Represent model-produced decisions and learned changes as proposals against a specific cognitive revision.
93. [ ] Validate proposals against current beliefs, goals, plan assumptions, permissions, budgets, and applicable policy before applying them.
94. [ ] Apply compatible changes atomically where practical.
95. [ ] Reject or rebase/re-deliberate stale proposals rather than allowing last-writer-wins mutation.
96. [ ] Preserve change provenance and causation for every committed cognitive revision.
97. [ ] Support rollback or compensating revision for learned/experimental cognitive artifacts where safe.

## Memory, knowledge, skills, learning, and experience evolution

Persistent cognition must consume HAgent's existing resource layers rather than duplicate them. Learning should allow an agent to become more efficient over time without silently modifying the cognitive kernel.

```text
Experience
   ↓
Memory record
   ↓
reflection / analysis / validation
   ↓
Candidate:
   Knowledge | Skill | Policy | Goal heuristic | Cognitive improvement
   ↓
Evaluation / governance
   ↓
Versioned adoption
   ↓
Future cognition becomes more efficient / deterministic
```

Requirements:

98. [ ] Define how working cognitive state differs from persistent Memory records, Knowledge, Skills, policies, and conversation context.
99. [ ] Represent execution outcomes and environmental outcomes as attributable experience where appropriate.
100. [ ] Retrieve episodic/semantic/procedural resources selectively based on current attention, goal, plan, and reasoning requirements.
101. [ ] Avoid rebuilding full memory, wiki, skill, or conversation stores into every LLM request.
102. [ ] Allow runtime policy to choose which memory/knowledge/skill resources are eligible for an execution.
103. [ ] Support bounded retrieval and context budgets.
104. [ ] Allow successful repeated experience to produce versioned skill/policy candidates that can reduce future model dependence when validated.
105. [ ] Do not assume that every successful model trace can or should become deterministic code; support partial proceduralization and continued model use for unresolved portions.
106. [ ] Require validation, provenance, versioning, rollback, and explicit governance before learned candidates become trusted deterministic behavior.
107. [ ] Never allow learning to silently rewrite the cognitive kernel itself.
108. [ ] Distinguish at least Experience, Memory, Skill, Policy, and Cognitive-Strategy evolution as separate concepts.
109. [ ] Integrate Phase 0.11 governance contracts when available without making governance logic a cognitive-runtime-specific exception.
110. [ ] Preserve private versus shared resource boundaries and authorization rules.

## Execution integration

Persistent cognition must use the existing generic execution boundary rather than inventing a second provider path.

Requirements:

111. [ ] Create deliberative work through the canonical `AgentExecutionRequest` boundary.
112. [ ] Reuse Phase 0.96 capability-aware target planning for model/provider selection.
113. [ ] Preserve host correlation, execution identity, runtime-instance identity, cancellation, timeout, and stale-result protection.
114. [ ] Associate an execution with the cognitive activation, attended event set, reasoning requirement, goal, intention, and plan revision that caused it.
115. [ ] Prevent an obsolete execution from mutating current beliefs, goals, intentions, plans, or working state after superseding state exists.
116. [ ] Support execution budgets across time, model calls, tokens, tools, and host-defined units where available.
117. [ ] Support independent concurrency limits for cognitive runtimes while allowing unrelated runtimes to continue.
118. [ ] Support long-running deliberation without blocking host event ingestion.

## Context efficiency

The persistent runtime exists partly to eliminate unnecessary repeated prompting and context transfer.

Requirements:

119. [ ] Maintain bounded persistent working context rather than reconstructing the entire cognitive state for every execution.
120. [ ] Prefer state deltas, event summaries, belief changes, plan changes, and relevant memory retrieval over repeatedly sending unchanged state.
121. [ ] Cache stable execution environment information where safe, including resolved profile/resource configuration, with explicit invalidation/version rules.
122. [ ] Avoid reloading unchanged agent/provider configuration from persistence for every cognitive activation when a valid runtime snapshot already exists.
123. [ ] Rebuild or refresh cached execution configuration when persistent configuration, capability data, permissions, or runtime overrides change.
124. [ ] Expose diagnostics for context size, retrieved resources, estimated token cost, activation frequency, deliberation frequency, escalation frequency, latency, and cache refreshes.

## Persistence and recovery

125. [ ] Define an optional persistence contract for cognitive state independent from HAgent provider transport state.
126. [ ] Persist identity, beliefs, goals, intentions, plan state, activation state, bounded working state, cognitive revision, and recovery metadata when enabled.
127. [ ] Do not persist live tasks, cancellation tokens, synchronization primitives, raw provider sessions, secrets, or transient HTTP state as cognitive state.
128. [ ] Support restart recovery with explicit reconciliation of in-flight executions.
129. [ ] On recovery, invalidate obsolete execution authority and rebuild the runtime from the latest durable cognitive revision.
130. [ ] Support optional checkpoints to bound recovery cost.

## Collaboration and social extensibility

The runtime should be usable by applications that have multiple agents without hard-coding social semantics.

131. [ ] Allow an environment to provide participant/reference information to cognition through provider-neutral contracts.
132. [ ] Allow cognitive plans to request communication/delegation through generic tools or collaboration APIs.
133. [ ] Preserve sender, recipient, correlation, causation, and ordering metadata where collaboration systems provide them.
134. [ ] Keep domain-specific social rules outside the generic cognitive runtime.
135. [ ] Allow HAgent Workspaces to use persistent cognitive runtimes without making workspace participation mandatory.

## Observability and cognitive evaluation

Observability must evaluate not only whether the agent succeeded, but how cognition was achieved and whether the architecture improves over time.

136. [ ] Add lifecycle events for cognitive activation, belief changes, attention selection, goal changes, intention changes, plan creation/update/completion/failure, impasses, sleep/wake, deliberation start/end, learning proposals, and strategy changes.
137. [ ] Expose a structured cognitive trace separate from raw provider prompt/response logging.
138. [ ] Track activation-to-execution relationships and end-to-end latency.
139. [ ] Track reactive versus deliberative decisions and the percentage of events handled without model execution.
140. [ ] Track reasoning escalation paths and whether escalation improved decision quality.
141. [ ] Expose budget consumption and reasons for skipped/delayed/failed deliberation.
142. [ ] Make stale-result rejection, cognitive revision conflicts, plan supersession, and belief invalidation observable.
143. [ ] Track repeated situations that become progressively more deterministic through validated skills/policies.
144. [ ] Provide evaluation hooks for comparing multiple cognitive strategies on the same event/goal scenarios.
145. [ ] Measure at least task success, recovery success, unnecessary model-call rate, model-call escalation rate, token/cost efficiency, latency, stale-rejection rate, and cognitive-state consistency where measurable.
146. [ ] Preserve enough provenance to reproduce why a strategy and reasoning requirement were selected without requiring storage of sensitive prompts/responses.

## Safety and policy

147. [ ] Ensure cognitive autonomy never bypasses HAgent permission or host authorization boundaries.
148. [ ] Require explicit policy for autonomous tool invocation and consequential actions.
149. [ ] Support host-controlled maximum deliberation frequency and budget.
150. [ ] Support suspension, retirement, and shutdown of a cognitive runtime independently from persistent state deletion.
151. [ ] Ensure sleeping/retired runtimes do not continue to originate new executions.
152. [ ] Treat model-produced beliefs, goals, intentions, plans, skills, and policies as untrusted proposals until validated against applicable policy.
153. [ ] Require explicit governance before a learned candidate can move from experimental or proposed status into trusted deterministic behavior.
154. [ ] Provide safe fallback when a cognitive strategy fails, is unavailable, or produces incompatible state transitions.

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

The cognitive runtime sees both as events, beliefs, goals, plans, memory, skills, tools, and execution requests. The environment supplies the domain semantics.

## Explicit non-goals

- No HWorld types, physics, simulation clocks, actors, factions, or world rules.
- No CRM/ERP/helpdesk domain model.
- No renderer/UI dependency.
- No mandatory LLM usage for every event or tick.
- No requirement that cognition and execution share a thread.
- No replacement of the existing `AgentExecution` lifecycle.
- No provider-specific cognitive implementation.
- No claim that AHC or any other single strategy is the final architecture of intelligence.
- No direct reproduction of BDI, SOAR, ACT-R, CoALA, Global Workspace, ReAct, or other research architectures as a monolith.
- No automatic promotion of every model trace or successful experience into trusted deterministic behavior.
- No hidden self-modification of the cognitive kernel.

## Example verification

Add deterministic Example coverage for:

1. [ ] Request-oriented execution remains unchanged and does not require a cognitive runtime.
2. [ ] A persistent cognitive runtime can receive events while multiple executions run asynchronously.
3. [ ] Routine events are handled reactively without an LLM execution.
4. [ ] Novel/uncertain events trigger progressive reasoning assessment.
5. [ ] The system can explicitly decide that no model execution is needed.
6. [ ] A lightweight reasoning path can escalate to a stronger reasoning path when evidence or capability requirements are insufficient.
7. [ ] The execution planner selects a provider/model from a provider-neutral reasoning requirement.
8. [ ] Belief contradiction triggers explicit belief revision and, when required, plan/intention reconsideration.
9. [ ] Existing plans continue through deterministic operators without repeated model calls.
10. [ ] A missing precondition or blocked action produces an explicit impasse.
11. [ ] Impasse resolution is bounded and cannot overwrite newer cognitive revisions.
12. [ ] Plan failure triggers bounded re-deliberation.
13. [ ] Higher-priority goals can supersede lower-priority intentions.
14. [ ] A successful repeated experience can produce a versioned skill/policy candidate.
15. [ ] Unvalidated learned candidates cannot become trusted deterministic behavior.
16. [ ] Persistent state survives runtime restart where persistence is enabled.
17. [ ] Late/stale executions cannot overwrite newer cognitive revisions.
18. [ ] Execution configuration is reused safely and refreshed when invalidated.
19. [ ] Cognitive trace reports why an event was ignored, reacted to, escalated, or deliberated.
20. [ ] Two independent cognitive runtimes can operate concurrently without state or memory leakage.
21. [ ] Multiple cognitive strategies can be evaluated against the same scenario through the same kernel and execution boundary.
22. [ ] A future experimental strategy can be added without changing runtime identity, persistence, authorization, or execution semantics.
23. [ ] The same cognitive runtime model works with a business-style event host and a simulation-style event host using only public generic APIs.

## Architectural additions and explicit contracts

The following contracts are first-class architectural concepts even when their initial implementations are deliberately small:

### DecisionContext

`DecisionContext` is the bounded, immutable-at-decision-time cognitive input shared by reactive policies, planners, and LLM-backed policies. It should be assembled from current working state, current belief state, attended events, active goals/intentions, current plan state, relevant retrieved resources, available capabilities, constraints, and reasoning requirements.

```text
DecisionContext
├── current situation
├── current belief state / relevant beliefs
├── attended events / observations
├── active goals
├── active intention
├── current plan
├── relevant Memory resources
├── relevant Knowledge resources
├── applicable Skills
├── available capabilities
├── constraints / permissions
└── reasoning requirements
```

The context builder must not automatically include all stored memory, Wiki/Knowledge, Skills, or conversation history. Retrieval, workspace bounds, and context budgets determine the working set.

### DecisionPolicy

The cognitive runtime should expose a provider-neutral decision-policy boundary rather than embedding decision logic directly in the runtime loop.

Conceptually:

```text
IDecisionPolicy
├── ReactivePolicy
├── UtilityPolicy
├── LlmPolicy
├── HybridPolicy
└── future strategy-specific policies
```

A policy consumes `DecisionContext` and returns a provider-neutral decision result, an internal cognitive action, or a bounded request for deliberation. Policies remain host-neutral; domain-specific authority stays with the host.

### Cognitive strategy

A cognitive strategy is the higher-level policy/architecture that determines how a runtime reasons over its cognitive state. It is intentionally separate from provider/model selection.

```text
ICognitiveStrategy
├── AdaptiveHybridCognition
├── future research-derived strategies
└── experimental strategies
```

A strategy may choose among reactive cognition, deterministic planning, internal cognitive actions, retrieval, reflection, and one or more deliberative executions. It must express provider/model needs through `ReasoningRequirement` and let the execution planner select the concrete target.

### ReasoningRequirement

`ReasoningRequirement` is the bridge between cognition and execution planning.

```text
ReasoningRequirement
├── required capabilities
├── preferred capabilities
├── reasoning depth / quality needs
├── context requirements
├── tool-use / structured-output requirements
├── latency tolerance
├── cost/budget constraints
└── other provider-neutral constraints
```

It must not embed provider-specific model names or transport details. Phase 0.96 remains responsible for target selection and execution admission.

### Plan and operator

A `Plan` is state/data. A `Planner` creates or revises plans. An `Operator` or cognitive action represents a bounded transition that may have preconditions and intended effects. Keep these concepts separate so future planning strategies can be added without changing plan storage or execution semantics.

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

### Impasse

`Impasse` represents inability of the current cognitive path to safely continue. Examples include missing operators, conflicting goals, failed preconditions, contradictory beliefs, inadequate evidence, or policy/resource blockage.

The runtime should be able to resolve an impasse by applying another deterministic strategy, retrieving more information, waiting, requesting LLM deliberation, switching strategy, or escalating to the host, subject to bounds and policy.

### Cognitive revision

A cognitive revision is the versioned state boundary for beliefs, goals, intentions, plans, working state, and adopted learned artifacts.

Model outputs and learning proposals are never authoritative state mutations. They are proposals against a specific revision and must pass validation before being committed as the next revision.

### Cognitive planning versus execution planning

These are deliberately separate:

```text
Cognitive Planner / Strategy
    Question: "What should the agent do, and what reasoning is required?"
    Input: DecisionContext + beliefs + goals + knowledge + skills + strategy rules
    Output: Decision / Plan / ReasoningRequirement

Execution Planner
    Question: "Where/how should required inference execute?"
    Input: AgentExecutionRequest + ReasoningRequirement / capability requirements
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
GlobalWorkspaceFrame / DecisionContext
```

Relevance should remain composable. Initial ranking may use resource type, scope, tags, lexical similarity, goal/task metadata, recency, importance, belief relevance, and other deterministic signals. Optional semantic/vector retrieval or model-assisted ranking can be added later without making it mandatory for Core.

Attention and relevance are different operations: **attention determines what matters now; retrieval/relevance determines what information is useful about it.**

### Learning and proceduralization

Learning may produce new Knowledge, Skills, Policies, heuristics, or strategy proposals, but adoption must remain governed and versioned.

The intended progression is:

```text
new / unfamiliar situation
        ↓
probabilistic reasoning when required
        ↓
experience + outcome
        ↓
candidate skill / policy / knowledge
        ↓
validation + governance
        ↓
trusted reusable behavior
        ↓
future situation may require less or no model use
```

This is an optimization and adaptation mechanism, not a guarantee that every problem converges to deterministic code. Unknown portions must remain able to return to deliberation.

### Validation boundary

Decisions, beliefs, plans, skills, policies, and learned candidates must remain subject to explicit validation before consequential execution or trust promotion. A provider/model response, even when structurally valid, is not authorization. HAgent permissions, tool capability, budgets, cognitive revision checks, and host authorization remain enforcement boundaries.

## Exit criterion

A host can create a long-lived cognitive runtime for a runtime agent, feed it events/observations, maintain persistent beliefs, goals, intentions, and plans, selectively retrieve memory/knowledge/skills, handle routine situations without an LLM, progressively escalate reasoning only when justified, select an appropriate model through a provider-neutral reasoning requirement, encounter and resolve explicit impasses, learn versioned skills/policies from validated experience without silently mutating the cognitive kernel, recover safely after restart, compare multiple cognitive strategies through the same runtime substrate, and observe the full cognitive lifecycle — without HAgent gaining ownership of host-domain state or side effects.
