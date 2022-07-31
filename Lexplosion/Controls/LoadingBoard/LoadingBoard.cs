using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lexplosion.Controls
{
    public class LoadingBoard : ContentControl
    {
        #region Properties and Events

        public static readonly DependencyProperty IsLoadingFinishedProperty 
            = DependencyProperty.Register("IsLoadingFinished", typeof(bool), typeof(LoadingBoard), new PropertyMetadata(false));

        public static readonly DependencyProperty PlaceholderProperty 
            = DependencyProperty.Register("Placeholder", typeof(string), typeof(LoadingBoard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RectangeColorProperty 
            = DependencyProperty.Register("RectangeColor", typeof(Brush), typeof(LoadingBoard), new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty BorderColorProperty
            = DependencyProperty.Register("BorderColor", typeof(Brush), typeof(LoadingBoard), new PropertyMetadata(Brushes.White));

        public bool IsLoadingFinished
        {
            get => (bool)GetValue(IsLoadingFinishedProperty);
            set => SetValue(IsLoadingFinishedProperty, value);
        }

        public string Placeholder 
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public Brush RectangeColor
        {
            get => (Brush)GetValue(RectangeColorProperty);
            set => SetValue(RectangeColorProperty, value);
        }

        public Brush BorderColor
        {
            get => (Brush)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        #endregion

        static LoadingBoard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingBoard), new FrameworkPropertyMetadata(typeof(LoadingBoard)));
        }
    }
}
