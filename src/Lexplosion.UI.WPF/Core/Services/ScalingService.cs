using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.UI.WPF.Core.Services
{
    public sealed class ScalingService
    {
        private bool _isScalled = false;
        public double ScalingKeff { get; private set; } = 1;
        public double ScalingFactor { get; private set; } = 1;


        public const int DefaultMinWidth = 944;
        public const int DefaultMinHeight = 528;

        private double NoFactorWidth;
        private double NoFactorHeight;

        private double PreviousScaleValue = 1;

        private readonly AppCore _appCore;


        private readonly Window _target;
        private readonly FrameworkElement _targetContainer;

        public ScalingService(AppCore appCore, Window target, FrameworkElement targetContainer)
        {
            _appCore = appCore;
            _target = target;
            _targetContainer = targetContainer;
            _appCore.Settings.SettingsFieldChanged += OnAppSettingsFieldChanged;
        }

        private void OnAppSettingsFieldChanged(string obj)
        {
            if (obj == "ZoomLevel")
            {
                if (_appCore.Settings.Core.IsScalingAnimationEnabled)
                {
                    RescaleWithAnimation(_target, _targetContainer);
                }
                else
                {
                    Rescale(_target, _targetContainer);
                }
            }
        }

        public void ChangeNoFactorSizeValues(double noFactorWidth = 944, double noFactorHeight = 528)
        {
            NoFactorWidth = noFactorWidth;
            NoFactorHeight = noFactorHeight;
        }

        public void Rescale(Window target, FrameworkElement targetContainer)
        {
            var scalingFactor = _appCore.Settings.Core.ZoomLevel;
            var isCenterWindowAuto = (bool)_appCore.Settings.Core.IsCenterWindowAuto;
            ScalingFactor = scalingFactor > 1 ? scalingFactor - 1 : 0;

            ScalingKeff = ScalingFactor + 1;
            var isScalled = ScalingFactor > 0;

            var scaleTransform = targetContainer.RenderTransform as ScaleTransform ?? new ScaleTransform(ScalingFactor, ScalingFactor);
            if (scaleTransform.ScaleX != ScalingKeff && scaleTransform.ScaleY != ScalingKeff)
            {
                var newMinWidth = isScalled ? DefaultMinWidth + DefaultMinWidth * ScalingFactor : DefaultMinWidth;
                var newMinHeight = isScalled ? DefaultMinHeight + DefaultMinHeight * ScalingFactor : DefaultMinHeight;

                var newWidth = NoFactorWidth * (1 + ScalingFactor);
                var newHeight = NoFactorHeight * (1 + ScalingFactor);

                targetContainer.LayoutTransform = new ScaleTransform(ScalingKeff, ScalingKeff);

                // Удаляем scope анимаций, чтобы иметь возможность изменить значения свойств.
                target.BeginAnimation(Window.MinWidthProperty, null);
                target.BeginAnimation(Window.MinHeightProperty, null);
                target.BeginAnimation(Window.LeftProperty, null);
                target.BeginAnimation(Window.TopProperty, null);
                target.BeginAnimation(Window.WidthProperty, null);
                target.BeginAnimation(Window.HeightProperty, null);

                target.MinWidth = newMinWidth;
                target.MinHeight = newMinHeight;
                target.Width = newWidth;
                target.Height = newHeight;

                if (isCenterWindowAuto)
                {
                    double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                    double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

                    target.Left = (screenWidth / 2) - (target.Width / 2);
                    target.Top = (screenHeight / 2) - (target.Height / 2);
                }
            }
        }

        public void RescaleWithAnimation(Window target, FrameworkElement targetContainer, Action scaleAnimationCompletedAction = null)
        {
            var scalingFactor = _appCore.Settings.Core.ZoomLevel;
            var isCenterWindowAuto = (bool)_appCore.Settings.Core.IsCenterWindowAuto;
            ScalingFactor = scalingFactor > 1 ? scalingFactor - 1 : 0;

            ScalingKeff = ScalingFactor + 1;
            var isScalled = ScalingFactor > 0;

            var scaleTransform = targetContainer.RenderTransform as ScaleTransform ?? new ScaleTransform(ScalingFactor, ScalingFactor);

            if (scaleTransform.ScaleX != ScalingKeff && scaleTransform.ScaleY != ScalingKeff)
            {
                scaleTransform = new ScaleTransform(scaleTransform.ScaleX, scaleTransform.ScaleY);
                targetContainer.LayoutTransform = scaleTransform;

                var newWidth = NoFactorWidth * (1 + ScalingFactor);
                var newHeight = NoFactorHeight * (1 + ScalingFactor);

                var animation = new DoubleAnimation(PreviousScaleValue, ScalingKeff,
                    new Duration(TimeSpan.FromMilliseconds(300)))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

                var newMinWidth = isScalled ? DefaultMinWidth + DefaultMinWidth * ScalingFactor : DefaultMinWidth;
                var newMinHeight = isScalled ? DefaultMinHeight + DefaultMinHeight * ScalingFactor : DefaultMinHeight;

                Storyboard minPropertiesStoryboard = new Storyboard();

                var minWidthAnimation = new DoubleAnimation()
                {
                    To = newMinWidth,
                    From = target.MinWidth,
                    Duration = TimeSpan.FromMilliseconds(50)
                };

                var minHeightAnimation = new DoubleAnimation()
                {
                    To = newMinHeight,
                    From = target.MinHeight,
                    Duration = TimeSpan.FromMilliseconds(50)
                };

                Storyboard.SetTarget(minWidthAnimation, target);
                Storyboard.SetTargetProperty(minWidthAnimation, new PropertyPath(Window.MinWidthProperty));
                Storyboard.SetTarget(minHeightAnimation, target);
                Storyboard.SetTargetProperty(minHeightAnimation, new PropertyPath(Window.MinHeightProperty));
                minPropertiesStoryboard.Children.Add(minWidthAnimation);
                minPropertiesStoryboard.Children.Add(minHeightAnimation);

                minPropertiesStoryboard.Completed += (s, e) =>
                {
                    ThreadPool.QueueUserWorkItem((state) =>
                    {
                        Thread.Sleep(100);
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            target.MinWidth = newMinWidth;
                            target.MinHeight = newMinHeight;
                            target.Width = newWidth;
                            target.Height = newHeight;
                            if (isCenterWindowAuto)
                            {
                                CenterWindowWithAnimation(target);
                            }
                            else if (scaleAnimationCompletedAction != null)
                            {
                                scaleAnimationCompletedAction.Invoke();
                            }
                        });
                    });
                };

                minPropertiesStoryboard.Begin();

                PreviousScaleValue = ScalingKeff;
                _isScalled = isScalled;
            }
        }

        public void CenterWindowWithAnimation(Window target)
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            Storyboard centerWindowStoryboard = new Storyboard();

            var newLeft = (screenWidth / 2) - (target.Width / 2);
            var newTop = (screenHeight / 2) - (target.Height / 2);

            var leftPointAnimation = new DoubleAnimation()
            {
                To = newLeft,
                From = target.Left,
                Duration = TimeSpan.FromMilliseconds(1500),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn },
                DecelerationRatio = 0.1
            };

            var topPointAnimation = new DoubleAnimation()
            {
                To = newTop,
                From = target.Left,
                Duration = TimeSpan.FromMilliseconds(1500),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn },
                DecelerationRatio = 0.1
            };

            Storyboard.SetTarget(leftPointAnimation, target);
            Storyboard.SetTargetProperty(leftPointAnimation, new PropertyPath(Window.LeftProperty));
            Storyboard.SetTarget(topPointAnimation, target);
            Storyboard.SetTargetProperty(topPointAnimation, new PropertyPath(Window.TopProperty));
            centerWindowStoryboard.Children.Add(leftPointAnimation);
            centerWindowStoryboard.Children.Add(topPointAnimation);

            centerWindowStoryboard.Begin();
        }
    }
}
