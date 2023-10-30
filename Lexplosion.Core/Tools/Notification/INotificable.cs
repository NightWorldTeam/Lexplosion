using System;

namespace Lexplosion.Core.Tools.Notification
{
    public interface INotificable
    {
        public NotificationType Type { get; }
        public string Title { get; }
        public string Content { get; }
        public TimeSpan VisibleTime { get; }
        public DateTime Time { get; }
    }
}
