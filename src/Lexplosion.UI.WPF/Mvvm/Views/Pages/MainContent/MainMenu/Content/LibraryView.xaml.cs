using Lexplosion.UI.WPF.Extensions;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System.Windows.Media;
using System.Collections.Specialized;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.MainMenu
{
	/// <summary>
	/// Логика взаимодействия для LibraryView.xaml
	/// </summary>
	public partial class LibraryView : System.Windows.Controls.UserControl
	{
		LibraryViewModel _viewModel;
		static int posIndex = 0;
		bool _isFilterHidden = false;
        private INotifyCollectionChanged _observedInstances;

		public LibraryView()
		{
			InitializeComponent();
			DataContextChanged += OnDataContextChanged;
			InstanceList.Loaded += OnInstanceListLoaded;
			Runtime.DebugWrite("LibraryView ctor");
		}

		private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
		{
			Runtime.DebugWrite("OnDataContextChanged");

            if (_viewModel != null)
            {
                if (_observedInstances != null)
                    _observedInstances.CollectionChanged -= OnInstanceListChanged;
                _viewModel.InstanceProfileOpened -= OnInstanceProfileOpened;
                _observedInstances = null;
            }

			_viewModel = DataContext as LibraryViewModel;

			if (_viewModel == null)
			{
				posIndex = GetFirstVisibleItemIndex();
				Runtime.DebugWrite($"Pos: {posIndex}", color: System.ConsoleColor.Red);
				return;
			}

            _observedInstances = _viewModel.Model.InstanceController.Instances as INotifyCollectionChanged;
            if (_observedInstances != null)
                _observedInstances.CollectionChanged += OnInstanceListChanged;
            _viewModel.InstanceProfileOpened += OnInstanceProfileOpened;
		}

        private void OnInstanceProfileOpened(InstanceModelBase model)
        {
            posIndex = GetFirstVisibleItemIndex();
        }

        private void OnInstanceListChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) 
			{
                var scrollViewer = InstanceList.FindVisualDescendant<ScrollViewer>();
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToBottom();
                }
            }
        }


		private int GetFirstVisibleItemIndex()
		{
            using (Lexplosion.QA.PerformanceTelemetry.Measure("library.visible-index.scan", 10))
            {
			var scrollViewer = InstanceList.FindVisualDescendant<ScrollViewer>();
			if (scrollViewer == null) return -1;

			// Получаем первый видимый элемент
			for (int i = 0; i < InstanceList.Items.Count; i++)
			{
				var container = InstanceList.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
				if (container != null && container.TransformToVisual(scrollViewer).Transform(new Point(0, 0)).Y >= 0)
				{
					return i; // Возвращаем индекс первого видимого элемента
				}
			}

			return -1; // Если ничего не найдено
            }
		}

		private void OnInstanceListLoaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Runtime.DebugWrite($"OnInstanceListLoaded", color: System.ConsoleColor.Red);
			if (InstanceList.Items.Count > 0)
			{
				if (_viewModel != null)
				{
					if (_viewModel.IsScrollToEnd)
					{
						var lastItem = InstanceList.Items[InstanceList.Items.Count - 1];
						InstanceList.ScrollIntoView(lastItem);
						return;
					}
				}

				if (posIndex > 0 && posIndex < InstanceList.Items.Count)
				{
					var item = InstanceList.Items[Math.Min(posIndex + 2, InstanceList.Items.Count - 1)];
					InstanceList.ScrollIntoView(item);

					// Ожидание рендеринга элемента
					InstanceList.Dispatcher.BeginInvoke(new Action(() =>
					{
						var container = InstanceList.ItemContainerGenerator.ContainerFromIndex(posIndex) as FrameworkElement;
						if (container != null)
						{
							container.BringIntoView();
						}
					}), DispatcherPriority.Render);
				}
			}
		}

		private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (BackTopButton.TargetScroll == null)
			{
				BackTopButton.TargetScroll = e.OriginalSource as ScrollViewer;
			}
		}


		private void CloseContextMenuWhenButtonClicked() 
		{
			(Resources["GroupItemContextMenu"] as ContextMenu).IsOpen = false;
        }


        private void Remove_Click(object sender, RoutedEventArgs e)
        {
			CloseContextMenuWhenButtonClicked();
        }
		
		private void Edit_Click(object sender, RoutedEventArgs e)
        {
            CloseContextMenuWhenButtonClicked();
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			if (_viewModel != null) 
			{
				_viewModel.Model.IsGroupDrawerOpen = false;
			}
        }

        private void Grid_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            DragDropField.Visibility = System.Windows.Visibility.Visible;
        }

        private void DragDropField_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            DragDropField.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void DragDropField_Drop(object sender, System.Windows.DragEventArgs e)
        {
            var fe = sender as FrameworkElement;

            fe.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
