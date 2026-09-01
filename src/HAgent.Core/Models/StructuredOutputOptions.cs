using System;

namespace HAgent.Models
{
    /// <summary>
    /// Host-owned structured-output requirements for one execution.
    /// The schema is provider-neutral and is validated by HAgent after provider normalization.
    /// </summary>
    public sealed class StructuredOutputOptions
    {
        public StructuredOutputOptions()
        {
            SchemaJson = string.Empty;
            RequireValidJson = true;
        }

        public string SchemaJson { get; set; }
        public bool RequireValidJson { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SchemaJson))
                throw new ArgumentException("Structured-output schema is required when structured output is configured.", nameof(SchemaJson));
            if (SchemaJson.Length > 65536)
                throw new ArgumentOutOfRangeException(nameof(SchemaJson), "Structured-output schema must not exceed 65536 characters.");
        }
    }
}
