using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Lexplosion.UI.Core.Threading;

namespace Lexplosion.UI.WPF.Core.Threading
{
    /// <summary>
    /// WPF adapter for the portable UI scheduling contract.
    /// A future Avalonia frontend supplies its own implementation.
    /// </summary>
    public sealed class WpfUiDispatcher : IUiDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public WpfUiDispatcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public bool CheckAccess() => _dispatcher.CheckAccess();

        public void Invoke(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            if (CheckAccess())
                action();
            else
                _dispatcher.Invoke(action);
        }

        public void Post(Action action, UiDispatchPriority priority = UiDispatchPriority.Normal)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _dispatcher.BeginInvoke(action, MapPriority(priority));
        }

        public Task InvokeAsync(Action action,
            UiDispatchPriority priority = UiDispatchPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return _dispatcher.InvokeAsync(action, MapPriority(priority), cancellationToken).Task;
        }

        private static DispatcherPriority MapPriority(UiDispatchPriority priority)
        {
            switch (priority)
            {
                case UiDispatchPriority.Input:
                    return DispatcherPriority.Input;
                case UiDispatchPriority.Background:
                    return DispatcherPriority.Background;
                default:
                    return DispatcherPriority.Normal;
            }
        }
    }
}
