using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class LaunchButton : Button
    {
        public static readonly DependencyProperty IsLoadedProperty
            = DependencyProperty.Register(nameof(IsLoaded), typeof(bool), typeof(LaunchButton),
                new FrameworkPropertyMetadata(defaultValue: false, propertyChangedCallback: OnIsLoadedChanged));

        public static readonly DependencyProperty IsLoadingProperty
            = DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LaunchButton),
                new FrameworkPropertyMetadata(defaultValue: false, propertyChangedCallback: OnIsLoadingChanged));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            protected set { SetValue(IsLoadingProperty, value); }
        }

        public bool IsLoaded
        {
            get { return (bool)GetValue(IsLoadedProperty); }
            set { SetValue(IsLoadedProperty, value); }
        }


        protected override void OnClick()
        {
            if (!IsLoading)
            {
                IsLoading = true;
            }
            else
            {
                IsLoading = false;
            }

            base.OnClick();
        }


        #region Private Methods


        private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LaunchButton _this)
            {
                var newValue = (bool)e.NewValue;
                if (newValue) 
                {
                    _this.IsLoading = newValue;
                    _this.IsLoaded = false;
                }
            }
        }

        private static void OnIsLoadedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LaunchButton _this)
            {
                var newValue = (bool)e.NewValue;
                if (_this.IsLoading && newValue)
                {
                    _this.IsLoaded = newValue;
                    _this.IsLoading = false;
                }
            }
        }


        #endregion Private Methods
    }
}
