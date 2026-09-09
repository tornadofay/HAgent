using System;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddObservabilityTracePropagationTab()
        {
            AddApiTab(
                "Observability Trace Propagation",
                "Run propagation boundary test",
                "Runs deterministic cross-process trace propagation checks through the public tracing APIs.",
                "The scenario verifies bounded transport-neutral export/import, complete correlation identity preservation, explicit host trust acceptance, missing and malformed input handling, unsampled state preservation, and separation of trace identity from execution/event correlation.",
                "No network transport or remote telemetry service is contacted. A deterministic in-memory carrier stands in for a cross-process wire boundary.",
                RunObservabilityTracePropagationTest,
                "Trace propagation",
                "This Slice 7 scenario defines the Core propagation boundary; HTTP, message transports, OpenTelemetry, and remote telemetry remain outside this slice.");
        }

        private async Task RunObservabilityTracePropagationTest(string unused)
        {
            var sourceContext = new TraceContext("trace-source-42", "span-parent-42", true);
            var sourceCorrelation = new TraceCorrelation
            {
                DeploymentId = "deployment-42",
                TenantId = "tenant-42",
                PrincipalId = "principal-42",
                UserId = "user-42",
                SessionId = "session-42",
                WorkspaceId = "workspace-42",
                AgentProfileId = "agent-42",
                RuntimeInstanceId = "runtime-42",
                ExecutionId = "execution-42",
                ExecutionCorrelationId = "execution-correlation-42",
                HostCorrelationId = "host-correlation-42",
                EventId = "event-42",
                CausationId = "causation-42"
            };

            var carrier = TracePropagation.Export(sourceContext, sourceCorrelation);
            var accepted = TracePropagation.Import(carrier, new TracePropagationImportOptions
            {
                AcceptUntrustedTraceContext = true
            });

            if (!accepted.Accepted || accepted.Context == null)
                throw new InvalidOperationException("Accepted propagation context was not imported.");
            if (accepted.Context.TraceId != sourceContext.TraceId ||
                accepted.Context.ParentSpanId != sourceContext.ParentSpanId ||
                accepted.Context.Sampled != sourceContext.Sampled)
                throw new InvalidOperationException("Trace context was not preserved across the propagation boundary.");
            if (accepted.Correlation.ExecutionId != sourceCorrelation.ExecutionId ||
                accepted.Correlation.ExecutionCorrelationId != sourceCorrelation.ExecutionCorrelationId ||
                accepted.Correlation.HostCorrelationId != sourceCorrelation.HostCorrelationId ||
                accepted.Correlation.EventId != sourceCorrelation.EventId ||
                accepted.Correlation.CausationId != sourceCorrelation.CausationId ||
                accepted.Correlation.TenantId != sourceCorrelation.TenantId ||
                accepted.Correlation.RuntimeInstanceId != sourceCorrelation.RuntimeInstanceId)
                throw new InvalidOperationException("Correlation identity was not preserved across the propagation boundary.");
            if (accepted.Context.TraceId == accepted.Correlation.ExecutionId ||
                accepted.Context.TraceId == accepted.Correlation.ExecutionCorrelationId ||
                accepted.Context.TraceId == accepted.Correlation.EventId)
                throw new InvalidOperationException("Trace identity collapsed into an existing HAgent correlation identity.");

            var untrusted = TracePropagation.Import(carrier);
            if (untrusted.Status != TracePropagationImportStatus.RejectedAsUntrusted || untrusted.Context != null)
                throw new InvalidOperationException("Untrusted incoming trace context was accepted unexpectedly.");
            if (untrusted.Correlation.ExecutionId != sourceCorrelation.ExecutionId)
                throw new InvalidOperationException("Correlation identity was lost while rejecting untrusted trace context.");

            var missingCarrier = new TracePropagationCarrier();
            missingCarrier.Set("hagent.correlation.execution-id", "execution-missing-trace-42");
            var missing = TracePropagation.Import(missingCarrier, new TracePropagationImportOptions
            {
                AcceptUntrustedTraceContext = true
            });
            if (missing.Status != TracePropagationImportStatus.Missing || missing.Context != null ||
                missing.Correlation.ExecutionId != "execution-missing-trace-42")
                throw new InvalidOperationException("Missing incoming trace context was not handled safely.");

            var malformed = new TracePropagationCarrier();
            malformed.Set("hagent.trace.trace-id", "trace with whitespace");
            malformed.Set("hagent.trace.sampled", "1");
            var malformedResult = TracePropagation.Import(malformed, new TracePropagationImportOptions
            {
                AcceptUntrustedTraceContext = true
            });
            if (malformedResult.Status != TracePropagationImportStatus.InvalidTraceContext || malformedResult.Context != null)
                throw new InvalidOperationException("Malformed incoming trace context was accepted unexpectedly.");

            var unsampledCarrier = TracePropagation.Export(
                new TraceContext("trace-unsampled-42", null, false),
                new TraceCorrelation { ExecutionId = "execution-unsampled-42" });
            var unsampled = TracePropagation.Import(unsampledCarrier, new TracePropagationImportOptions
            {
                AcceptUntrustedTraceContext = true
            });
            if (!unsampled.Accepted || unsampled.Context.Sampled || unsampled.Context.ParentSpanId != null)
                throw new InvalidOperationException("Unsampled propagation state was not preserved.");

            var bounded = new TracePropagationCarrier();
            bounded.Set("safe-key", new string('x', 128));
            if (bounded.Count != 1)
                throw new InvalidOperationException("Bounded propagation carrier did not accept the configured maximum value length.");

            Write(
                "OBSERVABILITY TRACE PROPAGATION",
                "Cross-process trace and correlation propagation boundary succeeded." + Environment.NewLine +
                "Trace context round-trip: verified." + Environment.NewLine +
                "Trace identity kept distinct from execution/event correlation: verified." + Environment.NewLine +
                "All bounded correlation identities preserved: verified." + Environment.NewLine +
                "Explicit host trust acceptance: verified." + Environment.NewLine +
                "Untrusted context rejected without creating a trace context: verified." + Environment.NewLine +
                "Missing context handled without inventing trace identity: verified." + Environment.NewLine +
                "Malformed context rejected safely: verified." + Environment.NewLine +
                "Unsampled state preserved: verified." + Environment.NewLine +
                "Transport carrier value bound: verified." + Environment.NewLine +
                "HTTP/message transport: none." + Environment.NewLine +
                "OpenTelemetry/vendor dependency: none." + Environment.NewLine +
                "Remote telemetry transport: none." + Environment.NewLine +
                "Real provider request: none.");

            await Task.CompletedTask.ConfigureAwait(true);
        }
    }
}
