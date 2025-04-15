using Lexplosion.WPF.NewInterface.Core.Notifications;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;


namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class InstanceNotification : INotification
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Title { get; }
        public NotificationType Type { get; }
        public string Content { get; }
        public TimeSpan VisibleTime { get; }
        public DateTime Time { get; }

        public string InstanceName { get; } = "Long Tech";

        public string MemberName { get; }
        public ICommand CloseCommand { get; set; }

        public InstanceNotification(string title, string message, NotificationType notificationType, TimeSpan visibleTime, [CallerMemberName] string memberName = "")
        {
            Title = title;
            Content = message;
            Type = notificationType;
            VisibleTime = visibleTime;
            Time = DateTime.Now;
            MemberName = memberName;
        }
    }
}
