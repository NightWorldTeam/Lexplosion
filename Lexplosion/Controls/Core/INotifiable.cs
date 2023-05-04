using System;

namespace Lexplosion.Controls
{
    public interface INotifiable
    {
        /// <summary>
        /// <para>Header - first string arg</para>
        /// <para>Message - second string arg</para>
        /// <para>Time - uint arg (time in seconds)</para>
        /// <para>Notification Type - byte arg (type of notification)</para>
        /// </summary>
        Action<string, string, uint, byte> DoNotification { get; }
    }
}
