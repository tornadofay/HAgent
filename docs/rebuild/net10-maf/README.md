# HAgent .NET 10 + Microsoft Agent Framework Rebuild

This directory contains the architectural report for rebuilding HAgent as a **.NET 10-only** library with **Microsoft Agent Framework (MAF)** as a foundation and source of reusable building blocks.

This is a redesign/rebuild plan, not a compatibility migration plan. It starts at **Phase 1** and treats the existing HAgent implementation as an inventory of required behavior and evidence, not as a design constraint.

## Documents

| Document | Purpose |
|---|---|
| [`00-executive-report.md`](00-executive-report.md) | Complete executive report, goals, target position, and conclusions |
| [`01-maf-capability-mapping.md`](01-maf-capability-mapping.md) | HAgent-to-MAF capability mapping and reuse strategy |
| [`02-target-architecture.md`](02-target-architecture.md) | Proposed .NET 10 target architecture and dependency boundaries |
| [`03-functionality-preservation-and-deduplication.md`](03-functionality-preservation-and-deduplication.md) | Existing functionality preservation, duplication removal, and replacement rules |
| [`04-phase-plan.md`](04-phase-plan.md) | Full migration/rebuild phases beginning with Phase 1 |
| [`05-verification-and-completion.md`](05-verification-and-completion.md) | Verification strategy, completion criteria, and no-regression requirements |
| [`06-decisions-required.md`](06-decisions-required.md) | Architectural choices where MAF and HAgent contracts may legitimately differ; final choices remain with the project owner |

## Target

```text
.NET 10 only
        |
        v
+-------------------------------+
|            HAgent             |
|                               |
| HAgent-specific architecture  |
| Persistent Cognitive Runtime  |
| Learning governance           |
| Resource ownership/scope      |
| Execution planning/admission  |
| Host authority boundaries     |
| HAgent persistence            |
| Lifecycle/recovery semantics  |
| WinForms integration          |
+---------------+---------------+
                |
                v
+-------------------------------+
| Microsoft Agent Framework     |
| Agents / Workflows / Tools    |
| Middleware / Context          |
| Orchestration / HITL / State |
+---------------+---------------+
                |
                v
+-------------------------------+
| Microsoft.Extensions.AI       |
| IChatClient / embeddings etc.|
+-------------------------------+
                |
                v
       Model/provider ecosystem
```

## Core rule

HAgent must **not duplicate MAF functionality that already satisfies the required semantics**.

When MAF currently provides only part of an HAgent requirement, HAgent may add a missing layer, but that layer must be **replaceable** so a future MAF implementation can take over without requiring a redesign of HAgent's public target architecture.

When MAF provides a lower-level primitive but HAgent needs a materially different contract, the report presents the alternatives and trade-offs. Those cases are explicit project-owner decisions; they are not silently resolved during implementation.

## Source basis

The report was prepared against the HAgent repository's current architecture, roadmap/plan structure, and documented project invariants, together with the current Microsoft Agent Framework and Microsoft.Extensions.AI documentation and packages available in September 2026.
