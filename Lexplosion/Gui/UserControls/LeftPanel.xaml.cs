using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Interaction logic for LeftPanel.xaml
    /// </summary>


    public partial class LeftPanel : UserControl
    {
        enum Functions
        {
            Catalog,
            Library,
            Multiplayer,
            LauncherSettings,
            Instance,
            Export,
            InstanceSettings,
            Back
        }

        public enum Pages
        {
            InstanceContainer,
            InstanceLibrary,
            MultiplayerContainer,
            LauncherSettings,
            OpenedInstance
        }

        private Pages activePage;

        public LeftPanel(Pages page)
        {
            InitializeComponent();
            activePage = page;
            InitializeFucntions();
            //InitializeLeftSideMenu("Каталог", "Библиотека", "Сетевая игра", "Настройки");
        }

        private void InitializeFucntions() 
        {
            switch (activePage) 
            {
                case Pages.InstanceContainer:
                    InitializeLeftSideMenu("Каталог", "Библиотека", "Сетевая игра", "Настройки");
                    break;
                case Pages.InstanceLibrary:
                    InitializeLeftSideMenu("Каталог", "Библиотека", "Сетевая игра", "Настройки");
                    break;
                case Pages.LauncherSettings:
                    InitializeLeftSideMenu("Каталог", "Библиотека", "Сетевая игра", "Настройки");
                    break;
                case Pages.MultiplayerContainer:
                    InitializeLeftSideMenu("Каталог", "Библиотека", "Сетевая игра", "Настройки");
                    break;
                case Pages.OpenedInstance:
                    InitializeLeftSideMenu("Каталог", "Библиотека", "Сетевая игра", "Настройки");
                    break;
            }
        }


        private void InitializeLeftSideMenu(string btn0, string btn1, string btn2, string btn3)
        {
            MenuButton0.Content = btn0;
            MenuButton1.Content = btn1;
            MenuButton2.Content = btn2;
            MenuButton3.Content = btn3;
        }
    }
}