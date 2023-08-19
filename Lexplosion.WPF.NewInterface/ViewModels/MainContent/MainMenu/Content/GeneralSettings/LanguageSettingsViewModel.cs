using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.MainContent.Content.GeneralSettings;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public sealed class LanguageSettingsViewModel : ViewModelBase
    {
        public LanguageSettingsModel Model { get; }

        public LanguageSettingsViewModel()
        {
            Model = new LanguageSettingsModel();
        }
    }
}
