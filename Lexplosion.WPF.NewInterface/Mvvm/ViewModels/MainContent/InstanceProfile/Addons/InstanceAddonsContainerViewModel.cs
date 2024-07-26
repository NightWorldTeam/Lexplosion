using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.InstanceProfile;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.AddonsRepositories;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile.Addons;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public interface IInstanceAddonContainerActions 
    {
        public void SearchStateChanged(bool state);
        public void OpenAddonRepository();
        public void OpenFolder();
        public void Reload();
    }

    public sealed class InstanceAddonsContainerViewModel : ViewModelBase, IVisualFormat<VisualFormat>, IInstanceAddonContainerActions
    {
        private readonly INavigationStore _navigationStore;
        private readonly InstanceModelBase _instanceModelBase;


        #region Properties


        public InstanceAddonsContainerModel Model { get; private set; }

        public VisualFormat CurrentFormat { get; private set; }

        public bool IsCurrentFormatBlock { get => CurrentFormat == VisualFormat.Block; }


        #endregion Properties


        #region Commands


        private RelayCommand _openExternalResourceCommand;
        public ICommand OpenExternalResourceCommand
        {
            get => RelayCommand.GetCommand<InstanceAddon>(ref _openExternalResourceCommand, Model.OpenExternalResource);
        }


        private RelayCommand _updateCommand;
        public ICommand UpdateCommand
        {
            get => RelayCommand.GetCommand(ref _updateCommand, Model.UpdateAddon);
        }

        private RelayCommand _uninstallCommand;
        public ICommand UninstallCommand
        {
            get => RelayCommand.GetCommand(ref _uninstallCommand, (obj) =>
            {
                var dialogViewModel = new DialogBoxViewModel("delete", "delete",
                (obj) =>
                {
                    Model.UninstallAddon(obj);
                }, (obj) => { //ModalNavigationStore.Close();
                });
                ModalNavigationStore.Instance.Open(dialogViewModel);
            });
        }


        #endregion Commands


        #region Constructors


        public InstanceAddonsContainerViewModel(INavigationStore navigationStore, AddonType addonType, InstanceModelBase instanceModelBase)
        {
            _navigationStore = navigationStore;
            _instanceModelBase = instanceModelBase;
            Model = new InstanceAddonsContainerModel(addonType, instanceModelBase, () => instanceModelBase.DirectoryPath);
        }


        #endregion Constructors


        #region Public Methods


        public void ChangeVisualFormat(VisualFormat format)
        {
            CurrentFormat = CurrentFormat == VisualFormat.Block ? VisualFormat.Line : VisualFormat.Block;
            OnPropertyChanged(nameof(CurrentFormat));
            OnPropertyChanged(nameof(IsCurrentFormatBlock));
        }

        /// <summary>
        /// Включает/Выключает поиск.
        /// </summary>
        /// <param name="state"></param>
        public void SearchStateChanged(bool state)
        {
            Model.IsSearchEnabled = state;
        }

        /// <summary>
        /// Открывает Curseforge/Modrinth
        /// </summary>
        public void OpenAddonRepository()
        {
            var currentViewModel = _navigationStore.CurrentViewModel;
            var backNavCommand = new NavigateCommand<ViewModelBase>(_navigationStore, () => currentViewModel);
            // TODO: Давать возможно назначать репозиторий по-умолчанию
            // Возможно даже делать параметр по умолчанию
            _navigationStore.CurrentViewModel = new LexplosionAddonsRepositoryViewModel(_instanceModelBase, Model.Type, backNavCommand, _navigationStore); //new ModrinthRepositoryViewModel(_instanceModelBase, Model.Type, backNavCommand, _navigationStore);
        }

        /// <summary>
        /// Открывает папку с игрой
        /// </summary>
        public void OpenFolder()
        {
            Model.OpenFolder();
        }

        /// <summary>
        /// Обновляет список аддонов.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Reload()
        {
            Model.Reload();
        }


        #endregion Public Methods
    }
}
