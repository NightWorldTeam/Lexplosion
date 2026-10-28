using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Lexplosion.QA;

namespace Lexplosion.UI.WPF.Core.Threading
{
    /// <summary>
    /// Detects Dispatcher starvation, not GPU frame time. Only active with
    /// LEXPLOSION_QA=1. The timer does not perform IO on the UI thread.
    /// </summary>
    public sealed class UiStallMonitor
    {
        private readonly DispatcherTimer _timer;
        private long _lastTick;

        private UiStallMonitor(Dispatcher dispatcher)
        {
            _lastTick = Stopwatch.GetTimestamp();
            _timer = new DispatcherTimer(DispatcherPriority.Background, dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += OnTick;
            _timer.Start();
        }

        public static UiStallMonitor Start(Dispatcher dispatcher)
        {
            if (!PerformanceTelemetry.Enabled) return null;
            return new UiStallMonitor(dispatcher);
        }

        private void OnTick(object sender, EventArgs e)
        {
            long now = Stopwatch.GetTimestamp();
            double elapsed = (now - _lastTick) * 1000.0 / Stopwatch.Frequency;
            _lastTick = now;

            var window = Application.Current?.MainWindow;
            if (window == null || !window.IsVisible || window.WindowState == WindowState.Minimized)
                return;

            // Expected timer interval is 100 ms; log stalls exceeding ~150 ms.
            PerformanceTelemetry.Record("ui.dispatcher.stall", elapsed - 100, 150);
        }
    }
}
