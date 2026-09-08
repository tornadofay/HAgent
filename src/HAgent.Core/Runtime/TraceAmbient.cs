using System;
using System.Threading;
using HAgent.Models;

namespace HAgent.Runtime
{
    internal static class TraceAmbient
    {
        private static readonly AsyncLocal<TraceContext> CurrentValue = new AsyncLocal<TraceContext>();

        public static TraceContext Current
        {
            get { return CurrentValue.Value; }
        }

        public static IDisposable Push(TraceContext context)
        {
            var previous = CurrentValue.Value;
            CurrentValue.Value = context;
            return new Scope(previous);
        }

        private sealed class Scope : IDisposable
        {
            private readonly TraceContext _previous;
            private int _disposed;

            public Scope(TraceContext previous)
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
