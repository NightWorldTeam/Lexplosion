using Lexplosion.UI.WPF.Core;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels
{
    public enum NotificationSource
    {
        All,
        NightWorld
    }

    public class NotificationsModel
    {
        public NotificationSource CurrentNotificationSource { get; set; } = NotificationSource.All;
    }

    public sealed class NotificationsViewModel : ViewModelBase
    {
        public NotificationsModel Model { get; }

        public NotificationsViewModel()
        {
            Model = new();
        }
    }
}
