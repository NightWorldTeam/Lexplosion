using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Converters;
using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Profile
{
    public class TestConverter : ConverterBase<TestConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Console.WriteLine(value);
            return value;
        }
    }

    public enum FinanceOperationStatus 
    {
        Finished,
        Waiting,
        Cancelled,
    }

    public class PurchaseOrder : ObservableObject
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime OperationDate { get; set; } = DateTime.Now;
        public FinanceOperationStatus Status { get; set; }
    }

    public class ProfilePurchasesModel : ObservableObject
    {
        public ObservableCollection<PurchaseOrder> Orders { get; } = new();

        public ProfilePurchasesModel()
        {
            for (var i = 0; i < 1000; i++)
            {
                Orders.Add(new()
                {
                    Name = $"Подписка Eclipse {i}",
                    OperationDate = DateTime.Now.AddHours(i),
                    Price = 1000.0M + i
                });
            }
        }
    }

    public sealed class ProfilePurchasesViewModel : ViewModelBase
    {
        public ProfilePurchasesModel Model { get; }

        public ProfilePurchasesViewModel()
        {
            Model = new();
        }
    }
}
