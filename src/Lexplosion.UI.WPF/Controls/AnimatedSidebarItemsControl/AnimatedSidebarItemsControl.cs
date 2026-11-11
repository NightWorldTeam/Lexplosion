using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Animations;
#if DEBUG
using Lexplosion.QA;
#endif
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu;

namespace Lexplosion.UI.WPF.Controls
{
    [TemplatePart(Name = "PART_SelectionCaret", Type = typeof(Border))]
    public class AnimatedSidebarItemsControl : ItemsControl
    {
        private Border _caret;
        private TranslateTransform _position;
        private bool _positionInitialized;
        private bool _updatePending;
        private int _animationVersion;
        private readonly WpfMotionTimeline _caretMotion = new WpfMotionTimeline();
        private int _pendingCaretVersion;
        private int _coordinatedSelectionVersion;
        private double _pendingCaretTarget;
        private TranslateTransform _pendingCaretTransform;
        private DefaultMainMenuTabItem _pendingCaretItem;
        private LeftPanelViewModel _navigationModel;

        static AnimatedSidebarItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedSidebarItemsControl),
                new FrameworkPropertyMetadata(typeof(AnimatedSidebarItemsControl)));
        }

        public AnimatedSidebarItemsControl()
        {
            AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(OnItemChecked));
            Loaded += (sender, args) =>
            {
                AttachNavigationModel(DataContext as LeftPanelViewModel);
                QueuePositionUpdate();
            };
            Unloaded += OnUnloaded;
            DataContextChanged += (sender, args) =>
            {
                if (IsLoaded)
                    AttachNavigationModel(args.NewValue as LeftPanelViewModel);
            };
            ItemContainerGenerator.StatusChanged += (sender, args) =>
            {
                if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    QueuePositionUpdate();
            };
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _caretMotion.Cancel();
            CompleteCoordinatedSelection();
            AttachNavigationModel(null);
        }

        public override void OnApplyTemplate()
        {
            _caretMotion.Cancel();
            base.OnApplyTemplate();
            _caret = GetTemplateChild("PART_SelectionCaret") as Border;
            _position = _caret?.RenderTransform as TranslateTransform;
            if (_position?.IsFrozen == true)
            {
                // Templates may freeze their inline transforms when sealed.
                _position = (TranslateTransform)_position.CloneCurrentValue();
                _caret.RenderTransform = _position;
            }
            _positionInitialized = false;
            ++_animationVersion;
            QueuePositionUpdate();
        }

        private void AttachNavigationModel(LeftPanelViewModel model)
        {
            if (ReferenceEquals(_navigationModel, model)) return;

            // The selected ViewModel is published immediately. Only the WPF
            // visual transition is deferred; cancellation must not lose a click.
            _navigationModel = model;
        }

        private void OnItemChecked(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is DefaultMainMenuTabItem selected) ||
                selected.IsChecked != true)
                return;

            // RadioButton.Checked is raised before its TwoWay binding finishes
            // updating the selected ViewModel. Start the caret on this input
            // event instead of queuing it behind page layout and DataTemplates.
            bool canMoveNow = IsLoaded && selected.ActualHeight > 0 && _position != null;
            var coordinator = NavigationTransitionCoordinator.GetCurrent(this);
            if (coordinator != null)
            {
                TimeSpan duration = TimeSpan.Zero;
                var motion = Template?.Resources["SelectionMotion"] as DoubleAnimation;
                if (canMoveNow && _positionInitialized && AnimationPreferences.Enabled &&
                    motion != null && motion.Duration.HasTimeSpan)
                    duration = motion.Duration.TimeSpan;

                _coordinatedSelectionVersion = coordinator.BeginSelection(duration);
            }

            if (canMoveNow)
                MoveCaret(selected, true);
            else
                QueuePositionUpdate();
        }

        private void CommitSelectedContent(DefaultMainMenuTabItem selected)
        {
            // Main-menu content is now published immediately by the selection
            // model; the WPF coordinator handles when it appears on screen.
            if (NavigationTransitionCoordinator.GetCurrent(this) != null)
                return;

            if (!(selected.DataContext is LeftPanelMenuItem item)) return;

            // Queue page creation below render/input priority once movement starts.
            // Waiting for Completed added 260 ms of dead time before content appeared.
            var model = _navigationModel;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!IsLoaded || !ReferenceEquals(model, _navigationModel))
                    return;

