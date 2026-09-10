# Persistent Cognitive Runtime — Core Cognitive Algorithms

## Purpose

This document is the normative algorithm specification for Phase 0.97 Persistent Cognitive Runtime. It fills the gap between the stable runtime architecture in `docs/architecture/16-cognitive-runtime.md` and future implementation.

The document specifies decision procedures, state transitions, authority boundaries, concurrency semantics, bounded deliberation, skill formation, resource adaptation, and verification requirements. It does not define host-domain models, provider-specific behavior, or one universal theory of intelligence.

The algorithms are intentionally provider-neutral. A cognitive strategy may implement them differently, but an implementation must preserve the externally observable invariants defined here.

## Design position

HAgent should not claim to have solved general cognition, neural continual learning, or universal skill induction. The objective is narrower and engineering-focused:

- maintain coherent persistent cognitive state;
- make cognitive commitments durable but revisable;
- allow methods to change without unnecessarily changing goals;
- isolate uncertain deliberation from authoritative state;
- apply proposals transactionally against versioned state;
- learn reusable procedures without discarding the original experience;
- adapt learned resources without silently corrupting previously trusted behavior;
- remain bounded under concurrency, latency, failure, and resource pressure;
- make the reasons for cognitive transitions inspectable and evaluable.

## Algorithmic invariants

1. The Cognitive Kernel is the authority for cognitive state ownership, revision, lifecycle, and persistence.
2. A Cognitive Strategy is a decision mechanism, not an authority boundary.
3. Model output is always a proposal or evidence unless explicitly transformed by a validated HAgent transition.
4. Host-authoritative domain state is never replaced by inferred belief state.
5. Every state-changing proposal identifies the cognitive revision against which it was produced.
6. A stale proposal never wins by last-writer-wins semantics.
7. A proposal may be rejected even when it is internally consistent if policy, authorization, resource state, or newer evidence makes it inapplicable.
8. Persistent goals, intentions, and plans have distinct identities and lifecycle semantics.
9. Intention persistence does not imply method persistence.
10. An impasse is a bounded failure of the current cognitive path, not merely "the model was uncertain."
11. Deliberative substates are isolated proposal workspaces, not alternate authoritative runtimes.
12. Experience remains available after proceduralization; a derived Skill never becomes the only record of what happened.
13. Learned resources are versioned and governed; learning does not silently rewrite the Cognitive Kernel.
14. Deterministic cognition is preferred when sufficient; model execution is justified by an explicit reasoning requirement.
15. Every unbounded loop in the cognitive layer must have a configured or inherited bound: time, depth, steps, queue work, model calls, tokens, or another applicable resource dimension.
16. Failure to complete deliberation is a valid outcome. The runtime must have bounded fallback behavior.

---

# 1. Cognitive state model

## 1.1 Authoritative state

The runtime maintains one authoritative cognitive state at revision `R`:

```text
CognitiveState(R)
├── Beliefs
├── WorkingState
├── AttentionFrame
├── Goals
├── Intentions
├── Plans
├── Impasses
├── ResourceReferences
├── ActivationState
└── StrategyStateReference
```

The state is immutable for readers. A state transition creates revision `R+1` after validation and commit.

## 1.2 Proposal model

All asynchronous reasoning, learning, intervention, and external observation interpretation that can change cognitive state must produce a typed proposal before mutation.

Conceptually:

```text
CognitiveProposal
├── ProposalId
├── BaseRevision
├── ReadSet
├── AssumptionSet
├── ProposedChanges
├── EvidenceReferences
├── Producer
├── Reason / Trigger
├── CreatedAt
├── BudgetUsed
└── Expiration
```

The `ReadSet` identifies the cognitive entities and resource revisions that materially influenced the proposal. `AssumptionSet` identifies conditions whose validity is required for safe application.

A proposal can contain several change operations, but each operation must identify its target entity and expected revision/version where applicable.

## 1.3 Atomic state transition

The canonical state transition is:

```text
Read current state R
      ↓
Build proposal against R
      ↓
Validate authority / policy / assumptions
      ↓
Check semantic conflicts against current state
      ↓
Apply compatible changes atomically
      ↓
Commit R+1
      ↓
Publish revision event
```

If validation fails, the proposal is rejected, expires, or is re-deliberated according to the failure class.

No partial application is allowed for a proposal declared atomic. Implementations may split unrelated work into separate proposals when the strategy explicitly defines that boundary.

---

# 2. Goal, intention, and method authority

## 2.1 Separate authorities

HAgent must maintain three distinct layers:

```text
Goal authority
    What outcome is desired?

Intention authority
    Which goals has the runtime committed to pursue?

Method authority
    Which current plan/operators are being used to pursue an intention?
```

