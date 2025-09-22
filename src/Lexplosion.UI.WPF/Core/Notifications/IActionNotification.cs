using System.Collections.Generic;

namespace Lexplosion.UI.WPF.Core.Notifications
{
    public interface IActionNotification : INotification
    {
        public IEnumerable<INotificationAction> Actions { get; }
    }
}
