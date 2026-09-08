# HAgent engineering rules

This repository is designed to be worked on by human developers and coding agents.

## Development mode: redesign and rebuild

HAgent is currently in **redesign/rebuild mode**. Architectural correctness and a clean target design take priority over backward compatibility with obsolete code, APIs, storage shapes, or internal mechanisms.

1. Existing code is not presumed worth preserving during redesign/rebuild mode.
2. Remove, rename, reshape, or replace obsolete mechanisms when the new architecture requires it. Do not add compatibility wrappers, aliases, migration shims, duplicate models, or parallel mechanisms merely to avoid changing existing code.
3. Prefer one coherent canonical architecture over transitional dual paths.
4. Existing tests, Example code, storage schemas, and public APIs are evidence of current state, not constraints on the target architecture.
5. When structural changes invalidate old code, update dependent code and tests to the new design rather than preserving obsolete behavior.
6. The user will explicitly declare when HAgent enters **code-lock/stabilization mode**. Until then, redesign-first changes are authorized.
7. Once code-lock/stabilization mode is declared, compatibility, migration, deprecation, and regression-preservation requirements may be introduced deliberately.

## Global architecture invariants

8. Keep `HAgent.Core` dependency-light and provider-neutral.
9. Never put WinForms types or SQL Server/MySQL implementation details in Core.
10. Provider-specific transport belongs in provider adapter assemblies.
11. HAgent remains a generic cognition/execution layer; Core must not become tied to one project type, provider, model family, or host domain.
12. Host domain state, lifecycle, scheduling policy, persistence, authorization, and side effects remain outside HAgent unless exposed through a generic host-owned contract.
13. A provider describes connection/transport concerns; an agent profile describes reusable behavior/configuration.
14. Keep agent profile identity separate from runtime agent-instance identity. Runtime scope is a binding concept, not a separate agent class.
15. Preserve .NET Framework 4.8.1 compatibility where currently targeted and avoid framework-sized dependencies when a focused adapter is sufficient.
16. Public network/database APIs must support cancellation and remain async.
17. Active execution must use immutable/effective snapshots so configuration edits cannot corrupt running work.
18. Core routing must not assume OpenAI-specific semantics.
19. Memory must remain viable without GPU, vector database, or a large resident RAM index.
20. Capability support must be explicit as `Supported`, `Unsupported`, or `Unknown`, with evidence/provenance where practical.
21. Provider responses remain provider-neutral while preserving supported structured output, tool calls/results, explicitly exposed reasoning metadata, usage, and appropriate raw metadata.
22. Streaming is optional; providers that do not stream remain supported by Core contracts.
23. Observability must avoid secrets and sensitive payloads by default and use correlation IDs with configurable redaction.
24. Provider credentials may be persisted with provider configuration, but must be encrypted at rest and redacted from diagnostics. Do not introduce a separate secret-reference/vault subsystem unless a future requirement explicitly demands it.

## Security and authority

25. The model is a requester, not an authority.
26. Permissions, authorization, approval, budgets, cancellation, and host-side validation are enforcement mechanisms, not prompt instructions.
27. Tool execution is a host capability boundary. Models may request only registered tools and never receive arbitrary reflection, process, file, database, control-tree, or memory access.
28. Tool definitions and executable handlers are separate. Executable handlers are runtime-owned and never serialized.
29. Learning must create typed candidates before promotion. Model-generated text must not directly mutate authoritative knowledge or published Skills.
30. Learning promotion, cognitive intervention, and external side effects require explicit policy and authorization boundaries.
31. Structured data contracts are not raw SQL access. HAgent storage never grants implicit access to the host application's business database.
32. System prompts are additive instruction layers. Lower layers may add narrower constraints but must not replace or erase higher systems. Prompt text is never an authorization boundary.

## Execution, resources, and portability

