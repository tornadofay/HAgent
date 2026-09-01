using System.Collections.Generic;

namespace HAgent.Models
{
    /// <summary>
    /// Optional runtime-only overrides. Values never mutate the persisted agent profile.
    /// </summary>
    public sealed class AgentRuntimeOverrides
    {
        public string ProviderId { get; set; }
        public string Model { get; set; }
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
