# Tool architecture

A tool has two separate parts:

```text
Tool definition
    name, description, schema, category, metadata

Trusted handler
    executable behavior supplied by HAgent or the host
```

The definition can be persisted. Executable handlers are runtime-owned and are not serialized.

## Categories

- BuiltIn
- Application
- Declarative
- UI
- SqlServer
- MySql

Extension tools are a later extensibility feature.

## Execution boundary

The model may request a registered tool. HAgent validates structured arguments and invokes only a registered trusted handler. The host remains responsible for real-world/application side effects and validation.

Each tool execution carries execution-local correlation metadata. The execution context and result identify the agent, tool, and model tool-call, and carry a correlation ID plus start/completion timing. Validation failures and disabled/missing-tool failures receive the same metadata shape so callers can correlate accepted and rejected attempts without inspecting or logging tool arguments by default.

Correlation metadata is execution state, not permission. It does not authorize a tool, expand its arguments, or grant access to host resources. Persistent audit storage is a separate capability and is not implied by these runtime fields.

## HAgent internal read tools

HAgent may expose selected HAgent-owned repositories through trusted built-in read tools. The first foundation is `HAgentInternalInventoryTool`, which reports a bounded inventory of providers, agents, and persisted tool definitions.

The inventory tool accepts an optional `maxItems` argument. The default is 50 and the hard maximum is 100 items per category. It propagates caller cancellation and returns only non-secret metadata such as IDs, names, relationships, enabled state, and tool type. Provider secrets, passwords, raw connection strings, arbitrary storage records, and executable handlers are not returned.

An internal read tool remains a trusted capability boundary. Registering it is an explicit host/runtime decision; the tool does not grant the model authority to inspect unrelated host application data or bypass other HAgent permissions.

## Tool loops

Provider tool calls may be processed through a bounded multi-turn loop. Loop limits, cancellation, timeouts, and failures remain runtime concerns.

## Database tools

SQL Server and MySQL tools must expose restricted structured operations. Arbitrary model-supplied SQL is not an acceptable generic capability.