A method revision must not implicitly revise the goal. An intention may be revised without deleting the underlying goal. A goal may be revised independently of an active plan.

## 2.2 Goal lifecycle

Minimum goal states:

```text
Proposed → Active → Succeeded
                 ├→ Failed
                 ├→ Suspended
                 ├→ Abandoned
                 └→ Superseded
```

A goal may return from `Suspended` to `Active` only through an explicit validated transition.

## 2.3 Intention lifecycle

Minimum intention states:

```text
Candidate → Adopted → Suspended → Completed
                    ├→ Revised
                    ├→ Abandoned
                    └→ Superseded
```

An intention references a goal identity and records the commitment revision at which it was adopted.

## 2.4 Method authority

A plan is the current method attached to an intention. The plan may be replaced, revised, or abandoned while the intention remains active.

The default rule is:

```text
new evidence that invalidates method assumptions
    → reconsider plan first

new evidence that invalidates commitment rationale
    → reconsider intention

new evidence that invalidates desired outcome itself
    → reconsider goal
```

The runtime must never infer which level is invalid solely from the fact that a lower level failed. The strategy must explicitly classify the impact.

## 2.5 Reconsideration triggers

Reconsideration is mandatory when one or more of the following occur and the applicable policy says the state is affected:

- a required plan precondition becomes false;
- a plan action produces an incompatible outcome;
- a belief supporting a goal/intention becomes contradicted or stale;
- a higher-priority goal creates a conflict;
- the environment materially changes relative to plan assumptions;
- policy, authority, capability, or resource constraints invalidate the current method;
- repeated failure exceeds the configured retry/reconsideration threshold;
- a human or host explicitly requests reconsideration.

## 2.6 Reconsideration decision procedure

```text
Trigger detected
      ↓
Identify affected goals / intentions / plans
      ↓
Classify impact:
  MethodInvalid
  IntentionInvalid
  GoalInvalid
  ConstraintChanged
  EvidenceInsufficient
  Unknown
      ↓
Apply strategy/policy rules
      ↓
Select action:
  continue
  revise plan
  revise intention
  revise goal
  suspend
  abandon
  request deliberation
  escalate to host
```

The classification is evidence-based and must be explainable. An implementation must not use a hidden scalar score as the only reason for goal reconsideration.

## 2.7 Conflict arbitration between intentions

When active intentions conflict, the runtime must not use insertion order or model preference as the implicit winner.

The arbitration procedure is:

```text
Collect conflicting intentions
      ↓
Apply explicit conflict constraints
      ↓
Compare priority / deadline / consequence / policy
      ↓
Check host constraints and resource feasibility
      ↓
Select outcome:
  retain both
  serialize
  suspend lower priority
  revise one method
  supersede one intention
  escalate
```

Tie-breaking must be deterministic for equal inputs. An unresolved conflict is an explicit `Impasse` rather than silent arbitrary selection.

## 2.8 Commitment stability

The runtime must resist unnecessary intention thrashing.

An intention should not be reconsidered solely because a new event exists. Reconsideration requires a trigger that materially affects feasibility, expected outcome, priority, constraints, or supporting evidence.

Implementations should support hysteresis/cooldown policy so an intention does not repeatedly alternate between two methods or goals without meaningful state change.

Minimum safeguards:

- minimum commitment interval where appropriate;
- maximum reconsiderations per time window;
- repeated-proposal detection;
- no-progress threshold;
- escalation after oscillation is detected.

---

# 3. Belief formation, interpretation, and revision

## 3.1 Observation is not belief

Incoming events and host observations are evidence. They do not automatically become beliefs.

```text
Observation
   ↓
Interpretation
   ↓
BeliefCandidate
   ↓
Validation / provenance / confidence
   ↓
Belief revision proposal
   ↓
Cognitive commit
```

Interpretation may be deterministic, model-assisted, or hybrid.

## 3.2 Interpretation contract

An interpretation result should identify:

```text
source observation(s)
interpreted entities/facts
confidence/quality
alternative interpretations when materially ambiguous
supporting evidence
required follow-up information
```

When ambiguity materially affects a consequential decision, the correct result may be `Ambiguous` rather than a fabricated definite belief.

## 3.3 Belief precedence

Belief conflict resolution must consider at least:

```text
source authority
observation freshness
directness of evidence
confidence/quality
scope
recency
explicit host authority
```

Host-authoritative facts outrank inferred beliefs when the host provides authoritative state. Lower-quality or stale inferred beliefs must not silently overwrite stronger current evidence.

## 3.4 Belief dependencies

