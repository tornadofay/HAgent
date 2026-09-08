# Context Engineering

Context is bounded information made available to an HAgent execution by the host and approved HAgent resource/runtime boundaries. Context engineering turns available information into a bounded, provider-neutral evidence set for an execution. It is not prompt concatenation and it is not cognitive decision making.

## Context boundary

A host may expose information from any environment through generic context mechanisms. Context may represent observations, state snapshots, events, records, objects, resources, structured data, conversation turns, memory, knowledge, skills, tool descriptions, or other execution-relevant information.

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

When candidate context exceeds budget, the subsystem may deduplicate, summarize/compact, or truncate according to an explicit strategy.

Compaction must preserve required authority/policy information and provenance. It must not silently replace a higher-authority instruction with a lower-authority summary, erase source identity, or convert uncertainty into fact.

Provider adapters may provide tokenizer-aware estimates or provider-specific compaction assistance, but Core remains functional with tokenizer-free estimates and deterministic bounded strategies.

## Existing context mechanisms

The existing generic `AgentExecutionRequest.HostContext` dictionary is a bounded host convenience input. It remains useful as an input form while 0.955 evolves, but is not the canonical multi-source context model.

The existing `ConversationContextBuilder` is a legacy conversation-history limiter. It proves bounded conversation selection but does not provide the 0.955 context-item, provenance, multi-source, ranking, or compaction architecture.

The existing WinForms `IUiContext` / `WinFormsUiContext` surface is a host adapter boundary. It already supplies bounded inspection, data reads, projections, discovery, and application-object exposure. It must feed the generic context subsystem rather than move WinForms types into HAgent.Core.

These mechanisms are implementation evidence and producer/adaptor boundaries, not competing canonical context architectures.

## Execution integration

The result of context engineering is a bounded context snapshot associated with one execution. The assembled snapshot is immutable from the execution's point of view so later provider configuration, agent configuration, resource state, or host collection changes cannot mutate already-running work.

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

## Reuse and caching

Context sources and reusable context components may be cached when their scope, ownership, configuration version, resource version, and freshness rules permit it.

Caching must not leak private context across runtimes, users, tenants, or workspaces. Cache keys must reflect the ownership and validity boundaries that affect the represented data.

The assembled execution snapshot is never a mutable shared cache entry.

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
Bounded Context Snapshot
        ↓
Execution Request / Provider Adapter
```

Context engineering constructs bounded evidence. Cognition decides what matters. The provider adapter decides how the bounded result is transported. The host remains authoritative over the environment itself.
