# Phase 0.97 — Persistent Cognitive Runtime

## Status

**Planned after Phase 0.96 and before higher-level autonomous-agent features.**

## Goal

Add a provider-neutral, long-lived cognitive runtime above individual HAgent executions while preserving the existing execution engine as the reusable foundation.

A runtime agent instance identifies and owns the lifecycle of a live agent. The Persistent Cognitive Runtime adds the engineering mechanisms required for that agent to remain active over time: it receives observations/events, maintains versioned cognitive state, selects bounded context, manages goals and intentions, maintains plans, decides when deterministic processing is sufficient, requests probabilistic reasoning when justified, executes through the existing execution engine, and incorporates validated outcomes.

The runtime is generic. It must work for business applications, automation, research systems, simulations, games, and other hosts without embedding domain-specific world models, schedulers, persistence engines, UI frameworks, or side-effect authority.

This phase specifies production mechanisms. It does **not** implement a named cognitive theory. Research foundations remain rationale and evaluation material in `docs/architecture/15-research-foundations.md` and `docs/research/` rather than implementation requirements.

Normative implementation details are defined in `docs/architecture/17-cognitive-algorithms.md`. This roadmap defines delivery slices, dependencies, and acceptance criteria.

## Architectural position

Persistent cognition is an optional layer above request-oriented execution, not a replacement for `ExecuteAsync`.

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
    ├── authoritative cognitive state
    ├── observations / events
    ├── bounded decision workspace
    ├── goals / intentions / methods
    ├── plans / operators
    ├── reactive processing
    ├── deliberation / reasoning requirements
    ├── experience / learning integration
    └── execution requests
             │
             ▼
       Existing Execution Engine
