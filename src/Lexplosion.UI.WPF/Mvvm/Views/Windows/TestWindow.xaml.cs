using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using static Lexplosion.Logic.Objects.Curseforge.CurseforgeProjectInfo;

namespace Lexplosion.UI.WPF.Mvvm.Views.Windows
{
    public class ObjModel
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        private Rectangle dragVisual;

        private bool isDragging;
        private ObjModel draggedModel;
        private object draggedItem;

        private readonly ObservableCollection<ObjModel> list = new ObservableCollection<ObjModel>();

        public TestWindow()
        {
            InitializeComponent();

            for (var i = 0; i < 1000; i++) 
            {
                list.Add(new() { Name = $"Obj {i}" });
            }
            CurrentListBox.ItemsSource = list;
        }

        private void Moving(DependencyObject element)
        {
            object dataContext = draggedModel;

            DragDropEffects dragDropResult = DragDrop.DoDragDrop(element, new DataObject(DataFormats.Serializable, dataContext), DragDropEffects.Move);

            if (dragDropResult == DragDropEffects.None)
            {

            }
        }

        private void Insert(ObjModel insertedTodoItem, ObjModel targetTodoItem)
        {
            if (insertedTodoItem == targetTodoItem)
            {
                return;
            }

            int oldIndex = list.IndexOf(insertedTodoItem);
            int nextIndex = list.IndexOf(targetTodoItem);

            if (oldIndex != -1 && nextIndex != -1)
            {
                list.Move(oldIndex, nextIndex);
            }
        }

        private void ListViewItem_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
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

        private void ListViewItem_DragOver(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var targetTodoItem = (ObjModel)element.DataContext;
                var insertedTodoItem = (ObjModel)e.Data.GetData(DataFormats.Serializable);

                Insert(insertedTodoItem, targetTodoItem);
            }
        }

        private void ListViewItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is ObjModel model)
            {
                draggedModel = model;
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {

        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            (sender as Grid).DataContext = draggedModel;
        }
    }
}


//private void Hex_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
//{
//try
//{
//var newColor = (Color)ColorConverter.ConvertFromString(hex.Text);
//    object latestColor = Color.FromRgb(22, 127, 252); // App.Current.Resources["ActivityColor"] ?? ;
//    var updatedColor = (Color)ColorConverter.ConvertFromString(hex.Text);
//    var intervalColors = Gradient.GenerateGradient((Color)latestColor, updatedColor, 50); //ColorTools.GetIntervalColor((Color)latestColor, (Color)ColorConverter.ConvertFromString(hex.Text), 50);

//    Runtime.TaskRun(() =>
//    { 
//        var i = 0;
//        foreach (var newColor in intervalColors) 
//        {
//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine($"{i}. {newColor.ToString()}");
//            App.Current.Dispatcher.Invoke(() => { 
//                App.Current.Resources["DefaultButtonBackgroundColor"] = newColor;
//                App.Current.Resources["DefaultButtonBackgroundColorBrush"] = new SolidColorBrush(newColor);
//            });
//            App.Current.Resources["HoverAccentColor1"] = ColorTools.GetDarkerColor(newColor, 10);
//            App.Current.Resources["HoverAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["HoverAccentColor1"]);
//            App.Current.Resources["PressedAccentColor1"] = ColorTools.GetDarkerColor(newColor, 20);
//            App.Current.Resources["PressedAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["PressedAccentColor1"]);
//            App.Current.Resources["DisableAccentColor1"] = ColorTools.GetDarkerColor(newColor, 70);
//            App.Current.Resources["DisableAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["DisableAccentColor1"]);

//            App.Current.Resources["ForegroundAccentColor1"] = ColorTools.ForegroundByColor(newColor);
//            App.Current.Resources["ForegroundAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["ForegroundAccentColor1"]);
//            Thread.Sleep(10);
//            i++;
//        }
//        Thread.Sleep(10);
//        App.Current.Dispatcher.Invoke(() => {
//            App.Current.Resources["DefaultButtonBackgroundColor"] = updatedColor;
//            App.Current.Resources["DefaultButtonBackgroundColorBrush"] = new SolidColorBrush(updatedColor);
//        });
//    });
//    Console.ForegroundColor = ConsoleColor.White;
//    Console.WriteLine(App.Current.Resources["AccentColor1"]);
//}
//catch (Exception ea) 
//{
//    Console.ForegroundColor = ConsoleColor.Red;
//    Console.WriteLine(ea);
//    Console.ForegroundColor = ConsoleColor.White;
//}

// #13f287
// #167FFC
//}

