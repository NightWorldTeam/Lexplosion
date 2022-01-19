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
        // хранит объект этого окна
        public static MainWindow Obj = null;

        public string instanceTitle;
        public string instanceId;
        public string instanceDescription;
        public string instanceAuthor;
        public string outsideInstanceId;
        public Uri instanceLogoPath;

        private Dictionary<string, Page> Pages = new Dictionary<string, Page>();
        public Page SelectedPage;
        public LeftPanel LeftPanel;

        public static bool IsGameRun;

        public delegate Page CreateObject();

        public MainWindow()
        {
            InitializeComponent();
            MainWindow.Obj = this;

            MouseDown += delegate { try { DragMove(); } catch { } };
            
            SelectedPage = InstanceContainerPage.obj;
            InitializeLeftMenu();
            IsGameRun = false;
        }

        private void InitializeLeftMenu() 
        {
            LeftPanel leftPanel = new LeftPanel(SelectedPage, LeftPanel.PageType.InstanceContainer, this);
            LeftPanel = leftPanel;
            Grid.SetColumn(leftPanel, 0);
            MainColumns.Children.Add(leftPanel);
            //this.PagesController<InstanceContainerPage>("InstanceContainerPage", this.RightFrame);

            this.PagesController("InstanceContainerPage", this.RightFrame, delegate ()
            {
                return new InstanceContainerPage(this);
            });   
        }

        public void PagesController(string page, Frame frame, CreateObject createObject)
        {
            Page obj;
            if (!Pages.ContainsKey(page))
            {
                obj = createObject();
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