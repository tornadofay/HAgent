# HAgent Example Host Maintenance Guide

## Purpose

`HAgent.Example` is the manual developer/verification host. Its UI is organized by architecture and capability, not by implementation file or class name.

The top-level Example tabs are **feature groups**. Most feature groups contain one nested `TabControl`; each child tab represents one independently runnable example scenario. When a feature becomes large enough, it may use a second grouping layer: **Feature → Sub-area → Example**. This is currently used for Runtime and Context because those areas contain several distinct architectural concerns.

This structure is required so the Example host can grow without becoming a single flat list of unrelated tabs.

## UI structure

```text
HAgent Example
├── Core
│   ├── Messaging
│   ├── Session
│   └── Persistent Session
├── Memory
│   ├── Memory
│   ├── Automatic Memory
│   └── ...
├── Context
│   ├── Context Core
│   │   └── Context Budget
│   ├── UI Context
│   │   ├── UI Context
│   │   ├── UI Context UserControl
│   │   ├── UI Native IList
│   │   ├── UI Data Relationships
│   │   ├── UI Custom Control Adapter
│   │   └── Application Object Context
│   └── Data Access Context
│       └── Data Query Contract
├── Tools
├── Providers
├── Policy
├── Events
├── Identity
│   ├── Identity Snapshot
│   ├── Identity Execution
│   ├── Identity Tool
│   └── Identity Isolation
├── Runtime
│   ├── Runtime Instances
│   │   ├── Runtime Instances
│   │   ├── RUNTIME OVERRIDES
│   │   ├── RUNTIME SHUTDOWN
│   │   ├── RUNTIME SCHEDULING
│   │   └── RUNTIME CONCURRENCY
│   ├── Execution
│   │   ├── Runtime Execution
│   │   ├── RUNTIME TERMINAL STATE
│   │   └── RESOURCE CAPABILITY
│   ├── Intervention
│   │   ├── EXECUTION INTERVENTION
│   │   └── INTERVENTION HARDENING
│   ├── Planning & Capacity
│   │   ├── Execution Target Planning
│   │   ├── EXECUTION TARGET CATALOG
│   │   └── Quota Admission
│   └── Diagnostics
│       └── Execution Audit
├── Workspace
├── Cognition
├── Configuration
└── Diagnostics
```

The exact child examples evolve with the implementation. The architectural grouping is the stable concern. A second grouping level should be introduced only when a feature contains multiple coherent sub-areas; do not add another level merely because a tab count is high.

## Canonical implementation

The grouping shell is implemented in:

`src/HAgent.Example/MainForm.ExampleOrganization.cs`

Important members:

- `ExampleFeatureOrder` defines the order of top-level feature groups.
- `OrganizeExampleTabs()` moves the existing child `TabPage` instances into the appropriate feature group and creates the nested `TabControl`.
- `RequiresExampleSubGroups(string group)` identifies feature groups that need a second organization level.
- `CreateExampleSubGroups(...)` creates the sub-area tabs and their child example tabs.
- `GetExampleSubGroup(string title)` maps an example title to the appropriate sub-area.
- `GetExampleFeatureGroup(string title)` maps an example title to its architecture-level feature group.
- `NormalizeExampleTabContentLayouts()` applies the shared content layout correction after grouping.
- `MainForm.OnLoad` must add all Example tabs that participate in grouping before calling `OrganizeExampleTabs()`.

Individual test implementations remain in focused partial files such as `MainForm.IdentityTests.cs`, `MainForm.PolicyTests.cs`, `MainForm.ExecutionPlannerTests.cs`, and related files. The organization shell should not contain test implementation logic.

## Adding a new top-level feature group

Create a new group only when the examples represent a meaningful architecture/capability boundary. Do not create a group for one class, one provider, or one implementation detail.

1. Add the group name to `ExampleFeatureOrder` at the desired position.
2. Add a title-to-group rule in `GetExampleFeatureGroup` for the child example titles that belong to that group.
3. Keep the new examples in focused partial files/components rather than putting their test implementation into `MainForm.ExampleOrganization.cs`.
4. Add or register the child tabs before `OrganizeExampleTabs()` runs.
5. Verify the new group is visible as a top-level tab and its examples appear as nested child tabs.

Example:

```csharp
private static readonly string[] ExampleFeatureOrder =
{
    "Core",
    "Memory",
    "Context",
    "Tools",
    "Providers",
    "Policy",
    "Events",
    "Identity",
    "Runtime",
    "Workspace",
    "Cognition",
    "New Feature",
    "Configuration",
    "Diagnostics"
};
```

Then classify its child titles explicitly:

```csharp
if (key == "NEW FEATURE CONTRACT" || key == "NEW FEATURE PERSISTENCE")
    return "New Feature";
```

## Adding a second-level sub-area

Use a sub-area only when one top-level feature contains several distinct architectural concerns that are easier to understand separately. Runtime and Context are the current examples.

1. Add the sub-area name to the relevant `subgroupOrder` array in `CreateExampleSubGroups(...)`.
2. Add explicit title rules in `GetExampleSubGroup(string title)`.
3. Keep the sub-area meaningful in architectural terms, not merely a UI grouping such as "Other" or "Miscellaneous".
4. Ensure the child examples remain independently runnable focused tabs.
5. Keep the number of hierarchy levels bounded. The intended maximum is currently **three levels: Feature → Sub-area → Example**.

Do not introduce a fourth navigation level without an explicit architectural reason.

## Adding a child example to an existing group

Do not edit the grouping shell just because a child example is added when the existing classification rule already handles it.

