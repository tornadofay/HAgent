The durable candidate boundary is now closed. It remains separate from authoritative resource publication.

### Slice 10 — Authoritative Promotion + Version-Safe Resource Creation — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 Example succeeded.
- .NET 9 Example succeeded.
- Both Examples verified Memory promotion, creation of a new published Knowledge version, creation of a new immutable Skill version, fresh unified promotion authorization, publication-before-lifecycle transition, provenance evidence, and no mutation of existing published Knowledge or Skill versions.
- Full `HAgent.Tests`: **205/205 passed, 0 failed, 0 skipped** on .NET 9.

The authoritative promotion boundary is now closed.

### Slice 11 — Context, Instruction, Runtime, and Observability Integration — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 Example succeeded.
- .NET 9 Example succeeded.
- Both Examples verified provider-neutral learned instruction, policy/capability-gated learned context, bounded execution context snapshots, authoritative runtime outcome observation capture, non-creation of candidates from observations, and non-authoritative prompt text.
- Full `HAgent.Tests`: **208/208 passed, 0 failed, 0 skipped** on .NET 9.

The execution-integration boundary is now closed.

### Slice 12 — Learning Review management UI — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 real WinForms configuration flow succeeded.
- .NET 9 real WinForms configuration flow succeeded.
- Durable candidate-store reopen was verified on both targets.
- Manual workflow verified: `Learning Review Seed` → `Configuration → Learning Review` → inspect/filter → Approve → Promote → `Learning Review Verify`.
- Final candidate status was `Promoted` with lifecycle revision `3` on both targets.
- Reviewer identity evidence persisted.
- Review/promotion authorization evidence persisted with outcome `Allow`.
- The management surface reused the existing provider-neutral `AiLearningPromotionService`; WinForms did not directly publish authoritative resources.

The complete Learning Review management boundary is now closed.

### Current Slice 12 management increment — Authoritative Resource Inventory + Detail Inspection

This increment remains inside 0.9575 and establishes the shared inventory foundation, a scalable master-detail WinForms inventory surface, and provider-neutral read-only detail inspection.

### Implemented

- `IAiResourceInventorySource` for host/provider/storage-specific enumeration;
- `IAiResourceInventory` for the shared management-facing read boundary;
- bounded `AiResourceInventoryQuery` and `AiResourceInventoryItem` contracts;
- lifecycle, exact-version, updated-time, owner/agent, scope, type, search, and authoritative-only query filters;
- deterministic aggregation, filtering, authoritative-only selection, logical-resource deduplication, highest-version selection, ordering, and `SkipResults` + `MaxResults` paging;
- extensible string resource types so future resource families do not require central Agent model changes;
- provider-neutral `AiMemoryResourceInventorySource` adapting the existing `IMemoryStore.SearchAsync` contract without adding storage-specific enumeration logic;
- provider-neutral `IAiKnowledgeResourceSource` + bounded `AiKnowledgeEnumerationQuery` read boundary;
- `AiKnowledgeResourceInventorySource` adapting Knowledge/Wiki resources into the common inventory projection without embedding storage-specific enumeration;
- no Skill storage enumeration implementation because its current contract exposes lookup but not generic authoritative enumeration;
- focused unit coverage in `HAgent.Tests/ResourceInventoryTests.cs` including filter and paging contracts;
- focused Memory-source coverage in `HAgent.Tests/MemoryResourceInventorySourceTests.cs`;
- focused Knowledge-source coverage in `HAgent.Tests/KnowledgeResourceInventorySourceTests.cs`;
- dedicated Example scenario `HAgent.Example → Authoritative Resource Inventory` now exercises a real provider-neutral `InMemoryMemoryStore` through the Memory inventory adapter plus a provider-neutral Example Knowledge source through the Knowledge inventory adapter and a deterministic Skill projection;
- WinForms `Configuration → Authoritative Resources` page consuming the inventory and detail contracts;
- persistent master resource list that remains visible while inspecting a selected resource;
- user-resizable SplitContainer starting near a 55/45 list/detail balance rather than a fixed detail width;
- filter/action region for Search, Resource type, Agent/Owner, Scope, Lifecycle, Version, Updated window, Authoritative-only, Apply/Reset/Refresh, and bounded page navigation;
- paging controls placed beneath the master list and omitted when the result set fits on one page;
- Overview and Content tabs confined to the selected-resource detail pane rather than replacing the master list;
- stale asynchronous detail-result protection so an earlier selection cannot overwrite the current selection;
- edit/delete/publish/archive/retire operations remain outside the read-only detail boundary.

### Remaining work

- connect real storage-backed Knowledge/Wiki and Skill authoritative sources through resource-specific contracts where host persistence exists, without inventing provider-specific behavior in Core;
- add a supported Skill authoritative enumeration boundary before adding its inventory adapter;
- expose effective agent-resource visibility without duplicating authoritative resource models;
- extend the management surface with governed resource-specific editing/version-creation workflows;
- add appropriate governed lifecycle operations rather than unconditional CRUD/delete behavior;
- consider provider-side paging/virtualization optimizations only when a concrete storage source requires them;
- keep reliability/adaptation separate for 0.9576.

User verification already reported for this management slice: full `HAgent.Tests` **222/222 passed, 0 failed, 0 skipped** on .NET 9 after clean rebuild. The earlier xUnit metadata-cache runner anomaly did not recur in that clean run. The new Knowledge enumeration increment is not yet locally verified by the user.

Architecture: `docs/architecture/93-learning-review-management-ui.md`, `docs/architecture/94-learning-review-candidate-details.md`, `docs/architecture/95-authoritative-resource-inventory.md`, and `docs/architecture/96-resource-detail-inspection.md`.

**Tests to run:** `HAgent.Tests → KnowledgeResourceInventorySourceTests.cs` (focused), then the full `HAgent.Tests` suite at the management-slice checkpoint.

**Example to run:** `HAgent.Example → Authoritative Resource Inventory` on .NET Framework 4.8.1 and .NET 9; then open `Configuration → Authoritative Resources` on both targets, resize the master/detail splitter, exercise the filters and page navigation, and select Memory, Knowledge, and Skill resources to verify the aligned Overview and readable Content views.
