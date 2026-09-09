using System;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Optional runtime capability that publishes authoritative execution outcome facts.
    /// Implementations remain independent of any tracing or telemetry vendor.
    /// </summary>
    public interface IExecutionObservationSource
    {
        event EventHandler<AgentExecutionObservationEventArgs> ExecutionObserved;
    }
}
