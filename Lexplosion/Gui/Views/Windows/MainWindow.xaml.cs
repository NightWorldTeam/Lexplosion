using System.Windows;

namespace Lexplosion.Gui.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };
        }
    }
}
