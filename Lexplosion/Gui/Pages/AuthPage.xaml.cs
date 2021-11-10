using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Management;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.Pages
{
    /// <summary>
    /// Interaction logic for AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {

        private string login = "";
        private string password = "";

        // Переменные для хранения значения водных знаков
        private const string Login_WaterMark = "Логин";
        private const string Password_WaterMark = "Пароль";
        private AuthWindow authWindow = null;
        public AuthPage(AuthWindow aw)
        {
            InitializeComponent();
            authWindow = aw;
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
            authWindow.ShowRegisterPage();
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

                var isChecked = SaveMe.IsChecked;
                Lexplosion.Run.ThreadRun(delegate () { 
                    AuthCode code = ManageLogic.Auth(login, password, isChecked is true);

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

        void ChangeWindow(sbyte status)
        {
            authWindow.ShowMainWindow();
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


        private void PasswordVisible_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox passwordVisibile = (TextBox)sender;
            if (passwordVisibile.Text.Trim().Equals(string.Empty))
            {
                passwordVisibile.Text = Password_WaterMark;
                passwordVisibile.GotFocus += Login_GotFocus;
            }
        }

        private void ShowPassword_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ShowPassword.IsChecked)
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
    }
}
