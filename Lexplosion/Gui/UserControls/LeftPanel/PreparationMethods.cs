using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;


namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Interaction logic for LeftPanel.xaml
    /// </summary>
    public partial class LeftPanel : UserControl
    {
        private void InitializeContent(string btn0, string btn1, string btn2, string btn3)
        {
            if (_buttons.Count == 0)
            {
                _buttons.Add(MenuButton0, Functions.Initialize);
                _buttons.Add(MenuButton1, Functions.Initialize);
                _buttons.Add(MenuButton2, Functions.Initialize);
                _buttons.Add(MenuButton3, Functions.Initialize);
            }

            if (_activePageType != PageType.OpenedInstance && _activePageType != PageType.Installers && _activePageType != PageType.OpenedInstanceShowCase)
            {
                MenuButton1.Visibility = Visibility.Visible;
                MenuButton2.Visibility = Visibility.Visible;

                _buttons[MenuButton0] = Functions.Catalog;
                _buttons[MenuButton1] = Functions.Library;
                _buttons[MenuButton2] = Functions.Multiplayer;
                _buttons[MenuButton3] = Functions.LauncherSettings;
            }
            else if (_activePageType == PageType.Installers)
            {
                MenuButton1.Visibility = Visibility.Collapsed;
                MenuButton2.Visibility = Visibility.Collapsed;
                _buttons[MenuButton0] = Functions.Vanilla;
                _buttons[MenuButton3] = Functions.Back;
            }
            else if (_activePageType == PageType.OpenedInstanceShowCase)
            {
                MenuButton1.Visibility = Visibility.Collapsed;
                MenuButton2.Visibility = Visibility.Collapsed;
                _buttons[MenuButton0] = Functions.Instance;
                _buttons[MenuButton3] = Functions.Back;
            }
            else if (_activePageType == PageType.OpenedInstance)
            {
                MenuButton1.Visibility = Visibility.Collapsed;
                _buttons[MenuButton0] = Functions.Instance;
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
                //case Functions.AddInstance:
                //    MenuButton1.Click += AddNewInstance;
                //    break;
            }

            switch (_buttons[MenuButton2])
            {
                case Functions.Multiplayer:
                    MenuButton2.Click += MultiplayerSelected;
                    break;
                case Functions.InstanceSettings:
                    MenuButton2.Click += InstanceSettingsSelected;
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

    }
}
