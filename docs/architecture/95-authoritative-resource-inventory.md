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

`IAiResourceInventorySource` owns enumeration for one host/provider/storage boundary. `AiResourceInventory` aggregates sources, validates projections, applies common filters, normalizes one logical resource identity, selects the highest available version, orders results deterministically, and enforces the requested result bound.

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
- owner filtering;
- bounded display-name/resource-ID search;
- authoritative-only filtering;
- bounded result count.

Filtering is performed against inventory metadata, not serialized resource payloads.

## Storage boundary

Current HAgent contracts do not provide generic enumeration for every resource family. Memory has store search capabilities, while Knowledge and Skill currently expose retrieval/lookup and publication contracts. This inventory slice therefore establishes the common source boundary without inventing SQL Server/MySQL enumeration behavior or forcing provider-specific storage into Core.

Storage-specific enumeration adapters are a later storage concern and must implement `IAiResourceInventorySource` rather than changing the inventory contract.

## Management boundary

`Configuration → Authoritative Resources` consumes `IAiResourceInventory` through `ConfigurationContext`. The page now provides:

- type, scope, search, and authoritative-only filtering;
- a deterministic list of current logical resources;
- aligned inventory metadata for the selected resource;
- read-only detail inspection through the separate `IAiResourceDetailSource` boundary;
- a readable Content view plus bounded type-specific fields/sections when the host supplies them.

Inventory remains a read model and does not perform mutation. Detail inspection also never grants edit, publish, delete, archive, or authorization capability.

The Example host injects deterministic provider-neutral inventory and detail sources into the configuration composition so the management surface can be exercised without requiring SQL Server/MySQL enumeration. This is Example verification data, not a storage implementation.

Future WPF or ASP configuration surfaces should consume the same inventory and detail contracts and retain the same resource semantics; only presentation and host composition should differ.

Resource-specific editors and lifecycle/CRUD commands remain subsequent management increments. Authoritative resources must not be silently mutated in place; future editing must produce a governed candidate or new immutable version appropriate to the resource family. Reliability/adaptation lifecycle operations remain separate for 0.9576.

The detail-specific architecture is defined in `docs/architecture/96-resource-detail-inspection.md`.

## Verification

The focused inventory test contract is `HAgent.Tests/ResourceInventoryTests.cs`.

The focused detail test contract is `HAgent.Tests/ResourceDetailInspectionTests.cs`.

The canonical Example is:

`HAgent.Example → Authoritative Resource Inventory`

The WinForms management verification path is:

`Configuration → Authoritative Resources`

The Example contract scenario verifies deterministic Memory/Knowledge/Skill projections, authoritative-only filtering, type/search filtering, version normalization, bounded results, and readable detail inspection. The configuration page consumes the same provider-neutral inventory/detail boundaries and exposes read-only management information without storage/provider-specific enumeration assumptions.