# HAgent engineering rules

This repository is designed to be worked on by human developers and coding agents.

## Architecture invariants

1. Keep `HAgent.Core` dependency-light and provider-neutral.
2. Never put WinForms types in Core.
3. Never put SQL Server/MySQL implementation details in Core.
4. Provider-specific transport belongs in provider adapter assemblies.
5. Provider credentials may be part of persisted provider configuration, but must be encrypted at rest and redacted from diagnostics. Do not introduce a separate secret-reference/vault subsystem unless a future requirement explicitly demands it.
6. A provider describes connection/transport concerns. An agent profile describes reusable behavior/configuration.
7. Keep agent profile identity separate from runtime agent instance identity.
8. Runtime scope is a binding concept, not a separate agent class.
9. Public network/database APIs must support cancellation and remain async.
10. Preserve .NET Framework 4.8.1 compatibility where currently targeted.
11. Avoid framework-sized dependencies when a focused adapter is sufficient.
12. Active execution must use snapshots so configuration edits/deletion cannot corrupt running work.
13. Core provider routing must not assume OpenAI-specific semantics.
14. Memory must remain viable without GPU, vector database, or a large resident RAM index.
15. Capability support must be explicit and may be `Supported`, `Unsupported`, or `Unknown`, with evidence/provenance where practical.
16. Tool execution is a host capability boundary. Models may request only registered tools; they never receive arbitrary reflection, process, file, database, control-tree, or memory access.
17. Permissions, authorization, approval, budgets, and cancellation are enforcement mechanisms, not prompt instructions.
18. Provider responses must remain provider-neutral while preserving structured output, tool calls/results, reasoning metadata when explicitly exposed, usage, and raw metadata as appropriate.
19. Streaming is optional; providers that do not stream must remain supported by Core contracts.
20. Observability must avoid secrets and sensitive payloads by default and use correlation IDs with configurable redaction.
21. Tool definitions and executable handlers are separate. Executable handlers are never serialized.
22. Initial tool taxonomy is `BuiltIn`, `Application`, `Declarative`, `UI`, `SqlServer`, and `MySql`. Extension tools are deferred.
23. System prompts are additive layers. A lower layer may add narrower instructions/restrictions but must not replace or erase higher layers. Prompt text is not an authorization boundary.
24. HAgent is the generic LLM cognition/execution layer for host software that needs LLM-driven behavior; Core must not become tied to one project type.
25. Host domain state, lifecycle, scheduling policy, persistence, authorization, and side effects remain outside HAgent unless exposed through a generic host-owned contract.
26. The canonical execution boundary must support generic host input/context, host correlation, execution options, and optional structured-output requirements without requiring plain string message as the only model.
27. Host correlation identity must remain distinct from HAgent execution identity and runtime-instance identity and must not be embedded into prompt text.
28. Structured output must be a real request/validation contract. Valid JSON text alone is not proof that a structured-output schema was honored.
29. Execution terminal-state transitions must be protected against late provider completion after cancellation, timeout, retirement, shutdown, or another terminal outcome.
30. Independent runtime instances must not share mutable runtime identity, override state, execution state, shutdown signaling, or private memory ownership.
31. Shared infrastructure such as stores, provider adapters, and tool registries may be reused across instances only through contracts that support concurrent use.
32. Skills are reusable versioned capability definitions, not private copies owned by every runtime instance.
33. Knowledge is a broader retrievable-information concept; Wiki is a managed knowledge source, not the universal representation for every future knowledge type.
34. Memory ownership and scope must be explicit. Working memory is execution-local; private long-term memory must remain isolated across independent runtime instances; shared memory requires explicit scope and authorization.
35. Learning must create typed candidates before promotion. Model-generated text must not directly mutate authoritative Wiki/knowledge or published Skills.
36. `LearningMode` governs learning behavior and is distinct from capability enable/disable policy.
37. Agent capability policy must support profile defaults plus runtime tri-state overrides (`Inherit`, `Enabled`, `Disabled`) for Skills, Knowledge/Wiki, Memory families/types, and future resource types.
38. Effective capability state must be resolved into an execution snapshot. Later profile/runtime edits must not alter an already-running execution.
39. Resource/type identity must be extensible so future knowledge types can be inventoried and shown without adding hard-coded Agent properties.
40. Learning promotion must preserve provenance, source execution/runtime identity, proposed scope, and evidence/confidence when available.
41. Configuration portability must serialize authoritative HAgent configuration rather than creating a second configuration model. Export/import must work across supported File, SQL Server, and MySQL storage backends.
42. Configuration packages must be versioned, compatibility-checked, and explicit about conflicts. Executable handlers, live runtimes, active executions, and transient process state are not portable configuration.
43. Credential-bearing exports are opt-in. Included provider API keys must remain encrypted inside the package and protected by the package's basic encryption/password mechanism.

