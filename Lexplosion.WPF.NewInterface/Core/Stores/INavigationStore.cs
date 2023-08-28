using Lexplosion.WPF.NewInterface.Core;

namespace Lexplosion.WPF.NewInterface.Stores
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
