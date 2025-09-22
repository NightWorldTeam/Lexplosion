using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.Content.GeneralSettings;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class AppearanceSettingsViewModel : ViewModelBase
    {
        public AppearanceSettingsModel Model { get; }

        public AppearanceSettingsViewModel(AppCore appCore)
        {
            Model = new AppearanceSettingsModel(appCore);
        }
    }
}
