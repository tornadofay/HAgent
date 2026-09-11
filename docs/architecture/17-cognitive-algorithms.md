# Persistent Cognitive Runtime — Production Cognitive Algorithms

## Purpose

This document is the **production implementation specification** for the future Phase 0.97 Persistent Cognitive Runtime.

It defines concrete software mechanisms. It does not require HAgent to implement or reproduce BDI, SOAR, ACT-R, Global Workspace Theory, LIDA, ReAct, Reflexion, MemGPT, Voyager, theories of consciousness, human cognitive timing, or any other research architecture.

Those materials remain research rationale in `docs/research/` and `docs/architecture/15-research-foundations.md`. They are not implementation requirements.

The rule is simple:

> Implement mechanisms that solve real persistent-agent engineering problems. Do not implement a cognitive theory merely because the theory has a name.

HAgent does not claim to solve general cognition, universal skill induction, or neural continual learning. It provides a bounded runtime in which competing decision and learning strategies can be implemented and empirically evaluated.

## Production scope

Phase 0.97 is concerned with:

```text
Persistent cognitive state
Versioned state transitions
Goal / intention / method separation
Observation / belief separation
Event triage and bounded working context
Deterministic decision making
Bounded probabilistic deliberation
Typed impasses
Semantic stale-proposal protection
Plan execution and revision
Experience capture and governed learning candidates
Learned-resource applicability and invalidation
Runtime concurrency and scheduling bounds
Restart/recovery semantics
Policy / authorization / host-boundary enforcement
Structured observability and evaluation
```

## Architectural boundary

```text
Host / Environment
        │
        ▼
 Event / Observation
        │
        ▼
Persistent Cognitive Runtime
        │
        ├── State
        ├── Decision / Planning
        ├── Deliberation
        ├── Learning
        └── Revision / Recovery
        │
        ▼
Reasoning Requirement
        │
        ▼
Phase 0.96 Execution Planner
        │
        ▼
Existing HAgent Execution Engine
```

The cognitive runtime answers **what should happen next**. The execution planner answers **where/how inference should execute**. The host remains authoritative over host state, authorization, scheduling policy, and external side effects.

---

# 1. Authoritative cognitive state

The runtime owns one authoritative cognitive state at revision `R`.

```text
CognitiveState(R)
├── Beliefs
├── WorkingState
├── DecisionWorkspace
├── Goals
├── Intentions
├── Plans
├── Impasses
├── ResourceReferences
├── ActivationState
└── RuntimeRevisionMetadata
```

Readers receive immutable snapshots. A successful state transition creates `R+1`.

`DecisionWorkspace` is an engineering term for the bounded information selected for the current decision. It is not a consciousness model.

### Invariants

1. Background operations do not mutate authoritative state directly.
2. State-changing asynchronous work produces a typed proposal or explicit kernel transition.
3. Every proposal identifies the revision against which it was produced.
4. A stale proposal cannot overwrite newer state.
5. Host-authoritative facts are never replaced by inferred cognitive state.
6. State changes retain provenance and causation.
7. State, queues, and deliberation work are bounded by configured resource limits.

---

# 2. Proposal, revision, and semantic concurrency

A state-changing proposal contains at least:

```text
ProposalId
OperationKey
BaseRevision
ReadSet
AssumptionSet
ProposedChanges
EvidenceReferences
Producer
Trigger/Reason
CreatedAt
Expiration
BudgetUsage
```

`ReadSet` identifies the state/resource versions that materially influenced the proposal.

`AssumptionSet` identifies conditions that must still hold for safe application.

### Commit procedure

```text
Read state R
    ↓
Build proposal against R
    ↓
Validate lifecycle / policy / authorization
    ↓
Validate referenced versions and assumptions
    ↓
Classify conflicts
    ↓
Apply compatible changes atomically
    ↓
Commit R+1
    ↓
Publish revision/outcome event
```

Revision number alone is insufficient. A changed revision may be unrelated to the proposal. Therefore conflict detection must consider declared dependencies and assumptions.

Minimum conflict outcomes:

