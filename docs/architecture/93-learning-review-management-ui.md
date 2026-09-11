# Learning Review Management UI

## Purpose

Slice 12 begins the WinForms management surface for the governed learning layer without creating a second learning lifecycle or moving authorization into UI prompts.

## Configuration boundary

`AISettingsForm` remains a composition shell. The `Learning Review` page lives under `UI/Configuration/Learning/` and owns candidate listing and review actions.

The page uses:

```text
ConfigurationContext
    -> durable learning candidate store
    -> existing AI store/policy set
        ↓
LearningReviewPage
        ↓
AiLearningCandidateReviewService
        ↓
IAiPolicyEngine
```

The page does not directly mutate candidates. Approve/Reject operations go through the existing review service, which re-evaluates `learning.review`, records reviewer identity/policy evidence, and uses the existing optimistic revision boundary.

## Reviewer identity

The UI requires an explicit reviewer user identifier before a review action. Optional tenant and workspace values are captured as structured `AgentIdentityContext` data. The identity is not inferred from prompt text and is not treated as authenticated merely because the user entered a value; host authentication remains outside HAgent.

## Candidate visibility

The initial page shows only non-expired `PendingReview` candidates and exposes bounded metadata:

```text
candidate type
candidate identity
proposed scope
source agent profile
confidence
lifecycle revision
last update time
```

Payload content, credentials, and raw provider data are not copied into the list projection.

## Review semantics

Approve and Reject are enforced through `AiLearningCandidateReviewService`. A denied policy decision leaves the candidate unchanged. Successful review advances the candidate lifecycle and persists reviewer/policy evidence through the existing candidate store.

The UI does not publish Memory, Knowledge, or Skill resources. Authoritative promotion remains Slice 10's separate operation.

## Storage boundary

The current reference WinForms configuration composition exposes the existing durable `FileLearningCandidateStore` through `ConfigurationContext`. This is intentionally a storage adapter dependency rather than a second learning repository model. Database-backed candidate storage can replace that dependency later without changing the review page contract.

## Explicit scope of this increment

This increment establishes Learning Review management and the configuration navigation boundary. Full Knowledge/Wiki and Skill CRUD editors, effective agent resource inventory editing, and additional management areas remain subsequent Slice 12 work; they must continue to use the same configuration-page and provider-neutral resource contracts.
