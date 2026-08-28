using System.Collections.Generic;
namespace HAgent.Models
{
    public sealed class AiAgent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProviderId { get; set; }
        public List<string> ProviderIds { get; set; }
        public string Model { get; set; }
        public string SystemPrompt { get; set; }
        public bool UseProviderSystemPrompt { get; set; }
        public double? Temperature { get; set; }
        public int? MaxOutputTokens { get; set; }
        public List<string> ToolIds { get; set; }
        public bool Enabled { get; set; }

        public AiAgent()
        {
            Id = System.Guid.NewGuid().ToString("N");
            Name = "New Agent";
            ProviderId = string.Empty;
            ProviderIds = new List<string>();
            Model = string.Empty;
            SystemPrompt = string.Empty;
            UseProviderSystemPrompt = true;
            Temperature = null;
            MaxOutputTokens = null;
            ToolIds = new List<string>();
            Enabled = true;
        }
    }
}
