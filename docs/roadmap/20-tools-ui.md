# Active and near-term roadmap

## 0.5 Tools + Agent Loop

Tool definitions, validation, persistence, per-agent assignment, provider tool transport, deterministic tool loops, and live Groq tool calling are implemented.

Remaining tool hardening:

- [ ] Per-session temporary tools.
- [ ] Built-in tool handlers.
- [ ] Declarative execution engine.
- [ ] Tool aliases/versioning.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool audit/history and budgets.
- [ ] Stronger loop detection and provider/tool capability negotiation.

## 0.6 Safety + Permissions

The permission model is a shared authorization concept, not just a WinForms checkbox collection.

- [x] General permission configuration UI.
- [ ] Read/write/invoke/export permissions across all tool categories.
- [ ] Host authorization callbacks.
- [ ] Human approval lifecycle.
- [ ] Input/output/tool guardrails.
- [ ] Budgets and observability.
- [ ] Sensitive-data redaction.

The first WinForms policy UI persists coarse permissions with safe defaults. Database-specific permissions and approval workflows remain separate work.

## 0.7 WinForms UI Context + Data Discovery — COMPLETE

The public concept is **UI Context / Control Adapters**. “Form serialization” is only one possible implementation technique inside a broader system.

Completed capabilities include:

- Form and arbitrary control-tree/UserControl attachment with stable root identity.
- Read-only inspection and bounded control/data reads.
- Semantic control discovery.
- Bound/native data-source discovery for `DataTable`, `DataView`, `BindingSource`, `IList`, arrays, and compatible collections.
- CurrencyManager/current-item/position/count relationship metadata.
- Control-to-source relationship discovery based on actual bindings.
- Convention-based application control adapters, including external `IHyperControl`-style controls.
- Live application-object attachment and bounded structural discovery.
- `maxDepth` and `maxCollectionItems` limits.
- Provider-neutral structured data-query contract: fields, scalar filters, sorting, and bounded paging without SQL or executable expressions.
- Local `HAgent.Example` verification of the complete 0.7 slice.

## 0.8 Data Access + Authorization

This is the next major platform milestone. The goal is to convert the verified discovery/query contracts into safe, real application and database data access.

### Application data

- [ ] Application-owned data adapter implementing `IDataQuerySource`.
- [ ] Schema/field allow-list independent of model requests.
- [ ] Query authorization by source/table/field/operation.
- [ ] Projection, query, export, and write permissions separated.
- [ ] Query limits, cancellation, timeout, and resource budgets.

### SQL Server / MySQL

- [ ] Restricted SQL Server query adapter using generated parameterized commands only.
- [ ] Restricted MySQL query adapter using generated parameterized commands only.
- [ ] Schema discovery restricted to explicitly authorized databases/schemas.
- [ ] No arbitrary SQL tool.
- [ ] Read-only database operations before write operations.
- [ ] Database operation audit metadata and correlation IDs.

### Live Example verification

When the SQL adapter is ready, `HAgent.Example` should expose temporary connection fields for an explicitly disposable/read-only test database:

```text
Server Name
User Name
Password
Database
```

These are runtime test inputs only. They must not be persisted as agent/tool configuration or written to normal Example output/logging. The Example should verify connection, schema allow-listing, structured query execution, bounded results, cancellation/timeout, and rejection of unauthorized fields/operations.

### Authorization and safety

- [ ] Host authorization callbacks.
- [ ] Explicit approval lifecycle for sensitive database operations.
- [ ] Sensitive-field redaction policies.
- [ ] No authorization inferred from UI binding, object provenance, table metadata, or model instructions.

## 0.9 UI Automation + Agent Scope + Chat

UI write/invoke behavior should begin only after the 0.8 authorization foundation is established.

- [ ] `ui.write_control`.
- [ ] `ui.invoke` / approved click.
- [ ] Move/resize/enable/disable operations.
- [ ] Batch operations.
- [ ] Dry-run/preview.
- [ ] Human approval.
- [ ] Per-control permissions.
- [ ] Undo/rollback hooks where hosts support them.
- [ ] Agent profile separated from runtime binding/lifetime.
- [ ] Application/global, form, session, task, and ephemeral scopes.
- [ ] User ↔ agent chat with global/form agent selector.
- [ ] Persistent conversations and conversation switching/search.
- [ ] Streaming UI and tool activity visualization.
- [ ] Reasoning visibility policy.
- [ ] Cancel/stop and simultaneous conversations.
- [ ] Cross-form memory governed by explicit scope and authorization policy.

## Cross-platform UI direction

The same UI-context concepts should later be available through adapters for:

- HControl/BaseForm and custom controls.
- GDI-rendered objects and scenes.
- DirectX interactive objects.
- Unity components/scenes.
- Other interactive application surfaces.

These platform implementations remain outside `HAgent.Core`.

## Data representation rule

Always use the lightest representation that preserves the required information. Prefer bound/native sources, lazy adapters, projections, paging, and streaming. Avoid unnecessary copying/materialization. `DataTable` is valid when naturally present or actually useful, but it is never the mandatory representation.

## Example developer experience

Every meaningful Example feature should provide:

- editable input/message when the capability has meaningful user input;
- expected behavior and explanation;
- copyable C# reproduction snippet beside the input;
- global agent selection where an agent is involved;
- a global output area when the result can be shared;
- a self-contained setup snippet or clearly identified shared setup section.

SQL connection fields are a future live-integration Example feature, not part of the provider-neutral Core contract.
