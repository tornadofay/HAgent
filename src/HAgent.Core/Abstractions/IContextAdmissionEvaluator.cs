using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Evaluates whether a context source or candidate may enter bounded context assembly.
    /// Implementations compose the existing policy and resource-capability authorities.
    /// </summary>
    public interface IContextAdmissionEvaluator
    {
        ContextAdmissionDecision EvaluateSource(
            ContextRetrievalSource source,
            ContextAdmissionContext context);

        ContextAdmissionDecision EvaluateItem(
            ContextRetrievalSource source,
            ContextItem item,
            ContextAdmissionContext context);
    }
}
