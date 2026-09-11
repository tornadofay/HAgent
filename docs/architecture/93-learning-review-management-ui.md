# Learning Review Management UI

## Purpose

Slice 12 begins the WinForms management surface for the governed learning layer without creating a second learning lifecycle or moving authorization into UI prompts.

## Configuration boundary

`AISettingsForm` remains a composition shell. The `Learning Review` page lives under `UI/Configuration/Learning/` and owns candidate listing, filtering, inspection, and review actions.

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

## Candidate visibility and filtering

The management page provides one filterable candidate workspace rather than separate status sub-pages.

Filters currently include:

- lifecycle status: `All`, `Proposed`, `PendingReview`, `Approved`, `Rejected`, `Promoted`;
- candidate type: `All`, `Memory`, `Knowledge`, `Skill`.

The list shows bounded metadata and excludes expired records:

```text
candidate type
candidate identity
lifecycle status
proposed scope
source agent profile
confidence
lifecycle revision
last update time
```

Payload content, credentials, and raw provider data are not copied into the list projection.

## Candidate details

Selecting a candidate opens a read-only details workspace beside the list. Existing durable candidate fields are surfaced without changing the persistence contract, including:

- candidate type, status, scope, source agent, revision, and confidence;
- evidence, provenance, contradiction, retention, and evaluation state;
- created, updated, and expiry timestamps;
- admission policy identity/version/rule and promotion authorization;
- the provider-neutral payload rendered as readable JSON;
- source execution, runtime instance, and agent profile;
- last review action/timestamp and reviewer/policy evidence.

The details view is observational. It does not publish authoritative Memory, Knowledge, or Skill resources.

## Review semantics

Approve and Reject are enabled only for a selected `PendingReview` candidate and remain enforced by `AiLearningCandidateReviewService`. A denied policy decision leaves the candidate unchanged. Successful review advances the candidate lifecycle and persists reviewer/policy evidence through the existing candidate store.

## Promotion semantics

Promote is enabled only for an `Approved` candidate when the host injects `AiLearningPromotionService`. The UI calls the existing provider-neutral promotion capability and does not construct publication targets or mutate candidate lifecycle state itself.

The promotion service performs fresh `learning.promote` authorization, publishes the authoritative resource first, and only then advances the candidate from `Approved` revision `2` to `Promoted` revision `3` with optimistic revision protection.

The UI does not publish Memory, Knowledge, or Skill resources itself. Authoritative promotion remains the separate core capability established by Slice 10.

## Storage boundary

The current reference WinForms composition receives the durable `FileLearningCandidateStore` from the host when one is available. The Example creates the store from the same configured effective storage root used by its other file-backed state and injects that store into `AISettingsForm`, preventing the management page from reading a different hardcoded candidate file.

Database-backed candidate storage can replace this adapter dependency later without changing the review page contract.

## Manual integration verification

The Example contains two explicit management tests:

1. `Learning Review Seed` creates a real durable `PendingReview` candidate and ensures deterministic Example policy rules authorize both Approve and Reject.
2. The user opens `Configuration → Learning Review`, uses the status/type filters as needed, selects that candidate, inspects its details, Approves it, and uses `Promote` to exercise the existing authoritative promotion service.
3. `Learning Review Verify` opens a fresh candidate-store instance and verifies the terminal `Promoted` status, lifecycle revision `3`, persisted reviewer identity evidence, and `Allow` policy evidence.

The workflow was manually verified by the user on both .NET Framework 4.8.1 and .NET 9 on 2026-09-11.

This tests the complete UI-to-governance-to-persistence-to-authoritative-promotion path rather than merely checking that the controls render.

## Explicit scope of this increment

This increment establishes Learning Review management, filtering, candidate inspection, and governed promotion through the existing provider-neutral promotion capability. Full Knowledge/Wiki and Skill CRUD editors, effective agent resource inventory editing, editing of learned payloads, replacement workflows, and reliability/adaptation management remain subsequent management work and must continue to use the same configuration-page and provider-neutral resource contracts.
