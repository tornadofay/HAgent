using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Process-local admission state. It is intentionally independent from provider transport.
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
                        .Where(x => x.Dimension == limit.Dimension && string.Equals(GetScope(x), limit.Scope, StringComparison.OrdinalIgnoreCase))
                        .Sum(x => x.Amount);

                    if (requested + used <= limit.Maximum)
                        continue;

                    var candidate = state.Entries
                        .Where(x => x.Dimension == limit.Dimension && string.Equals(GetScope(x), limit.Scope, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(x => x.At)
                        .FirstOrDefault();
                    if (candidate != null)
                    {
                        var candidateWait = candidate.At + limit.Window;
                        if (!waitUntil.HasValue || candidateWait > waitUntil.Value)
                            waitUntil = candidateWait;
                    }
                    else
                    {
                        waitUntil = timestamp + limit.Window;
                    }
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
                    target.Id,
                    timestamp,
                    new Dictionary<AiQuotaDimension, long>(requestedUsage),
                    (r, actual) => Commit(state, policy, r, actual),
                    r => Release(state, r));

                return new AiQuotaAdmissionResult
                {
                    Decision = AiAdmissionDecision.Admitted,
                    Reason = "Requested usage admitted under all configured limits.",
                    Reservation = reservation
                };
            }
        }

        private static void Cleanup(TargetState state, AiQuotaPolicy policy, DateTimeOffset now)
        {
            if (policy.Limits.Count == 0) return;
            state.Entries.RemoveAll(entry =>
            {
                var limit = policy.Limits.FirstOrDefault(x => x.Dimension == entry.Dimension);
                return limit != null && entry.At + limit.Window <= now;
            });
        }

        private static void Commit(
            TargetState state,
            AiQuotaPolicy policy,
            AiQuotaReservation reservation,
            IReadOnlyDictionary<AiQuotaDimension, long> actualUsage)
        {
            lock (state.Sync)
            {
                foreach (var entry in state.Entries.Where(x => x.ReservationId == GetReservationId(reservation)).ToList())
                    state.Entries.Remove(entry);

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
                state.Entries.RemoveAll(x => x.ReservationId == GetReservationId(reservation));
            }
        }

        private static Guid GetReservationId(AiQuotaReservation reservation)
        {
            var field = typeof(AiQuotaReservation).GetField("_commit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return reservation.GetHashCode() == 0 ? Guid.Empty : Guid.Empty;
        }

        private static string GetScope(UsageEntry entry)
        {
            return string.Empty;
        }
    }
}
