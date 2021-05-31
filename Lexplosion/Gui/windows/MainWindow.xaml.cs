using System.Windows;
using Lexplosion.Gui.Pages.Left;
using Lexplosion.Gui.Pages.Right.Menu;

namespace Lexplosion.Gui.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // хранит объект этого окна
        public static MainWindow Obj = null; 
        public static MainWindow instance = null;

        public MainWindow()
        {
            InitializeComponent();
            MainWindow.Obj = this;
            instance = this;

            MouseDown += delegate { try { DragMove(); } catch { } };

            LeftSideFrame.Navigate(new LeftSideMenuPage(this));
            RightSideFrame.Navigate(new InstanceContainerPage(this));
        }

        /* <-- Функционал MessageBox --> */
        private void Okey(object sender, RoutedEventArgs e)
        {

        }

        public void SetMessageBox(string message, string title = "Ошибка")
        {
            MessageBox.Show(message + " " + title);
        }

        private void CloseWindow(object sender, RoutedEventArgs e) => Run.Exit();
        private void HideWindow(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
    }
}
