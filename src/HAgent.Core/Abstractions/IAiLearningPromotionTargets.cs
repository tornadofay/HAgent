using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiKnowledgePromotionTarget
    {
        Task<AiKnowledgeResource> PublishAsync(
            AiKnowledgeResource candidate,
            AiLearningPromotionContext context,
            CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IAiSkillPromotionTarget
    {
        Task<AiSkillDefinition> PublishAsync(
            AiSkillDefinition candidate,
            AiLearningPromotionContext context,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
