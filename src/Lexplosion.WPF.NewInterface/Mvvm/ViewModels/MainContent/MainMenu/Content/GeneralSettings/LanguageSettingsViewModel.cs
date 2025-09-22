using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.Content.GeneralSettings;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
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
