using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.Controls
{
    [TemplatePart(Name = PART_DOWNLOAD_BUTTON, Type = typeof(Border))]
    [TemplatePart(Name = PART_LOADER, Type = typeof(Border))]
    [TemplatePart(Name = PART_PROGRESSBAR, Type = typeof(ProgressBar))]
    [TemplatePart(Name = PART_LOADER_TEXT, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_PLAY_BUTTON, Type = typeof(Border))]
    [TemplatePart(Name = PART_PLAY_BUTTON_TEXT, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_CLOSE_BUTTON, Type = typeof(Border))]
    public sealed class DownloadButton : Control
    {
        /* data */
        private const string PART_DOWNLOAD_BUTTON = "PART_Download_Button";
        private const string PART_LOADER = "PART_Loader";
        private const string PART_PROGRESSBAR = "PART_ProgressBar";
        private const string PART_LOADER_TEXT = "PART_Loader_Text";
        private const string PART_PLAY_BUTTON = "PART_Play_Button";
        private const string PART_PLAY_BUTTON_TEXT = "PART_Play_Button_Text";
        private const string PART_CLOSE_BUTTON = "PART_Close_Button";

        private Border _downloadButton;
        private Border _loader;
        private ProgressBar _progressBar;
        private Border _playButton;
        private Border _closeButton;

        private TextBlock _playButtonText;
        private TextBlock _loaderText;
        /* data */

        // stage
        // 1) download
        // 2) loading + progressbar
        // 3) done (play button)
        // 4) close game 

        #region Dependency Properities 


        public static readonly DependencyProperty InstanceFormVMProperty
            = DependencyProperty.Register(
                "InstanceFormVM",
                typeof(InstanceFormViewModel),
                typeof(DownloadButton),
                new FrameworkPropertyMetadata(null, OnInstanceFormChanged)
                );

        /* background */

        public static readonly DependencyProperty DownloadButtonBackgroundProperty
            = DependencyProperty.Register("DownloadButtonBackground", typeof(Brush), typeof(DownloadButton), new FrameworkPropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty LoaderBackgroundProperty
            = DependencyProperty.Register("LoaderBackground", typeof(Brush), typeof(DownloadButton), new FrameworkPropertyMetadata(new BrushConverter().ConvertFromString("#099176")));

        public static readonly DependencyProperty PlayButtonBackgroundProperty
            = DependencyProperty.Register("PlayButtonBackground", typeof(Brush), typeof(DownloadButton), new FrameworkPropertyMetadata(new BrushConverter().ConvertFromString("#5972d9")));

        public static readonly DependencyProperty CloseButtonBackgroundProperty
            = DependencyProperty.Register("CloseButtonBackground", typeof(Brush), typeof(DownloadButton), new FrameworkPropertyMetadata(Brushes.Black));

        /* foreground */

        public static readonly DependencyProperty DownloadButtonForegroundProperty
            = DependencyProperty.Register("DownloadButtonForeground", typeof(Brush), typeof(DownloadButton), new FrameworkPropertyMetadata(Brushes.White));

        public static readonly DependencyProperty LoaderForegroundProperty
            = DependencyProperty.Register("LoaderForeground", typeof(Brush), typeof(DownloadButton), new FrameworkPropertyMetadata(Brushes.White));

        public static readonly DependencyProperty PlayButtonForegroundProperty
            = DependencyProperty.Register("PlayButtonForeground", typeof(Brush), typeof(DownloadButton), new FrameworkPropertyMetadata(Brushes.White));

        public static readonly DependencyProperty CloseButtonForegroundProperty
            = DependencyProperty.Register("CloseButtonForeground", typeof(Brush), typeof(DownloadButton), new FrameworkPropertyMetadata(Brushes.White));

        /* font size */

        public static readonly DependencyProperty DownloadButtonFontSizeProperty
            = DependencyProperty.Register("DownloadButtonFontSize", typeof(double), typeof(DownloadButton), new UIPropertyMetadata(12d));

        public static readonly DependencyProperty LoaderFontSizeProperty
            = DependencyProperty.Register("LoaderFontSize", typeof(double), typeof(DownloadButton), new UIPropertyMetadata(12d));

        public static readonly DependencyProperty PlayButtonFontSizeProperty
            = DependencyProperty.Register("PlayButtonFontSize", typeof(double), typeof(DownloadButton), new UIPropertyMetadata(12d));

        public static readonly DependencyProperty CloseButtonFontSizeProperty
            = DependencyProperty.Register("CloseButtonFontSize", typeof(double), typeof(DownloadButton), new UIPropertyMetadata(12d));

        /* font weight */

        public static readonly DependencyProperty DownloadButtonFontWeightProperty
            = DependencyProperty.Register("DownloadButtonFontWeight", typeof(FontWeight), typeof(DownloadButton), new UIPropertyMetadata(FontWeights.Bold));

        public static readonly DependencyProperty LoaderFontWeightProperty
            = DependencyProperty.Register("LoaderFontWeight", typeof(FontWeight), typeof(DownloadButton), new UIPropertyMetadata(FontWeights.Bold));

        public static readonly DependencyProperty PlayButtonFontWeightProperty
            = DependencyProperty.Register("PlayButtonFontWeight", typeof(FontWeight), typeof(DownloadButton), new UIPropertyMetadata(FontWeights.Bold));

        public static readonly DependencyProperty CloseButtonFontWeightProperty
            = DependencyProperty.Register("CloseButtonFontWeight", typeof(FontWeight), typeof(DownloadButton), new UIPropertyMetadata(FontWeights.Bold));

        /* progressbar */

        public static readonly DependencyProperty BackgroundProgressBarProperty
            = DependencyProperty.Register("BackgroundProgressBar", typeof(Brush), typeof(DownloadButton), new UIPropertyMetadata(new BrushConverter().ConvertFromString("#099176")));

        public static readonly DependencyProperty ForegroundProgressBarProperty
            = DependencyProperty.Register("ForegroundProgressBar", typeof(Brush), typeof(DownloadButton), new UIPropertyMetadata(new BrushConverter().ConvertFromString("#24e0bb")));

        public InstanceFormViewModel InstanceFormVM
        {
            get => (InstanceFormViewModel)GetValue(InstanceFormVMProperty);
            set => SetValue(InstanceFormVMProperty, value);
        }


        #region background

        public Brush DownloadButtonBackground
        {
            get => (Brush)GetValue(DownloadButtonBackgroundProperty);
            set => SetValue(DownloadButtonBackgroundProperty, value);
        }

        public Brush LoaderBackground
        {
            get => (Brush)GetValue(LoaderBackgroundProperty);
            set => SetValue(LoaderBackgroundProperty, value);
        }

        public Brush PlayButtonBackground
        {
            get => (Brush)GetValue(PlayButtonBackgroundProperty);
            set => SetValue(PlayButtonBackgroundProperty, value);
        }

        public Brush CloseButtonBackground
        {
            get => (Brush)GetValue(CloseButtonBackgroundProperty);
            set => SetValue(CloseButtonBackgroundProperty, value);
        }

        #endregion background

        #region foreground

        public Brush DownloadButtonForeground
        {
            get => (Brush)GetValue(DownloadButtonForegroundProperty);
            set => SetValue(DownloadButtonForegroundProperty, value);
        }

        public Brush LoaderForeground
        {
            get => (Brush)GetValue(LoaderForegroundProperty);
            set => SetValue(LoaderForegroundProperty, value);
        }

        public Brush PlayButtonForeground
        {
            get => (Brush)GetValue(PlayButtonForegroundProperty);
            set => SetValue(PlayButtonForegroundProperty, value);
        }
        public Brush CloseButtonForeground
        {
            get => (Brush)GetValue(CloseButtonForegroundProperty);
            set => SetValue(CloseButtonForegroundProperty, value);
        }

        #endregion foreground

        #region fontsize

        public double DownloadButtonFontSize
        {
            get => (double)GetValue(DownloadButtonFontSizeProperty);
            set => SetValue(DownloadButtonFontSizeProperty, value);
        }

        public double LoaderFontSize
        {
            get => (double)GetValue(LoaderFontSizeProperty);
            set => SetValue(LoaderFontSizeProperty, value);
        }

        public double PlayButtonFontSize
        {
            get => (double)GetValue(PlayButtonFontSizeProperty);
            set => SetValue(PlayButtonFontSizeProperty, value);
        }
        public double CloseButtonFontSize
        {
            get => (double)GetValue(CloseButtonFontSizeProperty);
            set => SetValue(CloseButtonFontSizeProperty, value);
        }

        #endregion fontsize

        #region fontweight

        public FontWeight DownloadButtonFontWeight
        {
            get => (FontWeight)GetValue(DownloadButtonFontWeightProperty);
            set => SetValue(DownloadButtonFontWeightProperty, value);
        }

        public FontWeight LoaderFontWeight
        {
            get => (FontWeight)GetValue(LoaderFontWeightProperty);
            set => SetValue(LoaderFontWeightProperty, value);
        }

        public FontWeight PlayButtonFontWeight
        {
            get => (FontWeight)GetValue(PlayButtonFontWeightProperty);
            set => SetValue(PlayButtonFontWeightProperty, value);
        }
        public FontWeight CloseButtonFontWeight
        {
            get => (FontWeight)GetValue(CloseButtonFontWeightProperty);
            set => SetValue(CloseButtonFontWeightProperty, value);
        }

        #endregion fontweight

        #region progressbar

        public Brush BackgroundProgressBar
        {
            get => (Brush)GetValue(BackgroundProgressBarProperty);
            set => SetValue(BackgroundProgressBarProperty, value);
        }

        public Brush ForegroundProgressBar
        {
            get => (Brush)GetValue(ForegroundProgressBarProperty);
            set => SetValue(ForegroundProgressBarProperty, value);
        }

        #endregion progressbar

        #endregion dependency properities



        #region Constructors


        static DownloadButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DownloadButton), new FrameworkPropertyMetadata(typeof(DownloadButton)));
        }


        #endregion Constructors

        // TODO WARNING HERE NEEDS IMPORTANT FIX.
        // When one instance ran, all kind of this button has the same state.
        public override void OnApplyTemplate()
        {
            _downloadButton = Template.FindName(PART_DOWNLOAD_BUTTON, this) as Border;
            _loader = Template.FindName(PART_LOADER, this) as Border;
            _playButton = Template.FindName(PART_PLAY_BUTTON, this) as Border;
            _closeButton = Template.FindName(PART_CLOSE_BUTTON, this) as Border;

            if (_downloadButton != null)
            {
                _downloadButton.MouseDown += OnDownloadButtonClicked;
            }
            if (_loader != null)
            {
                _progressBar = GetTemplateChild(PART_PROGRESSBAR) as ProgressBar;
            }
            if (_playButton != null)
            {
                _playButton.MouseDown += OnPlayButtonClicked;
            };
            if (_closeButton != null)
            {
                _closeButton.MouseDown += OnCloseButtonClicked;
            }

            base.OnApplyTemplate();
        }


        #region EventHandlers


        private void OnCloseButtonClicked(object sender, MouseButtonEventArgs e)
        {
            InstanceFormVM.CloseInstance();
            PreviewButtonAnimation(_playButton);
        }

        private void OnPlayButtonClicked(object sender, MouseButtonEventArgs e)
        {
            ChangePlayButtonText(ResourceGetter.GetString("prepareRun"));
            ComplitedLaunchCallback complitedLaunch = delegate (string instanceId, bool successful)
            {
                // тут хз что
                // можно написать при запуске, до полного запуска, что-то типо запускается.
                App.Current.Dispatcher.Invoke(() =>
            {
                ChangePlayButtonText(ResourceGetter.GetString("play"));
                NextButtonAnimation(_playButton);
            });
            };

            GameExitedCallback gameExitedCallback = delegate (string instanceId)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    PreviewButtonAnimation(_playButton);
                });
            };

            InstanceFormVM.LaunchInstance(complitedLaunch, gameExitedCallback);
        }

        private void OnDownloadButtonClicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            // тут запуск скачивания.

            // ProgressHandler
            Download();

            NextButtonAnimation(button);
        }


        #endregion EventHandlers


        #region Private Methods

        /// <summary>
        /// Анимация. Смена одной кнопки на другую.
        /// </summary>
        /// <param name="control">кнопка</param>
        /// <param name="animationComplitedEvent">Эвент который выполниться после анимации.</param>
        private void NextButtonAnimation(FrameworkElement control, EventHandler animationComplitedEvent = null)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = control.ActualHeight,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            if (animationComplitedEvent != null) animation.Completed += animationComplitedEvent;

            control.BeginAnimation(FrameworkElement.HeightProperty, animation);
        }

        /// <summary>
        /// Анимация. Смена на предыдущую кнопку.
        /// </summary>
        /// <param name="control">Кнопка</param>
        /// <param name="animationComplitedEvent">Эвент выполняющийся после анимации.</param>
        private void PreviewButtonAnimation(FrameworkElement control, EventHandler animationComplitedEvent = null)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0.0,
                To = this.ActualHeight,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            if (animationComplitedEvent != null) animation.Completed += animationComplitedEvent;

            control.BeginAnimation(FrameworkElement.HeightProperty, animation);
        }

        /// <summary>
        /// Моментальное скрытие кнопки. Без анимации
        /// </summary>
        /// <param name="control">Кнопка</param>
        private void HideButton(FrameworkElement control)
        {
            control.Height = 0.0;
        }

        /// <summary>
        /// Моментальный показ кнопки. Без анимации.
        /// </summary>
        /// <param name="control">Кнопка</param>
        private void ShowButton(FrameworkElement control)
        {
            control.Height = this.ActualHeight;
        }


        private void Download()
        {
            Action<DownloadStageTypes, ProgressHandlerArguments> progressHandlerMethod = delegate (DownloadStageTypes stageType, ProgressHandlerArguments progressHandlerArguments)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    _progressBar.Value = progressHandlerArguments.Procents;
                });
            };

            Action<InstanceInit, List<string>, bool> complitedDownloadMethod = delegate (InstanceInit result, List<string> downloadErrors, bool launchGame)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    NextButtonAnimation(_loader);
                });
            };

            InstanceFormVM.DownloadInstance(progressHandlerMethod, complitedDownloadMethod);
        }


        private void ChangePlayButtonText(string text)
        {
            if (this._playButtonText == null)
            {
                this._playButtonText = GetTemplateChild(PART_PLAY_BUTTON_TEXT) as TextBlock;
            }
            this._playButtonText.Text = text;
        }

        private void ChangeLoaderText(string text)
        {
            if (this._loaderText == null)
            {
                this._loaderText = GetTemplateChild(PART_LOADER_TEXT) as TextBlock;
            }
            this._loaderText.Text = text;
        }


        #endregion Private Methods


        #region On DependencyProperties Changed


        private static void OnInstanceFormChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || e.OldValue == e.NewValue)
                return;

            var button = (DownloadButton)d;
            var newValue = (InstanceFormViewModel)e.NewValue;

            if (newValue.Client.IsInstalled && !newValue.Model.LaunchModel.IsGameLaunched)
            {
                button.HideButton(button._downloadButton);
                button.HideButton(button._loader);
            }
            else if (newValue.Model.LaunchModel.IsGameLaunched)
            {
                button.HideButton(button._downloadButton);
                button.HideButton(button._loader);
                button.HideButton(button._playButton);
            }
            else if (newValue.Model.DownloadModel.IsDownloadInProgress)
            {
                button.HideButton(button._downloadButton);
                button.Download();
            }
            else
            {
                button.ShowButton(button._downloadButton);
                button.ShowButton(button._loader);
            }
        }


        #endregion On DependencyProperties Changed
    }
}