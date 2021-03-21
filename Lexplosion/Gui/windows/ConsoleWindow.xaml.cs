using System.Windows;

namespace Lexplosion.Gui.Windows
{
    /// <summary>
    /// Логика взаимодействия для ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        public static ConsoleWindow Window = new ConsoleWindow();
        public static bool isShow = false;

        public ConsoleWindow()
        {
            InitializeComponent();
            MouseDown += delegate { DragMove(); };
        }

        public void Update(string str)
        {
            textBlock.AppendText(str + "\r\n");
            textBlock.CaretIndex = textBlock.Text.Length;
            textBlock.ScrollToEnd();
        }

        /* <-- Кастомное меню --> */
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
            Window = new ConsoleWindow();
            isShow = false;
        }

        private void HideWindow(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
    }
}
