using Lexplosion.Gui.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.Gui
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        private const string Login_WaterMark = "Логин";
        private const string Password_WaterMark = "Пароль";
        private const string ConfirmPassword_WaterMark = "Повтор Пароля";
        private const string Email_WaterMark = "Электронная почта";

        private AuthWindow authWindow = null;

        public RegisterPage(AuthWindow aw)
        {
            InitializeComponent();
            authWindow = aw;
            if (TBLogin.Text == string.Empty && TBPassword.Password == string.Empty && TBConfirmPassword.Password == string.Empty && TBEmail.Text == string.Empty)
            {
                TBLogin.Text = Login_WaterMark;
                PasswordBoxWaterMark.Text = Password_WaterMark;
                ConfirmPasswordBoxWaterMark.Text = ConfirmPassword_WaterMark;
                TBEmail.Text = Email_WaterMark;
            }
        }


        private void ToLoginForm(object sender, RoutedEventArgs e)
        {
            authWindow.ShowAuthPage();
        }

        public bool IsValidEmailAddress(string email)
        {
            return Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        }

        private void RegisterAccount(object sender, RoutedEventArgs e)
        {
            if (TBLogin.Text != null && TBPassword.Password != null && TBConfirmPassword.Password != null && TBEmail.Text != null)
            {
                if (TBPassword.Password == TBConfirmPassword.Password)
                {
                    if (IsValidEmailAddress(TBEmail.Text))
                    {
                        SetMessageBox("Work");
                    }
                    else
                    {
                        SetMessageBox(TBEmail.Text + ": не является email");
                    }
                }
                else
                {
                    SetMessageBox("Пароли не совпадают!");
                }
            }
        }

        private void Login_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == Login_WaterMark)
            {
                textbox.Text = string.Empty;
                textbox.GotFocus -= Login_GotFocus;
            }
        }

        private void ConfirmPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TBPassword.Password == string.Empty)
            {
                TBPassword.GotFocus -= Password_GotFocus;
                PasswordBoxWaterMark.Visibility = Visibility.Collapsed;
            }
        }
        private void ConfirmPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TBConfirmPassword.Password == string.Empty)
            {
                TBConfirmPassword.GotFocus -= ConfirmPassword_GotFocus;
                ConfirmPasswordBoxWaterMark.Visibility = Visibility.Collapsed;
            }
        }

        private void Email_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == Email_WaterMark)
            {
                textbox.Text = string.Empty;
                textbox.GotFocus -= Email_GotFocus;
            }
        }

        // WaterMark Function --> LostFocus //

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
            if (TBPassword.Password.Trim().Equals(string.Empty))
            {
                PasswordBoxWaterMark.Text = Password_WaterMark;
                PasswordBoxWaterMark.Visibility = Visibility.Visible;
                TBPassword.GotFocus += Password_GotFocus;
            }
        }
        private void ConfirmPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TBConfirmPassword.Password.Trim().Equals(string.Empty))
            {
                ConfirmPasswordBoxWaterMark.Text = ConfirmPassword_WaterMark;
                ConfirmPasswordBoxWaterMark.Visibility = Visibility.Visible;
                TBConfirmPassword.GotFocus += ConfirmPassword_GotFocus;
            }
        }
        private void Email_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text.Trim().Equals(string.Empty))
            {
                textbox.Text = Email_WaterMark;
                textbox.GotFocus += Email_GotFocus;
            }
        }


        /* <-- Custom MessageBox --> */
        private void Okay(object sender, RoutedEventArgs e)
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
