using System;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.AddonsRepositories.Lexplosion
{
    /// <summary>
    /// Interaction logic for AddonRepositoryCatalogView.xaml
    /// </summary>
    public partial class AddonRepositoryCatalogView : UserControl
    {
        public event Action PaginationChanged;

        public AddonRepositoryCatalogView()
        {
            InitializeComponent();
        }

        private void Paginator_PageChanged(uint obj)
        {
            PaginationChanged?.Invoke();
        }
    }
}
