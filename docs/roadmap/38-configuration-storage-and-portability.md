# Phase 0.96.x — Configuration, Storage, and Portability Evolution

## Status

**Required cross-cutting work for Phase 0.96 capability-aware execution and the later 0.97 persistent cognitive runtime.**

## Goal

Evolve HAgent persistence so the new provider/model selection architecture, capability-aware execution, agent policies, resource relationships, global settings, and configuration portability can be stored consistently across the File, SQL Server, and MySQL backends.

The storage design should remain deliberately simple. HAgent configuration is HAgent-owned data. Provider API keys are persisted with provider configuration and encrypted at rest; there is no separate secret-reference, vault, or centralized secret-provider architecture.

The same database-backed configuration can be consumed by multiple HAgent processes/machines, allowing a network deployment to share providers, models, agents, skills, knowledge, policies, and credentials without configuring each client independently.

## Storage architecture direction

```text
HAgent Configuration
├── General/system settings
├── Providers
│   ├── connection configuration
│   └── encrypted API key
├── Models
├── Concrete execution targets
├── Discovery metadata/evidence
├── Capability state
├── Constraints
├── Quota/rate/capacity state
├── Agents
│   ├── selection mode
│   ├── requirements/preferences
│   └── fallback/cost policy
├── Skills
├── Knowledge / Wiki
├── Memory configuration/policy
├── Learning configuration
├── Tools
├── Permissions
└── resource relationships

Configuration Portability
├── versioned export package
├── import/compatibility validation
├── explicit conflict handling
└── optional encrypted credential bundle
```

## Provider credentials

1. [ ] Replace the current conceptual requirement that provider credentials live only in a separate secret store with direct provider configuration persistence.
2. [ ] Add an `ApiKey`-style provider credential field to the authoritative provider configuration contract where the provider uses an API key.
3. [ ] Encrypt provider API keys at rest before writing them to File, SQL Server, or MySQL persistence.
4. [ ] Keep the encryption mechanism simple, documented, deterministic for the supported deployment model, and independent of provider-specific logic.
5. [ ] Ensure decrypted credentials are available to provider adapters only when constructing provider execution requests.
6. [ ] Redact provider credentials from diagnostics, logs, audits, discovery evidence, planner assessments, exceptions, and UI diagnostic output.
7. [ ] Support credential replacement/removal so revoking a provider credential only requires updating/removing the persisted configuration and refreshing active snapshots.
8. [ ] Remove the requirement for `SecretReference`-based provider persistence from the new architecture.
9. [ ] Retire or simplify `ISecretStore` usage as part of implementation; it must not remain an unnecessary parallel source of truth for provider credentials.
10. [ ] Preserve runtime-only handling of storage-server connection passwords where appropriate; do not place database connection passwords into ordinary provider configuration records.

## Global configuration persistence

11. [ ] Persist the system-wide `General` configuration described by the architecture, including at minimum Cost Policy, default AI selection mode, default fallback policy, default Learning Mode, and discovery/refresh defaults.
12. [ ] Support explicit inherit/override semantics for settings that may be overridden at Agent or runtime/host scope.
13. [ ] Persist effective policy inputs without mutating global defaults when an Agent or runtime override is applied.
14. [ ] Version configuration records so cache/snapshot invalidation can detect changes reliably.

## Provider and model persistence

15. [ ] Redesign the provider persistence model so Provider is independent from Model and concrete Execution Target.
16. [ ] Remove obsolete permanent Agent `ProviderId`/`ProviderIds` and model-binding storage from the new design rather than preserving legacy fields unnecessarily.
17. [ ] Persist normalized logical-model records where logical identity can be established.
18. [ ] Persist provider-native model identifiers separately from logical-model identity.
19. [ ] Persist concrete execution targets with provider, endpoint/account/project, deployment/model identity, version/revision where available, and routing/deployment identity.
20. [ ] Persist execution-target commercial state: `Free`, `FreeWithinQuota`, `Paid`, or `Unknown`.
21. [ ] Persist discovery metadata including verification time, source/provenance, confidence where applicable, and refresh/expiration information.
22. [ ] Persist capability evidence and normalized tri-state capability state: `Supported`, `Unsupported`, `Unknown`.
23. [ ] Persist normalized request/target constraints such as context limits, output limits, modality restrictions, schema limitations, and provider-specific values through extensible metadata where needed.
24. [ ] Persist operational state separately from capability: availability/health, quota, rate, concurrency/capacity, reset information, and observed remaining capacity.
25. [ ] Distinguish configured/manual overrides from provider-discovered/observed values so refresh does not silently erase administrator intent.
26. [ ] Permit unknown discovery data without requiring fake defaults. Unknown must remain a valid persisted state.
27. [ ] Preserve multiple execution targets for the same logical model across different providers/accounts/projects/endpoints.

