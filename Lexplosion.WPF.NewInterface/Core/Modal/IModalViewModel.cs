using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Core.Modal
{
    public interface IModalViewModel
    {
        public event Action<object> CloseCommandExecutedEvent;
        /// <summary>
        /// Команда на закрытие модального окна.
        /// </summary>
        ICommand CloseCommand { get; }
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
