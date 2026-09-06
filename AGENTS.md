# HAgent engineering rules

This repository is designed to be worked on by human developers and coding agents.

## Development mode: redesign and rebuild

HAgent is currently in **redesign/rebuild mode**. Architectural correctness and a clean target design take priority over backward compatibility with obsolete code, APIs, storage shapes, or internal mechanisms.

1. During redesign/rebuild mode, existing code is **not** presumed worth preserving.
2. Remove, rename, reshape, or replace obsolete mechanisms when the new architecture requires it. Do not add compatibility wrappers, adapters, aliases, migration shims, duplicate models, or parallel mechanisms merely to avoid changing existing code.
3. Prefer one coherent canonical architecture over transitional dual paths.
4. Existing tests, Example code, storage schemas, and public APIs are evidence of current state, not constraints on the target architecture.
5. When a structural change makes old code invalid, update dependent code and tests to the new design rather than preserving the old behavior.
6. The user will explicitly declare when HAgent enters **code-lock/stabilization mode**. Until then, redesign-first changes are authorized.
7. Once code-lock/stabilization mode is explicitly declared, compatibility, migration, deprecation, and regression-preservation requirements may be introduced deliberately.

## Architecture invariants

8. Keep `HAgent.Core` dependency-light and provider-neutral.
9. Never put WinForms types in Core.
10. Never put SQL Server/MySQL implementation details in Core.
11. Provider-specific transport belongs in provider adapter assemblies.
12. Provider credentials may be part of persisted provider configuration, but must be encrypted at rest and redacted from diagnostics. Do not introduce a separate secret-reference/vault subsystem unless a future requirement explicitly demands it.
13. A provider describes connection/transport concerns. An agent profile describes reusable behavior/configuration.
14. Keep agent profile identity separate from runtime agent instance identity.
15. Runtime scope is a binding concept, not a separate agent class.
16. Public network/database APIs must support cancellation and remain async.
17. Preserve .NET Framework 4.8.1 compatibility where currently targeted.
18. Avoid framework-sized dependencies when a focused adapter is sufficient.
19. Active execution must use snapshots so configuration edits/deletion cannot corrupt running work.
20. Core provider routing must not assume OpenAI-specific semantics.
21. Memory must remain viable without GPU, vector database, or a large resident RAM index.
22. Capability support must be explicit and may be `Supported`, `Unsupported`, or `Unknown`, with evidence/provenance where practical.
23. Tool execution is a host capability boundary. Models may request only registered tools; they never receive arbitrary reflection, process, file, database, control-tree, or memory access.
24. Permissions, authorization, approval, budgets, and cancellation are enforcement mechanisms, not prompt instructions.
25. Provider responses must remain provider-neutral while preserving structured output, tool calls/results, reasoning metadata when explicitly exposed, usage, and raw metadata as appropriate.
26. Streaming is optional; providers that do not stream must remain supported by Core contracts.
27. Observability must avoid secrets and sensitive payloads by default and use correlation IDs with configurable redaction.
28. Tool definitions and executable handlers are separate. Executable handlers are never serialized.
29. Initial tool taxonomy is `BuiltIn`, `Application`, `Declarative`, `UI`, `SqlServer`, and `MySql`. Extension tools are deferred.
30. System prompts are additive layers. A lower layer may add narrower instructions/restrictions but must not replace or erase higher layers. Prompt text is not an authorization boundary.
31. HAgent is the generic LLM cognition/execution layer for host software that needs LLM-driven behavior; Core must not become tied to one project type.
32. Host domain state, lifecycle, scheduling policy, persistence, authorization, and side effects remain outside HAgent unless exposed through a generic host-owned contract.
33. The canonical execution boundary must support generic host input/context, host correlation, execution options, and optional structured-output requirements without requiring plain string message as the only model.
34. Host correlation identity must remain distinct from HAgent execution identity and runtime-instance identity and must not be embedded into prompt text.
35. Structured output must be a real request/validation contract. Valid JSON text alone is not proof that a structured-output schema was honored.
36. Execution terminal-state transitions must be protected against late provider completion after cancellation, timeout, retirement, shutdown, or another terminal outcome.
37. Independent runtime instances must not share mutable runtime identity, override state, execution state, shutdown signaling, or private memory ownership.
38. Shared infrastructure such as stores, provider adapters, and tool registries may be reused across instances only through contracts that support concurrent use.
39. Skills are reusable versioned capability definitions, not private copies owned by every runtime instance.
40. Knowledge is a broader retrievable-information concept; Wiki is a managed knowledge source, not the universal representation for every future knowledge type.
41. Memory ownership and scope must be explicit. Working memory is execution-local; private long-term memory must remain isolated across independent runtime instances; shared memory requires explicit scope and authorization.
42. Learning must create typed candidates before promotion. Model-generated text must not directly mutate authoritative Wiki/knowledge or published Skills.
43. `LearningMode` governs learning behavior and is distinct from capability enable/disable policy.
44. Agent capability policy must support profile defaults plus runtime tri-state overrides (`Inherit`, `Enabled`, `Disabled`) for Skills, Knowledge/Wiki, Memory families/types, and future resource types.
45. Effective capability state must be resolved into an execution snapshot. Later profile/runtime edits must not alter an already-running execution.
46. Resource/type identity must be extensible so future knowledge types can be inventoried and shown without adding hard-coded Agent properties.
47. Learning promotion must preserve provenance, source execution/runtime identity, proposed scope, and evidence/confidence when available.
48. Configuration portability must serialize authoritative HAgent configuration rather than creating a second configuration model. Export/import must work across supported File, SQL Server, and MySQL storage backends.
49. Configuration packages must be versioned, compatibility-checked, and explicit about conflicts. Executable handlers, live runtimes, active executions, and transient process state are not portable configuration.
50. Credential-bearing exports are opt-in. Included provider API keys must remain encrypted inside the package and protected by the package's basic encryption/password mechanism.

