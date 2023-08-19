using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;

namespace Lexplosion.WPF.NewInterface.Controls
{
    [TemplatePart(Name = PART_BACKGROUND_LAYER, Type = typeof(Border))]
    [TemplatePart(Name = PART_CONTENT, Type = typeof(ContentPresenter))]
    public class ModalControl : ContentControl
    {
        private const string PART_BACKGROUND_LAYER = "PART_BackgroundLayer";
        private const string PART_CONTENT = "PART_Content";

        private Border _partBackgroundLayer;
        private ContentPresenter _partContent;


        #region Properties


        public static readonly DependencyProperty IsOpenProperty
            = DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(ModalControl),
                new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnIsOpenChanged));

        public static readonly DependencyProperty DimmingOpacityProperty
            = DependencyProperty.Register(nameof(DimmingOpacity), typeof(double), typeof(ModalControl),
                new FrameworkPropertyMetadata(defaultValue: 0.7));

        public static readonly DependencyProperty IsAnimationEnableProperty
            = DependencyProperty.Register(nameof(IsAnimationEnable), typeof(bool), typeof(ModalControl),
                new PropertyMetadata(defaultValue: true));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public double DimmingOpacity
        {
            get => (double)GetValue(DimmingOpacityProperty);
            set => SetValue(DimmingOpacityProperty, value);
        }

        public bool IsAnimationEnable
        {
            get => (bool)GetValue(IsAnimationEnableProperty);
            set => SetValue(IsAnimationEnableProperty, value);
        }


        #endregion Properties


        #region Constructors


        static ModalControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModalControl), new FrameworkPropertyMetadata(typeof(ModalControl)));
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _partBackgroundLayer = GetTemplateChild(PART_BACKGROUND_LAYER) as Border;
            _partContent = GetTemplateChild(PART_CONTENT) as ContentPresenter;

            _partBackgroundLayer.Visibility = Visibility.Collapsed;

            if (_partBackgroundLayer == null || _partContent == null)
            {
                throw new NullReferenceException("Template parts not available");
            }

            if (IsOpen) 
            {
                ShowModalAnimation();
            }
        }


        #endregion Public & Protected Methods


        #region Private Methods


        public void Close()
        {
            _partBackgroundLayer.Visibility = Visibility.Collapsed;
        }

        public void Open()
        {
            _partBackgroundLayer.Visibility = Visibility.Visible;
        }

        private void ShowModalAnimation()
        {
            if (_partBackgroundLayer == null)
                return;

            _partBackgroundLayer.Opacity = 0.0;
            _partBackgroundLayer.Visibility = Visibility.Visible;

            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.35)
            };
            _partBackgroundLayer.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
        }

        private void CloseModalAnimation()
        {
            if (_partBackgroundLayer == null)
                return;

            _partBackgroundLayer.Visibility = Visibility.Visible;
            var doubleAnimation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.35)
            };

            doubleAnimation.Completed += (object sender, EventArgs e) =>
            {
                _partBackgroundLayer.Visibility = Visibility.Collapsed;
            };

            _partBackgroundLayer.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dObj = (ModalControl)d;

            var newValue = (bool)e.NewValue;
            if (dObj.IsAnimationEnable)
            {
                if (newValue)
                {
                    dObj.ShowModalAnimation();
                }
                else
                {
                    dObj.CloseModalAnimation();
                }
            }
            else
            {
                if (newValue)
                {
                    dObj.Open();
                }
                else
                {
                    dObj.Close();
                }
            }
        }


        #endregion Private Methods
    }
}
