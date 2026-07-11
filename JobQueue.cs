using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrangBooster
{
    public sealed class JobQueue
    {
        public static JobQueue Instance { get; } = new();

        public static JobQueue Apps { get; } = new();

        private readonly SemaphoreSlim _gate = new(1, 1);
        private int _pending;

        public event Action<int, string>? Changed;

        public int Pending => Volatile.Read(ref _pending);

        public Task Enqueue(string name, Func<Task> work)
            => Enqueue(name, async () => { await work(); return 0; });

        public async Task<T> Enqueue<T>(string name, Func<Task<T>> work)
        {
            int pending = Interlocked.Increment(ref _pending);
            Changed?.Invoke(pending, pending > 1 ? $"Queued: {name}" : name);
            Logger.Info($"JobQueue enqueue '{name}' (pending={pending})");
            await _gate.WaitAsync();
            try
            {
                Changed?.Invoke(Volatile.Read(ref _pending), name);
                Logger.Info($"JobQueue run '{name}'");
                return await work();
            }
            finally
            {
                int left = Interlocked.Decrement(ref _pending);
                _gate.Release();
                Changed?.Invoke(left, left > 0 ? "Running queued work…" : "");
                Logger.Info($"JobQueue done '{name}' (pending={left})");
            }
        }
    }
}
