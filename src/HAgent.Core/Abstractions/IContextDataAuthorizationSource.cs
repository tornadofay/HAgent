using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Opt-in marker for context sources whose retrieval is a host-authorized data operation.
    /// It does not authorize the operation; ContextPolicyAssembler delegates that decision to the host IDataAccessAuthorizer.
    /// </summary>
    public interface IContextDataAuthorizationSource
    {
        DataAccessOperation AuthorizationOperation { get; }
        DataQueryRequest AuthorizationQuery { get; }
        IReadOnlyDictionary<string, object> AuthorizationRuntimeContext { get; }
    }
}
