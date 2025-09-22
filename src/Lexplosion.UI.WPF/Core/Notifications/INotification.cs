using System;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Core.Notifications
{
    public interface INotification
    {
        public Guid Id { get; }
        public NotificationType Type { get; }
        public string Title { get; }
        public string Content { get; }
        public TimeSpan VisibleTime { get; }
        public DateTime Time { get; }
        public string MemberName { get; }

        public ICommand CloseCommand { get; set; }
    }
}
