# HAgent roadmap

HAgent is a general-purpose agent runtime that can serve simple chat, business applications, games, simulations, and other host environments.

## Status

- 0.1 Foundation — complete
- 0.2 Runtime foundation — complete
- 0.3 Memory + Context — foundation complete
- 0.4 Capabilities + Response Normalization — foundation complete
- 0.5 Tool foundation — verified; hardening remains
- 0.6 Safety + authorization foundation — partial
- 0.7 UI Context + Data Discovery — complete
- 0.8 Data Access + Authorization — active
- 0.9 Agent Runtime + Workspaces + Chat — planned after the security/data foundation
- 1.0 Collaboration + Workflows — planned
- Later: provider/extensibility/developer platform work and stable 1.0 release hardening

## Dependency order

```text
Provider/runtime foundation
        ↓
Memory/context
        ↓
Tools
        ↓
UI/data discovery
        ↓
Data access + authorization
        ↓
Runtime agent instances
        ↓
Workspaces + routing + visible collaboration
        ↓
Collaboration/workflows
        ↓
Extensibility + release hardening
```

## Documentation rule

Architecture describes what HAgent is. The implementation plan describes what is being built now. This roadmap describes what comes next. Engineering invariants live in `AGENTS.md`.
