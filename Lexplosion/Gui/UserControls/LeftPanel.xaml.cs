using Lexplosion.Global;
using Lexplosion.Gui.InstanceCreator;
using Lexplosion.Gui.Pages;
using Lexplosion.Gui.Pages.Instance;
using Lexplosion.Gui.Pages.MW;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Interaction logic for LeftPanel.xaml
    /// </summary>
    public partial class LeftPanel : UserControl
    {
        enum Functions
        {
            Initialize,
            // Главное меню
            Catalog,
            Library,
            Multiplayer,
            LauncherSettings,
            // Профиль сборки
            Instance,
            Export,
            InstanceSettings,
            // Установки
            Vanilla,
            AddInstance,
            ImportInstance,
            // Другое
            Back
        }

        public enum PageType
        {
            InstanceContainer,
            InstanceLibrary,
            MultiplayerContainer,
            LauncherSettings,
            OpenedInstance,
            Installers,
        }

        private PageType _activePageType;
        private MainWindow _mainWindow;
        private InstanceProperties _instanceProperties;

        private List<ToggleButton> _toggleButtons = new List<ToggleButton>();
        private Dictionary<ToggleButton, Functions> _buttons = new Dictionary<ToggleButton, Functions>();

        public delegate void AddCustomModpackClicked();
        public static event AddCustomModpackClicked AddModpackClicked;

        public LeftPanel(Page obj, PageType page, MainWindow mw)
        {
            InitializeComponent();

            _activePageType = page;
            _mainWindow = mw;

            _toggleButtons.Add(MenuButton0);
            _toggleButtons.Add(MenuButton1);
            _toggleButtons.Add(MenuButton2);
            _toggleButtons.Add(MenuButton3);

            MenuButton0.IsChecked = true;
            ReselectionButton(MenuButton0);
            InitializeContent("Каталог", "Библиотека", "Сетевая игра", "Настройки");
            InstanceForm.InstanceOpened += InitializeInstancePage;
            SetupUserLogin();

        }

        private void SetupUserLogin() 
        {
            UserLogin.Text = UserData.Login;
            if (UserData.Offline)
                UserStatus.Fill = Brushes.Red;
        }

        private void InitializeContent(string btn0, string btn1, string btn2, string btn3)
        {
            if (_buttons.Count == 0)
            {
                _buttons.Add(MenuButton0, Functions.Initialize);
                _buttons.Add(MenuButton1, Functions.Initialize);
                _buttons.Add(MenuButton2, Functions.Initialize);
                _buttons.Add(MenuButton3, Functions.Initialize);
            }

            if (_activePageType != PageType.OpenedInstance && _activePageType != PageType.Installers)
            {
                _buttons[MenuButton0] = Functions.Catalog;
                _buttons[MenuButton1] = Functions.Library;
                _buttons[MenuButton2] = Functions.Multiplayer;
                _buttons[MenuButton3] = Functions.LauncherSettings;
            }
            else if (_activePageType == PageType.Installers) 
            {
                _buttons[MenuButton0] = Functions.Vanilla;
                _buttons[MenuButton1] = Functions.AddInstance;
                _buttons[MenuButton2] = Functions.ImportInstance;
                _buttons[MenuButton3] = Functions.Back;
            }
            else if (_activePageType == PageType.OpenedInstance)
            {
                _buttons[MenuButton0] = Functions.Instance;
                _buttons[MenuButton1] = Functions.Export;
                _buttons[MenuButton2] = Functions.InstanceSettings;
                _buttons[MenuButton3] = Functions.Back;
            }

            MenuButton0.Content = btn0;
            MenuButton1.Content = btn1;
            MenuButton2.Content = btn2;
            MenuButton3.Content = btn3;

            InitializeFunctions();
        }

        private void InitializeFunctions()
        {
            switch (_buttons[MenuButton0])
            {
                case Functions.Catalog:
                    MenuButton0.Click += CatalogSelected;
                    break;
                case Functions.Instance:
                    MenuButton0.Click += InstanceSelected;
                    break;
            }

            switch (_buttons[MenuButton1])
            {
                case Functions.Library:
                    MenuButton1.Click += LibrarySelected;
                    break;
                case Functions.Export:
                    MenuButton1.Click += ExportSelected;
                    break;
                case Functions.AddInstance:
                    MenuButton1.Click += AddNewInstance;
                    break;
            }

            switch (_buttons[MenuButton2])
            {
                case Functions.Multiplayer:
                    MenuButton2.Click += MultiplayerSelected;
                    break;
                case Functions.InstanceSettings:
                    MenuButton2.Click += InstanceSettingsSelected;
                    break;
                case Functions.ImportInstance:
                    MenuButton2.Click += ImportInstance;
                    break;
            }

            switch (_buttons[MenuButton3])
            {
                case Functions.LauncherSettings:
                    MenuButton3.Click += LauncherSettingsSelected;
                    break;
                case Functions.Back:
                    MenuButton3.Click += BackSelected;
                    break;
            }
        }

        private void CatalogSelected(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController("InstanceContainerPage", _mainWindow.RightFrame, delegate ()
            {
                return new InstanceContainerPage(_mainWindow);
            });
            ReselectionButton(MenuButton0);
        }

        private void InstanceSelected(object sender, RoutedEventArgs e)
        {
            var content = new Dictionary<string, ToggleItem>();
            content.Add("Overview", new ToggleItem("Overview", "OverviewPage", new OverviewPage(_instanceProperties)));
            content.Add("Mods", new ToggleItem("Mods", "ModsListPage", new ModsListPage()));
            content.Add("Version", new ToggleItem("Version", "VersionPage", new VersionPage()));

            _mainWindow.PagesController("InstancePage" + _instanceProperties.Id, _mainWindow.RightFrame, delegate ()
            {
                return new SwitcherPage(_instanceProperties.Name, content, _mainWindow);
            });
            ReselectionButton(MenuButton0);
        }

        private void LibrarySelected(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController("LibraryContainerPage", _mainWindow.RightFrame, delegate ()
            {
                return new LibraryContainerPage(_mainWindow);
            });
            ReselectionButton(MenuButton1);
        }

        private void ExportSelected(object sender, RoutedEventArgs e)
        {
            //_mainWindow.PagesController<InstanceExportPage>("InstanceExportPage", _mainWindow.RightFrame);
            ReselectionButton(MenuButton1);   
        }

        private void MultiplayerSelected(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController("MultiplayerContainerPage", _mainWindow.RightFrame, delegate ()
            {
                return new MultiplayerContainerPage(_mainWindow);
            });
            ReselectionButton(MenuButton2);
        }

        private void InstanceSettingsSelected(object sender, RoutedEventArgs e)
        {
            var content = new Dictionary<string, ToggleItem>();
            content.Add("General", new ToggleItem("Основное", "SettingsPage", new SettingsPage(_mainWindow)));
            content.Add("Instance", new ToggleItem("Профиль", "OverviewPage", new SettingsPage(_mainWindow)));
            _mainWindow.PagesController("SwitcherPage", _mainWindow.RightFrame, delegate ()
            {
                return new SwitcherPage("Настройки сборки", content, _mainWindow);
            });
            ReselectionButton(MenuButton2);
        }

        private void LauncherSettingsSelected(object sender, RoutedEventArgs e)
        {
            var content = new Dictionary<string, ToggleItem>();
            content.Add("General", new ToggleItem("Основное", "SettingsContainerPage", new SettingsContainerPage(_mainWindow)));
            content.Add("Account", new ToggleItem("Учетная запись", "MultiplayerContainerPage", new MultiplayerContainerPage(_mainWindow)));
            content.Add("About", new ToggleItem("О программе", "MultiplayerContainerPage", new MultiplayerContainerPage(_mainWindow)));
            _mainWindow.PagesController("LancherSettingsSelected", _mainWindow.RightFrame, delegate ()
            {
                return new SwitcherPage("Настройки", content, _mainWindow);
            });
            ReselectionButton(MenuButton3);
        }

        private void AddNewInstance(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController("InstanceMasterPage", _mainWindow.RightFrame, delegate ()
            {
                return new InstanceMasterPage(_mainWindow);
            });
            ReselectionButton(MenuButton1);
        }

        private void ImportInstance(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController("InstanceImportPage", _mainWindow.RightFrame, delegate ()
            {
                return new InstanceImportPage(_mainWindow);
            });
            ReselectionButton(MenuButton2);
        }

        private void BackSelected(object sender, RoutedEventArgs e) => BackToInstanceContainer(PageType.InstanceContainer, null);

        public void BackToInstanceContainer(PageType pageType, string[] btnNames) 
        {
            _activePageType = pageType;
            if (btnNames == null) btnNames = new string[] { "Каталог", "Библиотека", "Сетевая игра", "Настройки" };
            switch (pageType) 
            {
                case PageType.InstanceContainer:
                    _mainWindow.PagesController("InstanceContainerPage", _mainWindow.RightFrame, delegate ()
                    {
                        return new InstanceContainerPage(_mainWindow);
                    });
                    ReselectionButton(MenuButton0);
                    break;
                case PageType.InstanceLibrary:
                    _mainWindow.PagesController("LibraryContainerPage", _mainWindow.RightFrame, delegate ()
                    {
                        return new LibraryContainerPage(_mainWindow);
                    });
                    ReselectionButton(MenuButton1);
                    break;
            }
            InitializeContent(btnNames[0], btnNames[1], btnNames[2], btnNames[3]);
        }


        private void ReselectionButton(ToggleButton selectedButton)
        {
            foreach (ToggleButton toggleButton in _toggleButtons)
            {
                if (toggleButton != selectedButton)
                {
                    toggleButton.IsEnabled = true;
                    toggleButton.IsChecked = false;
                }
                else
                {
                    selectedButton.IsChecked = true;
                    selectedButton.IsEnabled = false;
                }
            }
        }

        private void InitializeInstancePage(InstanceProperties instanceProperties) 
        {
            _activePageType = PageType.OpenedInstance;
            _instanceProperties = instanceProperties;
            var content = new Dictionary<string, ToggleItem>();
            content.Add("Overview", new ToggleItem("Overview", "OverviewPage", new OverviewPage(_instanceProperties)));
            content.Add("Mods", new ToggleItem("Mods", "ModsListPage", new ModsListPage()));
            content.Add("Version", new ToggleItem("Version", "VersionPage", new VersionPage()));

            _mainWindow.PagesController("InstancePage" + _instanceProperties.Id, _mainWindow.RightFrame, delegate ()
            {
                return new SwitcherPage(_instanceProperties.Name, content, _mainWindow);
            });
            
            InitializeContent("Modpack", "Экспорт", "Настройки", "Назад");
            ReselectionButton(MenuButton0);
        }

        // -- DropDownMenu -- //
        public void AddCustomModpack()
        {
            HiddenDropDownMenuAnimation();
            _activePageType = PageType.Installers;
            _mainWindow.PagesController("InstanceMasterPage", _mainWindow.RightFrame, delegate ()
            {
                return new InstanceMasterPage(_mainWindow);
            });
            InitializeContent("Создать сборку", "...", "Импорт сборки", "Назад");
            ReselectionButton(MenuButton0);
            AddModpackClicked.Invoke();
        }

        private void AddCustomModpack_Click(object sender, RoutedEventArgs e) 
        {
            AddCustomModpack();
        }

        private void MenuArrow(object sender, RoutedEventArgs e)
        {
            if (DropDownMenu.Margin == new Thickness(0, 286, 0, 0))
            {
                HiddenDropDownMenuAnimation();
            }
            else if (DropDownMenu.Margin == new Thickness(0, 466, 0, 0))
            {
                ShowDropDownMenuAnimation();
            }
        }

        private void ShowDropDownMenuAnimation() 
        {
            DropDownMenuSwitcher.IsChecked = false;

            ThicknessAnimation animation = new ThicknessAnimation()
            {
                From = DropDownMenu.Margin,
                To = new Thickness(0, 286, 0, 0),
                Duration = TimeSpan.FromSeconds(0.5)
            };

            DropDownMenu.BeginAnimation(Canvas.MarginProperty, animation);
        }

        private void HiddenDropDownMenuAnimation() 
        {
            DropDownMenuSwitcher.IsChecked = true;

            ThicknessAnimation thicknessAnimation = new ThicknessAnimation()
            {
                From = DropDownMenu.Margin,
                To = new Thickness(0, 466, 0, 0),
                Duration = TimeSpan.FromSeconds(0.7),
            };

            DropDownMenu.BeginAnimation(Canvas.MarginProperty, thicknessAnimation);
        }
    }
}