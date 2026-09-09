# Observability and Distributed Tracing

## Purpose

HAgent observability provides a provider-neutral way to explain execution and runtime behavior across process boundaries without turning telemetry into authorization, transcript storage, or a second event system.

Tracing is an observability layer over existing HAgent operations. Existing execution state, event delivery, policy decisions, context/admission diagnostics, tool results, provider results, and execution audit remain authoritative in their own subsystems. Tracing records bounded observability metadata about those operations and links them into a trace hierarchy.

## Architectural position

```text
Host / Event / Request
          |
          v
   Trace identity/context
          |
          v
     Root execution span
       /     |      \
   Policy  Context  Provider
      |       |        |
   Tools  Retrieval  Tool calls
      \       |        /
       \---- Outcome
```

Tracing does not replace:

- `EventEnvelope` or `IEventDispatcher` for event delivery;
- `IExecutionAuditStore` for terminal execution audit persistence;
- `IAiPolicyEngine` for authorization/policy enforcement;
- context admission/ranking/compaction diagnostics for context decisions;
- provider/tool contracts for actual execution;
- host identity/authentication for identity authority.

These systems remain producers of observable activity. A future tracer consumes bounded metadata from them without making any of them depend on a telemetry vendor.

## Identity model

A trace has its own observability identity. Trace identity must not replace existing HAgent identities.

```text
DeploymentId
TenantId
PrincipalId
UserId
SessionId
WorkspaceId
AgentProfileId
RuntimeInstanceId
ExecutionId
ExecutionCorrelationId
HostCorrelationId
EventId / CausationId
TraceId
SpanId
ParentSpanId
```

The meanings are distinct:

- `TraceId` identifies one observable operation tree.
- `SpanId` identifies one operation within a trace.
- `ParentSpanId` identifies the immediate causal parent span when one exists.
- `ExecutionId` identifies the HAgent execution and remains the execution authority.
- `ExecutionCorrelationId` is the existing execution-level correlation value and remains available for application/audit correlation.
- `HostCorrelationId` is the host-supplied correlation value and remains external to HAgent execution identity.
- `EventId` and `CausationId` preserve event provenance; they are not converted into span identity.
- `RuntimeInstanceId`, profile identity, workspace, session, user, tenant, and deployment identify ownership/context dimensions and are recorded only when available and permitted.

A trace may contain multiple executions and non-execution operations. An execution normally creates or adopts a root span, but `TraceId` and `ExecutionCorrelationId` are never aliases by contract.

## Trace context propagation

The canonical provider-neutral internal propagation object is a small immutable trace context containing:

```text
TraceId
ParentSpanId (optional)
TraceFlags / sampled state
```

The context is attached to internal operation boundaries rather than embedded in prompts, tool arguments, provider payloads, event payloads, or host-domain data.

When an operation begins without an incoming trace context, the tracer creates a new `TraceId` and root `SpanId`. Child operations inherit the trace ID and use the creating span as their parent. A child operation may create another span even when the underlying component already exposes an execution or event correlation ID.

Cross-process propagation is a host/transport concern. HAgent Core defines the values and relationship semantics but does not prescribe HTTP headers, W3C/OpenTelemetry types, message formats, or a particular distributed tracing transport.

## Cross-process propagation boundary

HAgent Core exposes a bounded transport-neutral carrier for hosts that need to move trace context and existing correlation identities across a process boundary. The carrier is a constrained key/value representation rather than a network protocol.

The canonical Core boundary is:

```text
TraceContext + TraceCorrelation
            |
            v
   TracePropagation.Export
            |
            v
 TracePropagationCarrier
            |
       host / transport
            |
            v
 TracePropagation.Import
            |
            v
TraceContext + TraceCorrelation
```

The carrier may contain:

- `TraceId`;
- optional `ParentSpanId`;
- sampled state;
- bounded deployment, tenant, principal, user, session, workspace, agent-profile, runtime, execution, execution-correlation, host-correlation, event, and causation identifiers.

These values remain semantically distinct during export/import. In particular, a trace ID is never treated as an execution ID, execution correlation ID, event ID, or causation ID merely because they crossed the same transport boundary.

The carrier has bounded key/value counts and lengths. Export does not serialize prompts, responses, provider payloads, tool arguments/results, host context, credentials, connection strings, raw exceptions, or arbitrary objects.

Incoming trace context is not implicitly trusted. The host must explicitly choose whether the incoming trace context is trusted at its own boundary. Core import defaults to rejecting an incoming trace context as untrusted until that acceptance is explicit. Missing trace context creates no synthetic remote identity; the receiving operation may create a new local root as appropriate. Malformed trace or correlation values are rejected without constructing a partial `TraceContext`.

