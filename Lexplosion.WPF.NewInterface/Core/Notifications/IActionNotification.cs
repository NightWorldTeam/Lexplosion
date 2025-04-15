using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Core.Notifications
{
    public interface IActionNotification : INotification
    {
        public IEnumerable<INotificationAction> Actions { get; }
    }
}
