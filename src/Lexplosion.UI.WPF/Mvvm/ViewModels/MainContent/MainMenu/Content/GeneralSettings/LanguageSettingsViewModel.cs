using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.Content.GeneralSettings;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
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
