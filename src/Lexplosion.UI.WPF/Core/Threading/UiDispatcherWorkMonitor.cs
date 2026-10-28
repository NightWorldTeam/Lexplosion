using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;
using Lexplosion.QA;

namespace Lexplosion.UI.WPF.Core.Threading
{
    /// <summary>
    /// QA-only timing of individual Dispatcher operations. No synchronous
    /// disk IO or visual-tree inspection in event handlers.
    /// Wall time is not GPU frame time or an exclusive CPU measurement.
    /// </summary>
    internal sealed class UiDispatcherWorkMonitor
    {
        private readonly Dictionary<DispatcherOperation, long> _started =
            new Dictionary<DispatcherOperation, long>();

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
                _started[e.Operation] = Stopwatch.GetTimestamp();
        }

        private void OnCompleted(object sender, DispatcherHookEventArgs e)
        {
            if (!_started.TryGetValue(e.Operation, out long start))
                return;

            _started.Remove(e.Operation);
            double duration = (Stopwatch.GetTimestamp() - start) *
                1000.0 / Stopwatch.Frequency;
            PerformanceTelemetry.Record("ui.dispatcher.work." +
                e.Operation.Priority, duration, 40);
        }

        private void OnAborted(object sender, DispatcherHookEventArgs e)
        {
            _started.Remove(e.Operation);
        }
    }
}
