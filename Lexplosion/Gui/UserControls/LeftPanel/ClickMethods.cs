using Lexplosion.Gui.InstanceCreator;
using Lexplosion.Gui.Pages;
using Lexplosion.Gui.Pages.Instance;
using Lexplosion.Gui.Pages.MW;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Interaction logic for LeftPanel.xaml
    /// </summary>
    public partial class LeftPanel : UserControl
    {
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

        private void MultiplayerSelected(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController("MultiplayerContainerPage", _mainWindow.RightFrame, delegate ()
            {
                return new MultiplayerContainerPage(_mainWindow);
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

        private void InstanceSettingsSelected(object sender, RoutedEventArgs e)
        {
            var content = new Dictionary<string, ToggleItem>();
            content.Add("General", new ToggleItem("Основное", "SettingsPage", new SettingsPage(_mainWindow, _instanceProperties.LocalId)));
            content.Add("Instance", new ToggleItem("Профиль", "OverviewPage", new SettingsPage(_mainWindow, _instanceProperties.LocalId)));
            _mainWindow.PagesController("SettingsPageSeleced", _mainWindow.RightFrame, delegate ()
            {
                return new SwitcherPage("Настройки сборки", content, _mainWindow);
            });
            ReselectionButton(MenuButton2);
        }

        private void AddNewInstance(object sender, RoutedEventArgs e)
        {
            var content = new Dictionary<string, ToggleItem>();
            content.Add("General", new ToggleItem("Основное", "InstanceCreateMainPage", new InstanceCreateMainPage(_mainWindow)));
            content.Add("Mods", new ToggleItem("Моды", "InstanceCreateMainPage", new SettingsPage(_mainWindow, _instanceProperties.LocalId)));
            content.Add("Resourcepacks", new ToggleItem("Текстуры", "InstanceCreateMainPage", new SettingsPage(_mainWindow, _instanceProperties.LocalId)));
            content.Add("Shaderspacks", new ToggleItem("Шейдеры", "InstanceCreateMainPage", new SettingsPage(_mainWindow, _instanceProperties.LocalId)));
            _mainWindow.PagesController("AddInstanceSelected", _mainWindow.RightFrame, delegate ()
            {
                return new SwitcherPage("Добавление сборки", content, _mainWindow);
            });
            ReselectionButton(MenuButton1);
        }

        //private void ImportInstance(object sender, RoutedEventArgs e)
        //{
        //    _mainWindow.PagesController("InstanceImportPage", _mainWindow.RightFrame, delegate ()
        //    {
        //        return new InstanceImportPage(_mainWindow);
        //    });
        //    ReselectionButton(MenuButton2);
        //}

        private void BackSelected(object sender, RoutedEventArgs e)
        {
            BackToInstanceContainer(PageType.InstanceContainer, null);
        }

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

        private void InstancePageSelected(InstanceProperties instanceProperties)
        {
            _instanceProperties = instanceProperties;

            var content = new Dictionary<string, ToggleItem>();
            content.Add("Overview", new ToggleItem("Overview", "OverviewPage", new OverviewPage(_instanceProperties)));
            content.Add("Mods", new ToggleItem("Mods", "ModsListPage", new ModsListPage()));
            content.Add("Version", new ToggleItem("Version", "VersionPage", new VersionPage()));

            if (instanceProperties.IsInstalled) 
            { 
                _activePageType = PageType.OpenedInstance;
            }
            else
            { 
                _activePageType = PageType.OpenedInstanceShowCase;
            }

            _mainWindow.PagesController("InstancePage" + _instanceProperties.Id, _mainWindow.RightFrame, delegate ()
            {
                return new SwitcherPage(_instanceProperties.Name, content, _mainWindow);
            });

            InitializeContent("Modpack", "Экспорт", "Настройки", "Назад");
            ReselectionButton(MenuButton0);
        }
    }
}
