namespace KuduDiagnosticsDemo.Simulators
{
    static class ProblemSimulator
    {
        // Store for managed leak blocks.
        private static readonly List<byte[]> LeakStore = new();
        private static readonly object LeakLock = new();

        private static Task? _leakTask;
        private static long _approxTotalMb;

        // Rough caps chosen to stay within a sensible range for an S1 worker.
        private const int LeakChunkMb = 10;      // 10 MB per allocation
        private const int LeakDelayMs = 500;     // every 0.5 seconds
        private const int LeakMaxMb = 1200;      // stop leaking after ~1.2 GB total

        public static long StartTimedLeak(TimeSpan duration)
        {
            lock (LeakLock)
            {
                // If a leak is already running, just report the current total.
                if (_leakTask is { IsCompleted: false })
                {
                    return Interlocked.Read(ref _approxTotalMb);
                }

                _leakTask = Task.Run(async () =>
                {
                    var end = DateTime.UtcNow + duration;

                    while (DateTime.UtcNow < end)
                    {
                        // Stop early if we have already leaked a lot.
                        if (Interlocked.Read(ref _approxTotalMb) >= LeakMaxMb)
                        {
                            break;
                        }

                        // Allocate a 10 MB block and touch every page so it is committed.
                        var buffer = new byte[LeakChunkMb * 1024 * 1024];
                        for (var i = 0; i < buffer.Length; i += 4096)
                        {
                            buffer[i] = 1;
                        }

                        lock (LeakStore)
                        {
                            LeakStore.Add(buffer);
                        }

                        Interlocked.Add(ref _approxTotalMb, LeakChunkMb);

                        await Task.Delay(LeakDelayMs);
                    }
                });

                return Interlocked.Read(ref _approxTotalMb);
            }
        }

        public static long AllocateMemoryBomb()
        {
            // About 512 MB total, but do NOT hold on to it.
            const int totalMb = 512;
            var remaining = totalMb;

            // Local list only, goes out of scope when the request finishes.
            var local = new List<byte[]>();

            while (remaining > 0)
            {
                var chunkMb = Math.Min(remaining, 64); // 64 MB chunks
                var buffer = new byte[chunkMb * 1024 * 1024];

                // Touch each page so the memory is committed.
                for (var i = 0; i < buffer.Length; i += 4096)
                {
                    buffer[i] = 1;
                }

                local.Add(buffer);
                remaining -= chunkMb;
            }

            // Report how much we just allocated, but do not store it anywhere.
            return totalMb;
        }

        public static void StartThreadPoolStarvation()
        {
            // Block 200 threads to make the app sluggish
            const int tasks = 200;
            const int sleepMilliseconds = 60000;

            for (var i = 0; i < tasks; i++)
            {
                Task.Run(() =>
                {
                    // This blocks a thread pool thread for the full duration.
                    Thread.Sleep(sleepMilliseconds);
                });
            }
        }
    }
}