Goals, intentions, plan steps, and learned resources may declare dependencies on belief identities and versions.

A belief revision can therefore produce:

```text
Belief B17 changed
      ↓
Dependents discovered
      ↓
Plan step P4 affected
Intention I2 affected
Skill assumption S9 affected
      ↓
Revalidate each dependent
```

Dependency revalidation must be selective; unrelated state changes must not unnecessarily invalidate all cognition.

---

# 4. Attention and event triage

## 4.1 Event triage

Every incoming event first passes through bounded deterministic triage.

```text
Event
  ↓
Dedup / expiry / scope checks
  ↓
Salience assessment
  ↓
Classify:
  Ignore
  Retain
  UpdateState
  ReactiveAction
  DeliberationTrigger
  HostEscalation
```

An event may be retained without immediately causing cognition.

## 4.2 Salience model

Salience is a multi-signal decision, not a single model-generated number.

Relevant dimensions include:

- urgency;
- novelty;
- goal relevance;
- intention relevance;
- uncertainty;
- risk/consequence;
- relationship/social relevance when supplied;
- policy-defined importance;
- freshness;
- expected information value;
- estimated processing cost.

Each dimension is bounded and explainable. The strategy may choose the weighting or rule set.

## 4.3 Attention selection

The runtime selects an attended set under a hard budget:

```text
Candidate events/resources
      ↓
Eligibility filtering
      ↓
Deterministic score / ranking
      ↓
Redundancy reduction
      ↓
Budget allocation
      ↓
GlobalWorkspaceFrame
```

Selection must be stable for equal inputs and preserve the provenance of selected items.

---

# 5. Reactive cognition

Reactive cognition is the first path evaluated after attention.

## 5.1 Reactive decision procedure

```text
DecisionContext
      ↓
Applicable operators / active plan step lookup
      ↓
Check preconditions
      ↓
Check policy / authorization / resource availability
      ↓
Check expected consequence bounds
      ↓
If one safe deterministic transition exists:
      execute transition
Else:
      return NoSafeReactiveResolution
```

Reactive cognition must not guess when several incompatible actions are plausible and no deterministic policy resolves the conflict.

## 5.2 Reactive execution

A reactive transition may:

- advance a plan step;
- update bounded working state;
- emit an observation;
- invoke a governed tool;
- create a wait/sleep state;
- mark a belief stale;
- create a typed impasse;
- request deliberation.

---

# 6. Deliberation activation and reasoning escalation

## 6.1 Deliberation trigger

A deliberation request is created only when deterministic cognition cannot safely resolve the current decision or an explicit policy/host trigger requires reasoning.

Minimum triggers:

- novelty above configured coverage;
- unresolved ambiguity;
- insufficient evidence;
- conflicting beliefs;
- blocked plan/failed precondition;
- significant goal conflict;
- high consequence decision;
- repeated deterministic failure;
- explicit human/host request.

## 6.2 Progressive reasoning assessment

HAgent must not treat model size as the cognitive complexity metric.

Instead:

```text
Assess:
  uncertainty
  evidence adequacy
  novelty
  consequence
  branching
  planning horizon
  context requirement
  tool requirement
  structural output requirement
  latency budget
  cost policy
      ↓
ReasoningRequirement
```

The result may explicitly be `NoModelRequired`.

## 6.3 Escalation stages

The reference AHC policy should support at least:

```text
Stage 0: deterministic cognition
Stage 1: lightweight reasoning
Stage 2: stronger/deeper reasoning
Stage 3: bounded multi-pass deliberation
Stage 4: host escalation / unresolved
```

Escalation occurs only when the previous stage fails to satisfy the decision requirement or its confidence/evidence threshold.

Every escalation records:

```text
previous stage
trigger
missing capability/evidence
selected next stage
budget remaining
result
```

## 6.4 Deliberation stopping rules

Deliberation must stop when any of the following occurs:

- an acceptable validated proposal is produced;
- the reasoning budget expires;
- the maximum reasoning depth is reached;
- repeated equivalent proposals are produced;
- required evidence is unavailable;
- the current cognitive revision becomes invalid;
- policy denies further reasoning;
- cancellation/retirement/shutdown occurs.

An incomplete deliberation must produce a typed outcome such as `Resolved`, `Unresolved`, `Superseded`, `BudgetExceeded`, `Cancelled`, or `Escalated`.

---

# 7. Semantic concurrency and proposal application

## 7.1 Why revision number alone is insufficient

A revision number detects that something changed but does not tell whether the change matters to the proposal.

Therefore the implementation must use both:

```text
BaseRevision
+
ReadSet / AssumptionSet
+
entity/resource revision checks
```

## 7.2 Conflict classification

