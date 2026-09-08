# Context Engineering

Context is bounded information made available to an HAgent execution by the host and approved HAgent resource/runtime boundaries. Context engineering turns available information into a bounded, provider-neutral evidence set for an execution. It is not prompt concatenation and it is not cognitive decision making.

## Context boundary

A host may expose information from any environment through generic context mechanisms. Context may represent observations, state snapshots, events, records, objects, resources, conversation turns, memory, knowledge, skills, tool descriptions, or other execution-relevant information.

HAgent must not require the host to convert every item to a plain string. Structured values remain structured until a provider-facing representation is required.

The host remains authoritative over domain meaning, lifecycle, authorization, and side effects. HAgent may normalize, rank, bound, project, serialize, compact, and explain context without inventing host-domain semantics.

## Context item

The canonical provider-neutral unit is a context item. An item carries the information needed to make bounded selection decisions without turning the item into an instruction or an authorization grant.

A context item contains, at minimum:

```text
Id
Source / source identifier
Type / producer-supplied category
Payload / structured or textual value
Provenance
Trust
Importance
Freshness / captured or effective time
Scope / ownership context
Estimated size
```

The payload may be structured rather than text. A context item may reference a host object or resource through an adapter-defined representation, but a live object reference never implies executable access.

`Type` describes producer-supplied information. It does not grant a capability or tell HAgent what the domain object means. `Trust` and provenance describe evidence quality and origin; they do not substitute for authorization.

## Provenance and scope

Every selected item retains enough provenance to explain where it came from and why it was eligible. Provenance may identify the producer, resource, version, capture time, execution/runtime identity, and other bounded evidence appropriate to the source.

Context scope follows the canonical HAgent ownership model. An item may be global, tenant-, user-, workspace-, agent-, runtime-, or execution-scoped. Private resources must never become visible merely because a producer discovered them.

Identity is carried as structured execution context, not embedded into prompt text.

## Context budgets

Context assembly is governed by an explicit budget. At minimum the budget supports independent limits for:

```text
maximum estimated tokens
maximum characters
maximum items
```

Additional dimensions may be introduced when a source or provider requires them, but Core must not depend on a specific tokenizer or vendor accounting model.

Budgets are hard bounds for the assembled result unless an explicit future policy declares another behavior. An estimate is evidence for selection; it is not a provider-verified token count unless a tokenizer/provider adapter supplied that evidence.

## Acquisition, retrieval, and assembly

The context subsystem separates these responsibilities:

```text
available sources
    -> acquisition / retrieval
    -> policy + permission filtering
    -> ranking / deduplication
    -> compaction / truncation
    -> bounded assembly
```

Acquisition obtains candidate information. Retrieval selects information from a source according to an explicit query and bounds. Assembly decides the final bounded context presented to an execution.

The Core acquisition boundary is `IContextSource`, which exposes only a bounded `ContextSourceRequest` and returns provider-neutral `ContextItem` candidates. `IContextAcquirer` consumes sources in the caller-supplied order, passes each source an isolated request copy, enforces the configured item/character/token limits, propagates cancellation, and produces an execution-owned `ContextSnapshot`. Acquisition does not authorize a source, rank candidates, deduplicate them, compact them, or contact a provider.

When a hard estimated-token budget is configured, a candidate without a token estimate is not admitted because its cost cannot be proven to fit the hard bound. When no token budget is configured, known token usage may still be reported while remaining token capacity stays unspecified.

Cognitive attention is a separate decision. Cognition may identify goals, focus, or required evidence; context engineering translates those decisions into bounded evidence. Context engineering must not become an implicit second cognitive planner.

## Source types

The common pipeline must be able to consume, when enabled by the host and applicable policy:

- conversation history;
- working/long-term memory;
- Knowledge/Wiki resources;
- Skills and relevant descriptions;
- tool definitions/descriptions;
- instruction sources governed by the instruction subsystem;
- explicit host context;
- application observations and structured state;
- data records/projections/query results;
- supported live application objects exposed through bounded inspection;
- future provider-neutral resource types.

These remain separate producer boundaries. A source does not bypass policy merely because it is represented as a context item.

## Policy and authority

Context engineering must respect policy, permissions, resource capability state, and instruction authority.

Discovery is evidence, not authorization. A discovered control, data source, object, memory entry, knowledge item, Skill, or tool description does not become executable or readable merely because it was discovered.

Instruction authority and context usefulness are different concepts. Instruction sources remain governed by instruction governance. Context items may carry trust/provenance and may originate from an instruction source, but ordinary evidence must not be silently promoted into an instruction or authority boundary.

## Ranking and relevance

Candidate ranking should be deterministic for equal inputs and should be able to account for:

```text
goal relevance
attention/focus
recency / freshness
importance
trust
redundancy
estimated cost
source policy
```

Ranking is an assembly concern. It may use host/cognitive hints, but remains provider-neutral and must not infer domain meaning that the host did not provide.

Tie-breaking must be deterministic so Examples and tests can reproduce inclusion/exclusion decisions.

## Compaction and truncation

