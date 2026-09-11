# Learning Review Candidate Details Workspace

## Purpose

This Slice 12 increment extends the Learning Review management page from a PendingReview-only list into a filterable inspection and governed promotion workspace. It does not create a second learning lifecycle and does not move authorization or publication logic into the UI.

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

Promotion is a provider-neutral HAgent capability established in the authoritative-promotion slice. It validates the candidate's approved state, evaluates fresh `learning.promote` authorization, publishes the authoritative resource before changing candidate lifecycle state, and preserves optimistic revision/version safety.

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
  ├─ Approve / Reject / Promote / Refresh
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

`Approve` and `Reject` are enabled only for `PendingReview`. `Promote` is enabled only for `Approved` candidates and only when the host injects an `AiLearningPromotionService`.

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

Approve and Reject continue through `AiLearningCandidateReviewService`, which re-evaluates the unified `learning.review` policy, records reviewer identity and policy evidence, and persists through the existing optimistic revision boundary.

## Promotion boundary in the UI

The Promote action is intentionally thin. The page calls the host-supplied `AiLearningPromotionService`; it does not construct publication targets or mutate candidate lifecycle state itself.

The service performs the existing authoritative workflow:

1. reopen and validate the candidate;
2. require `Approved` and non-expired state;
3. evaluate fresh `learning.promote` authorization using the supplied reviewer identity;
4. publish Memory, Knowledge, or Skill through the configured provider-neutral promotion target;
5. only after publication succeeds, transition the candidate to `Promoted` and persist using the expected lifecycle revision.

A successful UI promotion therefore changes the durable candidate revision from `2` to `3` and refreshes the page so the candidate appears under the `Promoted` filter.

If the host does not inject a promotion service, the Promote action remains disabled. This allows minimal hosts to consume Learning Review without accidentally gaining publication side effects.

## Example integration

`HAgent.Example` now injects an `AiLearningPromotionService` into the configuration surface. Its deterministic Skill promotion target is the same provider-neutral target contract used by the core promotion Example; no model or network request is required.

The management workflow is:

```text
Learning Review Seed
      ↓
PendingReview / revision 1
      ↓
Configuration → Learning Review
      ↓
Approve
      ↓
Approved / revision 2
      ↓
Promote
      ↓
Promoted / revision 3
      ↓
Learning Review Verify
```

The Example seeds the required `learning.review` Approve/Reject rules and `learning.promote` rule for its deterministic Skill candidate. Verify accepts the reviewed revision-2 result or the revision-3 Promoted result.

## Filtering boundary

The current UI exposes lifecycle status and candidate type because both dimensions already exist in the provider-neutral `AiLearningCandidateQuery` contract. No ad-hoc filtering is performed against serialized payload JSON.

Additional query dimensions can be added later at the store/query contract level when there is a real cross-provider requirement.

## Future management work

The next management work remains authoritative Memory/Knowledge/Skill inventory and CRUD surfaces, editing workflows where appropriate, and later reliability/adaptation management. This slice does not add in-place editing of learned payloads or resource replacement.
