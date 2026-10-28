using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Lexplosion.QA
{
    /// <summary>
    /// Opt-in, local-only timing diagnostics. UI callers only enqueue records;
    /// file writes happen on the ThreadPool, never on the UI thread.
    /// </summary>
    public static class PerformanceTelemetry
    {
        private const int MaxQueuedEvents = 512;
        private const long MaxFileBytes = 2 * 1024 * 1024;
        private static readonly ConcurrentQueue<PerformanceEvent> Pending = new ConcurrentQueue<PerformanceEvent>();
        private static string _filePath;
        private static int _enabled;
        private static int _queuedCount;
        private static int _writerScheduled;

        public static bool Enabled => Volatile.Read(ref _enabled) == 1;

        /// <summary>Call at startup only. Does not open a log file until the first event.</summary>
        public static void Configure(bool enabled, string path)
        {
            if (enabled && string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A local log path is required.", nameof(path));

            _filePath = enabled ? path : null;
            Volatile.Write(ref _enabled, enabled ? 1 : 0);
        }

        public static IDisposable Measure(string operation, double minimumMilliseconds = 0)
        {
            return Enabled ? (IDisposable)new TimingScope(operation, minimumMilliseconds) : EmptyScope.Instance;
        }

        public static void Record(string operation, double milliseconds, double minimumMilliseconds = 0)
        {
            if (!Enabled || double.IsNaN(milliseconds) || double.IsInfinity(milliseconds) ||
                milliseconds < minimumMilliseconds || milliseconds < 0)
                return;

            if (Interlocked.Increment(ref _queuedCount) > MaxQueuedEvents)
            {
                Interlocked.Decrement(ref _queuedCount);
                return; // Diagnostics must never grow memory usage without bound.
            }

            Pending.Enqueue(new PerformanceEvent(DateTime.UtcNow, operation, milliseconds,
                Thread.CurrentThread.ManagedThreadId));
            ScheduleWriter();
        }

        private static void ScheduleWriter()
        {
            if (Interlocked.CompareExchange(ref _writerScheduled, 1, 0) != 0) return;
            ThreadPool.QueueUserWorkItem(_ => Drain());
        }

        private static void Drain()
        {
            try
            {
                var batch = new StringBuilder();
                while (Pending.TryDequeue(out var entry))
                {
                    Interlocked.Decrement(ref _queuedCount);
                    batch.AppendLine(entry.ToLogLine());
                }

                if (batch.Length == 0 || !Enabled) return;

                var path = _filePath;
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                // Keep only one backup. This file contains timings, not personal data.
                if (File.Exists(path) && new FileInfo(path).Length > MaxFileBytes)
                {
                    var backup = path + ".old";
                    if (File.Exists(backup)) File.Delete(backup);
                    File.Move(path, backup);
                }
                File.AppendAllText(path, batch.ToString(), Encoding.UTF8);
            }
            catch (Exception error)
            {
                // A full disk / denied folder must never affect launcher navigation.
                Trace.WriteLine("Lexplosion QA log write failed: " + error.GetType().Name);
            }
            finally
            {
                Interlocked.Exchange(ref _writerScheduled, 0);
                if (!Pending.IsEmpty) ScheduleWriter();
            }
        }

        private sealed class TimingScope : IDisposable
        {
            private readonly string _operation;
            private readonly double _minimumMilliseconds;
            private readonly long _start = Stopwatch.GetTimestamp();
            private int _disposed;

            public TimingScope(string operation, double minimumMilliseconds)
            {
                _operation = operation;
                _minimumMilliseconds = minimumMilliseconds;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
                Record(_operation, (Stopwatch.GetTimestamp() - _start) * 1000.0 / Stopwatch.Frequency,
                    _minimumMilliseconds);
            }
        }

        private sealed class EmptyScope : IDisposable
        {
            public static readonly EmptyScope Instance = new EmptyScope();
            public void Dispose() { }
        }
    }
}