33. The canonical execution boundary supports generic host input/context, host correlation, execution options, and optional structured-output requirements; plain string messaging is only a convenience form.
34. Host correlation identity must remain distinct from HAgent execution identity and runtime-instance identity and must not be embedded into prompt text.
35. Structured output is a real request/validation contract. Valid JSON text alone is not proof that a schema was honored.
36. Execution terminal-state transitions must be protected against late provider completion after cancellation, timeout, retirement, shutdown, or another terminal outcome.
37. Independent runtime instances must not share mutable runtime identity, override state, execution state, shutdown signaling, or private memory ownership.
38. Shared stores, provider adapters, and tool registries may be reused across instances only through concurrently safe contracts.
39. Skills are reusable versioned capability definitions, not private copies owned by every runtime instance.
40. Knowledge is broader than Wiki; Wiki is one managed knowledge source, not the universal representation for every future knowledge type.
41. Memory ownership and scope must be explicit. Working memory is execution-local; private long-term memory stays isolated across independent runtimes; shared memory requires explicit scope and authorization.
42. `LearningMode` is distinct from capability enable/disable policy.
43. Agent capability policy supports profile defaults plus runtime tri-state overrides (`Inherit`, `Enabled`, `Disabled`) for Skills, Knowledge/Wiki, Memory families/types, and future resource types.
44. Effective capability state is resolved into an execution snapshot. Later profile/runtime edits must not alter an already-running execution.
45. Resource/type identity remains extensible so future resource types can be inventoried without adding hard-coded Agent properties.
46. Learning promotion preserves provenance, source execution/runtime identity, proposed scope, and evidence/confidence when available.
47. Configuration portability serializes authoritative HAgent configuration rather than creating a second configuration model.
48. Configuration packages are versioned, compatibility-checked, and explicit about conflicts. Executable handlers, live runtimes, active executions, and transient process state are not portable configuration.
49. Credential-bearing exports are opt-in. Included provider API keys remain encrypted inside the package and protected by the package mechanism.

## Identity and tenancy

50. Use the canonical provider-neutral identity and ownership model defined in `docs/architecture/05-identity.md`; do not create subsystem-specific identity models.
51. Distinguish deployment, tenant, principal, user, session, workspace, agent profile, runtime instance, and execution identities.
52. Tenancy is optional; single-tenant hosts must not need artificial tenant identities.
53. Resource scope is explicit and must not be inferred from a generic user ID. Canonical scopes are `Global`, `Tenant`, `User`, `Workspace`, `Agent`, `Runtime`, and `Execution`.
54. Resource ownership/partition keys are canonical architecture, not a compatibility layer. Subsystems must use the ownership model directly when redesigned.
55. Identity context must propagate through applicable execution, tools, audit, memory, knowledge, learning, policy, events, tracing, evaluation, runtime persistence, workspaces, and human approval boundaries.
56. Missing identity is valid only where the subsystem explicitly permits anonymous/single-user operation; identity-required boundaries fail closed.
57. Private resources must be isolated by explicit owner identity. Shared resources require explicit scope and authorization.

## Context and discovery

58. Before changing context or discovery behavior, read the authoritative context architecture, especially `docs/architecture/20-context.md` and relevant implementation documents.
59. WinForms integration belongs in `HAgent.WinForms`, not Core.
60. The public WinForms concept is **UI Context / Control Adapters**, not generic form serialization.
61. Prefer native/bound data sources and bounded projections over scraping visible control state.
62. `DataTable` is optional, not the mandatory data representation.
63. Application-owned objects may be attached as live runtime context and inspected through bounded, non-executable discovery.
64. Discovery describes evidence; it never grants authorization or invents business meaning.
65. Explicit developer semantics/authorization may override or enrich automatic discovery.
66. Generic context may represent observations, state snapshots, events, records, objects, resources, or other host information without HAgent assigning domain meaning.

## Multi-agent and workspace behavior

67. Before changing workspace or coordination behavior, read the authoritative workspace architecture, especially `docs/architecture/50-workspaces.md`.
68. A workspace is a communication context, not an instruction to broadcast every message.
69. Unaddressed user messages go only to the configured workspace default recipient.
70. Direct user messages and agent delegation target explicit runtime participants.
71. Visible agent-to-agent dialogue is a real workspace message stream when the host enables it.
72. Coordinator and specialist are roles over the same generic runtime agent model.
73. Specialists may represent a whole domain, table, subsystem, or capability; they are not inherently tied to one record.
74. Dynamically created runtime agents come from reusable profiles and do not become permanent configuration entries by default.
75. Runtime retirement is explicit or follows host shutdown/lifecycle policy.
76. Runtime persistence must distinguish host instance, user/session, workspace, profile ID, and runtime instance ID where those concepts apply.
77. Private memory belongs to runtime ownership; shared memory requires explicit scope and authorization.

## Persistent cognition

78. Before changing persistent cognition, read `docs/architecture/16-cognitive-runtime.md` and the relevant roadmap/plan documents.
79. Persistent cognition uses a stable Cognitive Kernel plus replaceable Cognitive Strategies. Do not couple the kernel or strategy to a specific provider/model.
80. The LLM is a replaceable reasoning component, not the authoritative cognitive runtime.
81. A cognitive strategy may choose deterministic behavior or no LLM when current state, policies, memory, knowledge, and operators are sufficient.
82. Reasoning Requirement and Execution Planner are distinct decisions: cognition determines what reasoning capability is needed; execution planning determines where/how it executes.
83. Beliefs, goals, intentions, plans, operators, impasses, cognitive actions, experience, and revision remain provider-neutral and revision-safe.
84. Learned behavior must not silently rewrite the Cognitive Kernel. Proceduralization and cognitive improvement remain versioned, evidence-based, reversible, and policy-governed.

