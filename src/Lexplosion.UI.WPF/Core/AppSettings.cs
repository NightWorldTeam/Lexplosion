using Lexplosion.Logic;
using Lexplosion.Logic.Management;
using Lexplosion.UI.WPF.Core.Services;
using System;

namespace Lexplosion.UI.WPF.Core
{
    public class AppSettings
    {
        public event Action<string> SettingsFieldChanged;

        public AppColorThemeService ThemeService { get; set; }

        public readonly Settings Core;
        public readonly AppServiceContainer ServiceContainer;

        public AppSettings(AppServiceContainer serviceContainer, Settings settingsCore, AppResources resources)
        {
            ServiceContainer = serviceContainer;
            Core = settingsCore;
            ThemeService = new(serviceContainer, settingsCore);

            Core.ValueChanged += (str) => SettingsFieldChanged?.Invoke(str);
        }

        public void SaveCore() 
        {
            ServiceContainer.DataFilesService.SaveSettings(Core);
        }
    }
}
