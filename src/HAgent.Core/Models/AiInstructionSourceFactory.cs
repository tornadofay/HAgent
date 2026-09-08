using System;

namespace HAgent.Models
{
    /// <summary>
    /// Creates canonical instruction sources for resource and external-content boundaries.
    /// This factory assigns instruction authority/trust metadata only; it never grants authorization.
    /// </summary>
    public static class AiInstructionSourceFactory
    {
        public static AiInstructionSource CreateResource(
            AiInstructionSourceType sourceType,
            string sourceId,
            string content,
            string resourceVersion,
            AiInstructionScope scope,
            string conflictKey = null)
        {
            if (sourceType != AiInstructionSourceType.Skill &&
                sourceType != AiInstructionSourceType.Knowledge &&
                sourceType != AiInstructionSourceType.Memory &&
                sourceType != AiInstructionSourceType.ToolDescription)
                throw new ArgumentException("The resource source type must be Skill, Knowledge, Memory, or ToolDescription.", nameof(sourceType));

            return Create(
                sourceId,
                sourceType,
                AiInstructionAuthority.TrustedResource,
                AiInstructionTrustLevel.HAgentTrusted,
                content,
                resourceVersion,
                scope,
                "resource",
                sourceId,
                conflictKey);
        }

        public static AiInstructionSource CreateRuntimeContext(
            string sourceId,
            string content,
            AiInstructionScope scope,
            string conflictKey = null)
        {
            return Create(
                sourceId,
                AiInstructionSourceType.RuntimeContext,
                AiInstructionAuthority.Runtime,
                AiInstructionTrustLevel.HostTrusted,
                content,
                string.Empty,
                scope,
                "runtime",
                sourceId,
                conflictKey);
        }

        public static AiInstructionSource CreateHostContext(
            string sourceId,
            string content,
            AiInstructionScope scope,
            string conflictKey = null)
        {
            return Create(
                sourceId,
                AiInstructionSourceType.HostContext,
                AiInstructionAuthority.Runtime,
                AiInstructionTrustLevel.HostTrusted,
                content,
                string.Empty,
                scope,
                "host",
                sourceId,
                conflictKey);
        }

        public static AiInstructionSource CreateExternalContent(
            string sourceId,
            string content,
            string evidence,
            string contentVersion,
            AiInstructionScope scope,
            string conflictKey = null)
        {
            var source = Create(
                sourceId,
                AiInstructionSourceType.ExternalContent,
                AiInstructionAuthority.External,
                AiInstructionTrustLevel.External,
                content,
                contentVersion,
                scope,
                "external",
                sourceId,
                conflictKey);
            source.Provenance.Evidence = evidence ?? string.Empty;
            return source;
        }

        public static AiInstructionSource CreateUserInput(
            string sourceId,
            string content,
            AiInstructionScope scope,
            string conflictKey = null)
        {
            return Create(
                sourceId,
                AiInstructionSourceType.UserInput,
                AiInstructionAuthority.User,
                AiInstructionTrustLevel.UserSupplied,
                content,
                string.Empty,
                scope,
                "request",
                sourceId,
                conflictKey);
        }

        private static AiInstructionSource Create(
            string sourceId,
            AiInstructionSourceType sourceType,
            AiInstructionAuthority authority,
            AiInstructionTrustLevel trust,
            string content,
            string version,
            AiInstructionScope scope,
            string provenanceKind,
            string provenanceId,
            string conflictKey)
        {
            var capturedAt = DateTimeOffset.UtcNow;
            var source = new AiInstructionSource
            {
                Id = sourceId,
                Name = sourceId,
                SourceType = sourceType,
                Authority = authority,
                TrustLevel = trust,
                Scope = scope == null ? new AiInstructionScope() : scope.Clone(),
                Lifecycle = AiInstructionLifecycleState.Active,
                Priority = 0,
                ConflictKey = conflictKey ?? string.Empty,
                Version = version ?? string.Empty,
                CreatedAt = capturedAt,
                UpdatedAt = capturedAt,
                Content = content ?? string.Empty,
                Provenance = new AiInstructionProvenance
                {
                    SourceKind = provenanceKind,
                    SourceId = provenanceId,
                    SourceVersion = version ?? string.Empty,
                    Evidence = string.Empty,
                    CapturedAt = capturedAt
                }
            };
            source.Validate();
            return source;
        }
    }
}
