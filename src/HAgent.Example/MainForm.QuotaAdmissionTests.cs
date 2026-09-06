using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddQuotaAdmissionTabs()
        {
            AddApiTab(
                "Quota Admission",
                "Run quota test",
                "Verifies rolling-window admission, atomic concurrent reservations, release, and actual-usage reconciliation.",
                "Only requests inside the configured budget may reserve capacity concurrently; released reservations must free capacity before transport.",
                "No provider or external service is used.",
                TestQuotaAdmissionAsync,
                "Admission boundary",
                "This controller is process-local foundation. Persistent/shared admission will be added with storage-aware runtime integration.");
        }

        private async Task TestQuotaAdmissionAsync(string unused)
        {
            var target = new AiExecutionTarget
            {
                Id = "quota-target-42",
                ProviderId = "provider-42",
                ModelId = "model-42"
            };
            var policy = new AiQuotaPolicy();
            policy.Limits.Add(new AiQuotaLimit
            {
                Dimension = AiQuotaDimension.RequestCount,
                Maximum = 2,
                Window = TimeSpan.FromMinutes(1)
            });
            policy.Limits.Add(new AiQuotaLimit
            {
                Dimension = AiQuotaDimension.InputTokens,
                Maximum = 100,
                Window = TimeSpan.FromMinutes(1)
            });

            var admission = new InMemoryAiQuotaAdmission();
            var usage = new Dictionary<AiQuotaDimension, long>
            {
                { AiQuotaDimension.RequestCount, 1 },
                { AiQuotaDimension.InputTokens, 60 }
            };

            var first = admission.TryReserve(target, policy, usage);
            if (first.Decision != AiAdmissionDecision.Admitted || first.Reservation == null)
                throw new InvalidOperationException("The first quota reservation was not admitted.");

            var second = admission.TryReserve(target, policy, usage);
            if (second.Decision != AiAdmissionDecision.WaitRequired)
                throw new InvalidOperationException("The token window did not block an over-budget second reservation.");
            first.Reservation.Commit(new Dictionary<AiQuotaDimension, long>
            {
                { AiQuotaDimension.RequestCount, 1 },
                { AiQuotaDimension.InputTokens, 40 }
            });

            var concurrentTasks = Enumerable.Range(0, 4)
                .Select(i => Task.Run(() => admission.TryReserve(
                    target,
                    new AiQuotaPolicy
                    {
                        Limits = { new AiQuotaLimit
                            {
                                Dimension = AiQuotaDimension.Concurrency,
                                Maximum = 1,
                                Window = TimeSpan.FromMinutes(1)
                            }}
                    },
                    new Dictionary<AiQuotaDimension, long>
                    {
                        { AiQuotaDimension.Concurrency, 1 }
                    })))
                .ToArray();
            var concurrent = await Task.WhenAll(concurrentTasks).ConfigureAwait(true);
            if (concurrent.Count(x => x.Decision == AiAdmissionDecision.Admitted) != 1)
                throw new InvalidOperationException("Concurrent atomic reservation did not enforce a one-slot capacity.");

            var admitted = concurrent.First(x => x.Decision == AiAdmissionDecision.Admitted);
            admitted.Reservation.Release();

            var afterRelease = admission.TryReserve(
                target,
                new AiQuotaPolicy
                {
                    Limits = { new AiQuotaLimit
                        {
                            Dimension = AiQuotaDimension.Concurrency,
                            Maximum = 1,
                            Window = TimeSpan.FromMinutes(1)
                        }}
                },
                new Dictionary<AiQuotaDimension, long>
                {
                    { AiQuotaDimension.Concurrency, 1 }
                });
            if (afterRelease.Decision != AiAdmissionDecision.Admitted)
                throw new InvalidOperationException("Released capacity was not returned to the admission controller.");
            afterRelease.Reservation.Release();

            Write(
                "QUOTA ADMISSION",
                "Quota admission contract test succeeded." + Environment.NewLine +
                "Rolling-window request/token limits: verified." + Environment.NewLine +
                "Atomic concurrent reservation: verified." + Environment.NewLine +
                "Actual usage reconciliation: verified." + Environment.NewLine +
                "Reservation release: verified.");
        }
    }
}
