using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile.Addons;
using Lexplosion.WPF.NewInterface.Stores;
using Microsoft.VisualBasic;
using System;
using System.Windows.Input;
using static System.Windows.Forms.AxHost;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileAddonsLayoutViewModel : ContentLayoutViewModelBase
    {
        private readonly ViewModelBase _modsViewModel;
        private readonly ViewModelBase _resourcepacksViewModel;
        private readonly ViewModelBase _mapsViewModel;
        private readonly ViewModelBase _shadersViewModel;

        private bool _isLargeBlocks;
        public bool IsLargeBlocks 
        { 
            get => _isLargeBlocks; set 
            {
                _isLargeBlocks = value;

                if (_modsViewModel != null)
                {
                    (_modsViewModel as IVisualFormat<VisualFormat>).ChangeVisualFormat(VisualFormat.Block);
                }
                if (_shadersViewModel != null)
                { 
                    (_shadersViewModel as IVisualFormat<VisualFormat>).ChangeVisualFormat(VisualFormat.Block);
                }

                (_resourcepacksViewModel as IVisualFormat<VisualFormat>).ChangeVisualFormat(VisualFormat.Block);
                (_mapsViewModel as IVisualFormat<VisualFormat>).ChangeVisualFormat(VisualFormat.Block);
                OnPropertyChanged();
            }
        }

        private bool _isSearchEnable = true;
        public bool IsSearchEnable 
        {
            get => _isSearchEnable; set 
            {
                _isSearchEnable = value;
                OnPropertyChanged();
            }
        }


        #region Commands


        private RelayCommand _searchStateChanged;
        public ICommand SearchStateChangedCommand
        {
            get => RelayCommand.GetCommand(ref _searchStateChanged, () => 
            {
                (SelectedItem.Content as IInstanceAddonContainerActions).SearchStateChanged(IsSearchEnable);
            });
        }



        private RelayCommand _openFolderCommand;
        public ICommand OpenFolderCommand
        {
            get => RelayCommand.GetCommand(ref _openFolderCommand, () => 
            {
                (SelectedItem.Content as IInstanceAddonContainerActions).OpenFolder();
            });
        }


        // TODO: Rename to Repository
        private RelayCommand _openMarketCommand;
        public ICommand OpenMarketCommand
        {
            get => RelayCommand.GetCommand(ref _openMarketCommand, () => 
            {
                (SelectedItem.Content as IInstanceAddonContainerActions).OpenAddonRepository();
            });
        }

        private RelayCommand _reloadCommand;
        public ICommand ReloadCommand
        {
            get => RelayCommand.GetCommand(ref _reloadCommand, (obj) =>
            {
                (SelectedItem.Content as IInstanceAddonContainerActions).Reload();
            });
        }


        #endregion 


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
            if (instanceModelBase.InstanceData.Modloader != ClientType.Vanilla) { 
                _tabs.Add(new TabItemModel { Id = 0, TextKey = "Mods", Content = _modsViewModel });
                _tabs.Add(new TabItemModel { Id = 3, TextKey = "Shaders", Content = _shadersViewModel });
            }
            _tabs.Add(new TabItemModel { Id = 2, TextKey = "Resourcepacks", Content = _resourcepacksViewModel });
            _tabs.Add(new TabItemModel { Id = 3, TextKey = "Maps", Content = _mapsViewModel });
            _tabs[0].IsSelected = true;

            (_tabs[0].Content as IInstanceAddonContainerActions).SearchStateChanged(IsSearchEnable);
        }
    }
}
