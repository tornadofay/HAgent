using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IExecutionPlanner
    {
        AiExecutionPlan Plan(
            IReadOnlyCollection<AiExecutionTarget> candidates,
            AiCapabilityRequirements requirements,
            AiExecutionSelectionPolicy policy);
    }
}
