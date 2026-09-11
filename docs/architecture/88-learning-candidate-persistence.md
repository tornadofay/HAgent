# Learning Candidate Persistence, Retention, and Review

## Position

Phase 0.9575 Slice 9 makes the canonical `AiLearningCandidate` lifecycle durable without creating a second candidate lifecycle or resource model.

The persisted form is an `AiLearningCandidateRecord`. It is a durable envelope containing the canonical lifecycle state/revision, typed candidate metadata, a serialized typed payload, retention metadata, evaluation/provenance evidence, and review authorization evidence. It does not become an authoritative Memory, Knowledge resource, or Skill definition.

## Canonical flow

```text
Typed candidate
    ↓
Slice 8 lifecycle gate
    ↓
AiLearningCandidateRecord
    ↓
IAiLearningCandidateStore
    ↓
PendingReview / Approved / Rejected durable state
    ↓
Learning Review authorization
    ↓
Optimistic revision-checked transition
    ↓
Later Slice 10 authoritative promotion
```

## Store boundary

`IAiLearningCandidateStore` is provider-neutral. It owns durable candidate records, bounded queries, optimistic revision checks, and expiry cleanup. It does not authorize promotion and does not publish resources.

The File backend provides the first durable implementation using an atomic temporary-file replacement pattern. `InMemoryAiLearningCandidateStore` provides deterministic test/runtime infrastructure. SQL Server/MySQL candidate-specific backends remain optional future storage work and must consume the same boundary rather than introduce another candidate model.

## Persistence semantics

A record preserves:

- candidate ID/type and canonical lifecycle status;
- lifecycle revision;
- proposed scope and source execution/runtime/profile identity;
- confidence, evidence, provenance, contradiction, retention, and evaluation state;
- Slice 8 learning-policy and promotion-authorization provenance;
- typed payload JSON required for later resource-specific promotion;
- creation/update and optional expiry timestamps;
- the latest review action, reviewer identity, review policy provenance, outcome, reason, and timestamp.

The typed payload remains non-authoritative. Persistence is storage of a proposal/lifecycle record, not publication.

## Retention

`AiLearningCandidateRetentionPolicy` maps the candidate's declared retention class to an optional bounded number of days. Expiry is calculated at capture time and is never silently extended by review updates. Stores exclude expired candidates from normal reads and support deterministic purge.

An unmapped retention class means no store-imposed expiry; the record remains subject to later policy/administrative lifecycle rules.

## Review and authorization

`AiLearningCandidateReviewService` is the host-facing review boundary. It evaluates the existing unified `IAiPolicyEngine` with operation `learning.review` and resource type `learning-candidate`. Review identity is explicit and never substituted by an anonymous identity.

Only `PendingReview` candidates can be reviewed. The service applies the canonical candidate lifecycle transition and persists the new revision using an expected-revision compare-and-swap boundary. A stale reviewer cannot overwrite a newer lifecycle state.

Review authorization evidence is persisted with the candidate record for later audit/diagnostic consumption; the store itself remains an enforcement of persistence and revision integrity, not a second authorization engine.

## Restart and isolation

A persisted record can be read by a new store instance/process without sharing the previous in-memory runtime object. Returned records are detached copies. Concurrent stale updates fail rather than silently overwriting a newer lifecycle revision.

## Deferred boundaries

Slice 9 does not create authoritative Memory, Knowledge, or Skill state. It also does not build the production Learning Review UI, context integration, runtime learning-input capture, or resource-specific promotion. Those remain later 0.9575 slices.
