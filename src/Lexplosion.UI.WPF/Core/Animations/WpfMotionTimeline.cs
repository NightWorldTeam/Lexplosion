using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.UI.WPF.Core.Animations
{
    /// <summary>
    /// Owns one WPF storyboard at a time. All tracks added before Begin share
    /// one timeline and one completion callback. It does not own navigation
    /// state, page loading or a ViewModel.
    /// </summary>
    public sealed class WpfMotionTimeline
    {
        private Storyboard _pending = new Storyboard();
        private Storyboard _running;
        private FrameworkElement _owner;
        private Action _completed;
        private bool _renderingSubscribed;
        private long _completionDeadline;

        public void Add(DependencyObject target, DependencyProperty property, DoubleAnimation animation)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            if (animation == null)
                throw new ArgumentNullException(nameof(animation));

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));
            _pending.Children.Add(animation);
        }

        public void Begin(FrameworkElement owner, Action completed)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            StopActive();

            var storyboard = _pending;
            _pending = new Storyboard();

            if (storyboard.Children.Count == 0)
            {
                if (completed != null)
                    completed();
                return;
            }

            _owner = owner;
            _completed = completed;
            _running = storyboard;
            storyboard.Completed += OnCompleted;
            storyboard.Begin(owner, true);

            // WPF can drop Completed when a control/template is replaced during
            // rapid navigation. Do not leave the next destination queued forever.
            TimeSpan duration = TimeSpan.Zero;
            foreach (Timeline track in storyboard.Children)
            {
                if (track.Duration.HasTimeSpan && track.Duration.TimeSpan > duration)
                    duration = track.Duration.TimeSpan;
            }

            if (ReferenceEquals(_running, storyboard) && duration > TimeSpan.Zero)
            {
                _completionDeadline = Stopwatch.GetTimestamp() +
                    (long)(duration.TotalSeconds * Stopwatch.Frequency);
                CompositionTarget.Rendering += OnRendering;
                _renderingSubscribed = true;
            }
        }

        public void Cancel()
        {
            StopActive();
            _pending.Children.Clear();
        }

        private void OnCompleted(object sender, EventArgs e)
        {
            CompleteActive();
        }

        private void OnRendering(object sender, EventArgs e)
        {
            if (_running != null && Stopwatch.GetTimestamp() >= _completionDeadline)
                CompleteActive();
        }

        private void CompleteActive()
        {
            if (_running == null)
                return;

            var completed = _completed;
            StopActive();
            if (completed != null)
                completed();
        }

        private void StopActive()
        {
            if (_renderingSubscribed)
            {
                CompositionTarget.Rendering -= OnRendering;
                _renderingSubscribed = false;
            }

            var running = _running;
            var owner = _owner;

            _running = null;
            _owner = null;
            _completed = null;

            if (running == null)
                return;

            running.Completed -= OnCompleted;
            if (owner != null)
                running.Remove(owner);
        }
    }
}
