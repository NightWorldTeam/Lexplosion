using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Core.Notifications
{
    public class SimpleNotification : INotification
    {
        #region Properties


        public Guid Id { get; }
        public NotificationType Type { get; }
        public string Title { get; }
        public string Content { get; }
        public TimeSpan VisibleTime { get; }
        public DateTime Time { get; }
        public string MemberName { get; }


        public ICommand CloseCommand { get; set; }


        #endregion Properties


        #region Constructors


        public SimpleNotification(string title, string content, TimeSpan? visibleTime = null, DateTime? time = null, NotificationType type = NotificationType.Info, [CallerMemberName] string memberName = "")
        {
            Id = Guid.NewGuid();
            Type = type;
            Title = title;
            Content = content;
            VisibleTime = visibleTime ?? TimeSpan.FromSeconds(5);
            Time = time ?? DateTime.Now;
            MemberName = memberName;
        }


        #endregion Constructors
    }
}
