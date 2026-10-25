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
        SlideToBottom
    }

    [TemplatePart(Name = "PART_FirstContent", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_SecondContent", Type = typeof(ContentPresenter))]
    public class AnimatedContentControl : ContentControl
    {
        private ContentPresenter _current;
        private ContentPresenter _next;
        private bool _transitionPending;
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
            Unloaded += (sender, args) => FinishTransition();
        }

        public override void OnApplyTemplate()
        {
            ++_transitionVersion;
            base.OnApplyTemplate();

            _current = GetTemplateChild("PART_FirstContent") as ContentPresenter;
            _next = GetTemplateChild("PART_SecondContent") as ContentPresenter;
            _transitionPending = false;

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

            // Always keep only two views alive, including during rapid navigation.
            // A superseded animation is completed before the next transition.
            FinishTransition();

            if (ReferenceEquals(oldContent, newContent))
                return;

            if (!IsLoaded || oldContent == null || newContent == null ||
                AnimationType == ContentControlAnimation.None ||
                !SystemParameters.ClientAreaAnimation ||
                !TransitionDuration.HasTimeSpan || TransitionDuration.TimeSpan <= TimeSpan.Zero)
            {
                ShowImmediately(newContent);
                return;
            }

            _next.Content = newContent;
            _next.Visibility = Visibility.Hidden; // Measure without showing a misplaced frame.
            _next.IsHitTestVisible = false;
            _transitionPending = true;
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
            var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

            double distance = AnimationType == ContentControlAnimation.SlideToTop ||
                              AnimationType == ContentControlAnimation.SlideToBottom
                ? ActualHeight : ActualWidth;

            double sign = AnimationType == ContentControlAnimation.SlideToRight ||
                          AnimationType == ContentControlAnimation.SlideToBottom ? -1 : 1;

            bool vertical = AnimationType == ContentControlAnimation.SlideToTop ||
                            AnimationType == ContentControlAnimation.SlideToBottom;
            bool swipe = AnimationType != ContentControlAnimation.Fade;

            // The new page travels a full viewport. The previous page moves only
            // 28% of it, giving the depth/parallax of a mobile navigation stack.
            if (swipe)
            {
                var oldTransform = new TranslateTransform();
                var newTransform = new TranslateTransform();
                oldPresenter.RenderTransform = oldTransform;
                newPresenter.RenderTransform = newTransform;

                var property = vertical ? TranslateTransform.YProperty : TranslateTransform.XProperty;
                double incoming = sign * distance;
                double outgoing = -sign * distance * 0.28;
                newTransform.SetValue(property, incoming);

                oldTransform.BeginAnimation(property, Motion(0, outgoing, duration, easing));
                newTransform.BeginAnimation(property, Motion(incoming, 0, duration, easing));
            }

            // The old page is still rendered while the incoming page moves over it.
            // No bitmap snapshots or animations of Margin/Width/layout.
            Panel.SetZIndex(oldPresenter, 0);
            Panel.SetZIndex(newPresenter, 1);
            newPresenter.Visibility = Visibility.Visible;
            oldPresenter.IsHitTestVisible = false;
            newPresenter.IsHitTestVisible = true;

            oldPresenter.BeginAnimation(OpacityProperty, Motion(1, swipe ? 0.72 : 0, duration, easing));
            var reveal = Motion(swipe ? 0.95 : 0, 1, duration, easing);
            reveal.Completed += (sender, args) =>
            {
                if (version == _transitionVersion)
                    FinishTransition();
            };
            newPresenter.BeginAnimation(OpacityProperty, reveal);
        }

        private static DoubleAnimation Motion(double from, double to, Duration duration, IEasingFunction easing)
        {
            return new DoubleAnimation(from, to, duration) { EasingFunction = easing };
        }

        private void FinishTransition()
        {
            ++_transitionVersion;
            if (_current == null || _next == null || !_transitionPending)
                return;

            _transitionPending = false;
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
        }

        private void ShowImmediately(object content)
        {
            ++_transitionVersion;
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
