# Capability-Aware Execution Planning

## Purpose

HAgent separates **agent intent** from **execution selection**. An agent describes what it needs and what it prefers; the execution planner selects a currently compatible concrete target without permanently binding the reusable agent profile to a provider or model.

## Canonical execution target

`AiExecutionTarget` identifies a concrete serving environment using provider, account/project, endpoint, model, logical model, version/revision, and deployment identity. The target also carries normalized capability knowledge, commercial state, and availability.

A logical model may therefore have multiple concrete targets:

```text
Logical model: shared-model
    |
    +-- provider-free / target-free
    +-- provider-paid / target-paid
    +-- provider-unknown / target-unknown
```

Those targets are not interchangeable. Their capabilities, cost, health, quota, capacity, and permissions can differ.

## Capability requirements

Requirements are independent of provider names and model strings. Each requirement has explicit strength:

- `Required`: must be `Supported`.
- `Preferred`: increases candidate score when supported.
- `Optional`: adds value when supported but does not constrain selection.
- `Forbidden`: rejects a candidate when the capability is known to be supported.

`Unknown` is never treated as `Supported` for a required capability.

## Selection policy

`AiExecutionSelectionPolicy` owns execution preference and fallback intent:

- `Auto`: choose the best compatible target.
- `Preferred`: prioritize a configured preferred target/provider/logical model but allow another compatible target.
- `Fixed`: evaluate only the named concrete target; all capability and policy enforcement still applies.

Cost policy is independent of technical capability:

- `FreeOnly`: only known free or free-within-quota targets qualify.
- `FreePreferred`: free targets are favored but other known states may remain eligible.
- `NoRestriction`: cost does not exclude a target.

Unknown cost never qualifies as free under `FreeOnly`.

## Planner boundary

`IExecutionPlanner` is provider-neutral and produces an `AiExecutionPlan` containing the selected target plus per-candidate diagnostics. Diagnostics explain acceptance/rejection and major score contributions so hosts and management UI can explain routing decisions.

```text
Agent requirements + execution policy
                 |
                 v
       IExecutionPlanner
                 |
        candidate evaluation
          /           \
      rejected       accepted
                         |
                         v
               concrete execution target
                         |
                         v
             ProviderExecutionRequest
```

The planner does not call provider transport. `ProviderExecutionRequest` remains the transport boundary.

## Current implementation slice

The first 0.96 implementation slice establishes:

1. concrete execution-target identity;
2. capability requirement semantics;
3. AI selection/fallback/cost policy;
4. deterministic candidate diagnostics;
5. deterministic Example verification across multiple providers exposing the same logical model.

Quota/rate admission, capability discovery, operational capacity, target health feedback, transport integration, and removal of the obsolete agent-level provider/model binding remain subsequent 0.96 slices.
