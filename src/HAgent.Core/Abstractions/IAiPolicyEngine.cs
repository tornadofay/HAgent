using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiPolicyEngine
    {
        string PolicyVersion { get; }
        AiPolicyDecision Evaluate(AiPolicyEvaluationContext context);
    }
}