When applying a proposal against current state, classify changes as:

```text
NoConflict
RelatedChange
AssumptionBroken
TargetAlreadyChanged
PolicyChanged
ResourceChanged
GoalConflict
PlanConflict
BeliefConflict
UnknownConflict
```

Unrelated changes may permit application when all declared assumptions still hold.

Related or assumption-breaking changes require revalidation. Unknown conflicts fail closed.

## 7.3 Compare-and-apply procedure

```text
Current state C
Proposal P(base R)
      ↓
Check terminal/lifecycle authority
      ↓
Check policy/authorization
      ↓
Resolve referenced entity versions
      ↓
Check ReadSet and AssumptionSet
      ↓
Classify conflicts
      ↓
NoConflict      → apply
RelatedChange   → revalidate proposal
AssumptionBroken→ reject/re-deliberate
TargetChanged   → reject or merge only if explicitly supported
UnknownConflict → reject
      ↓
Commit revision N+1 if valid
```

There is no generic semantic auto-merge for arbitrary cognitive state. A strategy may define safe merge functions for specific entity types.

## 7.4 Idempotency

Every state-changing proposal must have a stable proposal identity and operation key suitable for duplicate detection.

Replaying the same successfully committed proposal must not produce a second logical transition.

---

# 8. Impasse algorithm

## 8.1 Impasse definition

An `Impasse` is created when the current cognitive path cannot safely continue within its deterministic authority and available evidence.

Minimum fields:

```text
ImpasseId
Type
Severity
AffectedEntities
Trigger
BaseRevision
EvidenceReferences
DependencySet
ResolutionStatus
ParentImpasseId (optional)
Budget
```

Suggested types:

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

## 8.2 Impasse entry

```text
Current decision cannot safely progress
      ↓
Create typed Impasse at revision R
      ↓
Freeze the affected decision path
      ↓
Choose resolution policy
```

The runtime may continue unrelated cognition that does not depend on the blocked state.

## 8.3 Resolution strategies

An impasse may be resolved by:

```text
Deterministic alternate operator
Additional retrieval
Wait for information/event
Plan revision
Goal/intention arbitration
Bounded deliberative substate
Human/host escalation
Permanent failure / abandonment
```

The strategy must select the cheapest sufficient resolution before escalating to a model.

## 8.4 Bounded deliberative substate

A deliberative substate is:

```text
Substate
├── ParentRuntimeIdentity
├── ParentRevision
├── ReadSet
├── AssumptionSet
├── LocalWorkingState
├── Hypotheses
├── CandidatePlans
├── CandidateBeliefs
├── CandidateResolution
├── Budget
└── Status
```

It is not a second authoritative runtime and does not commit directly to global state.

## 8.5 Isolation rules

The substate may read only an immutable snapshot of:

- the parent state at the captured revision;
- explicitly authorized memory/knowledge/skill resources at their captured versions;
- the triggering event/evidence;
- additional evidence retrieved through permitted mechanisms.

It may create local hypotheses and candidate state, but those are not visible as authoritative parent state until a resolution proposal is validated.

## 8.6 Nested impasses

Nested impasses are allowed only to a configured maximum depth.

```text
Parent Impasse
  └── child impasse
      └── child impasse
          └── depth limit
```

At depth limit, the child must resolve through a non-recursive fallback: return unresolved, wait, escalate, or fail according to policy.

## 8.7 Impasse resolution commit

The substate produces an `ImpasseResolutionProposal` containing:

```text
ParentRevision
ReadSet
AssumptionSet
ResolutionType
ProposedChanges
Evidence
ExpectedBenefits
KnownLimitations
```

Application uses the normal semantic compare-and-apply procedure. A newer conflicting revision causes revalidation or rejection; it never silently merges stale reasoning.

---

# 9. Plan execution, failure, and replanning

## 9.1 Plan step contract

A plan step must expose enough structure for deterministic evaluation:

```text
StepId
Revision
Preconditions
Action/Operator
ExpectedEffects
CompletionCriteria
FailureConditions
Dependencies
CheckpointPolicy
RetryPolicy
```

## 9.2 Step execution loop

```text
Current step
   ↓
Check plan/intention/cognitive revision
   ↓
Check preconditions
   ↓
Check policy/authorization/capability
   ↓
Execute operator or request execution
   ↓
Validate outcome
   ↓
Completed → advance
Failed → classify failure
Unknown → reconcile / wait
Invalidated → replan
```

## 9.3 Unknown outcome

An externally observable action may have an unknown outcome when timeout/cancellation occurs after dispatch or when the host cannot determine the external result.

`Unknown` must not be converted automatically to `Failed` or `Succeeded`.