```text
NoConflict
RelatedChange
AssumptionBroken
TargetChanged
PolicyChanged
ResourceChanged
BeliefConflict
GoalConflict
PlanConflict
UnknownConflict
```

Unrelated changes may permit application when assumptions remain valid. Related or unknown conflicts require revalidation, rejection, or re-deliberation. There is no generic last-writer-wins merge for cognitive state.

Every state-changing operation must be idempotent by stable proposal/operation identity.

---

# 3. Goals, intentions, and methods

HAgent keeps these as distinct production concepts:

```text
Goal
    desired outcome

Intention
    active commitment to pursue a goal

Plan / Method
    current approach used by the intention
```

### Goal lifecycle

```text
Proposed → Active → Succeeded
                 ├→ Failed
                 ├→ Suspended
                 ├→ Abandoned
                 └→ Superseded
```

### Intention lifecycle

```text
Candidate → Adopted → Suspended → Completed
                    ├→ Revised
                    ├→ Abandoned
                    └→ Superseded
```

### Authority rule

Changing a plan does not implicitly change the intention or goal.

```text
method assumption invalid
    → reconsider plan first

commitment rationale invalid
    → reconsider intention

desired outcome invalid
    → reconsider goal
```

The affected level must be explicit in the proposal.

### Reconsideration triggers

Typical triggers are:

- failed plan precondition;
- incompatible action outcome;
- invalidated supporting belief;
- higher-priority goal conflict;
- material environment change;
- policy/authorization/capability/resource change;
- repeated failure beyond configured threshold;
- explicit host/operator request.

A new event by itself is not sufficient reason to reconsider an intention.

### Conflict arbitration

When intentions conflict:

```text
Collect conflicting intentions
        ↓
Apply explicit constraints
        ↓
Compare priority / deadline / consequence / policy
        ↓
Check feasibility and host constraints
        ↓
retain both | serialize | suspend | revise | supersede | escalate
```

Equal inputs must have deterministic tie-breaking. An unresolved conflict becomes an `Impasse`.

### Anti-thrashing

The implementation must support bounded reconsideration using some combination of:

- maximum reconsiderations per time window;
- repeated-proposal detection;
- no-progress threshold;
- cooldown/commitment interval where appropriate;
- escalation after oscillation.

---

# 4. Observations and beliefs

An observation is evidence. A belief is HAgent's current bounded interpretation of evidence.

```text
Observation / Event
      ↓
Interpretation
      ↓
BeliefCandidate
      ↓
Validation + provenance + confidence
      ↓
Belief transition proposal
      ↓
Commit
```

Interpretation may be deterministic, model-assisted, or hybrid.

Material ambiguity must be representable as `Ambiguous` or `InsufficientEvidence` rather than fabricated certainty.

A belief should preserve, where applicable:

```text
identity
content/reference
source/provenance
confidence/quality
observed/created time
validity/expiry
scope/owner
revision
```

Belief updates must record why they changed.

### Conflict precedence

Conflict handling considers:

```text
host authority
source authority
directness
freshness
confidence/quality
scope
```

Stronger authoritative evidence must not be silently replaced by weaker inferred state.

### Dependencies

Goals, intentions, plan steps, and learned resources may depend on specific belief versions.

When a belief changes:

```text
belief changed
    ↓
find declared dependents
    ↓
revalidate affected dependents only
```

---

# 5. Event triage and bounded working context

Every incoming event passes through bounded triage before expensive reasoning.

```text
Event
  ↓
deduplication / expiry / scope
  ↓
relevance / salience assessment
  ↓
Ignore | Retain | StateUpdate | ReactiveAction | Deliberate | HostEscalation
```

The runtime maintains a bounded `DecisionWorkspace` containing only information needed for the current decision.

Selection may use:

- urgency;
- goal/intention relevance;
- novelty;
- uncertainty;
- consequence/risk;
- freshness;
- information value;
- policy importance;
- processing cost;
- resource eligibility.

The runtime must not require a universal scalar salience or intelligence score. Strategies may use deterministic rules or bounded multi-signal scoring.

