# Completed implementation ledger

Only completed work belongs here. Future work is not listed as planned items.

## 0.1 Foundation

- Provider/agent configuration models.
- OpenAI-compatible provider foundation.
- File, SQL Server, and MySQL persistence foundations.
- Protected local secrets.
- Provider/agent/tool management UI.
- HAgent.Example integration host.

## 0.2 Runtime

- Stable execution IDs and lifecycle state.
- Provider routing and fallback candidates.
- Retries, timeout, and cancellation.
- Provider error classification and actionable failures.
- Execution snapshots.
- System-prompt resolution.

## 0.3 Memory + Context

- Persistent JSONL memory.
- Explicit remember/recall/forget.
- Agent/task/event/fact/preference memory records.
- Persistent conversations and sessions.
- Context budgets and tokenizer-free estimation.
- Conservative automatic memory.
- Lightweight relevance ranking.
- Episodic memory with provenance.

## 0.4 Capabilities + Response Normalization

- Tri-state provider/model capabilities with evidence.
- Capability caching.
- Normalized text, reasoning, structured output, tool calls, usage, and provider metadata.
- Reasoning separation and diagnostic `<think>` handling.
- Provider error advice.
- Streaming contract, OpenAI-compatible SSE, cancellation, and live streaming verification.

## 0.5 Tool Foundation

- BuiltIn, Application, Declarative, UI, SqlServer, and MySql tool types.
- Definition/handler separation.
- Tool registry and application handlers.
- JSON Schema validation.
- Provider tool-definition transport.
- Bounded multi-turn tool loop.
- Tool-definition persistence.
- Agent `ToolIds` assignment.
- Live Groq tool-loop verification.

## 0.7 UI Context + Data Discovery

- Form and arbitrary WinForms control-tree/UserControl attachment.
- Stable logical root identity.
- UI-thread-safe read-only inspection and control reads.
- Native/bound `DataGridView` data extraction without mandatory `DataTable` normalization.
- Standard semantic control discovery.
- Data-source discovery for DataTable, DataView, BindingSource, IList, arrays, and compatible collections.
- CurrencyManager/current-item/position/count metadata.
- Control-to-source relationship discovery.
- Convention-based custom control adapters.
- External `IHyperControl`-style adaptation without assembly dependency.
- Live application-object attachment and bounded structural discovery.
- `maxDepth` and `maxCollectionItems` resource bounds.
- Provider-neutral structured data projection/query contracts.
- Verified HAgent.Example coverage for the complete 0.7 slice.

## Verification rule

A completed milestone is based on actual local verification, not merely code existence.
