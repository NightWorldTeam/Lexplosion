using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Stores;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileAddonsLayoutViewModel : ContentLayoutViewModelBase
    {
        private readonly ViewModelBase _modsViewModel;
        private readonly ViewModelBase _resourcepacksViewModel;
        private readonly ViewModelBase _mapsViewModel;
        private readonly ViewModelBase _shadersViewModel;


        #region Constructors


        public InstanceProfileAddonsLayoutViewModel(INavigationStore navigationStore, InstanceModelBase instanceModelBase) : base()
        {
            HeaderKey = "Addons";
            if (instanceModelBase.InstanceData.Modloader != ClientType.Vanilla) 
            {
                _modsViewModel = new InstanceAddonsContainerViewModel(navigationStore, AddonType.Mods, instanceModelBase);
                _shadersViewModel = new InstanceAddonsContainerViewModel(navigationStore, AddonType.Shaders, instanceModelBase);
            }
            _resourcepacksViewModel = new InstanceAddonsContainerViewModel(navigationStore, AddonType.Resourcepacks, instanceModelBase);
            _mapsViewModel = new InstanceAddonsContainerViewModel(navigationStore, AddonType.Maps, instanceModelBase);
            
            InitAddonsTabMenu(instanceModelBase);
        }


        #endregion Constructors


        private void InitAddonsTabMenu(InstanceModelBase instanceModelBase)
        {
            _tabs.Add(new TabItemModel { TextKey = "Mods", Content = _modsViewModel, IsSelected = true });
            _tabs.Add(new TabItemModel { TextKey = "Resourcepacks", Content = _resourcepacksViewModel });
            _tabs.Add(new TabItemModel { TextKey = "Maps", Content = _mapsViewModel });
            _tabs.Add(new TabItemModel { TextKey = "Shaders", Content = _shadersViewModel });
        }
    }
}