Correlation values may still be carried as bounded diagnostic identity even when a trace context is rejected, but they do not become authorization or authentication authority. Host authentication/authorization remains the source of trust.

An accepted imported context preserves sampled state exactly. An unsampled imported context remains unsampled; import does not upgrade it to sampled. A receiving tracer may then create a child span using the imported context according to normal HAgent span lifecycle rules.

The Core boundary intentionally stops before wire serialization, transport headers, message envelopes, authentication, signature verification, replay protection, or vendor-specific telemetry APIs. Those concerns belong to the host/adapter that owns the process boundary.

## Span model

The canonical provider-neutral span represents one bounded observable operation.

Required span data:

```text
SpanId
TraceId
ParentSpanId (optional)
OperationName
Kind
StartedAt
CompletedAt (optional while active)
Duration (derived when complete)
Status
Identity/context metadata (bounded)
Safe metadata (bounded)
```

`Kind` is an open string/category contract, not a vendor enum. The initial categories are:

```text
Execution
Event
Policy
Context
Retrieval
Admission
Planning
Provider
Tool
Memory
Knowledge
Learning
Lifecycle
Evaluation
Outcome
```

Future operation kinds must not require a closed Core enum.

Span status is provider-neutral and represents terminal observability outcome, for example:

```text
Unset
Succeeded
Failed
Cancelled
Timeout
Rejected
Skipped
```

Status is descriptive telemetry only. A trace status must never authorize, cancel, retry, approve, or reject an operation.

## Parentage and causal relationships

Parent/child span hierarchy expresses operation nesting, not domain authorization.

```text
Execution span
  -> Policy span
  -> Context span
      -> Retrieval span
      -> Admission span
  -> Planning span
  -> Provider span
      -> Tool span
  -> Outcome span
```

Existing event `CorrelationId` and `CausationId` remain available as bounded metadata/relationships where applicable. Event causation can explain why an execution or child operation began, but it does not replace `ParentSpanId`.

When an operation is initiated by another traced operation, `ParentSpanId` is the hierarchy relationship. When an event or external request explains the origin, its event/request correlation and causation identifiers are preserved independently.

## Operation coverage

0.956 observability is expected to cover the following existing and future boundaries without requiring all of them in the first implementation slice:

```text
Request intake
Execution lifecycle
Policy evaluation / denial / approval / defer
Execution planning / fallback
Context retrieval / admission / ranking / compaction
Provider target selection / provider execution / retry / fallback
Tool invocation / tool result
Memory / Knowledge activity
Learning candidate handling
Runtime lifecycle / shutdown / waiting / recovery
Evaluation activity
Terminal outcome / stale-result rejection / late completion
```

Operations should record decision reasons and failure classification when an existing producer already exposes those values. Tracing must not invent domain meaning that the producer does not provide.

## Relationship to execution audit

`AgentExecutionAuditRecord` remains the bounded terminal execution projection for audit persistence. It contains execution/correlation identity, agent/provider/model metadata, lifecycle timestamps, state, and classified failure metadata; prompts, responses, credentials, and raw exceptions remain outside that record.

Tracing is broader and time-oriented. It may contain multiple spans before terminal execution completion and may represent policy, context, provider, tool, and lifecycle operations that are not part of the terminal audit record.

The two representations may share correlation fields but must not serialize one another or create a second persistence authority.

## Relationship to events

`EventEnvelope` remains the provider-neutral event contract with event identity, correlation, causation, scope, identity, payload, and bounded context. Event dispatch remains independent of tracing.

The observability integration may create a span for event publication, handling, or an event-triggered execution. Event payload/context is never automatically copied into trace metadata.

A trace may retain bounded references such as event ID, type, source, scope, and causation ID when useful and allowed.

## Relationship to policy and authorization

Policy decisions are already structured as `AiPolicyDecision` and evaluated from `AiPolicyEvaluationContext`. Tracing may record the decision outcome, selected rule identity, bounded reason, and relevant operation/resource identity.

Raw policy attributes are not automatically copied into traces. Authorization state is enforced by policy and host boundaries; trace metadata is diagnostic only.

A denial, approval requirement, or defer result should be observable as a span status/metadata outcome, but the tracing layer must not be able to alter that decision.

## Safe metadata boundary

Observability follows a default-deny payload policy.

Safe by default:

- identifiers required to correlate operations;
- operation/category names;
- timestamps and bounded durations;
- lifecycle state and classified failure kinds;
- provider/target/tool/resource identifiers where already non-secret and permitted;
- policy outcome, rule identifier, priority, and bounded reason;
- context item/source identifiers, estimated size, ranking/admission decisions, and safe provenance metadata;
- sampling/redaction decisions themselves.

