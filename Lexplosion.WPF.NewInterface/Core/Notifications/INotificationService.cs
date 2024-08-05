using System;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Core.Notifications
{
    public interface INotificationService
    {
        event Action<INotification> NotificationAdded;

        IEnumerable<INotification> Notifications { get; }
        void Notify(INotification notification);
    }
}