1. Implement the example in a focused partial file.
2. Register it with the normal `AddApiTab(...)`, specialized tab method, or feature-specific registration method.
3. Give it a stable, descriptive title that makes its architecture boundary obvious.
4. Ensure the registration occurs before `OrganizeExampleTabs()`.
5. Confirm `GetExampleFeatureGroup` classifies the title into the intended existing group.
6. When the feature uses sub-areas, also confirm `GetExampleSubGroup` classifies it into the intended sub-area.
7. Keep the example independently runnable and deterministic where the capability permits deterministic verification.

Prefer explicit title rules over broad accidental matches when a name could belong to more than one architecture area.

## Editing an existing example

Edit the focused partial file that owns the example. For example:

```text
MainForm.IdentityTests.cs
MainForm.PolicyTests.cs
MainForm.ExecutionInterventionHardeningTests.cs
MainForm.ExecutionPlannerTests.cs
```

Do not move test logic into `MainForm.ExampleOrganization.cs`. That file owns presentation grouping only.

Changing an example title may change its group. When a title changes, re-check both `GetExampleFeatureGroup` and `GetExampleSubGroup` where applicable, and update the documentation only when the architecture classification changes.

## Removing an example

Remove the child example's registration and obsolete implementation from its focused partial file. Do not remove the parent feature group merely because one child was removed; the grouping shell automatically omits empty groups.

If the removed example was the only scenario for a sub-area, remove that sub-area from the current grouping list only if no other example belongs to it. If the removed example was the only scenario for a top-level feature, remove the feature from `ExampleFeatureOrder` only when that architectural boundary is no longer represented by any Example scenario.

Do not leave hidden, unreachable, or duplicate registration paths behind.

## Shared example content layout

Each focused example keeps the established visual structure:

```text
Run button
    ↓
Test input      | C# reproduction snippet
    ↓
Description / Expected result
    ↓
Architecture note
```

`Description` and `Expected result` intentionally remain one combined information label. This avoids competing panels and keeps the main content position stable.

`NormalizeExampleTabContentLayouts()` gives this combined block enough vertical space for wrapped text and reserves a fixed-height row for the architecture note. Do not replace this with arbitrary overlapping `DockStyle.Top` controls or separate floating information panels.

## Registration order and lifecycle

All child tabs must exist before `OrganizeExampleTabs()` is called. A common failure mode is registering a tab from `Application.Idle` or another later callback; that tab will bypass the initial grouping pass and appear outside the expected feature group.

The canonical initialization sequence is:

```text
Build shell
    ↓
Register feature/example tabs
    ↓
Add special ahead-of-roadmap examples that are intentionally retained
    ↓
OrganizeExampleTabs()
    ↓
NormalizeExampleTabContentLayouts()
    ↓
Show grouped Example UI
```

Do not add delayed tab registration merely to work around initialization order. Correct the initialization sequence instead.

## Classification rules

Examples should be grouped according to the architecture/capability they verify. Typical boundaries include:

- **Core** — basic messaging/session behavior.
- **Memory** — explicit, automatic, episodic, task/event, and related memory capabilities.
- **Context** — generic context, UI context, data relationships, bounded queries, and discovery.
- **Tools** — tool definitions, registry, validation, loops, persistence, and assignment.
- **Providers** — adapters, provider transport, capabilities, normalization, and streaming.
- **Policy** — unified policy and policy-governed approval/defer behavior.
- **Events** — event contracts and event lifecycle behavior.
- **Identity** — deployment, tenant, principal, user, session, workspace, ownership, propagation, and isolation examples.
- **Runtime** — execution lifecycle, runtime instances, intervention, scheduling, quotas/admission, execution planning, auditing, and runtime capability controls.
- **Workspace** — workspace routing, roles, participants, and workspace behavior.
- **Cognition** — learning candidates, learning intervention, cognition workbench/runtime behavior, and future cognitive-state scenarios.
- **Configuration** — examples specifically demonstrating configuration behavior.
- **Diagnostics** — internal inventory or diagnostic inspection examples.

Current Runtime sub-areas are:

```text
Runtime Instances = runtime identity, instance overrides, lifecycle, scheduling, and concurrency
Execution         = execution lifecycle/terminality and effective runtime capabilities
Intervention      = human execution-control/intervention behavior
Planning & Capacity = execution target planning, target catalog, quota and admission
Diagnostics       = execution auditing and related operational inspection
```

Current Context sub-areas are:

```text
Context Core      = generic context budgeting and related core context behavior
UI Context        = WinForms/control/data-source/object discovery and relationships
Data Access Context = provider-neutral structured data query contracts
```

These are guidelines, not permission to force unrelated scenarios into an existing group. When a new capability introduces a genuinely distinct architectural boundary, add a new top-level group deliberately.

## Verification requirements

After changing Example organization or registration:

1. Build `HAgent.Example` on the supported target being used.
2. Run the application.
3. Verify the expected top-level feature groups and nested child tabs are present.
4. For Runtime and Context, verify the expected sub-area tabs are present and each child example is still independently accessible.
5. Verify Description/Expected result text no longer overlaps or clips on the affected examples.
6. Open affected child tabs and confirm their existing run actions still work.
7. For new or changed capabilities, run their deterministic verification and record the result in the appropriate roadmap/active-work document.

Do not claim Example UI or test success based only on source inspection.

## Relationship to repository rules

This guide implements the Example requirements in `AGENTS.md`: architecture-level top-level feature tabs, nested tabs for multiple examples, focused partial files, independently understandable scenarios, deterministic verification, and no loss or duplication of Example capabilities.

When `AGENTS.md` changes, this maintenance guide must remain consistent with the global repository rules and should be updated when the Example UI contract changes.
