using System.Collections.Generic;

namespace HAgent.Models
{
    /// <summary>
    /// Optional runtime-only behavioral overrides. Provider/model selection remains owned by execution policy.
    /// Values never mutate the persisted agent profile.
    /// </summary>
    public sealed class AgentRuntimeOverrides
    {
        public double? Temperature { get; set; }
        public int? MaxOutputTokens { get; set; }
        public string SystemPrompt { get; set; }
        public IDictionary<string, string> Context { get; private set; }

        public AgentRuntimeOverrides()
        {
            Context = new Dictionary<string, string>();
        }
    }
}
