# HAgent Development Plan

This directory is the authoritative implementation ledger. The root `plan.md` is generated from these files.

## Current state

- Target frameworks: `.NET Framework 4.8.1` and `.NET 9`.
- `HAgent.Example` is the manual integration/verification host.
- User-facing WinForms uses the project's `Header`, `HMessage`, and `HButton` helpers.
- Core remains provider-neutral and lightweight.
- Base memory does not require GPU, vector database, or a large resident model.
- Initial tool categories: BuiltIn, Application, Declarative, UI, SqlServer, MySql.
- Extension tools are deferred.

## Working rule

A feature is marked complete only after its implementation exists and matching `HAgent.Example` verification has passed locally.

## Development workflow

1. Implement one focused slice.
2. Add or update an Example test for that slice.
3. User builds and tests locally.
4. Record the actual result here.
5. Only then mark the slice complete.
6. Keep root `plan.md` and `roadmap.md` generated from these smaller source files.
