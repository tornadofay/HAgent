using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class AiInstructionCompositionResult
    {
        internal AiInstructionCompositionResult(
            AiInstructionSnapshot snapshot,
            string composedText,
            IEnumerable<string> diagnostics)
        {
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            ComposedText = composedText ?? string.Empty;
            Diagnostics = new ReadOnlyCollection<string>((diagnostics ?? Enumerable.Empty<string>()).ToList());
        }

        public AiInstructionSnapshot Snapshot { get; private set; }
        public string ComposedText { get; private set; }
        public IReadOnlyList<string> Diagnostics { get; private set; }
    }

    public static class AiInstructionComposer
    {
        public static AiInstructionCompositionResult Compose(
            IEnumerable<AiInstructionSource> sources,
            DateTimeOffset? at = null)
        {
            var pointInTime = at ?? DateTimeOffset.UtcNow;
            var diagnostics = new List<string>();
            var eligible = new List<AiInstructionSource>();

            foreach (var source in sources ?? Enumerable.Empty<AiInstructionSource>())
            {
                if (source == null)
                {
                    diagnostics.Add("Instruction source rejected: null source.");
                    continue;
                }

                try
                {
                    source.Validate();
                }
                catch (Exception ex)
                {
                    diagnostics.Add("Instruction source rejected: id='" + SafeId(source.Id) + "', reason='" + SafeReason(ex.Message) + "'.");
                    continue;
                }

                if (!source.IsActiveAt(pointInTime))
                {
                    diagnostics.Add("Instruction source excluded from composition: id='" + SafeId(source.Id) + "', lifecycle='" + source.Lifecycle + "'.");
                    continue;
                }

                eligible.Add(source.Clone());
            }

            var included = new List<AiInstructionSource>();
            var conflicts = new List<AiInstructionConflict>();

            foreach (var group in eligible
                .Where(x => !string.IsNullOrWhiteSpace(x.ConflictKey))
                .GroupBy(x => x.ConflictKey.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var ordered = group
                    .OrderByDescending(x => x, Comparer<AiInstructionSource>.Create(AiInstructionPrecedence.Compare))
                    .ToList();
                var winner = ordered[0];

                foreach (var source in ordered)
                {
                    if (source.Id == winner.Id)
                        included.Add(source);
                }

                if (ordered.Count > 1)
                {
                    var conflict = new AiInstructionConflict
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        ConflictKey = group.Key,
                        WinnerSourceId = winner.Id,
                        Disposition = AiInstructionConflictDisposition.HigherPrecedenceWins,
                        Reason = "The highest-precedence eligible source won deterministically.",
                        DetectedAt = pointInTime
                    };
                    foreach (var source in ordered)
                        conflict.SourceIds.Add(source.Id);
                    conflict.Validate();
                    conflicts.Add(conflict);
                }
            }

            foreach (var source in eligible.Where(x => string.IsNullOrWhiteSpace(x.ConflictKey)))
                included.Add(source);

            included = included
                .OrderBy(x => x, Comparer<AiInstructionSource>.Create((left, right) => AiInstructionPrecedence.Compare(right, left)))
                .ToList();

            var snapshot = new AiInstructionSnapshot(included, conflicts);
            var composedText = ComposeText(snapshot.Sources);
            return new AiInstructionCompositionResult(snapshot, composedText, diagnostics);
        }

        private static string ComposeText(IReadOnlyList<AiInstructionSource> sources)
        {
            if (sources == null || sources.Count == 0)
                return string.Empty;

            var builder = new StringBuilder();
            foreach (var source in sources)
            {
                if (builder.Length > 0)
                    builder.AppendLine().AppendLine();

                var title = string.IsNullOrWhiteSpace(source.Name) ? source.Id : source.Name.Trim();
                if (!string.IsNullOrWhiteSpace(title))
                    builder.Append("[Instruction Source: ").Append(title).AppendLine("]");

                builder.Append(source.Content == null ? string.Empty : source.Content.Trim());
            }
            return builder.ToString();
        }

        private static string SafeId(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<missing>" : value.Trim();
        }

        private static string SafeReason(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "validation failed";
            return value.Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }
}
