using Lexplosion.Core.Tools.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class InstanceNotification : INotificable
    {
        public string Title { get; }
        public Lexplosion.Core.Tools.Notification.NotificationType Type { get; }
        public string Content { get; }
        public TimeSpan VisibleTime { get; }
        public DateTime Time { get; }

        public string InstanceName { get; } = "Long Tech";

        public InstanceNotification(string title, string message, Lexplosion.Core.Tools.Notification.NotificationType notificationType, TimeSpan visibleTime)
        {
            Title = title;
            Content = message;
            Type = notificationType;
            VisibleTime = visibleTime;
            Time = DateTime.Now;
        }
    }
}