```

The host remains authoritative over domain truth, authorization, scheduling policy, and side effects.

## Production principles

1. [ ] Preserve request-oriented `ExecuteAsync` as a first-class public API.
2. [ ] Make persistent cognition optional; ordinary execution semantics must not change.
3. [ ] Keep cognitive state provider-neutral and independent of any specific model/provider.
4. [ ] Treat observations as inputs and beliefs as interpreted state; never confuse belief with host-authoritative truth.
5. [ ] Use immutable snapshots plus explicit proposals and atomic compare-and-apply for cognitive state mutation.
6. [ ] Detect semantic conflicts, not only revision-number conflicts.
7. [ ] Keep deterministic processing on the fast path and model execution conditional.
8. [ ] Bound event intake, workspace size, deliberation, execution, retries, recursion, and storage growth.
9. [ ] Never let a strategy or model bypass authorization, policy, budget, capability, provenance, or stale-result protection.
10. [ ] Treat learning as candidate production followed by validation, evaluation, and publication; do not auto-promote raw episodes.
11. [ ] Reuse Phase 0.96 execution planning rather than creating a second provider/model routing system.
12. [ ] Preserve provenance, causality, and observability across activation, decision, execution, and learning.
13. [ ] Make restart, cancellation, retirement, and shutdown explicit lifecycle operations.
14. [ ] Keep host-specific domain state and external side effects outside the cognitive kernel.
15. [ ] Implement mechanisms that solve persistent-agent engineering problems; do not implement a cognitive theory merely because the theory has a name.

## Scope boundaries

This phase includes:

- persistent runtime identity and lifecycle integration;
- authoritative cognitive state and revision-safe mutation;
- observations, beliefs, goals, intentions, methods, plans, operators, and bounded decision context;
- event intake and bounded activation;
- deterministic/reactive processing;
- deliberation activation and provider-neutral reasoning requirements;
- impasse handling with bounded isolated substates;
- execution outcome integration and recovery;
- experience capture and conservative procedural-learning candidates;
- integration with Memory, Knowledge, Skills, Policy, and 0.9576 learned-resource reliability contracts;
- persistence/recovery contracts where the host enables persistence;
- concurrency, cancellation, shutdown, observability, and evaluation boundaries.

This phase does not include:

- a claim that HAgent implements BDI, SOAR, ACT-R, Global Workspace Theory, LIDA, ReAct, Reflexion, MemGPT, Voyager, or another complete research architecture;
- consciousness or machine-mind semantics;
- human psychological timing or universal cognitive-complexity measures;
- domain-specific world models or simulation rules;
- a second execution scheduler/router parallel to Phase 0.96;
- automatic conversion of arbitrary trajectories into trusted skills;
- a universal solution to continual-learning stability/plasticity.

## Delivery slices

### Slice 1 — Cognitive State and Revision Core

- [ ] Define the authoritative `CognitiveState` contract.
- [ ] Define immutable state snapshots and explicit state revision numbers.
- [ ] Define `CognitiveProposal` with proposal identity, operation key, base revision, read set, assumptions, proposed changes, evidence references, producer, trigger, expiry, and budget usage.
- [ ] Implement atomic compare-and-apply.
- [ ] Implement semantic conflict detection when two proposals touch overlapping or invalidated assumptions even if their base revisions can otherwise be replayed.
- [ ] Define deterministic conflict outcomes: apply, reject, retry-from-new-snapshot, or escalate.
- [ ] Preserve source/cause/correlation metadata for every accepted mutation.
- [ ] Ensure stale asynchronous executions cannot overwrite newer cognitive state.

Acceptance:

- concurrent proposals never produce torn cognitive state;
- stale proposals are rejected or safely rebased according to explicit rules;
- each accepted state transition is traceable to its evidence and producer;
- no model/provider dependency exists in the state core.

### Slice 2 — Goals, Intentions, and Method Authority

- [ ] Define goal identity, status, priority, constraints, provenance, lifecycle, and completion/failure reasons.
- [ ] Support multiple active goals with deterministic conflict/priority policy.
- [ ] Define intention as a commitment to pursue a goal, separate from the goal definition.
- [ ] Define method/plan authority separately from intention authority.
- [ ] Support intention states such as proposed, active, suspended, reconsidering, completed, failed, abandoned, and superseded.
- [ ] Define explicit reconsideration triggers: invalidated assumptions, repeated failure, changed constraints, higher-priority work, deadline/risk changes, or newly available evidence.
- [ ] Add anti-thrashing controls: minimum commitment interval, change budget, cooldown, or equivalent policy.
- [ ] Record why an intention was created, kept, revised, or abandoned.

Acceptance:

- a persistent intention can survive while its method changes;
- method changes never silently replace the higher-level goal;
- repeated oscillation is bounded and observable.

### Slice 3 — Observations, Beliefs, and Dependency Tracking

- [ ] Define provider-neutral observation/event identity, source, timestamp, correlation/causation, and bounded payload metadata.
- [ ] Distinguish host observations from inferred beliefs.
- [ ] Define belief identity, content/reference, provenance, quality/confidence, validity, scope, and revision.
- [ ] Support belief addition, reinforcement, weakening, contradiction, expiry/staleness, and retraction.
- [ ] Record the reason for each belief revision.
- [ ] Track dependencies from beliefs to assumptions, goals, intentions, plans, and workspace entries where required.
- [ ] Prevent stale/low-authority evidence from silently overriding newer/higher-authority evidence.
- [ ] Define ambiguity and insufficient-evidence outcomes; do not force a false interpretation.

Acceptance:

- observation and belief are audibly distinct in state and telemetry;
- belief invalidation can identify affected downstream state;
- contradictory evidence produces an explicit resolution path.

### Slice 4 — Event Intake and Bounded Decision Workspace

- [ ] Define bounded event intake with deduplication, expiry, backpressure, and retention rules.
- [ ] Support event-triggered, explicit, and scheduled wake-up.
- [ ] Support idle/sleep behavior without continuous model usage.
- [ ] Define a provider-neutral `DecisionWorkspace` contract for bounded attended state.
- [ ] Keep workspace selection separate from prompt construction.
- [ ] Bound workspace by count, size, budget, or policy.
- [ ] Define deterministic relevance/attention inputs such as urgency, novelty, goal relevance, uncertainty, risk, relationship relevance, and policy-defined importance where supplied.
- [ ] Produce either a bounded workspace or an explicit no-deliberation-needed outcome.
- [ ] Keep the name and implementation of the workspace replaceable; the contract is what matters.

Acceptance:

- event storms cannot cause unbounded activation work;
- a persistent agent can sleep while retaining its state;
- workspace contents and selection reasons are observable.

### Slice 5 — Reactive Processing and Deliberation Activation

- [ ] Define a host-neutral reactive processing boundary.
- [ ] Allow deterministic actions/state transitions to resolve routine cases.
- [ ] Define deliberation triggers for novelty, ambiguity, blocked progress, contradiction, elevated risk, goal failure, social interaction, or host request.
- [ ] Allow a valid current plan to continue without unnecessary deliberation.
- [ ] Require re-deliberation when plan assumptions or relevant beliefs become invalid.
- [ ] Keep deliberation activation independent from provider-specific model selection.
- [ ] Record why deterministic processing was sufficient or why deliberation was activated.

Acceptance:

- routine events can complete without an LLM call;
- justified complex events activate bounded deliberation;
- activation reasons are reproducible from structured telemetry.

### Slice 6 — Reasoning Requirement and Phase 0.96 Integration

- [ ] Define provider-neutral `ReasoningRequirement`.
- [ ] Represent required/preferred reasoning capabilities instead of a single universal complexity score.
- [ ] Allow requirements for context capacity, structured output, tool use, depth, latency tolerance, cost policy, and supported capability constraints.
- [ ] Support staged escalation from deterministic processing to lightweight reasoning to deeper deliberation.
- [ ] Reuse Phase 0.96 capability-aware execution planning for provider/model selection, quota, health, capacity, cost, and routing.
- [ ] Keep provider/model names out of the cognitive kernel and strategy contracts.
- [ ] Support an explicit no-model-execution outcome.

Acceptance:

- the cognitive layer emits a provider-neutral requirement;
- Phase 0.96 remains the sole concrete execution-selection layer;
- reasoning escalation is bounded and observable.

### Slice 7 — Plans, Operators, Recovery, and Impasses

- [ ] Define plan identity, status, assumptions, preconditions, expected effects, checkpoints, and provenance.
- [ ] Define operator/action contracts with explicit preconditions/effects where applicable.
- [ ] Support ordered and partially ordered plans where justified by host requirements.
- [ ] Model unknown outcomes explicitly; never convert timeout/provider failure into false success.
- [ ] Define typed impasse states for blocked progress, missing knowledge, contradictory beliefs, unavailable capability, failed action, authorization/policy conflict, and equivalent runtime conditions.
- [ ] Give an impasse a bounded isolated substate with its own base revision/read set/dependency set and resource budget.
- [ ] Prevent nested impasses from creating unbounded recursive cognition.
- [ ] Ensure an impasse substate cannot silently become an alternate authority over the parent runtime.
- [ ] Define admissible resolution outputs: revised plan, newly established fact, capability request, wait/retry, escalation, abandonment, or equivalent typed result.
- [ ] Apply resolution back to the parent only through normal proposal/revision semantics.

Acceptance:

- blocked work has explicit, inspectable state;
- isolated impasse work cannot corrupt the parent state;
- recursive resolution and resource usage are bounded;
- successful resolution does not imply universal knowledge or skill formation.

### Slice 8 — Experience, Procedural Learning, and Skill Candidates

- [ ] Define an experience record distinct from a skill, knowledge item, memory, and policy.
- [ ] Capture relevant state, observation, action, outcome, assumptions, context, and provenance for completed episodes.
- [ ] Generate procedural-learning candidates only from sufficient evidence.
- [ ] Require positive and negative evidence where available.
- [ ] Represent preconditions/applicability context and expected outcomes.
- [ ] Distinguish correlation from demonstrated causal usefulness; do not overstate certainty.
- [ ] Support candidate validation, evaluation, rejection, revision, and promotion through existing learning governance/policy.
- [ ] Keep candidate synthesis conservative under noisy, partial, or contradictory trajectories.
- [ ] Do not automatically promote a single successful trajectory into a trusted skill.

Acceptance:

- a skill candidate has evidence, applicability, provenance, and validation state;
- failed examples remain usable as negative evidence where policy permits;
- raw experiences are never silently promoted to production skills.

### Slice 9 — Learned Resource Reliability and Adaptation

- [ ] Consume Phase 0.9576 learned-resource reliability/adaptation contracts rather than creating a parallel system.
- [ ] Track usefulness, uncertainty, contradiction, staleness, invalidation, and evidence quality for memory/knowledge/skill resources as supported by 0.9576.
- [ ] Propagate relevant invalidation to dependent goals, intentions, plans, and workspace selections.
- [ ] Support replacement, revalidation, conflict handling, and utility-aware archival/forgetting where policy allows.
- [ ] Keep learning publication reversible/versioned where feasible.

Acceptance:

- cognitive decisions can stop relying on degraded resources;
- resource changes retain provenance and version lineage;
- HAgent does not claim to solve neural catastrophic forgetting; stability is managed at the resource/version/policy layer.

### Slice 10 — Concurrency, Lifecycle, Persistence, and Recovery

- [ ] Integrate with existing runtime-instance identity and lifecycle ownership.
- [ ] Support concurrent observations, deliberations, executions, and state proposals without shared mutable-state races.
- [ ] Define cancellation and timeout behavior for cognitive work.
- [ ] Define retirement and shutdown semantics including cancellation of outstanding work and prevention of post-retirement commits.
- [ ] Define persistence boundaries for cognitive state where a host enables persistence.
- [ ] Define restart recovery, in-flight work invalidation, and safe resumption from the latest durable revision.
- [ ] Separate runtime ownership from persistence implementation so HAgent remains host-neutral.

Acceptance:

- retired/shutdown runtimes reject new work according to lifecycle policy;
- in-flight stale results cannot mutate newer state after restart or retirement;
- persistence recovery produces a self-consistent cognitive snapshot.

### Slice 11 — Policy, Authorization, Security, and External Boundaries

- [ ] Route cognitive proposals, internal actions, resource use, learning promotion, and external execution through shared policy/authorization boundaries.
- [ ] Keep host/domain side effects outside the cognitive kernel.
- [ ] Treat event payloads, retrieved content, learned resources, tool outputs, and model outputs as untrusted input unless validated by policy.
- [ ] Define provenance and trust metadata for external evidence.
- [ ] Bound prompt/context/resource amplification caused by untrusted or recursive data.
- [ ] Preserve the distinction between recommendation/decision and authorized execution.

Acceptance:

- no cognitive strategy or model output can bypass common authorization/policy controls;
- untrusted content cannot silently become trusted persistent state;
- external side effects require the host-authorized execution path.

### Slice 12 — Observability, Evaluation, and Strategy Extensibility

- [ ] Emit structured telemetry for event intake, workspace selection, proposal creation, conflict, decision path, reasoning requirement, execution correlation, outcome, impasse, learning candidate, and publication.
- [ ] Provide stable correlation/causation identifiers across asynchronous work.
- [ ] Make decisions inspectable without requiring prompt inspection.
- [ ] Define evaluation hooks for correctness, latency, cost, unnecessary deliberation, conflict rate, recovery rate, and learning reliability.
- [ ] Define a provider-neutral cognitive-strategy boundary.
- [ ] Keep strategy replacement/versioning explicit and compatibility-checked.
- [ ] Treat the first hybrid strategy as an implementation baseline, not architectural truth.

Acceptance:

- runtime behavior can be diagnosed from structured records;
- strategies can be compared without rewriting the kernel;
- evaluation can identify both capability gains and regressions.

## Required cross-cutting invariants

The following are phase-wide invariants and must hold across all slices:

1. [ ] No stale asynchronous result may silently overwrite newer authoritative cognitive state.
2. [ ] No model/provider identity may become part of the provider-neutral cognitive-state contract.
3. [ ] No host-authoritative domain state may be silently replaced by an inferred belief.
4. [ ] No unbounded event, recursion, context, or learning queue may be introduced.
5. [ ] No single episode may become a trusted skill without the configured validation/policy path.
6. [ ] No strategy may bypass shared authorization, capability, budget, provenance, or lifecycle boundaries.
7. [ ] No second provider/model execution router may be introduced in 0.97.
8. [ ] Every externally meaningful cognitive mutation must be attributable to a source, cause, and revision.
9. [ ] Shutdown and retirement must be authoritative over outstanding asynchronous work.
10. [ ] Future research ideas may be implemented as strategies or policies without changing the stable kernel merely to adopt their vocabulary.

## Dependency order

```text
0.96 execution planning
        │
        ├── ReasoningRequirement integration
        │
        └───────────────┐
                        ▼
