using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Lexplosion.QA;

namespace Lexplosion.UI.WPF.Core.Threading
{
    /// <summary>
    /// Independent wall-clock probe of how quickly Dispatcher can process
    /// input-priority work. Unlike a Background DispatcherTimer, it measures
    /// the enqueue-to-execution delay and does not confuse elapsed timer
    /// interval with the duration of a single UI operation.
    ///
    /// QA-only, at most one outstanding operation (no unbounded UI backlog).
    /// This still measures UI scheduling latency, not GPU frame time or
    /// the CPU stack responsible for the delay.
    /// </summary>
    internal sealed class UiInputLatencyMonitor
    {
        private readonly Dispatcher _dispatcher;
        private readonly Timer _timer;
        private int _pending;

        private UiInputLatencyMonitor(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            PerformanceTelemetry.Record("qa.input.monitor.start", 0);
            _timer = new Timer(Poll, null, 100, 100);
        }

        public static UiInputLatencyMonitor Start(Dispatcher dispatcher)
        {
            return PerformanceTelemetry.Enabled ? new UiInputLatencyMonitor(dispatcher) : null;
        }

        private void Poll(object state)
        {
            if (_dispatcher.HasShutdownStarted || _dispatcher.HasShutdownFinished)
                return;

            if (Interlocked.CompareExchange(ref _pending, 1, 0) != 0)
                return;

            long queuedAt = Stopwatch.GetTimestamp();
            try
            {
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        var window = Application.Current?.MainWindow;
                        if (window == null || !window.IsVisible ||
                            window.WindowState == WindowState.Minimized)
                            return;

                        var milliseconds = (Stopwatch.GetTimestamp() - queuedAt) *
                            1000.0 / Stopwatch.Frequency;
                        PerformanceTelemetry.Record("ui.input.lag", milliseconds, 150);
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _pending, 0);
                    }
                }), DispatcherPriority.Input);
            }
            catch (InvalidOperationException)
            {
                Interlocked.Exchange(ref _pending, 0);
            }
        }
    }
}
