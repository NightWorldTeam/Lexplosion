using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Lexplosion.Global;
using Lexplosion.Gui.Pages.MW;
using Lexplosion.Gui.UserControls;

namespace Lexplosion.Gui.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // хранит объект этого окна
        public static MainWindow Obj = null;

        public string instanceTitle;
        public string instanceId;
        public string instanceDescription;
        public string instanceAuthor;
        public int curseforgeInstanceId;
        public Uri instanceLogoPath;

        public ToggleButton selectedToggleButton;
        public List<string> instanceTags = new List<string>();

        private Dictionary<string, Page> Pages = new Dictionary<string, Page>();

        public MainWindow()
        {
            InitializeComponent();
            MainWindow.Obj = this;

            MouseDown += delegate { try { DragMove(); } catch { } };
            this.PagesController<InstanceContainerPage>("MainFrame", this.MainFrame);
        }

        public void PagesController<T>(string page, Frame frame) where T : Page
        {
            Page obj;
            if (!Pages.ContainsKey(page))
            {
                obj = (Page)((T)Activator.CreateInstance(typeof(T), new object[1] { this }));
                Pages[page] = obj;
            }
            else
            {
                obj = Pages[page];
            }

            frame.Navigate(obj);
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