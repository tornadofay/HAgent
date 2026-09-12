# Resource Detail Inspection

## Purpose

The resource inventory identifies current authoritative resources. Resource detail inspection is the separate read boundary that resolves one selected inventory item into useful, bounded, human-readable content for management surfaces.

The boundary exists so WinForms, future WPF, and future ASP surfaces can read actual resource content without embedding provider-specific storage access or reusing editor/publish commands.

## Boundary

```text
IAiResourceInventory
        ↓ selected AiResourceInventoryItem
IAiResourceDetailSource
        ↓
AiResourceDetail
        ↓
management presentation
```

`IAiResourceDetailSource` is host/provider/storage-owned. Core owns only the provider-neutral detail projection and the read contract.

## Detail projection

`AiResourceDetail` contains:

- the selected `AiResourceInventoryItem` used to bind the detail to its logical resource;
- an optional summary;
- optional primary content;
- bounded name/value fields for type-specific metadata;
- bounded titled sections for structured resource content.

The projection is intentionally generic. Known resource types can expose specialized content while future resource types remain inspectable without new properties on the central Agent model.

The UI must validate that the returned detail still identifies the selected resource type and resource ID before displaying it.

## WinForms presentation

`Configuration → Authoritative Resources` remains the inventory page, but selecting a resource now exposes:

- aligned inventory metadata in a two-column property view;
- a readable Summary when supplied;
- a Content tab containing the actual bounded resource content;
- additional structured sections for type-specific information such as Skill inputs/steps or Knowledge tags.

The page remains read-only. It does not construct, mutate, publish, delete, archive, or authorize resources.

The page also guards against stale asynchronous selections: a detail response received for an earlier selection is not rendered over the currently selected resource.

## Type-specific behavior

Memory should expose its retained content and relevant memory metadata.

Knowledge/Wiki should expose readable content plus structured metadata such as version, source, tags, categories, or relationships when a source provides them.

Skill should expose its description and structured definition such as inputs, outputs, preconditions, steps, dependencies, and constraints.

These are presentation/projection responsibilities. They do not move resource-family persistence or publication logic into WinForms.

## Edit and delete boundary

Detail inspection is intentionally separate from mutation.

Editing an authoritative resource must not silently mutate a published version. The management path must eventually create a governed replacement/candidate or a new immutable version according to the resource family, then pass through the existing policy/authorization boundaries before authoritative publication.

Delete likewise must not become an unconditional UI CRUD command. Lifecycle-specific operations such as archive, retire, forget, quarantine, or replacement belong to explicit resource-management contracts. Reliability/adaptation concerns remain in phase 0.9576.

## Storage boundary

This contract does not add SQL Server or MySQL enumeration. A host-specific detail source may read from whatever storage implementation already exists, and later storage work may provide the corresponding source implementations.

The Example host uses a deterministic in-process detail source so the public contract and WinForms presentation can be verified without requiring a database.

## Verification

Focused unit coverage:

`HAgent.Tests/ResourceDetailInspectionTests.cs`

Canonical Example:

`HAgent.Example → Authoritative Resource Inventory`

WinForms management verification:

`Configuration → Authoritative Resources`

The Example verifies readable detail resolution for Memory, Knowledge, and Skill and exercises the same provider-neutral detail contract injected into the WinForms configuration composition.