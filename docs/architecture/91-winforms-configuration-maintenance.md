# WinForms Configuration UI Maintenance Guide

## Purpose

`HAgent.WinForms` exposes one default configuration window through `AISettingsForm`. The form is the configuration shell. Feature pages live under `src/HAgent.WinForms/UI/Configuration/` and own the UI and behavior for one configuration domain.

The structure exists to keep future changes localized. A configuration change should normally touch the smallest feature directory possible rather than turning `AISettingsForm.cs` into a second business-logic container.

## Configuration shell responsibilities

`src/HAgent.WinForms/Forms/AISettingsForm.cs` owns only:

- construction of the shared `ConfigurationContext`;
- construction of configuration pages;
- registration and ordering of navigation items;
- switching the visible page;
- top-level loading/reload of shared providers and agents.

It must not contain provider CRUD logic, agent CRUD logic, tool CRUD logic, policy evaluation logic, storage implementation logic, or permission implementation logic.

`ConfigurationContext` is the shared state/dependency boundary. It carries the `IAiStore`, `ISecretStore`, provider adapters, tool registry, and currently loaded provider/agent collections to feature pages.

## Where to add a new configuration area

When adding a new configuration area:

1. Create a directory under `src/HAgent.WinForms/UI/Configuration/<Area>/`.
2. Add a page class such as `<Area>Page.cs` deriving from `ConfigurationPageBase`.
3. Keep all list/edit/delete/settings behavior for that area inside its directory or its specialized editor files.
4. Add the page as a field on `AISettingsForm` only if it needs a long-lived page instance.
5. Register exactly one navigation item in `AISettingsForm.RegisterPages()` or `RegisterAction()`.
6. Add the page to the appropriate navigation position. Do not inject buttons later through reflection or recursive control discovery.
7. If the new area needs shared data, extend `ConfigurationContext` instead of reaching into another page or using reflection.
8. Update the deterministic Example verification when the new configuration capability has observable behavior.

The navigation order should remain explicit and readable from `AISettingsForm`; future maintainers should not need to inspect event handlers or control-tree searches to know what appears in the configuration window.

## How to edit an existing configuration area

First identify the navigation item and then work inside its feature directory:

```text
Overview  -> UI/Configuration/Overview/
Providers -> UI/Configuration/Providers/
Agents    -> UI/Configuration/Agents/
Tools     -> UI/Configuration/Tools/
Policy    -> UI/Configuration/Policy/
About     -> UI/Configuration/About/
```

If the change concerns an editor rather than the page list, use the corresponding specialized editor under `Forms/` or move the editor into the feature directory when that editor is refactored.

Do not put feature-specific code back into `AISettingsForm.cs` merely because the change is visible from the configuration window.

## How to remove a configuration area

Remove the navigation registration from `AISettingsForm`, remove the page field/construction when no longer referenced, then remove the feature directory or obsolete files. Also remove any now-unused `ConfigurationContext` members and update documentation/verification that referred to the area.

Do not leave hidden navigation injections, compatibility aliases, or dead page factories behind.

## Page layout rule

Every list-based configuration page uses three distinct vertical regions:

```text
+---------------------------------------+
| Header / title                        |
+---------------------------------------+
| Action bar                            |
+---------------------------------------+
| List / content                        |
|                                       |
+---------------------------------------+
```

Do not place header, action bar, and content as competing `DockStyle.Top` controls in the same parent. Use a layout container such as `TableLayoutPanel` with explicit rows (or an equivalent nested panel structure) so the content cannot overlap the header or action bar.

The standard page pattern is:

- row 0: fixed-height header;
- row 1: fixed-height action bar;
- row 2: `Percent, 100` content area.

This rule applies to Providers, Agents, Tools, Policy, and future list-oriented configuration pages.

## AI editing guidance

For an AI making a configuration UI change, the preferred inspection sequence is:

```text
1. AISettingsForm.cs
2. ConfigurationContext.cs
3. The target UI/Configuration/<Area>/ directory
4. Only the editor/helper files directly used by that area
5. Relevant deterministic Example verification
```

Avoid loading the entire WinForms project unless the change crosses configuration-shell boundaries. Avoid solving a local page change by adding reflection, control-tree discovery, or a new global navigation mechanism.

## Architectural invariant

`AISettingsForm` is a composition shell, not the implementation home for every configuration feature. Configuration features must remain independently understandable, independently editable, and independently removable.
