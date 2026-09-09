using System;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class ObservabilityTracePropagationTests
    {
        [Fact]
        public void ExportImport_PreservesTraceContextAndAllCorrelationIdentities()
        {
            var context = new TraceContext("trace-42", "parent-42", true);
            var correlation = new TraceCorrelation
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
                CausationId = "cause-42"
            };

            var carrier = TracePropagation.Export(context, correlation);
            var result = TracePropagation.Import(carrier, new TracePropagationImportOptions
            {
                AcceptUntrustedTraceContext = true
            });

            Assert.True(result.Accepted);
            Assert.Equal(TracePropagationImportStatus.Accepted, result.Status);
            Assert.Equal("trace-42", result.Context.TraceId);
            Assert.Equal("parent-42", result.Context.ParentSpanId);
            Assert.True(result.Context.Sampled);
            Assert.Equal("deployment-42", result.Correlation.DeploymentId);
            Assert.Equal("tenant-42", result.Correlation.TenantId);
            Assert.Equal("principal-42", result.Correlation.PrincipalId);
            Assert.Equal("user-42", result.Correlation.UserId);
            Assert.Equal("session-42", result.Correlation.SessionId);
            Assert.Equal("workspace-42", result.Correlation.WorkspaceId);
            Assert.Equal("agent-42", result.Correlation.AgentProfileId);
            Assert.Equal("runtime-42", result.Correlation.RuntimeInstanceId);
            Assert.Equal("execution-42", result.Correlation.ExecutionId);
            Assert.Equal("execution-correlation-42", result.Correlation.ExecutionCorrelationId);
            Assert.Equal("host-correlation-42", result.Correlation.HostCorrelationId);
            Assert.Equal("event-42", result.Correlation.EventId);
            Assert.Equal("cause-42", result.Correlation.CausationId);
            Assert.NotEqual(result.Context.TraceId, result.Correlation.ExecutionId);
            Assert.NotEqual(result.Context.TraceId, result.Correlation.ExecutionCorrelationId);
        }

        [Fact]
        public void ExportImport_PreservesUnsampledStateWithoutChangingTraceIdentity()
        {
            var carrier = TracePropagation.Export(
                new TraceContext("trace-unsampled", null, false),
                new TraceCorrelation { ExecutionId = "execution-unsampled" });

            var result = TracePropagation.Import(carrier, new TracePropagationImportOptions
            {
                AcceptUntrustedTraceContext = true
            });

            Assert.True(result.Accepted);
            Assert.False(result.Context.Sampled);
            Assert.Null(result.Context.ParentSpanId);
            Assert.Equal("execution-unsampled", result.Correlation.ExecutionId);
        }

        [Fact]
        public void Import_MissingTraceContextReturnsMissingAndDoesNotInventIdentity()
        {
            var carrier = new TracePropagationCarrier();
            carrier.Set("hagent.correlation.execution-id", "execution-42");

            var result = TracePropagation.Import(carrier, new TracePropagationImportOptions
            {
                AcceptUntrustedTraceContext = true
            });

            Assert.False(result.Accepted);
            Assert.Equal(TracePropagationImportStatus.Missing, result.Status);
            Assert.Null(result.Context);
            Assert.Equal("execution-42", result.Correlation.ExecutionId);
        }

        [Fact]
        public void Import_RejectsIncomingTraceContextUntilHostExplicitlyAcceptsTrust()
        {
            var carrier = TracePropagation.Export(
                new TraceContext("trace-untrusted", "parent-untrusted", true),
                new TraceCorrelation { HostCorrelationId = "host-42" });

            var result = TracePropagation.Import(carrier);

            Assert.False(result.Accepted);
            Assert.Equal(TracePropagationImportStatus.RejectedAsUntrusted, result.Status);
            Assert.Null(result.Context);
            Assert.Equal("host-42", result.Correlation.HostCorrelationId);
            Assert.Contains("trust boundary", result.RejectionReason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Import_RejectsMalformedTraceContextWithoutPartiallyCreatingContext()
        {
            var carrier = new TracePropagationCarrier();
            carrier.Set("hagent.trace.trace-id", "trace bad");
            carrier.Set("hagent.trace.parent-span-id", "parent-42");
            carrier.Set("hagent.trace.sampled", "1");

            var result = TracePropagation.Import(carrier, new TracePropagationImportOptions
            {
                AcceptUntrustedTraceContext = true
            });

            Assert.False(result.Accepted);
            Assert.Equal(TracePropagationImportStatus.InvalidTraceContext, result.Status);
            Assert.Null(result.Context);
        }

        [Fact]
        public void Import_RejectsInvalidSampledState()
        {
            var carrier = new TracePropagationCarrier();
            carrier.Set("hagent.trace.trace-id", "trace-42");
            carrier.Set("hagent.trace.sampled", "true");

            var result = TracePropagation.Import(carrier, new TracePropagationImportOptions
            {
                AcceptUntrustedTraceContext = true
            });

            Assert.False(result.Accepted);
            Assert.Equal(TracePropagationImportStatus.InvalidTraceContext, result.Status);
            Assert.Null(result.Context);
        }

        [Fact]
        public void Import_RejectsOversizedCorrelationValues()
        {
            var carrier = new TracePropagationCarrier();
            carrier.Set("hagent.trace.trace-id", "trace-42");
            carrier.Set("hagent.trace.sampled", "1");
            carrier.Set("hagent.correlation.execution-id", new string('e', 128));

            var result = TracePropagation.Import(carrier, new TracePropagationImportOptions
            {
                AcceptUntrustedTraceContext = true,
                MaxValueLength = 64
            });

            Assert.False(result.Accepted);
            Assert.Equal(TracePropagationImportStatus.InvalidCorrelation, result.Status);
            Assert.Null(result.Context);
        }

        [Fact]
        public void Carrier_IsBoundedAndCloningDoesNotAliasStoredValues()
        {
            var carrier = new TracePropagationCarrier();
            carrier.Set("key-42", "value-42");

            var clone = carrier.Clone();
            clone.Set("key-43", "value-43");

            Assert.Equal(1, carrier.Count);
            Assert.Equal(2, clone.Count);
            Assert.True(carrier.TryGetValue("key-42", out var value));
            Assert.Equal("value-42", value);
        }
    }
}
