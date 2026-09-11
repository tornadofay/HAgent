# Learning Lifecycle Gate

## Position

Phase 0.9575 Slice 8 defines the canonical admission gate between a typed learning candidate and later authoritative promotion.

The lifecycle is:

```text
Experience / observation
        ↓
Typed candidate
        ↓
Learning Policy validation
        ↓
Learning Mode gate
        ↓
Unified promotion authorization
        ↓
Rejected | PendingReview | Approved
        ↓
Later authoritative promotion
```

The lifecycle gate does not publish Knowledge, create a new Skill version, or write Memory. Those are later resource-persistence/promotion responsibilities.

## Responsibilities

`AiLearningLifecycleCoordinator` composes four decisions:

1. typed candidate validation;
2. deterministic `AiLearningPolicy` evaluation;
3. provider-neutral `AiLearningMode` interpretation;
4. the existing `IAiPolicyEngine` learning-promotion authorization boundary when automatic promotion is eligible.

No model call is required.

## Learning Mode semantics

- `Disabled` rejects admission into the active learning lifecycle.
- `SuggestOnly` routes a policy-approved candidate to `PendingReview`.
- `AutomaticWithPolicy` may enter `Approved` only when learning policy and unified authorization permit promotion without an additional review requirement.
- `FullyAutomatic` may enter `Approved` when the same authorization conditions permit unreviewed promotion. A policy requiring review still produces `PendingReview`.

## State ownership

`AiLearningCandidate` remains the single lifecycle state holder. The coordinator returns a decision object and applies only the canonical candidate transitions already defined by that lifecycle.

A lifecycle decision cannot be applied to a different candidate identity or to a candidate that is no longer in `Proposed` state.

## Authority boundary

Approval is not publication.

An `Approved` candidate is only eligible for the later resource-specific promotion boundary. The coordinator never mutates an authoritative Knowledge resource or published Skill definition and never bypasses the unified policy engine.

## Deferred responsibilities

The following remain later 0.9575 work:

- candidate persistence and retention/expiry storage;
- authoritative Memory promotion;
- Knowledge promotion as a new authoritative version;
- Skill promotion as a new immutable published version;
- promotion audit persistence;
- Learning Review UI;
- context/instruction integration;
- runtime learning-input capture;
- final observability and persistence verification.
