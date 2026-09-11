# Learning Authoritative Promotion

## Purpose

Slice 10 is the single boundary that converts an `Approved` durable typed learning candidate into authoritative Memory, Knowledge, or Skill state.

The model remains:

```text
Typed candidate
    ↓
Slice 8 lifecycle admission
    ↓
Slice 9 durable candidate + review
    ↓
Slice 10 fresh promotion authorization
    ↓
Typed payload validation/restoration
    ↓
Authoritative publication target
    ↓
Candidate → Promoted
```

Model output is never an authority. Promotion is an explicit host-controlled operation.

## Authorization

Promotion re-evaluates the existing unified `IAiPolicyEngine` with operation `learning.promote` and an explicit `AgentIdentityContext`.

The promotion service fails closed when identity is missing, when the candidate is not `Approved`, when retention has expired, or when the persisted promotion classification does not permit promotion.

`AiLearningPromotionRequest` remains the provider-neutral authorization request. No prompt text is used as an authorization mechanism.

## Memory

Memory reuses the existing `IMemoryStore` contract. The candidate payload is cloned, receives a fresh authoritative memory identity, and is tagged with bounded learning provenance metadata including the candidate ID and promotion policy rule. The original candidate remains unchanged.

## Knowledge

Knowledge has no hidden persistence implementation in the promotion service. `IAiKnowledgePromotionTarget` is an explicit provider-neutral publication boundary implemented by the host/storage layer.

A target must publish a new authoritative `AiKnowledgeResource` version. It must never mutate an existing published version in place. A candidate whose version is equal to or lower than the current authoritative version is a deterministic conflict/stale candidate.

## Skill

Skill uses the same explicit publication-target boundary through `IAiSkillPromotionTarget`.

Published definitions are immutable versions. A candidate must publish a new version; equal/lower versions are deterministic conflicts/stale candidates. Executable handlers remain outside the persisted Skill definition.

## Candidate lifecycle

The candidate lifecycle is not duplicated. After successful publication, the restored canonical `AiLearningCandidate` calls `Promote()`, advancing its revision. The durable record is updated with an optimistic expected-revision check.

Publication failures therefore leave the candidate `Approved` rather than falsely marking it `Promoted`.

The service serializes promotion attempts for the same candidate within one process and uses the existing revision check as the durable stale-update boundary. Cross-process concurrency remains a later persistence/observability concern rather than a new candidate lifecycle state.

## Provenance and result

The candidate retains its source execution, runtime, profile, provenance, evidence, evaluation, and Learning Policy provenance. `AiLearningPromotionResult` reports the authoritative resource identity/version plus the fresh promotion policy decision and source provenance for later observability/audit integration.

## Explicit exclusions

This slice does not add management UI, context/instruction integration, automatic runtime learning-input capture, or a second persistent Knowledge/Skill repository architecture. Those remain later roadmap work.
