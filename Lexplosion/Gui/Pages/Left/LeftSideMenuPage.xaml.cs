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
        //public static LeftSideMenuPage instance = null;
        public static string selectedInstance = "";
        private Dictionary<string, bool> IsInstalled = new Dictionary<string, bool>();
        private ToggleButton selected;
        private ToggleButton selectedFavoriteInstance;

        public LeftSideMenuPage()
        {
            // instance = this;
            InitializeComponent();
            selected = this.Instances;

            foreach (string pack in UserData.InstancesList.Keys)
            {
                //отрисовываем кнопки в цикле
                ToggleButton button = UpdatePacks(UserData.InstancesList[pack], pack);

                // если выбранный модпак равен этому модпаку тогда присваиваем ему IsChecked = true
                if (UserData.settings["selectedModpack"] == pack) 
                {
                    button.IsChecked = true;
                    selectedFavoriteInstance = button;

                }
            }

            if (LaunchGame.isRunning)
            {
                //selectedInstance
            }
        }

        private Uri instancePage = new Uri("pack://application:,,,/Gui/Pages/Right/Instance/InstancePage.xaml");
        private Uri overviewPage = new Uri("pack://application:,,,/Gui/Pages/Right/Modpack/OverviewPage.xaml");
        private Uri versionPage = new Uri("pack://application:,,,/Gui/Pages/Right/Modpack/VersionPage.xaml");
        private Uri modsListPage = new Uri("pack://application:,,,/Gui/Pages/Right/Modpack/ModsListPage.xaml");

        private Uri modpacksContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/ModpacksContainerPage.xaml");
        private Uri favoritesContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/FavoritesContainerPage.xaml");
        private Uri serversContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/ServersContainerPage.xaml");
        private Uri settingsContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/SettingsContainerPage.xaml");

        //Надо дописать динамическое добавление кнопок меню.
        // Или можно уже не добавлять
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

        public ToggleButton UpdatePacks(string instanceName, string pack)
        {
            ToggleButton instanceButton = new ToggleButton()
            {
                Width = 242,
                Height = 60,
                Content = instanceName,
                Style = (Style)Application.Current.FindResource("MWCBS1"),
                Name = pack
            };

            instanceButton.Click += FavoriteInstanceButtonClick;
            FavoriteInstancesPanel.Children.Add(instanceButton);

            return instanceButton;
        }
        private void FavoriteInstanceButtonClick(object sender, RoutedEventArgs e) 
        {
            ToggleButton button = (ToggleButton)sender;
            if (button.Name != selected.Name)
            {
                button.IsChecked = true;
                selected.IsChecked = false;
                selected = button;
                selectedInstance = button.Name;
            }
            else //костыль. Что бы при повтороном клике IsChecked не слетало
            {
                button.IsChecked = true;
            }
        }

        private void OpenInstances(object sender, RoutedEventArgs e) 
        {
            ToggleButton button = (ToggleButton)sender;
            if (button.Name != selected.Name)
            {
                button.IsChecked = true;
                selected.IsChecked = false;
                selected = button;
            }
            else //костыль. Что бы при повтороном клике IsChecked не слетало
            {
                button.IsChecked = true;
            }

            MainWindow.instance.RightSideFrame.Source = modpacksContainerPage;
        }
        
        private void OnlineGame(object sender, RoutedEventArgs e) 
        {
            ToggleButton button = (ToggleButton)sender;
            if (button.Name != selected.Name)
            {
                button.IsChecked = true;
                selected.IsChecked = false;
                selected = button;
            }
            else //костыль. Что бы при повтороном клике IsChecked не слетало
            {
                button.IsChecked = true;
            }

            MainWindow.instance.RightSideFrame.Source = serversContainerPage;
        }

        private void Favorites(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            if (button.Name != selected.Name)
            {
                button.IsChecked = true;
                selected.IsChecked = false;
                selected = button;

            }
            else //костыль. Что бы при повтороном клике IsChecked не слетало
            {
                button.IsChecked = true;
            }

            LeftSideMenu.Visibility = Visibility.Hidden;
            FavoriteInstancesPanel.Visibility = Visibility.Visible;
            MainWindow.instance.RightSideFrame.Source = favoritesContainerPage;
        }

        private void Settings(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            if (button.Name != selected.Name)
            {
                button.IsChecked = true;
                selected.IsChecked = false;
                selected = button;
            }
            else //костыль. Что бы при повтороном клике IsChecked не слетало
            {
                button.IsChecked = true;
            }

            MainWindow.instance.RightSideFrame.Source = settingsContainerPage;
        }

        private void UserProfile(object sender, RoutedEventArgs e)
        {

        }

        private void AddCustomModpack(object sender, RoutedEventArgs e)
        {

        }

        private void LauncherSettings(object sender, RoutedEventArgs e)
        {

        }

        private void Network(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(WithDirectory.ImportInstance(@"C:\Users\Putin\Desktop\struct\lt.zip", out _).ToString());
        }

        private void MenuArrow(object sender, RoutedEventArgs e)
        {
            // при клике по стрелке
            // проверяем Margin нашего меню если 320(закрыто)
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
