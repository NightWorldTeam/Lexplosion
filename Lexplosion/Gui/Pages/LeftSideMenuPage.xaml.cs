using Lexplosion.Logic;
using Lexplosion.Objects;
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
        public static LeftSideMenuPage instance = null;
        public string selectedModpack = "";
        private Dictionary<string, bool> IsInstalled = new Dictionary<string, bool>();
        public LeftSideMenuPage()
        {
            InitializeComponent();
            instance = this;
            UpdatePacks(MP_TB_StackPanel);
        }

        public void UpdatePacks(StackPanel stackPanel)
        {
            foreach (string pack in UserData.PacksList.Keys) //отрисовываем кнопки в цикле
            {
                ToggleButton mp_togglebutton = new ToggleButton();
                mp_togglebutton.Width = 242;
                mp_togglebutton.Height = 60;
                mp_togglebutton.Content = UserData.PacksList[pack];
                mp_togglebutton.Style = (Style)Application.Current.FindResource("MWCBS1");
                mp_togglebutton.BorderThickness = new Thickness(10, 0, 0, 0);
                mp_togglebutton.Name = pack;
                //mp_togglebutton.Click += ModpackButtonClick;
                stackPanel.RegisterName(pack, mp_togglebutton);
                stackPanel.Children.Add(mp_togglebutton);

                //помещаем в список информацию о том установлен модпак или нет
                IsInstalled[pack] = WithDirectory.InstanceIsInstalled(pack);

            }

            //эта часть кода устанавливает выбранный модпак. по умолчанию устанавливается первый в списке модпак
            if (UserData.settings.ContainsKey("selectedModpack") && UserData.settings["selectedModpack"] != null)
            {
                try
                {
                    selectedModpack = UserData.settings["selectedModpack"];
                    ToggleButton buttonActive = (ToggleButton)MP_TB_StackPanel.FindName(selectedModpack);

                    if (buttonActive == null)
                    {
                        var first = UserData.PacksList.First();
                        selectedModpack = first.Key;
                        buttonActive = (ToggleButton)MP_TB_StackPanel.FindName(selectedModpack);
                    }

                    buttonActive.IsChecked = true;
                }
                catch
                {
                    var first = UserData.PacksList.First();
                    selectedModpack = first.Key;

                    ToggleButton buttonActive = (ToggleButton)MP_TB_StackPanel.FindName(selectedModpack);
                    buttonActive.IsChecked = true;
                }
            }
            else
            {
                var first = UserData.PacksList.First();
                selectedModpack = first.Key;

                ToggleButton buttonActive = (ToggleButton)MP_TB_StackPanel.FindName(selectedModpack);
                buttonActive.IsChecked = true;
            }
            /*
            if (IsInstalled[selectedModpack]) { 
                ClientManagement.Content = "Играть";
            }*/
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
    }
}
