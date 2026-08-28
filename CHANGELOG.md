# Changelog

## 0.1.8

- Fixed management-page docking so Providers, Agents, and Tools lists cannot overlap their action bars or headings.
- Ordered navigation as Overview, Agents, Providers, Tools, About.
- Added provider connection testing.
- Added optional provider model catalog discovery and model ComboBoxes.
- Added agent configuration testing.
- Added safe provider and agent deletion from the management UI.
- Provider deletion is now rejected by the storage layer when agents reference the provider.
- Added `plan.md` as the active implementation tracker.

## 0.1.1

- Temporarily removed .NET 10 targets from the solution to support Visual Studio 2022 with the .NET 9 SDK.
- Current build targets are .NET Framework 4.8.1 and .NET 9 (Windows targets for WinForms).


## 0.1.0 — 2026-08-28

Initial foundation release.

- Core provider/agent model.
- OpenAI-compatible provider adapter.
- JSON file storage.
- Windows DPAPI secret storage.
- SQL Server storage.
- MySQL storage.
- WinForms configuration UI.
- Session `SendAsync` / `ReadAsync` API.
- Sample application.
