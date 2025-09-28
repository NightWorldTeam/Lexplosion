using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.Profile;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Profile
{
    public enum FriendListVisibilityOptions 
    {
        ForEveryone,
        ToFriends,
        ToNobody
    }

    public class ProfileSettings 
    {
        public FriendListVisibilityOptions SelectedFriendListVisibilityOption { get; }
    }

    public sealed class ProfileSettingsModel : ObservableObject 
    {
        public ProfileInfo Info { get; }

        public IList<string> Covers { get; } = new ObservableCollection<string>();

        public ProfileSettingsModel(ProfileInfo info)
        {
            Info = info;
            var path = "https://night-world.org/assets/img/profileCovers/";
            Covers.Add($"https://night-world.org/assets/img/profileCovers/{0}.jpeg");
            for (var i = 1; i < 8; i++) 
            {
                Covers.Add($"https://night-world.org/assets/img/profileCovers/{i}.jpg");
            }
        }
    }

    public sealed class ProfileSettingsViewModel : ViewModelBase
    {
        public ProfileSettingsModel Model { get; }

        public ProfileSettingsViewModel(ProfileInfo profileInfo)
        {
            Model = new(profileInfo);
        }
    }
}
