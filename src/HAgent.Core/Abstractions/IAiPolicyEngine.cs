using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiPolicyEngine
    {
        string PolicyVersion { get; }

        /// <summary>
        /// Returns an owned policy snapshot that can be captured by an execution boundary.
        /// </summary>
        AiPolicySet GetPolicySnapshot();

        AiPolicyDecision Evaluate(AiPolicyEvaluationContext context);
    }
}
