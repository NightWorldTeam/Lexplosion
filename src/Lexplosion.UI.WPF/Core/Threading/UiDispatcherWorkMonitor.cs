#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;
using Lexplosion.QA;
using Lexplosion.UI.WPF.Core;

namespace Lexplosion.UI.WPF.Core.Threading
{
    /// <summary>
    /// QA-only timing of individual Dispatcher operations. No synchronous
    /// disk IO or visual-tree inspection in event handlers.
    /// Wall time is not GPU frame time or an exclusive CPU measurement.
    /// </summary>
    internal sealed class UiDispatcherWorkMonitor
    {
        private sealed class TimedWork
        {
            internal long Started;
            internal string SlowestLeaf;
            internal double LeafMilliseconds;
        }

        [ThreadStatic] private static UiDispatcherWorkMonitor _active;
        private readonly Stack<DispatcherOperation> _operations = new Stack<DispatcherOperation>();
        private readonly Dictionary<DispatcherOperation, TimedWork> _started =
            new Dictionary<DispatcherOperation, TimedWork>();

        // Only called from a completed, developer-named synchronous Debug scope.
        // It can explain a portion of an operation, but is not a CPU stack sample.
        internal static void Attribute(string name, double milliseconds)
        {
            var monitor = _active;
            if (monitor == null || monitor._operations.Count == 0) return;
            if (!monitor._started.TryGetValue(monitor._operations.Peek(), out var sample)) return;
            if (milliseconds <= sample.LeafMilliseconds) return;
            sample.SlowestLeaf = name;
            sample.LeafMilliseconds = milliseconds;
        }

        private UiDispatcherWorkMonitor(Dispatcher dispatcher)
        {
            dispatcher.Hooks.OperationStarted += OnStarted;
            dispatcher.Hooks.OperationCompleted += OnCompleted;
            dispatcher.Hooks.OperationAborted += OnAborted;
            PerformanceTelemetry.Record("qa.dispatcher.work.start", 0);
        }

        public static UiDispatcherWorkMonitor Start(Dispatcher dispatcher)
        {
            return PerformanceTelemetry.Enabled ? new UiDispatcherWorkMonitor(dispatcher) : null;
        }

        private void OnStarted(object sender, DispatcherHookEventArgs e)
        {
            if (_started.Count < 128)
            {
                _started[e.Operation] = new TimedWork { Started = Stopwatch.GetTimestamp() };
                _active = this;
                _operations.Push(e.Operation);
            }
        }

        private void OnCompleted(object sender, DispatcherHookEventArgs e)
        {
            if (!_started.TryGetValue(e.Operation, out var sample)) return;
            _started.Remove(e.Operation);
            RemoveActive(e.Operation);

            double duration = (Stopwatch.GetTimestamp() - sample.Started) *
                1000.0 / Stopwatch.Frequency;
            if (duration < 40) return;

            string priority = e.Operation.Priority.ToString();
            PerformanceTelemetry.Record("ui.dispatcher.work." + priority, duration);
            if (sample.SlowestLeaf != null)
                PerformanceTelemetry.Record("ui.dispatcher.hotspot." + priority + "." +
                    sample.SlowestLeaf, sample.LeafMilliseconds);
            else
                PerformanceTelemetry.Record("ui.dispatcher.unattributed." + priority, duration);
        }

        private void OnAborted(object sender, DispatcherHookEventArgs e)
        {
            _started.Remove(e.Operation);
            RemoveActive(e.Operation);
        }

        private void RemoveActive(DispatcherOperation operation)
        {
            if (_operations.Count == 0) return;
            if (ReferenceEquals(_operations.Peek(), operation))
                _operations.Pop();
            else
            {
                // Nested Dispatcher frames can finish in a non-LIFO order.
                var remaining = new Stack<DispatcherOperation>();
                while (_operations.Count != 0)
                {
                    var current = _operations.Pop();
                    if (!ReferenceEquals(current, operation)) remaining.Push(current);
                }
                while (remaining.Count != 0) _operations.Push(remaining.Pop());
            }
            if (_operations.Count == 0) _active = null;
        }
    }
}

#endif
