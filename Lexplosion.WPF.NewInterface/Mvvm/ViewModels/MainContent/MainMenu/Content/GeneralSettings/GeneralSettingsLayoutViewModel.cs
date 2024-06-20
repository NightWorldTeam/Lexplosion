﻿using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Stores;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class GeneralSettingsLayoutViewModel : ContentLayoutViewModelBase
    {
        private readonly ViewModelBase _generalSettingsViewModel = new GeneralSettingsViewModel();
        private readonly ViewModelBase _appearanceViewModel = new AppearanceSettingsViewModel();
        private readonly ViewModelBase _languageViewModel = new LanguageSettingsViewModel();
        private readonly ViewModelBase _accountsViewModel;
        private readonly ViewModelBase _aboutViewModel = new AboutUsViewModel();

        public GeneralSettingsLayoutViewModel(ModalNavigationStore modalNavigationStore) : base()
        {
            _accountsViewModel = new AccountsSettingsViewModel(modalNavigationStore);

            InitDefaultSettingsTabMenu();
        }

        private void InitDefaultSettingsTabMenu()
        {
            _tabs.Add(new TabItemModel { TextKey = "General", Content = _generalSettingsViewModel, IsSelected = true });
            _tabs.Add(new TabItemModel { TextKey = "Appearance", Content = _appearanceViewModel });
            _tabs.Add(new TabItemModel { TextKey = "Language", Content = _languageViewModel });
            _tabs.Add(new TabItemModel { TextKey = "Accounts", Content = _accountsViewModel });
            _tabs.Add(new TabItemModel { TextKey = "About", Content = _aboutViewModel });
        }
    }
}
