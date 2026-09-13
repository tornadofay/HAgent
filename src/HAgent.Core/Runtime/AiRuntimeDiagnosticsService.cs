using System;
using HAgent.Models;

namespace HAgent.Runtime
{
    public static class AiRuntimeDiagnosticsService
    {
        public static AiRuntimeDiagnosticsSnapshot Capture(AgentRuntimeInstance instance, DateTimeOffset? capturedAt = null)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            return new AiRuntimeDiagnosticsSnapshot(instance, capturedAt ?? DateTimeOffset.UtcNow);
        }
    }
}
