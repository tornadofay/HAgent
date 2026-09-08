# Phase 0.8 — Data Access + Authorization + Internal Storage + Resource Foundations

## Goal

Provide bounded structured data contracts, HAgent-owned persistence, and the canonical resource foundations required by the first-class Knowledge, Skills, Memory, and Learning architecture across File, SQL Server, and MySQL backends without ever using HAgent storage as access to a host application's business database.

This phase establishes the **resource substrate**. Mature resource governance, capability inheritance, runtime overrides, learning review, promotion, and management UI belong to the later Phase 0.9575 and must build on these foundations rather than introduce a second resource architecture.

## Steps

1. [x] Application-owned structured-query contract and authoritative field schema.
2. [x] Separate data-operation permissions and request-specific host authorization contracts.
3. [x] Query/result limits, cancellation, timeout, and resource budgets.
4. [x] HAgent internal storage backend configuration for File, SQL Server, and MySQL.
5. [x] Application-specific File storage layout.
6. [x] SQL Server HAgent database creation and initial schema bootstrap.
7. [x] MySQL HAgent database creation and initial schema bootstrap.
8. [ ] Wire providers, agents, tools, memory, conversations, skills, wiki/content, learning candidates, and runtime repositories to the selected backend.
9. [x] Versioned schema migrations beyond the initial bootstrap version.
10. [ ] Read-only HAgent internal data tools, audit/correlation metadata, and live Example verification before any internal writes beyond repository persistence.

## Resource foundation

11. [ ] Establish one provider-neutral resource identity contract that can represent Skills, Knowledge/Wiki resources, Memory families/types, Learning candidates, and future resource types without adding hard-coded resource properties to the agent model.
12. [ ] Establish explicit resource scope/ownership metadata compatible with the canonical identity model: Global, Tenant, Domain, User, Agent, Runtime, and Execution where applicable.
13. [ ] Establish resource provenance/source metadata and lifecycle/status/version metadata as shared foundation concepts.
14. [ ] Define the stable distinction between Skills, Knowledge/Wiki, Memory, and Learning candidates; do not collapse them into one persisted object model.
15. [ ] Establish stable/versioned Skill definitions and references while keeping executable handlers outside persistence.
16. [ ] Establish provider-neutral Knowledge/Wiki resource/source contracts and bounded retrieval semantics independent of keyword/vector/index implementation.
17. [ ] Normalize Memory foundations around working, episodic, semantic, procedural, and future extensible types while keeping memory ownership separate from physical storage.
18. [ ] Establish typed learning-candidate contracts and provenance fields needed for later governed promotion without making the model authoritative.
19. [ ] Preserve immutable snapshot compatibility: resource definitions and references must be safe to capture into active execution snapshots without later configuration edits mutating running executions.
20. [ ] Ensure the resource foundation remains usable without GPU hardware, vector databases, embeddings, or large resident indexes.

The read-only foundation now includes bounded provider/agent/tool inventory, memory inspection with scope/owner isolation, explicit-session conversation inspection, and execution-audit inspection. Execution audit persistence is available through File, SQL Server, and MySQL using a secret-safe payload-free record.

## Internal database naming

The default HAgent database name is derived from the host application name using `<application-name>-ai`, for example `nap-ai` or `hworld-ai`. The database name is controlled by HAgent storage naming rules and is not a user-editable field in the Storage UI.

## File backend

File storage is application-specific and rooted beneath the host executable directory in `HAgentData`, with dedicated areas for configuration, providers, agents, tools, skills, memory, conversations, wiki, runtime, cache, logs, and audit data. Resource foundation records remain HAgent-owned and must use additive schema/layout changes rather than host databases.

## Database backends

SQL Server and MySQL storage providers receive server name and username as persisted configuration metadata and a password through the secret/runtime boundary. They connect to the server, create the HAgent-owned database if it does not exist, and initialize only HAgent-owned tables. Schema version metadata supports deterministic migrations.

The internal database schema includes provider, agent, tool, memory, conversation, execution-audit, skill, wiki document/chunk, and schema metadata areas. Mature resource governance in Phase 0.9575 will add or complete persistence required for knowledge-resource relationships, skill versions/relationships, learning candidates/review state, capability assignments/overrides, and extensible memory-type policy where those foundations remain outstanding.

## Audit foundation

`AgentExecution` carries an execution-level correlation ID. `AgentExecutionAuditRecord` projects only execution/correlation identity, agent/provider/model metadata, lifecycle timing, state, and classified failure metadata. Prompts, responses, provider secrets, secret IDs, connection strings, raw exceptions, and other payloads are excluded.

`IExecutionAuditStore` provides bounded append/search persistence. File uses an HAgent-owned `audit/executions.jsonl` file; SQL Server and MySQL use the HAgent-owned `HAgentExecutionAudits` table.

## Live Example

The Example storage verification will exercise File, SQL Server, and MySQL initialization where the corresponding backend is configured. It will verify database creation when absent, idempotent initialization when present, schema version reporting, persistence through the HAgent repositories, execution-audit round trips, and strict separation from host application data. Resource-foundation Example coverage must also verify identity/scope/provenance/versioning boundaries for the canonical resource contracts as they become implemented.

## Boundaries

- No raw SQL from model input.
- No implicit access to the host application's business database.
- HAgent storage providers are internal persistence providers, not host database adapters.
- Database passwords remain in the secret/runtime boundary.
- UI discovery, object provenance, and model instructions do not grant database authorization.
- Knowledge, Skills, Memory, and Learning data remain HAgent-owned resources and do not grant host-business-data access.
- This phase establishes resource identity and persistence foundations; mature capability governance and learning promotion are deliberately later concerns.

## Exit criterion

A host can select an HAgent-owned storage backend, initialize or upgrade it deterministically, use HAgent repositories against it, and rely on canonical provider-neutral foundations for Knowledge, Skills, Memory, Learning candidates, scope, provenance, versioning, and future resource types without HAgent gaining access to the host application's business database.
