using System.Windows;

namespace Lexplosion.Gui.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для Console.xaml
    /// </summary>
    public partial class Console : Window
    {
        public Console()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };
        }
    }
}
