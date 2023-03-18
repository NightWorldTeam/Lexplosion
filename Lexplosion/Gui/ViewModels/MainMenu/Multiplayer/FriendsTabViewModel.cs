using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Documents;

namespace Lexplosion.Gui.ViewModels.MainMenu.Multiplayer
{
    public class Friend 
    {
        public string Name { get; }
        public int ManualFriendsCount { get; }
        public Image Avatar { get; }
        public ActivityStatus Status { get; }

        public Friend(string name, int friendsCount, ActivityStatus status)
        {
            Runtime.DebugWrite(status);
            Name = name;
            ManualFriendsCount = friendsCount;
            Status = status;
        }
    }

    public class FriendsTabModel : VMBase 
    {
        public ObservableCollection<Friend> Friends { get; } = new ObservableCollection<Friend>();

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

        public FriendsTabViewModel()
        {
            Model = new FriendsTabModel();
        }
    }
}
