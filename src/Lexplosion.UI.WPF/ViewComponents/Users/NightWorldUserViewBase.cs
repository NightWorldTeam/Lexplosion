using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.UI.WPF.Tools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lexplosion.UI.WPF.ViewComponents.Users
{
    [TemplatePart(Name = PART_NICKNAME_TB, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_STATUS_TB, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_STATUS_INDICATOR, Type = typeof(Border))]
    [TemplatePart(Name = PART_BODYBORDER, Type = typeof(Border))]
    public abstract class NightWorldUserViewBase : UserControl
    {
        private const string PART_NICKNAME_TB = "PART_NicknameTB";
        private const string PART_STATUS_TB = "PART_StatusTB";
        private const string PART_STATUS_INDICATOR = "PART_StatusIndicator";
        private const string PART_BODYBORDER = "PABodyBorder";

        protected TextBlock NicknameTB;
        protected TextBlock StatusTB;
        protected Border StatusIndicator;
        protected Border BodyBorder;


        #region Dependency Properties


        public static readonly DependencyProperty NicknameProperty
            = DependencyProperty.Register(nameof(Nickname), typeof(string), typeof(NightWorldUserViewBase),
                new PropertyMetadata(defaultValue: "NW Player", propertyChangedCallback: OnUserNameChanged));

        public static readonly DependencyProperty StatusProperty
            = DependencyProperty.Register(nameof(Status), typeof(ActivityStatus), typeof(NightWorldUserViewBase),
                new PropertyMetadata(defaultValue: ActivityStatus.Offline, propertyChangedCallback: OnUserStatusChanged));

        public static readonly DependencyProperty RunningClientNameProperty
            = DependencyProperty.Register(nameof(RunningClientName), typeof(string), typeof(NightWorldUserViewBase),
                new PropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnRunningClientChanged));

        public static readonly DependencyProperty AvatarProperty
            = DependencyProperty.Register(nameof(Avatar), typeof(ImageSource), typeof(NightWorldUserViewBase),
                new PropertyMetadata(defaultValue: null, propertyChangedCallback: OnAvatarChanged));

        public static readonly DependencyProperty BannerProperty
            = DependencyProperty.Register(nameof(Banner), typeof(NwUserBanner), typeof(NightWorldUserViewBase),
                new PropertyMetadata(defaultValue: null, propertyChangedCallback: OnBannerUrlLoaded));

        public static readonly DependencyProperty ViewModelDataContextProperty
            = DependencyProperty.Register(nameof(ViewModelDataContext), typeof(object), typeof(NightWorldUserViewBase),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));


        #endregion Dependency Properties


        #region Properties


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

        public NwUserBanner Banner
        {
            get => (NwUserBanner)GetValue(BannerProperty);
            set => SetValue(BannerProperty, value);
        }


        public object ViewModelDataContext
        {
            get => (object)GetValue(ViewModelDataContextProperty);
            set => SetValue(ViewModelDataContextProperty, value);
        }


        #endregion Properties


        #region  Public & Protected Methods


        /*
         * Так как Template.FindName не выдает элементы по имени заменим обычными абстрактными гетерами.
         **/
        protected abstract Border GetBodyBorder();
        protected abstract TextBlock GetNicknameTB();
        protected abstract TextBlock GetStatusTB();
        protected abstract Border GetStatusIndicator();


        protected override void OnInitialized(EventArgs e)
        {
            BodyBorder = GetBodyBorder();
            NicknameTB = GetNicknameTB();
            StatusTB = GetStatusTB();
            StatusIndicator = GetStatusIndicator();

            //UpdateBanner();

            NicknameTB.Text = Nickname;
            StatusTB.SetResourceReference(TextBlock.TextProperty, Status.ToString());
            StatusIndicator.SetResourceReference(Border.BackgroundProperty, GetStatusColorKey(Status));

            base.OnInitialized(e);
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private static void OnRunningClientChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NightWorldUserViewBase _this)
            {
                if (_this.Status == ActivityStatus.InGame)
                {
                    _this.StatusTB.Text = string.Empty;

                    var status = new Run();
                    status.SetResourceReference(Run.TextProperty, "PlayingIn");
                    _this.StatusTB.Inlines.Add(status);
                    _this.StatusTB.Inlines.Add(" ");
                    _this.StatusTB.Inlines.Add(new Run(_this.RunningClientName));
                }
            }
        }


        private static void OnUserStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NightWorldUserViewBase _this)
            {
                if (_this.Status == ActivityStatus.InGame)
                {
                    _this.StatusTB.Text = string.Empty;

                    var status = new Run();
                    status.SetResourceReference(Run.TextProperty, "PlayingIn");
                    _this.StatusTB.Inlines.Add(status);
                    _this.StatusTB.Inlines.Add(" ");
                    _this.StatusTB.Inlines.Add(new Run(_this.RunningClientName));
                }
                else
                    _this.StatusTB.SetResourceReference(TextBlock.TextProperty, _this.Status.ToString());

                _this.StatusIndicator.SetResourceReference(Border.BackgroundProperty, GetStatusColorKey(_this.Status));
            }
        }

        private static void OnUserNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NightWorldUserViewBase _this)
            {
                _this.NicknameTB.Text = e.NewValue as string;
            }
        }

        private static void OnAvatarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d is NightWorldUserViewBase _this) 
            //{
            //    _this.AvatarBorder.Background = new ImageBrush() { ImageSource = f.Avatar };
            //}
        }

        private static string GetStatusColorKey(ActivityStatus status)
        {
            switch (status)
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


        protected virtual void UpdateBanner()
        {
            if (Banner == null)
            {
                BodyBorder.SetResourceReference(Border.BackgroundProperty, "FriendSolidColorBrush");
                NicknameTB.SetResourceReference(ForegroundProperty, "PrimaryForegroundSolidColorBrush");
                StatusIndicator.SetResourceReference(BorderBrushProperty, "SeparateSolidColorBrush");
                StatusTB.SetResourceReference(ForegroundProperty, "SecondaryForegroundSolidColorBrush");
                return;
            }

            if (!string.IsNullOrEmpty(Banner.Url))
            {
                var bitmap = new BitmapImage(uriSource: new Uri(Banner.Url))
                {
                    CacheOption = BitmapCacheOption.OnLoad
                };
                BodyBorder.Background = new ImageBrush(bitmap);
            }
            else
            {
                BodyBorder.SetResourceReference(Border.BackgroundProperty, "FriendSolidColorBrush");
            }

            if (Banner.Colors == null)
            {
                NicknameTB.SetResourceReference(ForegroundProperty, "PrimaryForegroundSolidColorBrush");
                StatusIndicator.SetResourceReference(BorderBrushProperty, "SeparateSolidColorBrush");
                StatusTB.SetResourceReference(ForegroundProperty, "SecondaryForegroundSolidColorBrush");
                return;
            }

            if (Banner.Colors.PrimaryForeColor != null && Banner.Colors.PrimaryForeColor > 0x01000000)
            {
                NicknameTB.Foreground = new SolidColorBrush(ColorTools.GetColor(Banner.Colors.PrimaryForeColor.Value));
            }
            else
            {
                NicknameTB.SetResourceReference(ForegroundProperty, "PrimaryForegroundSolidColorBrush");
            }

            if (Banner.Colors.SecondaryColor != null && Banner.Colors.SecondaryColor > 0x01000000)
            {
                StatusIndicator.BorderBrush = new SolidColorBrush(ColorTools.GetColor(Banner.Colors.SecondaryColor.Value));
            }
            else
            {
                StatusIndicator.SetResourceReference(BorderBrushProperty, "SeparateSolidColorBrush");
            }

            if (Banner.Colors.SecondaryForeColor != null && Banner.Colors.SecondaryForeColor > 0x01000000)
            {
                StatusTB.Foreground = new SolidColorBrush(ColorTools.GetColor(Banner.Colors.SecondaryForeColor.Value));
            }
            else
            {
                ;
                StatusTB.SetResourceReference(ForegroundProperty, "SecondaryForegroundSolidColorBrush");
            }
        }

        private static void OnBannerUrlLoaded(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NightWorldUserViewBase _this)
            {
                if (_this.BodyBorder != null)
                {
                    _this.UpdateBanner();
                }
            }
        }


        #endregion Private Methods
    }
}