The workspace is not a second memory store.

---

# 6. Deterministic decision path

The first decision path is deterministic whenever sufficient.

```text
DecisionContext
      ↓
Find applicable operators / current plan step
      ↓
Check preconditions
      ↓
Check policy / authorization / resources
      ↓
Check consequence bounds
      ↓
One safe deterministic transition?
   yes → apply
   no  → deliberation assessment
```

A deterministic transition may:

- advance a plan;
- update working state;
- emit an observation;
- invoke a governed tool;
- mark a belief stale;
- wait/sleep/wake;
- create an impasse;
- request deliberation.

When several incompatible actions remain possible without a deterministic tie-breaker, the runtime must not guess.

---

# 7. Deliberation and reasoning requirements

Probabilistic reasoning is activated when deterministic cognition cannot safely resolve the decision or policy/host explicitly requires reasoning.

The strategy produces a provider-neutral `ReasoningRequirement` containing, where applicable:

```text
required/preferred capabilities
context capacity
structured-output requirements
tool-use requirements
reasoning depth/quality needs
latency tolerance
cost/budget constraints
```

It must not contain provider-specific model identifiers.

### Assessment signals

Possible signals include:

```text
uncertainty
novelty
evidence quality
consequence/risk
branching/planning horizon
conflicting beliefs
tool/context requirements
latency budget
cost policy
resource availability
```

No universal complexity score is required.

### Reference strategy

Adaptive Hybrid Cognition may be the first implementation strategy because it provides a useful production baseline:

```text
sufficient deterministic behavior
        → act
otherwise
        → assess reasoning requirement
        → execute bounded reasoning
        → validate proposal
        → commit or retry/escalate
```

AHC is an implementation strategy, not an architectural truth. A later strategy must consume the same kernel and execution boundaries.

### Deliberation bounds

Every deliberative run must have applicable limits for:

```text
wall time
model calls
tokens/usage
reasoning depth
proposal count
retrieval/working-context size
```

Stop when a valid proposal is produced, a bound is reached, the base revision becomes invalid, evidence is unavailable, policy denies further work, or cancellation/retirement/shutdown occurs.

Incomplete deliberation is a valid typed outcome such as `Unresolved`, `BudgetExceeded`, `Cancelled`, `Superseded`, or `Escalated`.

---

# 8. Impasses and bounded substates

An `Impasse` is a typed state in which the current decision path cannot safely continue.

Useful production types include:

```text
MissingOperator
FailedPrecondition
ConflictingBeliefs
ConflictingIntentions
InsufficientEvidence
UnknownSituation
ResourceUnavailable
PolicyBlocked
RepeatedFailure
StaleProposal
ResolutionConflict
```

Minimum data:

```text
ImpasseId
Type
Severity
AffectedEntities
BaseRevision
EvidenceReferences
DependencySet
ResolutionStatus
ParentImpasseId (optional)
Budget
```

### Resolution options

An impasse may be resolved by:

```text
known alternative
additional retrieval
waiting
plan revision
deliberation
strategy change
host escalation
```

### Isolated deliberative substate

A substate is a bounded proposal workspace derived from a parent revision. It may hold local hypotheses, candidate plans, retrieved evidence, assumptions, and working state.

It is not a second authoritative runtime.

Its dependency/read set determines whether parent changes are relevant:

```text
unrelated parent change
    → may continue
related parent change
    → revalidate
assumption broken
    → discard/re-deliberate
unknown conflict
    → fail closed
```

Nested substates are allowed only within explicit depth/time/model-call/usage limits.

A successful resolution produces experience/evidence and may produce a `SkillCandidate`. Solving an impasse once never automatically makes a permanent Skill authoritative.

---

# 9. Plans, operators, and recovery

A `Plan` is durable state. A planner/strategy creates or revises it.

A plan should support:

```text
identity/version
steps
dependencies
preconditions
checkpoints
completion criteria
failure conditions
assumptions
expected effects
```

An operator/action may specify:

```text
identity
preconditions
intended effects
required resources/capabilities
bounded execution semantics
provenance
```