Cognitive State / Revision Core
        │
        ├── Goals / Intentions
        ├── Observations / Beliefs
        ├── Decision Workspace / Events
        │
        ▼
Reactive + Deliberative Activation
        │
        ▼
Plans / Operators / Impasses
        │
        ├── Execution outcome integration
        │
        ▼
Experience / Skill Candidates
        │
        ▼
0.9576 Learned-resource reliability
        │
        ▼
Persistence / Recovery / Strategy Extensibility / Evaluation
```

Do not implement later slices by creating parallel versions of contracts already established by earlier slices.

## Implementation and verification standard

Each slice must have:

- production contracts and implementation;
- focused unit/contract coverage for normal, conflict, stale, cancellation, failure, and boundary cases;
- an HAgent Example scenario when the feature is externally demonstrable and no existing example already covers it;
- .NET Framework 4.8.1 and .NET 9 compatibility where the owning project targets both;
- deterministic behavior where deterministic behavior is required by the contract;
- explicit documentation of any intentionally unresolved research question.

Do not treat an LLM-generated response as proof that a cognitive contract works. Runtime invariants must be verified by structured tests and observable state transitions.

## Research boundary

Research remains valuable for selecting mechanisms, designing experiments, and comparing strategies. Research does not become implementation scope merely because a paper or architecture supplies a name.

Examples:

- Goal/Intention separation may be useful without implementing full BDI.
- Explicit operators and impasses may be useful without implementing SOAR.
- Modular state and procedural utility may inform implementation without reproducing ACT-R.
- Bounded attended context may be useful without claiming Global Workspace Theory.
- Reason/action/observation interleaving may be useful without making ReAct the system architecture.
- Reflection or self-evaluation techniques may be useful only when they satisfy a concrete HAgent contract.

The stable question for implementation is:

> What production problem are we solving, what invariant must hold, what contract exposes it, and how do we verify it?

## Definition of done for Phase 0.97

Phase 0.97 is complete only when all of the following are true:

- [ ] A persistent runtime agent can maintain versioned cognitive state across multiple events/executions.
- [ ] Goals can persist while intentions and methods are revised safely.
- [ ] Observations and beliefs remain distinct and provenance-aware.
- [ ] Event intake and decision context are bounded.
- [ ] Routine decisions can avoid model execution.
- [ ] Deliberation can request provider-neutral reasoning requirements and reuse Phase 0.96 execution planning.
- [ ] Plans and impasses have bounded, revision-safe execution semantics.
- [ ] Execution outcomes can revise beliefs, goals, intentions, and plans without stale-result corruption.
- [ ] Experience can produce governed skill candidates without automatic unsafe promotion.
- [ ] Learned-resource reliability from 0.9576 can influence cognitive state/resource selection.
- [ ] Cancellation, retirement, shutdown, persistence, and restart behavior are explicit and tested where supported.
- [ ] Shared policy, authorization, provenance, and host side-effect boundaries are enforced.
- [ ] Structured observability supports diagnosis and evaluation without relying on prompt text.
- [ ] At least one concrete cognitive strategy exists behind the stable kernel, but the kernel does not depend on that strategy's theoretical vocabulary.
- [ ] Examples and compatibility coverage are present for newly externally visible behavior.

Phase 0.97 should leave HAgent with a real, inspectable, versioned persistent cognitive runtime—not a collection of research architecture names—and with enough stable boundaries to evaluate future strategies without rebuilding the foundation.