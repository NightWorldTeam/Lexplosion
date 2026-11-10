using Lexplosion.UI.WPF.Core.Animations;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для MainMenuLayoutViewModel.xaml
    /// </summary>
    public partial class MainMenuLayoutView : UserControl
    {
        public static readonly DependencyProperty DisplayedContentProperty =
            DependencyProperty.Register(nameof(DisplayedContent), typeof(object),
                typeof(MainMenuLayoutView), new PropertyMetadata(null));

        private readonly NavigationTransitionCoordinator _transitionCoordinator;
        private MainMenuLayoutViewModel _observedModel;

        public object DisplayedContent
        {
            get { return GetValue(DisplayedContentProperty); }
            private set { SetValue(DisplayedContentProperty, value); }
        }

        public MainMenuLayoutView()
        {
            InitializeComponent();
            _transitionCoordinator = new NavigationTransitionCoordinator(Dispatcher, PublishContent);
            NavigationTransitionCoordinator.SetCurrent(this, _transitionCoordinator);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AttachModel(DataContext as MainMenuLayoutViewModel);
            _transitionCoordinator.Resume();
            _transitionCoordinator.ShowImmediately(_observedModel?.Content);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _transitionCoordinator.Suspend();
            AttachModel(null);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            AttachModel(e.NewValue as MainMenuLayoutViewModel);
            _transitionCoordinator.ShowImmediately(_observedModel?.Content);
        }

        private void AttachModel(MainMenuLayoutViewModel model)
        {
            if (ReferenceEquals(_observedModel, model))
                return;

            if (_observedModel != null)
                _observedModel.PropertyChanged -= OnModelPropertyChanged;

            _observedModel = model;
            if (_observedModel != null)
                _observedModel.PropertyChanged += OnModelPropertyChanged;
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainMenuLayoutViewModel.Content) ||
                string.IsNullOrEmpty(e.PropertyName))
            {
                _transitionCoordinator.Request(_observedModel.Content);
            }
        }

        private void PublishContent(object content)
        {
            DisplayedContent = content;
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var actualHeight = ((Grid)sender).ActualHeight;
            var actualWidth = ((Grid)sender).ActualWidth;

            var grid = (Grid)sender;


            foreach (var i in grid.ColumnDefinitions)
            {
                Runtime.DebugWrite(i.ActualWidth);
            }
        }

    }
}
