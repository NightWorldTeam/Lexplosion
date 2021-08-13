using Lexplosion.Gui.Pages.Instance;
using Lexplosion.Gui.Pages.MW;
using Lexplosion.Gui.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        public enum PageType
        {
            InstanceContainer,
            InstanceLibrary,
            MultiplayerContainer,
            LauncherSettings,
            OpenedInstance
        }

        private PageType activePageType;
        private Page pageObj;
        private MainWindow _mainWindow;

        private Dictionary<ToggleButton, Functions> Buttons = new Dictionary<ToggleButton, Functions>();

        public LeftPanel(Page obj, PageType page, MainWindow mw)
        {
            InitializeComponent();
            
            pageObj = obj;
            activePageType = page;
            _mainWindow = mw;
            ReselectionButton();
            InitializeLeftMenu();
        }

        private void InitializeLeftMenu()
        {
            switch (activePageType)
            {
                case PageType.InstanceContainer:
                    InitializeContent("Каталог", "Библиотека", "Сетевая игра", "Настройки");
                    break;
                case PageType.InstanceLibrary:
                    InitializeContent("Каталог", "Библиотека", "Сетевая игра", "Настройки");
                    break;
                case PageType.LauncherSettings:
                    InitializeContent("Каталог", "Библиотека", "Сетевая игра", "Настройки");
                    break;
                case PageType.MultiplayerContainer:
                    InitializeContent("Каталог", "Библиотека", "Сетевая игра", "Настройки");
                    break;
                case PageType.OpenedInstance:
                    InitializeContent("Модпак", "Экспорт", "Настройки", "Назад");
                    break;
            }
        }


        private void InitializeContent(string btn0, string btn1, string btn2, string btn3)
        {
            if (activePageType != PageType.OpenedInstance) {
                Buttons.Add(MenuButton0, Functions.Catalog);
                Buttons.Add(MenuButton1, Functions.Library);
                Buttons.Add(MenuButton2, Functions.Multiplayer);
                Buttons.Add(MenuButton3, Functions.LauncherSettings);
            }
            else
            {
                Buttons.Add(MenuButton0, Functions.Instance);
                Buttons.Add(MenuButton1, Functions.Export);
                Buttons.Add(MenuButton2, Functions.InstanceSettings);
                Buttons.Add(MenuButton3, Functions.Back);
            }

            MenuButton0.Content = btn0;
            MenuButton1.Content = btn1;
            MenuButton2.Content = btn2;
            MenuButton3.Content = btn3;

            InitializeFucntions();
        }

        private void InitializeFucntions()
        {
            switch (Buttons[MenuButton0])
            {
                case Functions.Catalog:
                    MenuButton0.Click += IsCatalogSelected;
                    // TODO: Установка фрейма
                    break;
                case Functions.Instance:
                    _mainWindow.PagesController<InstancePage>("InstancePage", _mainWindow.MainFrame);
                    break;
            }

            switch (Buttons[MenuButton1])
            {
                case Functions.Library:
                    MenuButton1.Click += IsLibrarySelected;
                    break;
                case Functions.Export:
                    break;
            }

            switch (Buttons[MenuButton2])
            {
                case Functions.Multiplayer:
                    MenuButton2.Click += IsMultiplayerSelected;
                    break;
                case Functions.InstanceSettings:
                    break;
            }

            switch (Buttons[MenuButton3])
            {
                case Functions.LauncherSettings:
                    MenuButton3.Click += IsLauncherSettingsSelected;
                    break;
                case Functions.Back:
                    MenuButton3.Click += IsBackSelected;
                    break;
            }
        }

        private void IsCatalogSelected(object sender, RoutedEventArgs e) 
        {
            _mainWindow.PagesController<InstanceContainerPage>("InstanceContainerPage", _mainWindow.MainFrame);
            _mainWindow.selectedToggleButton = MenuButton0;
            MenuButton1.IsChecked = false;
            MenuButton2.IsChecked = false;
            MenuButton3.IsChecked = false;
        }
        private void IsInstanceSelected(object sender, RoutedEventArgs e) 
        {
            _mainWindow.selectedToggleButton = MenuButton0;
            MenuButton1.IsChecked = false;
            MenuButton2.IsChecked = false;
            MenuButton3.IsChecked = false;
        }

        private void IsLibrarySelected(object sender, RoutedEventArgs e) 
        {
            _mainWindow.PagesController<LibraryContainerPage>("LibraryContainerPage", _mainWindow.MainFrame);
            _mainWindow.selectedToggleButton = MenuButton1;
        }

        private void IsExportSelected(object sender, RoutedEventArgs e) 
        {
            _mainWindow.selectedToggleButton = MenuButton1;
        }


        private void IsMultiplayerSelected(object sender, RoutedEventArgs e) 
        {
            _mainWindow.PagesController<MultiplayerContainerPage>("MultiplayerContainerPage", _mainWindow.MainFrame);
            _mainWindow.selectedToggleButton = MenuButton2;
        }

        private void IsInstanceSettingsSelected(object sender, RoutedEventArgs e) 
        {
            _mainWindow.selectedToggleButton = MenuButton2;
        }

        private void IsLauncherSettingsSelected(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController<SettingsContainerPage>("SettingsContainerPage", _mainWindow.MainFrame);
            _mainWindow.selectedToggleButton = MenuButton3;
        }

        private void IsBackSelected(object sender, RoutedEventArgs e) 
        {
            _mainWindow.PagesController<InstanceContainerPage>("InstanceContainerPage", _mainWindow.MainFrame);
            _mainWindow.selectedToggleButton = MenuButton3;
        }

        private void ReselectionButton() 
        {
            if (_mainWindow.selectedToggleButton != null)
            {
                foreach (ToggleButton button in LeftPanelMenu.Children)
                {
                    if (_mainWindow.selectedToggleButton != button)
                    {
                        button.IsChecked = false;
                    }
                }
            }
            else
            {
                MenuButton0.IsChecked = true;
                _mainWindow.selectedToggleButton = MenuButton0;
            }
        }
    }
}