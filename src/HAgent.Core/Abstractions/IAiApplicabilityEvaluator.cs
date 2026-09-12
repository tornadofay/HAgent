using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiApplicabilityEvaluator
    {
        Task<AiApplicabilityDecision> EvaluateAsync(
            AiApplicabilityRequest request,
            CancellationToken cancellationToken);
    }
}
