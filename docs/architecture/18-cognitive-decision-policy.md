# Persistent Cognitive Runtime — Bounded Cognitive Decision Policy

## Purpose

This document defines the production decision-policy rules used by Phase 0.97 when cognition must choose between deterministic processing, probabilistic reasoning, waiting, recovery, or host intervention.

It closes an important architectural gap without claiming that HAgent can compute a universal intelligence, difficulty, relevance, or progress score.

The governing principle is:

> When a cognitive question cannot be answered perfectly in general, HAgent defines a bounded operational contract whose inputs, outputs, limits, fallback behavior, and observable state transitions are explicit.

This is a production mechanism. It is not an experimental research subsystem and it does not require the runtime to know whether a model is "thinking well."

## 1. Bounded routing policy

The runtime must not require a universal deterministic-vs-reasoning boundary.

Instead, every decision is routed through a bounded policy over the current authoritative state, available deterministic operators, evidence, declared constraints, resource eligibility, and consequence limits.

The canonical routing shape is:

```text
Event / Decision Context
        ↓
Deterministic triage
        ↓
Deterministic transition available and safe?
    yes → deterministic action
    no  → reasoning assessment
              ↓
       classify required path
              ├→ NoModelRequired / deterministic recovery
              ├→ Wait / retain / state update
              ├→ bounded light reasoning
              ├→ bounded deeper reasoning
              ├→ host escalation
              └→ Unresolved
```

The policy is allowed to be conservative. It is not required to prove that the selected path is globally optimal.

### 1.1 Routing inputs

A routing decision may use only bounded, provider-neutral information available at the decision boundary, including:

```text
current cognitive revision
active goals / intentions / plan dependencies
applicable deterministic operators
belief validity / confidence / provenance
novelty / uncertainty
consequence or risk classification
required tools / knowledge / context
policy and authorization constraints
latency / model / token / call budgets
resource availability
previous routing and deliberation outcomes
```

The strategy may use deterministic rules, bounded multi-signal scoring, precedence tables, or another explicitly versioned strategy. No universal scalar routing score is required.

### 1.2 Routing decision contract

A routing decision must be represented explicitly enough to explain and bound the selected path:

```text
RoutingDecisionId
BaseRevision
SelectedPath
Trigger
EvidenceReferences
ReasoningRequirement (when applicable)
Budget
Expiration
FallbackPath
Policy/StrategyVersion
```

`SelectedPath` is provider-neutral and may include:

```text
Deterministic
Wait
Retain
StateUpdate
ReactiveAction
LightReasoning
DeepReasoning
HostEscalation
Unresolved
```

Provider/model selection remains the responsibility of Phase 0.96. The cognitive layer must not introduce another provider-selection path.

### 1.3 Routing bounds

Routing itself must be bounded. At minimum, the active policy must enforce applicable limits for:

```text
routing evaluation time
routing retries / re-evaluations
escalation depth
reasoning-call budget
overall cognitive budget
```

If routing cannot establish a permitted next path within its bounds, the runtime takes the configured fail-safe outcome rather than repeatedly re-routing.

## 2. Operational definition of cognitive progress

HAgent does not attempt to determine whether reasoning is intrinsically intelligent or high quality.

For runtime control, **progress means material advancement of the authoritative decision state or its validated evidence/dependency state**.

A deliberation cycle may produce progress in one or more bounded dimensions:

```text
GoalProgress
PlanProgress
DependencyResolution
EvidenceImprovement
UncertaintyReduction
ConflictReduction
FeasibleActionExpansion
ValidStateTransition
```

A strategy may define which dimensions are applicable to a decision. It must not invent unbounded or purely subjective progress criteria at runtime.

### 2.1 Material progress

A progress observation is material only when:

1. it changes a tracked state/dependency/evidence property;
2. the change is attributable to the current deliberation work;
3. the resulting state remains valid under current policy and revision rules.

Repeated generation of equivalent proposals, unchanged assumptions, unchanged dependencies, or new text without state/evidence advancement is not material progress.

### 2.2 No-progress condition

A `NoProgress` condition is reached only within configured bounds. The policy must define applicable thresholds such as:

```text
maximum consecutive no-progress cycles
maximum no-progress wall time
maximum equivalent proposal repetitions
maximum total deliberation budget
```

The threshold is a control mechanism, not a claim that the runtime has discovered a universal definition of stagnation.

On `NoProgress`, the runtime must select one of the allowed bounded outcomes:

```text
retry with changed strategy/constraints
return to deterministic recovery
wait for new evidence
abandon/supersede current path
escalate to host intervention
return Unresolved
```

