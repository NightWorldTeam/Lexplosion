using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Modal;
using Lexplosion.UI.WPF.Mvvm.Model.Modal;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal
{
    public sealed class InstanceCopyViewModel : ActionModalViewModelBase
    {
        private readonly Action<InstanceClient, ImportData> _addToLibrary;


        #region Properties


        public InstanceCopyModel Model { get; }

        private bool _isVanilla;
        public bool IsVanilla
        {
            get => _isVanilla; set
            {
                _isVanilla = value;
                OnPropertyChanged();
            }
        }

        private bool _isForge;
        public bool IsForge
        {
            get => _isForge; set
            {
                _isForge = value;
                OnPropertyChanged();
            }
        }

        private bool _isFabric;
        public bool IsFabric
        {
            get => _isFabric; set
            {
                _isFabric = value;
                OnPropertyChanged();
            }
        }

        private bool _isQuilt;
        public bool IsQuilt
        {
            get => _isQuilt; set
            {
                _isQuilt = value;
                OnPropertyChanged();
            }
        }

        private bool _isNeoForged;
        public bool IsNeoForged
        {
            get => _isNeoForged; set
            {
                _isNeoForged = value;
                OnPropertyChanged();
            }
        }


        private RelayCommand _changeInstanceClientTypeCommand;
        public ICommand ChangeInstanceClientTypeCommand
        {
            get => RelayCommand.GetCommand(ref _changeInstanceClientTypeCommand, Model.ChangeGameType);
        }


        #endregion Properties


        public InstanceCopyViewModel(AppCore appCore, ClientsManager clientsManager, InstanceModelBase instance, Action<InstanceClient, ImportData> addToLibrary)
        {
            Model = new(appCore, clientsManager, instance);
            _addToLibrary = addToLibrary;
            ActionCommandExecutedEvent += OnAction;
            Model.GameTypeChanged += UpdateSelectedGameType;
            UpdateSelectedGameType(instance.BaseData.Modloader);
        }

        private void UpdateSelectedGameType(ClientType clientType)
        {
            switch (clientType)
            {
                case ClientType.Vanilla: IsVanilla = true; break;
                case ClientType.Forge: IsForge = true; break;
                case ClientType.Fabric: IsFabric = true; break;
                case ClientType.Quilt: IsQuilt = true; break;
                case ClientType.NeoForge: IsNeoForged = true; break;
            }
        }

        private void OnAction(object obj)
        {
            var instanceData = Model.Copy();
            _addToLibrary?.Invoke(instanceData.instanceClient, instanceData.importData);
        }
    }
}
