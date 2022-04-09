using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.Pages
{
    /// <summary>
    /// Interaction logic for AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {

        private string _login;
        private string _password;

        // Переменные для хранения значения водных знаков
        private const string _loginWaterMark = "Логин";
        private const string _passwordWaterMark = "Пароль";
        private AuthWindow _authWindow = null;
        public AuthPage(AuthWindow aw)
        {
            InitializeComponent();
            _authWindow = aw;
            // Установка водного знака для поля
            if (TBLogin.Text == string.Empty && TBPassword.Password == string.Empty)
            {
                // Устанавливаем водяные знаки для полей Логин и Пароль
                TBLogin.Text = _loginWaterMark;
                PasswordBoxWaterMark.Text = _passwordWaterMark;
            }

            DataFilesManager.GetAccount(out _login, out _password);

            if (_login != null && _password != null)
            {
                TBLogin.Text = _login;
                TBPassword.Password = _password;
                PasswordBoxWaterMark.Text = "";
                SaveMe.IsChecked = true;
            }
        }

        private void Register(object sender, RoutedEventArgs e) => _authWindow.ShowRegisterPage();

        private bool CheckAuthData(string str, string waterMark="") 
        {
            return str != waterMark || str != null || str.Trim() != string.Empty;
        }

        private void Auth(object sender, RoutedEventArgs e)
        {
            if (!UserData.IsAuthorized) //на всякий случай проверяем не авторизирован ли пользователь уже
            {
                var inputLogin = TBLogin.Text.ToString();
                var inputPassword = TBPassword.Password.ToString();

                if (CheckAuthData(inputPassword, _passwordWaterMark) && CheckAuthData(inputLogin, _loginWaterMark)) 
                {
                    _login = inputLogin;
                    _password = inputPassword;
                }
                else
                {
                    SetMessageBox("Заполните все поля!");
                    return;
                }

                var isChecked = SaveMe.IsChecked;
                Lexplosion.Run.TaskRun(delegate () { 
                    AuthCode code = ManageLogic.Auth(_login, _password, isChecked is true);

                    this.Dispatcher.Invoke(() => { 
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
                    });
                });
            }
        }

        void ChangeWindow(sbyte status) => _authWindow.ShowMainWindow();

        private void PlayOffline(object sender, RoutedEventArgs e)
        {
            string login = TBLogin.Text.ToString();

            if (login != null && login.Trim() != string.Empty)
            {
                UserData.Offline = true;
                UserData.Login = login;
                UserData.IsAuthorized = true;

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
            if (textbox.Text == _loginWaterMark)
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


        private void PasswordVisible_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox passwordVisibile = (TextBox)sender;
            if (passwordVisibile.Text == string.Empty)
            {
                passwordVisibile.Text = string.Empty;
                PasswordBoxWaterMark.Visibility = Visibility.Collapsed;
                passwordVisibile.GotFocus -= Password_GotFocus;
            }
        }


        // Функционал для водных знаков --> GotFocus (Потеря фокуса с элемента интерфейса)

        private void Login_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text.Trim().Equals(string.Empty))
            {
                textbox.Text = _loginWaterMark;
                textbox.GotFocus += Login_GotFocus;
            }
        }

        private void Password_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            if (passwordBox.Password.Trim().Equals(string.Empty))
            {
                PasswordBoxWaterMark.Visibility = Visibility.Visible;
                PasswordBoxWaterMark.Text = _passwordWaterMark;
                passwordBox.GotFocus += Password_GotFocus;
            }
        }


        private void PasswordVisible_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox passwordVisibile = (TextBox)sender;
            if (passwordVisibile.Text.Trim().Equals(string.Empty))
            {
                passwordVisibile.Text = _passwordWaterMark;
                passwordVisibile.GotFocus += Login_GotFocus;
            }
        }

        private void ShowPassword_Click(object sender, RoutedEventArgs e)
        {
            if (ShowPassword.IsChecked.Value)
            {
                TBPassword.Visibility = Visibility.Visible;
                PasswordVisible.Visibility = Visibility.Hidden;
                TBPassword.Password = PasswordVisible.Text;
            }
            else
            {
                PasswordVisible.Visibility = Visibility.Visible;
                TBPassword.Visibility = Visibility.Hidden;
                PasswordVisible.Text = TBPassword.Password;
            }
        }

        /* <-- Функционал MessageBox --> */
        private void Okey(object sender, RoutedEventArgs e) => this.GridMessageBox.Visibility = Visibility.Collapsed;

        public void SetMessageBox(string message, string title = "Ошибка")
        {
            this.GridMessageBox.Visibility = Visibility.Visible;
            this.TextMarker.Text = message;
            this.MessageTitle.Text = title;
        }
    }
}
