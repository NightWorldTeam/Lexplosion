using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.Management;

namespace Lexplosion.Gui.Windows
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
            MouseDown += delegate { try { DragMove(); } catch { } };

            // Установка водного знака для поля
            if (TBLogin.Text == string.Empty && TBPassword.Password == string.Empty)
            {
                // Устанавливаем водяные знаки для полей Логин и Пароль
                TBLogin.Text = Login_WaterMark;
                PasswordBoxWaterMark.Text = Password_WaterMark;
            }

            if (UserData.settings != null && UserData.settings.ContainsKey("login") && UserData.settings.ContainsKey("password"))
            {
                TBLogin.Text = UserData.settings["login"];
                TBPassword.Password = UserData.settings["password"];
                PasswordBoxWaterMark.Text = "";
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
            this.Close();
            regWindow.ShowDialog(); regWindow.Activate();
        }

        private void Auth(object sender, RoutedEventArgs e)
        {

            if (!UserData.isAuthorized) //на всякий случай проверяем не авторизирован ли пользователь уже
            {
                login = TBLogin.Text.ToString();

                if (TBPassword.Password.ToString() != "" && TBPassword.Password.ToString() != null && TBPassword.Password.ToString().Trim() != string.Empty)
                    password = TBPassword.Password.ToString();

                if (password == Password_WaterMark || password == null || password.Trim() == string.Empty || login == Login_WaterMark || login == null || login.Trim() == string.Empty)
                {
                    SetMessageBox("Заполните все поля!");
                    return;
                }

                AuthCode code = ManageLogic.Auth(login, password, SaveMe.IsChecked == true);

                switch (code)
                {
                    case AuthCode.Successfully:
                        ChangeWindow(1);
                        break;

                    case AuthCode.DataError:
                        SetMessageBox("Неверный логин или пароль!");
                        break;

                    case AuthCode.NoConnect:
                        SetMessageBox("Нет соединения с сервером!");
                        break;
                }
            }

        }

        void ChangeWindow(sbyte status)
        {
            ManageLogic.DefineListInstances();

            MainWindow mainWindow = new MainWindow
            {
                Left = this.Left,
                Top = this.Top,
                WindowState = WindowState.Normal
            };
            this.Close();
            mainWindow.ShowDialog(); mainWindow.Activate();
            

            // mainWindow.NameBlock.Text = UserData.login;
            // if (status == 0)
            //    mainWindow.StatusBlock.Fill = Brushes.Red;
        }

        private void PlayOffline(object sender, RoutedEventArgs e)
        {
            string login = TBLogin.Text.ToString();

            if (login != null && login.Trim() != string.Empty)
            {
                UserData.offline = true;
                UserData.login = login;
                UserData.isAuthorized = true;

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
            if (textbox.Text == Login_WaterMark)
            {
                textbox.Text = string.Empty;
                textbox.GotFocus -= Login_GotFocus;
            }
        }

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            if (passwordBox.Password == string.Empty)
            {
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
