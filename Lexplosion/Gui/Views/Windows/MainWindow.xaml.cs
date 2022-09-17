using Lexplosion.Tools;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Media;
using ContextMenu = System.Windows.Controls.ContextMenu;

namespace Lexplosion.Gui.Views.Windows
{
    public class NofityIconMenuItem 
    {
        public 
    }
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private Popup TrayMenu;
        public MainWindow()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(App.GetResourceStream(new Uri("pack://application:,,,/Assets/images/icons/logo.ico")).Stream);
            notifyIcon.Visible = true;
            notifyIcon.Text = "Lexplosion";


            TrayMenu = (Popup)this.TryFindResource("TTrayMenu");

            notifyIcon.Click += NofityIcon_Click;
        }

        private void NofityIcon_Click(object sender, EventArgs e)
        {
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs.Button == MouseButtons.Left)
            {
                // тут разворачивать окно.
                
            }
            else if (mouseEventArgs.Button == MouseButtons.Right) 
            {
                // открываем меню
                TrayMenu.IsOpen = true;
            }
        }

        private void MenuExitClick(object sender, RoutedEventArgs e) 
        {
            // тут закрытие программы
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
