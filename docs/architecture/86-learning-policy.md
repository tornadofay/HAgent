# Learning Policy and Typed Candidates

## Position

Slice 7 extends HAgent's existing learning boundary. It does not create a second candidate lifecycle, a second authorization engine, or authoritative mutation through model output.

The canonical flow is:

```text
Experience / observation
        ↓
Typed candidate payload
        ↓
AiLearningPolicy
        ↓
Existing AiLearningCandidate lifecycle
        ↓
Existing unified IAiPolicyEngine promotion authorization
        ↓
Later promotion work
```

`AiLearningCandidate` remains the canonical lifecycle state machine: `Proposed`, `PendingReview`, `Approved`, `Rejected`, and `Promoted`. `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` are typed payload contracts that compose that lifecycle rather than replacing it.

## Learning policy

`AiLearningPolicy` is a provider-neutral deterministic admission/evaluation contract. Rules select by candidate type and optional proposed scope, then enforce bounded governance requirements:

- minimum confidence;
- evidence requirement;
- provenance requirement;
- contradiction requirement;
- evaluation requirement;
- retention class;
- promotion authorization classification.

No model call is required to evaluate the policy. An unmatched candidate is denied by default. The policy decision records policy identity/version, matched rule identity, reason, and promotion-authorization classification for later audit/promotion work.

The final external promotion authorization remains the existing `IAiPolicyEngine` / `AiLearningPromotionPolicy` boundary. `AiLearningPolicy` does not bypass or replace that boundary.

## Typed candidates

`MemoryCandidate` carries a cloned `MemoryEntry` and validates its proposed scope against the canonical memory scope. `KnowledgeCandidate` carries a cloned `AiKnowledgeResource` and rejects published/authoritative resources. `SkillCandidate` carries a cloned `AiSkillDefinition` and rejects published/authoritative definitions.

Typed candidates also carry bounded learning metadata for confidence, evidence state, provenance state, contradiction state, retention class, and evaluation state. Source execution, runtime, and profile identity remain on the canonical `AiLearningCandidate` lifecycle object.

Payloads are cloned at construction so later caller mutation does not alter a candidate's captured proposal.

## Authority boundary

Model-generated output may be used to propose a candidate, but it cannot make Knowledge authoritative, publish a Skill, or bypass the candidate lifecycle. Authoritative Knowledge and Skills remain published resources outside candidate payloads.

## Scope of Slice 7

Included: deterministic policy contracts, typed candidate payload contracts, validation, provenance/source identity preservation, and matching unit/Example verification.

Deferred: candidate persistence, extraction pipelines, promotion orchestration, Learning Review UI, authoritative version creation, and broader context integration.
