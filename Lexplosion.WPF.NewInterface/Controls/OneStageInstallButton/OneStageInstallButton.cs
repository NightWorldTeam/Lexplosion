using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class OneStageInstallButton : Button
    {
        private const string PART_PROGRESS_BAR = "PART_ProgressBar";
        private const string PART_PERCENTAGES_TEXTBLOCK = "PART_Percentages";
        private const string PART_CONTENT = "PART_Content";

        private ProgressBar _progressBar;
        private TextBlock _persentageTextBlock;
        private ContentPresenter _content;


        private bool _isInstalling;


        #region Properties


        public static readonly DependencyProperty BeforeInstallContentProperty
            = DependencyProperty.Register(nameof(BeforeInstallContent), typeof(object), typeof(OneStageInstallButton),
                new FrameworkPropertyMetadata(null, propertyChangedCallback: OnContentDependencyPropertyChanged));
        
        public static readonly DependencyProperty AfterInstallContentProperty
            = DependencyProperty.Register(nameof(AfterInstallContent), typeof(object), typeof(OneStageInstallButton),
                new FrameworkPropertyMetadata(null, propertyChangedCallback: OnContentDependencyPropertyChanged));

        public static readonly DependencyProperty ProgressValueProperty
            = DependencyProperty.Register(nameof(ProgressValue), typeof(double), typeof(OneStageInstallButton),
                new PropertyMetadata(defaultValue: 0d, propertyChangedCallback: OnProgressValueChanged));

        public static readonly DependencyProperty IsInstalledProperty = 
            DependencyProperty.RegisterAttached(nameof(IsInstalled), typeof(bool), typeof(OneStageInstallButton),
                new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnIsInstalledChanged));
        
        public object BeforeInstallContent 
        {
            get => (object)GetValue(BeforeInstallContentProperty);
            set => SetValue(BeforeInstallContentProperty, value);
        }

        public object AfterInstallContent
        {
            get => (object)GetValue(AfterInstallContentProperty);
            set => SetValue(AfterInstallContentProperty, value);
        }


        public bool IsInstalled 
        {
            get => (bool)GetValue(IsInstalledProperty);
            set => SetValue(IsInstalledProperty, value);
        }

        public double ProgressValue 
        {
            get => (double)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }


        #endregion Properties



        #region Constructors


        static OneStageInstallButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OneStageInstallButton), new FrameworkPropertyMetadata(typeof(OneStageInstallButton)));
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _progressBar = GetTemplateChild(PART_PROGRESS_BAR) as ProgressBar;
            _persentageTextBlock = GetTemplateChild(PART_PERCENTAGES_TEXTBLOCK) as TextBlock;
            _content = GetTemplateChild(PART_CONTENT) as ContentPresenter;
           
            if (_progressBar == null || _persentageTextBlock == null || _content == null)
            {
                throw new NullReferenceException("Template parts not available");
            }

            _persentageTextBlock.Visibility = Visibility.Collapsed;
            _progressBar.Visibility = Visibility.Collapsed;


            _progressBar.Value = ProgressValue;
            _persentageTextBlock.Text = $"{ProgressValue}%";
        }




        protected override void OnClick()
        {
            if (IsInstalled || _isInstalling)
                return;

            _content.Visibility = Visibility.Hidden;

            _progressBar.Visibility = Visibility.Visible;
            _persentageTextBlock.Visibility = Visibility.Visible;

            base.OnClick();
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private static void OnIsInstalledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OneStageInstallButton _this)
            {
                if (_this._progressBar == null)
                    return;

                _this._progressBar.Visibility = Visibility.Collapsed;
                _this._persentageTextBlock.Visibility = Visibility.Collapsed;

                _this._content.Visibility = Visibility.Visible;
                _this.UpdateContent();
                _this.Cursor = Cursors.No;
            }
        }

        private static void OnContentDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OneStageInstallButton _this)
            {
                _this.UpdateContent();
            }
        }

        private void UpdateContent()
        {
            if (IsInstalled)
            {
                Content = AfterInstallContent;
                Cursor = null;
            }
            else
            {
                Content = BeforeInstallContent;
                Cursor = Cursors.Hand;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            _progressBar.MaxWidth = sizeInfo.NewSize.Width;

            base.OnRenderSizeChanged(sizeInfo);
        }

        private static void OnProgressValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OneStageInstallButton _this)
            {
                if (_this._progressBar == null)
                    return;
                var newValue = (double)e.NewValue;
                _this._progressBar.Value = newValue;
                _this._persentageTextBlock.Text = $"{newValue}%";
            }
        }


        #endregion Private Methods
    }
}
