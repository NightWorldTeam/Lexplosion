using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Lexplosion.UI.WPF.Core;
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
            Unloaded += (sender, args) => AttachNavigationModel(null);
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

        public override void OnApplyTemplate()
        {
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

            _navigationModel?.SetContentNavigationDeferred(false);
            _navigationModel = model;
            _navigationModel?.SetContentNavigationDeferred(true);
        }

        private void OnItemChecked(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is DefaultMainMenuTabItem selected) ||
                selected.IsChecked != true)
                return;

            // The binding to LeftPanelMenuItem.IsSelected may not have updated
            // its source yet when the Checked routed event is raised.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!IsLoaded || selected.IsChecked != true) return;

                if (selected.ActualHeight > 0 && _position != null)
                    MoveCaret(selected, true);
                else
                    QueuePositionUpdate();
            }), DispatcherPriority.Loaded);
        }

        private void CommitSelectedContent(DefaultMainMenuTabItem selected)
        {
            if (!(selected.DataContext is LeftPanelMenuItem item)) return;

            // Queue page creation below render/input priority once movement starts.
            // Waiting for Completed added 260 ms of dead time before content appeared.
            var model = _navigationModel;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (IsLoaded && ReferenceEquals(model, _navigationModel))
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
                ++_animationVersion;
                _position.BeginAnimation(TranslateTransform.YProperty, null);
                _position.Y = target;
                _positionInitialized = true;
                CommitSelectedContent(selected);
                return;
            }

            double current = _position.Y;
            _position.BeginAnimation(TranslateTransform.YProperty, null);
            _position.Y = current;
            if (Math.Abs(current - target) < 0.5)
            {
                CommitSelectedContent(selected);
                return;
            }

            // Duration/easing are defined by the external ControlTemplate in XAML.
            var animation = Template.Resources["SelectionMotion"] as DoubleAnimation;
            if (animation == null)
            {
                _position.Y = target;
                CommitSelectedContent(selected);
                return;
            }

            var motion = (DoubleAnimation)animation.Clone();
            motion.To = target;
            var position = _position;
            int version = ++_animationVersion;
            motion.Completed += (sender, args) =>
            {
                // Cancelled animations don't get to override newer selections.
                if (_position == null || !ReferenceEquals(_position, position) ||
                    version != _animationVersion) return;

                position.BeginAnimation(TranslateTransform.YProperty, null);
                position.Y = target;
            };
            _position.BeginAnimation(TranslateTransform.YProperty, motion);
            CommitSelectedContent(selected);
        }
    }
}