The next cognitive action may be:

```text
query/reconcile
retry only when idempotent/safe
wait
compensate
escalate
```

This uses the durable goal/plan recovery architecture from Phase 0.9591.

## 9.4 Replanning threshold

Plan failure does not imply immediate full replanning. The runtime first determines whether:

```text
retry is safe
local step replacement is sufficient
another operator satisfies the same subgoal
remaining plan is still valid
```

Full reconsideration occurs when the failure materially changes the feasibility of the current method or its assumptions.

---

# 10. Experience retention and episodic memory

## 10.1 Experience record

A significant cognitive episode should be captured as attributable experience, containing bounded references to:

```text
Situation/context reference
Relevant observations
Beliefs used
Goal/intention/plan revision
Actions/operators used
Tool/execution references
Outcome evidence
Failures / surprises
Resource versions used
Learning signals
```

Raw payload storage remains subject to existing HAgent retention, privacy, and observability rules.

## 10.2 Experience selection for learning

Not every episode should trigger proceduralization.

Candidate episode selection should prioritize:

- repeated occurrence;
- success or informative failure;
- high effort/model cost;
- deterministic outcome evidence;
- novelty followed by successful resolution;
- repeated impasse followed by successful resolution;
- stable environment/contract conditions;
- explicit host marking for learning.

---

# 11. Episodic experience → SkillCandidate synthesis

This is the first algorithmic specification for proceduralization. It is deliberately conservative.

## 11.1 Fundamental rule

**Do not derive a trusted Skill from one successful trajectory by default.**

A single episode may create a `SkillCandidate`, but publication requires stronger evidence according to learning policy.

## 11.2 Candidate formation pipeline

```text
Eligible experiences
      ↓
Episode normalization
      ↓
Pattern detection
      ↓
Action-sequence analysis
      ↓
Invariant/precondition extraction
      ↓
Outcome/effect extraction
      ↓
Negative/exception evidence collection
      ↓
Candidate skill synthesis
      ↓
Offline validation against retained episodes
      ↓
Evaluation
      ↓
Policy / approval
      ↓
Publish new Skill version or reject
```

## 11.3 Episode normalization

The system converts episodes into provider-neutral procedure traces:

```text
State predicates / context features
Action/operator
Observed result
Outcome
```

The representation must preserve ordering and the distinction between:

```text
required context
incidental context
action
observation
outcome
```

The normalization process must not assume that natural-language narration is an executable procedure.

## 11.4 Pattern detection

The initial reference implementation should support deterministic structural matching before model-assisted abstraction.

Suitable first-pass signals include:

- repeated operator sequences;
- repeated precondition/effect structures;
- repeated tool-operation patterns;
- repeated successful subplans;
- common subsequences across episodes.

Model-assisted generalization may propose additional abstractions, but cannot bypass validation.

## 11.5 Causal caution

HAgent must not claim causal certainty from sequence correlation alone.

For each candidate step, classify evidence as:

```text
Required
Likely useful
Observed but unproven
Incidental
Unknown
```

A step may be removed from a candidate only when retained evidence demonstrates it is unnecessary or policy explicitly permits abstraction risk.

## 11.6 Preconditions

Candidate preconditions are formed from conditions common to successful episodes and absent/violated in relevant failures.

```text
Positive successful episodes
        +
Negative/failed episodes
        ↓
Candidate applicability conditions
```

When negative evidence is unavailable, the candidate must remain conservatively scoped instead of assuming broad applicability.

## 11.7 Generalization levels

The skill synthesizer should support at least:

```text
Exact replay pattern
Bounded parameterized procedure
Conditionally generalized procedure
```

A candidate must never generalize beyond its evidence boundary merely because the model can produce a more abstract description.

## 11.8 Candidate confidence

Confidence is evidence metadata, not authorization.

A candidate should accumulate evidence over time:

```text
candidate confidence
    ↑ with independent successful applications
    ↓ with failures / contradictions
    ↓ with environment drift
```

The promotion policy decides the threshold and evidence requirements.

## 11.9 Partial proceduralization

A candidate may contain deterministic and unresolved portions:

```text
Step 1 deterministic
Step 2 deterministic
Step 3 requires deliberation
Step 4 deterministic
```

This is preferred over forcing the whole episode into either a fully deterministic Skill or a fully model-driven procedure.

## 11.10 Negative transfer protection

Before publication, evaluate the candidate against:

- successful episodes;
- known failures;
- edge cases;
- materially different contexts;
- current policy/capability constraints.

A candidate that improves one case class while damaging another must remain scoped, revised, or rejected.

---

# 12. Learned resource applicability and reliability

