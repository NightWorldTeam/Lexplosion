using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class GeneralSettingsLayoutViewModel : ContentLayoutViewModelBase
    {
        private readonly ViewModelBase _generalSettingsViewModel;
        private readonly ViewModelBase _appearanceViewModel = new AppearanceSettingsViewModel();
        private readonly ViewModelBase _languageViewModel = new LanguageSettingsViewModel();
        private readonly ViewModelBase _accountsViewModel;
        private readonly ViewModelBase _aboutViewModel = new AboutUsViewModel();

        public GeneralSettingsLayoutViewModel(AppCore appCore) : base()
        {
            _accountsViewModel = new AccountsSettingsViewModel(appCore);

            _generalSettingsViewModel = new GeneralSettingsViewModel(appCore);

            InitDefaultSettingsTabMenu();
        }

        private void InitDefaultSettingsTabMenu()
        {
            AddTabItem(new TabItemModel("General", _generalSettingsViewModel, true));
            SelectedItem = Tabs.First();
            AddTabItem(new TabItemModel("Appearance", _appearanceViewModel));
            AddTabItem(new TabItemModel("Language", _languageViewModel));
            AddTabItem(new TabItemModel("Accounts", _accountsViewModel));
            AddTabItem(new TabItemModel("About", _aboutViewModel));
        }
    }
}
