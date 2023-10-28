using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.Content.GeneralSettings;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class AppearanceSettingsViewModel : ViewModelBase
    {
        public AppearanceSettingsModel Model { get; }

        public AppearanceSettingsViewModel()
        {
            Model = new AppearanceSettingsModel();
        }
    }
}
