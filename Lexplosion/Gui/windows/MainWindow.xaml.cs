using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Lexplosion.Objects;
using Lexplosion.Logic;
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Windows.Media.Animation;
using Lexplosion.Gui.Pages.Right.Instance;

namespace Lexplosion.Gui.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string selectedModpack = "";
        private byte selectedSection = 0; // 0 - overviem, 1 - version, 2 - mods list
        public static MainWindow Obj = null; // хранит объект этого окна
        public Dictionary<string, bool> IsInstalled = new Dictionary<string, bool>();
        public string launchedModpack = "";

        // заранее создаем переменные ссылками, чтобы потом не срать в память новыми экземплярами
        // Windows
        private Uri modpackSettingsPage = new Uri("pack://application:,,,/Gui/Windows/SettingsWindow.xaml");
        // Pages
        private Uri overviewPage = new Uri("pack://application:,,,/Gui/Pages/Right/Instance/OverviewPage.xaml");
        private Uri versionPage = new Uri("pack://application:,,,/Gui/Pages/Right/Instance/VersionPage.xaml");
        private Uri modsListPage = new Uri("pack://application:,,,/Gui/Pages/Right/Instance/ModsListPage.xaml");
        private Uri leftSideMenuPage = new Uri("pack://application:,,,/Gui/Pages/Left/LeftSideMenuPage.xaml");
        private Uri modpacksContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/ModpacksContainerPage.xaml");

        public static MainWindow instance = null;

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };
            MainWindow.Obj = this;
            instance = this;

            // updatePacks(MP_TB_StackPanel); //вызываем метод отрисовывающий все модпаки
            LeftSideFrame.Source = leftSideMenuPage; //это страница по умолчанию
            RightSideFrame.Source = modpacksContainerPage; //это страница по умолчанию

            var test = new List<string>();
            test.Add(@"C:\Games\night-world\instances\lt\mods\AdvancedSolarPanel-1.7.10-3.5.1.jar");
            test.Add(@"C:\Games\night-world\instances\lt\mods\BetterFps-1.0.1.jar");
            test.Add(@"C:\Games\night-world\instances\lt\config\AdvancedSolarPanel_MTRecipes.cfg");


            //MessageBox.Show(WithDirectory.ExportInstance("lt", test, @"C:\Users\Слава\Desktop\struct", "Пиздец какой-то").ToString());

            //MessageBox.Show(WithDirectory.ImportInstance(@"C:\Users\Putin\Desktop\struct\lt.zip").ToString());

            //selectedModpack = "lt";
            //ManageLogic.СlientManager();
        }

        private void СlientManager(object sender, RoutedEventArgs e)
        {
            if (launchedModpack != "" && selectedModpack != launchedModpack)
                return;
        }

        public void updatePacks(StackPanel stackPanel)
        {
            /*
            foreach (string pack in UserData.PacksList.Keys) //отрисовываем кнопки в цикле
            {
                ToggleButton mp_togglebutton = new ToggleButton();
                mp_togglebutton.Width = 242;
                mp_togglebutton.Height = 60;
                mp_togglebutton.Content = UserData.PacksList[pack];
                mp_togglebutton.Style = (Style)Application.Current.FindResource("MWCBS1");
                mp_togglebutton.BorderThickness = new Thickness(10, 0, 0, 0);
                mp_togglebutton.Name = pack;
                mp_togglebutton.Click += ModpackButtonClick;
                stackPanel.RegisterName(pack, mp_togglebutton);
                stackPanel.Children.Add(mp_togglebutton);

                //помещаем в список информацию о том установлен модпак или нет
                IsInstalled[pack] = WithDirectory.ModpackIsInstalled(pack);

            }

            //эта часть кода устанавливает выбранный модпак. по умолчанию устанавливается первый в списке модпак
            if (UserData.settings.ContainsKey("selectedModpack") && UserData.settings["selectedModpack"] != null)
            {
                try
                {
                    selectedModpack = UserData.settings["selectedModpack"];
                    ToggleButton buttonActive = (ToggleButton)MP_TB_StackPanel.FindName(selectedModpack);

                    if(buttonActive == null)
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

        private void ModpackButtonClick(object sender, RoutedEventArgs e) //обработчик клика по модпаку
        {
            /*
            ToggleButton toggleButton = (ToggleButton)sender;
            if (toggleButton.Name.ToString() != selectedModpack)
            {
                ToggleButton buttonActive = (ToggleButton)MP_TB_StackPanel.FindName(selectedModpack);
                buttonActive.BorderThickness = new Thickness(0, 0, 0, 0);
                buttonActive.IsChecked = false;
                selectedModpack = toggleButton.Name.ToString();
                toggleButton.IsChecked = true;

                toggleButton.BorderThickness = new Thickness(0, 0, 0, 0);
                // забыл как эта штука называется
                ThicknessAnimation animation = new ThicknessAnimation();
                // изменяемое свойство
                animation.From = new Thickness(0, 0, 0, 0);
                // на что изменяем с анимацией
                animation.To = new Thickness(10, 0, 0, 0);
                //время анимации
                animation.Duration = TimeSpan.FromSeconds(0.6);
                //старт анимиции
                toggleButton.BeginAnimation(Border.BorderThicknessProperty, animation); 

                if (IsInstalled[selectedModpack])
                {
                    if (selectedModpack == launchedModpack)
                    {
                        ClientManagement.IsEnabled = true;
                        ClientManagement.Content = "Остановить";
                    }
                    else
                    {
                        if (launchedModpack != "")
                            ClientManagement.IsEnabled = false;
                        else
                            ClientManagement.IsEnabled = true;

                        ClientManagement.Content = "Играть";
                    }
                }
                else
                {
                    if(launchedModpack != "")
                        ClientManagement.IsEnabled = false;
                    else
                        ClientManagement.IsEnabled = true;

                    ClientManagement.Content = "Загрузить";
                }
                // если выбрана Overview то отрисовываем асестсы данного модпака. Так же, на всякий случай проверяем OverviewPage.instance на null
                if (selectedSection == 0 && OverviewPage.instance != null) 
                {
                    OverviewPage.instance.SetAssets();
                }

                UserData.settings["selectedModpack"] = selectedModpack;
                // TODO: добавить асинхронность
                WithDirectory.SaveSettings(UserData.settings); 
            }
            else
            {
                //фикс border Tbutton
                toggleButton.IsChecked = true; 
            }
            */
        }

        /* Алгоритм таков: при клике на одну из кнопок (ну сверху которые) мы берем экземпляр класса OverviewPage из 
         * его же статичного поля instance и вызываем метод SetAssets, в который передаем название выбранного модпака и его асетсы.
         * Полю instance экземляр класса присваевается при создании класса (в конструкторе), но по какой-то причине при самой первой
         * отрисовке SetAssets вызывался раньше, чем полю instance успевает присвоиться значение. Поэтому в классе OverviewPage
         * метод SetAssets вызывается примо в конструкторе (поэтому поля selectedModpack и modpacksAssets пришлось 
         * сделать статичными: чтобы вызываемый в конструкторе класса OverviewPage метод SetAssets мог получить нужные ему значения). Происходит это только 1 раз.
         * Короче, самая первая отрисовка асестсов происходит в конструкторе класса OverviewPage.
         */

        private void OverviewClick(object sender, RoutedEventArgs e)
        {
            // нужно чтобы при повторном клике на эту же кнопку всё не отрисовывалось второй раз
            if (selectedSection != 0) 
            {
                // OverviewTB.IsChecked = true;
                // VersionTB.IsChecked = false;
                // ModsListTB.IsChecked = false;

                selectedSection = 0;
                // MenuFrame.Source = overviewPage;
                //проверяем на нул, ведь при срабатывании этого метода конструктор класса OverviewPage возможно еще не сработал
                if (OverviewPage.instance != null) 
                {
                    //отрисовываем асетсы
                    OverviewPage.instance.SetAssets(); 
                }
            }
            else
            {
                // OverviewTB.IsChecked = true;
            }

        }

        private void VersionClick(object sender, RoutedEventArgs e)
        {
            if (selectedSection != 1)
            {
                // OverviewTB.IsChecked = false;
                // VersionTB.IsChecked = true;
                // ModsListTB.IsChecked = false;

                selectedSection = 1;
                // MenuFrame.Source = versionPage;
            }
            else
            {
                // VersionTB.IsChecked = true;
            }
        }

        private void ModsListClick(object sender, RoutedEventArgs e)
        {
            if (selectedSection != 2)
            {
                // OverviewTB.IsChecked = false;
                //VersionTB.IsChecked = false;
                //ModsListTB.IsChecked = true;

                selectedSection = 2;
               // MenuFrame.Source = modsListPage;
            }
            else
            {
                //ModsListTB.IsChecked = true;
            }

        }

        public void SetProcessBar(string title)
        {
            //InitProgressBar.Visibility = Visibility.Visible;
            //ProcessText.Text = title;
        }

        private void SetProgress(int procent)
        {
            //this.GridLoadingWindow.Visibility = Visibility.Visible;
            //this.ProgressBar.Value = procent;
        }

        /* <-- Функционал MessageBox --> */
        private void Okey(object sender, RoutedEventArgs e)
        {
            //this.GridMessageBox.Visibility = Visibility.Collapsed;
        }

        public void SetMessageBox(string message, string title = "Ошибка")
        {
            //this.GridMessageBox.Visibility = Visibility.Visible;
            //this.TextMarker.Text = message;
            //this.MessageTitle.Text = title;
        }

        private void OpenedModPackSettings(object sender, RoutedEventArgs e) {
            SettingsWindow window = new SettingsWindow
            {
                Left = this.Left + 188,
                Top = this.Top + 129

            };
            window.ShowDialog();
            window.Activate();
        }

        /* <-- Функционал кастомного меню --> */
        private void CloseWindow(object sender, RoutedEventArgs e) => Process.GetCurrentProcess().Kill();
        private void HideWindow(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

    }
}