## WinForms UI conventions

85. Do not use `System.Windows.Forms.MessageBox` directly in `HAgent.WinForms`.
86. Use `HMessage.ShowDelete`, `ShowQuestion`, `ShowInformation`, `ShowError`, and `ShowException` for dialogs.
87. Use the shared HAgent `Header` for HAgent form chrome.
88. Use `HButton` for HAgent action buttons.
89. Preserve existing UI/layout work unless the task explicitly requests UI changes.
90. Knowledge/Skill/Learning management UI must use the shared HAgent conventions and expose effective agent configuration, not only persisted profile references.
91. Known resource types may receive specialized panels while future/unknown resource types remain visible through generic resource inventory contracts.
92. `AISettingsForm` is a composition shell only. Do not add feature-specific CRUD, evaluation, persistence, or editor logic to it.
93. Configuration features belong under `src/HAgent.WinForms/UI/Configuration/<Area>/`; one feature directory owns one configuration domain.
94. Add a new configuration area by creating a focused page class, registering one navigation entry in `AISettingsForm`, and passing shared dependencies through `ConfigurationContext`. Do not add reflection-based navigation injection or a second configuration launcher.
95. Edit an existing configuration area in its feature directory first. Only change `AISettingsForm` when the shell/navigation contract itself changes.
96. Remove a configuration area by removing its explicit navigation registration, page construction/reference, obsolete context members, and feature files. Do not leave hidden or duplicate navigation paths.
97. Use `docs/architecture/91-winforms-configuration-maintenance.md` as the detailed maintenance guide for configuration UI changes.
98. List-oriented configuration pages must use separate header, action-bar, and content regions. Prefer the shared three-row page layout helper; do not place competing `DockStyle.Top` controls directly over a list/content sibling.

## Example and testing rules

99. `HAgent.Example` is the manual developer/verification host; it is separate from `HAgent.Tests`.
100. Every meaningful completed capability requires matching Example verification using public APIs.
101. Organize Example UI by feature. As the number of examples grows, use top-level feature tabs such as **Tools**, **Context**, **Memory**, **Policy**, **Providers**, **Runtime**, **Cognition**, etc., rather than an ever-growing flat tab list.
102. When a feature has multiple examples, place a nested `TabControl` inside that feature page and give each example its own focused sub-tab. For example: **Tools** → **Tool Registry**, **Tool Loop**, **Tool Validation**, **Tool Persistence**, and related tool scenarios.
103. Group examples according to architecture and capability boundaries, not merely implementation class names. Keep each example independently understandable and easy to run.
104. Keep Example code split across focused partial files/components matching feature groupings where practical.
105. Example scenarios must be reproducible and explain required setup or shared setup.
106. Every meaningful capability should have deterministic verification for important success, failure, cancellation, concurrency, persistence, and boundary cases appropriate to its design.
107. Do not claim build/test success unless it was actually executed.
108. Network-provider automated tests must use fakes/local test infrastructure rather than a real vendor.

## Documentation and source-of-truth rules

109. `README.md` is the public introduction and quick start.
110. `docs/architecture/` is the authoritative stable architecture description.
111. `docs/plan/` is implementation state: master direction, current state, active work, and durable project decisions.
112. `docs/roadmap/` is the ordered implementation path, including completed foundation history and future phases.
113. `docs/storage.md` contains storage-specific details.
114. Root `plan.md` and `roadmap.md` are generated; do not hand-edit them except to synchronize a generated view when automation has not yet run.
115. When implementation changes architecture or milestone state, update the authoritative source document in the same change when practical.
116. Do not duplicate architectural decisions across source documents when a referenced authoritative document can own the decision.
117. During redesign/rebuild mode, document the target architecture rather than compatibility with obsolete mechanisms.
118. Before changing a subsystem, identify and read its authoritative architecture document. Treat `AGENTS.md` as global constraints and the subsystem document as the detailed authority.

## Complete-architecture implementation standard

