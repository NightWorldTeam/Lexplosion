using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.InstanceProfile
{
    /// <summary>
    /// Логика взаимодействия для InstanceProfileAddonsView.xaml
    /// </summary>
    public partial class InstanceProfileAddonsView : UserControl
    {
        public InstanceProfileAddonsView()
        {
            InitializeComponent();
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
            Console.WriteLine(e.Data);

            fe.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
