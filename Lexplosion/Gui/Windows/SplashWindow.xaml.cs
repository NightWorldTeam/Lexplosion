using System.Diagnostics;
using System.Windows;

namespace Lexplosion.Gui.Windows
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };
        }

        /* <-- Функционал кастомного меню --> */
        private void CloseWindow(object sender, RoutedEventArgs e) { Process.GetCurrentProcess().Kill(); }
        private void HideWindow(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
    }
}
