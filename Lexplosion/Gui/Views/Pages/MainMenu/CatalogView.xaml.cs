using System.Windows.Controls;

namespace Lexplosion.Gui.Views.Pages.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для CatalogView.xaml
    /// </summary>
    public partial class CatalogView : UserControl
    {
        public CatalogView()
        {
            InitializeComponent();
        }

        private void ContainerPage_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var viewer = (ScrollViewer)sender;
            try
            {
                var onScrollCommand = Lexplosion.Gui.Extension.ScrollViewer.GetOnScrollCommand(viewer);
                onScrollCommand.Execute(null);
            }
            catch
            {

            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltersDropdownMenu.IsOpen = false;
        }
    }
}
