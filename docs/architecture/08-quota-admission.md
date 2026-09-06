# Quota and Rate Admission

## Purpose

HAgent treats quota/rate limits as **proactive admission control** before provider transport. Capability answers whether a target can perform an operation; admission answers whether the target can accept the operation now under its configured budget.

## Normalized dimensions

`AiQuotaDimension` supports provider-neutral accounting units including:

- request count;
- input/output/total tokens;
- concurrency;
- audio duration;
- image count;
- bytes;
- spend in provider-neutral minor units.

The enum can be extended as new provider dimensions are needed without introducing provider-specific types into HAgent.Core.

## Rolling-window limits

`AiQuotaLimit` describes a maximum amount over an arbitrary positive time window. The current implementation maintains process-local target-scoped rolling-window state.

```text
Execution request
      |
      v
Target + quota policy + estimated usage
      |
      v
Atomic reservation
   /          \
admitted     wait required
      |
      v
provider transport
      |
      v
actual usage reconciliation
```

## Atomic reservations

Reservation and admission occur under the same target-scoped synchronization boundary. Therefore concurrent callers cannot both observe the same remaining capacity and over-reserve it.

A reservation can be:

- `Commit(actualUsage)` to replace the estimate with actual observed consumption;
- `Release()` when the reservation does not reach provider transport.

This keeps quota state distinct from provider retries and transport implementation.

## Wait semantics

When a request would exceed a rolling-window limit, admission returns `WaitRequired` with the computed earliest safe time after the currently blocking window entries expire. If several limits block the request, the latest blocking time is returned.

Higher-level runtime policy will later decide whether to `Wait`, `TryNextCandidate`, or `Fail` based on the execution selection/fallback policy and maximum admission wait.

## Scope evolution

The current in-memory controller is intentionally **target-scoped**. The model already includes a scope field on limits so future storage-backed admission can represent provider-enforced scopes such as account, organization, project, endpoint, or deployment without redesigning the public quota dimension model.

Shared/multi-process admission is a later runtime/storage responsibility; local in-memory state must not be mistaken for globally authoritative provider quota.
