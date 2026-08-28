namespace HAgent.Models
{
    public sealed class AIMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }

        public AIMessage()
        {
            Role = string.Empty;
            Content = string.Empty;
        }

        public AIMessage(string role, string content)
        {
            Role = role ?? string.Empty;
            Content = content ?? string.Empty;
        }
    }
}
