#if DEBUG
using Lexplosion.QA;
using Lexplosion.UI.WPF.Core.Threading;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.Core
{
    /// <summary>
    /// Explicit developer-named synchronous sections. Times are wall-clock,
    /// not exclusive CPU samples. Names must not contain any user data.
    /// </summary>
    internal static class UiDebugTrace
    {
        [ThreadStatic] private static Scope _current;
        private static string _route = "unknown";
        private static long _lastNavigationTick;

        internal static string Route => _route;
        internal static bool RecentNavigation => _lastNavigationTick > 0 &&
            (Stopwatch.GetTimestamp() - _lastNavigationTick) * 1000.0 /
                Stopwatch.Frequency < 3000;

        internal static void Navigate(object content)
        {
            if (!PerformanceTelemetry.Enabled) return;
            _route = content?.GetType().Name ?? "none";
            _lastNavigationTick = Stopwatch.GetTimestamp();
            PerformanceTelemetry.Record("ui.route." + _route, 0);
        }

        internal static IDisposable Measure(string section, double minimumMilliseconds = 3)
        {
            return PerformanceTelemetry.Enabled
                ? (IDisposable)new Scope(section, minimumMilliseconds)
                : Empty.Instance;
        }

        private sealed class Scope : IDisposable
        {
            private readonly long _started = Stopwatch.GetTimestamp();
            private readonly string _section;
            private readonly double _minimum;
            private readonly Scope _parent;
            private bool _hasChildren;
            private bool _disposed;

            internal Scope(string section, double minimum)
            {
                _section = section;
                _minimum = minimum;
                _parent = _current;
                if (_parent != null) _parent._hasChildren = true;
                _current = this;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                if (ReferenceEquals(_current, this)) _current = _parent;
                double duration = (Stopwatch.GetTimestamp() - _started) * 1000.0 /
                    Stopwatch.Frequency;
                PerformanceTelemetry.Record("ui.work." + _section, duration, _minimum);
                // Only leaf sections can help attribute a Dispatcher operation:
                // enclosing sections include their children's time.
                if (!_hasChildren) UiDispatcherWorkMonitor.Attribute(_section, duration);
            }
        }

        private sealed class Empty : IDisposable
        {
            internal static readonly Empty Instance = new Empty();
            public void Dispose() { }
        }
    }

    /// <summary>
    /// Interval between WPF Rendering notifications during navigation.
    /// NOT time spent rendering and not a GPU/FPS profiler.
    /// </summary>
    internal sealed class UiFrameProbe
    {
        private long _lastFrameAt;

        private UiFrameProbe()
        {
            CompositionTarget.Rendering += OnRendering;
            PerformanceTelemetry.Record("qa.frame.monitor.start", 0);
        }

        internal static UiFrameProbe Start()
        {
            return PerformanceTelemetry.Enabled ? new UiFrameProbe() : null;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            long now = Stopwatch.GetTimestamp();
            if (_lastFrameAt != 0 && UiDebugTrace.RecentNavigation)
            {
                var window = Application.Current?.MainWindow;
                if (window != null && window.IsVisible &&
                    window.WindowState != WindowState.Minimized)
                {
                    double interval = (now - _lastFrameAt) * 1000.0 / Stopwatch.Frequency;
                    PerformanceTelemetry.Record("ui.frame.interval." + UiDebugTrace.Route,
                        interval, 75);
                }
            }
            _lastFrameAt = now;
        }
    }
}
#endif