## Agent policy persistence

28. [ ] Persist Agent AI selection mode: `Auto`, `Preferred`, or `Fixed`.
29. [ ] Persist preferred provider/model/execution-target settings without treating them as permanent execution bindings.
30. [ ] Persist fixed execution-target selection when the administrator intentionally chooses Fixed mode.
31. [ ] Persist capability requirements and preferences, including required/preferred/optional/forbidden semantics.
32. [ ] Persist fallback/degradation policy.
33. [ ] Persist Agent cost-policy inheritance/override and effective policy inputs.
34. [ ] Persist runtime tri-state capability overrides separately from the reusable Agent profile.
35. [ ] Ensure execution snapshots contain resolved configuration versions so changes after execution start cannot alter active work.

## Resource and relationship persistence

36. [ ] Extend persistence for Skills, Knowledge/Wiki, Memory policy, Learning configuration, Tools, Permissions, and their Agent/runtime relationships.
37. [ ] Support reusable Skill definitions and versions without embedding executable handlers in persistence.
38. [ ] Support Knowledge/Wiki resources independently from Skill storage while allowing explicit Agent access relationships.
39. [ ] Support extensible resource/type identity so future resource categories can be stored and inventoried without hard-coded Agent columns.
40. [ ] Persist Agent/resource relationships with explicit scope and enabled/disabled state where required.
41. [ ] Preserve Learning candidate provenance, source execution/runtime identity, target scope, and evidence/confidence.

## Runtime and cache coordination

42. [ ] Add change/version metadata sufficient for long-lived runtime configuration snapshots.
43. [ ] Support cache invalidation when provider configuration, model/discovery metadata, capabilities, permissions, global settings, or Agent configuration changes.
44. [ ] Avoid reloading unchanged Agent/provider configuration from persistence on every execution when a valid runtime snapshot exists.
45. [ ] Ensure database-backed HAgent instances can safely observe shared configuration changes across processes/machines.
46. [ ] Define a lightweight refresh/invalidation strategy appropriate for File, SQL Server, and MySQL without requiring a distributed cache service.
47. [ ] Prevent stale configuration snapshots from being used indefinitely after a relevant configuration revision changes.

## Configuration export/import

48. [ ] Define a versioned HAgent configuration package format independent of the physical storage backend.
49. [ ] Export all HAgent-owned configuration that can be recreated on another deployment, including General settings, Providers, Models, execution targets, Agents, Skills, Knowledge/Wiki, Memory configuration/policy, Learning configuration, Tools, Permissions, capability/resource relationships, and relevant metadata.
50. [ ] Exclude executable tool handlers, live runtime objects, active executions, synchronization primitives, transient provider sessions, raw HTTP state, and other process-local state from portable configuration.
51. [ ] Support normal export without provider credentials by default.
52. [ ] Support explicit credential-bearing export for administrators who choose to move credentials with the configuration.
53. [ ] Encrypt included API keys inside a credential-bearing export package.
54. [ ] Protect credential-bearing exports with a basic user-supplied password/encryption mechanism; do not introduce a separate secret-vault architecture.
55. [ ] Validate package format/version compatibility before import.
56. [ ] Provide explicit conflict behavior for existing IDs, names, providers, models, skills, knowledge resources, and other imported objects.
57. [ ] Ensure import restores credentials into the normal encrypted-at-rest provider configuration of the selected storage backend.
58. [ ] Ensure export/import preserves authoritative IDs and relationships when possible while providing deterministic remapping when conflicts require new IDs.
59. [ ] Support round-trip export/import verification with the File, SQL Server, and MySQL backends.

## Multi-machine database deployment

60. [ ] Treat SQL Server and MySQL configuration storage as centrally shared HAgent configuration for all authorized HAgent processes connected to that database.
61. [ ] Ensure provider API keys stored in the shared database are usable by authorized execution processes after decryption.
62. [ ] Do not require each machine to maintain a separate provider API-key copy when using shared database-backed HAgent configuration.
63. [ ] Ensure configuration refresh/version checks prevent one machine from continuing to use a revoked or replaced provider credential indefinitely.
64. [ ] Preserve HAgent database isolation: shared HAgent storage remains an HAgent-owned database and must not become a gateway into the host application's business database.

