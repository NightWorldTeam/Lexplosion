using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lexplosion.UI.WPF.Core.Animations
{
    /// <summary>
    /// Keeps the latest navigation destination independent of WPF animation
    /// completion. A page is materialized after the selection motion, so a
    /// heavy DataTemplate cannot freeze the moving sidebar indicator.
    /// </summary>
    public sealed class NavigationTransitionCoordinator
    {
        public static readonly DependencyProperty CurrentProperty =
            DependencyProperty.RegisterAttached("Current", typeof(NavigationTransitionCoordinator),
                typeof(NavigationTransitionCoordinator),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        private readonly Dispatcher _dispatcher;
        private readonly Action<object> _publish;
        private object _displayedContent;
        private object _requestedContent;
        private bool _hasRequest;
        private bool _enabled;
        private bool _selectionAnimating;
        private bool _renderingSubscribed;
        private bool _publishScheduled;
        private int _selectionVersion;
        private long _motionDeadline;

        public NavigationTransitionCoordinator(Dispatcher dispatcher, Action<object> publish)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _publish = publish ?? throw new ArgumentNullException(nameof(publish));
        }

        public static NavigationTransitionCoordinator GetCurrent(DependencyObject element)
        {
            return (NavigationTransitionCoordinator)element.GetValue(CurrentProperty);
        }

        public static void SetCurrent(DependencyObject element, NavigationTransitionCoordinator value)
        {
            element.SetValue(CurrentProperty, value);
        }

        public void Resume()
        {
            _enabled = true;
            if (_hasRequest || _selectionAnimating)
                SubscribeRendering();
        }

        public void Suspend()
        {
            _enabled = false;
            _selectionAnimating = false;
            UnsubscribeRendering();
        }

        public void ShowImmediately(object content)
        {
            _selectionAnimating = false;
            _hasRequest = false;
            _requestedContent = content;
            UnsubscribeRendering();
            _displayedContent = content;
            _publish(content);
        }

        public void Request(object content)
        {
            _requestedContent = content;
            _hasRequest = true;
            if (_enabled)
                SubscribeRendering();
        }

        public int BeginSelection(TimeSpan duration)
        {
            int version = ++_selectionVersion;
            _selectionAnimating = duration > TimeSpan.Zero;
            if (_selectionAnimating)
            {
                // Rendering is also the fallback if a Storyboard is cancelled
                // before its Completed event can be delivered.
                _motionDeadline = Stopwatch.GetTimestamp() +
                    (long)(duration.TotalSeconds * Stopwatch.Frequency);
            }

            if (_enabled)
                SubscribeRendering();
            return version;
        }

        public void CompleteSelection(int version)
        {
            if (version != _selectionVersion)
                return;

            _selectionAnimating = false;
            if (_enabled)
                QueuePublish();
        }

        private void SubscribeRendering()
        {
            if (_renderingSubscribed)
                return;

            CompositionTarget.Rendering += OnRendering;
            _renderingSubscribed = true;
        }

        private void UnsubscribeRendering()
        {
            if (!_renderingSubscribed)
                return;

            CompositionTarget.Rendering -= OnRendering;
            _renderingSubscribed = false;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            if (!_enabled)
            {
                UnsubscribeRendering();
                return;
            }

            if (_selectionAnimating)
            {
                if (Stopwatch.GetTimestamp() < _motionDeadline)
                    return;

                // Even if the animation clock vanished, the last destination
                // must eventually become visible.
                _selectionAnimating = false;
            }

            UnsubscribeRendering();
            QueuePublish();
        }

        private void QueuePublish()
        {
            if (!_enabled || _selectionAnimating || !_hasRequest || _publishScheduled)
                return;

            _publishScheduled = true;
            _dispatcher.BeginInvoke(new Action(PublishLatest), DispatcherPriority.Background);
        }

        private void PublishLatest()
        {
            _publishScheduled = false;
            if (!_enabled || _selectionAnimating || !_hasRequest)
                return;

            _hasRequest = false;
            if (ReferenceEquals(_displayedContent, _requestedContent))
                return;

            _displayedContent = _requestedContent;
            _publish(_displayedContent);
        }
    }
}
