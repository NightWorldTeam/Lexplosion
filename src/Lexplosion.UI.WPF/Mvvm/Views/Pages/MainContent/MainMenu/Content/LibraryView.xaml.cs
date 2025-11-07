using Lexplosion.UI.WPF.Extensions;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

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

            (_viewModel.Model.InstanceController.Instances as INotifyCollectionChanged).CollectionChanged += OnInstanceListChanged;


            _viewModel.InstanceProfileOpened += (instanceModel) =>
            {
                posIndex = GetFirstVisibleItemIndex();
            };

            InstanceModelBase.GlobalDeletedEvent += InstanceModelBase_GlobalDeletedEvent;

            scrollTimer = new DispatcherTimer();
            scrollTimer.Interval = TimeSpan.FromMilliseconds(50);
            scrollTimer.Tick += ScrollTimer_Tick;
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

        private void Grid_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files != null)
            {
                if (files.All(file => IsImportFile(file)))
                {
                    DragDropField.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private bool IsImportFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var fileExt = System.IO.Path.GetExtension(filePath);

            if (_viewModel.Model.AvailableImportFileExtensions.Any(ext => ext.ToLower() == fileExt.ToLower()))
            {
                return true;
            }

            return false;
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

        private Rectangle dragVisual;

        private bool isDragging;
        private InstanceModelBase draggedModel;
        private object draggedItem;


        private void InstancesListListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            //_viewModel
            if (draggedModel == null)
            {
                return;
            }

            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                return;
            }

            if (sender is DependencyObject dpObj)
            {
                Moving(dpObj);
            }
        }

        private void InstancesListListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is InstanceModelBase model)
            {
                draggedModel = model;
            }
        }

        private void InstancesListListBoxItem_DragOver(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element)
            {

                var target = (InstanceModelBase)element.DataContext;
                var inserted = (InstanceModelBase)e.Data.GetData(DataFormats.Serializable);


                // Получаем позицию курсора относительно ListBox
                Point mousePos = Mouse.GetPosition(InstanceList);
                var screenPos = this.TranslatePoint(mousePos, InstanceList);

                // Проверяем, находится ли курсор в зоне прокрутки

                var container = InstanceList;
                double tolerance = 60;
                double verticalPos = e.GetPosition(container).Y;
                double offset = App.Current.MainWindow.ActualHeight * 0.03787;

                if (verticalPos < tolerance) // Top of visible list? 
                {
                    //Scroll up
                    ScrollListBox(-offset);
                }
                else if (verticalPos > container.ActualHeight - tolerance)
                {
                    //Scroll down
                    ScrollListBox(offset);
                }


                Insert(inserted, target);
            }
        }

        private void Moving(DependencyObject element)
        {
            object dataContext = draggedModel;

            scrollTimer.Start();
            isDragging = true;
            dragDropContentControl.Content = dataContext;
            DragDropEffects dragDropResult = DragDrop.DoDragDrop(element, new DataObject(DataFormats.Serializable, dataContext), DragDropEffects.Move);

            if (dragDropResult == DragDropEffects.None)
            {

            }

            dragDropContentControl.Content = null;
            scrollTimer.Stop();
        }

        private DispatcherTimer scrollTimer;
        private const double scrollZoneHeight = 50; // высота зоны прокрутки
        private const double scrollStep = 10;       // шаг прокрутки

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            if (!isDragging) return;


        }

        private void ScrollListBox(double offset)
        {
            // Ищем ScrollViewer внутри ListBox
            ScrollViewer scrollViewer = FindScrollViewer(InstanceList);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset);
            }
        }

        private ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is ScrollViewer)
                {
                    return (ScrollViewer)child;
                }
                else
                {
                    var result = FindScrollViewer(child);
                    if (result != null) return result;
                }
            }
            return null;
        }

        private void Insert(InstanceModelBase inserted, InstanceModelBase target)
        {
            _viewModel.Model.Insert(inserted, target);
        }
    }
}
