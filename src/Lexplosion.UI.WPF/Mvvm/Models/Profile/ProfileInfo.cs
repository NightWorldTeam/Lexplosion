using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Collections.ObjectModel;

namespace Lexplosion.UI.WPF.Mvvm.Models.Profile
{
    // Обычная модель данных описывающая профиль пользователя не привязанная к фреймворкам
    public class ProfileInfo : ObservableObject
    {
        public string Name { get; }
        public ProfileTitle Title { get; }
        public float HoursCount { get; }
        public string AccountAge { get; }

        private int _friendsCount;
        public int FriendsCount
        {
            get => _friendsCount; set
            {
                _friendsCount = value;
                OnPropertyChanged();
            }
        }

        private string _summary;
        public string Summary
        {
            get => _summary; set
            {
                _summary = value.Replace('\t', ' ');
                OnPropertyChanged();
            }
        }

        private ActivityStatus _status;

        public ProfileInfo(string name, ProfileTitle title, float hoursCount, string accountAge, int friendsCount, string summary, ActivityStatus status)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Title = title;
            HoursCount = hoursCount;
            AccountAge = accountAge ?? throw new ArgumentNullException(nameof(accountAge));
            FriendsCount = friendsCount;
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Status = status;
        }

        public ActivityStatus Status
        {
            get => _status; set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ProfileSocialMedia> SocialMedia { get; } = new();
    }
}
