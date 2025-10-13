namespace Lexplosion.Core.Tools.Notification
{
    public interface INotificationManager
    {
        /// <summary>
        /// Отправляет уведомление пользователю.
        /// </summary>
        /// <param name="notifiable"></param>
        public void Show(INotificable notifiable);
    }
}
