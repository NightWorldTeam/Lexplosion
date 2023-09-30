using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class Loader : ContentControl
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(Loader), new FrameworkPropertyMetadata(false));


        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }


        #region Contructors


        static Loader ()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Loader), new FrameworkPropertyMetadata(typeof(Loader)));
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }


        #endregion Public & Protected Methods


        #region Private Methods




        #endregion Private Methods
    }
}
