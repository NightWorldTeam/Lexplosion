using System.Windows.Input;

namespace Lexplosion.UI.WPF.Core.Notifications
{
    public interface INotificationAction
    {
        /// <summary>
        /// Ключ/Название
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Выполняемое действие
        /// </summary>
        public ICommand ActionCommand { get; }
    }
}
