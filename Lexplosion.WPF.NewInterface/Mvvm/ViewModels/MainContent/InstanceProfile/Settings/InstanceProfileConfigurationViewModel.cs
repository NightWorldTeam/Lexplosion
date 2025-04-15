using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.InstanceProfile.Settings;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileConfigurationViewModel : ViewModelBase
    {
        #region Properties


        public InstanceProfileConfigurationModel Model { get; }


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

        private bool _isNeoforged;
        public bool IsNeoforged
        {
            get => _isNeoforged; set
            {
                _isNeoforged = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Commands


        private RelayCommand _saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get => RelayCommand.GetCommand(ref _saveChangesCommand, Model.SaveData);
        }

        private RelayCommand _rebootChangesCommand;
        public ICommand RebootChangesCommand
        {
            get => RelayCommand.GetCommand(ref _rebootChangesCommand, Model.ResetChanges);
        }

        private RelayCommand _changeInstanceClientTypeCommand;
        public ICommand ChangeInstanceClientTypeCommand
        {
            get => RelayCommand.GetCommand(ref _changeInstanceClientTypeCommand, Model.ChangeGameType);
        }


        #endregion Commands


        #region Constructors


        public InstanceProfileConfigurationViewModel(InstanceModelBase instanceModelBase)
        {
            Model = new InstanceProfileConfigurationModel(instanceModelBase);
            Model.GameTypeChanged += UpdateSelectedGameType;
            UpdateSelectedGameType(instanceModelBase.BaseData.Modloader);
            // устанавливаем кнопку с типом игры в активное положение.
        }


        #endregion Constructors


        private void UpdateSelectedGameType(ClientType clientType)
        {
            switch (clientType)
            {
                case ClientType.Vanilla: IsVanilla = true; break;
                case ClientType.Forge: IsForge = true; break;
                case ClientType.Fabric: IsFabric = true; break;
                case ClientType.Quilt: IsQuilt = true; break;
                case ClientType.NeoForge: IsNeoforged = true; break;
            }
        }
    }
}
