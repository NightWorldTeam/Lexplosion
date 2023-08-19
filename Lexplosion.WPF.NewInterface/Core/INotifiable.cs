using System;

namespace Lexplosion.WPF.NewInterface.Core
{
    public enum NotificationType 
    {
        Error = 0,
        Successful = 1,
        Warning = 2,
    }

    public delegate void DoNotificationCallback(string titleKey, string messageKey, uint time, NotificationType type);

    public interface INotifiable
    {
        /// <summary>
        /// <para><c>HeaderKey</c> - key of header in language resource dictionary.<br />
        /// <c>MessageKey</c> - key of message in language resource dictionary.<br />
        /// <c>Time</c> - uint (time in seconds)<br />
        /// <c>Notification</c> Type - byte (type of notification)<br />
        /// </para>
        /// </summary>
        DoNotificationCallback DoNotification { get; }
    }
}
