using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace Lexplosion.Common.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для NotificationWindow.xaml
    /// </summary>
    /// 
    public class Notification
    {
        private readonly Action<Notification> _removeNotification;
        public static int removeTimeMS = 20000;

        public string Header { get; set; }
        public string Message { get; set; }

        public Notification(string header, string message, Action<Notification> removeNotification)
        {
            Header = header;
            Message = message;

            _removeNotification = removeNotification;

            Runtime.TaskRun(() =>
            {
                Thread.Sleep(removeTimeMS);
                Application.Current.Dispatcher.Invoke(() => removeNotification(this));
            });
        }
    }

    public partial class NotificationWindow : Window
    {
        public ObservableCollection<Notification> Notifications { get; } = new ObservableCollection<Notification>();

        public ObservableCollection<Notification> VisibleNotification { get; } = new ObservableCollection<Notification>();

        public NotificationWindow()
        {
            InitializeComponent();
            NotificationControl.ItemsSource = Notifications;
            Notifications.CollectionChanged += Notifications_CollectionChanged;
        }

        private void Notifications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var desktopArea = System.Windows.SystemParameters.WorkArea;
            if (Notifications.Count < 4)
            {
                this.Height = Notifications.Count * 108;
                NotificationContainer.Height = Notifications.Count * 108;
                //if (Notifications.Count > 2) 
                //{
                //    this.Height += 52;
                //}
                this.Top = desktopArea.Bottom - this.Height;
            }
        }

        public void RemoveNotification(Notification notification)
        {
            Notifications.Remove(notification);
        }

        public void Enqueue(Notification notification)
        {
            Notifications.Add(notification);
        }

        public void Dequeue()
        {
            Notifications.RemoveAt(0);
        }

        private void NotificationControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //this.Height = NotificationControl.Coun
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Runtime.TaskRun(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    Application.Current.Dispatcher?.Invoke(() =>
                    {
                        var not = new Notification("Header " + i, "message " + i, RemoveNotification);
                        Notifications.Add(not);
                        Thread.Sleep(1000);
                    });
                    Thread.Sleep(1000);
                    //if (Notifications.Count + 1 > 3) {
                    //    Application.Current.Dispatcher?.Invoke(() => Dequeue());
                    //}
                }
            });
        }
    }
}
