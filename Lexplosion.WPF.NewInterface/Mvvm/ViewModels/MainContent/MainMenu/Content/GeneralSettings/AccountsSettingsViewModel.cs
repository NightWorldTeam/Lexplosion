using Lexplosion.WPF.NewInterface.Core;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class AccountsSettingsModel : ViewModelBase
    {

    }

    public class AccountsSettingsViewModel : ViewModelBase
    {
        public AccountsSettingsModel Model { get; }


        public AccountsSettingsViewModel()
        {
            Model = new AccountsSettingsModel();
        }
    }
}
