using System;

namespace HAgent.Models
{
    /// <summary>
    /// Secret-safe, payload-free projection of an agent execution for observability and future audit persistence.
    /// </summary>
    public sealed class AgentExecutionAuditRecord
    {
        public string ExecutionId { get; set; }
        public string CorrelationId { get; set; }
        public string AgentId { get; set; }
        public string AgentName { get; set; }
        public string Model { get; set; }
        public string LastProviderId { get; set; }
        public string LastProviderName { get; set; }
        public Runtime.AgentExecutionState State { get; set; }
        public AgentExecutionFailureKind FailureKind { get; set; }
        public ProviderErrorKind ProviderErrorKind { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public TimeSpan? Duration { get; set; }

        public static AgentExecutionAuditRecord FromExecution(AgentExecution execution)
        {
            if (execution == null) throw new ArgumentNullException(nameof(execution));

            var providerName = string.Empty;
            if (execution.Snapshot != null && execution.Snapshot.Providers != null &&
                !string.IsNullOrWhiteSpace(execution.LastProviderId))
            {
                foreach (var provider in execution.Snapshot.Providers)
                {
                    if (provider != null && string.Equals(provider.Id, execution.LastProviderId, StringComparison.OrdinalIgnoreCase))
                    {
                        providerName = provider.Name ?? string.Empty;
                        break;
                    }
                }
            }

            var agent = execution.Snapshot == null ? null : execution.Snapshot.Agent;
            return new AgentExecutionAuditRecord
            {
                ExecutionId = execution.Id ?? string.Empty,
                CorrelationId = execution.CorrelationId ?? string.Empty,
                AgentId = agent == null ? string.Empty : agent.Id,
                AgentName = agent == null ? string.Empty : agent.Name,
                Model = agent == null ? string.Empty : agent.Model,
                LastProviderId = execution.LastProviderId ?? string.Empty,
                LastProviderName = providerName,
                State = execution.State,
                FailureKind = execution.FailureKind,
                ProviderErrorKind = execution.ProviderErrorKind,
                CreatedAt = execution.CreatedAt,
                StartedAt = execution.StartedAt,
                CompletedAt = execution.CompletedAt,
                Duration = execution.Duration
            };
        }
    }
}
