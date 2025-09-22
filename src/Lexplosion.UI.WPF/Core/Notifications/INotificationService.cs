using System;
using System.Collections.Generic;

namespace Lexplosion.UI.WPF.Core.Notifications
{
    public interface INotificationService
    {
        event Action<INotification> NotificationAdded;

        IEnumerable<INotification> Notifications { get; }
        void Notify(INotification notification);

        void Success(string title, string message);
    }
}
