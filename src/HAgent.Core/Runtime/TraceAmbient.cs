using System;
using System.Threading;
using HAgent.Models;

namespace HAgent.Runtime
{
    internal static class TraceAmbient
    {
        private sealed class AmbientState
        {
            public TraceContext Context;
            public TraceCorrelation Correlation;
            public ITraceRecorder Recorder;
        }

        private static readonly AsyncLocal<AmbientState> CurrentValue = new AsyncLocal<AmbientState>();

        public static TraceContext Current
        {
            get { return CurrentValue.Value == null ? null : CurrentValue.Value.Context; }
        }

        public static TraceCorrelation CurrentCorrelation
        {
            get { return CurrentValue.Value == null || CurrentValue.Value.Correlation == null ? null : CurrentValue.Value.Correlation.Clone(); }
        }

        public static ITraceRecorder CurrentRecorder
        {
            get { return CurrentValue.Value == null ? null : CurrentValue.Value.Recorder; }
        }

        public static IDisposable Push(TraceContext context)
        {
            return Push(context, null, CurrentRecorder);
        }

        public static IDisposable Push(TraceContext context, TraceCorrelation correlation)
        {
            return Push(context, correlation, CurrentRecorder);
        }

        public static IDisposable Push(TraceContext context, TraceCorrelation correlation, ITraceRecorder recorder)
        {
            var previous = CurrentValue.Value;
            CurrentValue.Value = new AmbientState
            {
                Context = context,
                Correlation = correlation == null ? null : correlation.Clone(),
                Recorder = recorder
            };
            return new Scope(previous);
        }

        public static void Set(TraceContext context, TraceCorrelation correlation)
        {
            Set(context, correlation, CurrentRecorder);
        }

        public static void Set(TraceContext context, TraceCorrelation correlation, ITraceRecorder recorder)
        {
            CurrentValue.Value = new AmbientState
            {
                Context = context,
                Correlation = correlation == null ? null : correlation.Clone(),
                Recorder = recorder
            };
        }

        private sealed class Scope : IDisposable
        {
            private readonly AmbientState _previous;
            private int _disposed;

            public Scope(AmbientState previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                    return;
                CurrentValue.Value = _previous;
            }
        }
    }
}
