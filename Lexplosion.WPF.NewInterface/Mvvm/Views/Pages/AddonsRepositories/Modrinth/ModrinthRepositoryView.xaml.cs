using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.AddonsRepositories
{
    /// <summary>
    /// Логика взаимодействия для ModrinthRepositoryView.xaml
    /// </summary>
    public partial class ModrinthRepositoryView : UserControl
    {
        public ModrinthRepositoryView()
        {
            InitializeComponent();
        }

        private void AddonsListScrollBar_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //var sv = (ScrollViewer)sender;
            //CategoriesScrollViewer.ScrollToVerticalOffset(sv.VerticalOffset);
        }

        private void CategoriesList_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CategoriesScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }

        private void CategoriesList_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CategoriesScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }
    }
}
