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

## Context rules

24. WinForms integration belongs in `HAgent.WinForms`, not Core.
25. The public WinForms concept is **UI Context / Control Adapters**, not generic form serialization.
26. UI adapters should prefer native/bound data sources and bounded projections over scraping visible control state.
27. `DataTable` is optional, not the mandatory data representation.
28. Application-owned objects may be attached as live runtime context and inspected through bounded, non-executable discovery.
29. Discovery describes evidence; it never grants authorization or invents business meaning.
30. Explicit developer semantics/authorization may override or enrich automatic discovery.

## Multi-agent rules

31. A workspace is a communication context, not an instruction to broadcast every message.
32. Unaddressed user messages go only to the configured workspace default recipient.
33. Direct user messages and agent delegation target explicit runtime participants.
34. Visible agent-to-agent dialogue is a real workspace message stream when the host enables it.
35. Coordinator and specialist are roles over the same generic runtime agent model.
36. Specialists may represent a whole domain, table, subsystem, or capability; they are not inherently tied to one record.
37. Dynamically created runtime agents come from reusable profiles and do not become permanent configuration entries by default.
38. Runtime retirement is explicit or follows host shutdown/lifecycle policy; closing a source form does not automatically retire an instance unless the host chooses that policy.
39. Runtime persistence, when enabled, must distinguish host instance, user/session, workspace, profile ID, and runtime instance ID.
40. Private memory belongs to runtime ownership; shared memory requires explicit scope and authorization.

## External consumers

41. HWorld is a supported external consumer target, not a dependency of HAgent.
42. HAgent must not contain HWorld types, physics, rendering, simulation time, world state, or world-specific actions.
43. External hosts remain authoritative for their state and side effects. HAgent supplies generic agent execution, context, tools, memory integrations, coordination, and telemetry.

## WinForms UI conventions

44. Do not use `System.Windows.Forms.MessageBox` directly in `HAgent.WinForms`.
45. Use `HMessage.ShowDelete`, `ShowQuestion`, `ShowInformation`, `ShowError`, and `ShowException` for dialogs.
46. Use the shared HAgent `Header` for HAgent form chrome.
47. Use `HButton` for HAgent action buttons.
48. Preserve existing UI/layout work unless a task explicitly requests UI changes.

## Example and testing rules

49. `HAgent.Example` is the manual developer/verification host; it is not `HAgent.Tests`.
50. Every meaningful completed capability requires a matching Example verification using public APIs.
51. Keep Example code split across focused partial files/components.
52. Example snippets must be reproducible and explain required setup or shared setup.
53. Do not claim build/test success unless it was actually executed.
54. Network-provider automated tests must use fakes/local test infrastructure rather than a real vendor.

## Documentation rules

55. `README.md` is the public introduction and quick start.
56. `docs/architecture/` is the authoritative stable architecture description.
57. `docs/plan/` is implementation state: master direction, current state, and active implementation only.
58. `docs/roadmap/` is the ordered implementation path, including completed foundation history and future phases.
59. `docs/storage.md` contains storage-specific details.
60. Root `plan.md` and `roadmap.md` are generated; do not hand-edit them except to synchronize a generated view when automation has not yet run.
61. When implementation changes architecture or milestone state, update the authoritative source document in the same change.
62. Never maintain the same architectural decision independently in multiple documents.