119. Substantial features must be implemented against the **complete intended architecture that can reasonably be derived before coding**, not against a deliberately simplified or temporary version intended to be redesigned later.
120. Before implementing substantial work, inspect the existing architecture and implementation, identify dependencies and invariants, and reason through known lifecycle, persistence, concurrency, failure, security, performance, compatibility, and extension requirements.
121. Do not knowingly defer foundational requirements merely to make implementation easier or faster to demonstrate when those requirements are already part of the intended design.
122. Testing is primarily for verification, defect discovery, regression detection, and genuinely unforeseen interactions. Do not use testing as a substitute for architectural analysis that should have happened before implementation.
123. When testing reveals a requirement or interaction that could not reasonably have been known beforehand, update the authoritative architecture/decision documentation rather than applying an undocumented workaround.
124. Code may be refined after testing, but refinement should normally correct defects, improve clarity/performance, or incorporate genuinely new information—not replace an intentionally incomplete architecture.
125. When uncertain whether a proposed implementation is a complete target design or only a temporary simplification, resolve that uncertainty before coding and make the decision explicit in the relevant architecture/decision document.
126. **Complete is scope-bounded:** implement the complete intended architecture for the current task/phase while remaining compatible with known future architecture. Do not prematurely implement unrelated future roadmap phases.

## Run-bounded execution standard

127. **Substantial work must be split into run-sized slices before implementation begins.** A “run” means one bounded implementation-and-verification cycle that the coding agent can reasonably complete within the available execution budget.
128. Each run must have one explicit objective, a small set of files/assemblies it is expected to change, and a concrete verification target. Do not start the next architectural slice merely because the current slice is partially implemented.
129. When a task contains multiple independent or sequential slices, record them as an ordered checklist in the authoritative active-work document. Mark exactly one slice as **current** and define its entry condition and completion condition.
130. Before coding, estimate whether the current slice can reach a verified checkpoint in the same run. If it cannot, split it further before making implementation changes.
131. Prefer a smaller verified slice over a larger partially implemented slice. Repository work must not depend on reaching the end of a large task in one uninterrupted session.
132. Do not combine implementation, broad refactoring, unrelated cleanup, UI expansion, documentation migration, and multi-framework verification into one run when they can be separated without architectural loss.
133. At the end of every run, reach one of two explicit states: **verified complete** or **verified checkpoint/blocker**. A timeout or interruption is not a completion state.
134. If the run ends before verification, do not claim success. Record the exact unfinished slice, files changed, known blocker/failure, and the next smallest safe step in the active-work document before moving to unrelated work.
135. Where practical, keep each run's code change internally coherent and buildable. Do not intentionally leave the repository in a known broken state merely because a later run is expected to fix it.
136. After a verified slice, update the active-work/current-state/architecture documentation required by the existing source-of-truth rules before selecting the next slice.
137. The next run must resume from the recorded checkpoint, not re-discover or duplicate completed work. Do not begin a parallel implementation of a partially completed slice.
138. The complete-architecture rule remains in force **within each bounded slice**: splitting a task changes execution size, not architectural quality or required analysis.

## Persistent project-memory protocol

139. The repository is the durable project memory for development across constrained, interrupted, or new AI sessions. Use small purpose-specific Markdown documents to preserve the minimum state needed to resume work safely.
140. Persistent project-memory documents are **compressed project state, not transcripts**. Never copy conversation history, raw chain-of-thought, or every implementation step into them.
141. Keep each memory document small and purpose-specific. Prefer updating/replacing current state over continuously appending history.
142. Use the current-state document for project position, the active-work document for unfinished work, the decisions document for durable decisions, and architecture documents for detailed subsystem design.
143. Before starting substantial work, read the relevant current-state, active-work, architecture, and decision documents before designing or changing implementation.
144. Active unfinished work must have one clear authoritative work record. Do not begin a parallel implementation of the same scope unless the active scope is explicitly changed.
145. Update active-work state when scope, completion state, blockers, or next work changes materially. Close/remove completed work from active state once it is represented by the appropriate authoritative project document.
146. Record only durable architectural decisions, invariants, rejected alternatives that prevent future confusion, and important discoveries that materially affect future work. Do not record routine coding history.
147. Never create duplicate sources of truth. One document should own each durable fact; other documents should reference it where useful.
148. When information conflicts, do not silently choose one. Resolve the conflict against the authoritative architecture/current state, then update the relevant source explicitly.
149. Never silently delete important persistent knowledge. If a decision becomes obsolete, mark it superseded and record the replacement where the distinction matters.
150. When implementation changes architecture, milestone state, or a durable decision, update the relevant authoritative document in the same change set whenever practical.
151. At the end of substantial work, update the compact project-memory state so the next session can identify what is complete, what remains, what is blocked, and where authoritative details live.
152. Before starting a new task, check the active-work state and current project state so unfinished work is continued or deliberately superseded rather than duplicated or lost.
153. Do not create a new memory file merely because a task is large. Add or update an existing authoritative document when the information fits its purpose; create a new persistent document only when it has a distinct long-lived responsibility.
154. Generated root documents remain views. Persistent memory belongs in authoritative source documents, not in generated `plan.md` or `roadmap.md`.
