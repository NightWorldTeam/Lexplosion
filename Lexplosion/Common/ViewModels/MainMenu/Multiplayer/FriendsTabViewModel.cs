using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Lexplosion.Common.ViewModels.MainMenu.Multiplayer
{
    public abstract class Person
    {
        public string Name { get; }
        public int ManualFriendsCount { get; }
        public Image Avatar { get; }
        public ActivityStatus Status { get; }

        public Person(string name, int friendsCount, ActivityStatus status)
        {
            Runtime.DebugWrite(status);
            Name = name;
            ManualFriendsCount = friendsCount;
            Status = status;
        }
    }



    public sealed class Friend : Person
    {
        public Friend(string name, int friendsCount, ActivityStatus status) : base(name, friendsCount, status)
        {

        }
    }

    public sealed class PotentialFriend : Person
    {
        public PotentialFriend(string name, int friendsCount, ActivityStatus status) : base(name, friendsCount, status)
        {
        }
    }

    public class FriendsTabModel : VMBase
    {
        public ObservableCollection<Friend> Friends { get; } = new ObservableCollection<Friend>();
        //public ObservableCollection<>

        public FriendsTabModel()
        {
            Random random = new Random();
            var nicks = new List<string> { "_Hel2x_", "VagOne", "Andrysha", "Sklaip", "Petya", "Eblan Kakoyta" };


            foreach (var nick in nicks)
            {
                Friends.Add(new Friend(nick, random.Next(1, 40), (ActivityStatus)random.Next(0, 4)));
            }
        }
    }

    public class FriendsTabViewModel : VMBase
    {
        public FriendsTabModel Model { get; }


        #region Commands

        /// <summary>
        /// Добавление друга, в качестве агрумента obj, будет передаваться ссылка на объект друга.
        /// </summary>
        private RelayCommand _addFriendCommand;
        public RelayCommand AddFriendCommand
        {
            get => _addFriendCommand ?? (_addFriendCommand = new RelayCommand(obj =>
            {
                if (obj is PotentialFriend)
                {

                }
            }));
        }

        #endregion


        public FriendsTabViewModel()
        {
            Model = new FriendsTabModel();
        }
    }
}
