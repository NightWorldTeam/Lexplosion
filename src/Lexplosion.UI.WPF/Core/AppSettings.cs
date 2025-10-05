using Lexplosion.Logic;
using Lexplosion.Logic.Management;
using Lexplosion.UI.WPF.Core.Services;

namespace Lexplosion.UI.WPF.Core
{
    public class AppSettings
    {
        public AppColorThemeService ThemeService { get; set; }

        public readonly Settings Core;
        public readonly AllServicesContainer ServiceContainer;

        public AppSettings(AllServicesContainer serviceContainer, Settings settingsCore, AppResources resources)
        {
            ServiceContainer = serviceContainer;
            Core = settingsCore;
            ThemeService = new(serviceContainer, settingsCore);
        }

        public void SaveCore() 
        {
            ServiceContainer.DataFilesService.SaveSettings(Core);
        }
    }
}