## File, SQL Server, and MySQL parity

65. [ ] Define one logical configuration/storage contract and maintain equivalent behavior across File, SQL Server, and MySQL implementations.
66. [ ] Add ordered schema migrations for SQL Server and MySQL covering the redesigned provider/agent/model configuration and new resource/policy records.
67. [ ] Keep provider-specific SQL differences isolated to storage implementation/migrations; HAgent.Core remains provider-neutral.
68. [ ] Add File persistence equivalents for the same authoritative configuration concepts so File mode does not become a second architecture.
69. [ ] Ensure the selected backend can persist the configuration required by Phase 0.96 and Phase 0.97 without depending on a host business database.

## UI implications

70. [ ] Update `Providers` UI to edit connection information and API key while exposing encryption/redaction behavior without exposing implementation details.
71. [ ] Update `Models` UI to display persisted/discovered model and execution-target metadata, capability evidence, limits, availability, cost state, and verification state.
72. [ ] Update `Agents` UI to edit the new selection policy instead of obsolete permanent ProviderId/Model fields.
73. [ ] Add configuration export/import management UI, including package type, credential-inclusion choice, password/protection flow, compatibility validation, conflict preview, and import result summary.
74. [ ] Make it clear in the UI that credential-bearing export is an explicit action and normal export does not include API keys.
75. [ ] Keep the user-facing UI organized around General, Providers, Models, Agents, Tools, Permissions, Storage, and related resource-management surfaces rather than exposing storage internals.

## Migration strategy

Because HAgent is still in active build/test and legacy configuration does not require preservation, this evolution should favor direct model replacement over a large backward-compatibility layer.

76. [ ] Remove obsolete Agent provider/model fields from the authoritative model and schema.
77. [ ] Remove obsolete provider-secret-reference assumptions from the new provider persistence path.
78. [ ] Add new schema versions/migrations as needed for the redesigned model without introducing compatibility tables solely for retired fields.
79. [ ] Update File, SQL Server, and MySQL serialization/persistence together so the backends remain behaviorally aligned.
80. [ ] Update Example verification and management UI against the new storage contracts before marking the architecture transition complete.

## Verification

81. [ ] File, SQL Server, and MySQL can persist and reload the same logical configuration model.
82. [ ] Two independent HAgent processes using one database observe the same provider, model, agent, skill, knowledge, and General configuration.
83. [ ] A stored API key is encrypted at rest and is not emitted by diagnostics/audit/logging paths.
84. [ ] Updating/removing a provider API key is reflected after configuration snapshot refresh/invalidation.
85. [ ] Same logical model with different provider/account cost, capability, quota, and health state remains represented as distinct execution targets.
86. [ ] Auto, Preferred, and Fixed Agent selection policies round-trip correctly through persistence.
87. [ ] General Cost Policy and Learning defaults round-trip correctly and preserve inherit/override semantics.
88. [ ] Export without credentials contains no API keys.
89. [ ] Credential-bearing export contains encrypted credentials and requires the export protection mechanism to import them.
90. [ ] Export/import round-trips providers, models, execution targets, agents, skills, knowledge/wiki, memory policy, learning configuration, tools, permissions, and relationships.
91. [ ] Import detects incompatible package versions and reports deterministic conflicts rather than silently overwriting unrelated configuration.
92. [ ] Running executions use immutable snapshots even when another process edits/deletes the underlying configuration.

## Architectural outcome

After this evolution, HAgent storage should conceptually look like:

```text
                 HAgent Configuration
                         │
          ┌──────────────┴──────────────┐
          │                             │
      File backend                Database backend
                                      │
                               SQL Server / MySQL
                                      │
                          shared by authorized HAgent
                              processes/machines

Provider
  ├── connection metadata
  └── encrypted API key

Model
  ├── logical identity
  └── provider-native identities

Execution Target
  ├── provider/account/project/endpoint
  ├── model/deployment
  ├── capabilities/evidence
  ├── constraints
  ├── quota/rate/capacity
  ├── health/availability
  └── cost state

Agent
  ├── selection policy
  ├── requirements/preferences
  ├── fallback
  ├── cost policy
  └── resource relationships

Export / Import
  └── versioned portable representation of the same authoritative configuration
```

The storage layer remains an implementation boundary. Provider routing, cognitive planning, and execution behavior consume normalized contracts rather than knowing whether the source was a JSON file, SQL Server, or MySQL.