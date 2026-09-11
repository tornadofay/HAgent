# Persistent Cognitive Runtime

## Purpose

HAgent's Persistent Cognitive Runtime is a long-lived cognitive layer above individual executions. It maintains provider-neutral cognitive state for a live runtime agent and coordinates events, beliefs, attention, goals, intentions, plans, memory, learning, and probabilistic reasoning.

The cognitive runtime does not assume that one cognitive architecture is universally correct. It provides a stable cognitive kernel and an extensible cognitive-strategy layer so new research can be implemented, evaluated, versioned, and replaced without redesigning the runtime substrate.

The detailed executable specifications for the Layer-2 cognitive algorithms are defined in [`17-cognitive-algorithms.md`](17-cognitive-algorithms.md). That document is the mechanism authority for state transitions, proposal application, reconsideration, impasse handling, proceduralization, concurrency, bounds, and deterministic verification. Its richer research/extension mechanisms do not automatically become V1 scope; the ordered V1 roadmap is authoritative for delivery scope.

## Core separation

```text
Cognitive Kernel
    -> persistent state, lifecycle, events, revision, history, governance

Cognitive Strategy
    -> how the agent interprets state and chooses cognitive actions

Reasoning Requirement
    -> what reasoning capability is needed

Execution Planner
    -> where/how that reasoning executes

Provider / Model
    -> concrete inference capability
```

The LLM is a replaceable reasoning component inside the cognitive runtime, not the cognitive runtime itself.

## First strategy

HAgent's first reference strategy is **Adaptive Hybrid Cognition (AHC)**.

AHC prefers deterministic cognition when the current state and available policies are sufficient. When uncertainty, novelty, ambiguity, risk, conflict, planning depth, missing knowledge, or other conditions exceed deterministic coverage, AHC escalates to an appropriate reasoning capability. It may choose a lightweight model, a stronger model, multiple reasoning passes, or no model at all.

AHC must not depend on a single provider or model family.

## Cognitive state

A runtime instance may maintain, subject to enabled capabilities and persistence policy:

- Belief State and belief provenance/confidence/validity.
- Working cognitive state.
- Bounded attended state / Global Workspace frame.
- Goals and goal hierarchy/priority.
- Intentions and commitments.
- Plans, steps, operators, dependencies, checkpoints, expected effects, and failure state.
- Memory references and retrieved context.
- Skills and reusable procedures.
- Experiences and execution outcomes.
- Cognitive revision history.
- Activation, waiting, sleeping, recovery, and lifecycle state.

Host-owned domain state remains authoritative. HAgent records interpretations, proposals, and cognitive state; it does not become the source of truth for the host world.

## Deterministic cognition and reasoning escalation

The runtime should evaluate whether it can progress without probabilistic reasoning before invoking an LLM.

The decision may consider:

- confidence and uncertainty;
- novelty and similarity to known situations;
- goal relevance and urgency;
- consequences of an incorrect decision;
- belief conflicts or invalidated assumptions;
- available deterministic operators/skills/policies;
- planning horizon and branching;
- available knowledge and memory;
- tool requirements;
- latency, quota, concurrency, and cost policy.

These signals describe the current reasoning requirement; they are not a promise that HAgent can calculate an objectively correct universal difficulty score.

When probabilistic reasoning is required, the cognition strategy produces a provider-neutral `ReasoningRequirement`. The existing Execution Planner then selects and admits a concrete target. This preserves the separation between cognitive planning and execution planning.

## Belief revision

Observations and execution outcomes can make previous beliefs stale, uncertain, contradicted, or invalid.

Belief revision must be explicit and versioned. A revision may invalidate dependent goals, intentions, plan steps, or learned assumptions. Newer cognitive state must win over stale asynchronous results through the same concurrency and revision protections used by execution state.

## Goals, intentions, plans, and impasses

Goals describe desired states or outcomes. Intentions represent adopted commitments. Plans describe executable structures for reaching goals.

Operators and cognitive actions provide explicit transitions with preconditions, effects, applicability, and bounded execution semantics. Routine progress should continue deterministically when possible.

An **Impasse** represents a bounded state in which current deterministic cognition cannot safely or confidently continue. In V1, impasse handling is intentionally bounded to five explicit outcomes: deterministic recovery or alternate safe action, bounded reasoning/deliberation request, waiting for a required condition or event, abandoning/superseding the current path, or escalation to an authorized host/intervention boundary.