0.9576 provides the post-promotion reliability foundation. The cognitive runtime must consume it rather than invent a parallel trust system.

## 12.1 Applicability evaluation

Before relying on a learned Skill/Knowledge artifact, the runtime should evaluate:

```text
scope compatibility
version validity
preconditions
resource dependencies
freshness
known contradictions
historical success
recent failures
environment compatibility
policy/capability availability
```

Possible result:

```text
Applicable
ConditionallyApplicable
Uncertain
Invalidated
Unavailable
```

## 12.2 Runtime response to uncertainty

The key rule is:

```text
learned resource uncertain
        ↓
fall back to broader evidence/reasoning
```

Uncertainty must not silently become confidence.

## 12.3 Drift detection

A resource becomes a drift candidate when one or more signals repeatedly change:

- required preconditions;
- observed environment structure;
- outcome distribution;
- relevant knowledge versions;
- provider/host capabilities;
- repeated unexpected failures.

Drift is a revalidation trigger, not immediate deletion.

---

# 13. Continual learning and contradiction handling

## 13.1 Layered stability

HAgent should preserve different stability levels:

```text
Cognitive Kernel
    ↓
validated Skills / Policies
    ↓
curated Knowledge
    ↓
Semantic memory
    ↓
Episodic experience
    ↓
Working state
```

New evidence should normally create a revision or a competing version rather than destructive overwrite.

## 13.2 Contradiction detection

Contradiction detection should be provider-neutral and composable. Initial deterministic support should compare normalized assertions where a common identity and mutually exclusive value space are known.

A contradiction record should identify:

```text
resource/assertion A
resource/assertion B
shared subject/identity
conflict relation
source/provenance
revision/version
```

Unknown semantics must remain `Unknown`, not forced into contradiction/non-contradiction.

## 13.3 Conflict resolution

When contradictory resources are both validly scoped:

```text
Check scope
Check authority
Check freshness
Check provenance quality
Check corroboration
Check explicit policy
      ↓
choose:
  A authoritative
  B authoritative
  keep both with scope
  mark uncertain
  request further evidence
  escalate
```

There must be no universal rule that "newer always wins" or "higher confidence always wins" without considering authority and scope.

## 13.4 Dependency invalidation

If Skill A depends on Knowledge K v3 and K becomes invalidated:

```text
K invalidated
    ↓
find dependents
    ↓
mark A conditionally applicable / stale
    ↓
revalidate before execution
```

Dependency invalidation is selective and version-aware.

## 13.5 Consolidation

Consolidation should reduce redundancy without discarding provenance:

```text
similar resources
      ↓
merge candidate
      ↓
verify compatibility
      ↓
publish new version
      ↓
retain provenance links to sources
```

Consolidation must never silently erase conflicting evidence.

## 13.6 Forgetting / archival

Forgetting is policy-controlled archival or removal, not arbitrary deletion.

A utility function may consider:

```text
recent use
success contribution
retrieval frequency
storage cost
redundancy
reliability
scope
retention policy
```

Exact scoring is policy-owned. A low-utility resource should normally become a candidate for archival/expiration before irreversible deletion when retention policy allows.

---

# 14. Goal/plan/skill interaction

The runtime must prevent learned procedures from becoming hidden authorities over persistent intentions.

Example:

```text
Goal G1: Resolve incident
Intention I1: Resolve incident
Plan P3: use Skill S4
```

If S4 becomes uncertain:

```text
Do not change G1 automatically.
Do not abandon I1 automatically.
Invalidate or suspend method P3.
Create a method-level impasse.
Try alternate method or deliberate.
```

Only evidence affecting the goal itself should force goal reconsideration.

This is the core mechanism for persistent intention with live method authority.

---

# 15. Cognitive scheduler and concurrency

## 15.1 Single-writer logical state

The reference implementation should use a **single logical commit authority per runtime instance** for authoritative cognitive revision.

This does not mean every operation is single-threaded. Retrieval, model execution, evaluation, and other expensive work may occur concurrently. Only state commits serialize through the cognitive commit boundary.

Conceptually:

```text
Many concurrent workers
        ↓
proposal queue
        ↓
Cognitive Commit Authority
        ↓
revision R → R+1 → R+2
```

## 15.2 Concurrent deliberations

Multiple deliberations may run concurrently when they affect independent state. However, they must declare their ReadSet/AssumptionSet and target entities.

A proposal affecting the same goal/intention/plan as another pending proposal must be arbitrated deterministically at commit time.

## 15.3 Priority classes

The scheduler should recognize at least:

```text
Safety / host-mandated
Goal-critical
Time-sensitive
Normal
Background learning/consolidation
```

