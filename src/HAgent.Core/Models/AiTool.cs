using System;

namespace HAgent.Models
{
    public sealed class AiTool
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string InputSchemaJson { get; set; }
        public string Category { get; set; }
        public bool IsBuiltIn { get; set; }
        public bool Enabled { get; set; }

        public AiTool()
        {
            Id = Guid.NewGuid().ToString("N");
            Name = "New Tool";
            Description = string.Empty;
            InputSchemaJson = "{\"type\":\"object\",\"properties\":{}}";
            Category = "Custom";
            IsBuiltIn = false;
            Enabled = true;
        }
    }
}
