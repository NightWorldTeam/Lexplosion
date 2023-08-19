namespace Lexplosion.Controls
{
    public delegate void DoNotificationCallback(string title, string message, uint time, byte type);

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
