using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Process-local target-scoped admission state. It is deliberately independent from provider transport.
    /// </summary>
    public sealed class InMemoryAiQuotaAdmission
    {
        private sealed class UsageEntry
        {
            public DateTimeOffset At;
            public AiQuotaDimension Dimension;
            public long Amount;
            public Guid ReservationId;
        }

        private sealed class TargetState
        {
            public readonly object Sync = new object();
            public readonly List<UsageEntry> Entries = new List<UsageEntry>();
        }

        private readonly ConcurrentDictionary<string, TargetState> _states = new ConcurrentDictionary<string, TargetState>(StringComparer.OrdinalIgnoreCase);

        public AiQuotaAdmissionResult TryReserve(
            AiExecutionTarget target,
            AiQuotaPolicy policy,
            IReadOnlyDictionary<AiQuotaDimension, long> requestedUsage,
            DateTimeOffset? now = null)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (requestedUsage == null) throw new ArgumentNullException(nameof(requestedUsage));
            target.Validate();
            policy.Validate();

            foreach (var pair in requestedUsage)
            {
                if (pair.Value < 0L)
                    throw new ArgumentOutOfRangeException(nameof(requestedUsage), "Requested usage cannot be negative.");
            }

            var timestamp = now ?? DateTimeOffset.UtcNow;
            var state = _states.GetOrAdd(target.Id, key => new TargetState());
            var reservationId = Guid.NewGuid();

            lock (state.Sync)
            {
                Cleanup(state, policy, timestamp);

                DateTimeOffset? waitUntil = null;
                foreach (var limit in policy.Limits)
                {
                    long requested;
                    if (!requestedUsage.TryGetValue(limit.Dimension, out requested) || requested <= 0L)
                        continue;

                    var used = state.Entries
                        .Where(x => x.Dimension == limit.Dimension)
                        .Sum(x => x.Amount);

                    if (requested + used <= limit.Maximum)
                        continue;

                    var firstReleasable = state.Entries
                        .Where(x => x.Dimension == limit.Dimension)
                        .OrderBy(x => x.At)
                        .FirstOrDefault();

                    var candidateWait = firstReleasable != null
                        ? firstReleasable.At + limit.Window
                        : timestamp + limit.Window;
                    if (!waitUntil.HasValue || candidateWait > waitUntil.Value)
                        waitUntil = candidateWait;
                }

                if (waitUntil.HasValue)
                {
                    return new AiQuotaAdmissionResult
                    {
                        Decision = AiAdmissionDecision.WaitRequired,
                        Reason = "At least one quota/rate limit is currently exhausted.",
                        WaitUntil = waitUntil
                    };
                }

                foreach (var pair in requestedUsage)
                {
                    if (pair.Value <= 0L) continue;
                    state.Entries.Add(new UsageEntry
                    {
                        At = timestamp,
                        Dimension = pair.Key,
                        Amount = pair.Value,
                        ReservationId = reservationId
                    });
                }

                var reservation = new AiQuotaReservation(
                    reservationId,
                    target.Id,
                    timestamp,
                    new Dictionary<AiQuotaDimension, long>(requestedUsage),
                    (r, actual) => Commit(state, policy, r, actual),
                    r => Release(state, r));

                return new AiQuotaAdmissionResult
                {
                    Decision = AiAdmissionDecision.Admitted,
                    Reason = "Requested usage admitted under all configured target-scoped limits.",
                    Reservation = reservation
                };
            }
        }

        private static void Cleanup(TargetState state, AiQuotaPolicy policy, DateTimeOffset now)
        {
            if (policy.Limits.Count == 0)
                return;

            state.Entries.RemoveAll(entry =>
            {
                var limits = policy.Limits.Where(x => x.Dimension == entry.Dimension).ToList();
                return limits.Count > 0 && limits.All(x => entry.At + x.Window <= now);
            });
        }

        private static void Commit(
            TargetState state,
            AiQuotaPolicy policy,
            AiQuotaReservation reservation,
            IReadOnlyDictionary<AiQuotaDimension, long> actualUsage)
        {
            if (actualUsage == null)
                throw new ArgumentNullException(nameof(actualUsage));

            foreach (var pair in actualUsage)
            {
                if (pair.Value < 0L)
                    throw new ArgumentOutOfRangeException(nameof(actualUsage), "Actual usage cannot be negative.");
            }

            lock (state.Sync)
            {
                state.Entries.RemoveAll(x => x.ReservationId == reservation.ReservationId);

                var now = DateTimeOffset.UtcNow;
                foreach (var pair in actualUsage)
                {
                    if (pair.Value <= 0L) continue;
                    state.Entries.Add(new UsageEntry
                    {
                        At = now,
                        Dimension = pair.Key,
                        Amount = pair.Value,
                        ReservationId = Guid.Empty
                    });
                }

                Cleanup(state, policy, now);
            }
        }

        private static void Release(TargetState state, AiQuotaReservation reservation)
        {
            lock (state.Sync)
            {
                state.Entries.RemoveAll(x => x.ReservationId == reservation.ReservationId);
            }
        }
    }
}
