using Lexplosion.Gui.ViewModels;
using Lexplosion.Gui.Views.CustomControls;
using Lexplosion.Logic.Management;
using LumiSoft.Net.Mime.vCard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Lexplosion.Controls
{
    [TemplatePart(Name = PART_DOWNLOAD_BUTTON, Type = typeof(Border))]
    [TemplatePart(Name = PART_LOADER, Type = typeof(Border))]
    [TemplatePart(Name = PART_PROGRESSBAR, Type = typeof(ProgressBar))]
    [TemplatePart(Name = PART_PLAY_BUTTON, Type = typeof(Border))]
    [TemplatePart(Name = PART_CLOSE_BUTTON, Type = typeof(Border))]
    public class DownloadButton : Control
    {
        /* data */
        private const string PART_DOWNLOAD_BUTTON = "PART_Download_Button";
        private const string PART_LOADER = "PART_Loader";
        private const string PART_PROGRESSBAR = "PART_ProgressBar";
        private const string PART_PLAY_BUTTON = "PART_Play_Button";
        private const string PART_CLOSE_BUTTON = "PART_Close_Button";

        private Border _downloadButton;
        private Border _loader;
        private ProgressBar _progressBar;
        private Border _playButton;
        private Border _closeButton;
        /* data */

        // stage
        // 1) download
        // 2) loading + progressbar
        // 3) done (play button)
        // 4) close game 

        private bool IsRunnedDownload = false;
        private bool IsRunnedGame = false;

        #region dependency properities 


        public static readonly DependencyProperty InstanceFormVMProperty
            = DependencyProperty.Register("InstanceFormVM", typeof(InstanceFormViewModel), typeof(DownloadButton), new FrameworkPropertyMetadata());


        public InstanceFormViewModel InstanceForm 
        {
            get => (InstanceFormViewModel)GetValue(InstanceFormVMProperty);
            set => SetValue(InstanceFormVMProperty, value);
        }

        #endregion dependency properities


        static DownloadButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DownloadButton), new FrameworkPropertyMetadata(typeof(DownloadButton)));
        }

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

        private void OnCloseButtonClicked(object sender, MouseButtonEventArgs e)
        {
            InstanceForm.CloseInstance();
        }

        private void OnPlayButtonClicked(object sender, MouseButtonEventArgs e)
        {
            if (!IsRunnedGame) 
            {
                ComplitedLaunchCallback complitedLaunch = delegate (string instanceId, bool successful)
                {
                    // тут хз что
                    // можно написать при запуске, до полного запуска, что-то типо запускается.
                };

                GameExitedCallback gameExitedCallback = delegate (string instanceId)
                {
                    PreviewButtonAnimation(_playButton);
                };
                InstanceForm.LaunchInstance();
            }
            NextButtonAnimation(_playButton);
        }

        private void OnDownloadButtonClicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            _progressBar = GetTemplateChild(PART_PROGRESSBAR) as ProgressBar;
            //MessageBox.Show("Donwload Button Clicked");
            // тут запуск скачивания.

            if (!IsRunnedDownload)
            {
                ProgressHandlerCallback progressHandlerMethod = delegate (DownloadStageTypes stageType, int stagesCount, int stage, int procents)
                {
                    _progressBar.Value = procents;
                };
                ComplitedDownloadCallback complitedDownloadMethod = delegate (InstanceInit result, List<string> downloadErrors, bool launchGame)
                {
                    NextButtonAnimation(_loader);
                };

                InstanceForm.DownloadInstance(progressHandlerMethod, complitedDownloadMethod);
            }
            else InstanceForm.DownloadInstance();


            NextButtonAnimation(button);
        }

        /// Для тестов loader.
        //private void OnLoadingStarted(object sender, EventArgs e)
        //{
        //    var thread = new Thread(() =>
        //    {
        //        if (_progressBar != null)
        //        {
        //            for (int i = 1; i < 101; i++)
        //            {
        //                this.Dispatcher.Invoke(() =>
        //                {
        //                    _progressBar.Value = i;
        //                    if (i == 100)
        //                    {
        //                        _progressBar.Visibility = Visibility.Collapsed;
        //                        NextButtonAnimation(_loader);
        //                    }
        //                });

        //                Thread.Sleep(25);
        //            }
        //        }
        //    });
        //    thread.IsBackground = true;
        //    thread.Start();
        //}

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

        public void PreviewButtonAnimation(FrameworkElement control, EventHandler animationComplitedEvent = null)
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
    }
}