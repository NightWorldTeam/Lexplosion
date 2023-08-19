using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Models.MainContent.Content;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
