using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Lexplosion;

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
