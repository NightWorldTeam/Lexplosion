using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Lexplosion.Logic;
using Lexplosion.Objects;

namespace Lexplosion.Gui
{
    
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private string login = "";
        private string password = "";

        // Переменные для хранения значения водных знаков
        private const string Login_WaterMark = "Логин";
        private const string Password_WaterMark = "Пароль";

        public AuthWindow()
        {  
            InitializeComponent();

            MouseDown += delegate { try { DragMove(); } catch {} };

            // Установка водного знака для поля
            if (TBLogin.Text == string.Empty && TBPassword.Password == string.Empty) { 
                TBLogin.Text = Login_WaterMark;
                TBPassword.Password = Password_WaterMark;
            }

            if (UserData.settings != null && UserData.settings.ContainsKey("login") && UserData.settings.ContainsKey("password"))
            {
                TBLogin.Text = UserData.settings["login"];
                TBPassword.Password = UserData.settings["password"];
                SaveMe.IsChecked = true;
            }
        }


        private void Register(object sender, RoutedEventArgs e)
        {
            RegisterWindow regWindow = new RegisterWindow
            {
                Left = this.Left,
                Top = this.Top,
                WindowState = WindowState.Normal
            };
            regWindow.Show(); regWindow.Activate();
            this.Close();
        }

        private void Auth(object sender, RoutedEventArgs e)
        {

            login = TBLogin.Text.ToString();

            if (TBPassword.Password .ToString() != "" && TBPassword.Password .ToString() != null && TBPassword.Password .ToString().Trim() != string.Empty)
                password = TBPassword.Password .ToString();

            if (password == Password_WaterMark || password == null || password.Trim() == string.Empty || login == Login_WaterMark || login == null || login.Trim() == string.Empty)
            {
                SetMessageBox("Заполните все поля!");
                return;
            }

            Dictionary<string, string> response = ToServer.Authorization(login, password);

            if (response != null)
            {
                if (response["status"] == "OK")
                {
                    UserData.login = response["login"];
                    UserData.UUID = response["UUID"];
                    UserData.accessToken = response["accesToken"];

                    if (SaveMe.IsChecked == true)
                    {
                        UserData.settings["login"] = login;
                        UserData.settings["password"] = password;

                        WithDirectory.SaveSettings(UserData.settings);
                    }
                    ChangeTestWindow();

                }
                else
                {
                    SetMessageBox("Неверный логин или пароль!");
                }

            }
            else
            {
                SetMessageBox("Нет соединения с сервером!");
            }
        }

        void ChangeTestWindow() {
            MainWindow mainWindow = new MainWindow
            {
                Left = this.Left,
                Top = this.Top,
                WindowState = WindowState.Normal
            };
            mainWindow.Show(); mainWindow.Activate();
            this.Close();
        }

        void ChangeWindow(sbyte status)
        {
            MainWindow mainWindow = new MainWindow
            {
                Left = this.Left,
                Top = this.Top,
                WindowState = WindowState.Normal
            };
            mainWindow.Show(); mainWindow.Activate();
            this.Close();

            mainWindow.NameBlock.Text = UserData.login;
            if (status == 0)
                mainWindow.StatusBlock.Fill = Brushes.Red;
        }

        private void PlayOffline(object sender, RoutedEventArgs e)
        {
            string login = TBLogin.Text.ToString();

            if (login != null && login.Trim() != string.Empty)
            {
                UserData.offline = true;
                UserData.login = login;
                ChangeWindow(0);

            }
            else
            {
                SetMessageBox("В поле логина введите ник для оффлайн игры!");
            }
        }

        // Функционал для водных знаков --> GotFocus (Получения фокуса с элемента интерфейса)
        private void Login_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == Login_WaterMark) { 
                textbox.Text = string.Empty;
                textbox.GotFocus -= Login_GotFocus;
            }
        }

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            if (passwordBox.Password == string.Empty) {
                passwordBox.Password = string.Empty;
                PasswordBoxWaterMark.Visibility = Visibility.Collapsed;
                passwordBox.GotFocus -= Password_GotFocus;
            }
        }

        // Функционал для водных знаков --> GotFocus (Потеря фокуса с элемента интерфейса)

        private void Login_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text.Trim().Equals(string.Empty))
            {
                textbox.Text = Login_WaterMark;
                textbox.GotFocus += Login_GotFocus;
            }
        }

        private void Password_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            if (passwordBox.Password.Trim().Equals(string.Empty))
            {
                PasswordBoxWaterMark.Visibility = Visibility.Visible;
                PasswordBoxWaterMark.Text = Password_WaterMark;
                passwordBox.GotFocus += Password_GotFocus;
            }
        }


        /* <-- Функционал MessageBox --> */
        private void Okey(object sender, RoutedEventArgs e)
        {
            this.GridMessageBox.Visibility = Visibility.Collapsed;
        }

        public void SetMessageBox(string message, string title = "Ошибка")
        {
            this.GridMessageBox.Visibility = Visibility.Visible;
            this.TextMarker.Text = message;
            this.MessageTitle.Text = title;
        }

        /* <-- Функционал кастомного меню --> */
        private void CloseWindow(object sender, RoutedEventArgs e) { Process.GetCurrentProcess().Kill(); }
        private void HideWindow(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
    }
}
