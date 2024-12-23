﻿using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для FriendsViewModel.xaml
    /// </summary>
    public partial class FriendsViewModel : UserControl
    {
        public FriendsViewModel()
        {
            InitializeComponent();
            var collectionViewSource = (Resources["FriendsViewSource"] as CollectionViewSource);
            var groupBy = new PropertyGroupDescription("Status");
            groupBy.CustomSort = new FriendStatusComparer();
            collectionViewSource.GroupDescriptions.Add(groupBy);
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


            var status1 = group1.Name as FriendStatus;
            var status2 = group2.Name as FriendStatus;

            if (status1.Priority > status2.Priority)
                return -1;
            else if (status1.Priority < status2.Priority)
                return 1;

            return 0;
        }
    }
}
