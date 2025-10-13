using Lexplosion.UI.WPF.Core.Objects.Users;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.Profile
{
    /// <summary>
    /// Interaction logic for ProfileCosmeticsView.xaml
    /// </summary>
    public partial class ProfileCosmeticsView : UserControl
    {
        public ProfileCosmeticsView()
        {
            InitializeComponent();
            //var collectionViewSource = (Resources["FriendsViewSource"] as CollectionViewSource);
            //var groupBy = new PropertyGroupDescription("GroupNameKey");
            ////groupBy.CustomSort = new FriendStatusComparer();
            //collectionViewSource.GroupDescriptions.Add(groupBy);
        }
    }

    public class FriendStatusComparer : IComparer
    {
        public int Compare(object obj1, object obj2)
        {
            if (!(obj1 is CollectionViewGroup group1))
                return 0;

            if (!(obj2 is CollectionViewGroup group2))
                return 0;


            var status1 = group1.Name as NightWorldUserStatus;
            var status2 = group2.Name as NightWorldUserStatus;

            if (status1.Priority > status2.Priority)
                return -1;
            else if (status1.Priority < status2.Priority)
                return 1;

            return 0;
        }
    }
}
