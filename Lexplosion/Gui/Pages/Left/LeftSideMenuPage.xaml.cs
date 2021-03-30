using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public LeftSideMenuPage()
        {
            // instance = this;
            InitializeComponent();
            foreach (string pack in UserData.InstancesList.Keys) //отрисовываем кнопки в цикле
            {
                UpdatePacks(UserData.InstancesList[pack], pack);
            }
        }

        private Uri instancePage = new Uri("pack://application:,,,/Gui/Pages/Right/Instance/InstancePage.xaml");
        private Uri overviewPage = new Uri("pack://application:,,,/Gui/Pages/Right/Modpack/OverviewPage.xaml");
        private Uri versionPage = new Uri("pack://application:,,,/Gui/Pages/Right/Modpack/VersionPage.xaml");
        private Uri modsListPage = new Uri("pack://application:,,,/Gui/Pages/Right/Modpack/ModsListPage.xaml");

        private Uri modpacksContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/ModpacksContainerPage.xaml");
        private Uri favoritesContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/FavoritesContainerPage.xaml");
        private Uri serversContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/ServersContainerPage.xaml");
        
        public void UpdatePacks(string instanceName, string pack)
        {
            ToggleButton instanceButton = new ToggleButton
            {
                Width = 242,
                Height = 60,
                Content = instanceName,
                Style = (Style)Application.Current.FindResource("MWCBS1"),
                BorderThickness = new Thickness(10, 0, 0, 0),
                Name = pack
            };

            instanceButton.Click += FavoriteInstanceButtonClick;
            FavoriteInstancesPanel.Children.Add(instanceButton);
        }
        private void FavoriteInstanceButtonClick(object sender, RoutedEventArgs e) 
        {

            selectedInstance = ((ToggleButton)sender).Name;

        }

        private void Search(object sender, RoutedEventArgs e) 
        {
            MainWindow.instance.RightSideFrame.Source = modpacksContainerPage;
        }
        
        private void Servers(object sender, RoutedEventArgs e) 
        {
            MainWindow.instance.RightSideFrame.Source = serversContainerPage;
        }

        private void Favorites(object sender, RoutedEventArgs e)
        {
       
            FavoriteInstancesPanel.Visibility = Visibility.Visible;
            LeftSideMenu.Visibility = Visibility.Hidden;
            MainWindow.instance.RightSideFrame.Source = instancePage;
        }

        private void Settings(object sender, RoutedEventArgs e)
        {

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

        }

        private void MenuArrow(object sender, RoutedEventArgs e)
        {
            // при клике по стрелке
            // проверяем Margin нашего меню если 320(закрыто)
            if (DropDownMenu.Margin == new Thickness(0, 286, 0, 0))
            {
                // открываем с анимицией
                DropDownMenuSwitcher.IsChecked = true;
                // забыл как эта штука называется
                ThicknessAnimation animation = new ThicknessAnimation();
                // изменяемое свойство
                animation.From = DropDownMenu.Margin;
                // на что изменяем с анимацией
                animation.To = new Thickness(0, 466, 0, 0);
                // время анимации
                animation.Duration = TimeSpan.FromSeconds(0.7);
                // старт анимиции
                DropDownMenu.BeginAnimation(Canvas.MarginProperty, animation);
            }
            else if (DropDownMenu.Margin == new Thickness(0, 466, 0, 0))
            {
                // открываем с анимицией
                // делаем кнопку активной
                DropDownMenuSwitcher.IsChecked = false;
                // забыл как эта штука называется
                ThicknessAnimation animation = new ThicknessAnimation();
                // изменяемое свойство
                animation.From = DropDownMenu.Margin;
                // на что изменяем с анимацией
                animation.To = new Thickness(0, 286, 0, 0);
                //время анимации
                animation.Duration = TimeSpan.FromSeconds(0.5);
                //старт анимиции
                DropDownMenu.BeginAnimation(Canvas.MarginProperty, animation);
            }
        }
    }
}