#if DEBUG
                UiDebugTrace.Navigate(item.Content);
                if (PerformanceTelemetry.Enabled && item.Content != null)
                    PerformanceTelemetry.Record("navigation.sidebar." +
                        item.Content.GetType().Name, 0);
                using (UiDebugTrace.Measure("sidebar.commit", 3))
#endif
                    model?.CommitSelection(item);
            }), DispatcherPriority.Background);
        }

        private void QueuePositionUpdate()
        {
            if (_updatePending) return;
            _updatePending = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _updatePending = false;
                if (!IsLoaded) return;

                foreach (var item in Items)
                {
                    var container = ItemContainerGenerator.ContainerFromItem(item);
                    if (container == null) continue;
                    var selected = FindSelectedItem(container);
                    if (selected == null) continue;

                    MoveCaret(selected, false);
                    return;
                }

                if (_caret != null)
                    _caret.Visibility = Visibility.Collapsed;
                _positionInitialized = false;
            }), DispatcherPriority.Loaded);
        }

        private static DefaultMainMenuTabItem FindSelectedItem(DependencyObject root)
        {
            if (root is DefaultMainMenuTabItem tab && tab.IsChecked == true)
                return tab;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var found = FindSelectedItem(VisualTreeHelper.GetChild(root, i));
                if (found != null) return found;
            }

            return null;
        }

        private void MoveCaret(DefaultMainMenuTabItem selected, bool animate)
        {
#if DEBUG
            using (UiDebugTrace.Measure("sidebar.caret", 3))
#endif
            {
            if (_caret == null || _position == null || !IsLoaded)
                return;

            double target;
            try
            {
                target = selected.TranslatePoint(new Point(0, 0), this).Y;
            }
            catch (InvalidOperationException)
            {
                QueuePositionUpdate();
                return;
            }

            _caret.Visibility = Visibility.Visible;

            // No motion on first render, layout refreshes or when disabled.
            if (!_positionInitialized || !animate || !AnimationPreferences.Enabled)
            {
                _caretMotion.Cancel();
                ++_animationVersion;
                _position.BeginAnimation(TranslateTransform.YProperty, null);
                _position.Y = target;
                _positionInitialized = true;
                CompleteCoordinatedSelection();
                CommitSelectedContent(selected);
                return;
            }

            double current = _position.Y;
            _caretMotion.Cancel();
            _position.BeginAnimation(TranslateTransform.YProperty, null);
            _position.Y = current;
            if (Math.Abs(current - target) < 0.5)
            {
                CompleteCoordinatedSelection();
                CommitSelectedContent(selected);
                return;
            }

            // Duration/easing are defined by the external ControlTemplate in XAML.
            var animation = Template.Resources["SelectionMotion"] as DoubleAnimation;
            if (animation == null)
            {
                _position.Y = target;
                CompleteCoordinatedSelection();
                CommitSelectedContent(selected);
                return;
            }

            var motion = (DoubleAnimation)animation.Clone();
            motion.To = target;
            _pendingCaretTransform = _position;
            _pendingCaretTarget = target;
            _pendingCaretItem = selected;
            _pendingCaretVersion = ++_animationVersion;
            _caretMotion.Add(_position, TranslateTransform.YProperty, motion);
            _caretMotion.Begin(this, OnCaretMotionCompleted);
            }
        }

        private void OnCaretMotionCompleted()
        {
            // Cancelled animations don't get to override newer selections.
            if (_position == null || !ReferenceEquals(_position, _pendingCaretTransform) ||
                _pendingCaretVersion != _animationVersion)
                return;

            _position.BeginAnimation(TranslateTransform.YProperty, null);
            _position.Y = _pendingCaretTarget;
#if DEBUG
            if (PerformanceTelemetry.Enabled)
                PerformanceTelemetry.Record("navigation.sidebar.motion.completed", 0);
#endif
            CompleteCoordinatedSelection();
            CommitSelectedContent(_pendingCaretItem);
        }

        private void CompleteCoordinatedSelection()
        {
            var coordinator = NavigationTransitionCoordinator.GetCurrent(this);
            if (coordinator != null)
                coordinator.CompleteSelection(_coordinatedSelectionVersion);
        }
    }
}
