using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Simple optional scheduler that bounds the number of executions admitted at once.
    /// It has no global ownership of application timing or queues.
    /// </summary>
    public sealed class AgentExecutionScheduler : IAgentExecutionScheduler, IDisposable
    {
        private readonly HAgentClient _client;
        private readonly SemaphoreSlim _slots;
        private int _disposed;

        public AgentExecutionScheduler(HAgentClient client, int maximumConcurrency = 1)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (maximumConcurrency <= 0) throw new ArgumentOutOfRangeException(nameof(maximumConcurrency));

            _client = client;
            _slots = new SemaphoreSlim(maximumConcurrency, maximumConcurrency);
            MaximumConcurrency = maximumConcurrency;
        }

        public int MaximumConcurrency { get; private set; }

        public async Task<AgentExecution> ScheduleAsync(
            AgentRuntimeInstance instance,
            string message,
            AgentExecutionOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            await _slots.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await _client.ExecuteAsync(instance, message, options, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _slots.Release();
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            _slots.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) != 0)
                throw new ObjectDisposedException(nameof(AgentExecutionScheduler));
        }
    }
}
