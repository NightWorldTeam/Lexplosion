using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Lexplosion.UI.WPF.Controls
{
    public enum ContentControlAnimation
    {
        None,
        Fade,
        SlideToLeft,
        SlideToRight,
        SlideToTop,
        SlideToBottom
    }

    // All motion is applied to the incoming presenter: no bitmap snapshots,
    // layout animations or movement of the outgoing visual tree.
    public class AnimatedContentControl : ContentControl
    {
        private ContentPresenter _presenter;
        private int _transitionVersion;

        public static readonly DependencyProperty AnimationTypeProperty =
            DependencyProperty.Register(nameof(AnimationType), typeof(ContentControlAnimation),
                typeof(AnimatedContentControl),
                new FrameworkPropertyMetadata(ContentControlAnimation.SlideToLeft));

        public ContentControlAnimation AnimationType
        {
            get => (ContentControlAnimation)GetValue(AnimationTypeProperty);
            set => SetValue(AnimationTypeProperty, value);
        }

        public override void OnApplyTemplate()
        {
            ++_transitionVersion;
            base.OnApplyTemplate();
            _presenter = GetTemplateChild("PART_MainContent") as ContentPresenter;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            int version = ++_transitionVersion;
            ResetPresenter();

            // Do not animate the initial page, empty content or disabled system motion.
            if (oldContent == null || newContent == null ||
                AnimationType == ContentControlAnimation.None ||
                !SystemParameters.ClientAreaAnimation)
                return;

            // Wait until WPF has created and measured the incoming page.
            Dispatcher.BeginInvoke(new Action(() => StartTransition(version)), DispatcherPriority.Loaded);
        }

        private void StartTransition(int version)
        {
            if (version != _transitionVersion || !IsLoaded || _presenter == null ||
                ActualWidth <= 0 || ActualHeight <= 0)
                return;

            var presenter = _presenter;
            var duration = new Duration(TimeSpan.FromMilliseconds(240));
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

            // Keep the movement small. A full-width slide is distracting and
            // makes HwndHost-based content appear to jump between layouts.
            double x = 0;
            double y = 0;
            switch (AnimationType)
            {
                case ContentControlAnimation.SlideToLeft:
                    x = 18;
                    break;
                case ContentControlAnimation.SlideToRight:
                    x = -18;
                    break;
                case ContentControlAnimation.SlideToTop:
                    y = 14;
                    break;
                case ContentControlAnimation.SlideToBottom:
                    y = -14;
                    break;
            }

            if (x != 0 || y != 0)
            {
                var translation = new TranslateTransform();
                presenter.RenderTransform = translation;
                if (x != 0)
                {
                    translation.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(x, 0, duration)
                    {
                        EasingFunction = easing
                    });
                }
                if (y != 0)
                {
                    translation.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(y, 0, duration)
                    {
                        EasingFunction = easing
                    });
                }
            }

            // Fade is used for full page navigation: no RenderTransform on WebView2.
            var opacity = new DoubleAnimation(0.65, 1, duration)
            {
                EasingFunction = easing
            };
            opacity.Completed += (sender, args) =>
            {
                if (version != _transitionVersion || !ReferenceEquals(_presenter, presenter))
                    return;
                ResetPresenter();
            };
            presenter.BeginAnimation(OpacityProperty, opacity);
        }

        private void ResetPresenter()
        {
            if (_presenter == null)
                return;

            _presenter.BeginAnimation(OpacityProperty, null);
            _presenter.Opacity = 1;
            _presenter.RenderTransform = null;
        }
    }
}