Known safe plan steps should progress without unnecessary model calls.

### Outcome classification

Every externally observable step should distinguish at least:

```text
Completed
Failed
UnknownOutcome
Cancelled
Superseded
```

An unknown external outcome must not be recorded as success.

### Revision and retry

Plan changes record a reason such as new evidence, failed precondition, failed action, belief revision, resource/policy change, higher-priority goal, or explicit reconsideration.

A newer plan revision invalidates stale asynchronous work against the old revision.

---

# 10. Experience and procedural learning

HAgent keeps these concepts distinct:

```text
Experience = what happened
Skill      = reusable procedure derived from evidence
Knowledge  = reusable information
Policy     = rule governing behavior
```

Original experience remains available after proceduralization.

### Candidate synthesis

The first learning implementation should be conservative:

```text
eligible experiences
      ↓
identify repeated structure
      ↓
identify candidate preconditions
      ↓
identify procedure steps
      ↓
identify expected effects
      ↓
collect positive evidence
      ↓
collect negative/counterexample evidence where available
      ↓
create SkillCandidate
```

The synthesis may be deterministic, model-assisted, or hybrid. No single synthesis algorithm is treated as universally correct.

### Candidate evidence

A SkillCandidate should retain:

```text
source experiences
candidate procedure
candidate preconditions
expected effects
applicability evidence
negative evidence/counterexamples
provenance
confidence/evidence summary
source execution/runtime identity
```

A single accidental success should not normally publish an automatic reusable procedure. The default path favors repeated independent evidence, explicit applicability conditions, regression checks, bounded rollout, and reversible publication.

A candidate may remain episodic knowledge without becoming a Skill.

---

# 11. Learned-resource reliability and adaptation

Published Knowledge and Skills remain subject to the existing 0.9576 reliability/adaptation architecture.

The cognitive runtime consumes its results; it does not create a second learned-resource system.

Relevant states include:

```text
Applicable
Uncertain
Invalidated
Stale
Contradictory
```

If a Skill depends on a versioned Knowledge item, belief assumption, tool contract, or other resource, that dependency must be represented where applicable.

```text
resource revision changes
      ↓
find dependent learned resources
      ↓
revalidate
      ↓
retain / downgrade / quarantine / replace / retire
```

Contradictory authoritative resources require an explicit contradiction result or configured precedence. Silent deletion is not acceptable.

Resource growth is bounded by retention/consolidation policy. Forgetting means controlled removal from active retrieval or archival, not a claim about human memory.

When a learned resource is uncertain or invalid, the runtime must have a safe fallback such as another resource, deterministic behavior, bounded reasoning, or host escalation.

---

# 12. Concurrency and scheduling

Persistent cognition is asynchronous. Typical concurrent work includes:

```text
events
retrieval
deliberation
plan execution
tools
learning evaluation
intervention
recovery
```

Required invariants:

- event ingestion does not block on LLM execution;
- cognitive work is bounded per runtime;
- independent runtimes remain isolated and independently schedulable;
- stale proposals are rejected or revalidated;
- cancellation propagates to owned work;
- retirement/shutdown invalidates new cognitive authority;
- host scheduling remains host-controlled where required.

A future scheduler may add priority/deadline policies, but cognitive correctness must not depend on one specific scheduling algorithm.

---

# 13. Failure and recovery semantics

Minimum failure classes include:

```text
InvalidInput
PolicyDenied
AuthorizationDenied
ResourceUnavailable
DeliberationTimeout
BudgetExceeded
Cancelled
Superseded
StaleProposal
Conflict
ProviderFailure
ToolFailure
UnknownOutcome
RecoveryFailure
```

No failure path may silently mutate newer authoritative state.

### Restart recovery

```text
load latest durable cognitive revision
      ↓
identify incomplete work
      ↓
invalidate obsolete execution authority
      ↓
reconcile known outcomes
      ↓
resume / retry / mark unknown / re-deliberate
```

Live provider sessions, cancellation tokens, synchronization primitives, secrets, and transport state are not persistent cognitive state.

---

