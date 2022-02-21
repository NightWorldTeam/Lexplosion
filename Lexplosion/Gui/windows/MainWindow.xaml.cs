using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Lexplosion.Gui.Pages.MW;
using Lexplosion.Gui.UserControls;

namespace Lexplosion.Gui.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class MultiPageInstanceForm
        {
            public InstanceForm _libraryInstanceForm;
            public InstanceForm _catalogInstanceForm;
        }

        // хранит объект этого окна
        public static MainWindow Obj = null;

        public string InstanceTitle;
        public string InstanceId;
        public string InstanceDescription;
        public string InstanceAuthor;
        public string OutsideInstanceId;
        public Uri InstanceLogoPath;

        private Dictionary<string, Page> _pages = new Dictionary<string, Page>();

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
            if (!_pages.ContainsKey(page))
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