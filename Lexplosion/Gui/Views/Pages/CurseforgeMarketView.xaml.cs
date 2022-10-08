using System.Windows.Controls;

namespace Lexplosion.Gui.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для CurseforgeMarketView.xaml
    /// </summary>
    public partial class CurseforgeMarketView : UserControl
    {

        public CurseforgeMarketView()
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
