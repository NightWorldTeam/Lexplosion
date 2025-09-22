using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public class LeftPanelViewModel : ViewModelBase
    {
        public AutoResetEvent WaitHandler = new AutoResetEvent(false);


        public event Action<ViewModelBase> SelectedItemChanged;


        #region Properties


        private ObservableCollection<LeftPanelMenuItem> _items = new ObservableCollection<LeftPanelMenuItem>();
        public IEnumerable<LeftPanelMenuItem> Items { get => _items; }


        private LeftPanelMenuItem _selectedItem;
        public LeftPanelMenuItem SelectedItem
        {
            get => _selectedItem; set
            {
                if (_selectedItem != null && _selectedItem != value) 
                {
                    _selectedItem.IsSelected = false;
                }
                _selectedItem = value;
                SelectedItemChanged?.Invoke(value.Content);
                OnPropertyChanged();
            }
        }


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


        #endregion Properties


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


        #endregion Commands


        #region Constructors


        public LeftPanelViewModel()
        {
            Account.LaunchAccountChanged += (acc) => SetUserDataToHeader();
            Account.ActiveAccountChanged += (acc) => SetUserDataToHeader();

            SetUserDataToHeader();
        }


        #endregion Constructors


        #region Public Methods


        public void AddTabItem(string name, string icon, ViewModelBase content, int id = -1, double iconWidth = 20, double iconHeight = 20)
        {
            if (id == -1 || id < 0)
            {
                id = _items.Count + 1;
            }

            var newTabItem = new LeftPanelMenuItem
            {
                Id = (uint)id,
                TextKey = name,
                Icon = icon,
                Content = content,
                IconWidth = iconWidth,
                IconHeight = iconHeight
            };

            newTabItem.SelectedEvent += OnSelectedTabItemChanged;

            _items.Add(newTabItem);
        }

        public void AddTabItem(LeftPanelMenuItem tabItem)
        {
            tabItem.SelectedEvent += OnSelectedTabItemChanged;
            _items.Add(tabItem);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public void AddTabItems(IEnumerable<LeftPanelMenuItem> em)
        {
            foreach (var tabItem in em)
            {
                AddTabItem(tabItem);
            }
        }

        /// <summary>
        /// Выбирает элемент по индексу в коллекции.
        /// Индексация как у обычной коллекции с нуля.
        /// </summary>
        /// <param name="index">Индекс элемента</param>
        public LeftPanelMenuItem SelectItem(int index)
        {
            if (SelectedItem != null)
                SelectedItem.IsSelected = false;
            _items[index].IsSelected = true;
            return _items[index];
        }

        public void SelectFirst()
        {
            if (SelectedItem != null)
                SelectedItem.IsSelected = false;

            _items[0].IsSelected = true;
        }

        public void SelectLast()
        {
            if (SelectedItem != null)
                SelectedItem.IsSelected = false;

            _items[_items.Count - 1].IsSelected = true;
        }

        public LeftPanelMenuItem GetByContentType(Type type)
        {
            return _items.FirstOrDefault(t => t.Content.GetType() == type);
        }


        #endregion Public Methods


        #region Private Methods


        private void OnSelectedTabItemChanged(LeftPanelMenuItem instance)
        {
            SelectedItem = instance;
        }

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
