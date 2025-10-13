using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Mvvm.Models.Profile;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Profile
{
    public sealed class ProfileLayoutViewModel : ViewModelBase
    {
        private ProfileInfo _profileInfo;

        public IList<TabItemModel> Tabs { get; } = new ObservableCollection<TabItemModel>();

        public ViewModelBase Content { get; set; }

        public ProfileLayoutViewModel(AppCore appCore)
        {
            // Load data here?
            _profileInfo = new(
                "_Hel2x_",
                new("dotnet", 0xFF00ffb5),
                50000.2f,
                "Долго",
                13,
                "The European languages are members of the same family. Their separate existence is a myth. For science, music, sport, etc, Europe.",
                ActivityStatus.NotDisturb
            );

            _profileInfo.SocialMedia.Add(new ProfileSocialMedia("VK", "https://vk.com/idhel2x", "VKontakte"));
            _profileInfo.SocialMedia.Add(new ProfileSocialMedia("Youtube", "https://vk.com/idhel2x", "Youtube"));
            _profileInfo.SocialMedia.Add(new ProfileSocialMedia("Discord", "https://vk.com/idhel2x", "Discord"));

            Tabs.Add(new()
            {
                Id = 0,
                Content = new ProfileViewModel(_profileInfo),
                IsSelected = true,
                TextKey = "Profile"
            });
            Tabs.Add(new()
            {
                Id = 1,
                Content = new FriendsLayoutViewModel(appCore),
                TextKey = "Friends"
            });
            Tabs.Add(new()
            {
                Id = 2,
                Content = new ProfileCosmeticsViewModel(),
                TextKey = "Diamond"
            });
            Tabs.Add(new()
            {
                Id = 3,
                Content = new ProfileSettingsViewModel(_profileInfo),
                TextKey = "Settings"
            });
            Tabs.Add(new()
            {
                Id = 4,
                Content = new ProfilePurchasesViewModel(),
                TextKey = "AccountWallet"
            });

            foreach (var i in Tabs)
            {
                i.SelectedChanged += OnTabSelectedChanged;
            }

            Content = Tabs[0].Content;
            OnPropertyChanged(nameof(Content));
        }

        private void OnTabSelectedChanged(TabItemModel arg1, bool arg2)
        {
            if (arg2)
            {
                Content = arg1.Content;
                OnPropertyChanged(nameof(Content));
            }
        }
    }
}
