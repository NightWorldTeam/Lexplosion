using Lexplosion.Tools;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Media;

namespace Lexplosion.Gui.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private Popup _trayMenu;

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(App.GetResourceStream(new Uri("pack://application:,,,/Assets/images/icons/logo.ico")).Stream);
            notifyIcon.Visible = true;
            notifyIcon.Text = "Lexplosion";

            _trayMenu = (Popup)this.TryFindResource("TTrayMenu");
            _trayMenu.Opened += _trayMenu_Opened;
            _trayMenu.Closed += _trayMenu_Closed;

            notifyIcon.Click += NofityIcon_Click;
            Runtime.ExitEvent += LauncherClosedHandler;
            Runtime.TrayMenuElementClicked += () => { _trayMenu.IsOpen = false; };
        }

        private void _trayMenu_Closed(object sender, EventArgs e)
        {
            MouseHelper.Stop();
            MouseHelper.MouseLeftClickedEvent -= MouseHelper_MouseLeftClickedEvent;
        }

        private void _trayMenu_Opened(object sender, EventArgs e)
        {
            MouseHelper.Start();
            MouseHelper.MouseLeftClickedEvent += MouseHelper_MouseLeftClickedEvent;
        }

        private void MouseHelper_MouseLeftClickedEvent(int x, int y)
        {
            var coords = NativeMethods.GetControlCoordinate(_trayMenu.Child);
            _trayMenu.IsOpen = IsPointInControl(coords[0], coords[1], coords[2], coords[3], x, y);
        }

        private void LauncherClosedHandler()
        {
            notifyIcon.Dispose();
        }

        private void NofityIcon_Click(object sender, EventArgs e)
        {
            var mouseEventArgs = e as System.Windows.Forms.MouseEventArgs;
            if (mouseEventArgs.Button == MouseButtons.Left)
            {
                Runtime.ShowApp();
                
            }
            else if (mouseEventArgs.Button == MouseButtons.Right) 
            {
                _trayMenu.IsOpen = true;
            }
        }

        private bool IsPointInControl(int left, int top, int right, int bottom, int x, int y) 
        {
            return left <= x && right >= x && top <= y && bottom >= y;
        }

        private void ChangeStatusButtonClick(object sender, RoutedEventArgs e)
        {
           var element = FindElementByName<Popup>(StatusButton, "StatusPopup");
            element.IsOpen = false;
        }

        public static T FindElementByName<T>(FrameworkElement element, string sChildName) where T : FrameworkElement
        {
            T childElement = null;
            var nChildCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < nChildCount; i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

                if (child == null)
                    continue;

                if (child is T && child.Name.Equals(sChildName))
                {
                    childElement = (T)child;
                    break;
                }

                childElement = FindElementByName<T>(child, sChildName);

                if (childElement != null)
                    break;
            }
            return childElement;
        }
    }
}
