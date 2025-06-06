using Lexplosion.WPF.NewInterface.Extensions;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.MainMenu
{
	/// <summary>
	/// Логика взаимодействия для LibraryView.xaml
	/// </summary>
	public partial class LibraryView : System.Windows.Controls.UserControl
	{
		LibraryViewModel _viewModel;
		static int posIndex = 0;
		bool _isFilterHidden = false;

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

			_viewModel = (LibraryViewModel)DataContext;

			if (_viewModel == null)
			{
				posIndex = GetFirstVisibleItemIndex();
				Runtime.DebugWrite($"Pos: {posIndex}", color: System.ConsoleColor.Red);
				return;
			}

			_viewModel.InstanceProfileOpened += (instanceModel) =>
			{
				posIndex = GetFirstVisibleItemIndex();
			};

			InstanceModelBase.GlobalDeletedEvent += InstanceModelBase_GlobalDeletedEvent;
		}

		private void InstanceModelBase_GlobalDeletedEvent(InstanceModelBase obj)
		{
			//if (_viewModel.Model.InstancesCollectionViewSource.Count == 1)
			//{
			//	Runtime.DebugWrite("Clear VirtualizingWrapPanel cache");
			//	var panel = InstanceList.FindVisualDescendant<VirtualizingWrapPanel>();
			//	panel.ClearItemSizeCache();
			//}
		}

		private int GetFirstVisibleItemIndex()
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
					var item = InstanceList.Items[posIndex + 2];
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
    }
}
