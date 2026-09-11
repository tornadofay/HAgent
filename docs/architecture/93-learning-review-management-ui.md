# Learning Review Management UI

## Purpose

Slice 12 begins the WinForms management surface for the governed learning layer without creating a second learning lifecycle or moving authorization into UI prompts.

## Configuration boundary

`AISettingsForm` remains a composition shell. The `Learning Review` page lives under `UI/Configuration/Learning/` and owns candidate listing and review actions.

The page uses:

```text
ConfigurationContext
    -> durable learning candidate store
    -> host-supplied reviewer identity
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

Reviewer identity is host-owned. The UI displays the supplied `AgentIdentityContext` as read-only metadata; it does not accept identity values from editable controls. When a caller does not supply an identity, the WinForms reference composition uses system-admin user ID `1` as its default reviewer identity. Tenant and workspace are optional identity scopes and are displayed as `Not supplied` when absent.

The identity is not inferred from prompt text and is not treated as authenticated merely because it exists; host authentication remains outside HAgent.

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

The current reference WinForms composition receives the durable `FileLearningCandidateStore` from the host when one is available. The Example now creates the store from the same configured effective storage root used by its other file-backed state and injects that store into `AISettingsForm`, preventing the management page from reading a different hardcoded candidate file.

Database-backed candidate storage can replace this adapter dependency later without changing the review page contract.

## Manual integration verification

The Example contains two explicit management tests:

1. `Learning Review Seed` creates a real durable `PendingReview` candidate and ensures deterministic Example policy rules authorize both Approve and Reject.
2. The user opens `Configuration → Learning Review`, selects that candidate, and explicitly Approves or Rejects it.
3. `Learning Review Verify` opens a fresh candidate-store instance and verifies the terminal status, lifecycle revision `2`, persisted reviewer identity evidence, and `Allow` policy evidence.

This tests the complete UI-to-governance-to-persistence path rather than merely checking that the controls render.

## Explicit scope of this increment

This increment establishes Learning Review management and the configuration navigation boundary. Full Knowledge/Wiki and Skill CRUD editors, effective agent resource inventory editing, and additional management areas remain subsequent Slice 12 work; they must continue to use the same configuration-page and provider-neutral resource contracts.
