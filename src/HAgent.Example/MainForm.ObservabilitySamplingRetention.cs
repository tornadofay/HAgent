using System;
using System.Collections.Generic;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddObservabilitySamplingRetentionTab()
        {
            AddApiTab(
                "Observability Sampling & Retention",
                "Run sampling & retention test",
                "Runs deterministic sampling and bounded in-memory retention checks through the public tracing APIs.",
                "The scenario verifies deterministic root sampling, sampled-state inheritance, trace-count and per-trace span bounds, aggregate metadata limits, and lifecycle independence when records are not retained.",
                "No network provider is contacted. Sampling and retention affect telemetry storage only; span completion remains independent.",
                RunObservabilitySamplingRetentionTest,
                "Sampling + retention",
                "This Slice 4 scenario exercises provider-neutral telemetry controls; exporters, persistent sinks, and diagnostic UI remain later slices.");
        }

        private void RunObservabilitySamplingRetentionTest(string unused)
        {
            var stableOptions = new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution",
                Correlation = new TraceCorrelation { ExecutionId = "example-sampling-42" }
            };
            var stableSampler = new DeterministicTraceSampler(new TraceSamplingOptions
            {
                SampleRate = 0.5d,
                Salt = "hagent-example"
            });

            var firstDecision = stableSampler.ShouldSample(stableOptions);
            var secondDecision = stableSampler.ShouldSample(stableOptions);
            if (firstDecision != secondDecision)
                throw new InvalidOperationException("Deterministic sampling produced inconsistent decisions.");

            var noneRecorder = new InMemoryTraceRecorder(
                new DeterministicTraceSampler(new TraceSamplingOptions { SampleRate = 0d }),
                new TraceRetentionOptions());
            var unsampledRoot = StartExampleRoot(noneRecorder, "example-unsampled-42");
            var unsampledChild = noneRecorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = unsampledRoot.Context,
                OperationName = "provider.invoke",
                Kind = "Provider",
                Correlation = unsampledRoot.Record.Correlation
            });
            unsampledRoot.TryComplete(TraceSpanStatus.Succeeded);
            unsampledChild.TryComplete(TraceSpanStatus.Succeeded);
            if (unsampledRoot.Context.Sampled || unsampledChild.Context.Sampled || noneRecorder.GetSpans().Count != 0)
                throw new InvalidOperationException("Unsampled trace state was not inherited or suppressed correctly.");

            var retention = new TraceRetentionOptions
            {
                MaxTraceCount = 2,
                MaxSpanCount = 8,
                MaxSpansPerTrace = 2,
                MaxAggregateMetadataCharacters = 200,
                MaxAge = TimeSpan.FromHours(1)
            };
            var recorder = new InMemoryTraceRecorder(
                new DeterministicTraceSampler(new TraceSamplingOptions { SampleRate = 1d }),
                retention);

            var first = StartExampleRoot(recorder, "example-retention-1");
            first.TryComplete(TraceSpanStatus.Succeeded);
            var second = StartExampleRoot(recorder, "example-retention-2");
            second.TryComplete(TraceSpanStatus.Succeeded);
            var third = StartExampleRoot(recorder, "example-retention-3");
            third.TryComplete(TraceSpanStatus.Succeeded);

            var retained = recorder.GetSpans();
            var traceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var span in retained)
                traceIds.Add(span.TraceId);
            if (traceIds.Count > 2)
                throw new InvalidOperationException("Maximum retained trace count was exceeded.");

            var boundedRoot = StartExampleRoot(recorder, "example-per-trace-42");
            var boundedChild1 = StartExampleChild(recorder, boundedRoot, "tool.execute");
            var boundedChild2 = StartExampleChild(recorder, boundedRoot, "context.assemble");
            boundedRoot.TryComplete(TraceSpanStatus.Succeeded);
            boundedChild1.TryComplete(TraceSpanStatus.Succeeded);
            boundedChild2.TryComplete(TraceSpanStatus.Succeeded);

            var boundedCount = CountForExecution(recorder.GetSpans(), "example-per-trace-42");
            if (boundedCount > 2)
                throw new InvalidOperationException("Maximum spans per trace was exceeded.");

            var oversizedMetadata = new TraceMetadata();
            oversizedMetadata.Add("diagnostic", new string('x', 250));
            var oversized = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "oversized",
                Kind = "Diagnostic",
                Correlation = new TraceCorrelation { ExecutionId = "example-oversized-42" },
                Metadata = oversizedMetadata
            });
            if (!oversized.TryComplete(TraceSpanStatus.Succeeded) || !oversized.IsCompleted)
                throw new InvalidOperationException("A non-retained span could not complete normally.");
            if (ContainsExecution(recorder.GetSpans(), "example-oversized-42"))
                throw new InvalidOperationException("Aggregate retention limit did not suppress the oversized span.");

            var finalSpans = recorder.GetSpans();
            if (finalSpans.Count > retention.MaxSpanCount)
                throw new InvalidOperationException("Maximum retained span count was exceeded.");

            Write(
                "OBSERVABILITY SAMPLING & RETENTION",
                "Sampling and bounded retention controls succeeded." + Environment.NewLine +
                "Deterministic sampler stability: verified." + Environment.NewLine +
                "Root sampling rate 0 + child inheritance: verified." + Environment.NewLine +
                "Sampled records suppressed without affecting span completion: verified." + Environment.NewLine +
                "Maximum retained traces: verified." + Environment.NewLine +
                "Maximum spans per trace: verified." + Environment.NewLine +
                "Aggregate metadata retention bound: verified." + Environment.NewLine +
                "Maximum retained spans: verified." + Environment.NewLine +
                "Execution correctness is independent from sampling/retention: verified." + Environment.NewLine +
                "Provider transport: none." + Environment.NewLine +
                "Real provider request: none.");
        }

        private static ITraceSpan StartExampleRoot(ITraceRecorder recorder, string executionId)
        {
            return recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution",
                Correlation = new TraceCorrelation { ExecutionId = executionId }
            });
        }

        private static ITraceSpan StartExampleChild(ITraceRecorder recorder, ITraceSpan parent, string operationName)
        {
            return recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = parent.Context,
                OperationName = operationName,
                Kind = "Diagnostic",
                Correlation = parent.Record.Correlation
            });
        }

        private static int CountForExecution(IReadOnlyList<TraceSpan> spans, string executionId)
        {
            var count = 0;
            foreach (var span in spans)
            {
                if (string.Equals(span.Correlation.ExecutionId, executionId, StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            return count;
        }

        private static bool ContainsExecution(IReadOnlyList<TraceSpan> spans, string executionId)
        {
            foreach (var span in spans)
            {
                if (string.Equals(span.Correlation.ExecutionId, executionId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
