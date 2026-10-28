using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.UI.Abstractions
{
    /// <summary>
    /// Common UI-thread scheduling contract for WPF, Avalonia and other frontends.
    /// This contract does not create a second UI thread.
    /// </summary>
    public interface IUiDispatcher
    {
        bool CheckAccess();

        /// <summary>Execute synchronously on the UI thread.</summary>
        void Invoke(Action action);

        /// <summary>Queue work on the UI thread without blocking the caller.</summary>
        void Post(Action action, UiDispatchPriority priority = UiDispatchPriority.Normal);

        /// <summary>
        /// Queue work on the UI thread and observe completion or failure.
        /// The action itself still runs on the UI thread.
        /// </summary>
        Task InvokeAsync(Action action,
            UiDispatchPriority priority = UiDispatchPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Framework-independent scheduling priorities. Adapters map them to
    /// the closest framework-specific priority.
    /// </summary>
    public enum UiDispatchPriority
    {
        Normal,
        Input,
        Background
    }
}
