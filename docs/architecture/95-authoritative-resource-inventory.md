# Authoritative Resource Inventory

## Purpose

The resource inventory is the provider-neutral management projection for authoritative HAgent resources. It exists so management surfaces can inspect Memory, Knowledge/Wiki, Skill, and future resource types through one contract without embedding storage-provider or UI-specific enumeration logic.

## Boundary

```text
provider/storage-specific source
            ↓
IAiResourceInventorySource
            ↓
AiResourceInventory
            ↓
AiResourceInventoryItem
            ↓
WinForms / future WPF / future ASP management surfaces
```

`IAiResourceInventorySource` owns enumeration for one host/provider/storage boundary. `AiResourceInventory` aggregates sources, validates projections, applies common filters, normalizes one logical resource identity, selects the highest available version, orders results deterministically, and enforces bounded paging.

The inventory contract deliberately does not perform create, update, delete, publish, archive, or authorization operations.

## Resource identity

Inventory identity is:

```text
resource type + resource id + scope + owner
```

Version is not part of logical identity. When several versions of the same logical resource are returned by sources, the inventory selects the highest version; where versions are unavailable it uses the newest authoritative/updated projection.

This makes the management inventory represent the current logical resource while preserving version-specific management operations for later workflows.

## Extensibility

`ResourceType` is a bounded string rather than a closed enum. Known types currently represented by HAgent include `memory`, `knowledge`, `wiki`, and `skill`, while future resource types can participate without adding properties to a central Agent model.

Lifecycle status is also represented as a bounded string because resource families can have different lifecycle vocabularies.

## Query boundary

`AiResourceInventoryQuery` supports:

- resource-type filtering;
- canonical scope filtering;
- owner filtering, suitable for agent/owner management views;
- bounded display-name/resource-ID search;
- lifecycle-status filtering;
- exact version filtering;
- bounded updated-time filtering through UTC lower/upper bounds;
- authoritative-only filtering;
- bounded result count with deterministic `SkipResults` paging.

Filtering is performed against inventory metadata, not serialized resource payloads. Paging is applied after source aggregation, logical-resource normalization, deterministic ordering, and metadata filtering so page boundaries remain stable for the same logical inventory snapshot.

## Storage boundary

Current HAgent contracts do not provide generic enumeration for every resource family. Memory already exposes provider-neutral `IMemoryStore.SearchAsync`, so `AiMemoryResourceInventorySource` adapts that existing contract into the common inventory projection without embedding File, SQL Server, or MySQL behavior in Core. The adapter treats the configured MemoryStore as authoritative, maps owner-bearing `MemoryScope` values to the canonical `AgentResourceScope`, derives `Published`/`Expired` lifecycle metadata from memory expiration, and projects `CreatedAt` as the available update timestamp. It remains bounded by the existing Memory search limit of 1000 records per source request.

Application-scoped Memory is not invented as a canonical Global resource because `MemoryEntry` requires an OwnerId. Owner-bearing User, Shared, Agent, Session, and Task scopes map to User, Workspace, Agent, Runtime, and Execution inventory scopes respectively.

Knowledge/Wiki previously exposed retrieval and publication contracts but no bounded authoritative enumeration boundary. The new provider-neutral `IAiKnowledgeResourceSource` + `AiKnowledgeEnumerationQuery` boundary supplies that missing read-only enumeration contract. `AiKnowledgeResourceInventorySource` adapts `AiKnowledgeResource` records into the common inventory projection and re-applies inventory metadata filters after source retrieval. It does not assume SQL Server, MySQL, files, or any other persistence mechanism; each host/storage implementation remains responsible for implementing `IAiKnowledgeResourceSource`.

Skill still exposes single-definition lookup through `IAiSkillDefinitionSource` but no generic authoritative enumeration boundary, so Skill inventory enumeration remains deferred. SQL Server/MySQL enumeration is not invented merely to populate the management UI.

Any future storage-specific enumeration adapter must implement `IAiResourceInventorySource` rather than changing the inventory contract.

## Management boundary

`Configuration → Authoritative Resources` consumes `IAiResourceInventory` through `ConfigurationContext`. The intended management interaction is a persistent master-detail workspace:

- a filter/action region for search, resource type, agent/owner, scope, lifecycle, version, updated-time window, authoritative-only selection, bounded page navigation, and refresh/reset;
- a persistent bounded resource list that remains visible while a resource is being inspected;
- a user-resizable splitter with the list starting near a 55/45 master/detail balance;
- a selected-resource detail pane containing tabs such as Overview and Content;
- read-only detail inspection through the separate `IAiResourceDetailSource` boundary.

The master list is not replaced by the detail tab control. The tabs belong inside the detail pane so long-running hosts with many agents/resources retain continuous discovery and selection context.

The management surface uses bounded pages rather than attempting to render an unbounded resource inventory in one visual list. Current paging is offset-based and deterministic; future hosts can optimize enumeration without changing the management semantics.

Inventory remains a read model and does not perform mutation. Detail inspection also never grants edit, publish, delete, archive, or authorization capability.

The canonical Example now composes a real provider-neutral `InMemoryMemoryStore` through `AiMemoryResourceInventorySource`, a provider-neutral Example implementation of `IAiKnowledgeResourceSource` through `AiKnowledgeResourceInventorySource`, and a deterministic Skill projection. This demonstrates the separate source boundaries without requiring SQL Server/MySQL enumeration, and does not turn the Example into a storage implementation for production hosts.

Future WPF or ASP configuration surfaces should consume the same inventory and detail contracts and retain the same resource semantics; only presentation and host composition should differ.

Resource-specific editors and lifecycle/CRUD commands remain subsequent management increments. Authoritative resources must not be silently mutated in place; future editing must produce a governed candidate or new immutable version appropriate to the resource family. Reliability/adaptation lifecycle operations remain separate for 0.9576.

The detail-specific architecture is defined in `docs/architecture/96-resource-detail-inspection.md`.

## Verification

The focused inventory test contract is `HAgent.Tests/ResourceInventoryTests.cs`.

The focused detail test contract is `HAgent.Tests/ResourceDetailInspectionTests.cs`.

The focused Memory-source contract is `HAgent.Tests/MemoryResourceInventorySourceTests.cs`.

The focused Knowledge-source contract is `HAgent.Tests/KnowledgeResourceInventorySourceTests.cs`.

The canonical Example is:

`HAgent.Example → Authoritative Resource Inventory`

The WinForms management verification path is:

`Configuration → Authoritative Resources`

The Example contract scenario verifies real `IMemoryStore` → inventory projection, provider-neutral Knowledge enumeration → inventory projection, deterministic Skill projection, authoritative-only filtering, type/search filtering, lifecycle/version/updated/owner filtering, deterministic paging, version normalization, bounded results, and readable detail inspection. The configuration page consumes the same provider-neutral inventory/detail boundaries and exposes read-only management information without storage/provider-specific enumeration assumptions.
