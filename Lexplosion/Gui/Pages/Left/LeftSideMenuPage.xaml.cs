using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Management;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.Pages
{
    /// <summary>
    /// Interaction logic for LeftSideMenuPage.xaml
    /// </summary>
    public partial class LeftSideMenuPage : Page
    {
        public static LeftSideMenuPage instance = null;
        public static LeftSideMenuPage Obj = null;
        public string selectedInstance = "";
        private ToggleButton selectedToggleButton;
        

        public LeftSideMenuPage()
        {
            InitializeComponent();
            LeftSideMenuPage.Obj = this;
            instance = this;

            InitializeToggleButtons();

            UserLogin.Text = UserData.login;

            if (LaunchGame.runnigInstance != "")
            {
                // TODO: тут определить какой модпак запущен и ему кнопку играть заменить на кнопку завершить
            }
        }

        public static Frame GetRightSideFrame() => MainWindow.instance.RightSideFrame;

        private void InitializeToggleButtons() 
        {
            ToggleButton[] toggleButtons = new ToggleButton[4] { LeftSideMenuButton0, LeftSideMenuButton1, LeftSideMenuButton2, LeftSideMenuButton3 };
            RoutedEventHandler[] clicks = new RoutedEventHandler[4] { StoreClicked, MultiplayerClicked, LibraryClicked, SettingsClicked };
            string[] contents = new string[4] { "Каталог", "Библиотека", "Сетевая Игра", "Настройки" };

            for (int i = 0; i < 4; i++) 
            {
                var toggleButton = toggleButtons[i];
                toggleButton.Content = contents[i];
                toggleButton.Click += clicks[i];
            }

            selectedToggleButton = LeftSideMenuButton0;
        } 

        private ToggleButton GetLeftSideMenu()
        {
            ToggleButton toggleButton = new ToggleButton()
            {
                Width = 242,
                Height = 60,
                Content = " ",
                Style = (Style)Application.Current.FindResource("MWCBS1"),
                Name = "  "
            };

            return toggleButton;
        }

        private void ReselectionToggleButton(object sender) 
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            if (toggleButton.Name != selectedToggleButton.Name)
            {
                toggleButton.IsChecked = true;
                selectedToggleButton.IsChecked = false;
                selectedToggleButton = toggleButton;
            }
            else toggleButton.IsChecked = true;
        }

        private void SelectDefaultButton(object sender) 
        {
            ToggleButton[] toggleButtons = new ToggleButton[4] { LeftSideMenuButton0, LeftSideMenuButton1, LeftSideMenuButton2, LeftSideMenuButton3 };

            for (int i = 0; i < 4; i++) 
            {
                toggleButtons[i].IsChecked = false;
            }
            selectedToggleButton = LeftSideMenuButton0;
            LeftSideMenuButton0.IsChecked = true;
        } 

        private void StoreClicked(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
            MainWindow.instance.RightSideFrame.Source = GuiUris.ModpacksContainerPage;
        }

        private void MultiplayerClicked(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
            MainWindow.instance.RightSideFrame.Source = GuiUris.ServersContainerPage;
        }

        private void LibraryClicked(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);

            MainWindow.instance.RightSideFrame.Source = GuiUris.FavoritesContainerPage;
        }

        private void SettingsClicked(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);

            MainWindow.instance.RightSideFrame.Source = GuiUris.SettingsContainerPage;
        }

        public void InstanceOverview(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
            GetRightSideFrame().Source = GuiUris.InstancePage;
        }

        public void InstanceExport(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
        }

        public void InstanceSetting(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
        }

        public void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            InitializeToggleButtons();
            SelectDefaultButton(sender);
            MainWindow.instance.RightSideFrame.Source = GuiUris.ModpacksContainerPage;
        }

        private void AddCustomModpack(object sender, RoutedEventArgs e)
        {

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
