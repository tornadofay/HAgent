using System;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Explicit provider-neutral propagation scope for nested HAgent operations.
    /// </summary>
    public static class TracePropagation
    {
        public static TraceContext Current
        {
            get { return TraceAmbient.Current; }
        }

        public static IDisposable Push(TraceContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return TraceAmbient.Push(context);
        }
    }
}
