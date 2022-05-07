using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Lexplosion.Gui.Pages.Instance;
using Lexplosion.Gui.Pages.MW;
using Lexplosion.Gui.UserControls;

namespace Lexplosion.Gui.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly static List<string> ScreenResolutions = new List<string>()
        {
            "1920x1080", "1768x992", "1680x1050",  "1600x1024", "1600x900", "1440x900", "1280x1024",
            "1280x960", "1366x768", "1360x768", "1280x800", "1280x768", "1152x864", "1280x720", "1176x768",
            "1024x768", "800x600", "720x576", "720x480", "640x480"
        };

        public class MultiPageInstanceForm
        {
            public InstanceForm _libraryInstanceForm;
            public InstanceForm _catalogInstanceForm;
        }

        // хранит объект этого окна
        public static MainWindow Obj;

        private Dictionary<string, Page> _pages = new Dictionary<string, Page>();
        private Dictionary<string, Page> _instancePages = new Dictionary<string, Page>();

        public Dictionary<string, MultiPageInstanceForm> ActiveInstanceForm = new Dictionary<string, MultiPageInstanceForm>();
        public Dictionary<string, InstanceForm> DownloadingInstanceForms = new Dictionary<string, InstanceForm>();

        public Page SelectedPage;
        public LeftPanel LeftPanel;

        public static bool IsGameRun;

        public delegate Page CreateObject();

        public MainWindow()
        {
            InitializeComponent();
            MainWindow.Obj = this;
            MouseDown += delegate { try { DragMove(); } catch { } };
            SelectedPage = InstanceContainerPage.Obj;
            InitializeLeftMenu();
            IsGameRun = false;
        }

        private void InitializeLeftMenu()
        {
            LeftPanel = new LeftPanel(SelectedPage, LeftPanel.PageType.InstanceContainer, this);
            Grid.SetColumn(LeftPanel, 0);
            MainColumns.Children.Add(LeftPanel);

            this.PagesController("InstanceContainerPage", this.RightFrame, delegate ()
            {
                return new InstanceContainerPage(this);
            });
        }

        public void PagesController(string page, Frame frame, CreateObject createObject)
        {
            Page obj;
            if (page == "InstancePage" || page == "OverviewPage" || page == "ModsListPage" || page == "VersionPage")
            {
                obj = createObject();
            }
            else if (!_pages.ContainsKey(page))
            {
                obj = createObject();
                _pages[page] = obj;
            }
            else
                obj = _pages[page];

            frame.Navigate(obj);
        }

        /* <-- Функционал MessageBox --> */
        private void Okay(object sender, RoutedEventArgs e)
        {

        }

        public void SetMessageBox(string message, string title = "Ошибка") => MessageBox.Show(message + " " + title);

        private void CloseWindow(object sender, RoutedEventArgs e) => Run.Exit();
        private void HideWindow(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
    }
}