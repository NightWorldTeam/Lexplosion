using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
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
        SlideToBottom,
        SoftReveal
    }

    [TemplatePart(Name = "PART_FirstContent", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_SecondContent", Type = typeof(ContentPresenter))]
    public class AnimatedContentControl : ContentControl
    {
        private ContentPresenter _current;
        private ContentPresenter _next;
        private bool _transitionPending;
        private int _transitionVersion;
        private bool _hasQueuedContent;
        private object _queuedContent;

        // The transition state is exposed to the hosting TabControl, so tab input
        // can be locked for the actual animation lifetime (no hard-coded timers).
        private static readonly DependencyPropertyKey IsTransitioningPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsTransitioning), typeof(bool),
                typeof(AnimatedContentControl), new PropertyMetadata(false));

        public static readonly DependencyProperty IsTransitioningProperty =
            IsTransitioningPropertyKey.DependencyProperty;

        public bool IsTransitioning => (bool)GetValue(IsTransitioningProperty);

        public static readonly DependencyProperty AnimationTypeProperty =
            DependencyProperty.Register(nameof(AnimationType), typeof(ContentControlAnimation),
                typeof(AnimatedContentControl),
                new FrameworkPropertyMetadata(ContentControlAnimation.None));

        public ContentControlAnimation AnimationType
        {
            get => (ContentControlAnimation)GetValue(AnimationTypeProperty);
            set => SetValue(AnimationTypeProperty, value);
        }

        public static readonly DependencyProperty TransitionDurationProperty =
            DependencyProperty.Register(nameof(TransitionDuration), typeof(Duration),
                typeof(AnimatedContentControl),
                new FrameworkPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(380))));

        public Duration TransitionDuration
        {
            get => (Duration)GetValue(TransitionDurationProperty);
            set => SetValue(TransitionDurationProperty, value);
        }

        public AnimatedContentControl()
        {
            Unloaded += (sender, args) =>
            {
                FinishTransition();
                Reset(_current);
            };
        }

        public override void OnApplyTemplate()
        {
            ++_transitionVersion;
            base.OnApplyTemplate();

            _current = GetTemplateChild("PART_FirstContent") as ContentPresenter;
            _next = GetTemplateChild("PART_SecondContent") as ContentPresenter;
            _transitionPending = false;
            _hasQueuedContent = false;
            _queuedContent = null;
            SetValue(IsTransitioningPropertyKey, false);

            if (_current == null || _next == null)
                return;

            Reset(_current);
            Reset(_next);
            _current.Content = Content;
            _current.Visibility = Visibility.Visible;
            _current.IsHitTestVisible = true;
            _next.Content = null;
            _next.Visibility = Visibility.Collapsed;
            _next.IsHitTestVisible = false;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (_current == null || _next == null)
                return;

            // Do not interrupt a running animation. Only the latest requested
            // destination is kept; intermediate navigation requests are discarded.
            if (_transitionPending)
            {
                _queuedContent = newContent;
                _hasQueuedContent = true;
                return;
            }

            ShowContent(newContent);
        }

        private void ShowContent(object newContent)
        {
            var oldContent = _current.Content;
            if (ReferenceEquals(oldContent, newContent))
                return;

            if (!IsLoaded || oldContent == null || newContent == null ||
                AnimationType == ContentControlAnimation.None ||
                !SystemParameters.ClientAreaAnimation ||
                !TransitionDuration.HasTimeSpan || TransitionDuration.TimeSpan <= TimeSpan.Zero ||
                ContainsNativeWindow(_current))
            {
                ShowImmediately(newContent);
                return;
            }

            // Stage the incoming visual tree before starting the dissolve.
            // It must complete DataTemplate creation and layout while invisible.
            if (AnimationType == ContentControlAnimation.SoftReveal)
                _next.Opacity = 0;
            _next.Content = newContent;
            _next.Visibility = Visibility.Hidden;
            _next.IsHitTestVisible = false;
            _transitionPending = true;
            SetValue(IsTransitioningPropertyKey, true);
            int version = ++_transitionVersion;

            // Let the new DataTemplate materialize and layout finish before moving it.
            Dispatcher.BeginInvoke(new Action(() => BeginTransition(version)), DispatcherPriority.Loaded);
        }

        private void BeginTransition(int version)
        {
            if (version != _transitionVersion || !_transitionPending ||
                !IsLoaded || _current == null || _next == null)
                return;

            if (ActualWidth <= 0 || ActualHeight <= 0 ||
                ContainsNativeWindow(_current) || ContainsNativeWindow(_next))
            {
                // A WebView2/HwndHost paints in its own HWND, outside WPF's compositor.
                // Never slide two native windows or attempt to capture them.
                FinishTransition();
                return;
            }

            var oldPresenter = _current;
            var newPresenter = _next;
            var duration = TransitionDuration;
            IEasingFunction easing = AnimationType == ContentControlAnimation.SoftReveal
                ? new SineEase { EasingMode = EasingMode.EaseInOut }
                : new CubicEase { EasingMode = EasingMode.EaseInOut };

            double distance = AnimationType == ContentControlAnimation.SlideToTop ||
                              AnimationType == ContentControlAnimation.SlideToBottom
                ? ActualHeight : ActualWidth;

            double sign = AnimationType == ContentControlAnimation.SlideToRight ||
                          AnimationType == ContentControlAnimation.SlideToBottom ? -1 : 1;

            bool vertical = AnimationType == ContentControlAnimation.SlideToTop ||
                            AnimationType == ContentControlAnimation.SlideToBottom;
            bool swipe = AnimationType != ContentControlAnimation.Fade &&
                         AnimationType != ContentControlAnimation.SoftReveal;

            // Both pages travel the same distance, with no overlapping regions.
            // Parallax kept the old page under the new one and leaked stale controls
            // through transparent views (e.g. Friends tabs).
            if (swipe)
            {
                var oldTransform = new TranslateTransform();
                var newTransform = new TranslateTransform();
                oldPresenter.RenderTransform = oldTransform;
                newPresenter.RenderTransform = newTransform;

                var property = vertical ? TranslateTransform.YProperty : TranslateTransform.XProperty;
                double incoming = sign * distance;
                double outgoing = -sign * distance;
                newTransform.SetValue(property, incoming);

                // TranslateTransform runs on the composition path. Avoid opacity
                // animations on two full-page surfaces during a slide.
                var arrive = Motion(incoming, 0, duration, easing);
                arrive.Completed += (sender, args) =>
                {
                    if (version == _transitionVersion)
                        FinishTransition();
                };

                oldTransform.BeginAnimation(property, Motion(0, outgoing, duration, easing));
                newTransform.BeginAnimation(property, arrive);
            }

            // Adjacent pages move as one strip: the outgoing page never paints
            // underneath the incoming page. No snapshots or layout animations.
            Panel.SetZIndex(oldPresenter, 0);
            Panel.SetZIndex(newPresenter, 1);
            newPresenter.Visibility = Visibility.Visible;
            oldPresenter.IsHitTestVisible = false;
            newPresenter.IsHitTestVisible = true;

            if (!swipe)
            {
                oldPresenter.BeginAnimation(OpacityProperty, Motion(1, 0, duration, easing));
                var reveal = Motion(0, 1, duration, easing);
                reveal.Completed += (sender, args) =>
                {
                    if (version == _transitionVersion)
                        FinishTransition();
                };
                newPresenter.BeginAnimation(OpacityProperty, reveal);
            }
        }

        private static DoubleAnimation Motion(double from, double to, Duration duration, IEasingFunction easing)
        {
            return new DoubleAnimation(from, to, duration) { EasingFunction = easing };
        }

        private void FinishTransition()
        {
            ++_transitionVersion;
            if (_current == null || _next == null || !_transitionPending)
            {
                SetValue(IsTransitioningPropertyKey, false);
                return;
            }

            _transitionPending = false;
            SetValue(IsTransitioningPropertyKey, false);
            var previous = _current;
            _current = _next;
            _next = previous;

            // Clear the outgoing visual tree only once the transition has finished.
            Reset(_current);
            _current.Visibility = Visibility.Visible;
            _current.IsHitTestVisible = true;

            Reset(_next);
            _next.Visibility = Visibility.Collapsed;
            _next.IsHitTestVisible = false;
            _next.Content = null;

            if (_hasQueuedContent)
            {
                var queuedContent = _queuedContent;
                _hasQueuedContent = false;
                _queuedContent = null;
                ShowContent(queuedContent);
            }
        }

        private void ShowImmediately(object content)
        {
            ++_transitionVersion;
            SetValue(IsTransitioningPropertyKey, false);
            _current.Content = content;
            _current.Visibility = Visibility.Visible;
            _current.IsHitTestVisible = true;
            Reset(_current);
            _next.Content = null;
            _next.Visibility = Visibility.Collapsed;
            _next.IsHitTestVisible = false;
            Reset(_next);
        }

        private static void Reset(ContentPresenter presenter)
        {
            presenter.BeginAnimation(OpacityProperty, null);
            presenter.Opacity = 1;
            presenter.RenderTransform = null;
        }

        private static bool ContainsNativeWindow(DependencyObject element)
        {
            if (element is HwndHost)
                return true;

            int count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                if (ContainsNativeWindow(VisualTreeHelper.GetChild(element, i)))
                    return true;
            }
            return false;
        }
    }
}
