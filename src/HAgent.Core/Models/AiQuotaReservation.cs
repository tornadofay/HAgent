using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiAdmissionDecision
    {
        Admitted,
        Rejected,
        WaitRequired
    }

    public sealed class AiQuotaAdmissionResult
    {
        public AiQuotaAdmissionResult()
        {
            Decision = AiAdmissionDecision.Rejected;
            Reason = string.Empty;
            WaitUntil = null;
            Reservation = null;
        }

        public AiAdmissionDecision Decision { get; internal set; }
        public string Reason { get; internal set; }
        public DateTimeOffset? WaitUntil { get; internal set; }
        public AiQuotaReservation Reservation { get; internal set; }
    }

    public sealed class AiQuotaReservation : IDisposable
    {
        private readonly Action<AiQuotaReservation, IReadOnlyDictionary<AiQuotaDimension, long>> _commit;
        private readonly Action<AiQuotaReservation> _release;
        private bool _completed;

        internal AiQuotaReservation(
            string targetId,
            DateTimeOffset createdAt,
            IReadOnlyDictionary<AiQuotaDimension, long> reserved,
            Action<AiQuotaReservation, IReadOnlyDictionary<AiQuotaDimension, long>> commit,
            Action<AiQuotaReservation> release)
        {
            TargetId = targetId;
            CreatedAt = createdAt;
            Reserved = reserved;
            _commit = commit;
            _release = release;
        }

        public string TargetId { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public IReadOnlyDictionary<AiQuotaDimension, long> Reserved { get; private set; }

        public void Commit(IReadOnlyDictionary<AiQuotaDimension, long> actualUsage)
        {
            if (_completed) throw new InvalidOperationException("The quota reservation is already completed.");
            if (actualUsage == null) throw new ArgumentNullException(nameof(actualUsage));
            _completed = true;
            _commit(this, actualUsage);
        }

        public void Release()
        {
            if (_completed) return;
            _completed = true;
            _release(this);
        }

        public void Dispose()
        {
            Release();
        }
    }
}
