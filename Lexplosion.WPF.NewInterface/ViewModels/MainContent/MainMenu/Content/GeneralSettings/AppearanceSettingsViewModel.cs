using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.MainContent.Content;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
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
