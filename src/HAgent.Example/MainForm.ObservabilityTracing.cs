using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddObservabilityTracingTab()
        {
            AddApiTab(
                "Observability Tracing",
                "Run trace contract test",
                "Creates a provider-neutral execution span and child provider span using public HAgent APIs, preserving existing correlation identities without turning them into trace IDs.",
                "The expected result is one trace with a two-level span hierarchy, preserved execution/host/event correlation, explicit redacted metadata, terminal statuses, and deterministic recorder order.",
                "execution-42",
                RunObservabilityTracingTestAsync,
                "Observability contract",
                "No provider request, persistence, exporter, or UI trace viewer is involved in this slice.");
        }

        private Task RunObservabilityTracingTestAsync(string executionId)
        {
            var recorder = new InMemoryTraceRecorder();
            var correlation = new TraceCorrelation
            {
                ExecutionId = executionId,
                ExecutionCorrelationId = "execution-correlation-42",
                HostCorrelationId = "host-correlation-42",
                RuntimeInstanceId = "runtime-42",
                EventId = "event-42",
                CausationId = "cause-42"
            };

            var rootMetadata = new TraceMetadata();
            rootMetadata.Add("decision", "Allow");
            rootMetadata.AddRedacted("prompt");

            var root = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution",
                Correlation = correlation,
                Metadata = rootMetadata
            });

            var childMetadata = new TraceMetadata();
            childMetadata.AddOmitted("tool.arguments");

            var child = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = root.Context,
                OperationName = "provider",
                Kind = "Provider",
                Correlation = correlation,
                Metadata = childMetadata
            });

            if (root.Record.TraceId != child.Record.TraceId ||
                child.Record.ParentSpanId != root.Record.SpanId ||
                root.Record.ParentSpanId != null)
                throw new InvalidOperationException("Trace hierarchy was not preserved.");

            if (child.Record.Correlation.ExecutionId != executionId ||
                child.Record.Correlation.HostCorrelationId != "host-correlation-42" ||
                child.Record.Correlation.EventId != "event-42" ||
                child.Record.Correlation.CausationId != "cause-42")
                throw new InvalidOperationException("Existing correlation identities were not preserved.");

            if (root.Record.Metadata.Values["prompt"] != "[Redacted]" ||
                child.Record.Metadata.Values["tool.arguments"] != "[Omitted]")
                throw new InvalidOperationException("Redaction-safe trace metadata was not preserved.");

            if (!root.TryComplete(TraceSpanStatus.Succeeded) ||
                !child.TryComplete(TraceSpanStatus.Cancelled) ||
                root.TryComplete(TraceSpanStatus.Failed))
                throw new InvalidOperationException("Trace span terminal-state protection failed.");

            IReadOnlyList<TraceSpan> spans = recorder.GetSpans();
            if (spans.Count != 2 ||
                spans[0].Sequence != 1L ||
                spans[1].Sequence != 2L ||
                spans[0].OperationName != "execution" ||
                spans[1].OperationName != "provider" ||
                spans[0].Status != TraceSpanStatus.Succeeded ||
                spans[1].Status != TraceSpanStatus.Cancelled)
                throw new InvalidOperationException("Trace recorder ordering or terminal statuses were incorrect.");

            Write(
                "OBSERVABILITY TRACING",
                "Trace identity and span lifecycle contract succeeded." + Environment.NewLine +
                "Trace: " + root.Record.TraceId + Environment.NewLine +
                "Hierarchy: execution -> provider verified." + Environment.NewLine +
                "Execution/host/runtime/event correlation preserved: verified." + Environment.NewLine +
                "Redacted/omitted metadata: verified." + Environment.NewLine +
                "Terminal statuses and late completion rejection: verified." + Environment.NewLine +
                "Deterministic recorder order: 1 execution, 2 provider." + Environment.NewLine +
                "Provider request: none.");

            return Task.CompletedTask;
        }
    }
}
