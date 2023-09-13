using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using System;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileSettingsLayoutViewModel : ContentLayoutViewModelBase
    {
        private ViewModelBase _settingsViewModel = null;
        private ViewModelBase _aboutViewModel = null;
        private ViewModelBase _configurationViewModel = null;

        public InstanceProfileSettingsLayoutViewModel(InstanceModelBase instanceModelBase) : base()
        {
            InitAddonsTabMenu(instanceModelBase);
        }

        private void InitAddonsTabMenu(InstanceModelBase instanceModelBase)
        {
            HeaderKey = "Settings";

            _settingsViewModel = new InstanceProfileSettingsViewModel(instanceModelBase);
            _aboutViewModel = new InstanceProfileAboutViewModel(instanceModelBase);
            _configurationViewModel = new InstanceProfileConfigurationViewModel(instanceModelBase);

            _tabs.Add(new TabItemModel { TextKey = "General", Content = _settingsViewModel, IsSelected = true });
            _tabs.Add(new TabItemModel { TextKey = "AboutInstance", Content = _aboutViewModel });
            _tabs.Add(new TabItemModel { TextKey = "Configuration", Content = _configurationViewModel });
        }
    }
}
