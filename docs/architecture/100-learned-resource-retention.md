# Learned Resource Retention and Archival

## Purpose

Phase 0.9576 Slice 4 adds a provider-neutral retention boundary for already-promoted learned resources. Retention decides whether a resource should remain active, be archived, be retired, or be restored from archive. It does not mutate the authoritative resource definition or bypass learning promotion governance.

## Signals

`AiLearnedResourceRetentionRequest` carries bounded host-provided signals:

- utility score;
- validated use count;
- repeated low-utility assessment count;
- last-use time and freshness window;
- supersession and contradiction signals;
- explicit retention or retirement request;
- authority rank and known competing authority rank;
- bounded evidence and policy identity.

Retention is evidence/state management. It is not authorization and it does not decide whether a resource is applicable to the current execution context.

## Deterministic eligibility

The reference service applies bounded deterministic rules:

- explicit retention keeps a non-archived resource in its current state;
- explicit retirement requests produce terminal `Retired` state when policy permits and authority validation passes;
- superseded, contradicted, stale, or persistently low-utility resources become eligible for `Archived` state;
- a higher-authority resource is preserved when a lower-utility competing resource is known;
- repeated low utility requires at least three assessments at or below the bounded `0.25` threshold before utility alone triggers archival.

These rules identify retention eligibility. Policy still controls the state mutation.

## Lifecycle states

Slice 3 provides `Active`, `UnderReview`, `Quarantined`, and `Retired`. Slice 4 adds `Archived` as a recoverable retention state.

`Archived` is intentionally not automatically usable. It remains outside normal automatic-use eligibility until an explicit governed retention recovery returns it to `Active`.

`Retired` remains terminal for the current architecture. Revalidation does not revive it.

## Recovery

An archived resource may be restored through `AssessRetentionAsync` when the host explicitly requests retention, the resource is not superseded or contradicted, and policy allows the transition.

Recovery changes lifecycle metadata only. The resource type, ID, published version, scope, and authoritative content remain unchanged.

## Authority protection

Retention must not remove a higher-authority resource merely because a lower-utility duplicate or competitor exists. The request carries an explicit bounded authority rank and highest known competing authority rank. A resource at or above the known competing authority rank is preserved by the reference retention policy unless a direct governed retirement boundary applies.

Authority signals are evidence inputs. The canonical resource ownership/identity model remains authoritative.

## Policy and revision safety

All mutating retention decisions use the existing `IAiPolicyEngine` through the operation `resource.lifecycle.retention`.

Lifecycle persistence remains compare-and-swap revisioned through `IAiLearnedResourceLifecycleStore`. A stale retention write is rejected instead of overwriting a newer lifecycle state.

Retention assessments update bounded lifecycle metadata and transition history; they do not publish or rewrite Skills, Knowledge, Memory, or other learned-resource definitions.

## Provenance

Each retention mutation records:

- decision;
- utility score;
- assessment timestamp/count;
- bounded reason;
- lifecycle transition history;
- policy rule/version through the existing lifecycle event boundary.

The original promoted resource version remains available as the identity being governed.

## Tests and Example

Focused tests:

`tests/HAgent.Tests/LearnedResourceRetentionTests.cs`

The focused suite verifies archival eligibility, identity preservation, higher-authority protection, archive recovery, policy denial, and cancellation boundaries.

Matching Example:

`src/HAgent.Example/MainForm.LearnedResourceRetention.cs`

Registered through the existing Learning registration path and classified by `MainForm.ExampleOrganization`.

Example title:

`LEARNED RESOURCE RETENTION`
