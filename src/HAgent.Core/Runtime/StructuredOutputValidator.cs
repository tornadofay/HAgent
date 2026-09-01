using System;
using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Validates normalized structured output against the host-owned JSON Schema contract.
    /// The initial contract intentionally supports object-root schemas, matching the existing
    /// provider-neutral tool-schema validation semantics.
    /// </summary>
    public static class StructuredOutputValidator
    {
        public static ToolValidationResult Validate(StructuredOutputOptions options, string structuredOutputJson)
        {
            if (options == null)
                return ToolValidationResult.Success(new Dictionary<string, object>());

            options.Validate();

            IDictionary<string, object> output;
            string parseError;
            if (!ToolArgumentParser.TryParseObject(structuredOutputJson, out output, out parseError))
            {
                return ToolValidationResult.Failure(new[]
                {
                    "Structured output must be a JSON object: " + parseError
                });
            }

            var definition = new AiTool
            {
                Id = "hagent.structured-output",
                Name = "Structured Output",
                InputSchemaJson = options.SchemaJson,
                Enabled = true
            };

            var schemaResult = ToolSchemaValidator.ValidateSchema(definition);
            if (!schemaResult.IsValid)
                return schemaResult;

            return ToolSchemaValidator.Validate(definition, output);
        }
    }
}
