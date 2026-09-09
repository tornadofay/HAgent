# Learning Mode

`AiLearningMode` is the provider-neutral control for whether HAgent creates learning candidates and whether promotion may occur without human review.

## Modes

```text
Disabled
SuggestOnly
AutomaticWithPolicy
FullyAutomatic
```

`Disabled` creates no learning candidates through the learning subsystem.

`SuggestOnly` permits candidate creation but requires review before promotion.

`AutomaticWithPolicy` permits policy-driven promotion but does not permit unreviewed promotion merely because the mode is enabled.

`FullyAutomatic` permits policy-driven promotion and is the only mode that explicitly permits unreviewed promotion.

## Separation from capability state

Learning Mode is not a resource capability. An agent may have Memory, Knowledge, or Skills enabled while Learning Mode is Disabled, or may have Learning Mode enabled while a particular resource capability remains disabled.

Capability enforcement continues to use `AiResourceCapabilityPolicy` and `AiResourceCapabilitySnapshot`.

## Profile/runtime/snapshot

`AiAgent.LearningMode` is persistent profile state and defaults to `Disabled`.

`AgentRuntimeOverrides.LearningMode` is nullable runtime-only state. `null` means inherit the profile value. A supplied value overrides only the execution snapshot and never mutates the persistent profile.

`AgentExecutionSnapshot.LearningMode` is the immutable effective value captured for one execution.

## Governance semantics

`AiLearningModePolicy` provides deterministic interpretation helpers:

- whether candidates may be produced;
- whether human review is required;
- whether policy-driven promotion is permitted;
- whether unreviewed promotion is permitted.

Invalid enum values are rejected. The learning mode itself never grants authorization; candidate promotion continues through the existing policy/authorization boundary.

## Persistence

File and in-memory agent persistence use the canonical `AiAgent` representation. SQL Server persists `LearningMode` as an explicit bounded column with a versioned migration. MySQL storage must retain the same explicit profile field and migration behavior in its bootstrap path.

No second learning configuration model is introduced.
