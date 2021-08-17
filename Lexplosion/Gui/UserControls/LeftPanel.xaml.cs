using Lexplosion.Gui.InstanceCreator;
using Lexplosion.Gui.Pages.Instance;
using Lexplosion.Gui.Pages.MW;
using Lexplosion.Gui.Windows;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        private List<ToggleButton> toggleButtons = new List<ToggleButton>();
        private Dictionary<ToggleButton, Functions> Buttons = new Dictionary<ToggleButton, Functions>();

        public delegate void AddCustomModpackClicked();
        public static event AddCustomModpackClicked AddModpackClicked;

        public LeftPanel(Page obj, PageType page, MainWindow mw)
        {
            InitializeComponent();

            pageObj = obj;
            activePageType = page;
            _mainWindow = mw;

            toggleButtons.Add(MenuButton0);
            toggleButtons.Add(MenuButton1);
            toggleButtons.Add(MenuButton2);
            toggleButtons.Add(MenuButton3);

            MenuButton0.IsChecked = true;
            ReselectionButton(MenuButton0);
            InitializeLeftMenu();
            InstanceForm.InstanceOpened += InitializeInstancePage;
        }

        private void InitializeLeftMenu()
        {
            InitializeContent("Каталог", "Библиотека", "Сетевая игра", "Настройки");
        }

        private void InitializeContent(string btn0, string btn1, string btn2, string btn3)
        {
            if (Buttons.Count == 0)
            {
                Buttons.Add(MenuButton0, Functions.Initialize);
                Buttons.Add(MenuButton1, Functions.Initialize);
                Buttons.Add(MenuButton2, Functions.Initialize);
                Buttons.Add(MenuButton3, Functions.Initialize);
            }

            if (activePageType != PageType.OpenedInstance)
            {
                Buttons[MenuButton0] = Functions.Catalog;
                Buttons[MenuButton1] = Functions.Library;
                Buttons[MenuButton2] = Functions.Multiplayer;
                Buttons[MenuButton3] = Functions.LauncherSettings;
            }
            else
            {
                Buttons[MenuButton0] = Functions.Instance;
                Buttons[MenuButton1] = Functions.Export;
                Buttons[MenuButton2] = Functions.InstanceSettings;
                Buttons[MenuButton3] = Functions.Back;
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
                    MenuButton0.Click += CatalogSelected;
                    break;
                case Functions.Instance:
                    MenuButton0.Click += InstanceSelected;
                    break;
            }

            switch (Buttons[MenuButton1])
            {
                case Functions.Library:
                    MenuButton1.Click += LibrarySelected;
                    break;
                case Functions.Export:
                    MenuButton1.Click += ExportSelected;
                    break;
            }

            switch (Buttons[MenuButton2])
            {
                case Functions.Multiplayer:
                    MenuButton2.Click += MultiplayerSelected;
                    break;
                case Functions.InstanceSettings:
                    MenuButton2.Click += InstanceSettingsSelected;
                    break;
            }

            switch (Buttons[MenuButton3])
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
            _mainWindow.PagesController("InstancePage", _mainWindow.RightFrame, delegate ()
            {
                return new InstancePage(_mainWindow);
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
            //_mainWindow.PagesController<InstanceSettingsPage>("InstanceExportPage", _mainWindow.RightFrame);
            ReselectionButton(MenuButton2);
        }

        private void LauncherSettingsSelected(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController("SettingsContainerPage", _mainWindow.RightFrame, delegate ()
            {
                return new SettingsContainerPage(_mainWindow);
            });
            ReselectionButton(MenuButton3);
        }

        private void BackSelected(object sender, RoutedEventArgs e)
        {
            InitializeContent("Каталог", "Библиотека", "Сетевая игра", "Настройки");
            _mainWindow.PagesController("InstanceContainerPage", _mainWindow.RightFrame, delegate ()
            {
                return new InstanceContainerPage(_mainWindow);
            });
            ReselectionButton(MenuButton0);
            activePageType = PageType.InstanceContainer;
        }

        private void ReselectionButton(ToggleButton selectedButton)
        {
            foreach (ToggleButton toggleButton in toggleButtons)
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

        private void InitializeInstancePage() 
        {
            activePageType = PageType.OpenedInstance;
            _mainWindow.PagesController("InstancePage", _mainWindow.RightFrame, delegate ()
            {
                return new InstancePage(_mainWindow);
            });
            InitializeContent("Модпак", "Экспорт", "Настройки", "Назад");
            ReselectionButton(MenuButton0);
        }

        // -- DropDownMenu -- //
        private void AddCustomModpack(object sender, RoutedEventArgs e)
        {
            AddModpackClicked.Invoke();
        }

        private void MenuArrow(object sender, RoutedEventArgs e)
        {
            if (DropDownMenu.Margin == new Thickness(0, 286, 0, 0))
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
            else if (DropDownMenu.Margin == new Thickness(0, 466, 0, 0))
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
        }
    }
}