Scheduling priority does not bypass policy or authorization.

## 15.4 Starvation protection

Background cognition must not starve indefinitely. The scheduler should support bounded fairness or aging, subject to host-controlled limits.

## 15.5 Event storm protection

Under event storms:

```text
bounded intake queue
→ dedup/coalesce where policy allows
→ salience ranking
→ drop/retain lower-value events explicitly
→ preserve critical events
```

Dropped/coalesced event counts remain observable.

---

# 16. Failure semantics

The runtime must treat failure as typed state, not generic exception text.

Minimum cognitive failure categories:

```text
NoApplicableOperator
EvidenceInsufficient
AmbiguousInterpretation
ConflictUnresolved
DeliberationTimeout
DeliberationBudgetExceeded
ProposalStale
ProposalConflict
ResourceUnavailable
ResourceUncertain
PolicyDenied
AuthorizationDenied
HostEscalationRequired
RepeatedNoProgress
StrategyUnavailable
RecoveryConflict
```

Every failure must have a bounded next action:

```text
retry
alternative
wait
re-deliberate
suspend
escalate
abandon
```

The runtime must avoid infinite automatic retry/re-deliberation loops.

---

# 17. Time and real-time behavior

The runtime distinguishes:

```text
Host/environment time
Cognitive scheduling time
Deliberation/model time
External action time
```

A host may impose a hard response deadline. When that deadline cannot accommodate deliberation, the runtime should prefer:

```text
existing safe plan/operator
safe fallback
wait/defer
host escalation
```

over unbounded reasoning.

Long-running deliberation must not block event ingestion or unrelated cognitive activity.

---

# 18. Security and adversarial input

## 18.1 Untrusted observations

Natural-language events, retrieved content, tool output, external messages, and model-generated beliefs are untrusted inputs.

They may provide evidence but cannot grant:

- authorization;
- capability;
- policy exceptions;
- host authority;
- trusted instruction status.

## 18.2 Proposal validation

Before a proposal changes authoritative cognition:

```text
schema/contract validation
→ identity/scope validation
→ revision/dependency validation
→ policy
→ authorization where applicable
→ semantic consistency checks
→ commit
```

## 18.3 Prompt injection resistance

Instruction governance remains the authoritative instruction boundary. A cognitive proposal cannot elevate untrusted content by changing a trust/authority field merely because a model requested it.

---

# 19. Evaluation algorithms

Layer-2 algorithms must be testable independently of a vendor model.

## 19.1 Intention persistence metrics

Controlled scenarios should measure:

```text
Goal retention under method failure
Method-change latency
Unnecessary goal-reconsideration rate
Intention oscillation rate
Conflict-resolution determinism
Stale-proposal rejection
```

## 19.2 Impasse metrics

Measure:

```text
Impasse detection precision/recall where a test oracle exists
Resolution success rate
Average deliberation cost
Nested-impasse depth
Repeated-impasse rate
Stale-resolution rejection rate
```

## 19.3 Skill formation metrics

Measure separately:

```text
Candidate usefulness
Applicability precision
Applicability recall
Success uplift vs baseline
Negative-transfer rate
Skill invocation success
Fallback-to-reasoning rate
Candidate promotion false-positive rate
```

A skill that increases one benchmark while increasing failures elsewhere is not considered a successful proceduralization.

## 19.4 Continual adaptation metrics

Measure:

```text
contradiction detection rate
stale-resource detection rate
revalidation success
regression after promotion
resource growth
retrieval noise
archival effectiveness
```

## 19.5 Strategy comparison

The same event/goal scenario must be executable through multiple strategies against the same kernel and evaluation harness.

Required comparison dimensions include:

- task success;
- unnecessary model-call rate;
- escalation rate;
- latency;
- cost;
- recovery success;
- state consistency;
- skill reuse;
- failure recovery quality.

---

# 20. Reference AHC control loop

The first implementation of Adaptive Hybrid Cognition should follow this control loop:

```text
while runtime active:

    receive bounded events

    triage / deduplicate / expire

    update observations and working state

    select attention frame

    detect affected goals / intentions / plans

    attempt deterministic reactive resolution

    if safe resolution exists:
        propose transition
        validate
        commit if current
        continue

    create typed impasse when needed

    assess reasoning requirement

    if NoModelRequired:
        apply deterministic alternative / wait / escalate
        continue

    create bounded deliberative substate

    perform progressive reasoning escalation

    produce typed proposal

    compare proposal against current revision and dependencies

    if valid:
        commit revision
    else if recoverable conflict:
        revalidate / re-deliberate within budget
    else:
        record unresolved outcome and apply fallback

    record experience/outcome

    trigger governed learning when policy permits

    adapt learned-resource applicability from validated outcomes
```