## Context rules

44. WinForms integration belongs in `HAgent.WinForms`, not Core.
45. The public WinForms concept is **UI Context / Control Adapters**, not generic form serialization.
46. UI adapters should prefer native/bound data sources and bounded projections over scraping visible control state.
47. `DataTable` is optional, not the mandatory data representation.
48. Application-owned objects may be attached as live runtime context and inspected through bounded, non-executable discovery.
49. Discovery describes evidence; it never grants authorization or invents business meaning.
50. Explicit developer semantics/authorization may override or enrich automatic discovery.
51. Generic context may represent observations, state snapshots, events, records, objects, resources, or other host information without HAgent assigning domain meaning.

## Multi-agent rules

52. A workspace is a communication context, not an instruction to broadcast every message.
53. Unaddressed user messages go only to the configured workspace default recipient.
54. Direct user messages and agent delegation target explicit runtime participants.
55. Visible agent-to-agent dialogue is a real workspace message stream when the host enables it.
56. Coordinator and specialist are roles over the same generic runtime agent model.
57. Specialists may represent a whole domain, table, subsystem, or capability; they are not inherently tied to one record.
58. Dynamically created runtime agents come from reusable profiles and do not become permanent configuration entries by default.
59. Runtime retirement is explicit or follows host shutdown/lifecycle policy.
60. Runtime persistence, when enabled, must distinguish host instance, user/session, workspace, profile ID, and runtime instance ID.
61. Private memory belongs to runtime ownership; shared memory requires explicit scope and authorization.

## External consumers

62. External hosts consume HAgent through provider-neutral public contracts and do not require host-specific dependencies in Core.
63. HAgent must not contain host-specific physics, rendering, simulation time, application state, domain actions, or other domain rules.
64. External hosts remain authoritative for their state and side effects. HAgent supplies generic agent execution, context, tools, memory integrations, coordination, structured output, and telemetry.

## WinForms UI conventions

65. Do not use `System.Windows.Forms.MessageBox` directly in `HAgent.WinForms`.
66. Use `HMessage.ShowDelete`, `ShowQuestion`, `ShowInformation`, `ShowError`, and `ShowException` for dialogs.
67. Use the shared HAgent `Header` for HAgent form chrome.
68. Use `HButton` for HAgent action buttons.
69. Preserve existing UI/layout work unless a task explicitly requests UI changes.
70. Knowledge/Skill/Learning management UI must use the shared HAgent conventions and must expose effective agent configuration, not only persisted profile references.

## Example and testing rules

71. `HAgent.Example` is the manual developer/verification host; it is not `HAgent.Tests`.
72. Every meaningful completed capability requires a matching Example verification using public APIs.
73. Keep Example code split across focused partial files/components.
74. Example snippets must be reproducible and explain required setup or shared setup.
75. Do not claim build/test success unless it was actually executed.
76. Network-provider automated tests must use fakes/local test infrastructure rather than a real vendor.

## Documentation rules

77. `README.md` is the public introduction and quick start.
78. `docs/architecture/` is the authoritative stable architecture description.
79. `docs/plan/` is implementation state: master direction, current state, and active implementation only.
80. `docs/roadmap/` is the ordered implementation path, including completed foundation history and future phases.
81. `docs/storage.md` contains storage-specific details.
82. Root `plan.md` and `roadmap.md` are generated; do not hand-edit them except to synchronize a generated view when automation has not yet run.
83. When implementation changes architecture or milestone state, update the authoritative source document in the same change.
84. Do not duplicate architectural decisions across multiple source documents when a referenced authoritative document can own the decision.
