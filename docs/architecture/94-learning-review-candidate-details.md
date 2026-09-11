# Learning Review Candidate Details Workspace

## Purpose

This increment extends the Slice 12 Learning Review management page from a PendingReview-only list into a filterable inspection workspace. It does not create a second learning lifecycle and does not move authorization into the UI.

## Learning candidate lifecycle

The learning lifecycle distinguishes discovering a candidate from requiring review and from making the learned resource authoritative:

```text
Proposed
   │
   │ lifecycle / policy evaluation determines review requirement
   ▼
PendingReview
   │
   ├──────────────► Rejected
   │
   ▼
Approved
   │
   │ governed authoritative promotion
   ▼
Promoted
```

`Proposed` means a learning candidate has been formed but has not necessarily entered a human-review state. `PendingReview` means the learning lifecycle/policy has determined that the candidate must pass the review boundary before it can be accepted. `Approved` means the candidate has passed the review boundary; it is not yet authoritative. `Promoted` means the approved candidate has been successfully converted into its authoritative resource through the separate promotion operation.

The exact route is governed by learning mode and policy. Suggest-only learning requires review; policy-driven and fully automatic modes can permit policy-driven promotion, while fully automatic mode may additionally permit unreviewed promotion according to policy. These lifecycle rules are not inferred from the UI.

## Promotion boundary

Approval and promotion are deliberately separate:

```text
candidate formation
      ↓
   Proposed
      ↓
 PendingReview
      ↓
  human/policy review
      ↓
   Approved
      ↓
 authoritative promotion
      ↓
   Promoted
```

Promotion is already a provider-neutral HAgent capability established in the earlier learning-promotion slice. It validates the candidate's approved state, evaluates fresh promotion authorization, publishes the authoritative resource before changing candidate lifecycle state, and preserves version safety for immutable resource families.

Resource outcome depends on candidate type:

- Memory candidate → authoritative Memory.
- Knowledge candidate → a new authoritative Knowledge version; existing published versions are not mutated in place.
- Skill candidate → a new immutable authoritative Skill version; existing published versions are not mutated in place.

Rejected candidates do not become authoritative resources. Candidate history and review evidence remain durable according to the candidate store's retention policy.

## UI shape

The Learning Review page now uses:

```text
Learning Review
  ├─ host-supplied reviewer identity (read-only)
  ├─ Approve / Reject / Refresh
  ├─ Status filter: All / Proposed / PendingReview / Approved / Rejected / Promoted
  ├─ Candidate type filter: All / Memory / Knowledge / Skill
  └─ workspace
       ├─ bounded candidate list
       └─ selected candidate details
            ├─ Overview
            ├─ Candidate Content
            └─ Review Evidence
```

Status is a filter, not a separate sub-page. This allows future lifecycle states to be added without duplicating navigation or review logic.

## Candidate list

The list is populated from `IAiLearningCandidateStore.QueryAsync` using the existing provider-neutral query contract. It shows bounded operational metadata:

- candidate type;
- candidate identity;
- lifecycle status;
- proposed scope;
- source agent profile;
- confidence;
- lifecycle revision;
- last update time.

Expired records remain excluded from the management view.

## Candidate details

The selected record is displayed without changing its persisted representation. Existing durable fields are surfaced as read-only UI data:

- candidate type, status, scope, source agent, revision, confidence;
- evidence, provenance, contradiction, retention, and evaluation state;
- creation, update, and expiry timestamps;
- admission policy identity/version/rule;
- promotion-authorization outcome;
- serialized provider-neutral candidate payload rendered as readable JSON;
- source execution, runtime instance, and agent profile;
- last review action and timestamp;
- reviewer identity evidence;
- review policy version/rule/outcome/reason.

The page does not expose provider credentials or create a second copy of the candidate payload.

## Review boundary

Approve and Reject remain disabled unless the selected candidate is `PendingReview`. Actions continue through `AiLearningCandidateReviewService`, which re-evaluates the unified `learning.review` policy, records reviewer identity and policy evidence, and persists through the existing optimistic revision boundary.

The details workspace is observational. It does not directly publish Memory, Knowledge, or Skill resources. Authoritative promotion remains the separate governed operation established by the authoritative-promotion slice.

## Filtering boundary

The current UI exposes lifecycle status and candidate type because both dimensions already exist in the provider-neutral `AiLearningCandidateQuery` contract. No ad-hoc filtering is performed against serialized payload JSON.

Additional query dimensions can be added later at the store/query contract level when there is a real cross-provider requirement.

## Future management work

The next natural management increment is authoritative promotion of an approved candidate from the same details workspace. That action must call the existing promotion service rather than mutate candidate state or publish resources directly from the UI.

This slice does not add editing of learned payloads, authoritative Skill/Wiki/Memory CRUD, resource replacement, or publication actions. Those remain separate management slices so inspection does not silently become mutation or publication.