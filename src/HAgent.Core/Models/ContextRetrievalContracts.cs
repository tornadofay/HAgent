using System;
using HAgent.Abstractions;

namespace HAgent.Models
{
    /// <summary>
    /// Standard producer categories for context retrieval. Custom kinds remain valid and extensible.
    /// </summary>
    public static class ContextSourceKinds
    {
        public const string Memory = "memory";
        public const string Knowledge = "knowledge";
        public const string Skill = "skill";
        public const string Conversation = "conversation";
        public const string HostContext = "host-context";
        public const string ToolDescription = "tool-description";
        public const string Instruction = "instruction";
    }

    /// <summary>
    /// Bounded retrieval request for one concrete context source.
    /// Source authorization and enablement remain outside this contract.
    /// </summary>
    public sealed class ContextRetrievalSource
    {
        public ContextRetrievalSource()
        {
            Query = string.Empty;
            MaxItems = 100;
        }

        public IContextSource Source { get; set; }
        public string Query { get; set; }
        public int MaxItems { get; set; }

        public ContextSourceRequest CreateRequest(int maximumItems = 1000)
        {
            Validate(maximumItems);
            return new ContextSourceRequest
            {
                Query = Query,
                MaxItems = MaxItems
            };
        }

        public void Validate(int maximumItems = 1000)
        {
            if (Source == null)
                throw new ArgumentNullException(nameof(Source));

            if (string.IsNullOrWhiteSpace(Source.Id))
                throw new ArgumentException("Context source ID is required.", nameof(Source));
            if (Source.Id.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(Source), "Context source ID is too long.");
            if (string.IsNullOrWhiteSpace(Source.Kind))
                throw new ArgumentException("Context source kind is required.", nameof(Source));
            if (Source.Kind.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(Source), "Context source kind is too long.");

            if (Query != null && Query.Length > 4096)
                throw new ArgumentOutOfRangeException(nameof(Query));
            if (MaxItems < 1 || MaxItems > maximumItems)
                throw new ArgumentOutOfRangeException(nameof(MaxItems));
        }
    }
}
