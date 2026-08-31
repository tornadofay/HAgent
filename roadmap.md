# HAgent Roadmap

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/roadmap`.

The roadmap is the ordered implementation path toward the HAgent master plan. It describes phases and dependencies; stable architectural definitions belong under `docs/architecture/`, and current work belongs under `docs/plan/`.

## Current position

- 0.1–0.2 — foundations complete
- 0.3–0.4 — memory/context and provider-capability foundations complete
- 0.5 — tool foundation complete; hardening remains
- 0.6 — safety/permission foundation complete; broader authorization remains
- 0.7 — WinForms UI Context + Data Discovery complete and locally verified
- 0.8 — Data Access + Authorization **active**
- 0.9 — Runtime Agent Instances
- 0.10 — Workspaces, Routing + Chat
- 1.0 — Collaboration + Workflows
- Later — provider ecosystem, extensibility, developer platform, release hardening

The sequence is intentional: secure host/data capabilities come before rich autonomous collaboration. HWorld can begin integration at the 0.9 runtime-instance boundary; it does not need the business-application chat layers.

This file records the completed foundation path and the remaining hardening work that grows directly out of those phases. It is the roadmap's implementation history; the master plan does not repeat these checklists.

## 0.1 Foundation — complete

Implemented:

- Multi-target .NET Framework 4.8.1 and .NET 9 where supported.
- Provider and agent configuration with multi-provider relationships.
- OpenAI-compatible provider adapter.
- File, SQL Server, and MySQL persistence foundations.
- Protected local secrets.
- Provider/agent/tool management UI.
- Model discovery and connection testing.
- Dependency-aware deletion behavior.
- `HAgent.Example` integration host and modular examples.
- Global agent selection and output handling.

## 0.2 Runtime — complete

Implemented:

- Execution lifecycle and stable execution IDs.
- Provider routing and ordered candidates.
- Retries, timeout, and cancellation.
- Diagnostics and structured failure categories.
- Actionable provider/model/account error reporting.
- System-prompt resolution.
- Execution snapshots so active work is isolated from later configuration changes.
- Low-RAM/no-GPU design constraints.

## 0.3 Memory + Context — foundation complete

Implemented:

- Persistent JSONL memory and bounded search.
- Explicit remember/recall/forget.
- Memory scopes.
- Typed Fact/Preference/Task/Event records.
- Persistent conversations and sessions.
- Context budgets and tokenizer-free estimation.
- Conservative automatic memory.
- Lightweight relevance ranking.
- Episodic memory with provenance.

Deferred maturation:

- Memory upsert/update semantics.
- Retention/expiration policies.
- Context compaction/summarization.
- Larger-store indexing improvements.
- SQL Server/MySQL memory stores.
- Conversation listing/search/metadata management.
- Optional vector-memory adapters and remote embeddings.

## 0.4 Provider Capabilities + Response Normalization — foundation complete

Implemented:

- Tri-state capability reporting with evidence/confidence.
- Capability caching.
- Normalized text, reasoning, raw text, structured output, tool calls, usage, and provider metadata.
- Separate reasoning handling and `<think>` diagnostics.
- Provider error classification/advice.
- Streaming delta contract.
- OpenAI-compatible SSE streaming.
- Streaming cancellation.
- Live streaming verification.

## 0.5 Tools + Agent Loop — foundation complete

Implemented:

- Six initial tool categories: BuiltIn, Application, Declarative, UI, SqlServer, MySql.
- Tool definition/handler separation.
- Tool registry and application-registered handlers.
- JSON Schema validation.
- Provider tool-definition transport.
- Bounded multi-turn tool loops.
- Persisted tool definitions.
- Per-agent tool assignment.
- Live Groq tool-loop verification.

Hardening remains:

- Per-session temporary tools.
- Built-in tool handlers.
- Declarative execution engine.
- Tool aliases/versioning.
- Tool timeout/cancellation/progress.
- Tool audit/history and budgets.
- Stronger loop detection and capability negotiation.

## 0.6 Safety + Permissions — foundation complete

Implemented:

- General permission configuration UI.
- Persisted current WinForms permission policy.
- Safe defaults for automatic discovery/read/write/invoke behavior.

Remaining platform safety work:

- Read/write/invoke/export authorization across tool categories.
- Host authorization callbacks.
- Human approval lifecycle.
- Input/output/tool guardrails.
- Execution/tool/memory budgets.
- Tracing and observability.
- Sensitive-data redaction.

## 0.7 WinForms UI Context + Data Discovery — complete

Implemented and locally verified:

- Form and arbitrary control-tree/UserControl attachment with stable root identity.
- Read-only inspection and control reads.
- Semantic control discovery.
- Bound/native data-source discovery for DataTable, DataView, BindingSource, IList, arrays, and compatible collections.
- CurrencyManager/current-item/position/count metadata.
- Control-to-source relationships based on actual bindings/source identity.
- Convention-based control adapters, including external `IHyperControl`-style controls using members such as `DbFieldName`, `GetValue()`, and `SetValue(object)`.
- Live application-object attachment and bounded structural inspection.
- `maxDepth` and `maxCollectionItems` resource limits.
- Provider-neutral structured data projection and query contracts with explicit fields, scalar filters, sorting, and bounded paging.
- Example verification for UI Context, UserControl, native IList, data relationships, custom control adaptation, application-object context, and query semantics.

## Foundation exit

These phases establish the base on which the remaining roadmap is built. The next work is not another UI discovery feature; it is safe real data access followed by the runtime-agent model required for multi-agent hosts and HWorld.

## Goal
Turn the verified data-discovery and structured-query contracts into safe real application/database access.

## Steps

1. Application-owned adapter implementing `IDataQuerySource` for explicitly approved sources.
2. Authoritative schema/field allow-list independent of model requests.
3. Separate permissions for discovery, projection/query, export, and write operations.
4. Host authorization callback contract.
5. Query/result limits, cancellation, timeout, and resource budgets.
6. Restricted SQL Server read adapter using generated parameterized commands only.
7. Restricted MySQL read adapter using generated parameterized commands only.
8. Database audit/correlation metadata.
9. Read-only database tools before database writes.
10. Live `HAgent.Example` integration against a disposable/read-only test database.

## Live Example

The SQL integration Example will provide runtime-only connection fields:

```text
Server Name
User Name
Password
Database
```

It will verify connection, authorization, schema/field allow-listing, structured queries, bounded results, cancellation/timeout, and rejection of unauthorized operations. Connection values must never become persistent agent/tool configuration or normal logs.

## Boundaries

- No raw SQL tool.
- No arbitrary SQL fragments in the structured query contract.
- No implicit access to every table or field.
- UI discovery, `TableInfo`-style metadata, provenance, or model instructions do not grant authorization.
- Database passwords remain in the secret/connection boundary.

## Exit criterion

A host can authorize and execute a bounded structured read against its application or database source through the public HAgent abstractions, and the Example verifies both successful access and denied access cases.

## Goal
Make live agents first-class runtime objects separate from reusable agent profiles.

## Steps

1. Introduce a provider-neutral runtime agent instance with its own stable instance ID and profile reference.
2. Define explicit runtime scopes: Application, Workspace, Context/Form, Session, Task, and Ephemeral.
3. Allow runtime-specific context and provider/model overrides without mutating stored profiles.
4. Give each runtime instance an independent memory owner.
5. Support multiple runtime instances executing concurrently.
6. Expose asynchronous scheduling, cancellation, timeout, correlation, and stale-result protection.
7. Define explicit active/retired/shutdown lifecycle behavior.
8. Keep dynamically created agents out of persistent configuration by default.
9. Add optional runtime-state persistence for recovery, collaboration, or multi-process deployments.
10. Verify the runtime contract with deterministic Example coverage.
11. Add the first HWorld adapter verification at this boundary.

## Runtime rule

One configured profile can produce many live instances. Roles such as coordinator and specialist are host policy over the same runtime model, not separate agent classes.

## HWorld gate

HWorld can begin consuming HAgent when the runtime exposes independent agent instances, asynchronous execution, caller-supplied observation/context, structured tool requests, cancellation/timeout, and stale-result protection. HWorld remains responsible for world state and action validation.

## Exit criterion

A host can create, run, cancel, and retire multiple independent runtime agents from reusable profiles without identity, memory, or execution-state collisions.

## Goal
Provide an optional shared conversation where a user and multiple runtime agents can visibly work together while every model request is routed deliberately.

## Steps

1. Introduce a workspace abstraction independent of WinForms.
2. Register users and runtime-agent participants with explicit lifecycle state.
3. Define one workspace default recipient for unaddressed user messages.
4. Define direct user-to-agent addressing.
5. Define addressed agent-to-agent delegation and responses.
6. Make coordinator/specialist behavior a role/policy over generic runtime agents.
7. Allow specialists to represent whole domains, tables, subsystems, or other host responsibilities.
8. Preserve sender, recipient, correlation, causation, ordering, and routing metadata.
9. Make agent-to-agent work visible in the workspace when the host enables it.
10. Add configurable addressing syntax at the host/UI layer.
11. Add loop protection and collaboration budgets.
12. Add optional workspace persistence and shared-memory policy.
13. Add the WinForms chat surface and global agent selection.

## Routing rules

- Unaddressed user message: send only to the workspace default recipient.
- Explicitly addressed user message: send to that participant.
- Agent delegation: send only to the addressed participant unless an explicit policy invokes others.
- Broadcast: explicit opt-in operation, never the default.

The user can observe the coordinator ask a specialist to work and the specialist return its result before the coordinator answers.

## Specialist context

A contextual specialist may be created automatically from a configured profile. Its runtime prompt/context can contain UI, data, application-object, task, or other host context according to authorization policy. The specialist can report what it knows, inferred information, unknowns, and authorization limits honestly.

## Exit criterion

A host can run a visible multi-agent conversation in which messages reach only their intended recipients, coordinator/specialist delegation works, and workspace execution remains bounded and traceable.

## Goal
Turn basic workspace messaging into reliable multi-agent collaboration and then into bounded task/workflow execution.

## Collaboration steps

1. First-class delegation/handoff operations.
2. Shared/private workspace context policies.
3. Parallel specialist work with bounded collaboration budgets.
4. Human intervention and approval points.
5. Explicit runtime/participant lifecycle states.
6. Cross-agent memory sharing only through explicit policy.
7. Collaboration history, audit, and traceability.

## Workflow steps

1. Task/job model and lifecycle.
2. Planning, execution, and verification stages.
3. Multi-step and branching workflows.
4. Background execution and scheduling.
5. Pause/resume and durable checkpoints.
6. Event-triggered execution.
7. Per-step timeout, cancellation, retry, approval, and budget policies.

## Boundary

These are generic orchestration facilities. HAgent does not become the authority for business rules, simulation state, or host-side side effects.

## Exit criterion

A host can coordinate multiple agents and long-running work with bounded execution, explicit authority, resumable state where required, and observable collaboration.

These capabilities follow the core runtime, data, and collaboration milestones. They should not block the primary host-integration path.

## Provider ecosystem

- [ ] Additional provider adapters such as Azure OpenAI, Anthropic, Google/Gemini, Ollama, LM Studio, and custom HTTP providers where justified.
- [ ] Multimodal and embedding adapters.
- [ ] Provider capability/contract harness.

## Extensibility

- [ ] Provider, tool, UI-adapter, and storage extension model.
- [ ] Extension validation and failure isolation.
- [ ] External secret stores and secret rotation.
- [ ] Optional MCP/vector integrations where they fit the lightweight architecture.

## Developer platform

- [ ] Optional DI/interoperability integrations.
- [ ] Simulation/test mode for external consumers such as HWorld.
- [ ] Diagnostics/trace viewer.
- [ ] Complete Example coverage for meaningful public APIs.
- [ ] SDK guidance for provider, tool, UI-context, and host integrations.

## Release hardening

- [ ] Stable public contracts and compatibility policy.
- [ ] Storage migration/versioning.
- [ ] NuGet packaging and release process.
- [ ] Security/provider/tool/memory integration coverage.
- [ ] Documentation and migration guidance.

`.NET 10` remains a future target after the development environment and compatibility policy are ready.