Excluded by default:

- provider API keys and other credentials;
- secret IDs when they could disclose a credential relationship unnecessarily;
- connection strings and raw transport headers containing secrets;
- raw prompts and responses;
- tool arguments/results when they can contain host or user data;
- raw host context values;
- raw context payloads;
- arbitrary serialized objects;
- raw exception objects or stack traces containing sensitive values.

There is no implicit safe assumption that a field is harmless because it is a string. Metadata is admitted through an explicit bounded representation and, where needed, a redaction policy.

Redaction is performed before a record reaches any sink. A sink is not allowed to recover excluded values. Redaction policy is configurable, but the minimum default exclusions above cannot be disabled by an ordinary telemetry sink.

## Bounded metadata

Trace metadata is provider-neutral and bounded. The contract should support a small set of scalar values and bounded strings, with explicit limits on:

```text
maximum metadata entries per span
maximum key length
maximum string value length
maximum aggregate metadata size
```

Large or unknown objects are not serialized into traces. Implementations may record a bounded classification such as `Present`, `Redacted`, or `Unavailable` instead of the underlying payload.

## Sampling

Sampling determines which trace/span records are retained or emitted; it is not a correctness mechanism.

The canonical policy is:

```text
operation starts
   ↓
sampling decision
   ↓
trace records are captured or dropped according to policy
```

Sampling must be deterministic for equivalent inputs when a deterministic sampler is selected. A sampled-out parent must not prevent creation of an in-memory diagnostic relationship required by an active operation; implementations may retain bounded internal state for the duration of the operation even when final export is suppressed.

Sampling configuration must be bounded and provider-neutral. Future vendor adapters may translate sampling decisions to their own transport mechanisms without changing Core contracts.

## Retention

Retention is a storage/inspection concern, separate from span lifecycle. The architecture supports bounded retention by:

```text
maximum trace count or record count
maximum age
maximum per-trace span count
maximum aggregate diagnostic size
```

An in-memory tracer is the first reference implementation target. Future persistent or remote sinks must preserve the same ownership, redaction, and retention boundaries and must not silently convert tracing into unrestricted transcript storage.

## Failure, cancellation, stale results, and fallback

Important control-flow outcomes must be observable when the producer exposes them:

```text
Policy denial / approval / defer
Provider unavailable
Retry
Fallback target
Wait / backpressure
Cancellation
Timeout
Late provider completion
Stale-result rejection
Runtime shutdown
Recovery / resumed operation
```

A late provider completion must never create an apparent successful terminal operation when the execution has already reached another terminal state. Tracing observes the actual accepted terminal transition and may record the late completion as a separate child/attempt operation with bounded metadata.

Cancellation and timeout are distinct from successful completion even when an underlying provider later completes normally.

## Human-readable projection

Management/UI projections should consume trace metadata through a safe, bounded diagnostic projection rather than rendering raw trace storage records directly.

The projection should be able to show:

```text
operation
start / duration
status
parent / child relationship
execution / runtime identity
provider / tool / resource identifiers
policy/admission reason
failure category
redaction indicator
```

The projection must not reveal secrets, prompts, raw responses, arbitrary object graphs, or unrestricted host context.

## Provider and transport neutrality

HAgent.Core owns the trace/span semantics only. It must not depend on OpenTelemetry, vendor SDKs, exporter protocols, a particular logging package, or a particular network transport.

Integration adapters may map HAgent spans to OpenTelemetry or another telemetry system later. Such adapters must treat the HAgent contract as the source semantics and must preserve the safe metadata boundary.

## First implementation boundary

The first implementation slice after this architecture checkpoint is deliberately narrow:

**0.956 Slice 2 — Trace identity and span lifecycle contracts**

It should introduce only the provider-neutral Core contracts necessary to:

1. create a trace identity and span identity;
2. carry immutable parent trace context;
3. start/complete a span with timestamps, status, and bounded metadata;
4. correlate trace records with existing execution/host/runtime/event identifiers without replacing them;
5. represent redacted/omitted metadata without accepting raw payloads;
6. expose an in-memory recorder boundary suitable for deterministic testing, without implementing persistence, vendor exporters, UI, or broad runtime instrumentation yet.

The matching verification should be a focused `HAgent.Tests` contract test plus a corresponding public-API `HAgent.Example` scenario covering hierarchy, correlation, redaction-safe metadata, completion/failure/cancellation status, and deterministic ordering. Later slices add runtime producers/instrumentation, sampling/retention behavior, integrated sinks, diagnostic projection, and broader failure/fallback coverage.

No later slice may be started in the same run as Slice 2 implementation.
