using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Model.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public sealed class InstanceCopyViewModel : ActionModalViewModelBase
    {
        private readonly Action<InstanceClient> _addToLibrary;


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


        public InstanceCopyViewModel(AppCore appCore, ClientsManager clientsManager, InstanceModelBase instance, Action<InstanceClient> addToLibrary)
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
            var ic = Model.Copy();
            _addToLibrary?.Invoke(ic);
        }
    }
}
