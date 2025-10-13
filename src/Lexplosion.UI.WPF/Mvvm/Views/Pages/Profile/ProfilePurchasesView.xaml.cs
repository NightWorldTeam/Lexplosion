using Lexplosion.UI.WPF.Mvvm.ViewModels.Profile;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.Profile
{
    /// <summary>
    /// Interaction logic for ProfilePurchasesView.xaml
    /// </summary>
    public partial class ProfilePurchasesView : UserControl
    {
        public ProfilePurchasesView()
        {
            InitializeComponent();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == nameof(PurchaseOrder.Status))
            {
                var column = e.Column;
                // Устанавливаем стиль для ячеек
                column.CellStyle = (Style)FindResource("StatusCellStyle");

                e.Column = column;
            }
        }
    }
}
