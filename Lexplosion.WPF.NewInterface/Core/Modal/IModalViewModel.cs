using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Core.Modal
{
    public interface IModalViewModel
    {
        /// <summary>
        /// Команда на закрытие модального окна.
        /// </summary>
        ICommand CloseCommand { get; }
        /// <summary>
        /// Действие которые должно выполниться, например экспорт, создание сборки и n
        /// </summary>
        ICommand ActionCommand { get; }
    }
}
