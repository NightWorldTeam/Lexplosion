using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.UI.WPF.Commands;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public class LeftPanelViewModel : LeftPanelViewModelBase
    {
        #region Header Data


        //TODO: вынести header в отдельный компонетн.

        public NwUserBanner ProfileBanner { get; private set; }

        private string _userLogin = "Unknown";
        public string UserLogin
        {
            get => _userLogin; private set
            {
                _userLogin = value;
                OnPropertyChanged();
            }
        }

        public bool IsUserAvatarLoaded { get; private set; } = false;

        private string _userAvatar;// = "pack://Application:,,,/Assets/images/icons/non_image1.png";
        public string UserAvatar
        {
            get => _userAvatar; private set
            {
                _userAvatar = value;

                if (!string.IsNullOrEmpty(_userAvatar))
                {
                    IsUserAvatarLoaded = true;
                    OnPropertyChanged(nameof(IsUserAvatarLoaded));
                }
                OnPropertyChanged();
            }
        }

        private AccountType _userAccountType = AccountType.NoAuth;
        public AccountType UserAccountType
        {
            get => _userAccountType; private set
            {
                _userAccountType = value;
                OnPropertyChanged();
            }
        }


        #endregion Header Data

        #region Commands


        private RelayCommand _toUserHowToPlayGuideCommand;
        public ICommand ToUserHowToPlayGuideCommand
        {
            get => RelayCommand.GetCommand(ref _toUserHowToPlayGuideCommand, () =>
            {
                try
                {
                    System.Diagnostics.Process.Start("https://vk.com/@nightworld_offical-instrukciya-k-launcheru-lexplosion");
                }
                catch { }
            });
        }

        private RelayCommand _toSupportCommand;
        public ICommand ToSupportCommand
        {
            get => RelayCommand.GetCommand(ref _toSupportCommand, () =>
            {
                try
                {
                    System.Diagnostics.Process.Start("https://vk.com/im?media=&sel=-155979422");
                }
                catch { }
            });
        }

        public ICommand ViewProfileCommand { get; }

        private RelayCommand _selectAccountCommand;
        public ICommand SelectAccountCommand 
        {
            get => RelayCommand.GetCommand<Account>(ref _selectAccountCommand, (acc) =>
            {
                if (acc.AccountType == AccountType.NightWorld)
                {
                    acc.IsActive = true;
                    acc.IsLaunch = true;
                }
                else 
                {
                    acc.IsLaunch = true; ;
                }

                Account.SaveAll();
            });
        }

        #endregion Commands


        #region Constructors


        public LeftPanelViewModel(ICommand toProfile)
        {
            ViewProfileCommand = toProfile;

            Account.LaunchAccountChanged += (acc) => SetUserDataToHeader();
            Account.ActiveAccountChanged += (acc) => SetUserDataToHeader();

            SetUserDataToHeader();
        }


        #endregion Constructors


        #region Private Methods


        private void SetUserDataToHeader()
        {
            if (Account.ActiveAccount != null)
            {
                UserLogin = Account.ActiveAccount.Login;
                UserAvatar = Account.ActiveAccount.HeadImageUrl;
                UserAccountType = AccountType.NightWorld;
                ProfileBanner = Account.ActiveAccount.ProfileBanner;
                OnPropertyChanged(nameof(ProfileBanner));
                return;
            }

            if (Account.LaunchAccount != null)
            {
                UserLogin = Account.LaunchAccount.Login;
                UserAvatar = Account.LaunchAccount.HeadImageUrl;
                UserAccountType = Account.LaunchAccount.AccountType;
                return;
            }

            UserLogin = "Unknown";
            UserAvatar = "pack://Application:,,,/Assets/images/icons/non_image1.png";
            UserAccountType = AccountType.NoAuth;
        }


        #endregion Private Methods
    }
}