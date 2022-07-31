using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Controls
{
    public class LoadingBoard : ContentControl
    {
        #region Properties and Events

        public static readonly DependencyProperty IsLoadingFinishedProperty =
            DependencyProperty.Register("IsLoadingFinished", typeof(bool), typeof(LoadingBoard), new PropertyMetadata(false));

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(LoadingBoard), new PropertyMetadata(string.Empty));

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

        #endregion

        static LoadingBoard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingBoard), new FrameworkPropertyMetadata(typeof(LoadingBoard)));
        }
    }
}