## Identity and tenancy

51. Identity is provider-neutral host-supplied context, not authentication or application-domain state.
52. Distinguish deployment, tenant, principal, user, session, workspace, agent profile, runtime instance, and execution identities.
53. Tenancy is optional; single-tenant hosts must not need artificial tenant identities.
54. Resource scope is explicit and must not be inferred from a generic user ID. Canonical resource scopes are `Global`, `Tenant`, `User`, `Workspace`, `Agent`, `Runtime`, and `Execution`.
55. Resource ownership/partition keys are canonical architecture, not a compatibility layer. Subsystems must use the new ownership model directly when redesigned.
56. Identity context must propagate through execution, tools, audit, memory, knowledge, learning, policy, events, tracing, evaluation, runtime persistence, workspaces, and human approval as each subsystem is implemented.
57. Missing identity is represented as an empty context only where the subsystem explicitly permits anonymous/single-user operation; identity-required boundaries must fail closed.
58. Private resources must be isolated by explicit owner identity. Shared resources require explicit scope and authorization.

## Context rules

59. WinForms integration belongs in `HAgent.WinForms`, not Core.
60. The public WinForms concept is **UI Context / Control Adapters**, not generic form serialization.
61. UI adapters should prefer native/bound data sources and bounded projections over scraping visible control state.
62. `DataTable` is optional, not the mandatory data representation.
63. Application-owned objects may be attached as live runtime context and inspected through bounded, non-executable discovery.
64. Discovery describes evidence; it never grants authorization or invents business meaning.
65. Explicit developer semantics/authorization may override or enrich automatic discovery.
66. Generic context may represent observations, state snapshots, events, records, objects, resources, or other host information without HAgent assigning domain meaning.

## Multi-agent rules

67. A workspace is a communication context, not an instruction to broadcast every message.
68. Unaddressed user messages go only to the configured workspace default recipient.
69. Direct user messages and agent delegation target explicit runtime participants.
70. Visible agent-to-agent dialogue is a real workspace message stream when the host enables it.
71. Coordinator and specialist are roles over the same generic runtime agent model.
72. Specialists may represent a whole domain, table, subsystem, or capability; they are not inherently tied to one record.
73. Dynamically created runtime agents come from reusable profiles and do not become permanent configuration entries by default.
74. Runtime retirement is explicit or follows host shutdown/lifecycle policy.
75. Runtime persistence, when enabled, must distinguish host instance, user/session, workspace, profile ID, and runtime instance ID.
76. Private memory belongs to runtime ownership; shared memory requires explicit scope and authorization.

## External consumers

77. External hosts consume HAgent through provider-neutral public contracts and do not require host-specific dependencies in Core.
78. HAgent must not contain host-specific physics, rendering, simulation time, application state, domain actions, or other domain rules.
79. External hosts remain authoritative for their state and side effects. HAgent supplies generic agent execution, context, tools, memory integrations, coordination, structured output, and telemetry.

## WinForms UI conventions

80. Do not use `System.Windows.Forms.MessageBox` directly in `HAgent.WinForms`.
81. Use `HMessage.ShowDelete`, `ShowQuestion`, `ShowInformation`, `ShowError`, and `ShowException` for dialogs.
82. Use the shared HAgent `Header` for HAgent form chrome.
83. Use `HButton` for HAgent action buttons.
84. Preserve existing UI/layout work unless a task explicitly requests UI changes.
85. Knowledge/Skill/Learning management UI must use the shared HAgent conventions and must expose effective agent configuration, not only persisted profile references.

## Example and testing rules

86. `HAgent.Example` is the manual developer/verification host; it is not `HAgent.Tests`.
87. Every meaningful completed capability requires a matching Example verification using public APIs.
88. Keep Example code split across focused partial files/components.
89. Example snippets must be reproducible and explain required setup or shared setup.
90. Do not claim build/test success unless it was actually executed.
91. Network-provider automated tests must use fakes/local test infrastructure rather than a real vendor.

## Documentation rules

92. `README.md` is the public introduction and quick start.
93. `docs/architecture/` is the authoritative stable architecture description.
94. `docs/plan/` is implementation state: master direction, current state, and active implementation only.
95. `docs/roadmap/` is the ordered implementation path, including completed foundation history and future phases.
96. `docs/storage.md` contains storage-specific details.
97. Root `plan.md` and `roadmap.md` are generated; do not hand-edit them except to synchronize a generated view when automation has not yet run.
98. When implementation changes architecture or milestone state, update the authoritative source document in the same change.
99. Do not duplicate architectural decisions across multiple source documents when a referenced authoritative document can own the decision.
100. During redesign/rebuild mode, document the target architecture rather than explaining compatibility with obsolete mechanisms.
