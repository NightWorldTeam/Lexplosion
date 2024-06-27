using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Stores;
using System.Linq;

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
            AddTabItem(new TabItemModel("general", "General", _generalSettingsViewModel, true));
            SelectedItem = Tabs.First();
            AddTabItem(new TabItemModel("appearance", "Appearance", _appearanceViewModel));
            AddTabItem(new TabItemModel("language", "Language", _languageViewModel));
            AddTabItem(new TabItemModel("accounts", "Accounts", _accountsViewModel));
            AddTabItem(new TabItemModel("about", "About", _aboutViewModel));
        }
    }
}
