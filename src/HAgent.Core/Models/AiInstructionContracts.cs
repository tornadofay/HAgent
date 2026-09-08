using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HAgent.Models
{
    public enum AiInstructionSourceType
    {
        SystemPolicy = 0,
        Agent = 1,
        Skill = 2,
        Knowledge = 3,
        Memory = 4,
        ToolDescription = 5,
        RuntimeContext = 6,
        HostContext = 7,
        UserInput = 8,
        ExternalContent = 9,
        ModelGenerated = 10
    }

    public enum AiInstructionAuthority
    {
        Untrusted = 0,
        External = 10,
        User = 20,
        Runtime = 30,
        TrustedResource = 40,
        Agent = 50,
        SystemPolicy = 60
    }

    public enum AiInstructionTrustLevel
    {
        Untrusted = 0,
        External = 10,
        UserSupplied = 20,
        HostTrusted = 30,
        HAgentTrusted = 40,
        SystemTrusted = 50
    }

    public enum AiInstructionLifecycleState
    {
        Active = 0,
        Disabled = 1,
        Expired = 2,
        Revoked = 3
    }

    public enum AiInstructionConflictDisposition
    {
        Unresolved = 0,
        HigherPrecedenceWins = 1,
        Rejected = 2
    }

    public sealed class AiInstructionScope
    {
        public AiInstructionScope()
        {
            ScopeType = "Global";
            ScopeId = string.Empty;
        }

        public string ScopeType { get; set; }
        public string ScopeId { get; set; }

        public AiInstructionScope Clone()
        {
            return new AiInstructionScope
            {
                ScopeType = ScopeType,
                ScopeId = ScopeId
            };
        }

        public int Specificity
        {
            get
            {
                switch ((ScopeType ?? string.Empty).Trim().ToUpperInvariant())
                {
                    case "EXECUTION": return 7;
                    case "RUNTIME": return 6;
                    case "AGENT": return 5;
                    case "WORKSPACE": return 4;
                    case "USER": return 3;
                    case "TENANT": return 2;
                    case "GLOBAL": return 1;
                    default: return 0;
                }
            }
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ScopeType))
                throw new ArgumentException("Instruction scope type is required.", nameof(ScopeType));
            if (ScopeType.Trim().Length > 64)
                throw new ArgumentOutOfRangeException(nameof(ScopeType));
            if (ScopeId != null && ScopeId.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(ScopeId));
        }
    }

    public sealed class AiInstructionProvenance
    {
        public AiInstructionProvenance()
        {
            SourceKind = string.Empty;
            SourceId = string.Empty;
            SourceVersion = string.Empty;
            ExecutionId = string.Empty;
            RuntimeInstanceId = string.Empty;
            PrincipalId = string.Empty;
            Evidence = string.Empty;
            CapturedAt = DateTimeOffset.UtcNow;
        }

        public string SourceKind { get; set; }
        public string SourceId { get; set; }
        public string SourceVersion { get; set; }
        public string ExecutionId { get; set; }
        public string RuntimeInstanceId { get; set; }
        public string PrincipalId { get; set; }
        public string Evidence { get; set; }
        public DateTimeOffset CapturedAt { get; set; }

        public AiInstructionProvenance Clone()
        {
            return new AiInstructionProvenance
            {
                SourceKind = SourceKind,
                SourceId = SourceId,
                SourceVersion = SourceVersion,
                ExecutionId = ExecutionId,
                RuntimeInstanceId = RuntimeInstanceId,
                PrincipalId = PrincipalId,
                Evidence = Evidence,
                CapturedAt = CapturedAt
            };
        }

        public void Validate()
        {
            ValidateLength(SourceKind, 128, nameof(SourceKind));
            ValidateLength(SourceId, 256, nameof(SourceId));
            ValidateLength(SourceVersion, 128, nameof(SourceVersion));
            ValidateLength(ExecutionId, 128, nameof(ExecutionId));
            ValidateLength(RuntimeInstanceId, 128, nameof(RuntimeInstanceId));
            ValidateLength(PrincipalId, 256, nameof(PrincipalId));
            ValidateLength(Evidence, 2000, nameof(Evidence));
            if (CapturedAt == default(DateTimeOffset))
                throw new ArgumentException("Provenance capture time is required.", nameof(CapturedAt));
        }

        private static void ValidateLength(string value, int max, string name)
        {
            if (value != null && value.Length > max)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiInstructionSource
    {
        public AiInstructionSource()
        {
            Id = Guid.NewGuid().ToString("N");
            Name = string.Empty;
            Content = string.Empty;
            SourceType = AiInstructionSourceType.UserInput;
            Authority = AiInstructionAuthority.User;
            TrustLevel = AiInstructionTrustLevel.UserSupplied;
            Scope = new AiInstructionScope();
            Lifecycle = AiInstructionLifecycleState.Active;
            Priority = 0;
            ConflictKey = string.Empty;
            Version = string.Empty;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            Provenance = new AiInstructionProvenance();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public AiInstructionSourceType SourceType { get; set; }
        public AiInstructionAuthority Authority { get; set; }
        public AiInstructionTrustLevel TrustLevel { get; set; }
        public AiInstructionScope Scope { get; set; }
        public AiInstructionLifecycleState Lifecycle { get; set; }
        public int Priority { get; set; }
        public string ConflictKey { get; set; }
        public string Version { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? NotBefore { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public string Content { get; set; }
        public AiInstructionProvenance Provenance { get; set; }

        public bool IsActiveAt(DateTimeOffset at)
        {
            return Lifecycle == AiInstructionLifecycleState.Active &&
                   (!NotBefore.HasValue || at >= NotBefore.Value) &&
                   (!ExpiresAt.HasValue || at < ExpiresAt.Value);
        }

        public AiInstructionSource Clone()
        {
            return new AiInstructionSource
            {
                Id = Id,
                Name = Name,
                SourceType = SourceType,
                Authority = Authority,
                TrustLevel = TrustLevel,
                Scope = Scope == null ? new AiInstructionScope() : Scope.Clone(),
                Lifecycle = Lifecycle,
                Priority = Priority,
                ConflictKey = ConflictKey,
                Version = Version,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                NotBefore = NotBefore,
                ExpiresAt = ExpiresAt,
                Content = Content,
                Provenance = Provenance == null ? new AiInstructionProvenance() : Provenance.Clone()
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentException("Instruction source ID is required.", nameof(Id));
            if (Id.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(Id));
            ValidateLength(Name, 256, nameof(Name));
            ValidateLength(ConflictKey, 256, nameof(ConflictKey));
            ValidateLength(Version, 128, nameof(Version));
            if (Content == null)
                throw new ArgumentException("Instruction content is required.", nameof(Content));
            if (Content.Length > 65536)
                throw new ArgumentOutOfRangeException(nameof(Content));
            if (!Enum.IsDefined(typeof(AiInstructionSourceType), SourceType))
                throw new ArgumentOutOfRangeException(nameof(SourceType));
            if (!Enum.IsDefined(typeof(AiInstructionAuthority), Authority))
                throw new ArgumentOutOfRangeException(nameof(Authority));
            if (!Enum.IsDefined(typeof(AiInstructionTrustLevel), TrustLevel))
                throw new ArgumentOutOfRangeException(nameof(TrustLevel));
            if (!Enum.IsDefined(typeof(AiInstructionLifecycleState), Lifecycle))
                throw new ArgumentOutOfRangeException(nameof(Lifecycle));
            if (Scope == null)
                throw new ArgumentException("Instruction scope is required.", nameof(Scope));
            Scope.Validate();
            if (NotBefore.HasValue && ExpiresAt.HasValue && ExpiresAt.Value <= NotBefore.Value)
                throw new ArgumentException("Instruction expiry must be after its activation time.", nameof(ExpiresAt));
            if (CreatedAt == default(DateTimeOffset) || UpdatedAt == default(DateTimeOffset))
                throw new ArgumentException("Instruction lifecycle timestamps are required.");
            if (UpdatedAt < CreatedAt)
                throw new ArgumentException("Instruction updated time cannot precede created time.", nameof(UpdatedAt));
            if (Provenance == null)
                throw new ArgumentException("Instruction provenance is required.", nameof(Provenance));
            Provenance.Validate();
        }

        private static void ValidateLength(string value, int max, string name)
        {
            if (value != null && value.Length > max)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiInstructionConflict
    {
        public AiInstructionConflict()
        {
            Id = Guid.NewGuid().ToString("N");
            ConflictKey = string.Empty;
            SourceIds = new List<string>();
            WinnerSourceId = string.Empty;
            Disposition = AiInstructionConflictDisposition.Unresolved;
            Reason = string.Empty;
            DetectedAt = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }
        public string ConflictKey { get; set; }
        public IList<string> SourceIds { get; private set; }
        public string WinnerSourceId { get; set; }
        public AiInstructionConflictDisposition Disposition { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset DetectedAt { get; set; }

        public AiInstructionConflict Clone()
        {
            var clone = new AiInstructionConflict
            {
                Id = Id,
                ConflictKey = ConflictKey,
                WinnerSourceId = WinnerSourceId,
                Disposition = Disposition,
                Reason = Reason,
                DetectedAt = DetectedAt
            };
            foreach (var sourceId in SourceIds ?? new List<string>())
                clone.SourceIds.Add(sourceId);
            return clone;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Id) || Id.Length > 256)
                throw new ArgumentException("Instruction conflict ID is invalid.", nameof(Id));
            if (ConflictKey == null || ConflictKey.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(ConflictKey));
            if (!Enum.IsDefined(typeof(AiInstructionConflictDisposition), Disposition))
                throw new ArgumentOutOfRangeException(nameof(Disposition));
            if (Reason != null && Reason.Length > 2000)
                throw new ArgumentOutOfRangeException(nameof(Reason));
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var sourceId in SourceIds ?? new List<string>())
            {
                if (string.IsNullOrWhiteSpace(sourceId) || !unique.Add(sourceId))
                    throw new ArgumentException("Conflict source IDs must be non-empty and unique.", nameof(SourceIds));
            }
            if (!string.IsNullOrWhiteSpace(WinnerSourceId) && !unique.Contains(WinnerSourceId))
                throw new ArgumentException("Conflict winner must be one of the conflicting sources.", nameof(WinnerSourceId));
        }
    }

    public static class AiInstructionPrecedence
    {
        public static int Compare(AiInstructionSource left, AiInstructionSource right)
        {
            if (left == null) return right == null ? 0 : -1;
            if (right == null) return 1;

            left.Validate();
            right.Validate();

            var comparison = left.Authority.CompareTo(right.Authority);
            if (comparison != 0) return comparison;

            comparison = left.Priority.CompareTo(right.Priority);
            if (comparison != 0) return comparison;

            comparison = left.TrustLevel.CompareTo(right.TrustLevel);
            if (comparison != 0) return comparison;

            comparison = left.Scope.Specificity.CompareTo(right.Scope.Specificity);
            if (comparison != 0) return comparison;

            comparison = SourceTypePrecedence(left.SourceType).CompareTo(SourceTypePrecedence(right.SourceType));
            if (comparison != 0) return comparison;

            return string.Compare(left.Id, right.Id, StringComparison.OrdinalIgnoreCase) * -1;
        }

        public static AiInstructionSource SelectWinner(IEnumerable<AiInstructionSource> sources)
        {
            var ordered = (sources ?? Enumerable.Empty<AiInstructionSource>())
                .Where(x => x != null)
                .Where(x => x.IsActiveAt(DateTimeOffset.UtcNow))
                .OrderByDescending(x => x, Comparer<AiInstructionSource>.Create(Compare))
                .ToList();

            return ordered.Count == 0 ? null : ordered[0];
        }

        private static int SourceTypePrecedence(AiInstructionSourceType type)
        {
            switch (type)
            {
                case AiInstructionSourceType.SystemPolicy: return 100;
                case AiInstructionSourceType.Agent: return 90;
                case AiInstructionSourceType.Skill: return 80;
                case AiInstructionSourceType.Knowledge: return 70;
                case AiInstructionSourceType.Memory: return 60;
                case AiInstructionSourceType.ToolDescription: return 50;
                case AiInstructionSourceType.RuntimeContext: return 40;
                case AiInstructionSourceType.HostContext: return 30;
                case AiInstructionSourceType.UserInput: return 20;
                case AiInstructionSourceType.ExternalContent: return 10;
                case AiInstructionSourceType.ModelGenerated: return 0;
                default: return -1;
            }
        }
    }

    public sealed class AiInstructionSnapshot
    {
        public AiInstructionSnapshot(IEnumerable<AiInstructionSource> sources, IEnumerable<AiInstructionConflict> conflicts)
        {
            var sourceList = new List<AiInstructionSource>();
            foreach (var source in sources ?? Enumerable.Empty<AiInstructionSource>())
            {
                if (source == null) throw new ArgumentException("Instruction snapshot cannot contain null sources.", nameof(sources));
                source.Validate();
                sourceList.Add(source.Clone());
            }

            var conflictList = new List<AiInstructionConflict>();
            foreach (var conflict in conflicts ?? Enumerable.Empty<AiInstructionConflict>())
            {
                if (conflict == null) throw new ArgumentException("Instruction snapshot cannot contain null conflicts.", nameof(conflicts));
                conflict.Validate();
                conflictList.Add(conflict.Clone());
            }

            Sources = new ReadOnlyCollection<AiInstructionSource>(sourceList);
            Conflicts = new ReadOnlyCollection<AiInstructionConflict>(conflictList);
        }

        public IReadOnlyList<AiInstructionSource> Sources { get; private set; }
        public IReadOnlyList<AiInstructionConflict> Conflicts { get; private set; }

        public AiInstructionSnapshot Clone()
        {
            return new AiInstructionSnapshot(Sources, Conflicts);
        }
    }
}
