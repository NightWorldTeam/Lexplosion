using System.Windows;

namespace Lexplosion.Gui.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };
        }
    }
}
