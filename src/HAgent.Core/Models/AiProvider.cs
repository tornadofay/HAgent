namespace HAgent.Models
{
    public sealed class AiProvider
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Kind { get; set; }
        public string BaseUrl { get; set; }
        public string DefaultModel { get; set; }
        public string DefaultSystemPrompt { get; set; }
        public string SecretId { get; set; }
        public bool Enabled { get; set; }

        public AiProvider()
        {
            Id = System.Guid.NewGuid().ToString("N");
            Name = "New Provider";
            Kind = "openai-compatible";
            BaseUrl = "https://api.openai.com/v1";
            DefaultModel = string.Empty;
            DefaultSystemPrompt = string.Empty;
            SecretId = string.Empty;
            Enabled = true;
        }
    }
}
