# Foundations — 0.1 through 0.7

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