V1 does not require nested impasse substates, recursive subproblem trees, or a separate general cognitive conflict/merge engine. The detailed algorithm specification may expose richer future extension points, but those mechanisms are not required for V1 delivery and must not be introduced as hidden scope.

The distinction between goal authority, intention authority, and method/plan authority is normative. A method may be replaced without automatically changing its intention or goal; changes to each level require an explicit classified cognitive proposal and revision-safe commit.

## Cognitive actions

Not every cognitive transition is an external tool call. The cognitive layer may expose provider-neutral internal actions such as:

```text
RecallMemory
RetrieveKnowledge
SelectSkill
CreateGoal
ReviseGoal
AdoptIntention
CreatePlan
RevisePlan
MarkBeliefStale
RequestDeliberation
RequestInformation
Wait
Sleep
Wake
EmitObservation
```

External tools remain separately governed capabilities and retain normal authorization and side-effect boundaries.

## Learning and proceduralization

Experience is distinct from memory, skills, policies, and cognitive strategy.

A successful or informative experience may produce governed candidates for:

```text
Experience
    -> Memory / Reflection
    -> Knowledge candidate
    -> Skill candidate
    -> Policy candidate
    -> Cognitive improvement candidate
    -> validation/evaluation
    -> versioned adoption or rejection
```

Repeated successful reasoning may be partially proceduralized so future occurrences can be handled deterministically. Proceduralization must be evidence-based, versioned, reversible, and policy-governed. Learning must never silently rewrite the cognitive kernel.

The detailed proceduralization algorithm is defined in `17-cognitive-algorithms.md`. In particular, source episodes are retained; candidate preconditions are derived conservatively from positive and negative evidence; causal certainty is not inferred from sequence correlation alone; generalized candidates must be evaluated against both successes and failures; and uncertain learned resources fall back to broader evidence or probabilistic reasoning.

## Extensible cognition

Cognitive strategy is a replaceable extension point. Future research may introduce strategies with their own names, policies, state requirements, or reasoning approaches while consuming the same HAgent cognitive kernel and execution boundary.

Strategies should be independently evaluable on the same scenarios. HAgent should be able to compare success, recovery, unnecessary model usage, latency, cost, determinism, and long-horizon behavior before promoting a strategy as a default.

## Live cognition workbench

`HAgent.WinForms` should provide a complete runtime Cognition Workbench for authorized operators. It should expose the current strategy/version, beliefs, goals, intentions, attention, global workspace, plans and current step, memory, knowledge, skills, experiences, events, executions, reasoning decisions, learning candidates, and full cognitive history.

Authorized intervention should be performed through runtime APIs, not direct mutation. Operations such as inserting or editing beliefs, creating or reprioritizing goals, revising intentions/plans, injecting observations, requesting deliberation, and pausing/resuming runtime activity must be version-checked, atomic, attributable, auditable, and protected from stale-result overwrite.

## Verified architectural evidence

On 2026-09-11, the existing `AgentRuntimeInstance` type was exercised by the Example-only single-owner architecture spike on both supported targets: .NET Framework 4.8.1 and .NET 9. The spike verified per-agent serialized mutation, stale-result rejection, cancellation, shutdown protection, independent runtime isolation, and concurrent operation of 12 independent runtime agents with all 12 owner loops overlapping.

This evidence validates the core ownership/concurrency assumption used by the Persistent Cognitive Runtime architecture. It does not claim that the production cognitive runtime or its future state contracts are implemented.

## Architectural constraints

- No domain-specific world model in `HAgent.Core`.
- No assumption that LLM reasoning is authoritative.
- No claim that AHC is the final or universally correct cognitive architecture.
- No direct authorization through model output, memory, or prompts.
- No automatic kernel rewriting from learned behavior.
- No coupling between cognitive strategy and a specific model/provider.
- Persistent cognitive state remains separate from live transport/session objects and secrets.
- Cognitive algorithms must remain bounded, deterministic where inputs are deterministic, and explicitly observable at decision boundaries.
- V1 scope decisions in the roadmap are authoritative for delivery: architecture documents must not silently turn a bounded production mechanism into a larger research architecture.
