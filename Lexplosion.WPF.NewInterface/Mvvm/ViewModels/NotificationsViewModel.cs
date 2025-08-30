using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels
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
