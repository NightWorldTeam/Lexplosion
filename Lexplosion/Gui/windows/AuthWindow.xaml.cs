using System.Diagnostics;
using System.Windows;
using Lexplosion.Gui.Pages;

namespace Lexplosion.Gui.Windows
{

    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };
            ShowAuthPage();
        }

        public void ShowAuthPage() => AuthFrame.Navigate(new AuthPage(this));

        public void ShowRegisterPage() => AuthFrame.Navigate(new RegisterPage(this));

        public void ShowMainWindow() 
        {
            MainWindow mainWindow = new MainWindow
            {
                Left = this.Left,
                Top = this.Top,
                WindowState = WindowState.Normal
            };

            mainWindow.Show(); 
            mainWindow.Activate();
            this.Close();
        }

        /* <-- Функционал кастомного меню --> */
        private void CloseWindow(object sender, RoutedEventArgs e) { Process.GetCurrentProcess().Kill(); }
        private void HideWindow(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
    }
}
