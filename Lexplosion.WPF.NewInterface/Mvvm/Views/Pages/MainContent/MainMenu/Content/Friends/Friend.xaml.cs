using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для Friend.xaml
    /// </summary>
    public partial class Friend : UserControl
    {
        static Friend() 
        {
        }

        public Friend()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty NicknameProperty
            = DependencyProperty.Register(nameof(Nickname), typeof(string), typeof(Friend), 
                new PropertyMetadata(defaultValue: "NW Player", propertyChangedCallback: OnUserNameChanged));

        public static readonly DependencyProperty StatusProperty
            = DependencyProperty.Register(nameof(Status), typeof(ActivityStatus), typeof(Friend), 
                new PropertyMetadata(defaultValue: ActivityStatus.Offline, propertyChangedCallback: OnUserStatusChanged));

        public static readonly DependencyProperty RunningClientNameProperty
            = DependencyProperty.Register(nameof(RunningClientName), typeof(string), typeof(Friend),
                new PropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnRunningClientChanged));

        public static readonly DependencyProperty AvatarProperty
            = DependencyProperty.Register(nameof(Avatar), typeof(ImageSource), typeof(Friend),
                new PropertyMetadata(defaultValue: null, propertyChangedCallback: OnAvatarChanged));

        public string Nickname
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public ActivityStatus Status
        {
            get => (ActivityStatus)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public ImageSource Avatar
        {
            get => (ImageSource)GetValue(AvatarProperty);
            set => SetValue(AvatarProperty, value);
        }

        public string RunningClientName 
        {
            get => (string)GetValue(RunningClientNameProperty);
            set => SetValue(RunningClientNameProperty, value);
        }

        protected override void OnInitialized(EventArgs e)
        {
            NicknameTB.Text = Nickname;
            StatusTB.SetResourceReference(TextBlock.TextProperty, Status.ToString());
            StatusIndicator.SetResourceReference(Border.BackgroundProperty, GetStatusColorKey(Status));

            base.OnInitialized(e);
        }


        #region Private Methods


        private static void OnRunningClientChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Friend)
            {
                var f = (Friend)d;

                if (f.Status == ActivityStatus.InGame)
                {
                    f.StatusTB.Text = string.Empty;

                    var status = new Run();
                    status.SetResourceReference(Run.TextProperty, "PlayingIn");
                    f.StatusTB.Inlines.Add(status);
                    f.StatusTB.Inlines.Add(" ");
                    f.StatusTB.Inlines.Add(new Run(f.RunningClientName));
                }
            }
        }


        private static void OnUserStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Friend)
            {
                var f = (Friend)d;

                if (f.Status == ActivityStatus.InGame) 
                {
                    f.StatusTB.Text = string.Empty;

                    var status = new Run(); 
                    status.SetResourceReference(Run.TextProperty, "PlayingIn");
                    f.StatusTB.Inlines.Add(status);
                    f.StatusTB.Inlines.Add(" ");
                    f.StatusTB.Inlines.Add(new Run(f.RunningClientName));
                }
                else
                    f.StatusTB.SetResourceReference(TextBlock.TextProperty, f.Status.ToString());

                f.StatusIndicator.SetResourceReference(Border.BackgroundProperty, GetStatusColorKey(f.Status));
            }
        }

        private static void OnUserNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Friend)
            {
                var f = (Friend)d;
                f.NicknameTB.Text = e.NewValue as string;
            }
        }

        private static void OnAvatarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d is Friend) 
            //{
            //    var f = (Friend)d;
            //    f.AvatarBorder.Background = new ImageBrush() { ImageSource = f.Avatar };
            //}
        }

        private static string GetStatusColorKey(ActivityStatus status) 
        {
            switch(status) 
            {
                case ActivityStatus.InGame:
                    return "InGameSolidColorBrush";
                case ActivityStatus.Online:
                    return "OnlineSolidColorBrush";
                case ActivityStatus.NotDisturb:
                    return "DoNotDisturbSolidColorBrush";
                case ActivityStatus.Offline:
                    return "OfflineSolidColorBrush";
                default:
                    return string.Empty;
            }
        }

        //private static string Stat

        #endregion Private Methods
    }
}
