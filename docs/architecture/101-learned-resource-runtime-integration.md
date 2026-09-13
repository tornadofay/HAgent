# Learned Resource Runtime Integration

Slice 5 connects promoted learned-resource state to execution-time admission.

The runtime consumes existing applicability, reliability, lifecycle, retention, and policy state. It does not create a second authorization or evaluation system.

Automatic learned-resource use requires current reliability and lifecycle revisions, `Applicable` applicability, `Active + Current` lifecycle state, no reliability review/quarantine flag, and an allowed `resource.runtime.use` policy decision.

`AiLearnedResourceExecutionSnapshot` captures exact resource identity/version/scope, reliability revision/score, lifecycle revision/status/condition, retention decision, applicability outcome, and capture time.

A reliability or lifecycle revision mismatch is stale asynchronous state and forces fallback rather than automatic use. Fallback selection is host-owned and bounded to deterministic safe action, alternate resource, bounded reasoning, or host escalation. HAgent does not execute the fallback itself.

Published resource definitions and versions remain unchanged.
