using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Core.Services
{
    public class NotificationService : INotificationService
    {
        public event Action<INotification> NotificationAdded;

        public readonly ObservableCollection<INotification> _notifications = new ObservableCollection<INotification>();


        #region Properties


        public IEnumerable<INotification> Notifications { get => _notifications; }


        #endregion Properties


        #region Commands


        public ICommand CloseNotification { get => new RelayCommand((id) => Remove((Guid)id)); }


        #endregion Commands


        #region Constructors


        public NotificationService()
        {
        }


        #endregion Constuctors


        #region Public & Protected Methods


        public void Notify(INotification notification)
        {
            if (_notifications.Count == 2) 
            {
                _notifications[0].CloseCommand.Execute(notification.Id);
            }

            App.Current.Dispatcher.Invoke(() => 
            {
                _notifications.Add(notification);
                notification.CloseCommand = new RelayCommand((obj) => _notifications.Remove(notification));
                NotificationAdded?.Invoke(notification);
            });
        }


        public void Remove(Guid notificationId) 
        {
            _notifications.Remove(_notifications.FirstOrDefault(i => i.Id == notificationId));
        }


        #endregion Public & Protected Methods
    }
}
