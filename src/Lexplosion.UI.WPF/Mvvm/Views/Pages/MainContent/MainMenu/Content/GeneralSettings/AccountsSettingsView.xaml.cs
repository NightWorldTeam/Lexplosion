using Lexplosion.UI.WPF.Core.Objects;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для AccountsSettingsView.xaml
    /// </summary>
    public partial class AccountsSettingsView : UserControl
    {
        public AccountsSettingsView()
        {
            InitializeComponent();
            var collectionViewSource = (Resources["AccountSourceView"] as CollectionViewSource);
            var groupBy = new PropertyGroupDescription("Source");
            groupBy.CustomSort = new AccountSourceComparer();
            collectionViewSource.GroupDescriptions.Add(groupBy);
        }


        public class AccountSourceComparer : IComparer
        {
            public int Compare(object obj1, object obj2)
            {
                if (!(obj1 is CollectionViewGroup group1))
                    return 0;

                if (!(obj2 is CollectionViewGroup group2))
                    return 0;


                var source1 = group1.Name as AccountSource;
                var source2 = group2.Name as AccountSource;

                if (source1.Type > source2.Type)
                    return 1;
                else if (source1.Type < source2.Type)
                    return -1;

                return 0;
            }
        }
    }
}