This is a reference algorithm, not a requirement that all strategies use the same internal loop.

---

# 21. Required public/provider-neutral contracts before implementation

The following contracts should exist before substantial implementation of the corresponding algorithms:

```text
ICognitiveStrategy
ICognitiveScheduler
ICognitiveStateStore
ICognitiveCommitAuthority
ICognitiveAttentionPolicy
IReactiveDecisionPolicy
IDeliberationPolicy
IInterpretationPolicy
IPlanner
IImpasseResolver
ICognitiveRevisionEvaluator
ISkillSynthesizer
ILearnedResourceApplicabilityEvaluator
```

Not every interface must be public from day one. The key requirement is that the responsibilities remain separable so strategies do not absorb persistence, authorization, provider routing, or host authority.

Core data contracts should include equivalents of:

```text
CognitiveState
CognitiveRevision
CognitiveProposal
CognitiveReadDependency
CognitiveAssumption
Belief
BeliefRevision
Goal
Intention
Plan
PlanStep
Impasse
ImpasseResolutionProposal
DecisionContext
GlobalWorkspaceFrame
ReasoningRequirement
Experience
SkillCandidate
ResourceApplicabilityAssessment
```

Exact names may change during implementation, but the responsibilities must not collapse into one generic mutable agent object.

---

# 22. Minimal deterministic acceptance scenarios

Before introducing live-model dependence, the implementation should pass deterministic scenarios for:

1. plan failure changes the method but preserves the goal;
2. supporting belief becomes invalid and forces plan reconsideration;
3. higher-priority intention suspends a conflicting lower-priority intention;
4. equal-priority unresolved conflict becomes an explicit impasse;
5. stale proposal affecting an unrelated entity may still apply when all dependencies remain valid;
6. stale proposal affecting a changed assumption is rejected;
7. two concurrent proposals for one plan serialize deterministically;
8. impasse substate cannot mutate parent state directly;
9. nested impasse stops at configured depth;
10. deliberation stops at time/token/depth budget;
11. late deliberation cannot overwrite a superseding cognitive revision;
12. unknown external action outcome is not converted to success/failure without evidence;
13. repeated successful episodes create a SkillCandidate while preserving source experiences;
14. a candidate containing an unresolved step remains partially proceduralized;
15. positive-only evidence produces conservative applicability rather than universal generalization;
16. negative evidence narrows candidate preconditions;
17. a promoted Skill becomes uncertain after repeated failures and the runtime falls back to reasoning;
18. a changed Knowledge dependency invalidates a dependent Skill applicability assessment;
19. contradiction between scoped resources does not automatically delete either resource;
20. event storms remain bounded without starving critical events;
21. two independent cognitive runtimes cannot share private cognitive state;
22. strategy replacement cannot bypass kernel revision or persistence rules;
23. policy denial prevents a cognitive proposal from becoming authoritative;
24. host-authoritative observation outranks conflicting inferred belief where the contract says it does;
25. a runtime restart rebuilds the latest durable revision and invalidates obsolete in-flight work.

---

# 23. Implementation order

The algorithms should be implemented in dependency order rather than all at once:

```text
A. Cognitive state + revision + commit authority
        ↓
B. Belief / observation / dependency model
        ↓
C. Goal / intention / plan / operator state machine
        ↓
D. Reactive cognition + attention
        ↓
E. Impasse + isolated substates
        ↓
F. Deliberation / ReasoningRequirement / AHC
        ↓
G. Semantic stale/conflict handling
        ↓
H. Plan recovery + reconsideration
        ↓
I. Experience capture
        ↓
J. Skill candidate synthesis + evaluation
        ↓
K. Learned-resource applicability / adaptation
        ↓
L. Multi-strategy evaluation and Workbench
```

Each slice must be complete within its own architectural scope and must include deterministic tests and a matching `HAgent.Example` scenario under the rules in `AGENTS.md`.

## Final architectural position

The Persistent Cognitive Runtime is not defined by one magic algorithm. It is defined by a set of explicit contracts and bounded decision procedures:

```text
Observe
  ↓
Interpret
  ↓
Attend
  ↓
Assess
  ↓
React or deliberate
  ↓
Propose against revision R
  ↓
Check dependencies / policy / authority
  ↓
Commit R+1 or reject
  ↓
Observe outcome
  ↓
Learn conservatively
  ↓
Revalidate over time
```

The architecture deliberately preserves uncertainty. When HAgent cannot establish that a goal, method, belief, or learned resource remains valid, it must represent that uncertainty and choose a bounded fallback rather than manufacture certainty.
