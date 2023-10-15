using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileSettingsLayoutViewModel : ContentLayoutViewModelBase
    {
        private const string HEADER_KEY = "Settings";

        private ViewModelBase _settingsViewModel = null;
        private ViewModelBase _aboutViewModel = null;
        private ViewModelBase _configurationViewModel = null;

        public InstanceProfileSettingsLayoutViewModel(InstanceModelBase instanceModelBase) : base()
        {
            InitAddonsTabMenu(instanceModelBase);
        }

        private void InitAddonsTabMenu(InstanceModelBase instanceModelBase)
        {
            HeaderKey = HEADER_KEY;

            _settingsViewModel = new InstanceProfileSettingsViewModel(instanceModelBase);

            _tabs.Add(new TabItemModel { TextKey = "General", Content = _settingsViewModel, IsSelected = true });

            // Если сборка создана через лаунчер, включаем вкладки.
            if (instanceModelBase.Source == InstanceSource.Local) 
            {
                _aboutViewModel = new InstanceProfileAboutViewModel(instanceModelBase);
                _configurationViewModel = new InstanceProfileConfigurationViewModel(instanceModelBase);

                _tabs.Add(new TabItemModel { TextKey = "AboutInstance", Content = _aboutViewModel });
                _tabs.Add(new TabItemModel { TextKey = "Configuration", Content = _configurationViewModel });
            }
        }
    }
}
