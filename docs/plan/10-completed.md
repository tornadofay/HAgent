# Completed implementation ledger

## 0.1 Foundation
- [x] Multi-target .NET 4.8.1 and .NET 9.
- [x] Provider/agent models and multi-provider references.
- [x] OpenAI-compatible adapter.
- [x] File/SQL Server/MySQL persistence foundations.
- [x] Protected local secrets.
- [x] Provider/agent/tool management UI.
- [x] HAgent.Example integration host and modular tests.

## 0.2 Runtime
- [x] Execution lifecycle and stable execution IDs.
- [x] Provider routing, attempts, retries, timeout, cancellation.
- [x] Diagnostics and structured failure categories.
- [x] Actionable provider/model/account errors.
- [x] System-prompt resolution and failure detail preservation.

## 0.3 Memory + Context
- [x] Persistent JSONL memory and bounded search.
- [x] Explicit remember/recall/forget and scopes.
- [x] Typed Task/Event/Fact/Preference records.
- [x] Conversation store and persistent sessions.
- [x] Context budgets and tokenizer-free estimate.
- [x] Conservative automatic memory policy.
- [x] Lightweight relevance ranking.
- [x] Episodic memory with provenance.

## 0.4 Capabilities + Response Normalization
- [x] Tri-state capabilities and evidence/provenance.
- [x] Capability cache.
- [x] Normalized text/reasoning/raw/structured/tool/usage metadata.
- [x] `<think>` detection without assuming native reasoning.
- [x] Provider error classification/advice.
- [x] Streaming delta contract and OpenAI-compatible SSE.
- [x] Streaming cancellation.

## 0.5 Verified tool foundation
- [x] Six initial tool types.
- [x] Tool definition/handler separation.
- [x] Tool registry and application handler.
- [x] JSON Schema validation.
- [x] OpenAI-compatible tool transport.
- [x] Bounded multi-turn tool loop.
- [x] File tool-definition persistence.
- [x] Agent `ToolIds` assignment model.
- [x] Live Groq tool loop verification.

## 0.7 WinForms UI Context + Data Discovery
- [x] `IUiContext` and `WinFormsUiContext`.
- [x] Form and arbitrary control-tree/UserControl attachment with stable root identity.
- [x] UI-thread-safe read-only inspection and control reads.
- [x] Bound/native `DataGridView` data-source extraction without mandatory `DataTable` normalization.
- [x] Semantic control discovery.
- [x] Data-source discovery for `DataTable`, `DataView`, `BindingSource`, `IList`, arrays, and compatible collections.
- [x] CurrencyManager, current-item, position/count, list/source metadata.
- [x] Control-to-source relationship discovery based on actual bindings and source identity.
- [x] Convention-based `IUiControlAdapter`.
- [x] Reflection adaptation of external `IHyperControl`-style controls using `DbFieldName`, `GetValue()`, and `SetValue(object)`.
- [x] Live application-object attachment by stable ID.
- [x] Bounded structural discovery of application-owned objects such as `TableInfo` without compile-time type knowledge.
- [x] `maxDepth` and `maxCollectionItems` resource bounds documented and enforced.
- [x] Provider-neutral structured data query contract with explicit fields, scalar filters, sorting, and bounded paging.
- [x] `HAgent.Example` verification for UI Context, UserControl, native `IList`, shared data relationships, custom control adaptation, application-object context, and structured data-query semantics.