The runtime must not continue indefinitely solely because a model continues producing different text.

## 3. Goal and intention reconsideration policy

A new event does not by itself justify reconsideration of an active intention.

Reconsideration requires an explicit trigger and policy decision.

Supported trigger classes include:

```text
PlanPreconditionFailed
PlanOutcomeIncompatible
SupportingBeliefInvalidated
HigherPriorityGoalConflict
MaterialConstraintChanged
PolicyChanged
AuthorizationChanged
CapabilityChanged
ResourceChanged
DeadlineThreatened
RepeatedFailureThresholdReached
NoProgressThresholdReached
AuthorizedHostIntervention
```

The runtime must classify the affected authority level:

```text
plan/method assumption invalid
    → reconsider plan first

intention commitment rationale invalid
    → reconsider intention

goal outcome or legitimacy invalid
    → reconsider goal
```

Changing a plan must never implicitly change its goal or intention.

### 3.1 Anti-thrashing controls

Reconsideration must be bounded by applicable controls including:

```text
maximum reconsiderations per time window
cooldown / commitment interval where configured
repeated-proposal detection
no-progress threshold
oscillation detection
escalation after repeated reversal
```

Equivalent goal/intention/plan proposals repeated without a material state change must not be treated as fresh progress merely because their textual form differs.

### 3.2 Deterministic conflict arbitration

When active intentions conflict, the runtime applies explicit constraints and deterministic tie-breaking inputs such as:

```text
policy
authorization
priority
deadline
consequence
feasibility
resource availability
host constraints
```

Equal inputs must produce deterministic tie-breaking. An unresolved conflict becomes a typed `Impasse`; the system does not delegate authority to the model merely because deterministic arbitration failed.

## 4. Bounded escalation lifecycle

A reasoning escalation is a controlled state transition:

```text
Deterministic path insufficient
        ↓
Create RoutingDecision
        ↓
Create provider-neutral ReasoningRequirement
        ↓
0.96 selects/admit execution target
        ↓
Execute within cognitive and execution budgets
        ↓
Validate returned proposal/evidence against base revision
        ↓
Commit, reject, retry, wait, abandon, or escalate
```

A model response is never sufficient by itself to prove that escalation was justified, that a plan succeeded, or that a goal changed.

Late or superseded reasoning results are subject to the same revision, lifecycle, cancellation, and authorization protections as every other asynchronous cognitive result.

## 5. Fail-safe behavior

Every bounded cognitive path must define a terminal fallback before execution begins.

Examples:

```text
reasoning budget exhausted
    → Unresolved / deterministic recovery / host escalation

required resource unavailable
    → Wait / alternate resource / host escalation

no-progress threshold reached
    → stop deliberation and select configured recovery path

newer related revision committed
    → reject or revalidate stale work

policy or authorization changed
    → stop affected path
```

The runtime must never turn uncertainty, exhaustion, or routing ambiguity into fabricated certainty.

## 6. Observability requirements

Decision-policy telemetry must make bounded routing and progress behavior inspectable without exposing sensitive model payloads.

At minimum, telemetry should identify:

```text
routing decision ID
base cognitive revision
selected path
trigger
policy/strategy version
budget allocation and consumption
progress dimensions observed
no-progress counters
reconsideration counters
final terminal outcome
```

The telemetry records the policy decision and its evidence, not a claim that the policy was globally optimal.

## 7. Production invariants

1. HAgent never requires a universal deterministic-vs-reasoning classifier.
2. HAgent uses an explicit bounded routing policy for cognitive escalation.
3. Routing policy is provider-neutral; concrete provider/model selection remains in 0.96.
4. Cognitive progress is an operational state-advancement concept, not a universal intelligence score.
5. No-progress detection is bounded and policy-defined.
6. A model producing more text does not by itself constitute progress.
7. Reconsideration requires an explicit trigger and policy decision.
8. Plan, intention, and goal authority remain separate.
9. Repeated equivalent proposals do not reset anti-thrashing limits.
10. Every bounded cognitive path has an explicit terminal fallback.
11. Uncertainty, budget exhaustion, or routing ambiguity never silently becomes authoritative success.
12. All routing, progress, reconsideration, and escalation decisions remain revision-safe, observable, cancellable, and host-policy constrained.

## Relationship to Phase 0.97

This document is a supporting production mechanism authority for `docs/architecture/16-cognitive-runtime.md` and `docs/architecture/17-cognitive-algorithms.md`.

The 0.97 roadmap remains the delivery authority. This document does not add a new milestone or require implementation outside the ordered 0.97 slices.
