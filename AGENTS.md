# HAgent engineering rules

This repository is designed to be worked on by human developers and coding agents.

## Architecture invariants

1. Keep `HAgent.Core` dependency-light and provider-neutral.
2. Never put WinForms types in Core.
3. Never put SQL Server/MySQL implementation details in Core.
4. Provider-specific transport belongs in provider adapter assemblies.
5. Secrets belong to `ISecretStore`; never add secrets to ordinary persistent provider/agent/tool models.
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

## Context rules

32. WinForms integration belongs in `HAgent.WinForms`, not Core.
33. The public WinForms concept is **UI Context / Control Adapters**, not generic form serialization.
34. UI adapters should prefer native/bound data sources and bounded projections over scraping visible control state.
35. `DataTable` is optional, not the mandatory data representation.
36. Application-owned objects may be attached as live runtime context and inspected through bounded, non-executable discovery.
37. Discovery describes evidence; it never grants authorization or invents business meaning.
38. Explicit developer semantics/authorization may override or enrich automatic discovery.
39. Generic context may represent observations, state snapshots, events, records, objects, resources, or other host information without HAgent assigning domain meaning.

## Multi-agent rules

40. A workspace is a communication context, not an instruction to broadcast every message.
41. Unaddressed user messages go only to the configured workspace default recipient.
42. Direct user messages and agent delegation target explicit runtime participants.
43. Visible agent-to-agent dialogue is a real workspace message stream when the host enables it.
44. Coordinator and specialist are roles over the same generic runtime agent model.
45. Specialists may represent a whole domain, table, subsystem, or capability; they are not inherently tied to one record.
46. Dynamically created runtime agents come from reusable profiles and do not become permanent configuration entries by default.
47. Runtime retirement is explicit or follows host shutdown/lifecycle policy.
48. Runtime persistence, when enabled, must distinguish host instance, user/session, workspace, profile ID, and runtime instance ID.
49. Private memory belongs to runtime ownership; shared memory requires explicit scope and authorization.

## External consumers

50. External hosts consume HAgent through provider-neutral public contracts and do not require host-specific dependencies in Core.
51. HAgent must not contain host-specific physics, rendering, simulation time, application state, domain actions, or other domain rules.
52. External hosts remain authoritative for their state and side effects. HAgent supplies generic agent execution, context, tools, memory integrations, coordination, structured output, and telemetry.

## WinForms UI conventions

53. Do not use `System.Windows.Forms.MessageBox` directly in `HAgent.WinForms`.
54. Use `HMessage.ShowDelete`, `ShowQuestion`, `ShowInformation`, `ShowError`, and `ShowException` for dialogs.
55. Use the shared HAgent `Header` for HAgent form chrome.
56. Use `HButton` for HAgent action buttons.
57. Preserve existing UI/layout work unless a task explicitly requests UI changes.

## Example and testing rules

58. `HAgent.Example` is the manual developer/verification host; it is not `HAgent.Tests`.
59. Every meaningful completed capability requires a matching Example verification using public APIs.
60. Keep Example code split across focused partial files/components.
61. Example snippets must be reproducible and explain required setup or shared setup.
62. Do not claim build/test success unless it was actually executed.
63. Network-provider automated tests must use fakes/local test infrastructure rather than a real vendor.

## Documentation rules

64. `README.md` is the public introduction and quick start.
65. `docs/architecture/` is the authoritative stable architecture description.
66. `docs/plan/` is implementation state: master direction, current state, and active implementation only.
67. `docs/roadmap/` is the ordered implementation path, including completed foundation history and future phases.
68. `docs/storage.md` contains storage-specific details.
69. Root `plan.md` and `roadmap.md` are generated; do not hand-edit them except to synchronize a generated view when automation has not yet run.
70. When implementation changes architecture or milestone state, update the authoritative source document in the same change.
71. Never maintain the same architectural decision independently in multiple documents.
