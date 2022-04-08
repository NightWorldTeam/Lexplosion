using Lexplosion.Global;
using Lexplosion.Gui.InstanceCreator;
using Lexplosion.Gui.Pages;
using Lexplosion.Gui.Pages.Instance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.UserControls
{
    public partial class LeftPanel : UserControl
    {
        private void SetupUserLogin()
        {
            UserLogin.Text = UserData.Login;
            if (UserData.Offline)
                UserStatus.Fill = Brushes.Red;
        }

        public void AddCustomModpack()
        {
            HiddenDropDownMenuAnimation();
            _activePageType = PageType.Installers;



            var content = new Dictionary<string, ToggleItem>();
            content.Add("General", new ToggleItem("Основное", "InstanceCreateMainPage", new InstanceCreateMainPage(_mainWindow)));
            content.Add("Mods", new ToggleItem("Моды", "InstanceCreateMainPage", new SettingsPage(_mainWindow)));
            content.Add("Resourcepacks", new ToggleItem("Текстуры", "InstanceCreateMainPage", new SettingsPage(_mainWindow)));
            content.Add("Shaderspacks", new ToggleItem("Шейдеры", "InstanceCreateMainPage", new SettingsPage(_mainWindow)));
            _mainWindow.PagesController("SwitcherPage", _mainWindow.RightFrame, delegate ()
            {
                return new SwitcherPage("Добавление сборки", content, _mainWindow);
            });

            InitializeContent("Создать сборку", "...", "Импорт сборки", "Назад");
            ReselectionButton(MenuButton0);
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
