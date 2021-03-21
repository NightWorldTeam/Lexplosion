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
using Lexplosion.Gui.Pages;

namespace Lexplosion.Gui.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string selectedModpack = "";
        private byte selectedSection = 0; // 0 - overviem, 1 - version, 2 - mods list
        public static MainWindow window = null;
        private Dictionary<string, bool> IsInstalled = new Dictionary<string, bool>();
        public string launchedModpack = "";

        // заранее создаем переменные ссылками, чтобы потом не срать в память новыми экземплярами
        // Windows
        private Uri modpackSettingsPage = new Uri("pack://application:,,,/Gui/Windows/SettingsWindow.xaml");
        // Pages
        private Uri overviewPage = new Uri("pack://application:,,,/Gui/Pages/OverviewPage.xaml");
        private Uri versionPage = new Uri("pack://application:,,,/Gui/Pages/VersionPage.xaml");
        private Uri modsListPage = new Uri("pack://application:,,,/Gui/Pages/ModsListPage.xaml");
        private Uri leftSideMenuPage = new Uri("pack://application:,,,/Gui/Pages/LeftSideMenuPage.xaml");
        private Uri profilesContainerPage = new Uri("pack://application:,,,/Gui/Pages/ProfilesContainerPage.xaml");

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };
            MainWindow.window = this;

            if (UserData.PacksList == null)
            {
                if (!UserData.offline)
                    UserData.PacksList = ToServer.GetModpaksList();
                else
                    UserData.PacksList = WithDirectory.GetModpaksList();
            }

            // updatePacks(MP_TB_StackPanel); //вызываем метод отрисовывающий все модпаки
            LeftSideFrame.Source = leftSideMenuPage; //это страница по умолчанию
            RightSideFrame.Source = profilesContainerPage; //это страница по умолчанию

            //selectedModpack = "lt";
            //СlientManager(null, null);
        }

        private void СlientManager(object sender, RoutedEventArgs e)
        {
            if (launchedModpack != "" && selectedModpack != launchedModpack)
                return;

            if (LaunchGame.isRunning)
            {
                LaunchGame.KillProcess();
                return;
            }

            SetProcessBar("Выполняется запуск игры");

            if (UserData.PacksList.ContainsKey(selectedModpack))
            {
                Dictionary<string, string> xmx = new Dictionary<string, string>();
                xmx["eos"] = "2700";
                xmx["tn"] = "2048";
                xmx["oth"] = "2048";
                xmx["lt"] = "512";

                int k = 0;
                int c = 0;
                if (xmx.ContainsKey(selectedModpack) && int.TryParse(xmx[selectedModpack], out k) && int.TryParse(UserData.settings["xmx"], out c))
                {
                    if (c < k)
                        SetMessageBox("Клиент может не запуститься из-за малого количества выделенной памяти. Рекомендуется выделить " + xmx[selectedModpack] + "МБ", "Предупреждение");
                }

                new Thread(delegate () {
                    Run(selectedModpack);
                }).Start();

                void Run(string initModPack)
                {
                    Dictionary<string, string> profileSettings = WithDirectory.GetSettings(initModPack);
                    InitData data = LaunchGame.Initialization(initModPack, profileSettings);

                    if (data != null)
                    {
                        if (data.errors.Contains("javaPathError"))
                        {
                            this.Dispatcher.Invoke(delegate {
                                SetMessageBox("Не удалось определить путь до Java!", "Ошибка 940");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                            return;

                        }
                        else if (data.errors.Contains("gamePathError"))
                        {
                            this.Dispatcher.Invoke(delegate {
                                SetMessageBox("Ошибка при определении игровой директории!", "Ошибка 950");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                            return;

                        }
                        else if (UserData.offline && (UserData.settings.ContainsKey(selectedModpack + "-update") && UserData.settings[selectedModpack + "-update"] == "true"))
                        { //если лаунчер запущен в оффлайн режиме и выбранный модпак поставлен на обновление
                            this.Dispatcher.Invoke(delegate {
                                SetMessageBox("Клиент поставлен на обновление, но лаунчер запущен в оффлайн режиме! Войдите в онлайн режим.", "Ошибка 980");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                            return;

                        }
                        else if ((data.files == null && (UserData.offline || UserData.settings["noUpdate"] == "true")) && !(UserData.settings.ContainsKey(selectedModpack + "-update") && UserData.settings[selectedModpack + "-update"] == "true"))
                        { //если  data.files равно null при вылюченных обновлениях или при оффлайн игре. При том модпак не стоит на обновлении
                            this.Dispatcher.Invoke(delegate {
                                SetMessageBox("Вы должны хотя бы 1 раз запустить клиент в онлайн режиме и с включенными обновлениями!", "Ошибка 970");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                            return;

                        }
                        else if (data.files == null)
                        {
                            this.Dispatcher.Invoke(delegate {
                                SetMessageBox("Не удалось запустить игру!", "Ошибка 930");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                            return;
                        }

                        string errorsText = "\n\n";
                        foreach (string error in data.errors)
                            errorsText += error + "\n";

                        if (errorsText == "\n\n")
                        {
                            string command = LaunchGame.FormCommand(initModPack, data.files.version, data.files.version.minecraftJar.name, data.files.libraries, profileSettings);
                            LaunchGame.Run(command, initModPack);
                            WithDirectory.SaveSettings(UserData.settings);

                            this.Dispatcher.Invoke(delegate {
                                launchedModpack = selectedModpack;
                                IsInstalled[selectedModpack] = true;
                                //ClientManagement.Content = "Остановить";
                            });

                        }
                        else
                        {
                            this.Dispatcher.Invoke(delegate {
                                SetMessageBox("Не удалось загрузить следующие файлы:" + errorsText, "Ошибка 960");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                        }

                        data = null;

                    }
                    else
                    {
                        this.Dispatcher.Invoke(delegate {
                            SetMessageBox("Не удалось запустить игру!", "Ошибка 930");
                            //InitProgressBar.Visibility = Visibility.Collapsed;
                        });
                    }

                }

            }
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
