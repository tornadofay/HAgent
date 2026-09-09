using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Provider-neutral boundary for a model-assisted evaluation judge.
    /// Implementations may be backed by a provider adapter or another host-owned
    /// grading service, but the Core evaluation layer only exchanges bounded contracts.
    /// </summary>
    public interface IAiEvaluationJudge
    {
        string Id { get; }
        string Version { get; }

        Task<AiEvaluationRating> JudgeAsync(
            AiEvaluationJudgeRequest request,
            CancellationToken cancellationToken);
    }
}
