using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using Lexplosion.WPF.NewInterface.Models.MainContent.InstanceProfile.Settings;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileConfigurationViewModel : ViewModelBase
    {
        #region Properties


        public InstanceProfileConfigurationModel Model { get; }


        private bool _isVanilla = true;
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


        #endregion Properties


        #region Constructors


        public InstanceProfileConfigurationViewModel(InstanceModelBase instanceModelBase)
        {
            Model = new InstanceProfileConfigurationModel(instanceModelBase);

            //switch (Model.GetInstanceExtenstion())
            //{
            //    case ClientType.Vanilla: IsVanilla = true; break;
            //    case ClientType.Forge: IsForge = true; break;
            //    case ClientType.Fabric: IsFabric = true; break;
            //    case ClientType.Quilt: IsQuilt = true; break;
            //}
        }


        #endregion Constructors


        #region Commands


        private RelayCommand _saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get => RelayCommand.GetCommand(ref _saveChangesCommand, (obj) => 
            { 
                //Model.SaveData(); 
            });
        }

        private RelayCommand _rebootChangesCommand;
        public ICommand RebootChangesCommand
        {
            get => RelayCommand.GetCommand(ref _rebootChangesCommand, (obj) => { });
        }


        #endregion Commands
    }
}