Compaction is a separate bounded assembly stage applied after ranking/deduplication. The canonical Core strategy is deterministic truncation: it walks the supplied ranked order and admits only candidates that fit the target item/character/token budget. A candidate that does not fit may be skipped so a later smaller candidate can still be admitted, unless the caller explicitly selects stop-on-exclusion behavior.

The initial Core strategy does not rewrite payloads, perform semantic summarization, or invent provider-specific token counts. It therefore remains usable without an LLM or tokenizer. Future compaction strategies may provide actual summarization/compression through a separate strategy boundary without changing the canonical context item or snapshot model.

A hard token budget rejects candidates with unknown token estimates because their admission cannot be proven safe. When no hard token budget exists, selected items may retain unknown token usage rather than fabricating an exact count.

Compaction produces an execution-owned `ContextSnapshot` plus optional safe `ContextCompactionDecision` diagnostics. Diagnostics report only bounded metadata such as candidate index, item ID, source, estimated size, prior usage, inclusion state, output position, and a deterministic reason. Payload content is not copied into diagnostics, so explainability does not itself expand the disclosure surface.

Selected context items are cloned before snapshot construction and retain their original provenance, scope, quality metadata, and payload representation. Compaction never silently changes the semantic identity or provenance of a selected item.

## Reuse and caching

Reusable context components may be cached independently of execution assembly when their validity can be expressed explicitly. The canonical reusable cache identity is `ContextCacheKey`, which includes:

```text
component identity
scope type + scope id
configuration version
resource version
freshness version
```

The cache key is an ownership/validity boundary, not merely a content hash. A change in scope, configuration version, resource version, or freshness version produces a distinct cache identity rather than reusing an older private result. Cache entries also have an explicit expiration boundary for time-based freshness.

`IContextCache` is a provider-neutral cache contract. The current Core implementation is `InMemoryContextCache`, which is thread-safe and stores reusable `ContextSnapshot` instances. Cache invalidation may be explicit or caused by expiration; version/freshness changes naturally miss the old entry because they use a different key.

Reusable cached context must never become a mutable execution-owned shared snapshot. The `ContextSnapshot` model remains the canonical bounded result, and its defensive metadata access prevents callers from changing cached item state through returned item copies. Cache implementations must also retain the ownership boundaries encoded by the key when they are replaced by future persistent/distributed implementations.

Caching must not leak private context across runtimes, users, tenants, or workspaces. Cache keys must reflect the ownership and validity boundaries that affect the represented data.

## Existing context mechanisms

The existing generic `AgentExecutionRequest.HostContext` dictionary is a bounded host convenience input. It remains useful as an input form while 0.955 evolves, but is not the canonical multi-source context model.

The existing `ConversationContextBuilder` is a legacy conversation-history limiter. It proves bounded conversation selection but does not provide the 0.955 context-item, provenance, multi-source, ranking, or compaction architecture.

The existing WinForms `IUiContext` / `WinFormsUiContext` surface is a host adapter boundary. It already supplies bounded inspection, data reads, projections, discovery, and application-object exposure. It must feed the generic context subsystem rather than move WinForms types into HAgent.Core.

These mechanisms are implementation evidence and producer/adaptor boundaries, not competing canonical context architectures.

## Execution integration

The result of context engineering is a bounded context snapshot associated with one execution. The assembled snapshot is immutable from the execution's point of view so later provider configuration, agent configuration, resource state, or host collection changes cannot mutate already-running work.

The current Core snapshot contract is `ContextSnapshot`. It defensively copies the selected items and budget at construction and returns defensive item/budget copies to callers. It records the selected item count, character usage, estimated-token usage when knowable, remaining budget dimensions, source count, creation time, and contract version. The snapshot is not a mutable shared cache entry and is not a provider transport representation.

The snapshot preserves, at minimum:

```text
selected context items
budget used / remaining
selection and compaction evidence where safe
source/provenance metadata
creation/version information
```

The provider-facing adapter may transform this snapshot into messages or another transport structure, but that transport representation is not the canonical context model.

## Diagnostics and explainability

Context decisions should be explainable without exposing secrets or sensitive payloads by default. Safe diagnostics should be able to report metadata such as:

```text
included item
excluded item
reason / policy decision
estimated size
source type
priority/relevance evidence
compaction or truncation action
```

Payload redaction follows the global observability/security rules. Explainability must never become an authorization bypass or secret-disclosure mechanism.

## Provider portability

Core never requires a provider-specific tokenizer. Provider adapters may supply token estimates, capabilities, limits, or provider-native context transport. Unknown token accounting remains explicitly unknown rather than being treated as exact.

The same context model must work for providers that accept chat messages, structured content, text prompts, or future transports.

## Canonical pipeline

```text
Available information
        ↓
Policy + permissions
        ↓
Attention / relevance hints
        ↓
Acquisition / retrieval
        ↓
Ranking / deduplication
        ↓
Compression / compaction / truncation
        ↓
Reusable cache where scope/version/freshness permits
        ↓
Bounded Context Snapshot
        ↓
Execution Request / Provider Adapter
```

Context engineering constructs bounded evidence. Cognition decides what matters. The provider adapter decides how the bounded result is transported. The host remains authoritative over the environment itself.
