using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiEvaluator
    {
        string Id { get; }
        AiEvaluatorKind Kind { get; }
        string Version { get; }

        Task<AiEvaluation> EvaluateAsync(AiEvaluationRequest request, CancellationToken cancellationToken);
    }
}
