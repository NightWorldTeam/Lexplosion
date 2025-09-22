using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Core.Modal
{
    public interface IModalViewModel
    {
        /// <summary>
        /// Данное модальное окно закрыто, вызывается когда ModalNavigationStore уже окончательно удалило CurrentViewModel
        /// </summary>
        public event Action Closed;
        
        public event Action<object> CloseCommandExecutedEvent;
        /// <summary>
        /// Команда на закрытие модального окна.
        /// </summary>
        ICommand CloseCommand { get; }
        /// <summary>
        /// Вызывается ModalNavigationStore, вызывается метод Closed, уведомляя подписчиков о том, что ModalNavigationStore готов принимать новые окна.
        /// </summary>
        public void ExecuteClosedEvent();
    }

    public interface IActionModelViewModel : IModalViewModel
    {
        public event Action<object> ActionCommandExecutedEvent;
        /// <summary>
        /// Действие которые должно выполниться, например экспорт, создание сборки и n
        /// </summary>
        ICommand ActionCommand { get; }
    }
}
