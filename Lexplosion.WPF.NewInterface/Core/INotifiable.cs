using System;

namespace Lexplosion.WPF.NewInterface.Core
{
    public enum NotificationType 
    {
        Error = 0,
        Successful = 1,
        Warning = 2,
    }

    public delegate void DoNotificationCallback(string title, string message, uint time, NotificationType type);

    public interface INotifiable
    {
        /// <summary>
        /// <para>Header - first string arg</para>
        /// <para>Message - second string arg</para>
        /// <para>Time - uint arg (time in seconds)</para>
        /// <para>Notification Type - byte arg (type of notification)</para>
        /// </summary>
        DoNotificationCallback DoNotification { get; }
    }
}
