using Lexplosion.UI.WPF.Core;

namespace Lexplosion.UI.WPF.Stores
{
    public delegate void CurrentViewModelChangedEventHandler();

    public interface INavigationStore
    {
        /// <summary>
        /// ViewModel выбранной страницы
        /// </summary>
        ViewModelBase CurrentViewModel { get; set; }
        /// <summary>
        /// Событие срабатывающие при изменении CurrentViewModel
        /// </summary>
        event CurrentViewModelChangedEventHandler CurrentViewModelChanged;
    }
}
