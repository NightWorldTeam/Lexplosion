using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для Friend.xaml
    /// </summary>
    public partial class Friend : UserControl
    {
        public Friend()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty NicknameProperty
            = DependencyProperty.Register(nameof(Nickname), typeof(string), typeof(Friend), new PropertyMetadata(defaultValue: "NW Player", propertyChangedCallback: OnUserNameChanged));

        private static void OnUserNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Friend) 
            {
                var f = (Friend)d;
                f.NicknameTB.Text = e.NewValue as string;
            }
        }

        public static readonly DependencyProperty StatusProperty
            = DependencyProperty.Register(nameof(Status), typeof(string), typeof(Friend), new PropertyMetadata(defaultValue: "Offline", propertyChangedCallback: OnUserStatusChanged));

        private static void OnUserStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Friend)
            {
                var f = (Friend)d;
                f.StatusTB.Text = e.NewValue as string;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            NicknameTB.Text = Nickname;
            StatusTB.Text = Status;
            base.OnInitialized(e);
        }

        public string Nickname
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }
    }
}