# 14. Policy, authorization, and adversarial input

Model output, retrieved content, memory, and external observations are untrusted unless a separate authoritative boundary says otherwise.

They cannot directly grant:

```text
authorization
resource capability
policy changes
host-side permissions
published Skill authority
published Knowledge authority
kernel mutation rights
```

Consequential changes pass through the existing HAgent policy, authorization, capability, budget, and host-side authority boundaries.

Prompt text is never an enforcement mechanism.

---

# 15. Observability and evaluation

Meaningful cognitive transitions must expose bounded diagnostic facts without requiring raw prompt/response storage.

At minimum observe:

```text
event triage decision
workspace selection
deterministic vs deliberative decision
reasoning requirement
escalation path
impasse creation/resolution
proposal conflict outcome
belief revision
goal/intention/plan revision
learning candidate lifecycle
resource invalidation
recovery outcome
```

Production evaluation should measure:

- task/goal success;
- plan completion;
- unnecessary model-call rate;
- deliberation rate;
- recovery success;
- stale-proposal rejection;
- cognitive-state consistency;
- candidate acceptance/rejection;
- published-resource regression;
- latency;
- cost/resource use;
- failure/escalation frequency.

Evaluation is evidence and does not directly mutate authoritative state.

---

# 16. Minimum implementation contracts

Before a full persistent cognitive runtime is considered complete, the following provider-neutral contracts must exist or have an explicit simpler equivalent:

```text
CognitiveState
CognitiveRevision
CognitiveProposal
CognitiveTransitionResult
Belief
Goal
Intention
Plan
PlanStep / Operator
Impasse
DecisionContext
DecisionWorkspace
ReasoningRequirement
Experience
SkillCandidate
ResourceApplicabilityAssessment
ICognitiveStrategy
ICognitiveScheduler
```

The names are not sacred. The externally observable behavior and invariants are.

### Implementation order

```text
A. state + revision/commit authority
B. goal / intention / plan state
C. observation / belief state
D. semantic proposal/conflict handling
E. event triage + bounded workspace
F. deterministic decision path
G. bounded deliberation + reasoning requirement
H. impasse + isolated substate
I. plan execution/revision/recovery
J. experience + learning candidates
K. learned-resource applicability/adaptation
L. cognitive scheduling + recovery hardening
M. strategy comparison/optimization
```

Every step must be independently verifiable before being treated as complete.

---

# 17. Verification standard

A cognitive capability is not complete because an interface or architecture document exists.

Completion requires:

1. implementation;
2. deterministic contract/unit tests where applicable;
3. public `HAgent.Example` verification for externally meaningful behavior;
4. verification on required framework targets;
5. documented failure and concurrency semantics;
6. compliance with policy, authorization, snapshot, persistence, and host-boundary invariants.

Minimum scenarios include:

```text
persistent state survives valid restart recovery
method revision does not silently change goal/intention
invalidated belief triggers affected revalidation
concurrent proposals cannot corrupt state
unrelated state changes do not unnecessarily invalidate proposals
related dependency changes force revalidation
routine work proceeds without an LLM
ambiguous work escalates within bounds
deliberation timeout produces safe typed outcome
impasse resolution cannot overwrite newer cognition
nested impasses stop at configured limits
failed steps do not become false success
unknown external outcomes remain unknown
repeated experience can create candidates without forced publication
rejected candidates cannot mutate authoritative resources
learned-resource dependency changes trigger selective revalidation
uncertain skills fall back safely
independent runtime instances remain isolated
retirement/shutdown prevents new authority
recovery invalidates obsolete execution work
adversarial content cannot bypass policy
strategy replacement cannot bypass kernel boundaries
multiple strategies can be evaluated on the same scenario
```

## Final rule

HAgent is not implementing a theory of mind.

HAgent is implementing a **persistent, versioned, bounded decision runtime** that can use deterministic procedures, external resources, and replaceable probabilistic reasoning while preserving state integrity, authorization, recovery, and empirical evaluation.

Research may improve the strategies later. The kernel and production invariants remain independent of any particular theory.